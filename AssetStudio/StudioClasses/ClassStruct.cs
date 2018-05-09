using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class ClassStruct : ListViewItem
    {
        public int ID;
        public List<ClassMember> members;

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var i in members)
            {
                sb.AppendFormat("{0}{1} {2} {3} {4}\r\n", new string('\t', i.Level), i.Type, i.Name, i.Size, (i.Flag & 0x4000) != 0);
            }
            return sb.ToString();
        }
    }
}
