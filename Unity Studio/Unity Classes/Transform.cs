using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class Transform
    {
        public PPtr m_GameObject = new PPtr();
        public float[] m_LocalRotation;
        public float[] m_LocalPosition;
        public float[] m_LocalScale;
        public List<PPtr> m_Children = new List<PPtr>();
        public PPtr m_Father = new PPtr();//can be transform or type 224 (as seen in Minions)

        public Transform(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            m_LocalRotation = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            m_LocalPosition = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            m_LocalScale = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
            int m_ChildrenCount = a_Stream.ReadInt32();
            for (int j = 0; j < m_ChildrenCount; j++)
            {
                m_Children.Add(sourceFile.ReadPPtr());
            }
            m_Father = sourceFile.ReadPPtr();
        }
    }
}
