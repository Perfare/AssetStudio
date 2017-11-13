using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class GameObject : TreeNode
    {
        public List<PPtr> m_Components = new List<PPtr>();
        public PPtr m_Transform;
        public PPtr m_MeshRenderer;
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
                    if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 5) || sourceFile.version[0] > 5)//5.5.0 and up
                    {
                        m_Components.Add(sourceFile.ReadPPtr());
                    }
                    else
                    {
                        int first = a_Stream.ReadInt32();
                        m_Components.Add(sourceFile.ReadPPtr());
                    }
                }

                m_Layer = a_Stream.ReadInt32();
                m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                if (m_Name == "") { m_Name = "GameObject #" + uniqueID; }
                m_Tag = a_Stream.ReadUInt16();
                m_IsActive = a_Stream.ReadBoolean();

                Text = m_Name;
                preloadData.Text = m_Name;
                //name should be unique
                Name = uniqueID;
            }
        }
    }
}
