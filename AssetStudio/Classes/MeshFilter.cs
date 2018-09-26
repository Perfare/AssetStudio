using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MeshFilter : Component
    {
        public long preloadIndex;
        public PPtr m_Mesh;

        public MeshFilter(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Mesh = sourceFile.ReadPPtr();
        }
    }
}
