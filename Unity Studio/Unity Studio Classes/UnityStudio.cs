using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace Unity_Studio
{
    internal static class UnityStudio
    {
        public static List<AssetsFile> assetsfileList = new List<AssetsFile>(); //loaded files
        public static Dictionary<string, int> sharedFileIndex = new Dictionary<string, int>(); //to improve the loading speed
        public static Dictionary<string, EndianBinaryReader> resourceFileReaders = new Dictionary<string, EndianBinaryReader>(); //use for read res files
        public static List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>(); //used to hold all assets while the ListView is filtered
        private static HashSet<string> exportableAssetsHash = new HashSet<string>(); //avoid the same name asset
        public static List<AssetPreloadData> visibleAssets = new List<AssetPreloadData>(); //used to build the ListView from all or filtered assets

        public static string productName = "";
        public static string mainPath = "";
        public static List<GameObject> fileNodes = new List<GameObject>();

        public static Dictionary<string, Dictionary<string, string>> jsonMats;
        public static Dictionary<string, SortedDictionary<int, ClassStruct>> AllClassStructures = new Dictionary<string, SortedDictionary<int, ClassStruct>>();

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

        public static int ExtractBundleFile(string bundleFileName)
        {
            int extractedCount = 0;
            if (CheckFileType(bundleFileName, out var reader) == FileType.BundleFile)
            {
                StatusStripUpdate($"Decompressing {Path.GetFileName(bundleFileName)} ...");
                var extractPath = bundleFileName + "_unpacked\\";
                Directory.CreateDirectory(extractPath);
                var bundleFile = new BundleFile(reader);
                foreach (var memFile in bundleFile.fileList)
                {
                    var filePath = extractPath + memFile.fileName.Replace('/', '\\');
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    if (!File.Exists(filePath))
                    {
                        StatusStripUpdate($"Extracting {Path.GetFileName(memFile.fileName)}");
                        extractedCount += 1;
                        using (var file = File.Create(filePath))
                        {
                            memFile.stream.WriteTo(file);
                            memFile.stream.Close();
                        }
                    }
                }
            }
            reader.Dispose();
            return extractedCount;
        }

        public static void BuildAssetStructures(bool loadAssetsMenuItem, bool displayAll, bool buildHierarchyMenuItem, bool buildClassStructuresMenuItem, bool displayOriginalName)
        {
            #region first loop - read asset data & create list
            if (loadAssetsMenuItem)
            {
                SetProgressBarValue(0);
                SetProgressBarMaximum(assetsfileList.Sum(x => x.preloadTable.Values.Count));
                string fileIDfmt = "D" + assetsfileList.Count.ToString().Length;

                for (var i = 0; i < assetsfileList.Count; i++)
                {
                    var assetsFile = assetsfileList[i];
                    StatusStripUpdate("Building asset list from " + Path.GetFileName(assetsFile.filePath));

                    string fileID = i.ToString(fileIDfmt);
                    AssetBundle ab = null;
                    foreach (var asset in assetsFile.preloadTable.Values)
                    {
                        asset.uniqueID = fileID + asset.uniqueID;
                        var exportable = false;
                        switch (asset.Type2)
                        {
                            case 1: //GameObject
                                {
                                    GameObject m_GameObject = new GameObject(asset);
                                    assetsFile.GameObjectList.Add(asset.m_PathID, m_GameObject);
                                    //totalTreeNodes++;
                                    break;
                                }
                            case 4: //Transform
                                {
                                    Transform m_Transform = new Transform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Transform);
                                    break;
                                }
                            case 224: //RectTransform
                                {
                                    RectTransform m_Rect = new RectTransform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Rect.m_Transform);
                                    break;
                                }
                            case 28: //Texture2D
                                {
                                    Texture2D m_Texture2D = new Texture2D(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 48: //Shader
                                {
                                    Shader m_Shader = new Shader(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 49: //TextAsset
                                {
                                    TextAsset m_TextAsset = new TextAsset(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 83: //AudioClip
                                {
                                    AudioClip m_AudioClip = new AudioClip(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 114: //MonoBehaviour
                                {
                                    var m_MonoBehaviour = new MonoBehaviour(asset, false);
                                    if (asset.Type1 != asset.Type2 && assetsFile.ClassStructures.ContainsKey(asset.Type1))
                                        exportable = true;
                                    break;
                                }
                            case 128: //Font
                                {
                                    UnityFont m_Font = new UnityFont(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 129: //PlayerSettings
                                {
                                    var plSet = new PlayerSettings(asset);
                                    productName = plSet.productName;
                                    break;
                                }
                            case 43: //Mesh
                                {
                                    Mesh m_Mesh = new Mesh(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 142: //AssetBundle
                                {
                                    ab = new AssetBundle(asset);
                                    break;
                                }
                            case 329: //VideoClip
                                {
                                    var m_VideoClip = new VideoClip(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 152: //MovieTexture
                                {
                                    var m_MovieTexture = new MovieTexture(asset, false);
                                    exportable = true;
                                    break;
                                }
                            case 213: //Sprite
                                {
                                    var m_Sprite = new Sprite(asset, false);
                                    exportable = true;
                                    break;
                                }
                                /*case 21: //Material                            
                                case 74: //AnimationClip
                                case 90: //Avatar
                                case 91: //AnimatorController
                                case 115: //MonoScript
                                case 687078895: //SpriteAtlas
                                    {
                                        if (asset.Offset + 4 > asset.sourceFile.a_Stream.BaseStream.Length)
                                            break;
                                        asset.sourceFile.a_Stream.Position = asset.Offset;
                                        var len = asset.sourceFile.a_Stream.ReadInt32();
                                        if (len > 0 && len < asset.Size - 4)
                                        {
                                            var bytes = asset.sourceFile.a_Stream.ReadBytes(len);
                                            asset.Text = Encoding.UTF8.GetString(bytes);
                                        }
                                        break;
                                    }*/
                        }
                        if (!exportable && displayAll)
                        {
                            asset.extension = ".dat";
                            exportable = true;
                        }
                        if (exportable)
                        {
                            if (asset.Text == "")
                            {
                                asset.Text = asset.TypeString + " #" + asset.uniqueID;
                            }
                            asset.SubItems.AddRange(new[] { asset.TypeString, asset.fullSize.ToString() });
                            //处理同名文件
                            if (!exportableAssetsHash.Add((asset.TypeString + asset.Text).ToUpper()))
                            {
                                asset.Text += " #" + asset.uniqueID;
                            }
                            //处理非法文件名
                            asset.Text = FixFileName(asset.Text);
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
                exportableAssetsHash.Clear();
            }
            #endregion

            #region second loop - build tree structure
            fileNodes = new List<GameObject>();
            if (buildHierarchyMenuItem)
            {
                SetProgressBarMaximum(1);
                SetProgressBarValue(1);
                SetProgressBarMaximum(assetsfileList.Sum(x => x.GameObjectList.Values.Count) + 1);
                foreach (var assetsFile in assetsfileList)
                {
                    StatusStripUpdate("Building tree structure from " + Path.GetFileName(assetsFile.filePath));
                    GameObject fileNode = new GameObject(null);
                    fileNode.Text = Path.GetFileName(assetsFile.filePath);
                    fileNode.m_Name = "RootNode";

                    foreach (var m_GameObject in assetsFile.GameObjectList.Values)
                    {
                        //ParseGameObject
                        foreach (var m_Component in m_GameObject.m_Components)
                        {
                            if (m_Component.m_FileID >= 0 && m_Component.m_FileID < assetsfileList.Count)
                            {
                                var sourceFile = assetsfileList[m_Component.m_FileID];
                                if (sourceFile.preloadTable.TryGetValue(m_Component.m_PathID, out var asset))
                                {
                                    switch (asset.Type2)
                                    {
                                        case 4: //Transform
                                            {
                                                m_GameObject.m_Transform = m_Component;
                                                break;
                                            }
                                        case 23: //MeshRenderer
                                            {
                                                m_GameObject.m_MeshRenderer = m_Component;
                                                break;
                                            }
                                        case 33: //MeshFilter
                                            {
                                                m_GameObject.m_MeshFilter = m_Component;
                                                break;
                                            }
                                        case 137: //SkinnedMeshRenderer
                                            {
                                                m_GameObject.m_SkinnedMeshRenderer = m_Component;
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                        //

                        var parentNode = fileNode;

                        if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out var m_Transform))
                        {
                            if (assetsfileList.TryGetTransform(m_Transform.m_Father, out var m_Father))
                            {
                                //GameObject Parent;
                                if (assetsfileList.TryGetGameObject(m_Father.m_GameObject, out parentNode))
                                {
                                    //parentNode = Parent;
                                }
                            }
                        }

                        parentNode.Nodes.Add(m_GameObject);
                        ProgressBarPerformStep();
                    }


                    if (fileNode.Nodes.Count == 0)
                    {
                        fileNode.Text += " (no children)";
                    }
                    fileNodes.Add(fileNode);
                }

                if (File.Exists(mainPath + "\\materials.json"))
                {
                    string matLine;
                    using (StreamReader reader = File.OpenText(mainPath + "\\materials.json"))
                    { matLine = reader.ReadToEnd(); }

                    jsonMats = new JavaScriptSerializer().Deserialize<Dictionary<string, Dictionary<string, string>>>(matLine);
                    //var jsonMats = new JavaScriptSerializer().DeserializeObject(matLine);
                }
            }
            #endregion

            #region build list of class strucutres
            if (buildClassStructuresMenuItem)
            {
                //group class structures by versionv
                foreach (var assetsFile in assetsfileList)
                {
                    if (AllClassStructures.TryGetValue(assetsFile.m_Version, out var curVer))
                    {
                        foreach (var uClass in assetsFile.ClassStructures)
                        {
                            curVer[uClass.Key] = uClass.Value;
                        }
                    }
                    else
                    {
                        AllClassStructures.Add(assetsFile.m_Version, assetsFile.ClassStructures);
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

        public static FileType CheckFileType(MemoryStream stream, out EndianBinaryReader reader)
        {
            reader = new EndianBinaryReader(stream);
            return CheckFileType(reader);
        }

        public static FileType CheckFileType(string fileName, out EndianBinaryReader reader)
        {
            reader = new EndianBinaryReader(File.OpenRead(fileName));
            return CheckFileType(reader);
        }

        public static FileType CheckFileType(EndianBinaryReader reader)
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
    }
}
