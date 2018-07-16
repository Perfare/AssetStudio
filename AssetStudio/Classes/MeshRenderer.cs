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

    public class MeshRenderer
    {
        public PPtr m_GameObject;
        public PPtr[] m_Materials;
        public StaticBatchInfo m_StaticBatchInfo;
        public uint[] m_SubsetIndices;

        protected MeshRenderer() { }

        public MeshRenderer(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var version = sourceFile.version;
            var reader = preloadData.InitReader();

            m_GameObject = sourceFile.ReadPPtr();
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
            }
        }
    }
}
