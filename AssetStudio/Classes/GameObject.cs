using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class GameObject : EditorExtension
    {
        public List<PPtr<Component>> m_Components;
        public string m_Name;

        public Transform m_Transform;
        public MeshRenderer m_MeshRenderer;
        public MeshFilter m_MeshFilter;
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        public Animator m_Animator;
        public Animation m_Animation;

        public GameObject(ObjectReader reader) : base(reader)
        {
            int m_Component_size = reader.ReadInt32();
            m_Components = new List<PPtr<Component>>(m_Component_size);
            for (int j = 0; j < m_Component_size; j++)
            {
                if ((version[0] == 5 && version[1] >= 5) || version[0] > 5) //5.5.0 and up
                {
                    m_Components.Add(new PPtr<Component>(reader));
                }
                else
                {
                    int first = reader.ReadInt32();
                    m_Components.Add(new PPtr<Component>(reader));
                }
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }
    }
}
