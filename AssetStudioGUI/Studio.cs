using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AssetStudio;
using static AssetStudioGUI.Exporter;
using Object = AssetStudio.Object;

namespace AssetStudioGUI
{
    internal static class Studio
    {
        public static AssetsManager assetsManager = new AssetsManager();
        public static ScriptDumper scriptDumper = new ScriptDumper();
        public static List<AssetItem> exportableAssets = new List<AssetItem>();
        public static List<AssetItem> visibleAssets = new List<AssetItem>();

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
                    Progress.Report(i + 1, fileNames.Length);
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

        public static void BuildAssetList(Dictionary<Object, AssetItem> tempDic, bool displayAll, bool displayOriginalName, out string productName)
        {
            Logger.Info("Building asset list...");

            productName = string.Empty;
            var assetsNameHash = new HashSet<string>();
            var progressCount = assetsManager.assetsFileList.Sum(x => x.Objects.Count);
            int j = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                var tempExportableAssets = new List<AssetItem>();
                AssetBundle ab = null;
                foreach (var asset in assetsFile.Objects.Values)
                {
                    var assetItem = new AssetItem(asset);
                    tempDic.Add(asset, assetItem);
                    assetItem.UniqueID = " #" + j;
                    var exportable = false;
                    switch (asset)
                    {
                        case GameObject m_GameObject:
                            assetItem.Text = m_GameObject.m_Name;
                            break;
                        case Texture2D m_Texture2D:
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                            assetItem.Text = m_Texture2D.m_Name;
                            exportable = true;
                            break;
                        case AudioClip m_AudioClip:
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                            assetItem.Text = m_AudioClip.m_Name;
                            exportable = true;
                            break;
                        case VideoClip m_VideoClip:
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = asset.byteSize + (long)m_VideoClip.m_Size;
                            assetItem.Text = m_VideoClip.m_Name;
                            exportable = true;
                            break;
                        case Shader m_Shader:
                            assetItem.Text = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                            exportable = true;
                            break;
                        case Mesh _:
                        case TextAsset _:
                        case AnimationClip _:
                        case Font _:
                        case MovieTexture _:
                        case Sprite _:
                            assetItem.Text = ((NamedObject)asset).m_Name;
                            exportable = true;
                            break;
                        case Animator m_Animator:
                            if (m_Animator.m_GameObject.TryGet(out var gameObject))
                            {
                                assetItem.Text = gameObject.m_Name;
                            }
                            exportable = true;
                            break;
                        case MonoBehaviour m_MonoBehaviour:
                            if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var m_Script))
                            {
                                assetItem.Text = m_Script.m_ClassName;
                            }
                            else
                            {
                                assetItem.Text = m_MonoBehaviour.m_Name;
                            }
                            exportable = true;
                            break;
                        case PlayerSettings m_PlayerSettings:
                            productName = m_PlayerSettings.productName;
                            break;
                        case AssetBundle m_AssetBundle:
                            ab = m_AssetBundle;
                            assetItem.Text = ab.m_Name;
                            break;
                        case SpriteAtlas m_SpriteAtlas:
                            foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites)
                            {
                                if (m_PackedSprite.TryGet(out var m_Sprite))
                                {
                                    if (m_Sprite.m_SpriteAtlas.IsNull())
                                    {
                                        m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                    }
                                }
                            }
                            break;
                        case NamedObject m_NamedObject:
                            assetItem.Text = m_NamedObject.m_Name;
                            break;
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
                    foreach (var item in tempExportableAssets)
                    {
                        var originalPath = ab.m_Container.FirstOrDefault(y => y.Value.asset.m_PathID == item.Asset.m_PathID).Key;
                        if (!string.IsNullOrEmpty(originalPath))
                        {
                            var extension = Path.GetExtension(originalPath);
                            if (!string.IsNullOrEmpty(extension) && item.Type == ClassIDType.TextAsset)
                            {
                                item.Extension = extension;
                            }

                            item.Text = Path.GetDirectoryName(originalPath) + "\\" + Path.GetFileNameWithoutExtension(originalPath);
                            if (!assetsNameHash.Add((item.TypeString + item.Text).ToUpper()))
                            {
                                item.Text += item.UniqueID;
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

        public static List<TreeNode> BuildTreeStructure(Dictionary<Object, AssetItem> tempDic)
        {
            Logger.Info("Building tree structure...");

            var treeNodeCollection = new List<TreeNode>();
            var treeNodeDictionary = new Dictionary<GameObject, GameObjectTreeNode>();
            var progressCount = assetsManager.assetsFileList.Count;
            int i = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                var fileNode = new GameObjectTreeNode(assetsFile.fileName); //RootNode

                foreach (var obj in assetsFile.Objects.Values)
                {
                    if (obj is GameObject m_GameObject)
                    {
                        if (!treeNodeDictionary.TryGetValue(m_GameObject, out var currentNode))
                        {
                            currentNode = new GameObjectTreeNode(m_GameObject);
                            treeNodeDictionary.Add(m_GameObject, currentNode);
                        }

                        if (m_GameObject.m_MeshFilter != null)
                        {
                            if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                            {
                                var item = tempDic[m_Mesh];
                                item.TreeNode = currentNode;
                            }
                            tempDic[m_GameObject.m_MeshFilter].TreeNode = currentNode;
                        }

                        if (m_GameObject.m_SkinnedMeshRenderer != null)
                        {
                            if (m_GameObject.m_SkinnedMeshRenderer.m_Mesh.TryGet(out var m_Mesh))
                            {
                                var item = tempDic[m_Mesh];
                                item.TreeNode = currentNode;
                            }
                            tempDic[m_GameObject.m_SkinnedMeshRenderer].TreeNode = currentNode;
                        }

                        var parentNode = fileNode;

                        if (m_GameObject.m_Transform != null)
                        {
                            if (m_GameObject.m_Transform.m_Father.TryGet(out var m_Father))
                            {
                                if (m_Father.m_GameObject.TryGet(out var parentGameObject))
                                {
                                    if (!treeNodeDictionary.TryGetValue(parentGameObject, out parentNode))
                                    {
                                        parentNode = new GameObjectTreeNode(parentGameObject);
                                        treeNodeDictionary.Add(parentGameObject, parentNode);
                                    }
                                }
                            }
                        }

                        parentNode.Nodes.Add(currentNode);
                    }
                }

                if (fileNode.Nodes.Count > 0)
                {
                    treeNodeCollection.Add(fileNode);
                }

                Progress.Report(++i, progressCount);
            }

            treeNodeDictionary.Clear();

            return treeNodeCollection;
        }

        public static Dictionary<string, SortedDictionary<int, TypeTreeItem>> BuildClassStructure()
        {
            var typeMap = new Dictionary<string, SortedDictionary<int, TypeTreeItem>>();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                if (typeMap.TryGetValue(assetsFile.unityVersion, out var curVer))
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
                    typeMap.Add(assetsFile.unityVersion, items);
                }
            }

            return typeMap;
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
                        exportpath += Path.GetFileNameWithoutExtension(asset.SourceFile.fullName) + "_export\\";
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

        public static void ExportSplitObjects(string savePath, TreeNodeCollection nodes, bool openAfterExport)
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
                        {
                            Progress.Report(++k, count);
                            continue;
                        }
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
                            MessageBox.Show($"Export GameObject:{j.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                        }

                        Progress.Report(++k, count);
                        Logger.Info($"Finished exporting {filename}.fbx");
                    }
                }
                if (openAfterExport)
                {
                    Process.Start(savePath);
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

        public static void ExportAnimatorWithAnimationClip(AssetItem animator, List<AssetItem> animationList, string exportPath, bool openAfterExport)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Progress.Reset();
                Logger.Info($"Exporting {animator.Text}");
                try
                {
                    ExportAnimator(animator, exportPath, animationList);
                    if (openAfterExport)
                    {
                        Process.Start(exportPath);
                    }
                    Progress.Report(1, 1);
                    Logger.Info($"Finished exporting {animator.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export Animator:{animator.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                    Logger.Info("Error in export");
                }
            });
        }

        public static void ExportObjectsWithAnimationClip(string exportPath, TreeNodeCollection nodes, bool openAfterExport, List<AssetItem> animationList = null)
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
                            MessageBox.Show($"Export GameObject:{gameObject.m_Name} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                            Logger.Info("Error in export");
                        }

                        Progress.Report(++i, count);
                    }
                    if (openAfterExport)
                    {
                        Process.Start(exportPath);
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
            if (scriptDumper == null)
            {
                var openFolderDialog = new OpenFolderDialog();
                openFolderDialog.Title = "Select Assembly Folder";
                if (openFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    scriptDumper = new ScriptDumper(openFolderDialog.Folder);
                }
                else
                {
                    scriptDumper = new ScriptDumper();
                }
            }

            return scriptDumper.DumpScript(reader);
        }
    }
}
