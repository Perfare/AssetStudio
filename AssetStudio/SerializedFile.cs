using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public class SerializedFile
    {
        public AssetsManager assetsManager;
        public EndianBinaryReader reader;
        public string fullName;
        public string originalPath;
        public string fileName;
        public string upperFileName;
        public int[] version = { 0, 0, 0, 0 };
        public BuildType buildType;
        public Dictionary<long, Object> Objects;

        public SerializedFileHeader header;
        private EndianType m_FileEndianess;
        public string unityVersion = "2.5.0f5";
        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;
        private bool m_EnableTypeTree = true;
        public List<SerializedType> m_Types;
        public List<ObjectInfo> m_Objects;
        private List<LocalSerializedObjectIdentifier> m_ScriptTypes;
        public List<FileIdentifier> m_Externals;

        public SerializedFile(AssetsManager assetsManager, string fullName, EndianBinaryReader reader)
        {
            this.assetsManager = assetsManager;
            this.reader = reader;
            this.fullName = fullName;
            fileName = Path.GetFileName(fullName);
            upperFileName = fileName.ToUpper();

            //ReadHeader
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

            //ReadMetadata
            if (m_FileEndianess == EndianType.LittleEndian)
            {
                reader.endian = EndianType.LittleEndian;
            }
            if (header.m_Version >= 7)
            {
                unityVersion = reader.ReadStringToNull();
                SetVersion(unityVersion);
            }
            if (header.m_Version >= 8)
            {
                m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform))
                {
                    m_TargetPlatform = BuildTarget.UnknownPlatform;
                }
            }
            if (header.m_Version >= 13)
            {
                m_EnableTypeTree = reader.ReadBoolean();
            }

            //ReadTypes
            int typeCount = reader.ReadInt32();
            m_Types = new List<SerializedType>(typeCount);
            for (int i = 0; i < typeCount; i++)
            {
                m_Types.Add(ReadSerializedType());
            }

            if (header.m_Version >= 7 && header.m_Version < 14)
            {
                var bigIDEnabled = reader.ReadInt32();
            }

            //ReadObjects
            int objectCount = reader.ReadInt32();
            m_Objects = new List<ObjectInfo>(objectCount);
            for (int i = 0; i < objectCount; i++)
            {
                var objectInfo = new ObjectInfo();
                if (header.m_Version < 14)
                {
                    objectInfo.m_PathID = reader.ReadInt32();
                }
                else
                {
                    reader.AlignStream();
                    objectInfo.m_PathID = reader.ReadInt64();
                }
                objectInfo.byteStart = reader.ReadUInt32();
                objectInfo.byteStart += header.m_DataOffset;
                objectInfo.byteSize = reader.ReadUInt32();
                objectInfo.typeID = reader.ReadInt32();
                if (header.m_Version < 16)
                {
                    objectInfo.classID = reader.ReadUInt16();
                    objectInfo.serializedType = m_Types.Find(x => x.classID == objectInfo.typeID);
                    var isDestroyed = reader.ReadUInt16();
                }
                else
                {
                    var type = m_Types[objectInfo.typeID];
                    objectInfo.serializedType = type;
                    objectInfo.classID = type.classID;
                }
                if (header.m_Version == 15 || header.m_Version == 16)
                {
                    var stripped = reader.ReadByte();
                }
                m_Objects.Add(objectInfo);
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
                        reader.AlignStream();
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
        }

        public void SetVersion(string stringVersion)
        {
            unityVersion = stringVersion;
            var buildSplit = Regex.Replace(stringVersion, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            buildType = new BuildType(buildSplit[0]);
            var versionSplit = Regex.Replace(stringVersion, @"\D", ".").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            version = versionSplit.Select(int.Parse).ToArray();
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

        private void ReadTypeTree(List<TypeTreeNode> typeTree, int level = 0)
        {
            var typeTreeNode = new TypeTreeNode();
            typeTree.Add(typeTreeNode);
            typeTreeNode.m_Level = level;
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
                ReadTypeTree(typeTree, level + 1);
            }
        }

        private void ReadTypeTree5(List<TypeTreeNode> typeTree)
        {
            int numberOfNodes = reader.ReadInt32();
            int stringBufferSize = reader.ReadInt32();

            var nodeSize = 24;
            if (header.m_Version > 17)
            {
                nodeSize = 32;
            }
            reader.Position += numberOfNodes * nodeSize;
            using (var stringBufferReader = new BinaryReader(new MemoryStream(reader.ReadBytes(stringBufferSize))))
            {
                reader.Position -= numberOfNodes * nodeSize + stringBufferSize;
                for (int i = 0; i < numberOfNodes; i++)
                {
                    var typeTreeNode = new TypeTreeNode();
                    typeTree.Add(typeTreeNode);
                    typeTreeNode.m_Version = reader.ReadUInt16();
                    typeTreeNode.m_Level = reader.ReadByte();
                    typeTreeNode.m_IsArray = reader.ReadBoolean() ? 1 : 0;
                    typeTreeNode.m_TypeStrOffset = reader.ReadUInt32();
                    typeTreeNode.m_NameStrOffset = reader.ReadUInt32();
                    typeTreeNode.m_ByteSize = reader.ReadInt32();
                    typeTreeNode.m_Index = reader.ReadInt32();
                    typeTreeNode.m_MetaFlag = reader.ReadInt32();

                    if (header.m_Version > 17)
                    {
                        reader.Position += 8;
                    }

                    typeTreeNode.m_Type = ReadString(stringBufferReader, typeTreeNode.m_TypeStrOffset);
                    typeTreeNode.m_Name = ReadString(stringBufferReader, typeTreeNode.m_NameStrOffset);
                }
                reader.Position += stringBufferSize;
            }

            string ReadString(BinaryReader stringBufferReader, uint value)
            {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset)
                {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                var offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str))
                {
                    return str;
                }
                return offset.ToString();
            }
        }
    }
}
