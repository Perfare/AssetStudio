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
            var reader = sourceFile.assetsFileReader;
            reader.Position = Offset;
            return reader;
        }
    }
}
