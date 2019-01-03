using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class Transform : Component
    {
        public Quaternion m_LocalRotation;
        public Vector3 m_LocalPosition;
        public Vector3 m_LocalScale;
        public PPtr<Transform>[] m_Children;
        public PPtr<Transform> m_Father;

        public Transform(ObjectReader reader) : base(reader)
        {
            m_LocalRotation = reader.ReadQuaternion();
            m_LocalPosition = reader.ReadVector3();
            m_LocalScale = reader.ReadVector3();

            int m_ChildrenCount = reader.ReadInt32();
            m_Children = new PPtr<Transform>[m_ChildrenCount];
            for (int i = 0; i < m_ChildrenCount; i++)
            {
                m_Children[i] = new PPtr<Transform>(reader);
            }
            m_Father = new PPtr<Transform>(reader);
        }
    }
}
