using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public class AssetsFile
    {
        public EndianBinaryReader reader;
        public string filePath;
        public string parentPath;
        public string fileName;
        public string upperFileName;
        public int[] version = { 0, 0, 0, 0 };
        public string[] buildType;
        public string platformStr;
        public bool valid;
        public Dictionary<long, AssetPreloadData> preloadTable = new Dictionary<long, AssetPreloadData>();
        public Dictionary<long, GameObject> GameObjectList = new Dictionary<long, GameObject>();
        public Dictionary<long, Transform> TransformList = new Dictionary<long, Transform>();
        public List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>();

        //class SerializedFile
        public SerializedFileHeader header;
        private EndianType m_FileEndianess;
        public string unityVersion = "2.5.0f5";
        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;
        private bool m_EnableTypeTree;
        public SortedDictionary<int, List<TypeTreeNode>> m_Type = new SortedDictionary<int, List<TypeTreeNode>>();
        private List<int[]> classIDs = new List<int[]>();
        public Dictionary<long, ObjectInfo> m_Objects;
        private List<LocalSerializedObjectIdentifier> m_ScriptTypes;
        public List<FileIdentifier> m_Externals;

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
                if (header.m_Version >= 13)
                {
                    m_EnableTypeTree = reader.ReadBoolean();
                }

                //Read types
                int typeCount = reader.ReadInt32();
                for (int i = 0; i < typeCount; i++)
                {
                    if (header.m_Version < 13)
                    {
                        int classID = reader.ReadInt32();
                        var typeTreeList = new List<TypeTreeNode>();
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

                //Read Objects
                int objectCount = reader.ReadInt32();

                string assetIDfmt = "D" + objectCount.ToString().Length; //format for unique ID

                for (int i = 0; i < objectCount; i++)
                {
                    AssetPreloadData asset = new AssetPreloadData();

                    if (header.m_Version < 14)
                    {
                        asset.m_PathID = reader.ReadInt32();
                    }
                    else
                    {
                        reader.AlignStream(4);
                        asset.m_PathID = reader.ReadInt64();
                    }
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
                    if (header.m_Version == 15 || header.m_Version == 16)
                    {
                        var stripped = reader.ReadByte();
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

                if (header.m_Version >= 11)
                {
                    int scriptCount = reader.ReadInt32();
                    m_ScriptTypes = new List<LocalSerializedObjectIdentifier>(scriptCount);
                    for (int i = 0; i < scriptCount; i++)
                    {
                        var m_ScriptType = new LocalSerializedObjectIdentifier();
                        m_ScriptType.localSerializedFileIndex = reader.ReadInt32();
                        if (header.m_Version < 14)
                        {
                            m_ScriptType.localIdentifierInFile = reader.ReadInt32();
                        }
                        else
                        {
                            reader.AlignStream(4);
                            m_ScriptType.localIdentifierInFile = reader.ReadInt64();
                        }
                        m_ScriptTypes.Add(m_ScriptType);
                    }
                }

                int externalsCount = reader.ReadInt32();
                m_Externals = new List<FileIdentifier>(externalsCount);
                for (int i = 0; i < externalsCount; i++)
                {
                    var m_External = new FileIdentifier();
                    if (header.m_Version >= 6)
                    {
                        var tempEmpty = reader.ReadStringToNull();
                    }
                    if (header.m_Version >= 5)
                    {
                        m_External.guid = new Guid(reader.ReadBytes(16));
                        m_External.type = reader.ReadInt32();
                    }
                    m_External.pathName = reader.ReadStringToNull();
                    m_External.fileName = Path.GetFileName(m_External.pathName);
                    m_Externals.Add(m_External);
                }

                if (header.m_Version >= 5)
                {
                    //var userInformation = reader.ReadStringToNull();
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

        private void ReadTypeTree(List<TypeTreeNode> typeTreeList, int depth)
        {
            var typeTree = new TypeTreeNode();
            typeTreeList.Add(typeTree);
            typeTree.m_Level = depth;
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

            if (m_EnableTypeTree)
            {
                int varCount = reader.ReadInt32();
                int stringSize = reader.ReadInt32();

                reader.Position += varCount * 24;
                using (var stringReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringSize))))
                {
                    var typeTreeList = new List<TypeTreeNode>();
                    reader.Position -= varCount * 24 + stringSize;
                    for (int i = 0; i < varCount; i++)
                    {
                        var typeTree = new TypeTreeNode();
                        typeTreeList.Add(typeTree);
                        typeTree.m_Version = reader.ReadUInt16();
                        typeTree.m_Level = reader.ReadByte();
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
                            typeTree.m_Type = CommonString.StringBuffer.ContainsKey(varTypeIndex) ? CommonString.StringBuffer[varTypeIndex] : varTypeIndex.ToString();
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
                            typeTree.m_Name = CommonString.StringBuffer.ContainsKey(varNameIndex) ? CommonString.StringBuffer[varNameIndex] : varNameIndex.ToString();
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

        public PPtr ReadPPtr()
        {
            var result = new PPtr
            {
                m_FileID = reader.ReadInt32(),
                m_PathID = header.m_Version < 14 ? reader.ReadInt32() : reader.ReadInt64(),
                assetsFile = this
            };
            return result;
        }
    }
}
