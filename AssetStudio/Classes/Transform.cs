using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class Transform
    {
        public PPtr m_GameObject;
        public float[] m_LocalRotation;
        public float[] m_LocalPosition;
        public float[] m_LocalScale;
        public List<PPtr> m_Children;
        public PPtr m_Father;

        public Transform(Vector3 t, Quaternion q, Vector3 s)
        {
            m_LocalPosition = new[] { t.X, t.Y, t.Z };
            m_LocalRotation = new[] { q.X, q.Y, q.Z, q.W };
            m_LocalScale = new[] { s.X, s.Y, s.Z };
        }

        public Transform(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();

            m_GameObject = sourceFile.ReadPPtr();
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
