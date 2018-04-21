using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
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
            var reader = preloadData.InitReader();

            m_GameObject = sourceFile.ReadPPtr();
            m_LocalRotation = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            m_LocalPosition = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            m_LocalScale = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            int m_ChildrenCount = reader.ReadInt32();
            for (int j = 0; j < m_ChildrenCount; j++)
            {
                m_Children.Add(sourceFile.ReadPPtr());
            }
            m_Father = sourceFile.ReadPPtr();
        }
    }
}
