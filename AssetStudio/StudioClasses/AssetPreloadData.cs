using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class AssetPreloadData : ListViewItem
    {
        public long m_PathID;
        public uint Offset;
        public int Size;
        public ClassIDType Type;
        public int typeID;
        public int classID;
        public SerializedType serializedType;
        public string TypeString;
        public int fullSize;
        public string InfoText;
        public AssetsFile sourceFile;
        public GameObject gameObject;
        public string uniqueID;

        public EndianBinaryReader InitReader()
        {
            var reader = sourceFile.reader;
            reader.Position = Offset;
            return reader;
        }

        public string Dump()
        {
            var reader = InitReader();
            if (serializedType.m_Nodes != null)
            {
                var sb = new StringBuilder();
                TypeTreeHelper.ReadTypeString(sb, serializedType.m_Nodes, reader);
                return sb.ToString();
            }
            return null;
        }

        public bool HasStructMember(string name)
        {
            return serializedType.m_Nodes != null && serializedType.m_Nodes.Any(x => x.m_Name == name);
        }
    }
}
