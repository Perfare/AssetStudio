using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class AssetPreloadData : ListViewItem
    {
        public long m_PathID;
        public uint Offset;
        public int Size;
        public int Type1;
        public int Type2;

        public string TypeString;
        public int fullSize;
        public string InfoText;
        public string extension;

        public AssetsFile sourceFile;
        public string uniqueID;
    }
}
