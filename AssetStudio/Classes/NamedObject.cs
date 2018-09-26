using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class NamedObject : EditorExtension
    {
        public string m_Name;

        public NamedObject(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Name = reader.ReadAlignedString();
        }
    }
}
