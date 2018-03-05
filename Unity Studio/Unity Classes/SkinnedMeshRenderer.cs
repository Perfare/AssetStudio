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
        public byte m_CastShadows;
        public bool m_ReceiveShadows;
        public ushort m_LightmapIndex;
        public ushort m_LightmapIndexDynamic;
        public PPtr[] m_Materials;
        public PPtr m_Mesh;
        public PPtr[] m_Bones;

        public SkinnedMeshRenderer(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var version = preloadData.sourceFile.version;
            var reader = preloadData.Reader;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = reader.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_GameObject = sourceFile.ReadPPtr();
            if (sourceFile.version[0] < 5)
            {
                m_Enabled = reader.ReadBoolean();
                m_CastShadows = reader.ReadByte();//bool
                m_ReceiveShadows = reader.ReadBoolean();
                m_LightmapIndex = reader.ReadByte();
            }
            else
            {
                m_Enabled = reader.ReadBoolean();
                reader.AlignStream(4);
                m_CastShadows = reader.ReadByte();
                m_ReceiveShadows = reader.ReadBoolean();
                reader.AlignStream(4);

                m_LightmapIndex = reader.ReadUInt16();
                m_LightmapIndexDynamic = reader.ReadUInt16();
            }

            if (version[0] >= 3) { reader.Position += 16; } //m_LightmapTilingOffset vector4d
            if (sourceFile.version[0] >= 5) { reader.Position += 16; } //Vector4f m_LightmapTilingOffsetDynamic

            m_Materials = new PPtr[reader.ReadInt32()];
            for (int m = 0; m < m_Materials.Length; m++)
            {
                m_Materials[m] = sourceFile.ReadPPtr();
            }

            if (version[0] < 3)
            {
                reader.Position += 16;//m_LightmapTilingOffset vector4d
            }
            else
            {
                if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 5) || sourceFile.version[0] > 5)//5.5.0 and up
                {
                    reader.Position += 4;//m_StaticBatchInfo
                }
                else
                {
                    int m_SubsetIndices_size = reader.ReadInt32();
                    reader.Position += m_SubsetIndices_size * 4;
                }
                PPtr m_StaticBatchRoot = sourceFile.ReadPPtr();

                if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 4) || sourceFile.version[0] > 5)//5.4.0 and up
                {
                    PPtr m_ProbeAnchor = sourceFile.ReadPPtr();
                    PPtr m_LightProbeVolumeOverride = sourceFile.ReadPPtr();
                }
                else if (version[0] >= 4 || (version[0] == 3 && version[1] >= 5))
                {
                    bool m_UseLightProbes = reader.ReadBoolean();
                    reader.Position += 3; //alignment
                    if (version[0] == 5) { int m_ReflectionProbeUsage = reader.ReadInt32(); }
                    //did I ever check if the anchor is conditioned by the bool?
                    PPtr m_LightProbeAnchor = sourceFile.ReadPPtr();
                }

                if (version[0] >= 5 || (version[0] == 4 && version[1] >= 3))
                {
                    if (version[0] == 4 && version[1] <= 3) { int m_SortingLayer = reader.ReadInt16(); }
                    else { int m_SortingLayer = reader.ReadInt32(); }

                    int m_SortingOrder = reader.ReadInt16();
                    reader.AlignStream(4);
                }
            }

            int m_Quality = reader.ReadInt32();
            bool m_UpdateWhenOffscreen = reader.ReadBoolean();
            bool m_SkinNormals = reader.ReadBoolean(); //3.1.0 and below
            reader.Position += 2;

            if (version[0] == 2 && version[1] < 6)
            {
                //this would be the only error if mainVersion is not read in time for a unity 2.x game
                PPtr m_DisableAnimationWhenOffscreen = sourceFile.ReadPPtr();
            }

            m_Mesh = sourceFile.ReadPPtr();

            m_Bones = new PPtr[reader.ReadInt32()];
            for (int b = 0; b < m_Bones.Length; b++)
            {
                m_Bones[b] = sourceFile.ReadPPtr();
            }

            if (version[0] < 3)
            {
                int m_BindPose = reader.ReadInt32();
                reader.Position += m_BindPose * 16 * 4;//Matrix4x4f
            }
            else
            {
                if (version[0] > 4 || (version[0] == 4 && version[1] >= 3))
                {
                    int m_BlendShapeWeights = reader.ReadInt32();
                    reader.Position += m_BlendShapeWeights * 4; //floats
                }

                if (version[0] > 4 || (version[0] >= 3 && version[1] >= 5))
                {
                    PPtr m_RootBone = sourceFile.ReadPPtr();
                }

                if (version[0] > 4 || (version[0] == 3 && version[1] >= 4))
                {
                    //AABB
                    float[] m_Center = { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
                    float[] m_Extent = { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
                    bool m_DirtyAABB = reader.ReadBoolean();
                }
            }
        }
    }
}
