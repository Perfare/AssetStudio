using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AssetStudio
{
    public class AssetsFile
    {
        public EndianBinaryReader reader;
        public string filePath;
        public string parentPath;
        public string fileName;
        public string upperFileName;
        public int fileGen;
        public bool valid;
        public string m_Version = "2.5.0f5";
        public int[] version = { 0, 0, 0, 0 };
        public string[] buildType;
        public int platform = 100663296;
        public string platformStr = "";
        public Dictionary<long, AssetPreloadData> preloadTable = new Dictionary<long, AssetPreloadData>();
        public Dictionary<long, GameObject> GameObjectList = new Dictionary<long, GameObject>();
        public Dictionary<long, Transform> TransformList = new Dictionary<long, Transform>();

        public List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>();
        public List<SharedAssets> sharedAssetsList = new List<SharedAssets> { new SharedAssets() };

        public SortedDictionary<int, ClassStruct> ClassStructures = new SortedDictionary<int, ClassStruct>();

        private bool baseDefinitions;
        private List<int[]> classIDs = new List<int[]>();//use for 5.5.0


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
                int tableSize = this.reader.ReadInt32();
                int dataEnd = this.reader.ReadInt32();
                fileGen = this.reader.ReadInt32();
                uint dataOffset = this.reader.ReadUInt32();
                sharedAssetsList[0].fileName = fileName; //reference itself because sharedFileIDs start from 1

                switch (fileGen)
                {
                    case 6: //2.5.0 - 2.6.1
                        {
                            this.reader.Position = (dataEnd - tableSize);
                            this.reader.Position += 1;
                            break;
                        }
                    case 7: //3.0.0 beta
                        {
                            this.reader.Position = (dataEnd - tableSize);
                            this.reader.Position += 1;
                            m_Version = this.reader.ReadStringToNull();
                            break;
                        }
                    case 8: //3.0.0 - 3.4.2
                        {
                            this.reader.Position = (dataEnd - tableSize);
                            this.reader.Position += 1;
                            m_Version = this.reader.ReadStringToNull();
                            platform = this.reader.ReadInt32();
                            break;
                        }
                    case 9: //3.5.0 - 4.6.x
                        {
                            this.reader.Position += 4; //azero
                            m_Version = this.reader.ReadStringToNull();
                            platform = this.reader.ReadInt32();
                            break;
                        }
                    case 14: //5.0.0 beta and final
                    case 15: //5.0.1 - 5.4
                    case 16: //??.. no sure
                    case 17: //5.5.0 and up
                        {
                            this.reader.Position += 4; //azero
                            m_Version = this.reader.ReadStringToNull();
                            platform = this.reader.ReadInt32();
                            baseDefinitions = this.reader.ReadBoolean();
                            break;
                        }
                    default:
                        {
                            //MessageBox.Show("Unsupported version!" + fileGen, "AssetStudio Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                }

                if (fileGen > 6 && m_Version == "")
                {
                    return;
                }

                if (platform > 255 || platform < 0)
                {
                    byte[] b32 = BitConverter.GetBytes(platform);
                    Array.Reverse(b32);
                    platform = BitConverter.ToInt32(b32, 0);
                    this.reader.endian = EndianType.LittleEndian;
                }

                platformStr = Enum.IsDefined(typeof(BuildTarget), platform) ? ((BuildTarget)platform).ToString() : "Unknown Platform";

                int baseCount = this.reader.ReadInt32();
                for (int i = 0; i < baseCount; i++)
                {
                    if (fileGen < 14)
                    {
                        int classID = this.reader.ReadInt32();
                        string baseType = this.reader.ReadStringToNull();
                        string baseName = this.reader.ReadStringToNull();
                        this.reader.Position += 20;
                        int memberCount = this.reader.ReadInt32();

                        var cb = new List<ClassMember>();
                        for (int m = 0; m < memberCount; m++)
                        {
                            readBase(cb, 1);
                        }

                        var aClass = new ClassStruct { ID = classID, Text = (baseType + " " + baseName), members = cb };
                        aClass.SubItems.Add(classID.ToString());
                        ClassStructures.Add(classID, aClass);
                    }
                    else
                    {
                        readBase5();
                    }
                }

                if (fileGen >= 7 && fileGen < 14)
                {
                    this.reader.Position += 4; //azero
                }

                int assetCount = this.reader.ReadInt32();

                #region asset preload table
                string assetIDfmt = "D" + assetCount.ToString().Length; //format for unique ID

                for (int i = 0; i < assetCount; i++)
                {
                    //each table entry is aligned individually, not the whole table
                    if (fileGen >= 14)
                    {
                        this.reader.AlignStream(4);
                    }

                    AssetPreloadData asset = new AssetPreloadData();
                    asset.m_PathID = fileGen < 14 ? this.reader.ReadInt32() : this.reader.ReadInt64();
                    asset.Offset = this.reader.ReadUInt32();
                    asset.Offset += dataOffset;
                    asset.Size = this.reader.ReadInt32();
                    if (fileGen > 15)
                    {
                        int index = this.reader.ReadInt32();
                        asset.Type1 = classIDs[index][0];
                        asset.Type2 = classIDs[index][1];
                    }
                    else
                    {
                        asset.Type1 = this.reader.ReadInt32();
                        asset.Type2 = this.reader.ReadUInt16();
                        this.reader.Position += 2;
                    }
                    if (fileGen == 15)
                    {
                        byte unknownByte = this.reader.ReadByte();
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
                    if (asset.Type == ClassIDReference.BuildSettings && fileGen == 6)
                    {
                        long nextAsset = this.reader.Position;

                        BuildSettings BSettings = new BuildSettings(asset);
                        m_Version = BSettings.m_Version;

                        this.reader.Position = nextAsset;
                    }
                    #endregion
                }
                #endregion

                buildType = Regex.Replace(m_Version, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                var firstVersion = int.Parse(m_Version.Split('.')[0]);
                version = Regex.Matches(m_Version, @"\d").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray();
                if (firstVersion > 5)//2017 and up
                {
                    var nversion = new int[version.Length - 3];
                    nversion[0] = firstVersion;
                    Array.Copy(version, 4, nversion, 1, version.Length - 4);
                    version = nversion;
                }
                if (fileGen >= 14)
                {
                    //this looks like a list of assets that need to be preloaded in memory before anytihng else
                    int someCount = this.reader.ReadInt32();
                    for (int i = 0; i < someCount; i++)
                    {
                        int num1 = this.reader.ReadInt32();
                        this.reader.AlignStream(4);
                        long m_PathID = this.reader.ReadInt64();
                    }
                }

                int sharedFileCount = this.reader.ReadInt32();
                for (int i = 0; i < sharedFileCount; i++)
                {
                    var shared = new SharedAssets();
                    shared.aName = this.reader.ReadStringToNull();
                    this.reader.Position += 20;
                    var sharedFilePath = this.reader.ReadStringToNull(); //relative path
                    shared.fileName = Path.GetFileName(sharedFilePath);
                    sharedAssetsList.Add(shared);
                }
                valid = true;
            }
            catch
            {
            }
        }

        private void readBase(List<ClassMember> cb, int level)
        {
            string varType = reader.ReadStringToNull();
            string varName = reader.ReadStringToNull();
            int size = reader.ReadInt32();
            int index = reader.ReadInt32();
            int isArray = reader.ReadInt32();
            int num0 = reader.ReadInt32();
            int flag = reader.ReadInt32();
            int childrenCount = reader.ReadInt32();

            cb.Add(new ClassMember
            {
                Level = level - 1,
                Type = varType,
                Name = varName,
                Size = size,
                Flag = flag
            });
            for (int i = 0; i < childrenCount; i++) { readBase(cb, level + 1); }
        }

        private void readBase5()
        {
            int classID = reader.ReadInt32();
            if (fileGen > 15)//5.5.0 and up
            {
                reader.ReadByte();
                int type1;
                if ((type1 = reader.ReadInt16()) >= 0)
                {
                    type1 = -1 - type1;
                }
                else
                {
                    type1 = classID;
                }
                classIDs.Add(new[] { type1, classID });
                if (classID == 114)
                {
                    reader.Position += 16;
                }
                classID = type1;
            }
            else if (classID < 0)
            {
                reader.Position += 16;
            }
            reader.Position += 16;

            if (baseDefinitions)
            {
                int varCount = reader.ReadInt32();
                int stringSize = reader.ReadInt32();

                reader.Position += varCount * 24;
                using (var stringReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringSize))))
                {
                    string className = "";
                    var classVar = new List<ClassMember>();
                    //build Class Structures
                    reader.Position -= varCount * 24 + stringSize;
                    for (int i = 0; i < varCount; i++)
                    {
                        ushort num0 = reader.ReadUInt16();
                        byte level = reader.ReadByte();
                        bool isArray = reader.ReadBoolean();

                        ushort varTypeIndex = reader.ReadUInt16();
                        ushort test = reader.ReadUInt16();
                        string varTypeStr;
                        if (test == 0) //varType is an offset in the string block
                        {
                            stringReader.BaseStream.Position = varTypeIndex;
                            varTypeStr = stringReader.ReadStringToNull();
                        }
                        else //varType is an index in an internal strig array
                        {
                            varTypeStr = baseStrings.ContainsKey(varTypeIndex) ? baseStrings[varTypeIndex] : varTypeIndex.ToString();
                        }

                        ushort varNameIndex = reader.ReadUInt16();
                        test = reader.ReadUInt16();
                        string varNameStr;
                        if (test == 0)
                        {
                            stringReader.BaseStream.Position = varNameIndex;
                            varNameStr = stringReader.ReadStringToNull();
                        }
                        else
                        {
                            varNameStr = baseStrings.ContainsKey(varNameIndex) ? baseStrings[varNameIndex] : varNameIndex.ToString();
                        }

                        int size = reader.ReadInt32();
                        int index = reader.ReadInt32();
                        int flag = reader.ReadInt32();

                        if (index == 0)
                        {
                            className = varTypeStr + " " + varNameStr;
                        }
                        else
                        {
                            classVar.Add(new ClassMember
                            {
                                Level = level - 1,
                                Type = varTypeStr,
                                Name = varNameStr,
                                Size = size,
                                Flag = flag
                            });
                        }
                    }
                    reader.Position += stringSize;
                    var aClass = new ClassStruct { ID = classID, Text = className, members = classVar };
                    aClass.SubItems.Add(classID.ToString());
                    ClassStructures[classID] = aClass;
                }
            }
        }
    }
}
