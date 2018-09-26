using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AssetStudio
{
    public sealed class TextAsset : NamedObject
    {
        public byte[] m_Script;

        public TextAsset(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Script = reader.ReadBytes(reader.ReadInt32());
        }
    }
}
