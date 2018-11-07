using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectReader : EndianBinaryReader
    {
        public AssetsFile assetsFile;
        public long m_PathID;
        public uint byteStart;
        public uint byteSize;
        public ClassIDType type;
        public SerializedType serializedType;
        public BuildTarget platform;
        private uint m_Version;

        public int[] version => assetsFile.version;
        public string[] buildType => assetsFile.buildType;

        public string exportName; //TODO Remove it

        public ObjectReader(EndianBinaryReader reader, AssetsFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.endian)
        {
            this.assetsFile = assetsFile;
            m_PathID = objectInfo.m_PathID;
            byteStart = objectInfo.byteStart;
            byteSize = objectInfo.byteSize;
            if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID))
            {
                type = (ClassIDType)objectInfo.classID;
            }
            else
            {
                type = ClassIDType.UnknownType;
            }
            serializedType = objectInfo.serializedType;
            platform = assetsFile.m_TargetPlatform;
            m_Version = assetsFile.header.m_Version;
        }

        public void Reset()
        {
            Position = byteStart;
        }

        public string Dump()
        {
            Reset();
            if (serializedType?.m_Nodes != null)
            {
                var sb = new StringBuilder();
                TypeTreeHelper.ReadTypeString(sb, serializedType.m_Nodes, this);
                return sb.ToString();
            }
            return null;
        }

        public bool HasStructMember(string name)
        {
            return serializedType?.m_Nodes != null && serializedType.m_Nodes.Any(x => x.m_Name == name);
        }

        public PPtr ReadPPtr()
        {
            return new PPtr
            {
                m_FileID = ReadInt32(),
                m_PathID = m_Version < 14 ? ReadInt32() : ReadInt64(),
                assetsFile = assetsFile
            };
        }

        public byte[] GetRawData()
        {
            Reset();
            return ReadBytes((int)byteSize);
        }
    }
}
