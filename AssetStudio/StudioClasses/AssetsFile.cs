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
        private bool m_EnableTypeTree = true;
        public List<SerializedType> m_Types;
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
                m_Types = new List<SerializedType>(typeCount);
                for (int i = 0; i < typeCount; i++)
                {
                    m_Types.Add(ReadSerializedType());;
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
                    asset.typeID = reader.ReadInt32();
                    if (header.m_Version < 16)
                    {
                        asset.classID = reader.ReadUInt16();
                        asset.serializedType = m_Types.Find(x => x.classID == asset.typeID);
                        reader.Position += 2;
                    }
                    else
                    {
                        var type = m_Types[asset.typeID];
                        asset.serializedType = type;
                        asset.classID = type.classID;
                    }
                    if (header.m_Version == 15 || header.m_Version == 16)
                    {
                        var stripped = reader.ReadByte();
                    }

                    if (Enum.IsDefined(typeof(ClassIDType), asset.classID))
                    {
                        asset.Type = (ClassIDType)asset.classID;
                        asset.TypeString = asset.Type.ToString();
                    }
                    else
                    {
                        asset.Type = ClassIDType.UnknownType;
                        asset.TypeString = $"UnknownType {asset.classID}";
                    }

                    asset.uniqueID = i.ToString(assetIDfmt);

                    asset.fullSize = asset.Size;
                    asset.sourceFile = this;

                    preloadTable.Add(asset.m_PathID, asset);

                    #region read BuildSettings to get version for version 2.x files
                    if (asset.Type == ClassIDType.BuildSettings && header.m_Version == 6)
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

        private SerializedType ReadSerializedType()
        {
            var type = new SerializedType();

            type.classID = reader.ReadInt32();

            if (header.m_Version >= 16)
            {
                type.m_IsStrippedType = reader.ReadBoolean();
            }

            if (header.m_Version >= 17)
            {
                type.m_ScriptTypeIndex = reader.ReadInt16();
            }

            if (header.m_Version >= 13)
            {
                if ((header.m_Version < 16 && type.classID < 0) || (header.m_Version >= 16 && type.classID == 114))
                {
                    type.m_ScriptID = reader.ReadBytes(16); //Hash128
                }
                type.m_OldTypeHash = reader.ReadBytes(16); //Hash128
            }

            if (m_EnableTypeTree)
            {
                var typeTree = new List<TypeTreeNode>();
                if (header.m_Version >= 12 || header.m_Version == 10)
                {
                    ReadTypeTree5(typeTree);
                }
                else
                {
                    ReadTypeTree(typeTree);
                }

                type.m_Nodes = typeTree;
            }

            return type;
        }

        private void ReadTypeTree(List<TypeTreeNode> typeTree, int depth = 0)
        {
            var typeTreeNode = new TypeTreeNode();
            typeTree.Add(typeTreeNode);
            typeTreeNode.m_Level = depth;
            typeTreeNode.m_Type = reader.ReadStringToNull();
            typeTreeNode.m_Name = reader.ReadStringToNull();
            typeTreeNode.m_ByteSize = reader.ReadInt32();
            if (header.m_Version == 2)
            {
                var variableCount = reader.ReadInt32();
            }
            if (header.m_Version != 3)
            {
                typeTreeNode.m_Index = reader.ReadInt32();
            }
            typeTreeNode.m_IsArray = reader.ReadInt32();
            typeTreeNode.m_Version = reader.ReadInt32();
            if (header.m_Version != 3)
            {
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }

            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                ReadTypeTree(typeTree, depth + 1);
            }
        }

        private void ReadTypeTree5(List<TypeTreeNode> typeTree)
        {
            int numberOfNodes = reader.ReadInt32();
            int stringBufferSize = reader.ReadInt32();

            reader.Position += numberOfNodes * 24;
            using (var stringBufferReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringBufferSize))))
            {
                reader.Position -= numberOfNodes * 24 + stringBufferSize;
                for (int i = 0; i < numberOfNodes; i++)
                {
                    var typeTreeNode = new TypeTreeNode();
                    typeTree.Add(typeTreeNode);
                    typeTreeNode.m_Version = reader.ReadUInt16();
                    typeTreeNode.m_Level = reader.ReadByte();
                    typeTreeNode.m_IsArray = reader.ReadBoolean() ? 1 : 0;

                    var m_TypeStrOffset = reader.ReadUInt16();
                    var temp = reader.ReadUInt16();
                    if (temp == 0)
                    {
                        stringBufferReader.BaseStream.Position = m_TypeStrOffset;
                        typeTreeNode.m_Type = stringBufferReader.ReadStringToNull();
                    }
                    else
                    {
                        typeTreeNode.m_Type = CommonString.StringBuffer.ContainsKey(m_TypeStrOffset) ? CommonString.StringBuffer[m_TypeStrOffset] : m_TypeStrOffset.ToString();
                    }

                    var m_NameStrOffset = reader.ReadUInt16();
                    temp = reader.ReadUInt16();
                    if (temp == 0)
                    {
                        stringBufferReader.BaseStream.Position = m_NameStrOffset;
                        typeTreeNode.m_Name = stringBufferReader.ReadStringToNull();
                    }
                    else
                    {
                        typeTreeNode.m_Name = CommonString.StringBuffer.ContainsKey(m_NameStrOffset) ? CommonString.StringBuffer[m_NameStrOffset] : m_NameStrOffset.ToString();
                    }

                    typeTreeNode.m_ByteSize = reader.ReadInt32();
                    typeTreeNode.m_Index = reader.ReadInt32();
                    typeTreeNode.m_MetaFlag = reader.ReadInt32();
                }
                reader.Position += stringBufferSize;
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
