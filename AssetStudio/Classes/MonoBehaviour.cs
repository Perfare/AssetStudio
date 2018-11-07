using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MonoBehaviour : Behaviour
    {
        public PPtr m_Script;
        public string m_Name;

        public MonoBehaviour(ObjectReader reader) : base(reader)
        {
            m_Script = reader.ReadPPtr();
            m_Name = reader.ReadAlignedString();
        }
    }
}
