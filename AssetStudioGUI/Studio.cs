using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AssetStudio;
using dnlib.DotNet;
using static AssetStudioGUI.Exporter;

namespace AssetStudioGUI
{
    internal static class Studio
    {
        public static AssetsManager assetsManager = new AssetsManager();
        private static HashSet<string> assetsNameHash = new HashSet<string>();
        public static List<AssetItem> exportableAssets = new List<AssetItem>();
        public static List<AssetItem> visibleAssets = new List<AssetItem>();
        public static Dictionary<string, SortedDictionary<int, TypeTreeItem>> AllTypeMap = new Dictionary<string, SortedDictionary<int, TypeTreeItem>>();
        public static Dictionary<GameObject, GameObjectTreeNode> treeNodeDictionary = new Dictionary<GameObject, GameObjectTreeNode>();
        public static bool ModuleLoaded;
        public static Dictionary<string, ModuleDef> LoadedModuleDic = new Dictionary<string, ModuleDef>();

        public static void ExtractFile(string[] fileNames)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                int extractedCount = 0;
                Progress.Reset();
                for (var i = 0; i < fileNames.Length; i++)
                {
                    var fileName = fileNames[i];
                    var type = ImportHelper.CheckFileType(fileName, out var reader);
                    if (type == FileType.BundleFile)
                        extractedCount += ExtractBundleFile(fileName, reader);
                    else if (type == FileType.WebFile)
                        extractedCount += ExtractWebDataFile(fileName, reader);
                    else
                        reader.Dispose();
                    Progress.Report(i + 1, fileName.Length);
                }

                Logger.Info($"Finished extracting {extractedCount} files.");
            });
        }

        private static int ExtractBundleFile(string bundleFileName, EndianBinaryReader reader)
        {
            Logger.Info($"Decompressing {Path.GetFileName(bundleFileName)} ...");
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
            Logger.Info($"Decompressing {Path.GetFileName(webFileName)} ...");
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

        public static void BuildAssetList(Dictionary<ObjectReader, AssetItem> tempDic, bool displayAll, bool displayOriginalName, out string productName)
        {
            productName = string.Empty;
            Logger.Info("Building asset list...");

            var progressCount = assetsManager.assetsFileList.Sum(x => x.ObjectReaders.Count);
            int j = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                var tempExportableAssets = new List<AssetItem>();
                AssetBundle ab = null;
                foreach (var objectReader in assetsFile.ObjectReaders.Values)
                {
                    var assetItem = new AssetItem(objectReader);
                    tempDic.Add(objectReader, assetItem);
                    assetItem.UniqueID = " #" + j;
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
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                    assetItem.SubItems.AddRange(new[] { assetItem.TypeString, assetItem.FullSize.ToString() });
                    //处理同名文件
                    if (!assetsNameHash.Add((assetItem.TypeString + assetItem.Text).ToUpper()))
                    {
                        assetItem.Text += assetItem.UniqueID;
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

                    Progress.Report(++j, progressCount);
                }
                if (displayOriginalName && ab != null)
                {
                    foreach (var x in tempExportableAssets)
                    {
                        var replacename = ab.m_Container.Find(y => y.second.asset.m_PathID == x.reader.m_PathID)?.first;
                        if (!string.IsNullOrEmpty(replacename))
                        {
                            var ex = Path.GetExtension(replacename);
                            x.Text = !string.IsNullOrEmpty(ex) ? replacename.Replace(ex, "") : replacename;
                            if (!assetsNameHash.Add((x.TypeString + x.Text).ToUpper()))
                            {
                                x.Text = Path.GetDirectoryName(replacename) + "\\" + Path.GetFileNameWithoutExtension(replacename) + x.UniqueID;
                            }
                        }
                    }
                }
                exportableAssets.AddRange(tempExportableAssets);
                tempExportableAssets.Clear();
            }

            visibleAssets = exportableAssets;
            assetsNameHash.Clear();
        }

        public static List<GameObjectTreeNode> BuildTreeStructure(Dictionary<ObjectReader, AssetItem> tempDic)
        {
            var treeNodeCollection = new List<GameObjectTreeNode>();
            var gameObjectCount = assetsManager.assetsFileList.Sum(x => x.GameObjects.Count);
            if (gameObjectCount > 0)
            {
                Logger.Info("Building tree structure...");
                int i = 0;
                Progress.Reset();
                foreach (var assetsFile in assetsManager.assetsFileList)
                {
                    var fileNode = new GameObjectTreeNode(assetsFile.fileName); //RootNode

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
                                            item.Text = tempDic[m_GameObject.reader].Text;
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

                        Progress.Report(++i, gameObjectCount);
                    }

                    if (fileNode.Nodes.Count > 0)
                    {
                        treeNodeCollection.Add(fileNode);
                    }
                }
            }

            return treeNodeCollection;
        }

        public static void BuildClassStructure()
        {
            foreach (var assetsFile in assetsManager.assetsFileList)
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

        public static string FixFileName(string str)
        {
            if (str.Length >= 260) return Path.GetRandomFileName();
            return Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));
        }

        public static void ExportAssets(string savePath, List<AssetItem> toExportAssets, int assetGroupSelectedIndex, bool openAfterExport)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                int toExportCount = toExportAssets.Count;
                int exportedCount = 0;
                int i = 0;
                Progress.Reset();
                foreach (var asset in toExportAssets)
                {
                    var exportpath = savePath + "\\";
                    if (assetGroupSelectedIndex == 1)
                    {
                        exportpath += Path.GetFileNameWithoutExtension(asset.sourceFile.fullName) + "_export\\";
                    }
                    else if (assetGroupSelectedIndex == 0)
                    {
                        exportpath = savePath + "\\" + asset.TypeString + "\\";
                    }
                    Logger.Info($"Exporting {asset.TypeString}: {asset.Text}");
                    try
                    {
                        switch (asset.Type)
                        {
                            case ClassIDType.Texture2D:
                                if (ExportTexture2D(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.AudioClip:
                                if (ExportAudioClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Shader:
                                if (ExportShader(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.TextAsset:
                                if (ExportTextAsset(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.MonoBehaviour:
                                if (ExportMonoBehaviour(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Font:
                                if (ExportFont(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Mesh:
                                if (ExportMesh(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.VideoClip:
                                if (ExportVideoClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.MovieTexture:
                                if (ExportMovieTexture(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Sprite:
                                if (ExportSprite(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.Animator:
                                if (ExportAnimator(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDType.AnimationClip:
                                break;
                            default:
                                if (ExportRawFile(asset, exportpath))
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

                    Progress.Report(++i, toExportCount);
                }

                var statusText = exportedCount == 0 ? "Nothing exported." : $"Finished exporting {exportedCount} assets.";

                if (toExportCount > exportedCount)
                {
                    statusText += $" {toExportCount - exportedCount} assets skipped (not extractable or files already exist)";
                }

                Logger.Info(statusText);

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
                var count = nodes.Cast<TreeNode>().Sum(x => x.Nodes.Count);
                int k = 0;
                Progress.Reset();
                foreach (GameObjectTreeNode node in nodes)
                {
                    //遍历一级子节点
                    foreach (GameObjectTreeNode j in node.Nodes)
                    {
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
                        Logger.Info($"Exporting {filename}.fbx");
                        try
                        {
                            ExportGameObject(j.gameObject, targetPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                        }

                        Progress.Report(++k, count);
                        Logger.Info($"Finished exporting {filename}.fbx");
                    }
                }
                Logger.Info("Finished");
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
                Logger.Info($"Exporting {animator.Text}");
                try
                {
                    ExportAnimator(animator, exportPath, animationList);
                    Logger.Info($"Finished exporting {animator.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                    Logger.Info("Error in export");
                }
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
                    var count = gameObjects.Count;
                    int i = 0;
                    Progress.Reset();
                    foreach (var gameObject in gameObjects)
                    {
                        Logger.Info($"Exporting {gameObject.m_Name}");
                        try
                        {
                            ExportGameObject(gameObject, exportPath, animationList);
                            Logger.Info($"Finished exporting {gameObject.m_Name}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                            Logger.Info("Error in export");
                        }

                        Progress.Report(++i, count);
                    }
                }
                else
                {
                    Logger.Info("No Object can be exported.");
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

        public static string GetScriptString(ObjectReader reader)
        {
            if (!ModuleLoaded)
            {
                var openFolderDialog = new OpenFolderDialog();
                openFolderDialog.Title = "Select Assembly Folder";
                if (openFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    var files = Directory.GetFiles(openFolderDialog.Folder, "*.dll");
                    var moduleContext = new ModuleContext();
                    var asmResolver = new AssemblyResolver(moduleContext, true);
                    var resolver = new Resolver(asmResolver);
                    moduleContext.AssemblyResolver = asmResolver;
                    moduleContext.Resolver = resolver;
                    try
                    {
                        foreach (var file in files)
                        {
                            var module = ModuleDefMD.Load(file, moduleContext);
                            LoadedModuleDic.Add(Path.GetFileName(file), module);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                ModuleLoaded = true;
            }

            return ScriptHelper.GetScriptString(reader, LoadedModuleDic);
        }
    }
}
