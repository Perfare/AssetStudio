using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class AssetsFile
    {
        public EndianBinaryReader a_Stream;
        public string filePath;
        public string bundlePath;
        public string fileName;
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
        public List<UnityShared> sharedAssetsList = new List<UnityShared> { new UnityShared() };

        public SortedDictionary<int, ClassStruct> ClassStructures = new SortedDictionary<int, ClassStruct>();

        private bool baseDefinitions;
        private List<int[]> classIDs = new List<int[]>();//use for 5.5.0

        public static string[] buildTypeSplit = { ".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

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

        public class UnityShared
        {
            public int Index = -1; //actual index in main list
            public string aName = "";
            public string fileName = "";
        }

        public AssetsFile(string fullName, EndianBinaryReader fileStream)
        {
            a_Stream = fileStream;
            filePath = fullName;
            fileName = Path.GetFileName(fullName);
            try
            {
                int tableSize = a_Stream.ReadInt32();
                int dataEnd = a_Stream.ReadInt32();
                fileGen = a_Stream.ReadInt32();
                uint dataOffset = a_Stream.ReadUInt32();
                sharedAssetsList[0].fileName = Path.GetFileName(fullName); //reference itself because sharedFileIDs start from 1

                switch (fileGen)
                {
                    case 6: //2.5.0 - 2.6.1
                        {
                            a_Stream.Position = (dataEnd - tableSize);
                            a_Stream.Position += 1;
                            break;
                        }
                    case 7: //3.0.0 beta
                        {
                            a_Stream.Position = (dataEnd - tableSize);
                            a_Stream.Position += 1;
                            m_Version = a_Stream.ReadStringToNull();
                            break;
                        }
                    case 8: //3.0.0 - 3.4.2
                        {
                            a_Stream.Position = (dataEnd - tableSize);
                            a_Stream.Position += 1;
                            m_Version = a_Stream.ReadStringToNull();
                            platform = a_Stream.ReadInt32();
                            break;
                        }
                    case 9: //3.5.0 - 4.6.x
                        {
                            a_Stream.Position += 4; //azero
                            m_Version = a_Stream.ReadStringToNull();
                            platform = a_Stream.ReadInt32();
                            break;
                        }
                    case 14: //5.0.0 beta and final
                    case 15: //5.0.1 - 5.4
                    case 16: //??.. no sure
                    case 17: //5.5.0 and up
                        {
                            a_Stream.Position += 4; //azero
                            m_Version = a_Stream.ReadStringToNull();
                            platform = a_Stream.ReadInt32();
                            baseDefinitions = a_Stream.ReadBoolean();
                            break;
                        }
                    default:
                        {
                            //MessageBox.Show("Unsupported Unity version!" + fileGen, "Unity Studio Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                }

                if (platform > 255 || platform < 0)
                {
                    byte[] b32 = BitConverter.GetBytes(platform);
                    Array.Reverse(b32);
                    platform = BitConverter.ToInt32(b32, 0);
                    a_Stream.endian = EndianType.LittleEndian;
                }

                switch (platform)
                {
                    case -2:
                        platformStr = "Unity Package";
                        break;
                    case 4:
                        platformStr = "OSX";
                        break;
                    case 5:
                        platformStr = "PC";
                        break;
                    case 6:
                        platformStr = "Web";
                        break;
                    case 7:
                        platformStr = "Web streamed";
                        break;
                    case 9:
                        platformStr = "iOS";
                        break;
                    case 10:
                        platformStr = "PS3";
                        break;
                    case 11:
                        platformStr = "Xbox 360";
                        break;
                    case 13:
                        platformStr = "Android";
                        break;
                    case 16:
                        platformStr = "Google NaCl";
                        break;
                    case 19:
                        platformStr = "CollabPreview";
                        break;
                    case 21:
                        platformStr = "WP8";
                        break;
                    case 25:
                        platformStr = "Linux";
                        break;
                    case 29:
                        platformStr = "Wii U";
                        break;
                    default:
                        platformStr = "Unknown Platform";
                        break;
                }

                int baseCount = a_Stream.ReadInt32();
                for (int i = 0; i < baseCount; i++)
                {
                    if (fileGen < 14)
                    {
                        int classID = a_Stream.ReadInt32();
                        string baseType = a_Stream.ReadStringToNull();
                        string baseName = a_Stream.ReadStringToNull();
                        a_Stream.Position += 20;
                        int memberCount = a_Stream.ReadInt32();

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
                    a_Stream.Position += 4; //azero
                }

                int assetCount = a_Stream.ReadInt32();

                #region asset preload table
                string assetIDfmt = "D" + assetCount.ToString().Length; //format for unique ID

                for (int i = 0; i < assetCount; i++)
                {
                    //each table entry is aligned individually, not the whole table
                    if (fileGen >= 14)
                    {
                        a_Stream.AlignStream(4);
                    }

                    AssetPreloadData asset = new AssetPreloadData();
                    asset.m_PathID = fileGen < 14 ? a_Stream.ReadInt32() : a_Stream.ReadInt64();
                    asset.Offset = a_Stream.ReadUInt32();
                    asset.Offset += dataOffset;
                    asset.Size = a_Stream.ReadInt32();
                    if (fileGen > 15)
                    {
                        int index = a_Stream.ReadInt32();
                        asset.Type1 = classIDs[index][0];
                        asset.Type2 = classIDs[index][1];
                    }
                    else
                    {
                        asset.Type1 = a_Stream.ReadInt32();
                        asset.Type2 = a_Stream.ReadUInt16();
                        a_Stream.Position += 2;
                    }
                    if (fileGen == 15)
                    {
                        byte unknownByte = a_Stream.ReadByte();
                        //this is a single byte, not an int32
                        //the next entry is aligned after this
                        //but not the last!
                    }

                    if (ClassIDReference.Names.TryGetValue(asset.Type2, out var typeString))
                    {
                        asset.TypeString = typeString;
                    }
                    else
                    {
                        asset.TypeString = "Unknown Type " + asset.Type2;
                    }

                    asset.uniqueID = i.ToString(assetIDfmt);

                    asset.fullSize = asset.Size;
                    asset.sourceFile = this;

                    preloadTable.Add(asset.m_PathID, asset);

                    #region read BuildSettings to get version for unity 2.x files
                    if (asset.Type2 == 141 && fileGen == 6)
                    {
                        long nextAsset = a_Stream.Position;

                        BuildSettings BSettings = new BuildSettings(asset);
                        m_Version = BSettings.m_Version;

                        a_Stream.Position = nextAsset;
                    }
                    #endregion
                }
                #endregion

                buildType = m_Version.Split(buildTypeSplit, StringSplitOptions.RemoveEmptyEntries);
                var strver = from Match m in Regex.Matches(m_Version, @"[0-9]") select m.Value;
                version = Array.ConvertAll(strver.ToArray(), int.Parse);
                if (version[0] == 2 && version[1] == 0 && version[2] == 1 && version[3] == 7)//2017.x
                {
                    var nversion = new int[version.Length - 3];
                    nversion[0] = 2017;
                    Array.Copy(version, 4, nversion, 1, version.Length - 4);
                    version = nversion;
                }
                if (fileGen >= 14)
                {
                    //this looks like a list of assets that need to be preloaded in memory before anytihng else
                    int someCount = a_Stream.ReadInt32();
                    for (int i = 0; i < someCount; i++)
                    {
                        int num1 = a_Stream.ReadInt32();
                        a_Stream.AlignStream(4);
                        long m_PathID = a_Stream.ReadInt64();
                    }
                }

                int sharedFileCount = a_Stream.ReadInt32();
                for (int i = 0; i < sharedFileCount; i++)
                {
                    var shared = new UnityShared();
                    shared.aName = a_Stream.ReadStringToNull();
                    a_Stream.Position += 20;
                    var sharedFilePath = a_Stream.ReadStringToNull(); //relative path
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
            string varType = a_Stream.ReadStringToNull();
            string varName = a_Stream.ReadStringToNull();
            int size = a_Stream.ReadInt32();
            int index = a_Stream.ReadInt32();
            int isArray = a_Stream.ReadInt32();
            int num0 = a_Stream.ReadInt32();
            int flag = a_Stream.ReadInt32();
            int childrenCount = a_Stream.ReadInt32();

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
            int classID = a_Stream.ReadInt32();
            if (fileGen > 15)//5.5.0 and up
            {
                a_Stream.ReadByte();
                int type1;
                if ((type1 = a_Stream.ReadInt16()) >= 0)
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
                    a_Stream.Position += 16;
                }
                classID = type1;
            }
            else if (classID < 0)
            {
                a_Stream.Position += 16;
            }
            a_Stream.Position += 16;

            if (baseDefinitions)
            {
                int varCount = a_Stream.ReadInt32();
                int stringSize = a_Stream.ReadInt32();

                a_Stream.Position += varCount * 24;
                var stringReader = new EndianBinaryReader(new MemoryStream(a_Stream.ReadBytes(stringSize)));
                string className = "";
                var classVar = new List<ClassMember>();
                //build Class Structures
                a_Stream.Position -= varCount * 24 + stringSize;
                for (int i = 0; i < varCount; i++)
                {
                    ushort num0 = a_Stream.ReadUInt16();
                    byte level = a_Stream.ReadByte();
                    bool isArray = a_Stream.ReadBoolean();

                    ushort varTypeIndex = a_Stream.ReadUInt16();
                    ushort test = a_Stream.ReadUInt16();
                    string varTypeStr;
                    if (test == 0) //varType is an offset in the string block
                    {
                        stringReader.Position = varTypeIndex;
                        varTypeStr = stringReader.ReadStringToNull();
                    }
                    else //varType is an index in an internal strig array
                    {
                        varTypeStr = baseStrings.ContainsKey(varTypeIndex) ? baseStrings[varTypeIndex] : varTypeIndex.ToString();
                    }

                    ushort varNameIndex = a_Stream.ReadUInt16();
                    test = a_Stream.ReadUInt16();
                    string varNameStr;
                    if (test == 0)
                    {
                        stringReader.Position = varNameIndex;
                        varNameStr = stringReader.ReadStringToNull();
                    }
                    else
                    {
                        varNameStr = baseStrings.ContainsKey(varNameIndex) ? baseStrings[varNameIndex] : varNameIndex.ToString();
                    }

                    int size = a_Stream.ReadInt32();
                    int index = a_Stream.ReadInt32();
                    int flag = a_Stream.ReadInt32();

                    if (index == 0) { className = varTypeStr + " " + varNameStr; }
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
                stringReader.Dispose();
                a_Stream.Position += stringSize;

                var aClass = new ClassStruct { ID = classID, Text = className, members = classVar };
                aClass.SubItems.Add(classID.ToString());
                ClassStructures[classID] = aClass;
            }
        }
    }
}
