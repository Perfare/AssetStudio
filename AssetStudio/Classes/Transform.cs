using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class Transform : Component
    {
        public float[] m_LocalRotation;
        public float[] m_LocalPosition;
        public float[] m_LocalScale;
        public List<PPtr> m_Children;
        public PPtr m_Father;

        public Transform(AssetPreloadData preloadData) : base(preloadData)
        {
            m_LocalRotation = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            m_LocalPosition = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            m_LocalScale = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            int m_ChildrenCount = reader.ReadInt32();
            m_Children = new List<PPtr>(m_ChildrenCount);
            for (int j = 0; j < m_ChildrenCount; j++)
            {
                m_Children.Add(sourceFile.ReadPPtr());
            }
            m_Father = sourceFile.ReadPPtr();
        }
    }
}
