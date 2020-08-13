using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class TypeTreeNode
    {
        public string m_Type;
        public string m_Name;
        public int m_ByteSize;
        public int m_Index;
        public int m_IsArray; //m_TypeFlags
        public int m_Version;
        public int m_MetaFlag;
        public int m_Level;
        public uint m_TypeStrOffset;
        public uint m_NameStrOffset;
        public ulong m_RefTypeHash;

        public TypeTreeNode() { }

        public TypeTreeNode(string type, string name, int level, bool align)
        {
            m_Type = type;
            m_Name = name;
            m_Level = level;
            m_MetaFlag = align ? 0x4000 : 0;
        }
    }
}
