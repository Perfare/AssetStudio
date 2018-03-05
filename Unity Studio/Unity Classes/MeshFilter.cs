﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class MeshFilter
    {
        public long preloadIndex;
        public PPtr m_GameObject = new PPtr();
        public PPtr m_Mesh = new PPtr();

        public MeshFilter(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.Reader;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = reader.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            m_Mesh = sourceFile.ReadPPtr();
        }
    }
}
