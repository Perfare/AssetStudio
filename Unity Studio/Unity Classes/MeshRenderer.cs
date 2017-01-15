using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class MeshRenderer
    {
        public PPtr m_GameObject;
        public bool m_Enabled;
        public byte m_CastShadows; //bool prior to Unity 5
        public bool m_ReceiveShadows;
        public ushort m_LightmapIndex;
        public ushort m_LightmapIndexDynamic;
        public PPtr[] m_Materials;

        public MeshRenderer(AssetPreloadData preloadData)
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

            if (sourceFile.version[0] < 5)
            {
                m_Enabled = a_Stream.ReadBoolean();
                m_CastShadows = a_Stream.ReadByte();
                m_ReceiveShadows = a_Stream.ReadBoolean();
                m_LightmapIndex = a_Stream.ReadByte();
            }
            else
            {
                m_Enabled = a_Stream.ReadBoolean();
                a_Stream.AlignStream(4);
                m_CastShadows = a_Stream.ReadByte();
                m_ReceiveShadows = a_Stream.ReadBoolean();
                a_Stream.AlignStream(4);

                m_LightmapIndex = a_Stream.ReadUInt16();
                m_LightmapIndexDynamic = a_Stream.ReadUInt16();
            }

            if (sourceFile.version[0] >= 3) { a_Stream.Position += 16; } //Vector4f m_LightmapTilingOffset
            if (sourceFile.version[0] >= 5) { a_Stream.Position += 16; } //Vector4f m_LightmapTilingOffsetDynamic

            m_Materials = new PPtr[a_Stream.ReadInt32()];
            for (int m = 0; m < m_Materials.Length; m++)
            {
                m_Materials[m] = sourceFile.ReadPPtr();
            }

        }
    }
}
