using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Behaviour : Component
    {
        public byte m_Enabled;

        protected Behaviour(ObjectReader reader) : base(reader)
        {
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
