using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static AssetStudio.Exporter;

namespace AssetStudio
{
    internal static class Studio
    {
        public static List<AssetsFile> assetsfileList = new List<AssetsFile>(); //loaded files
        public static Dictionary<string, int> assetsFileIndexCache = new Dictionary<string, int>();
        public static Dictionary<string, EndianBinaryReader> resourceFileReaders = new Dictionary<string, EndianBinaryReader>(); //use for read res files
        public static List<AssetItem> exportableAssets = new List<AssetItem>(); //used to hold all assets while the ListView is filtered
        private static HashSet<string> assetsNameHash = new HashSet<string>(); //avoid the same name asset
        public static List<AssetItem> visibleAssets = new List<AssetItem>(); //used to build the ListView from all or filtered assets
        public static Dictionary<string, SortedDictionary<int, TypeTreeItem>> AllTypeMap = new Dictionary<string, SortedDictionary<int, TypeTreeItem>>();
        public static List<GameObjectTreeNode> treeNodeCollection = new List<GameObjectTreeNode>();
        public static Dictionary<GameObject, GameObjectTreeNode> treeNodeDictionary = new Dictionary<GameObject, GameObjectTreeNode>();
        public static string mainPath;
        public static string productName = string.Empty;

        //UI
        public static Action<int> SetProgressBarValue;
        public static Action<int> SetProgressBarMaximum;
        public static Action ProgressBarPerformStep;
        public static Action<string> StatusStripUpdate;
        public static Action<int> ProgressBarMaximumAdd;

        public enum FileType
        {
            AssetsFile,
            BundleFile,
            WebFile
        }

        public static FileType CheckFileType(Stream stream, out EndianBinaryReader reader)
        {
            reader = new EndianBinaryReader(stream);
            return CheckFileType(reader);
        }

        public static FileType CheckFileType(string fileName, out EndianBinaryReader reader)
        {
            reader = new EndianBinaryReader(File.OpenRead(fileName));
            return CheckFileType(reader);
        }

        private static FileType CheckFileType(EndianBinaryReader reader)
        {
            var signature = reader.ReadStringToNull();
            reader.Position = 0;
            switch (signature)
            {
                case "UnityWeb":
                case "UnityRaw":
                case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                default:
                    {
                        var magic = reader.ReadBytes(2);
                        reader.Position = 0;
                        if (WebFile.gzipMagic.SequenceEqual(magic))
                        {
                            return FileType.WebFile;
                        }
                        reader.Position = 0x20;
                        magic = reader.ReadBytes(6);
                        reader.Position = 0;
                        if (WebFile.brotliMagic.SequenceEqual(magic))
                        {
                            return FileType.WebFile;
                        }
                        return FileType.AssetsFile;
                    }
            }
        }

        public static void ExtractFile(string[] fileNames)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                int extractedCount = 0;
                foreach (var fileName in fileNames)
                {
                    var type = CheckFileType(fileName, out var reader);
                    if (type == FileType.BundleFile)
                        extractedCount += ExtractBundleFile(fileName, reader);
                    else if (type == FileType.WebFile)
                        extractedCount += ExtractWebDataFile(fileName, reader);
                    else
                        reader.Dispose();
                    ProgressBarPerformStep();
                }
                StatusStripUpdate($"Finished extracting {extractedCount} files.");
            });
        }

        private static int ExtractBundleFile(string bundleFileName, EndianBinaryReader reader)
        {
            StatusStripUpdate($"Decompressing {Path.GetFileName(bundleFileName)} ...");
            var bundleFile = new BundleFile(reader, bundleFileName);
            reader.Dispose();
            if (bundleFile.fileList.Count > 0)
            {
                var extractPath = bundleFileName + "_unpacked\\";
                Directory.CreateDirectory(extractPath);
                return ExtractStreamFile(extractPath, bundleFile.fileList);
            }
            return 0;
        }

        private static int ExtractWebDataFile(string webFileName, EndianBinaryReader reader)
        {
            StatusStripUpdate($"Decompressing {Path.GetFileName(webFileName)} ...");
            var webFile = new WebFile(reader);
            reader.Dispose();
            if (webFile.fileList.Count > 0)
            {
                var extractPath = webFileName + "_unpacked\\";
                Directory.CreateDirectory(extractPath);
                return ExtractStreamFile(extractPath, webFile.fileList);
            }
            return 0;
        }

        private static int ExtractStreamFile(string extractPath, List<StreamFile> fileList)
        {
            int extractedCount = 0;
            foreach (var file in fileList)
            {
                var filePath = extractPath + file.fileName;
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }
                if (!File.Exists(filePath) && file.stream is MemoryStream stream)
                {
                    File.WriteAllBytes(filePath, stream.ToArray());
                    extractedCount += 1;
                }
                file.stream.Dispose();
            }
            return extractedCount;
        }

        public static void BuildAssetStructures(bool loadAssets, bool displayAll, bool buildHierarchy, bool buildClassStructures, bool displayOriginalName)
        {
            var tempDic = new Dictionary<ObjectReader, AssetItem>();
            // first loop - read asset data & create list
            if (loadAssets)
            {
                SetProgressBarValue(0);
                SetProgressBarMaximum(assetsfileList.Sum(x => x.ObjectReaders.Count));
                StatusStripUpdate("Building asset list...");

                var fileIDfmt = "D" + assetsfileList.Count.ToString().Length;

                for (var i = 0; i < assetsfileList.Count; i++)
                {
                    var assetsFile = assetsfileList[i];
                    var tempExportableAssets = new List<AssetItem>();
                    var fileID = i.ToString(fileIDfmt);
                    AssetBundle ab = null;
                    var j = 0;
                    var assetIDfmt = "D" + assetsFile.m_Objects.Count.ToString().Length;
                    foreach (var objectReader in assetsFile.ObjectReaders.Values)
                    {
                        var assetItem = new AssetItem(objectReader);
                        tempDic.Add(objectReader, assetItem);
                        assetItem.UniqueID = fileID + j.ToString(assetIDfmt);
                        var exportable = false;
                        switch (assetItem.Type)
                        {
                            case ClassIDType.GameObject:
                                {
                                    var m_GameObject = new GameObject(objectReader);
                                    assetItem.Text = m_GameObject.m_Name;
                                    assetsFile.GameObjects.Add(objectReader.m_PathID, m_GameObject);
                                    break;
                                }
                            case ClassIDType.Transform:
                                {
                                    var m_Transform = new Transform(objectReader);
                                    assetsFile.Transforms.Add(objectReader.m_PathID, m_Transform);
                                    break;
                                }
                            case ClassIDType.RectTransform:
                                {
                                    var m_Rect = new RectTransform(objectReader);
                                    assetsFile.Transforms.Add(objectReader.m_PathID, m_Rect);
                                    break;
                                }
                            case ClassIDType.Texture2D:
                                {
                                    var m_Texture2D = new Texture2D(objectReader, false);
                                    if (!string.IsNullOrEmpty(m_Texture2D.path))
                                        assetItem.FullSize = objectReader.byteSize + m_Texture2D.size;
                                    assetItem.Text = m_Texture2D.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.AudioClip:
                                {
                                    var m_AudioClip = new AudioClip(objectReader, false);
                                    if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                        assetItem.FullSize = objectReader.byteSize + m_AudioClip.m_Size;
                                    assetItem.Text = m_AudioClip.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.VideoClip:
                                {
                                    var m_VideoClip = new VideoClip(objectReader, false);
                                    if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                        assetItem.FullSize = objectReader.byteSize + (long)m_VideoClip.m_Size;
                                    assetItem.Text = m_VideoClip.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.Shader:
                                {
                                    var m_Shader = new Shader(objectReader);
                                    assetItem.Text = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.Mesh:
                            case ClassIDType.TextAsset:
                            case ClassIDType.AnimationClip:
                            case ClassIDType.Font:
                            case ClassIDType.MovieTexture:
                            case ClassIDType.Sprite:
                                {
                                    var obj = new NamedObject(objectReader);
                                    assetItem.Text = obj.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.Avatar:
                            case ClassIDType.AnimatorController:
                            case ClassIDType.AnimatorOverrideController:
                            case ClassIDType.Material:
                            case ClassIDType.MonoScript:
                            case ClassIDType.SpriteAtlas:
                                {
                                    var obj = new NamedObject(objectReader);
                                    assetItem.Text = obj.m_Name;
                                    break;
                                }
                            case ClassIDType.Animator:
                                {
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.MonoBehaviour:
                                {
                                    var m_MonoBehaviour = new MonoBehaviour(objectReader);
                                    if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var script))
                                    {
                                        var m_Script = new MonoScript(script);
                                        assetItem.Text = m_Script.m_ClassName;
                                    }
                                    else
                                    {
                                        assetItem.Text = m_MonoBehaviour.m_Name;
                                    }
                                    exportable = true;
                                    break;
                                }
                            case ClassIDType.PlayerSettings:
                                {
                                    var plSet = new PlayerSettings(objectReader);
                                    productName = plSet.productName;
                                    break;
                                }
                            case ClassIDType.AssetBundle:
                                {
                                    ab = new AssetBundle(objectReader);
                                    assetItem.Text = ab.m_Name;
                                    break;
                                }
                        }
                        if (assetItem.Text == "")
                        {
                            assetItem.Text = assetItem.TypeString + " #" + assetItem.UniqueID;
                        }
                        assetItem.SubItems.AddRange(new[] { assetItem.TypeString, assetItem.FullSize.ToString() });
                        //处理同名文件
                        if (!assetsNameHash.Add((assetItem.TypeString + assetItem.Text).ToUpper()))
                        {
                            assetItem.Text += " #" + assetItem.UniqueID;
                        }
                        //处理非法文件名
                        assetItem.Text = FixFileName(assetItem.Text);
                        if (displayAll)
                        {
                            exportable = true;
                        }
                        if (exportable)
                        {
                            tempExportableAssets.Add(assetItem);
                        }
                        objectReader.exportName = assetItem.Text;

                        ProgressBarPerformStep();
                        j++;
                    }
                    if (displayOriginalName)
                    {
                        foreach (var x in tempExportableAssets)
                        {
                            var replacename = ab?.m_Container.Find(y => y.second.asset.m_PathID == x.reader.m_PathID)?.first;
                            if (!string.IsNullOrEmpty(replacename))
                            {
                                var ex = Path.GetExtension(replacename);
                                x.Text = !string.IsNullOrEmpty(ex) ? replacename.Replace(ex, "") : replacename;
                                x.reader.exportName = x.Text;
                            }
                        }
                    }
                    exportableAssets.AddRange(tempExportableAssets);
                    tempExportableAssets.Clear();
                }

                visibleAssets = exportableAssets;
                assetsNameHash.Clear();
            }

            // second loop - build tree structure
            if (buildHierarchy)
            {
                var gameObjectCount = assetsfileList.Sum(x => x.GameObjects.Count);
                if (gameObjectCount > 0)
                {
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(gameObjectCount);
                    StatusStripUpdate("Building tree structure...");

                    foreach (var assetsFile in assetsfileList)
                    {
                        var fileNode = new GameObjectTreeNode(null); //RootNode
                        fileNode.Text = assetsFile.fileName;

                        foreach (var m_GameObject in assetsFile.GameObjects.Values)
                        {
                            foreach (var m_Component in m_GameObject.m_Components)
                            {
                                if (m_Component.TryGet(out var asset))
                                {
                                    switch (asset.type)
                                    {
                                        case ClassIDType.Transform:
                                            {
                                                m_GameObject.m_Transform = m_Component;
                                                break;
                                            }
                                        case ClassIDType.MeshRenderer:
                                            {
                                                m_GameObject.m_MeshRenderer = m_Component;
                                                break;
                                            }
                                        case ClassIDType.MeshFilter:
                                            {
                                                m_GameObject.m_MeshFilter = m_Component;
                                                if (m_Component.TryGet(out var objectReader))
                                                {
                                                    var m_MeshFilter = new MeshFilter(objectReader);
                                                    if (m_MeshFilter.m_Mesh.TryGet(out objectReader))
                                                    {
                                                        var item = tempDic[objectReader];
                                                        item.gameObject = m_GameObject;
                                                    }
                                                }
                                                break;
                                            }
                                        case ClassIDType.SkinnedMeshRenderer:
                                            {
                                                m_GameObject.m_SkinnedMeshRenderer = m_Component;
                                                if (m_Component.TryGet(out var objectReader))
                                                {
                                                    var m_SkinnedMeshRenderer = new SkinnedMeshRenderer(objectReader);
                                                    if (m_SkinnedMeshRenderer.m_Mesh.TryGet(out objectReader))
                                                    {
                                                        var item = tempDic[objectReader];
                                                        item.gameObject = m_GameObject;
                                                    }
                                                }
                                                break;
                                            }
                                        case ClassIDType.Animator:
                                            {
                                                m_GameObject.m_Animator = m_Component;
                                                var item = tempDic[asset];
                                                item.Text = m_GameObject.reader.exportName;
                                                asset.exportName = m_GameObject.reader.exportName;
                                                break;
                                            }
                                    }
                                }
                            }

                            var parentNode = fileNode;

                            if (m_GameObject.m_Transform != null && m_GameObject.m_Transform.TryGetTransform(out var m_Transform))
                            {
                                if (m_Transform.m_Father.TryGetTransform(out var m_Father))
                                {
                                    if (m_Father.m_GameObject.TryGetGameObject(out var parentGameObject))
                                    {
                                        if (!treeNodeDictionary.TryGetValue(parentGameObject, out parentNode))
                                        {
                                            parentNode = new GameObjectTreeNode(parentGameObject);
                                            treeNodeDictionary.Add(parentGameObject, parentNode);
                                        }
                                    }
                                }
                            }

                            if (!treeNodeDictionary.TryGetValue(m_GameObject, out var currentNode))
                            {
                                currentNode = new GameObjectTreeNode(m_GameObject);
                                treeNodeDictionary.Add(m_GameObject, currentNode);
                            }
                            parentNode.Nodes.Add(currentNode);

                            ProgressBarPerformStep();
                        }

                        if (fileNode.Nodes.Count > 0)
                        {
                            treeNodeCollection.Add(fileNode);
                        }
                    }
                }
            }
            tempDic.Clear();

            // build list of class strucutres
            if (buildClassStructures)
            {
                foreach (var assetsFile in assetsfileList)
                {
                    if (AllTypeMap.TryGetValue(assetsFile.unityVersion, out var curVer))
                    {
                        foreach (var type in assetsFile.m_Types.Where(x => x.m_Nodes != null))
                        {
                            var key = type.classID;
                            if (type.m_ScriptTypeIndex >= 0)
                            {
                                key = -1 - type.m_ScriptTypeIndex;
                            }
                            curVer[key] = new TypeTreeItem(key, type.m_Nodes);
                        }
                    }
                    else
                    {
                        var items = new SortedDictionary<int, TypeTreeItem>();
                        foreach (var type in assetsFile.m_Types.Where(x => x.m_Nodes != null))
                        {
                            var key = type.classID;
                            if (type.m_ScriptTypeIndex >= 0)
                            {
                                key = -1 - type.m_ScriptTypeIndex;
                            }
                            items.Add(key, new TypeTreeItem(key, type.m_Nodes));
                        }
                        AllTypeMap.Add(assetsFile.unityVersion, items);
                    }
                }
            }
        }

        public static string FixFileName(string str)
        {
            if (str.Length >= 260) return Path.GetRandomFileName();
            return Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));
        }

        public static string[] ProcessingSplitFiles(List<string> selectFile)
        {
            var splitFiles = selectFile.Where(x => x.Contains(".split"))
                .Select(x => Path.GetDirectoryName(x) + "\\" + Path.GetFileNameWithoutExtension(x))
                .Distinct()
                .ToList();
            selectFile.RemoveAll(x => x.Contains(".split"));
            foreach (var file in splitFiles)
            {
                if (File.Exists(file))
                {
                    selectFile.Add(file);
                }
            }
            return selectFile.Distinct().ToArray();
        }

        public static void ExportAssets(string savePath, List<AssetItem> toExportAssets, int assetGroupSelectedIndex, bool openAfterExport)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                int toExport = toExportAssets.Count;
                int exportedCount = 0;

                SetProgressBarValue(0);
                SetProgressBarMaximum(toExport);
                foreach (var asset in toExportAssets)
                {
                    var exportpath = savePath + "\\";
                    if (assetGroupSelectedIndex == 1)
                    {
                        exportpath += Path.GetFileNameWithoutExtension(asset.sourceFile.filePath) + "_export\\";
                    }
                    else if (assetGroupSelectedIndex == 0)
                    {
                        exportpath = savePath + "\\" + asset.TypeString + "\\";
                    }
                    StatusStripUpdate($"Exporting {asset.TypeString}: {asset.Text}");
                    var reader = asset.reader;
                    try
                    {
                        switch (asset.Type)
                        {
                            case ClassIDType.Texture2D:
                                if (ExportTexture2D(reader, exportpath, true))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.AudioClip:
                                if (ExportAudioClip(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Shader:
                                if (ExportShader(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.TextAsset:
                                if (ExportTextAsset(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.MonoBehaviour:
                                if (ExportMonoBehaviour(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Font:
                                if (ExportFont(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Mesh:
                                if (ExportMesh(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.VideoClip:
                                if (ExportVideoClip(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.MovieTexture:
                                if (ExportMovieTexture(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Sprite:
                                if (ExportSprite(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Animator:
                                if (ExportAnimator(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.AnimationClip:
                                break;
                            default:
                                if (ExportRawFile(reader, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export {asset.Type}:{asset.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                    }
                    ProgressBarPerformStep();
                }

                var statusText = exportedCount == 0 ? "Nothing exported." : $"Finished exporting {exportedCount} assets.";

                if (toExport > exportedCount)
                {
                    statusText += $" {toExport - exportedCount} assets skipped (not extractable or files already exist)";
                }

                StatusStripUpdate(statusText);

                if (openAfterExport && exportedCount > 0)
                {
                    Process.Start(savePath);
                }
            });
        }

        public static void ExportSplitObjects(string savePath, TreeNodeCollection nodes)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                foreach (GameObjectTreeNode node in nodes)
                {
                    //遍历一级子节点
                    foreach (GameObjectTreeNode j in node.Nodes)
                    {
                        ProgressBarPerformStep();
                        //收集所有子节点
                        var gameObjects = new List<GameObject>();
                        CollectNode(j, gameObjects);
                        //跳过一些不需要导出的object
                        if (gameObjects.All(x => x.m_SkinnedMeshRenderer == null && x.m_MeshFilter == null))
                            continue;
                        //处理非法文件名
                        var filename = FixFileName(j.Text);
                        //每个文件存放在单独的文件夹
                        var targetPath = $"{savePath}{filename}\\";
                        //重名文件处理
                        for (int i = 1; ; i++)
                        {
                            if (Directory.Exists(targetPath))
                            {
                                targetPath = $"{savePath}{filename} ({i})\\";
                            }
                            else
                            {
                                break;
                            }
                        }
                        Directory.CreateDirectory(targetPath);
                        //导出FBX
                        StatusStripUpdate($"Exporting {filename}.fbx");
                        try
                        {
                            ExportGameObject(j.gameObject, targetPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                        }
                        StatusStripUpdate($"Finished exporting {filename}.fbx");
                    }
                }
                StatusStripUpdate("Finished");
            });
        }

        private static void CollectNode(GameObjectTreeNode node, List<GameObject> gameObjects)
        {
            gameObjects.Add(node.gameObject);
            foreach (GameObjectTreeNode i in node.Nodes)
            {
                CollectNode(i, gameObjects);
            }
        }

        public static void ExportAnimatorWithAnimationClip(AssetItem animator, List<AssetItem> animationList, string exportPath)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                StatusStripUpdate($"Exporting {animator.Text}");
                try
                {
                    ExportAnimator(animator.reader, exportPath, animationList);
                    StatusStripUpdate($"Finished exporting {animator.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                    StatusStripUpdate("Error in export");
                }
                ProgressBarPerformStep();
            });
        }

        public static void ExportObjectsWithAnimationClip(string exportPath, TreeNodeCollection nodes, List<AssetItem> animationList = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var gameObjects = new List<GameObject>();
                GetSelectedParentNode(nodes, gameObjects);
                if (gameObjects.Count > 0)
                {
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(gameObjects.Count);
                    foreach (var gameObject in gameObjects)
                    {
                        StatusStripUpdate($"Exporting {gameObject.m_Name}");
                        try
                        {
                            ExportGameObject(gameObject, exportPath, animationList);
                            StatusStripUpdate($"Finished exporting {gameObject.m_Name}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                            StatusStripUpdate("Error in export");
                        }

                        ProgressBarPerformStep();
                    }
                }
                else
                {
                    StatusStripUpdate("No Object can be exported.");
                }
            });
        }

        private static void GetSelectedParentNode(TreeNodeCollection nodes, List<GameObject> gameObjects)
        {
            foreach (GameObjectTreeNode i in nodes)
            {
                if (i.Checked)
                {
                    gameObjects.Add(i.gameObject);
                }
                else
                {
                    GetSelectedParentNode(i.Nodes, gameObjects);
                }
            }
        }
    }
}
