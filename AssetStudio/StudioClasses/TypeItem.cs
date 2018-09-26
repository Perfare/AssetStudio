using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class TypeItem : ListViewItem
    {
        public List<TypeTree> typeTreeList;

        public TypeItem(int classID, List<TypeTree> typeTreeList)
        {
            this.typeTreeList = typeTreeList;
            Text = typeTreeList[0].m_Type + " " + typeTreeList[0].m_Name;
            SubItems.Add(classID.ToString());
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var i in typeTreeList)
            {
                sb.AppendFormat("{0}{1} {2} {3} {4}\r\n", new string('\t', i.m_Depth), i.m_Type, i.m_Name, i.m_ByteSize, (i.m_MetaFlag & 0x4000) != 0);
            }
            return sb.ToString();
        }
    }
}
