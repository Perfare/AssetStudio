using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Web.Script.Serialization;
using ManagedFbx;


namespace Unity_Studio
{
    internal static class UnityStudio
    {
        public static List<string> unityFiles = new List<string>(); //files to load
        public static HashSet<string> unityFilesHash = new HashSet<string>(); //improve performance
        public static List<AssetsFile> assetsfileList = new List<AssetsFile>(); //loaded files
        public static HashSet<string> assetsfileListHash = new HashSet<string>(); //improve performance
        public static Dictionary<string, EndianStream> assetsfileandstream = new Dictionary<string, EndianStream>(); //use for read res files
        public static List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>(); //used to hold all assets while the ListView is filtered
        private static HashSet<string> exportableAssetsHash = new HashSet<string>(); //improve performance
        public static List<AssetPreloadData> visibleAssets = new List<AssetPreloadData>(); //used to build the ListView from all or filtered assets

        public static string productName = "";
        public static string mainPath = "";
        public static List<GameObject> fileNodes = new List<GameObject>();

        private static Dictionary<string, Dictionary<string, string>> jsonMats;
        public static Dictionary<string, SortedDictionary<int, ClassStruct>> AllClassStructures = new Dictionary<string, SortedDictionary<int, ClassStruct>>();

        //UI
        public static Action<int> SetProgressBarValue;
        public static Action<int> SetProgressBarMaximum;
        public static Action ProgressBarPerformStep;
        public static Action<string> StatusStripUpdate;
        public static Action<int> ProgressBarMaximumAdd;

        public static void LoadAssetsFile(string fileName)
        {
            //var loadedAssetsFile = assetsfileList.Find(aFile => aFile.filePath == fileName);
            //if (loadedAssetsFile == null)
            if (!assetsfileListHash.Contains(fileName))
            {
                //open file here and pass the stream to facilitate loading memory files
                //also by keeping the stream as a property of AssetsFile, it can be used later on to read assets
                AssetsFile assetsFile = new AssetsFile(fileName, new EndianStream(File.OpenRead(fileName), EndianType.BigEndian));
                //if (Path.GetFileName(fileName) == "mainData") { mainDataFile = assetsFile; }

                //totalAssetCount += assetsFile.preloadTable.Count;

                assetsfileList.Add(assetsFile);
                assetsfileListHash.Add(fileName);

                #region for 2.6.x find mainData and get string version
                if (assetsFile.fileGen == 6 && Path.GetFileName(fileName) != "mainData")
                {
                    AssetsFile mainDataFile = assetsfileList.Find(aFile => aFile.filePath == Path.GetDirectoryName(fileName) + "\\mainData");
                    if (mainDataFile != null)
                    {
                        assetsFile.m_Version = mainDataFile.m_Version;
                        assetsFile.version = mainDataFile.version;
                        assetsFile.buildType = mainDataFile.buildType;
                    }
                    else if (File.Exists(Path.GetDirectoryName(fileName) + "\\mainData"))
                    {
                        mainDataFile = new AssetsFile(Path.GetDirectoryName(fileName) + "\\mainData", new EndianStream(File.OpenRead(Path.GetDirectoryName(fileName) + "\\mainData"), EndianType.BigEndian));

                        assetsFile.m_Version = mainDataFile.m_Version;
                        assetsFile.version = mainDataFile.version;
                        assetsFile.buildType = mainDataFile.buildType;
                    }
                }
                #endregion

                int value = 0;
                foreach (var sharedFile in assetsFile.sharedAssetsList)
                {
                    string sharedFilePath = Path.GetDirectoryName(fileName) + "\\" + sharedFile.fileName;
                    string sharedFileName = Path.GetFileName(sharedFile.fileName);

                    //var loadedSharedFile = assetsfileList.Find(aFile => aFile.filePath == sharedFilePath);
                    /*var loadedSharedFile = assetsfileList.Find(aFile => aFile.filePath.EndsWith(Path.GetFileName(sharedFile.fileName)));
                    if (loadedSharedFile != null) { sharedFile.Index = assetsfileList.IndexOf(loadedSharedFile); }
                    else if (File.Exists(sharedFilePath))
                    {
                        //progressBar1.Maximum += 1;
                        sharedFile.Index = assetsfileList.Count;
                        LoadAssetsFile(sharedFilePath);
                    }*/

                    //searching in unityFiles would preserve desired order, but...
                    //var quedSharedFile = unityFiles.Find(uFile => String.Equals(Path.GetFileName(uFile), sharedFileName, StringComparison.OrdinalIgnoreCase));
                    //if (quedSharedFile == null)
                    if (!unityFilesHash.Contains(sharedFileName))
                    {
                        //if (!File.Exists(sharedFilePath)) { sharedFilePath = Path.GetDirectoryName(fileName) + "\\" + sharedFileName; }
                        if (!File.Exists(sharedFilePath))
                        {
                            var findFiles = Directory.GetFiles(Path.GetDirectoryName(fileName), sharedFileName, SearchOption.AllDirectories);
                            if (findFiles.Length > 0) { sharedFilePath = findFiles[0]; }
                        }

                        if (File.Exists(sharedFilePath))
                        {
                            //this would get screwed if the file somehow fails to load
                            sharedFile.Index = unityFiles.Count;
                            unityFiles.Add(sharedFilePath);
                            unityFilesHash.Add(sharedFileName);
                            //progressBar1.Maximum++;
                            value++;
                        }
                    }
                    else
                    {
                        sharedFile.Index = unityFiles.IndexOf(sharedFilePath);
                    }
                }
                if (value > 0)
                    ProgressBarMaximumAdd(value);
            }
        }

        public static void LoadBundleFile(string bundleFileName)
        {
            StatusStripUpdate("Decompressing " + Path.GetFileName(bundleFileName) + "...");
            BundleFile b_File = new BundleFile(bundleFileName);

            List<AssetsFile> b_assetsfileList = new List<AssetsFile>();

            foreach (var memFile in b_File.MemoryAssetsFileList) //filter unity files
            {
                bool validAssetsFile = false;
                switch (Path.GetExtension(memFile.fileName))
                {
                    case ".assets":
                    case ".sharedAssets":
                        validAssetsFile = true;
                        break;
                    case "":
                        validAssetsFile = (memFile.fileName == "mainData" ||
                                            Regex.IsMatch(memFile.fileName, "level.*?") ||
                                            Regex.IsMatch(memFile.fileName, "CustomAssetBundle-.*?") ||
                                            Regex.IsMatch(memFile.fileName, "CAB-.*?") ||
                                            Regex.IsMatch(memFile.fileName, "BuildPlayer-.*?"));
                        break;
                }
                StatusStripUpdate("Loading " + memFile.fileName);
                //create dummy path to be used for asset extraction
                memFile.fileName = Path.GetDirectoryName(bundleFileName) + "\\" + memFile.fileName;

                AssetsFile assetsFile = new AssetsFile(memFile.fileName, new EndianStream(memFile.memStream, EndianType.BigEndian));
                if (assetsFile.fileGen == 6 && Path.GetFileName(bundleFileName) != "mainData") //2.6.x and earlier don't have a string version before the preload table
                {
                    //make use of the bundle file version
                    assetsFile.m_Version = b_File.versionEngine;
                    assetsFile.version = Array.ConvertAll((from Match m in Regex.Matches(assetsFile.m_Version, @"[0-9]") select m.Value).ToArray(), int.Parse);
                    assetsFile.buildType = b_File.versionEngine.Split(AssetsFile.buildTypeSplit, StringSplitOptions.RemoveEmptyEntries);
                }
                if (validAssetsFile)
                {
                    b_assetsfileList.Add(assetsFile);
                }
                assetsfileandstream[assetsFile.fileName] = assetsFile.a_Stream;
            }
            if (b_assetsfileList.Count > 0)
            {
                assetsfileList.AddRange(b_assetsfileList);
                foreach (var assetsFile in b_assetsfileList)
                {
                    foreach (var sharedFile in assetsFile.sharedAssetsList)
                    {
                        sharedFile.fileName = Path.GetDirectoryName(bundleFileName) + "\\" + sharedFile.fileName;
                        var loadedSharedFile = b_assetsfileList.Find(aFile => aFile.filePath == sharedFile.fileName);
                        if (loadedSharedFile != null)
                        {
                            sharedFile.Index = assetsfileList.IndexOf(loadedSharedFile);
                        }
                    }
                }
            }
        }

        public static void MergeSplitAssets(string dirPath)
        {
            string[] splitFiles = Directory.GetFiles(dirPath, "*.split0");
            foreach (var splitFile in splitFiles)
            {
                string destFile = Path.GetFileNameWithoutExtension(splitFile);
                string destPath = Path.GetDirectoryName(splitFile) + "\\";
                if (!File.Exists(destPath + destFile))
                {
                    string[] splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                    using (var destStream = File.Create(destPath + destFile))
                    {
                        for (int i = 0; i < splitParts.Length; i++)
                        {
                            string splitPart = destPath + destFile + ".split" + i;
                            using (var sourceStream = File.OpenRead(splitPart))
                                sourceStream.CopyTo(destStream); // You can pass the buffer size as second argument.
                        }
                    }
                }
            }
        }

        public static int extractBundleFile(string bundleFileName)
        {
            int extractedCount = 0;

            StatusStripUpdate("Decompressing " + Path.GetFileName(bundleFileName) + " ,,,");

            string extractPath = bundleFileName + "_unpacked\\";
            Directory.CreateDirectory(extractPath);

            BundleFile b_File = new BundleFile(bundleFileName);

            foreach (var memFile in b_File.MemoryAssetsFileList)
            {
                string filePath = extractPath + memFile.fileName.Replace('/', '\\');
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                }
                if (!File.Exists(filePath))
                {
                    StatusStripUpdate("Extracting " + Path.GetFileName(memFile.fileName));
                    extractedCount += 1;

                    using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        memFile.memStream.WriteTo(file);
                        memFile.memStream.Close();
                    }
                }
            }

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
                                    unityFont m_Font = new unityFont(asset, false);
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
                            case 21: //Material                            
                            case 74: //AnimationClip
                            case 90: //Avatar
                            case 91: //AnimatorController
                            case 115: //MonoScript
                            case 213: //Sprite
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
                                }
                        }
                        if (!exportable && displayAll)
                        {
                            if (asset.Text == "")
                            {
                                asset.Text = asset.TypeString + " #" + asset.uniqueID;
                            }
                            asset.extension = ".dat";
                            asset.SubItems.AddRange(new[] { asset.TypeString, asset.Size.ToString() });
                            exportable = true;
                        }
                        if (exportable)
                        {
                            if (!exportableAssetsHash.Add((asset.TypeString + asset.Text).ToUpper()))
                            {
                                asset.Text += " #" + asset.uniqueID;
                            }
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
                                x.Text = replacename.Replace(Path.GetExtension(replacename), "");
                        });
                    }
                    exportableAssets.AddRange(assetsFile.exportableAssets);
                    //if (assetGroup.Items.Count > 0) { listView1.Groups.Add(assetGroup); }
                }

                visibleAssets = exportableAssets;

                //will only work if ListView is visible
                exportableAssetsHash.Clear();
                //assetListView.EndUpdate();
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
                        assetsfileList.ParseGameObject(m_GameObject);

                        var parentNode = fileNode;

                        Transform m_Transform;
                        if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out m_Transform))
                        {
                            Transform m_Father;
                            if (assetsfileList.TryGetTransform(m_Transform.m_Father, out m_Father))
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
                    string matLine = "";
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
                    SortedDictionary<int, ClassStruct> curVer;
                    if (AllClassStructures.TryGetValue(assetsFile.m_Version, out curVer))
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

        public static void WriteFBX(string FBXfile, bool allNodes)
        {
            var timestamp = DateTime.Now;

            using (StreamWriter FBXwriter = new StreamWriter(FBXfile))
            {
                StringBuilder fbx = new StringBuilder();
                StringBuilder ob = new StringBuilder(); //Objects builder
                StringBuilder cb = new StringBuilder(); //Connections builder
                StringBuilder mb = new StringBuilder(); //Materials builder to get texture count in advance
                StringBuilder cb2 = new StringBuilder(); //and keep connections ordered
                cb.Append("\n}\n");//Objects end
                cb.Append("\nConnections:  {");

                HashSet<GameObject> GameObjects = new HashSet<GameObject>();
                HashSet<GameObject> LimbNodes = new HashSet<GameObject>();
                HashSet<AssetPreloadData> Skins = new HashSet<AssetPreloadData>();
                HashSet<AssetPreloadData> Meshes = new HashSet<AssetPreloadData>();//MeshFilters are not unique!!
                HashSet<AssetPreloadData> Materials = new HashSet<AssetPreloadData>();
                HashSet<AssetPreloadData> Textures = new HashSet<AssetPreloadData>();

                int DeformerCount = 0;
                /*
                uniqueIDs can begin with zero, so they are preceded by a number specific to their type
                this will also make it easier to debug FBX files
                1: Model
                2: NodeAttribute
                3: Geometry
                4: Deformer
                5: CollectionExclusive
                6: Material
                7: Texture
                8: Video
                9: 
                */

                #region loop nodes and collect objects for export
                foreach (var assetsFile in assetsfileList)
                {
                    foreach (var m_GameObject in assetsFile.GameObjectList.Values)
                    {
                        if (m_GameObject.Checked || allNodes)
                        {
                            GameObjects.Add(m_GameObject);

                            AssetPreloadData MeshFilterPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_MeshFilter, out MeshFilterPD))
                            {
                                //MeshFilters are not unique!
                                //MeshFilters.Add(MeshFilterPD);
                                MeshFilter m_MeshFilter = new MeshFilter(MeshFilterPD);
                                AssetPreloadData MeshPD;
                                if (assetsfileList.TryGetPD(m_MeshFilter.m_Mesh, out MeshPD))
                                {
                                    Meshes.Add(MeshPD);

                                    //write connections here and Mesh objects separately without having to backtrack through their MEshFilter to het the GameObject ID
                                    //also note that MeshFilters are not unique, they cannot be used for instancing geometry
                                    cb2.AppendFormat("\n\n\t;Geometry::, Model::{0}", m_GameObject.m_Name);
                                    cb2.AppendFormat("\n\tC: \"OO\",3{0},1{1}", MeshPD.uniqueID, m_GameObject.uniqueID);
                                }
                            }

                            #region get Renderer
                            AssetPreloadData RendererPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_MeshRenderer, out RendererPD))
                            {
                                MeshRenderer m_Renderer = new MeshRenderer(RendererPD);

                                foreach (var MaterialPPtr in m_Renderer.m_Materials)
                                {
                                    AssetPreloadData MaterialPD;
                                    if (assetsfileList.TryGetPD(MaterialPPtr, out MaterialPD))
                                    {
                                        Materials.Add(MaterialPD);
                                        cb2.AppendFormat("\n\n\t;Material::, Model::{0}", m_GameObject.m_Name);
                                        cb2.AppendFormat("\n\tC: \"OO\",6{0},1{1}", MaterialPD.uniqueID, m_GameObject.uniqueID);
                                    }
                                }
                            }
                            #endregion

                            #region get SkinnedMeshRenderer
                            AssetPreloadData SkinnedMeshPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_SkinnedMeshRenderer, out SkinnedMeshPD))
                            {
                                Skins.Add(SkinnedMeshPD);

                                SkinnedMeshRenderer m_SkinnedMeshRenderer = new SkinnedMeshRenderer(SkinnedMeshPD);

                                foreach (var MaterialPPtr in m_SkinnedMeshRenderer.m_Materials)
                                {
                                    AssetPreloadData MaterialPD;
                                    if (assetsfileList.TryGetPD(MaterialPPtr, out MaterialPD))
                                    {
                                        Materials.Add(MaterialPD);
                                        cb2.AppendFormat("\n\n\t;Material::, Model::{0}", m_GameObject.m_Name);
                                        cb2.AppendFormat("\n\tC: \"OO\",6{0},1{1}", MaterialPD.uniqueID, m_GameObject.uniqueID);
                                    }
                                }

                                if ((bool)Properties.Settings.Default["exportDeformers"])
                                {
                                    DeformerCount += m_SkinnedMeshRenderer.m_Bones.Length;

                                    //collect skeleton dummies to make sure they are exported
                                    foreach (var bonePPtr in m_SkinnedMeshRenderer.m_Bones)
                                    {
                                        Transform b_Transform;
                                        if (assetsfileList.TryGetTransform(bonePPtr, out b_Transform))
                                        {
                                            GameObject m_Bone;
                                            if (assetsfileList.TryGetGameObject(b_Transform.m_GameObject, out m_Bone))
                                            {
                                                LimbNodes.Add(m_Bone);
                                                //also collect the root bone
                                                if (m_Bone.Parent.Level > 0) { LimbNodes.Add((GameObject)m_Bone.Parent); }
                                                //should I collect siblings?
                                            }

                                            #region collect children because m_SkinnedMeshRenderer.m_Bones doesn't contain terminations
                                            foreach (var ChildPPtr in b_Transform.m_Children)
                                            {
                                                Transform ChildTR;
                                                if (assetsfileList.TryGetTransform(ChildPPtr, out ChildTR))
                                                {
                                                    GameObject m_Child;
                                                    if (assetsfileList.TryGetGameObject(ChildTR.m_GameObject, out m_Child))
                                                    {
                                                        //check that the Model doesn't contain a Mesh, although this won't ensure it's part of the skeleton
                                                        if (m_Child.m_MeshFilter == null && m_Child.m_SkinnedMeshRenderer == null)
                                                        {
                                                            LimbNodes.Add(m_Child);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                //if ((bool)Properties.Settings.Default["convertDummies"]) { GameObjects.Except(LimbNodes); }
                //else { GameObjects.UnionWith(LimbNodes); LimbNodes.Clear(); }
                //add either way and use LimbNodes to test if a node is Null or LimbNode
                GameObjects.UnionWith(LimbNodes);
                #endregion

                #region write Materials, collect Texture objects
                //StatusStripUpdate("Writing Materials");
                foreach (var MaterialPD in Materials)
                {
                    Material m_Material = new Material(MaterialPD);

                    mb.AppendFormat("\n\tMaterial: 6{0}, \"Material::{1}\", \"\" {{", MaterialPD.uniqueID, m_Material.m_Name);
                    mb.Append("\n\t\tVersion: 102");
                    mb.Append("\n\t\tShadingModel: \"phong\"");
                    mb.Append("\n\t\tMultiLayer: 0");
                    mb.Append("\n\t\tProperties70:  {");
                    mb.Append("\n\t\t\tP: \"ShadingModel\", \"KString\", \"\", \"\", \"phong\"");

                    #region write material colors
                    foreach (var m_Color in m_Material.m_Colors)
                    {
                        switch (m_Color.first)
                        {
                            case "_Color":
                            case "gSurfaceColor":
                                mb.AppendFormat("\n\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            case "_SpecularColor"://then what is _SpecColor??
                                mb.AppendFormat("\n\t\t\tP: \"SpecularColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            case "_ReflectColor":
                                mb.AppendFormat("\n\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            default:
                                mb.AppendFormat("\n;\t\t\tP: \"{3}\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2], m_Color.first);//commented out
                                break;
                        }
                    }
                    #endregion

                    #region write material parameters
                    foreach (var m_Float in m_Material.m_Floats)
                    {
                        switch (m_Float.first)
                        {
                            case "_Shininess":
                                mb.AppendFormat("\n\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                mb.AppendFormat("\n\t\t\tP: \"Shininess\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                break;
                            case "_Transparency":
                                mb.Append("\n\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",1,1,1");
                                mb.AppendFormat("\n\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                mb.AppendFormat("\n\t\t\tP: \"Opacity\", \"Number\", \"\", \"A\",{0}", (1 - m_Float.second));
                                break;
                            default:
                                mb.AppendFormat("\n;\t\t\tP: \"{0}\", \"Number\", \"\", \"A\",{1}", m_Float.first, m_Float.second);
                                break;
                        }
                    }
                    #endregion

                    //mb.Append("\n\t\t\tP: \"SpecularFactor\", \"Number\", \"\", \"A\",0");
                    mb.Append("\n\t\t}");
                    mb.Append("\n\t}");

                    #region write texture connections
                    foreach (var m_TexEnv in m_Material.m_TexEnvs)
                    {
                        AssetPreloadData TexturePD;
                        #region get Porsche material from json
                        if (!assetsfileList.TryGetPD(m_TexEnv.m_Texture, out TexturePD) && jsonMats != null)
                        {
                            Dictionary<string, string> matProp;
                            if (jsonMats.TryGetValue(m_Material.m_Name, out matProp))
                            {
                                string texName;
                                if (matProp.TryGetValue(m_TexEnv.name, out texName))
                                {
                                    foreach (var asset in exportableAssets)
                                    {
                                        if (asset.Type2 == 28 && asset.Text == texName)
                                        {
                                            TexturePD = asset;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (TexturePD != null && TexturePD.Type2 == 28)
                        {
                            Textures.Add(TexturePD);

                            cb2.AppendFormat("\n\n\t;Texture::, Material::{0}", m_Material.m_Name);
                            cb2.AppendFormat("\n\tC: \"OP\",7{0},6{1}, \"", TexturePD.uniqueID, MaterialPD.uniqueID);

                            switch (m_TexEnv.name)
                            {
                                case "_MainTex":
                                case "gDiffuseSampler":
                                    cb2.Append("DiffuseColor\"");
                                    break;
                                case "_SpecularMap":
                                case "gSpecularSampler":
                                    cb2.Append("SpecularColor\"");
                                    break;
                                case "_NormalMap":
                                case "gNormalSampler":
                                    cb2.Append("NormalMap\"");
                                    break;
                                case "_BumpMap":
                                    cb2.Append("Bump\"");
                                    break;
                                default:
                                    cb2.AppendFormat("{0}\"", m_TexEnv.name);
                                    break;
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                #region write generic FBX data after everything was collected
                fbx.Append("; FBX 7.1.0 project file");
                fbx.Append("\nFBXHeaderExtension:  {\n\tFBXHeaderVersion: 1003\n\tFBXVersion: 7100\n\tCreationTimeStamp:  {\n\t\tVersion: 1000");
                fbx.Append("\n\t\tYear: " + timestamp.Year);
                fbx.Append("\n\t\tMonth: " + timestamp.Month);
                fbx.Append("\n\t\tDay: " + timestamp.Day);
                fbx.Append("\n\t\tHour: " + timestamp.Hour);
                fbx.Append("\n\t\tMinute: " + timestamp.Minute);
                fbx.Append("\n\t\tSecond: " + timestamp.Second);
                fbx.Append("\n\t\tMillisecond: " + timestamp.Millisecond);
                fbx.Append("\n\t}\n\tCreator: \"Unity Studio by Chipicao\"\n}\n");

                fbx.Append("\nGlobalSettings:  {");
                fbx.Append("\n\tVersion: 1000");
                fbx.Append("\n\tProperties70:  {");
                fbx.Append("\n\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
                fbx.Append("\n\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
                fbx.Append("\n\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.AppendFormat("\n\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",{0}", Properties.Settings.Default["scaleFactor"]);
                fbx.Append("\n\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",1.0");
                //fbx.Append("\n\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
                //fbx.Append("\n\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
                //fbx.Append("\n\t\tP: \"TimeMode\", \"enum\", \"\", \"\",6");
                //fbx.Append("\n\t\tP: \"TimeProtocol\", \"enum\", \"\", \"\",2");
                //fbx.Append("\n\t\tP: \"SnapOnFrameMode\", \"enum\", \"\", \"\",0");
                //fbx.Append("\n\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
                //fbx.Append("\n\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",153953860000");
                //fbx.Append("\n\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
                //fbx.Append("\n\t\tP: \"TimeMarker\", \"Compound\", \"\", \"\"");
                //fbx.Append("\n\t\tP: \"CurrentTimeMarker\", \"int\", \"Integer\", \"\",-1");
                fbx.Append("\n\t}\n}\n");

                fbx.Append("\nDocuments:  {");
                fbx.Append("\n\tCount: 1");
                fbx.Append("\n\tDocument: 1234567890, \"\", \"Scene\" {");
                fbx.Append("\n\t\tProperties70:  {");
                fbx.Append("\n\t\t\tP: \"SourceObject\", \"object\", \"\", \"\"");
                fbx.Append("\n\t\t\tP: \"ActiveAnimStackName\", \"KString\", \"\", \"\", \"\"");
                fbx.Append("\n\t\t}");
                fbx.Append("\n\t\tRootNode: 0");
                fbx.Append("\n\t}\n}\n");
                fbx.Append("\nReferences:  {\n}\n");

                fbx.Append("\nDefinitions:  {");
                fbx.Append("\n\tVersion: 100");
                fbx.AppendFormat("\n\tCount: {0}", 1 + 2 * GameObjects.Count + Materials.Count + 2 * Textures.Count + ((bool)Properties.Settings.Default["exportDeformers"] ? Skins.Count + DeformerCount + Skins.Count + 1 : 0));

                fbx.Append("\n\tObjectType: \"GlobalSettings\" {");
                fbx.Append("\n\t\tCount: 1");
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Model\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", GameObjects.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"NodeAttribute\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", GameObjects.Count - Meshes.Count - Skins.Count);
                fbx.Append("\n\t\tPropertyTemplate: \"FbxNull\" {");
                fbx.Append("\n\t\t\tProperties70:  {");
                fbx.Append("\n\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
                fbx.Append("\n\t\t\t\tP: \"Size\", \"double\", \"Number\", \"\",100");
                fbx.Append("\n\t\t\t\tP: \"Look\", \"enum\", \"\", \"\",1");
                fbx.Append("\n\t\t\t}\n\t\t}\n\t}");

                fbx.Append("\n\tObjectType: \"Geometry\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Meshes.Count + Skins.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Material\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Materials.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Texture\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Textures.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Video\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Textures.Count);
                fbx.Append("\n\t}");

                if ((bool)Properties.Settings.Default["exportDeformers"])
                {
                    fbx.Append("\n\tObjectType: \"CollectionExclusive\" {");
                    fbx.AppendFormat("\n\t\tCount: {0}", Skins.Count);
                    fbx.Append("\n\t\tPropertyTemplate: \"FbxDisplayLayer\" {");
                    fbx.Append("\n\t\t\tProperties70:  {");
                    fbx.Append("\n\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
                    fbx.Append("\n\t\t\t\tP: \"Show\", \"bool\", \"\", \"\",1");
                    fbx.Append("\n\t\t\t\tP: \"Freeze\", \"bool\", \"\", \"\",0");
                    fbx.Append("\n\t\t\t\tP: \"LODBox\", \"bool\", \"\", \"\",0");
                    fbx.Append("\n\t\t\t}");
                    fbx.Append("\n\t\t}");
                    fbx.Append("\n\t}");

                    fbx.Append("\n\tObjectType: \"Deformer\" {");
                    fbx.AppendFormat("\n\t\tCount: {0}", DeformerCount + Skins.Count);
                    fbx.Append("\n\t}");

                    fbx.Append("\n\tObjectType: \"Pose\" {");
                    fbx.Append("\n\t\tCount: 1");
                    fbx.Append("\n\t}");
                }

                fbx.Append("\n}\n");
                fbx.Append("\nObjects:  {");

                FBXwriter.Write(fbx);
                fbx.Clear();
                #endregion

                #region write Model nodes and connections
                //StatusStripUpdate("Writing Nodes and hierarchy");
                foreach (var m_GameObject in GameObjects)
                {
                    if (m_GameObject.m_MeshFilter == null && m_GameObject.m_SkinnedMeshRenderer == null)
                    {
                        if ((bool)Properties.Settings.Default["exportDeformers"] && (bool)Properties.Settings.Default["convertDummies"] && LimbNodes.Contains(m_GameObject))
                        {
                            ob.AppendFormat("\n\tNodeAttribute: 2{0}, \"NodeAttribute::\", \"LimbNode\" {{", m_GameObject.uniqueID);
                            ob.Append("\n\t\tTypeFlags: \"Skeleton\"");
                            ob.Append("\n\t}");

                            ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"LimbNode\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                        }
                        else
                        {
                            ob.AppendFormat("\n\tNodeAttribute: 2{0}, \"NodeAttribute::\", \"Null\" {{", m_GameObject.uniqueID);
                            ob.Append("\n\t\tTypeFlags: \"Null\"");
                            ob.Append("\n\t}");

                            ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"Null\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                        }

                        //connect NodeAttribute to Model
                        cb.AppendFormat("\n\n\t;NodeAttribute::, Model::{0}", m_GameObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",2{0},1{0}", m_GameObject.uniqueID);
                    }
                    else
                    {
                        ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"Mesh\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                    }

                    ob.Append("\n\t\tVersion: 232");
                    ob.Append("\n\t\tProperties70:  {");
                    ob.Append("\n\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
                    ob.Append("\n\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                    ob.Append("\n\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");

                    Transform m_Transform;
                    if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out m_Transform))
                    {
                        float[] m_EulerRotation = QuatToEuler(new[] { m_Transform.m_LocalRotation[0], -m_Transform.m_LocalRotation[1], -m_Transform.m_LocalRotation[2], m_Transform.m_LocalRotation[3] });

                        ob.AppendFormat("\n\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",{0},{1},{2}", -m_Transform.m_LocalPosition[0], m_Transform.m_LocalPosition[1], m_Transform.m_LocalPosition[2]);
                        ob.AppendFormat("\n\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",{0},{1},{2}", m_EulerRotation[0], m_EulerRotation[1], m_EulerRotation[2]);//handedness is switched in quat
                        ob.AppendFormat("\n\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{0},{1},{2}", m_Transform.m_LocalScale[0], m_Transform.m_LocalScale[1], m_Transform.m_LocalScale[2]);
                    }

                    //mb.Append("\n\t\t\tP: \"UDP3DSMAX\", \"KString\", \"\", \"U\", \"MapChannel:1 = UVChannel_1&cr;&lf;MapChannel:2 = UVChannel_2&cr;&lf;\"");
                    //mb.Append("\n\t\t\tP: \"MaxHandle\", \"int\", \"Integer\", \"UH\",24");
                    ob.Append("\n\t\t}");
                    ob.Append("\n\t\tShading: T");
                    ob.Append("\n\t\tCulling: \"CullingOff\"\n\t}");

                    //connect Model to parent
                    GameObject parentObject = (GameObject)m_GameObject.Parent;
                    if (GameObjects.Contains(parentObject))
                    {
                        cb.AppendFormat("\n\n\t;Model::{0}, Model::{1}", m_GameObject.m_Name, parentObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",1{0},1{1}", m_GameObject.uniqueID, parentObject.uniqueID);
                    }
                    else
                    {
                        cb.AppendFormat("\n\n\t;Model::{0}, Model::RootNode", m_GameObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",1{0},0", m_GameObject.uniqueID);
                    }


                }
                #endregion

                #region write non-skinnned Geometry
                //StatusStripUpdate("Writing Geometry");
                foreach (var MeshPD in Meshes)
                {
                    Mesh m_Mesh = new Mesh(MeshPD, true);
                    MeshFBX(m_Mesh, MeshPD.uniqueID, ob);

                    //write data 8MB at a time
                    if (ob.Length > (8 * 0x100000))
                    { FBXwriter.Write(ob); ob.Clear(); }
                }
                #endregion

                #region write Deformer objects and skinned Geometry
                StringBuilder pb = new StringBuilder();
                //generate unique ID for BindPose
                pb.Append("\n\tPose: 5123456789, \"Pose::BIND_POSES\", \"BindPose\" {");
                pb.Append("\n\t\tType: \"BindPose\"");
                pb.Append("\n\t\tVersion: 100");
                pb.AppendFormat("\n\t\tNbPoseNodes: {0}", Skins.Count + LimbNodes.Count);

                foreach (var SkinnedMeshPD in Skins)
                {
                    SkinnedMeshRenderer m_SkinnedMeshRenderer = new SkinnedMeshRenderer(SkinnedMeshPD);

                    GameObject m_GameObject;
                    AssetPreloadData MeshPD;
                    if (assetsfileList.TryGetGameObject(m_SkinnedMeshRenderer.m_GameObject, out m_GameObject) && assetsfileList.TryGetPD(m_SkinnedMeshRenderer.m_Mesh, out MeshPD))
                    {
                        //generate unique Geometry ID for instanced mesh objects
                        //instanced skinned geometry is possible in FBX, but all instances are linked to the same skeleton nodes
                        //TODO: create instances if deformer option is not selected
                        //find a way to test if a mesh instance was loaded previously and if it uses the same skeleton, then create instance or copy
                        var keepID = MeshPD.uniqueID;
                        MeshPD.uniqueID = SkinnedMeshPD.uniqueID;
                        Mesh m_Mesh = new Mesh(MeshPD, true);
                        MeshFBX(m_Mesh, MeshPD.uniqueID, ob);

                        //write data 8MB at a time
                        if (ob.Length > (8 * 0x100000))
                        { FBXwriter.Write(ob); ob.Clear(); }

                        cb2.AppendFormat("\n\n\t;Geometry::, Model::{0}", m_GameObject.m_Name);
                        cb2.AppendFormat("\n\tC: \"OO\",3{0},1{1}", MeshPD.uniqueID, m_GameObject.uniqueID);

                        if ((bool)Properties.Settings.Default["exportDeformers"])
                        {
                            //add BindPose node
                            pb.Append("\n\t\tPoseNode:  {");
                            pb.AppendFormat("\n\t\t\tNode: 1{0}", m_GameObject.uniqueID);
                            //pb.Append("\n\t\t\tMatrix: *16 {");
                            //pb.Append("\n\t\t\t\ta: ");
                            //pb.Append("\n\t\t\t} ");
                            pb.Append("\n\t\t}");

                            ob.AppendFormat("\n\tCollectionExclusive: 5{0}, \"DisplayLayer::{1}\", \"DisplayLayer\" {{", SkinnedMeshPD.uniqueID, m_GameObject.m_Name);
                            ob.Append("\n\t\tProperties70:  {");
                            ob.Append("\n\t\t}");
                            ob.Append("\n\t}");

                            //connect Model to DisplayLayer
                            cb2.AppendFormat("\n\n\t;Model::{0}, DisplayLayer::", m_GameObject.m_Name);
                            cb2.AppendFormat("\n\tC: \"OO\",1{0},5{1}", m_GameObject.uniqueID, SkinnedMeshPD.uniqueID);

                            //write Deformers
                            if (m_Mesh.m_Skin.Length > 0 && m_Mesh.m_BindPose.Length >= m_SkinnedMeshRenderer.m_Bones.Length)
                            {
                                //write main Skin Deformer
                                ob.AppendFormat("\n\tDeformer: 4{0}, \"Deformer::\", \"Skin\" {{", SkinnedMeshPD.uniqueID);
                                ob.Append("\n\t\tVersion: 101");
                                ob.Append("\n\t\tLink_DeformAcuracy: 50");
                                ob.Append("\n\t}"); //Deformer end

                                //connect Skin Deformer to Geometry
                                cb2.Append("\n\n\t;Deformer::, Geometry::");
                                cb2.AppendFormat("\n\tC: \"OO\",4{0},3{1}", SkinnedMeshPD.uniqueID, MeshPD.uniqueID);

                                for (int b = 0; b < m_SkinnedMeshRenderer.m_Bones.Length; b++)
                                {
                                    Transform m_Transform;
                                    if (assetsfileList.TryGetTransform(m_SkinnedMeshRenderer.m_Bones[b], out m_Transform))
                                    {
                                        GameObject m_Bone;
                                        if (assetsfileList.TryGetGameObject(m_Transform.m_GameObject, out m_Bone))
                                        {
                                            int influences = 0, ibSplit = 0, wbSplit = 0;
                                            StringBuilder ib = new StringBuilder();//indices (vertex)
                                            StringBuilder wb = new StringBuilder();//weights

                                            for (int index = 0; index < m_Mesh.m_Skin.Length; index++)
                                            {
                                                if (m_Mesh.m_Skin[index][0].weight == 0 && (m_Mesh.m_Skin[index].All(x => x.weight == 0) || //if all weights (and indicces) are 0, bone0 has full control
                                                                                             m_Mesh.m_Skin[index][1].weight > 0)) //this implies a second bone exists, so bone0 has control too (otherwise it wouldn't be the first in the series)
                                                { m_Mesh.m_Skin[index][0].weight = 1; }

                                                var influence = m_Mesh.m_Skin[index].Find(x => x.boneIndex == b && x.weight > 0);
                                                if (influence != null)
                                                {
                                                    influences++;
                                                    ib.AppendFormat("{0},", index);
                                                    wb.AppendFormat("{0},", influence.weight);

                                                    if (ib.Length - ibSplit > 2000) { ib.Append("\n"); ibSplit = ib.Length; }
                                                    if (wb.Length - wbSplit > 2000) { wb.Append("\n"); wbSplit = wb.Length; }
                                                }

                                                /*float weight;
                                                if (m_Mesh.m_Skin[index].TryGetValue(b, out weight))
                                                {
                                                    if (weight > 0)
                                                    {
                                                        influences++;
                                                        ib.AppendFormat("{0},", index);
                                                        wb.AppendFormat("{0},", weight);
                                                    }
                                                    else if (m_Mesh.m_Skin[index].Keys.Count == 1)//m_Mesh.m_Skin[index].Values.All(x => x == 0)
                                                    {
                                                        influences++;
                                                        ib.AppendFormat("{0},", index);
                                                        wb.AppendFormat("{0},", 1);
                                                    }

                                                    if (ib.Length - ibSplit > 2000) { ib.Append("\n"); ibSplit = ib.Length; }
                                                    if (wb.Length - wbSplit > 2000) { wb.Append("\n"); wbSplit = wb.Length; }
                                                }*/
                                            }
                                            if (influences > 0)
                                            {
                                                ib.Length--;//remove last comma
                                                wb.Length--;//remove last comma
                                            }

                                            //SubDeformer objects need unique IDs because 2 or more deformers can be linked to the same bone
                                            ob.AppendFormat("\n\tDeformer: 4{0}{1}, \"SubDeformer::\", \"Cluster\" {{", b, SkinnedMeshPD.uniqueID);
                                            ob.Append("\n\t\tVersion: 100");
                                            ob.Append("\n\t\tUserData: \"\", \"\"");

                                            ob.AppendFormat("\n\t\tIndexes: *{0} {{\n\t\t\ta: ", influences);
                                            ob.Append(ib);
                                            ob.Append("\n\t\t}");
                                            ib.Clear();

                                            ob.AppendFormat("\n\t\tWeights: *{0} {{\n\t\t\ta: ", influences);
                                            ob.Append(wb);
                                            ob.Append("\n\t\t}");
                                            wb.Clear();

                                            ob.Append("\n\t\tTransform: *16 {\n\t\t\ta: ");
                                            //ob.Append(string.Join(",", m_Mesh.m_BindPose[b]));
                                            var m = m_Mesh.m_BindPose[b];
                                            ob.AppendFormat("{0},{1},{2},{3},", m[0, 0], -m[1, 0], -m[2, 0], m[3, 0]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 1], m[1, 1], m[2, 1], m[3, 1]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 2], m[1, 2], m[2, 2], m[3, 2]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 3], m[1, 3], m[2, 3], m[3, 3]);
                                            ob.Append("\n\t\t}");

                                            ob.Append("\n\t}"); //SubDeformer end

                                            //connect SubDeformer to Skin Deformer
                                            cb2.Append("\n\n\t;SubDeformer::, Deformer::");
                                            cb2.AppendFormat("\n\tC: \"OO\",4{0}{1},4{1}", b, SkinnedMeshPD.uniqueID);

                                            //connect dummy Model to SubDeformer
                                            cb2.AppendFormat("\n\n\t;Model::{0}, SubDeformer::", m_Bone.m_Name);
                                            cb2.AppendFormat("\n\tC: \"OO\",1{0},4{1}{2}", m_Bone.uniqueID, b, SkinnedMeshPD.uniqueID);
                                        }
                                    }
                                }
                            }
                        }

                        MeshPD.uniqueID = keepID;
                    }
                }

                if ((bool)Properties.Settings.Default["exportDeformers"])
                {
                    foreach (var m_Bone in LimbNodes)
                    {
                        //add BindPose node
                        pb.Append("\n\t\tPoseNode:  {");
                        pb.AppendFormat("\n\t\t\tNode: 1{0}", m_Bone.uniqueID);
                        //pb.Append("\n\t\t\tMatrix: *16 {");
                        //pb.Append("\n\t\t\t\ta: ");
                        //pb.Append("\n\t\t\t} ");
                        pb.Append("\n\t\t}");
                    }
                    pb.Append("\n\t}"); //BindPose end
                    ob.Append(pb); pb.Clear();
                }
                #endregion

                ob.Append(mb); mb.Clear();
                cb.Append(cb2); cb2.Clear();

                #region write & extract Textures
                foreach (var TexturePD in Textures)
                {
                    //TODO check texture type and set path accordingly; eg. CubeMap, Texture3D
                    string texPathName = Path.GetDirectoryName(FBXfile) + "\\Texture2D\\";
                    ExportTexture(TexturePD, texPathName, false);
                    texPathName = Path.GetFullPath(Path.Combine(texPathName, $"{TexturePD.Text}.png"));//必须是png文件
                    ob.AppendFormat("\n\tTexture: 7{0}, \"Texture::{1}\", \"\" {{", TexturePD.uniqueID, TexturePD.Text);
                    ob.Append("\n\t\tType: \"TextureVideoClip\"");
                    ob.Append("\n\t\tVersion: 202");
                    ob.AppendFormat("\n\t\tTextureName: \"Texture::{0}\"", TexturePD.Text);
                    ob.Append("\n\t\tProperties70:  {");
                    ob.Append("\n\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"UVChannel_0\"");
                    ob.Append("\n\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
                    ob.Append("\n\t\t}");
                    ob.AppendFormat("\n\t\tMedia: \"Video::{0}\"", TexturePD.Text);
                    ob.AppendFormat("\n\t\tFileName: \"{0}\"", texPathName);
                    ob.AppendFormat("\n\t\tRelativeFilename: \"{0}\"", texPathName.Replace($"{Path.GetDirectoryName(FBXfile)}\\", ""));
                    ob.Append("\n\t}");

                    ob.AppendFormat("\n\tVideo: 8{0}, \"Video::{1}\", \"Clip\" {{", TexturePD.uniqueID, TexturePD.Text);
                    ob.Append("\n\t\tType: \"Clip\"");
                    ob.Append("\n\t\tProperties70:  {");
                    ob.AppendFormat("\n\t\t\tP: \"Path\", \"KString\", \"XRefUrl\", \"\", \"{0}\"", texPathName);
                    ob.Append("\n\t\t}");
                    ob.AppendFormat("\n\t\tFileName: \"{0}\"", texPathName);
                    ob.AppendFormat("\n\t\tRelativeFilename: \"{0}\"", texPathName.Replace($"{Path.GetDirectoryName(FBXfile)}\\", ""));
                    ob.Append("\n\t}");

                    //connect video to texture
                    cb.AppendFormat("\n\n\t;Video::{0}, Texture::{0}", TexturePD.Text);
                    cb.AppendFormat("\n\tC: \"OO\",8{0},7{1}", TexturePD.uniqueID, TexturePD.uniqueID);
                }
                #endregion

                FBXwriter.Write(ob);
                ob.Clear();

                cb.Append("\n}");//Connections end
                FBXwriter.Write(cb);
                cb.Clear();

                StatusStripUpdate("Finished exporting " + Path.GetFileName(FBXfile));
            }
        }

        private static void MeshFBX(Mesh m_Mesh, string MeshID, StringBuilder ob)
        {
            if (m_Mesh.m_VertexCount > 0)//general failsafe
            {
                //StatusStripUpdate("Writing Geometry: " + m_Mesh.m_Name);

                ob.AppendFormat("\n\tGeometry: 3{0}, \"Geometry::\", \"Mesh\" {{", MeshID);
                ob.Append("\n\t\tProperties70:  {");
                var randomColor = RandomColorGenerator(m_Mesh.m_Name);
                ob.AppendFormat("\n\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",{0},{1},{2}", ((float)randomColor[0] / 255), ((float)randomColor[1] / 255), ((float)randomColor[2] / 255));
                ob.Append("\n\t\t}");

                #region Vertices
                ob.AppendFormat("\n\t\tVertices: *{0} {{\n\t\t\ta: ", m_Mesh.m_VertexCount * 3);

                int c = 3;//vertex components
                          //skip last component in vector4
                if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4) { c++; } //haha

                int lineSplit = ob.Length;
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Vertices[v * c], m_Mesh.m_Vertices[v * c + 1], m_Mesh.m_Vertices[v * c + 2]);

                    if (ob.Length - lineSplit > 2000)
                    {
                        ob.Append("\n");
                        lineSplit = ob.Length;
                    }
                }
                ob.Length--;//remove last comma
                ob.Append("\n\t\t}");
                #endregion

                #region Indices
                //in order to test topology for triangles/quads we need to store submeshes and write each one as geometry, then link to Mesh Node
                ob.AppendFormat("\n\t\tPolygonVertexIndex: *{0} {{\n\t\t\ta: ", m_Mesh.m_Indices.Count);

                lineSplit = ob.Length;
                for (int f = 0; f < m_Mesh.m_Indices.Count / 3; f++)
                {
                    ob.AppendFormat("{0},{1},{2},", m_Mesh.m_Indices[f * 3], m_Mesh.m_Indices[f * 3 + 2], (-m_Mesh.m_Indices[f * 3 + 1] - 1));

                    if (ob.Length - lineSplit > 2000)
                    {
                        ob.Append("\n");
                        lineSplit = ob.Length;
                    }
                }
                ob.Length--;//remove last comma

                ob.Append("\n\t\t}");
                ob.Append("\n\t\tGeometryVersion: 124");
                #endregion

                #region Normals
                if ((bool)Properties.Settings.Default["exportNormals"] && m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementNormal: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tNormals: *{0} {{\n\t\t\ta: ", (m_Mesh.m_VertexCount * 3));

                    if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3) { c = 3; }
                    else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4) { c = 4; }

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Normals[v * c], m_Mesh.m_Normals[v * c + 1], m_Mesh.m_Normals[v * c + 2]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Tangents
                if ((bool)Properties.Settings.Default["exportTangents"] && m_Mesh.m_Tangents != null && m_Mesh.m_Tangents.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementTangent: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tTangents: *{0} {{\n\t\t\ta: ", (m_Mesh.m_VertexCount * 3));

                    if (m_Mesh.m_Tangents.Length == m_Mesh.m_VertexCount * 3) { c = 3; }
                    else if (m_Mesh.m_Tangents.Length == m_Mesh.m_VertexCount * 4) { c = 4; }

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Tangents[v * c], m_Mesh.m_Tangents[v * c + 1], m_Mesh.m_Tangents[v * c + 2]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Colors
                if ((bool)Properties.Settings.Default["exportColors"] && m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementColor: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    //ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    //ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByPolygonVertex\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"IndexToDirect\"");
                    ob.AppendFormat("\n\t\t\tColors: *{0} {{\n\t\t\ta: ", m_Mesh.m_Colors.Length);
                    //ob.Append(string.Join(",", m_Mesh.m_Colors));

                    lineSplit = ob.Length;
                    if (m_Mesh.m_Colors.Length == m_Mesh.m_VertexCount * 3)
                    {
                        for (int i = 0; i < m_Mesh.m_VertexCount; i++)
                        {
                            ob.AppendFormat("{0},{1},{2},{3},", m_Mesh.m_Colors[i * 3], m_Mesh.m_Colors[i * 3 + 1], m_Mesh.m_Colors[i * 3 + 2], 1.0f);
                            if (ob.Length - lineSplit > 2000)
                            {
                                ob.Append("\n");
                                lineSplit = ob.Length;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < m_Mesh.m_VertexCount; i++)
                        {
                            ob.AppendFormat("{0},{1},{2},{3},", m_Mesh.m_Colors[i * 4], m_Mesh.m_Colors[i * 4 + 1], m_Mesh.m_Colors[i * 4 + 2], m_Mesh.m_Colors[i * 4 + 3]);
                            if (ob.Length - lineSplit > 2000)
                            {
                                ob.Append("\n");
                                lineSplit = ob.Length;
                            }
                        }
                    }
                    ob.Length--;//remove last comma

                    ob.Append("\n\t\t\t}");
                    ob.AppendFormat("\n\t\t\tColorIndex: *{0} {{\n\t\t\ta: ", m_Mesh.m_Indices.Count);

                    lineSplit = ob.Length;
                    for (int f = 0; f < m_Mesh.m_Indices.Count / 3; f++)
                    {
                        ob.AppendFormat("{0},{1},{2},", m_Mesh.m_Indices[f * 3], m_Mesh.m_Indices[f * 3 + 2], m_Mesh.m_Indices[f * 3 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma

                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region UV1
                //does FBX support UVW coordinates?
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_1\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV1.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV1[v * 2], 1 - m_Mesh.m_UV1[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV2
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 1 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_2\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV2.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV2[v * 2], 1 - m_Mesh.m_UV2[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV3
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV3 != null && m_Mesh.m_UV3.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 2 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_3\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV3.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV3[v * 2], 1 - m_Mesh.m_UV3[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV4
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV4 != null && m_Mesh.m_UV4.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 3 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_4\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV4.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV4[v * 2], 1 - m_Mesh.m_UV4[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Material
                ob.Append("\n\t\tLayerElementMaterial: 0 {");
                ob.Append("\n\t\t\tVersion: 101");
                ob.Append("\n\t\t\tName: \"\"");
                ob.Append("\n\t\t\tMappingInformationType: \"");
                if (m_Mesh.m_SubMeshes.Count == 1) { ob.Append("AllSame\""); }
                else { ob.Append("ByPolygon\""); }
                ob.Append("\n\t\t\tReferenceInformationType: \"IndexToDirect\"");
                ob.AppendFormat("\n\t\t\tMaterials: *{0} {{", m_Mesh.m_materialIDs.Count);
                ob.Append("\n\t\t\t\t");
                if (m_Mesh.m_SubMeshes.Count == 1) { ob.Append("0"); }
                else
                {
                    lineSplit = ob.Length;
                    for (int i = 0; i < m_Mesh.m_materialIDs.Count; i++)
                    {
                        ob.AppendFormat("{0},", m_Mesh.m_materialIDs[i]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                }
                ob.Append("\n\t\t\t}\n\t\t}");
                #endregion

                #region Layers
                ob.Append("\n\t\tLayer: 0 {");
                ob.Append("\n\t\t\tVersion: 100");
                if ((bool)Properties.Settings.Default["exportNormals"] && m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementNormal\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                if ((bool)Properties.Settings.Default["exportTangents"] && m_Mesh.m_Tangents != null && m_Mesh.m_Tangents.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementTangent\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementMaterial\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");
                //
                /*ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementTexture\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");
                ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementBumpTextures\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");*/
                if ((bool)Properties.Settings.Default["exportColors"] && m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementColor\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                ob.Append("\n\t\t}"); //Layer 0 end

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 1 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 1");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 1 end
                }

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV3 != null && m_Mesh.m_UV3.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 2 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 2");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 2 end
                }

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV4 != null && m_Mesh.m_UV4.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 3 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 3");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 3 end
                }
                #endregion

                ob.Append("\n\t}"); //Geometry end
            }
        }

        private static float[] QuatToEuler(float[] q)
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

            return new float[3] { (float)(eax * 180 / Math.PI), (float)(eay * 180 / Math.PI), (float)(eaz * 180 / Math.PI) };
        }

        private static byte[] RandomColorGenerator(string name)
        {
            int nameHash = name.GetHashCode();
            Random r = new Random(nameHash);
            //Random r = new Random(DateTime.Now.Millisecond);

            byte red = (byte)r.Next(0, 255);
            byte green = (byte)r.Next(0, 255);
            byte blue = (byte)r.Next(0, 255);

            return new byte[3] { red, green, blue };
        }

        public static void ExportRawFile(AssetPreloadData asset, string exportFilepath)
        {
            asset.sourceFile.a_Stream.Position = asset.Offset;
            var bytes = asset.sourceFile.a_Stream.ReadBytes(asset.Size);
            File.WriteAllBytes(exportFilepath, bytes);
        }

        public static bool ExportTexture(AssetPreloadData asset, string exportPathName, bool flip)
        {
            var m_Texture2D = new Texture2D(asset, true);
            var convert = (bool)Properties.Settings.Default["convertTexture"];
            if (convert)
            {
                ImageFormat format = null;
                var ext = (string)Properties.Settings.Default["convertType"];
                if (ext == "BMP")
                    format = ImageFormat.Bmp;
                else if (ext == "PNG")
                    format = ImageFormat.Png;
                else if (ext == "JPEG")
                    format = ImageFormat.Jpeg;
                var exportFullName = exportPathName + asset.Text + "." + ext.ToLower();
                if (ExportFileExists(exportFullName))
                    return false;
                var bitmap = m_Texture2D.ConvertToBitmap(flip);
                if (bitmap != null)
                {
                    bitmap.Save(exportFullName, format);
                    bitmap.Dispose();
                    return true;
                }
            }
            var exportFullName2 = exportPathName + asset.Text + asset.extension;
            if (ExportFileExists(exportFullName2))
                return false;
            File.WriteAllBytes(exportFullName2, m_Texture2D.ConvertToContainer());
            return true;
        }

        public static bool ExportAudioClip(AssetPreloadData asset, string exportFilename, string exportFileextension)
        {
            var oldextension = exportFileextension;
            var convertfsb = (bool)Properties.Settings.Default["convertfsb"];
            if (convertfsb && exportFileextension == ".fsb")
            {
                exportFileextension = ".wav";
            }
            var exportFullname = exportFilename + exportFileextension;
            if (ExportFileExists(exportFullname))
                return false;
            var m_AudioClip = new AudioClip(asset, true);
            if (convertfsb && oldextension == ".fsb")
            {
                FMOD.System system;
                FMOD.Sound sound;
                FMOD.RESULT result;
                FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();

                result = FMOD.Factory.System_Create(out system);
                if (result != FMOD.RESULT.OK) { return false; }

                result = system.setOutput(FMOD.OUTPUTTYPE.NOSOUND_NRT);
                if (result != FMOD.RESULT.OK) { return false; }

                result = system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
                if (result != FMOD.RESULT.OK) { return false; }

                exinfo.cbsize = Marshal.SizeOf(exinfo);
                exinfo.length = (uint)m_AudioClip.m_Size;

                result = system.createSound(m_AudioClip.m_AudioData, FMOD.MODE.OPENMEMORY, ref exinfo, out sound);
                if (result != FMOD.RESULT.OK) { return false; }

                result = sound.getSubSound(0, out sound);
                if (result != FMOD.RESULT.OK) { return false; }

                result = sound.setMode(FMOD.MODE.LOOP_OFF);
                if (result != FMOD.RESULT.OK) { return false; }

                result = sound.setLoopCount(-1);
                if (result != FMOD.RESULT.OK) { return false; }

                uint length;
                result = sound.getLength(out length, FMOD.TIMEUNIT.PCMBYTES);
                if (result != FMOD.RESULT.OK) { return false; }

                IntPtr ptr1, ptr2;
                uint len1, len2;
                result = sound.@lock(0, length, out ptr1, out ptr2, out len1, out len2);
                if (result != FMOD.RESULT.OK) { return false; }

                byte[] buffer = new byte[len1 + 44];
                //添加wav头
                Encoding.UTF8.GetBytes("RIFF").CopyTo(buffer, 0);
                BitConverter.GetBytes(len1 + 36).CopyTo(buffer, 4);
                Encoding.UTF8.GetBytes("WAVEfmt ").CopyTo(buffer, 8);
                BitConverter.GetBytes(16).CopyTo(buffer, 16);
                BitConverter.GetBytes((short)1).CopyTo(buffer, 20);
                BitConverter.GetBytes((short)m_AudioClip.m_Channels).CopyTo(buffer, 22);
                BitConverter.GetBytes(m_AudioClip.m_Frequency).CopyTo(buffer, 24);
                BitConverter.GetBytes(m_AudioClip.m_Frequency * m_AudioClip.m_Channels * m_AudioClip.m_BitsPerSample / 8).CopyTo(buffer, 28);
                BitConverter.GetBytes((short)(m_AudioClip.m_Channels * m_AudioClip.m_BitsPerSample / 8)).CopyTo(buffer, 32);
                BitConverter.GetBytes((short)m_AudioClip.m_BitsPerSample).CopyTo(buffer, 34);
                Encoding.UTF8.GetBytes("data").CopyTo(buffer, 36);
                BitConverter.GetBytes(len1).CopyTo(buffer, 40);
                Marshal.Copy(ptr1, buffer, 44, (int)len1);
                File.WriteAllBytes(exportFullname, buffer);

                result = sound.unlock(ptr1, ptr2, len1, len2);
                if (result != FMOD.RESULT.OK) { return false; }

                sound.release();
                system.release();
            }
            else
            {
                File.WriteAllBytes(exportFullname, m_AudioClip.m_AudioData);
            }
            return true;
        }

        public static void ExportMonoBehaviour(MonoBehaviour m_MonoBehaviour, string exportFilename)
        {
            File.WriteAllText(exportFilename, m_MonoBehaviour.serializedText);
        }

        public static void ExportShader(Shader m_Shader, string exportFilename)
        {
            File.WriteAllBytes(exportFilename, m_Shader.m_Script);
        }

        public static void ExportText(TextAsset m_TextAsset, string exportFilename)
        {
            File.WriteAllBytes(exportFilename, m_TextAsset.m_Script);
        }

        public static void ExportFont(unityFont m_Font, string exportFilename)
        {
            if (m_Font.m_FontData != null)
            {
                File.WriteAllBytes(exportFilename, m_Font.m_FontData);
            }
        }

        public static void ExportMesh(Mesh m_Mesh, string exportPath)
        {
            Scene m_scene = Scene.CreateScene("Scene");
            SceneNode root = m_scene.RootNode;
            SceneNode meshnode = Scene.CreateNode(m_scene, m_Mesh.m_Name);
            SceneNode.AddChild(root, meshnode);
            ManagedFbx.Mesh mesh = Scene.CreateMesh(m_scene, meshnode, "Mesh");
            if (m_Mesh.m_VertexCount > 0)
            {
                #region Vertices
                int count = 3;
                if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4)
                {
                    count = 4;
                }
                var vertices = new Vector3[m_Mesh.m_VertexCount];
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    vertices[v] = new Vector3(
                        m_Mesh.m_Vertices[v * count],
                        m_Mesh.m_Vertices[v * count + 1],
                        m_Mesh.m_Vertices[v * count + 2]);
                }
                mesh.Vertices = vertices;
                #endregion
                #region Indicies
                List<int> indices = new List<int>();
                for (int i = 0; i < m_Mesh.m_Indices.Count; i = i + 3)
                {
                    indices.Add((int)m_Mesh.m_Indices[i]);
                    indices.Add((int)m_Mesh.m_Indices[i + 1]);
                    indices.Add((int)m_Mesh.m_Indices[i + 2]);
                }
                mesh.AddPolygons(indices, 0);
                #endregion
                #region Normals
                if (m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                {
                    if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3)
                    {
                        count = 3;
                    }
                    else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4)
                    {
                        count = 4;
                    }

                    var normals = new Vector3[m_Mesh.m_VertexCount];
                    for (int n = 0; n < m_Mesh.m_VertexCount; n++)
                    {
                        normals[n] = new Vector3(
                            m_Mesh.m_Normals[n * count],
                            m_Mesh.m_Normals[n * count + 1],
                            m_Mesh.m_Normals[n * count + 2]);
                    }
                    mesh.Normals = normals;
                }
                #endregion
                #region Colors
                if (m_Mesh.m_Colors == null)
                {
                    var colors = new Colour[m_Mesh.m_VertexCount];
                    for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                    {
                        colors[c] = new Colour(
                            0.5f, 0.5f, 0.5f, 1.0f);
                    }
                    mesh.VertexColours = colors;
                }
                else if (m_Mesh.m_Colors.Length == m_Mesh.m_VertexCount * 3)
                {
                    var colors = new Colour[m_Mesh.m_VertexCount];
                    for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                    {
                        colors[c] = new Colour(
                            m_Mesh.m_Colors[c * 3],
                            m_Mesh.m_Colors[c * 3 + 1],
                            m_Mesh.m_Colors[c * 3 + 2],
                            1.0f);
                    }
                    mesh.VertexColours = colors;
                }
                else
                {
                    var colors = new Colour[m_Mesh.m_VertexCount];
                    for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                    {
                        colors[c] = new Colour(
                        m_Mesh.m_Colors[c * 4],
                        m_Mesh.m_Colors[c * 4 + 1],
                        m_Mesh.m_Colors[c * 4 + 2],
                        m_Mesh.m_Colors[c * 4 + 3]);
                    }
                    mesh.VertexColours = colors;
                }
                #endregion
                #region UV
                if (m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length == m_Mesh.m_VertexCount * 2)
                {
                    var uv = new Vector2[m_Mesh.m_VertexCount];
                    for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                    {
                        uv[c] = new Vector2(m_Mesh.m_UV1[c * 2], m_Mesh.m_UV1[c * 2 + 1]);
                    }
                    mesh.TextureCoords = uv;
                }
                else if (m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length == m_Mesh.m_VertexCount * 2)
                {
                    var uv = new Vector2[m_Mesh.m_VertexCount];
                    for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                    {
                        uv[c] = new Vector2(m_Mesh.m_UV2[c * 2], m_Mesh.m_UV2[c * 2 + 1]);
                    }
                    mesh.TextureCoords = uv;
                }
                #endregion
            }
            SceneNode.AddMesh(meshnode, mesh);

            //m_scene.Save(exportPath); //default is .fbx
            //m_scene.Save(exportPath + ".fbx");
            m_scene.Save(exportPath + ".obj");
            //m_scene.Save(exportPath + ".dae");
        }

        public static bool ExportFileExists(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            return false;
        }

        private static string FixFileName(string str)
        {
            if (str.Length >= 260) return Path.GetRandomFileName();
            return Path.GetInvalidFileNameChars().Aggregate(str, (current, c) => current.Replace(c, '_'));
        }
    }
}
