using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class GameObject : TreeNode
    {
        public PPtr m_Transform;
        public PPtr m_Renderer;
        public PPtr m_MeshFilter;
        public PPtr m_SkinnedMeshRenderer;
        public int m_Layer;
        public string m_Name;
        public ushort m_Tag;
        public bool m_IsActive;

        public string uniqueID = "0";//this way file and folder TreeNodes will be treated as FBX scene

        public GameObject(AssetPreloadData preloadData)
        {
            if (preloadData != null)
            {
                var sourceFile = preloadData.sourceFile;
                var a_Stream = preloadData.sourceFile.a_Stream;
                a_Stream.Position = preloadData.Offset;

                uniqueID = preloadData.uniqueID;

                if (sourceFile.platform == -2)
                {
                    uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                    PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                    PPtr m_PrefabInternal = sourceFile.ReadPPtr();
                }

                int m_Component_size = a_Stream.ReadInt32();
                for (int j = 0; j < m_Component_size; j++)
                {
                    int m_Component_type = a_Stream.ReadInt32();

                    switch (m_Component_type)
                    {
                        case 4:
                            m_Transform = sourceFile.ReadPPtr();
                            break;
                        case 23:
                            m_Renderer = sourceFile.ReadPPtr();
                            break;
                        case 33:
                            m_MeshFilter = sourceFile.ReadPPtr();
                            break;
                        case 137:
                            m_SkinnedMeshRenderer = sourceFile.ReadPPtr();
                            break;
                        default:
                            PPtr m_Component = sourceFile.ReadPPtr();
                            break;
                    }
                }

                m_Layer = a_Stream.ReadInt32();
                int namesize = a_Stream.ReadInt32();
                m_Name = a_Stream.ReadAlignedString(namesize);
                if (m_Name == "") { m_Name = "GameObject #" + uniqueID; }
                m_Tag = a_Stream.ReadUInt16();
                m_IsActive = a_Stream.ReadBoolean();

                base.Text = m_Name;
                preloadData.Text = m_Name;
                //name should be unique
                base.Name = uniqueID;
            }
        }
    }
}
