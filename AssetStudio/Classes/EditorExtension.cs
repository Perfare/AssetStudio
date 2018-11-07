using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class EditorExtension : Object
    {
        protected EditorExtension(ObjectReader reader) : base(reader)
        {
            if (platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = reader.ReadPPtr();
                var m_PrefabInternal = reader.ReadPPtr();
            }
        }
    }
}
