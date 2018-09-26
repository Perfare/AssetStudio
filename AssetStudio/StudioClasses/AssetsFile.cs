using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AssetStudio
{
    public class SerializedFileHeader
    {
        public uint m_MetadataSize;
        public uint m_FileSize;
        public uint m_Version;
        public uint m_DataOffset;
        public byte m_Endianess;
        public byte[] m_Reserved;
    }

    public class AssetsFile
    {
        public EndianBinaryReader reader;
        public SerializedFileHeader header;
        private EndianType m_FileEndianess;
        public string unityVersion = "2.5.0f5";
        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;
        private bool serializedTypeTrees;
        public SortedDictionary<int, List<TypeTree>> m_Type = new SortedDictionary<int, List<TypeTree>>();
        private List<int[]> classIDs = new List<int[]>();//use for 5.5.0

        public string filePath;
        public string parentPath;
        public string fileName;
        public string upperFileName;
        public int[] version = { 0, 0, 0, 0 };
        public string[] buildType;
        public string platformStr = "";

        public Dictionary<long, AssetPreloadData> preloadTable = new Dictionary<long, AssetPreloadData>();
        public Dictionary<long, GameObject> GameObjectList = new Dictionary<long, GameObject>();
        public Dictionary<long, Transform> TransformList = new Dictionary<long, Transform>();

        public List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>();
        public List<SharedAssets> sharedAssetsList = new List<SharedAssets> { new SharedAssets() };

        public bool valid;

        #region cmmon string
        private static Dictionary<int, string> baseStrings = new Dictionary<int, string>
        {
            {0, "AABB"},
            {5, "AnimationClip"},
            {19, "AnimationCurve"},
            {34, "AnimationState"},
            {49, "Array"},
            {55, "Base"},
            {60, "BitField"},
            {69, "bitset"},
            {76, "bool"},
            {81, "char"},
            {86, "ColorRGBA"},
            {96, "Component"},
            {106, "data"},
            {111, "deque"},
            {117, "double"},
            {124, "dynamic_array"},
            {138, "FastPropertyName"},
            {155, "first"},
            {161, "float"},
            {167, "Font"},
            {172, "GameObject"},
            {183, "Generic Mono"},
            {196, "GradientNEW"},
            {208, "GUID"},
            {213, "GUIStyle"},
            {222, "int"},
            {226, "list"},
            {231, "long long"},
            {241, "map"},
            {245, "Matrix4x4f"},
            {256, "MdFour"},
            {263, "MonoBehaviour"},
            {277, "MonoScript"},
            {288, "m_ByteSize"},
            {299, "m_Curve"},
            {307, "m_EditorClassIdentifier"},
            {331, "m_EditorHideFlags"},
            {349, "m_Enabled"},
            {359, "m_ExtensionPtr"},
            {374, "m_GameObject"},
            {387, "m_Index"},
            {395, "m_IsArray"},
            {405, "m_IsStatic"},
            {416, "m_MetaFlag"},
            {427, "m_Name"},
            {434, "m_ObjectHideFlags"},
            {452, "m_PrefabInternal"},
            {469, "m_PrefabParentObject"},
            {490, "m_Script"},
            {499, "m_StaticEditorFlags"},
            {519, "m_Type"},
            {526, "m_Version"},
            {536, "Object"},
            {543, "pair"},
            {548, "PPtr<Component>"},
            {564, "PPtr<GameObject>"},
            {581, "PPtr<Material>"},
            {596, "PPtr<MonoBehaviour>"},
            {616, "PPtr<MonoScript>"},
            {633, "PPtr<Object>"},
            {646, "PPtr<Prefab>"},
            {659, "PPtr<Sprite>"},
            {672, "PPtr<TextAsset>"},
            {688, "PPtr<Texture>"},
            {702, "PPtr<Texture2D>"},
            {718, "PPtr<Transform>"},
            {734, "Prefab"},
            {741, "Quaternionf"},
            {753, "Rectf"},
            {759, "RectInt"},
            {767, "RectOffset"},
            {778, "second"},
            {785, "set"},
            {789, "short"},
            {795, "size"},
            {800, "SInt16"},
            {807, "SInt32"},
            {814, "SInt64"},
            {821, "SInt8"},
            {827, "staticvector"},
            {840, "string"},
            {847, "TextAsset"},
            {857, "TextMesh"},
            {866, "Texture"},
            {874, "Texture2D"},
            {884, "Transform"},
            {894, "TypelessData"},
            {907, "UInt16"},
            {914, "UInt32"},
            {921, "UInt64"},
            {928, "UInt8"},
            {934, "unsigned int"},
            {947, "unsigned long long"},
            {966, "unsigned short"},
            {981, "vector"},
            {988, "Vector2f"},
            {997, "Vector3f"},
            {1006, "Vector4f"},
            {1015, "m_ScriptingClassIdentifier"},
            {1042, "Gradient"},
            {1051, "Type*"}
        };
        #endregion

        public class SharedAssets
        {
            public int Index = -2; //-2 - Prepare, -1 - Missing
            public string aName = "";
            public string fileName = "";
        }

        public AssetsFile(string fullName, EndianBinaryReader reader)
        {
            this.reader = reader;
            filePath = fullName;
            fileName = Path.GetFileName(fullName);
            upperFileName = fileName.ToUpper();
            try
            {
                //SerializedFile::ReadHeader
                header = new SerializedFileHeader();
                header.m_MetadataSize = reader.ReadUInt32();
                header.m_FileSize = reader.ReadUInt32();
                header.m_Version = reader.ReadUInt32();
                header.m_DataOffset = reader.ReadUInt32();

                if (header.m_Version >= 9)
                {
                    header.m_Endianess = reader.ReadByte();
                    header.m_Reserved = reader.ReadBytes(3);
                    m_FileEndianess = (EndianType)header.m_Endianess;
                }
                else
                {
                    reader.Position = header.m_FileSize - header.m_MetadataSize;
                    m_FileEndianess = (EndianType)reader.ReadByte();
                }

                //SerializedFile::ReadMetadata
                if (m_FileEndianess == EndianType.LittleEndian)
                {
                    reader.endian = EndianType.LittleEndian;
                }
                if (header.m_Version >= 7)
                {
                    unityVersion = reader.ReadStringToNull();
                }
                if (header.m_Version >= 8)
                {
                    m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                    if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform))
                    {
                        m_TargetPlatform = BuildTarget.UnknownPlatform;
                    }
                }
                platformStr = m_TargetPlatform.ToString();
                if (header.m_Version >= 14)
                {
                    serializedTypeTrees = reader.ReadBoolean();
                }

                // Read	types
                int typeCount = reader.ReadInt32();
                for (int i = 0; i < typeCount; i++)
                {
                    if (header.m_Version < 14)
                    {
                        int classID = reader.ReadInt32();
                        var typeTreeList = new List<TypeTree>();
                        ReadTypeTree(typeTreeList, 0);
                        m_Type.Add(classID, typeTreeList);
                    }
                    else
                    {
                        ReadTypeTree5();
                    }
                }

                if (header.m_Version >= 7 && header.m_Version < 14)
                {
                    var bigIDEnabled = reader.ReadInt32();
                }

                // Read Objects
                int objectCount = reader.ReadInt32();

                string assetIDfmt = "D" + objectCount.ToString().Length; //format for unique ID

                for (int i = 0; i < objectCount; i++)
                {
                    //each table entry is aligned individually, not the whole table
                    if (header.m_Version >= 14)
                    {
                        reader.AlignStream(4);
                    }

                    AssetPreloadData asset = new AssetPreloadData();
                    asset.m_PathID = header.m_Version < 14 ? reader.ReadInt32() : reader.ReadInt64();
                    asset.Offset = reader.ReadUInt32();
                    asset.Offset += header.m_DataOffset;
                    asset.Size = reader.ReadInt32();
                    if (header.m_Version > 15)
                    {
                        int index = reader.ReadInt32();
                        asset.Type1 = classIDs[index][0];
                        asset.Type2 = classIDs[index][1];
                    }
                    else
                    {
                        asset.Type1 = reader.ReadInt32();
                        asset.Type2 = reader.ReadUInt16();
                        reader.Position += 2;
                    }
                    if (header.m_Version == 15)
                    {
                        byte unknownByte = reader.ReadByte();
                        //this is a single byte, not an int32
                        //the next entry is aligned after this
                        //but not the last!
                    }

                    if (Enum.IsDefined(typeof(ClassIDReference), asset.Type2))
                    {
                        asset.Type = (ClassIDReference)asset.Type2;
                        asset.TypeString = asset.Type.ToString();
                    }
                    else
                    {
                        asset.Type = ClassIDReference.UnknownType;
                        asset.TypeString = "UnknownType " + asset.Type2;
                    }

                    asset.uniqueID = i.ToString(assetIDfmt);

                    asset.fullSize = asset.Size;
                    asset.sourceFile = this;

                    preloadTable.Add(asset.m_PathID, asset);

                    #region read BuildSettings to get version for version 2.x files
                    if (asset.Type == ClassIDReference.BuildSettings && header.m_Version == 6)
                    {
                        long nextAsset = reader.Position;

                        BuildSettings BSettings = new BuildSettings(asset);
                        unityVersion = BSettings.m_Version;

                        reader.Position = nextAsset;
                    }
                    #endregion
                }

                if (header.m_Version >= 14)
                {
                    //this looks like a list of assets that need to be preloaded in memory before anytihng else
                    int someCount = reader.ReadInt32();
                    for (int i = 0; i < someCount; i++)
                    {
                        int num1 = reader.ReadInt32();
                        reader.AlignStream(4);
                        long m_PathID = reader.ReadInt64();
                    }
                }

                sharedAssetsList[0].fileName = fileName; //reference itself because sharedFileIDs start from 1
                int sharedFileCount = reader.ReadInt32();
                for (int i = 0; i < sharedFileCount; i++)
                {
                    var shared = new SharedAssets();
                    shared.aName = reader.ReadStringToNull();
                    reader.Position += 20;
                    var sharedFilePath = reader.ReadStringToNull(); //relative path
                    shared.fileName = Path.GetFileName(sharedFilePath);
                    sharedAssetsList.Add(shared);
                }

                buildType = Regex.Replace(unityVersion, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                var firstVersion = int.Parse(unityVersion.Split('.')[0]);
                version = Regex.Matches(unityVersion, @"\d").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray();
                if (firstVersion > 5)//2017 and up
                {
                    var nversion = new int[version.Length - 3];
                    nversion[0] = firstVersion;
                    Array.Copy(version, 4, nversion, 1, version.Length - 4);
                    version = nversion;
                }

                valid = true;
            }
            catch
            {
            }
        }

        private void ReadTypeTree(List<TypeTree> typeTreeList, int depth)
        {
            var typeTree = new TypeTree();
            typeTreeList.Add(typeTree);
            typeTree.m_Depth = depth;
            typeTree.m_Type = reader.ReadStringToNull();
            typeTree.m_Name = reader.ReadStringToNull();
            typeTree.m_ByteSize = reader.ReadInt32();
            if (header.m_Version == 2)
            {
                var variableCount = reader.ReadInt32();
            }
            if (header.m_Version != 3)
            {
                typeTree.m_Index = reader.ReadInt32();
            }
            typeTree.m_IsArray = reader.ReadInt32();
            typeTree.m_Version = reader.ReadInt32();
            if (header.m_Version != 3)
            {
                typeTree.m_MetaFlag = reader.ReadInt32();

            }

            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                ReadTypeTree(typeTreeList, depth + 1);
            }
        }

        private void ReadTypeTree5()
        {
            int classID = reader.ReadInt32();
            if (header.m_Version > 15)//5.5.0 and up
            {
                reader.ReadByte();
                int typeID = reader.ReadInt16();
                if (typeID >= 0)
                {
                    typeID = -1 - typeID;
                }
                else
                {
                    typeID = classID;
                }
                classIDs.Add(new[] { typeID, classID });
                if (classID == 114)
                {
                    reader.Position += 16;
                }
                classID = typeID;
            }
            else if (classID < 0)
            {
                reader.Position += 16;
            }
            reader.Position += 16;

            if (serializedTypeTrees)
            {
                int varCount = reader.ReadInt32();
                int stringSize = reader.ReadInt32();

                reader.Position += varCount * 24;
                using (var stringReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringSize))))
                {
                    var typeTreeList = new List<TypeTree>();
                    reader.Position -= varCount * 24 + stringSize;
                    for (int i = 0; i < varCount; i++)
                    {
                        var typeTree = new TypeTree();
                        typeTreeList.Add(typeTree);
                        typeTree.m_Version = reader.ReadUInt16();
                        typeTree.m_Depth = reader.ReadByte();
                        typeTree.m_IsArray = reader.ReadBoolean() ? 1 : 0;

                        ushort varTypeIndex = reader.ReadUInt16();
                        ushort test = reader.ReadUInt16();
                        if (test == 0) //varType is an offset in the string block
                        {
                            stringReader.BaseStream.Position = varTypeIndex;
                            typeTree.m_Type = stringReader.ReadStringToNull();
                        }
                        else //varType is an index in an internal strig array
                        {
                            typeTree.m_Type = baseStrings.ContainsKey(varTypeIndex) ? baseStrings[varTypeIndex] : varTypeIndex.ToString();
                        }

                        ushort varNameIndex = reader.ReadUInt16();
                        test = reader.ReadUInt16();
                        if (test == 0)
                        {
                            stringReader.BaseStream.Position = varNameIndex;
                            typeTree.m_Name = stringReader.ReadStringToNull();
                        }
                        else
                        {
                            typeTree.m_Name = baseStrings.ContainsKey(varNameIndex) ? baseStrings[varNameIndex] : varNameIndex.ToString();
                        }

                        typeTree.m_ByteSize = reader.ReadInt32();
                        typeTree.m_Index = reader.ReadInt32();
                        typeTree.m_MetaFlag = reader.ReadInt32();
                    }
                    reader.Position += stringSize;
                    m_Type[classID] = typeTreeList;
                }
            }
        }
    }
}
