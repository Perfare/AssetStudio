using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class SkinnedMeshRenderer
    {
        public PPtr m_GameObject;
        public bool m_Enabled;
        public bool m_CastShadows;
        public bool m_ReceiveShadows;
        public ushort m_LightmapIndex;
        public ushort m_LightmapIndexDynamic;
        public PPtr[] m_Materials;
        public PPtr m_Mesh;

        public SkinnedMeshRenderer(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var version = preloadData.sourceFile.version;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            m_Enabled = a_Stream.ReadBoolean();
            m_CastShadows = a_Stream.ReadBoolean();
            m_ReceiveShadows = a_Stream.ReadBoolean();
            if (sourceFile.version[0] < 5) { m_LightmapIndex = a_Stream.ReadByte(); }
            else
            {
                a_Stream.Position += 5; //suspicious alignment, could be 2 alignments between bools
                m_LightmapIndex = a_Stream.ReadUInt16();
                m_LightmapIndexDynamic = a_Stream.ReadUInt16();
            }

            if (version[0] >= 3) { a_Stream.Position += 16; } //m_LightmapTilingOffset vector4d

            m_Materials = new PPtr[a_Stream.ReadInt32()];
            for (int m = 0; m < m_Materials.Length; m++)
            {
                m_Materials[m] = sourceFile.ReadPPtr();
            }

            if (version[0] < 3) { a_Stream.Position += 16; } //m_LightmapTilingOffset vector4d
            else
            {
                int m_SubsetIndices_size = a_Stream.ReadInt32();
                a_Stream.Position += m_SubsetIndices_size * 4;
                PPtr m_StaticBatchRoot = sourceFile.ReadPPtr();

                if ((version[0] == 3 && version[1] >= 5) || version[0] >= 4)
                {
                    bool m_UseLightProbes = a_Stream.ReadBoolean();
                    a_Stream.Position += 3; //alignment
                    if (version[0] == 5) { int m_ReflectionProbeUsage = a_Stream.ReadInt32(); }
                    //did I ever check if the anchor is conditioned by the bool?
                    PPtr m_LightProbeAnchor = sourceFile.ReadPPtr();
                }

                if (version[0] >= 4 && version[1] >= 3)
                {
                    if (version[1] >= 5) { int m_SortingLayer = a_Stream.ReadInt32(); }
                    else { int m_SortingLayer = a_Stream.ReadInt16(); }
                    int m_SortingOrder = a_Stream.ReadInt16();
                    a_Stream.AlignStream(4);
                }
            }

            int m_Quality = a_Stream.ReadInt32();
            bool m_UpdateWhenOffscreen = a_Stream.ReadBoolean();
            bool m_SkinNormals = a_Stream.ReadBoolean(); //3.1.0 and below
            a_Stream.Position += 2;

            if (version[0] == 2 && version[1] < 6)
            {
                //this would be the only error if mainVersion is not read in time for a unity 2.x game
                PPtr m_DisableAnimationWhenOffscreen = sourceFile.ReadPPtr();
            }

            m_Mesh = sourceFile.ReadPPtr();

            int m_Bones_size = a_Stream.ReadInt32();
            for (int b = 0; b < m_Bones_size; b++)
            {
                PPtr aBone = sourceFile.ReadPPtr();
            }

            if (version[0] < 3)
            {
                int m_BindPose_size = a_Stream.ReadInt32();
                a_Stream.Position += m_BindPose_size * 16 * 4;//Matrix4x4f
            }
            else if (version[0] >= 3 && version[1] >= 4)
            {
                if (version[1] >= 5)
                {
                    PPtr m_RootBone = sourceFile.ReadPPtr();
                }
                //AABB
                float[] m_Center = new float[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
                float[] m_Extent = new float[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
                bool m_DirtyAABB = a_Stream.ReadBoolean();
            }
        }
    }
}
