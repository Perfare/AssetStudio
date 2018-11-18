using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class GameObject : EditorExtension
    {
        public List<PPtr> m_Components;
        public string m_Name;
        public PPtr m_Transform;
        public PPtr m_MeshRenderer;
        public PPtr m_MeshFilter;
        public PPtr m_SkinnedMeshRenderer;
        public PPtr m_Animator;

        public GameObject(ObjectReader reader) : base(reader)
        {
            int m_Component_size = reader.ReadInt32();
            m_Components = new List<PPtr>(m_Component_size);
            for (int j = 0; j < m_Component_size; j++)
            {
                if ((version[0] == 5 && version[1] >= 5) || version[0] > 5)//5.5.0 and up
                {
                    m_Components.Add(reader.ReadPPtr());
                }
                else
                {
                    int first = reader.ReadInt32();
                    m_Components.Add(reader.ReadPPtr());
                }
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }
    }
}
