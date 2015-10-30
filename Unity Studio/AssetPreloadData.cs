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
        public int Offset;
        public int Size;
        public int Type1;
        public ushort Type2;

        //public string m_Name = "";
        public string TypeString;
        public int exportSize;
        public string InfoText;

        public AssetsFile sourceFile;
        public int specificIndex = -1; //index in specific asset list
        public string uniqueID;
    }
}
