using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class StaticBatchInfo
    {
        public ushort firstSubMesh;
        public ushort subMeshCount;
    }

    public abstract class Renderer : Component
    {
        public PPtr[] m_Materials;
        public StaticBatchInfo m_StaticBatchInfo;
        public uint[] m_SubsetIndices;

        protected Renderer(AssetPreloadData preloadData) : base(preloadData)
        {
            if (version[0] < 5)
            {
                var m_Enabled = reader.ReadBoolean();
                var m_CastShadows = reader.ReadByte();
                var m_ReceiveShadows = reader.ReadBoolean();
                var m_LightmapIndex = reader.ReadByte();
            }
            else
            {
                var m_Enabled = reader.ReadBoolean();
                reader.AlignStream(4);
                var m_CastShadows = reader.ReadByte();
                var m_ReceiveShadows = reader.ReadBoolean();
                reader.AlignStream(4);
                if (version[0] >= 2018)//2018 and up
                {
                    var m_RenderingLayerMask = reader.ReadUInt32();
                }
                var m_LightmapIndex = reader.ReadUInt16();
                var m_LightmapIndexDynamic = reader.ReadUInt16();
            }

            if (version[0] >= 3)
            {
                reader.Position += 16;//Vector4f m_LightmapTilingOffset
            }

            if (version[0] >= 5)
            {
                reader.Position += 16;//Vector4f m_LightmapTilingOffsetDynamic
            }

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
                    m_StaticBatchInfo = new StaticBatchInfo
                    {
                        firstSubMesh = reader.ReadUInt16(),
                        subMeshCount = reader.ReadUInt16()
                    };
                }
                else
                {
                    int numSubsetIndices = reader.ReadInt32();
                    m_SubsetIndices = reader.ReadUInt32Array(numSubsetIndices);
                }

                var m_StaticBatchRoot = sourceFile.ReadPPtr();

                if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 4) || sourceFile.version[0] > 5)//5.4.0 and up
                {
                    var m_ProbeAnchor = sourceFile.ReadPPtr();
                    var m_LightProbeVolumeOverride = sourceFile.ReadPPtr();
                }
                else if (version[0] >= 4 || (version[0] == 3 && version[1] >= 5))//3.5 - 5.3
                {
                    var m_UseLightProbes = reader.ReadBoolean();
                    reader.AlignStream(4);
                    if (version[0] == 5)//5.0 and up
                    {
                        int m_ReflectionProbeUsage = reader.ReadInt32();
                    }
                    var m_LightProbeAnchor = sourceFile.ReadPPtr();
                }

                if (version[0] >= 5 || (version[0] == 4 && version[1] >= 3))//4.3 and up
                {
                    if (version[0] == 4 && version[1] == 3)//4.3
                    {
                        int m_SortingLayer = reader.ReadInt16();
                    }
                    else
                    {
                        int m_SortingLayerID = reader.ReadInt32();
                        //SInt16 m_SortingOrder 5.6 and up
                    }

                    int m_SortingOrder = reader.ReadInt16();
                    reader.AlignStream(4);
                }
            }
        }
    }
}
