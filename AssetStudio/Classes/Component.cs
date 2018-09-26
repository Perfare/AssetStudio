using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Component : EditorExtension
    {
        public PPtr m_GameObject;

        protected Component(AssetPreloadData preloadData) : base(preloadData)
        {
            m_GameObject = sourceFile.ReadPPtr();
        }
    }
}
