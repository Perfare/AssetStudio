using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using dnlib.DotNet;
using static AssetStudio.Exporter;

namespace AssetStudio
{
    internal static class Studio
    {
        public static List<AssetsFile> assetsfileList = new List<AssetsFile>(); //loaded files
        public static Dictionary<string, int> sharedFileIndex = new Dictionary<string, int>(); //to improve the loading speed
        public static Dictionary<string, EndianBinaryReader> resourceFileReaders = new Dictionary<string, EndianBinaryReader>(); //use for read res files
        public static List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>(); //used to hold all assets while the ListView is filtered
        private static HashSet<string> assetsNameHash = new HashSet<string>(); //avoid the same name asset
        public static List<AssetPreloadData> visibleAssets = new List<AssetPreloadData>(); //used to build the ListView from all or filtered assets
        public static Dictionary<string, Dictionary<int, TypeItem>> AllTypeMap = new Dictionary<string, Dictionary<int, TypeItem>>();
        public static string mainPath;
        public static string productName = "";
        public static bool moduleLoaded;
        public static Dictionary<string, ModuleDef> LoadedModuleDic = new Dictionary<string, ModuleDef>();
        public static List<GameObjectTreeNode> treeNodeCollection = new List<GameObjectTreeNode>();
        public static Dictionary<GameObject, GameObjectTreeNode> treeNodeDictionary = new Dictionary<GameObject, GameObjectTreeNode>();

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
            #region first loop - read asset data & create list
            if (loadAssets)
            {
                SetProgressBarValue(0);
                SetProgressBarMaximum(assetsfileList.Sum(x => x.preloadTable.Values.Count));
                StatusStripUpdate("Building asset list...");

                string fileIDfmt = "D" + assetsfileList.Count.ToString().Length;

                for (var i = 0; i < assetsfileList.Count; i++)
                {
                    var assetsFile = assetsfileList[i];

                    string fileID = i.ToString(fileIDfmt);
                    AssetBundle ab = null;
                    foreach (var asset in assetsFile.preloadTable.Values)
                    {
                        asset.uniqueID = fileID + asset.uniqueID;
                        var exportable = false;
                        switch (asset.Type)
                        {
                            case ClassIDReference.GameObject:
                                {
                                    var m_GameObject = new GameObject(asset);
                                    asset.Text = m_GameObject.m_Name;
                                    assetsFile.GameObjectList.Add(asset.m_PathID, m_GameObject);
                                    break;
                                }
                            case ClassIDReference.Transform:
                                {
                                    var m_Transform = new Transform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Transform);
                                    break;
                                }
                            case ClassIDReference.RectTransform:
                                {
                                    var m_Rect = new RectTransform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Rect);
                                    break;
                                }
                            case ClassIDReference.Texture2D:
                                {
                                    var m_Texture2D = new Texture2D(asset, false);
                                    if (!string.IsNullOrEmpty(m_Texture2D.path))
                                        asset.fullSize = asset.Size + (int)m_Texture2D.size;
                                    goto case ClassIDReference.NamedObject;
                                }
                            case ClassIDReference.AudioClip:
                                {
                                    var m_AudioClip = new AudioClip(asset, false);
                                    if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                        asset.fullSize = asset.Size + (int)m_AudioClip.m_Size;
                                    goto case ClassIDReference.NamedObject;
                                }
                            case ClassIDReference.VideoClip:
                                {
                                    var m_VideoClip = new VideoClip(asset, false);
                                    if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                        asset.fullSize = asset.Size + (int)m_VideoClip.m_Size;
                                    goto case ClassIDReference.NamedObject;
                                }
                            case ClassIDReference.NamedObject:
                            case ClassIDReference.Mesh:
                            case ClassIDReference.Shader:
                            case ClassIDReference.TextAsset:
                            case ClassIDReference.AnimationClip:
                            case ClassIDReference.Font:
                            case ClassIDReference.MovieTexture:
                            case ClassIDReference.Sprite:
                                {
                                    var obj = new NamedObject(asset);
                                    asset.Text = obj.m_Name;
                                    exportable = true;
                                    break;
                                }
                            case ClassIDReference.Avatar:
                            case ClassIDReference.AnimatorController:
                            case ClassIDReference.AnimatorOverrideController:
                            case ClassIDReference.Material:
                            case ClassIDReference.MonoScript:
                            case ClassIDReference.SpriteAtlas:
                                {
                                    var obj = new NamedObject(asset);
                                    asset.Text = obj.m_Name;
                                    break;
                                }
                            case ClassIDReference.Animator:
                                {
                                    exportable = true;
                                    break;
                                }
                            case ClassIDReference.MonoBehaviour:
                                {
                                    var m_MonoBehaviour = new MonoBehaviour(asset);
                                    if (m_MonoBehaviour.m_Name == "" && assetsfileList.TryGetPD(m_MonoBehaviour.m_Script, out var script))
                                    {
                                        var m_Script = new MonoScript(script);
                                        asset.Text = m_Script.m_ClassName;
                                    }
                                    else
                                    {
                                        asset.Text = m_MonoBehaviour.m_Name;
                                    }
                                    exportable = true;
                                    break;
                                }
                            case ClassIDReference.PlayerSettings:
                                {
                                    var plSet = new PlayerSettings(asset);
                                    productName = plSet.productName;
                                    break;
                                }
                            case ClassIDReference.AssetBundle:
                                {
                                    ab = new AssetBundle(asset);
                                    asset.Text = ab.m_Name;
                                    break;
                                }
                        }
                        if (asset.Text == "")
                        {
                            asset.Text = asset.TypeString + " #" + asset.uniqueID;
                        }
                        asset.SubItems.AddRange(new[] { asset.TypeString, asset.fullSize.ToString() });
                        //处理同名文件
                        if (!assetsNameHash.Add((asset.TypeString + asset.Text).ToUpper()))
                        {
                            asset.Text += " #" + asset.uniqueID;
                        }
                        //处理非法文件名
                        asset.Text = FixFileName(asset.Text);
                        if (displayAll)
                        {
                            exportable = true;
                        }
                        if (exportable)
                        {
                            assetsFile.exportableAssets.Add(asset);
                        }
                        ProgressBarPerformStep();
                    }
                    if (displayOriginalName)
                    {
                        assetsFile.exportableAssets.ForEach(x =>
                        {
                            var replacename = ab?.m_Container.Find(y => y.second.asset.m_PathID == x.m_PathID)?.first;
                            if (!string.IsNullOrEmpty(replacename))
                            {
                                var ex = Path.GetExtension(replacename);
                                x.Text = !string.IsNullOrEmpty(ex) ? replacename.Replace(ex, "") : replacename;
                            }
                        });
                    }
                    exportableAssets.AddRange(assetsFile.exportableAssets);
                }

                visibleAssets = exportableAssets;
                assetsNameHash.Clear();
            }
            #endregion

            #region second loop - build tree structure
            if (buildHierarchy)
            {
                var gameObjectCount = assetsfileList.Sum(x => x.GameObjectList.Values.Count);
                if (gameObjectCount > 0)
                {
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(gameObjectCount);
                    StatusStripUpdate("Building tree structure...");

                    foreach (var assetsFile in assetsfileList)
                    {
                        var fileNode = new GameObjectTreeNode(null); //RootNode
                        fileNode.Text = assetsFile.fileName;

                        foreach (var m_GameObject in assetsFile.GameObjectList.Values)
                        {
                            foreach (var m_Component in m_GameObject.m_Components)
                            {
                                if (m_Component.m_FileID >= 0 && m_Component.m_FileID < assetsfileList.Count)
                                {
                                    var sourceFile = assetsfileList[m_Component.m_FileID];
                                    if (sourceFile.preloadTable.TryGetValue(m_Component.m_PathID, out var asset))
                                    {
                                        switch (asset.Type)
                                        {
                                            case ClassIDReference.Transform:
                                                {
                                                    m_GameObject.m_Transform = m_Component;
                                                    break;
                                                }
                                            case ClassIDReference.MeshRenderer:
                                                {
                                                    m_GameObject.m_MeshRenderer = m_Component;
                                                    break;
                                                }
                                            case ClassIDReference.MeshFilter:
                                                {
                                                    m_GameObject.m_MeshFilter = m_Component;
                                                    if (assetsfileList.TryGetPD(m_Component, out var assetPreloadData))
                                                    {
                                                        var m_MeshFilter = new MeshFilter(assetPreloadData);
                                                        if (assetsfileList.TryGetPD(m_MeshFilter.m_Mesh, out assetPreloadData))
                                                        {
                                                            assetPreloadData.gameObject = m_GameObject;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ClassIDReference.SkinnedMeshRenderer:
                                                {
                                                    m_GameObject.m_SkinnedMeshRenderer = m_Component;
                                                    if (assetsfileList.TryGetPD(m_Component, out var assetPreloadData))
                                                    {
                                                        var m_SkinnedMeshRenderer = new SkinnedMeshRenderer(assetPreloadData);
                                                        if (assetsfileList.TryGetPD(m_SkinnedMeshRenderer.m_Mesh, out assetPreloadData))
                                                        {
                                                            assetPreloadData.gameObject = m_GameObject;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case ClassIDReference.Animator:
                                                {
                                                    m_GameObject.m_Animator = m_Component;
                                                    asset.Text = m_GameObject.preloadData.Text;
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }

                            var parentNode = fileNode;

                            if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out var m_Transform))
                            {
                                if (assetsfileList.TryGetTransform(m_Transform.m_Father, out var m_Father))
                                {
                                    if (assetsfileList.TryGetGameObject(m_Father.m_GameObject, out var parentGameObject))
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
            #endregion

            #region build list of class strucutres
            if (buildClassStructures)
            {
                //group class structures by versionv
                foreach (var assetsFile in assetsfileList)
                {
                    if (AllTypeMap.TryGetValue(assetsFile.unityVersion, out var curVer))
                    {
                        foreach (var type in assetsFile.m_Type)
                        {
                            curVer[type.Key] = new TypeItem(type.Key, type.Value);
                        }
                    }
                    else
                    {
                        AllTypeMap.Add(assetsFile.unityVersion, assetsFile.m_Type.ToDictionary(x => x.Key, y => new TypeItem(y.Key, y.Value)));
                    }
                }
            }
            #endregion
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

        public static void ExportAssets(string savePath, List<AssetPreloadData> toExportAssets, int assetGroupSelectedIndex, bool openAfterExport)
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
                    try
                    {
                        switch (asset.Type)
                        {
                            case ClassIDReference.Texture2D:
                                if (ExportTexture2D(asset, exportpath, true))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.AudioClip:
                                if (ExportAudioClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.Shader:
                                if (ExportShader(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.TextAsset:
                                if (ExportTextAsset(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.MonoBehaviour:
                                if (ExportMonoBehaviour(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.Font:
                                if (ExportFont(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.Mesh:
                                if (ExportMesh(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.VideoClip:
                                if (ExportVideoClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.MovieTexture:
                                if (ExportMovieTexture(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.Sprite:
                                if (ExportSprite(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.Animator:
                                if (ExportAnimator(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ClassIDReference.AnimationClip:
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

        public static void ExportSplitObjects(string savePath, TreeNodeCollection nodes, bool isNew = false)
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
                        if (isNew)
                        {
                            try
                            {
                                ExportGameObject(j.gameObject, targetPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
                            }
                        }
                        else
                            FBXExporter.WriteFBX($"{targetPath}{filename}.fbx", gameObjects);
                        StatusStripUpdate($"Finished exporting {filename}.fbx");
                    }
                }
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

        public static void ExportAnimatorWithAnimationClip(AssetPreloadData animator, List<AssetPreloadData> animationList, string exportPath)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                StatusStripUpdate($"Exporting {animator.Text}");
                try
                {
                    ExportAnimator(animator, exportPath, animationList);
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

        public static void ExportObjectsWithAnimationClip(string exportPath, TreeNodeCollection nodes, List<AssetPreloadData> animationList = null)
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

        public static float[] QuatToEuler(float[] q)
        {
            double eax = 0;
            double eay = 0;
            double eaz = 0;

            float qx = q[0];
            float qy = q[1];
            float qz = q[2];
            float qw = q[3];

            double[,] M = new double[4, 4];

            double Nq = qx * qx + qy * qy + qz * qz + qw * qw;
            double s = (Nq > 0.0) ? (2.0 / Nq) : 0.0;
            double xs = qx * s, ys = qy * s, zs = qz * s;
            double wx = qw * xs, wy = qw * ys, wz = qw * zs;
            double xx = qx * xs, xy = qx * ys, xz = qx * zs;
            double yy = qy * ys, yz = qy * zs, zz = qz * zs;

            M[0, 0] = 1.0 - (yy + zz); M[0, 1] = xy - wz; M[0, 2] = xz + wy;
            M[1, 0] = xy + wz; M[1, 1] = 1.0 - (xx + zz); M[1, 2] = yz - wx;
            M[2, 0] = xz - wy; M[2, 1] = yz + wx; M[2, 2] = 1.0 - (xx + yy);
            M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0; M[3, 3] = 1.0;

            double test = Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);
            if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
            {
                eax = Math.Atan2(M[2, 1], M[2, 2]);
                eay = Math.Atan2(-M[2, 0], test);
                eaz = Math.Atan2(M[1, 0], M[0, 0]);
            }
            else
            {
                eax = Math.Atan2(-M[1, 2], M[1, 1]);
                eay = Math.Atan2(-M[2, 0], test);
                eaz = 0;
            }

            return new[] { (float)(eax * 180 / Math.PI), (float)(eay * 180 / Math.PI), (float)(eaz * 180 / Math.PI) };
        }

        public static string GetScriptString(AssetPreloadData assetPreloadData)
        {
            if (!moduleLoaded)
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

                moduleLoaded = true;
            }
            var m_MonoBehaviour = new MonoBehaviour(assetPreloadData);
            var sb = new StringBuilder();
            sb.AppendLine("PPtr<GameObject> m_GameObject");
            sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_GameObject.m_FileID}");
            sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_GameObject.m_PathID}");
            sb.AppendLine($"UInt8 m_Enabled = {m_MonoBehaviour.m_Enabled}");
            sb.AppendLine("PPtr<MonoScript> m_Script");
            sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_Script.m_FileID}");
            sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_Script.m_PathID}");
            sb.AppendLine($"string m_Name = \"{m_MonoBehaviour.m_Name}\"");
            if (assetsfileList.TryGetPD(m_MonoBehaviour.m_Script, out var script))
            {
                var m_Script = new MonoScript(script);
                if (!LoadedModuleDic.TryGetValue(m_Script.m_AssemblyName, out var module))
                {
                    /*using (var openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Title = $"Select {m_Script.m_AssemblyName}";
                        openFileDialog.FileName = m_Script.m_AssemblyName;
                        openFileDialog.Filter = $"{m_Script.m_AssemblyName}|{m_Script.m_AssemblyName}";
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            var moduleContext = new ModuleContext();
                            var asmResolver = new AssemblyResolver(moduleContext, true);
                            var resolver = new Resolver(asmResolver);
                            moduleContext.AssemblyResolver = asmResolver;
                            moduleContext.Resolver = resolver;
                            module = ModuleDefMD.Load(openFileDialog.FileName, moduleContext);
                            LoadedModule.Add(m_Script.m_AssemblyName, module);
                        }
                        else
                        {
                            return sb.ToString();
                        }
                    }*/
                    return sb.ToString();
                }
                var typeDef = module.Assembly.Find(m_Script.m_Namespace != "" ? $"{m_Script.m_Namespace}.{m_Script.m_ClassName}" : m_Script.m_ClassName, false);
                if (typeDef != null)
                {
                    try
                    {
                        DumpType(typeDef.ToTypeSig(), sb, assetPreloadData.sourceFile, null, -1, true);
                    }
                    catch
                    {
                        sb = new StringBuilder();
                        sb.AppendLine("PPtr<GameObject> m_GameObject");
                        sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_GameObject.m_FileID}");
                        sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_GameObject.m_PathID}");
                        sb.AppendLine($"UInt8 m_Enabled = {m_MonoBehaviour.m_Enabled}");
                        sb.AppendLine("PPtr<MonoScript> m_Script");
                        sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_Script.m_FileID}");
                        sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_Script.m_PathID}");
                        sb.AppendLine($"string m_Name = \"{m_MonoBehaviour.m_Name}\"");
                    }
                }
            }
            return sb.ToString();
        }

        private static void DumpType(TypeSig typeSig, StringBuilder sb, AssetsFile assetsFile, string name, int indent, bool isRoot = false)
        {
            var typeDef = typeSig.ToTypeDefOrRef().ResolveTypeDefThrow();
            var reader = assetsFile.reader;
            if (typeSig.IsPrimitive)
            {
                object value = null;
                switch (typeSig.TypeName)
                {
                    case "Boolean":
                        value = reader.ReadBoolean();
                        break;
                    case "Byte":
                        value = reader.ReadByte();
                        break;
                    case "SByte":
                        value = reader.ReadSByte();
                        break;
                    case "Int16":
                        value = reader.ReadInt16();
                        break;
                    case "UInt16":
                        value = reader.ReadUInt16();
                        break;
                    case "Int32":
                        value = reader.ReadInt32();
                        break;
                    case "UInt32":
                        value = reader.ReadUInt32();
                        break;
                    case "Int64":
                        value = reader.ReadInt64();
                        break;
                    case "UInt64":
                        value = reader.ReadUInt64();
                        break;
                    case "Single":
                        value = reader.ReadSingle();
                        break;
                    case "Double":
                        value = reader.ReadDouble();
                        break;
                    case "Char":
                        value = reader.ReadChar();
                        break;
                }
                reader.AlignStream(4);
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = {value}");
                return;
            }
            if (typeSig.FullName == "System.String")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = \"{reader.ReadAlignedString()}\"");
                return;
            }
            if (typeSig.FullName == "System.Object")
            {
                return;
            }
            if (typeDef.IsDelegate)
            {
                return;
            }
            if (typeSig is ArraySigBase)
            {
                if (!typeDef.IsEnum && !IsBaseType(typeDef) && !IsAssignFromUnityObject(typeDef) && !IsEngineType(typeDef) && !typeDef.IsSerializable)
                {
                    return;
                }
                var size = reader.ReadInt32();
                sb.AppendLine($"{new string('\t', indent)}{typeSig.TypeName} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}Array Array");
                sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                for (int i = 0; i < size; i++)
                {
                    sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                    DumpType(typeDef.ToTypeSig(), sb, assetsFile, "data", indent + 2);
                }
                return;
            }
            if (!isRoot && typeSig is GenericInstSig genericInstSig)
            {
                if (genericInstSig.GenericArguments.Count == 1)
                {
                    var type = genericInstSig.GenericArguments[0].ToTypeDefOrRef().ResolveTypeDefThrow();
                    if (!type.IsEnum && !IsBaseType(type) && !IsAssignFromUnityObject(type) && !IsEngineType(type) && !type.IsSerializable)
                    {
                        return;
                    }
                    var size = reader.ReadInt32();
                    sb.AppendLine($"{new string('\t', indent)}{typeSig.TypeName} {name}");
                    sb.AppendLine($"{new string('\t', indent + 1)}Array Array");
                    sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                    for (int i = 0; i < size; i++)
                    {
                        sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                        DumpType(genericInstSig.GenericArguments[0], sb, assetsFile, "data", indent + 2);
                    }
                }
                return;
            }
            if (indent != -1 && IsAssignFromUnityObject(typeDef))
            {
                var pptr = assetsFile.ReadPPtr();
                sb.AppendLine($"{new string('\t', indent)}PPtr<{typeDef.Name}> {name} = {{fileID: {pptr.m_FileID}, pathID: {pptr.m_PathID}}}");
                return;
            }
            if (typeDef.IsEnum)
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = {reader.ReadUInt32()}");
                return;
            }
            if (indent != -1 && !IsEngineType(typeDef) && !typeDef.IsSerializable)
            {
                return;
            }
            if (typeDef.FullName == "UnityEngine.Rect")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var rect = reader.ReadSingleArray(4);
                return;
            }
            if (typeDef.FullName == "UnityEngine.LayerMask")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var value = reader.ReadInt32();
                return;
            }
            if (typeDef.FullName == "UnityEngine.AnimationCurve")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var animationCurve = new AnimationCurve<float>(reader, reader.ReadSingle, assetsFile.version);
                return;
            }
            if (typeDef.FullName == "UnityEngine.Gradient")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                if (assetsFile.version[0] == 5 && assetsFile.version[1] < 5)
                    reader.Position += 68;
                else if (assetsFile.version[0] == 5 && assetsFile.version[1] < 6)
                    reader.Position += 72;
                else
                    reader.Position += 168;
                return;
            }
            if (typeDef.FullName == "UnityEngine.RectOffset")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var left = reader.ReadSingle();
                var right = reader.ReadSingle();
                var top = reader.ReadSingle();
                var bottom = reader.ReadSingle();
                return;
            }
            if (typeDef.FullName == "UnityEngine.GUIStyle") //TODO
            {
                return;
            }
            if (typeDef.IsClass || typeDef.IsValueType)
            {
                if (name != null && indent != -1)
                {
                    sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                }
                if (indent == -1 && typeDef.BaseType.FullName != "UnityEngine.Object")
                {
                    DumpType(typeDef.BaseType.ToTypeSig(), sb, assetsFile, null, indent, true);
                }
                if (indent != -1 && typeDef.BaseType.FullName != "System.Object")
                {
                    DumpType(typeDef.BaseType.ToTypeSig(), sb, assetsFile, null, indent, true);
                }
                foreach (var fieldDef in typeDef.Fields)
                {
                    var access = fieldDef.Access & FieldAttributes.FieldAccessMask;
                    if (access != FieldAttributes.Public)
                    {
                        if (fieldDef.CustomAttributes.Any(x => x.TypeFullName.Contains("SerializeField")))
                        {
                            DumpType(fieldDef.FieldType, sb, assetsFile, fieldDef.Name, indent + 1);
                        }
                    }
                    else if ((fieldDef.Attributes & FieldAttributes.Static) == 0 && (fieldDef.Attributes & FieldAttributes.InitOnly) == 0 && (fieldDef.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        DumpType(fieldDef.FieldType, sb, assetsFile, fieldDef.Name, indent + 1);
                    }
                }
            }
        }

        private static bool IsAssignFromUnityObject(TypeDef typeDef)
        {
            if (typeDef.FullName == "UnityEngine.Object")
            {
                return true;
            }
            if (typeDef.BaseType != null)
            {
                if (typeDef.BaseType.FullName == "UnityEngine.Object")
                {
                    return true;
                }
                while (true)
                {
                    typeDef = typeDef.BaseType.ResolveTypeDefThrow();
                    if (typeDef.BaseType == null)
                    {
                        break;
                    }
                    if (typeDef.BaseType.FullName == "UnityEngine.Object")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsBaseType(IFullName typeDef)
        {
            switch (typeDef.FullName)
            {
                case "System.Boolean":
                case "System.Byte":
                case "System.SByte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Single":
                case "System.Double":
                case "System.String":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsEngineType(IFullName typeDef)
        {
            switch (typeDef.FullName)
            {
                case "UnityEngine.Vector2":
                case "UnityEngine.Vector3":
                case "UnityEngine.Vector4":
                case "UnityEngine.Rect":
                case "UnityEngine.Quaternion":
                case "UnityEngine.Matrix4x4":
                case "UnityEngine.Color":
                case "UnityEngine.Color32":
                case "UnityEngine.LayerMask":
                case "UnityEngine.AnimationCurve":
                case "UnityEngine.Gradient":
                case "UnityEngine.RectOffset":
                case "UnityEngine.GUIStyle":
                    return true;
                default:
                    return false;
            }
        }
    }
}
