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
        public string extension;

        public AssetsFile sourceFile;
        public GameObject gameObject;
        public string uniqueID;

        public EndianBinaryReader InitReader()
        {
            var reader = sourceFile.reader;
            reader.Position = Offset;
            return reader;
        }

        public string GetClassString()
        {
            var reader = InitReader();
            if (sourceFile.ClassStructures.TryGetValue(Type1, out var classStructure))
            {
                var sb = new StringBuilder();
                ClassStructHelper.ReadClassString(sb, classStructure.members, reader);
                return sb.ToString();
            }
            return null;
        }

        public bool HasStructMember(string name)
        {
            return sourceFile.ClassStructures.TryGetValue(Type1, out var classStructure) && classStructure.members.Any(x => x.Name == name);
        }
    }
}
