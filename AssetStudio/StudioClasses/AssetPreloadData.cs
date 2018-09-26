using System;
using System.Collections.Generic;
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
        public ClassIDReference Type;
        public int Type1;
        public int Type2;

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
            if (sourceFile.m_Type.TryGetValue(Type1, out var typeTreeList))
            {
                var sb = new StringBuilder();
                TypeTreeHelper.ReadTypeString(sb, typeTreeList, reader);
                return sb.ToString();
            }
            return null;
        }

        public bool HasStructMember(string name)
        {
            return sourceFile.m_Type.TryGetValue(Type1, out var typeTreeList) && typeTreeList.Any(x => x.m_Name == name);
        }
    }
}
