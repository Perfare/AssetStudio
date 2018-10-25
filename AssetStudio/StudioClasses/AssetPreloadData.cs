using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class AssetPreloadData : ListViewItem
    {
        public AssetsFile sourceFile;
        public long m_PathID;
        public uint Offset;
        public uint Size;
        public long FullSize;
        public SerializedType serializedType;
        public ClassIDType Type;
        public string TypeString;
        public string InfoText;
        public string uniqueID;
        public GameObject gameObject;

        public AssetPreloadData(AssetsFile assetsFile, ObjectInfo objectInfo, string uniqueID)
        {
            sourceFile = assetsFile;
            m_PathID = objectInfo.m_PathID;
            Offset = objectInfo.byteStart;
            Size = objectInfo.byteSize;
            FullSize = objectInfo.byteSize;
            serializedType = objectInfo.serializedType;
            if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID))
            {
                Type = (ClassIDType)objectInfo.classID;
                TypeString = Type.ToString();
            }
            else
            {
                Type = ClassIDType.UnknownType;
                TypeString = $"UnknownType {objectInfo.classID}";
            }
            this.uniqueID = uniqueID;
        }

        public EndianBinaryReader InitReader()
        {
            var reader = sourceFile.reader;
            reader.Position = Offset;
            return reader;
        }

        public string Dump()
        {
            var reader = InitReader();
            if (serializedType?.m_Nodes != null)
            {
                var sb = new StringBuilder();
                TypeTreeHelper.ReadTypeString(sb, serializedType.m_Nodes, reader);
                return sb.ToString();
            }
            return null;
        }

        public bool HasStructMember(string name)
        {
            return serializedType?.m_Nodes != null && serializedType.m_Nodes.Any(x => x.m_Name == name);
        }
    }
}
