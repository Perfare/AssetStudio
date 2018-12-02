using System;
using System.Collections.Generic;
using SharpDX;
using RectangleF = System.Drawing.RectangleF;

namespace AssetStudio
{
    public enum SpritePackingRotation
    {
        kSPRNone = 0,
        kSPRFlipHorizontal = 1,
        kSPRFlipVertical = 2,
        kSPRRotate180 = 3,
        kSPRRotate90 = 4
    };

    public enum SpritePackingMode
    {
        kSPMTight = 0,
        kSPMRectangle
    };

    public class SpriteSettings
    {
        public uint settingsRaw;

        public uint packed;
        public SpritePackingMode packingMode;
        public SpritePackingRotation packingRotation;

        public SpriteSettings(ObjectReader reader)
        {
            settingsRaw = reader.ReadUInt32();

            packed = settingsRaw & 1; //1
            packingMode = (SpritePackingMode)((settingsRaw >> 1) & 1); //1
            packingRotation = (SpritePackingRotation)((settingsRaw >> 2) & 0xf); //4

            //meshType = (settingsRaw >> 6) & 1; //1
            //reserved
        }
    }

    public class SpriteVertex
    {
        public Vector3 pos;
        public Vector2 uv;

        public SpriteVertex(ObjectReader reader)
        {
            var version = reader.version;

            pos = reader.ReadVector3();
            if (version[0] < 4 || (version[0] == 4 && version[1] <= 3)) //4.3 and down
            {
                uv = reader.ReadVector2();
            }
        }
    }

    public class BoneWeights4
    {
        public float[] weight;
        public int[] boneIndex;

        public BoneWeights4(ObjectReader reader)
        {
            weight = reader.ReadSingleArray(4);
            boneIndex = reader.ReadInt32Array(4);
        }
    }

    public class StreamInfo
    {
        public uint channelMask;
        public uint offset;
        public uint stride;
        public uint align;
        public byte dividerOp;
        public ushort frequency;

        public StreamInfo() { }

        public StreamInfo(ObjectReader reader)
        {
            var version = reader.version;

            channelMask = reader.ReadUInt32();
            offset = reader.ReadUInt32();

            if (version[0] < 4)
            {
                stride = reader.ReadUInt32();
                align = reader.ReadUInt32();
            }
            else
            {
                stride = reader.ReadByte();
                dividerOp = reader.ReadByte();
                frequency = reader.ReadUInt16();
            }
        }
    }

    public class ChannelInfo
    {
        public byte stream;
        public byte offset;
        public byte format;
        public byte dimension;

        public ChannelInfo(ObjectReader reader)
        {
            stream = reader.ReadByte();
            offset = reader.ReadByte();
            format = reader.ReadByte();
            dimension = reader.ReadByte();
        }
    }

    public class VertexData
    {
        public uint m_CurrentChannels;
        public uint m_VertexCount;
        public ChannelInfo[] m_Channels;
        public StreamInfo[] m_Streams;
        public byte[] m_DataSize;

        public VertexData(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] < 2018)//2018 down
            {
                m_CurrentChannels = reader.ReadUInt32();
            }
            m_VertexCount = reader.ReadUInt32();

            if (version[0] >= 4)
            {
                var m_ChannelsSize = reader.ReadInt32();
                m_Channels = new ChannelInfo[m_ChannelsSize];
                for (int i = 0; i < m_ChannelsSize; i++)
                {
                    m_Channels[i] = new ChannelInfo(reader);
                }
            }

            if (version[0] < 5) //5.0 down
            {
                if (version[0] < 4)
                {
                    m_Streams = new StreamInfo[4];
                }
                else
                {
                    m_Streams = new StreamInfo[reader.ReadInt32()];
                }

                for (int i = 0; i < m_Streams.Length; i++)
                {
                    m_Streams[i] = new StreamInfo(reader);
                }
            }

            m_DataSize = reader.ReadBytes(reader.ReadInt32());
            reader.AlignStream();
        }
    }

    public class SubMesh
    {
        public uint firstByte;
        public uint indexCount;
        public int topology;
        public uint triangleCount;
        public uint baseVertex;
        public uint firstVertex;
        public uint vertexCount;
        public AABB localAABB;

        public SubMesh(ObjectReader reader)
        {
            var version = reader.version;

            firstByte = reader.ReadUInt32();
            indexCount = reader.ReadUInt32();
            topology = reader.ReadInt32();

            if (version[0] < 4) //4.0 down
            {
                triangleCount = reader.ReadUInt32();
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                baseVertex = reader.ReadUInt32();
            }

            if (version[0] >= 3) //3.0 and up
            {
                firstVertex = reader.ReadUInt32();
                vertexCount = reader.ReadUInt32();
                localAABB = new AABB(reader);
            }
        }
    }

    public class SpriteRenderData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public SubMesh[] m_SubMeshes;
        public byte[] m_IndexBuffer;
        public VertexData m_VertexData;
        public SpriteVertex[] vertices;
        public ushort[] indices;
        public Matrix[] m_Bindpose;
        public BoneWeights4[] m_SourceSkin;
        public RectangleF textureRect;
        public Vector2 textureRectOffset;
        public Vector2 atlasRectOffset;
        public SpriteSettings settingsRaw;
        public Vector4 uvTransform;
        public float downscaleMultiplier;

        public SpriteRenderData(ObjectReader reader)
        {
            var version = reader.version;

            texture = new PPtr<Texture2D>(reader);
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                alphaTexture = new PPtr<Texture2D>(reader);
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var m_SubMeshesSize = reader.ReadInt32();
                m_SubMeshes = new SubMesh[m_SubMeshesSize];
                for (int i = 0; i < m_SubMeshesSize; i++)
                {
                    m_SubMeshes[i] = new SubMesh(reader);
                }

                m_IndexBuffer = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream();

                m_VertexData = new VertexData(reader);
            }
            else
            {
                var verticesSize = reader.ReadInt32();
                vertices = new SpriteVertex[verticesSize];
                for (int i = 0; i < verticesSize; i++)
                {
                    vertices[i] = new SpriteVertex(reader);
                }

                indices = reader.ReadUInt16Array();
                reader.AlignStream();
            }

            if (version[0] >= 2018) //2018 and up
            {
                m_Bindpose = reader.ReadMatrixArray();

                if (version[0] == 2018 && version[1] < 2) //2018.2 down
                {
                    var m_SourceSkinSize = reader.ReadInt32();
                    for (int i = 0; i < m_SourceSkinSize; i++)
                    {
                        m_SourceSkin[i] = new BoneWeights4(reader);
                    }
                }
            }

            textureRect = reader.ReadRectangleF();
            textureRectOffset = reader.ReadVector2();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                atlasRectOffset = reader.ReadVector2();
            }

            settingsRaw = new SpriteSettings(reader);
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                uvTransform = reader.ReadVector4();
            }

            if (version[0] >= 2017) //2017 and up
            {
                downscaleMultiplier = reader.ReadSingle();
            }
        }
    }

    public sealed class Sprite : NamedObject
    {
        public RectangleF m_Rect;
        public Vector2 m_Offset;
        public Vector4 m_Border;
        public float m_PixelsToUnits;
        public Vector2 m_Pivot;
        public uint m_Extrude;
        public bool m_IsPolygon;
        public KeyValuePair<Guid, long> m_RenderDataKey;
        public string[] m_AtlasTags;
        public PPtr<SpriteAtlas> m_SpriteAtlas;
        public SpriteRenderData m_RD;
        public Vector2[][] m_PhysicsShape;

        public Sprite(ObjectReader reader) : base(reader)
        {
            m_Rect = reader.ReadRectangleF();
            m_Offset = reader.ReadVector2();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                m_Border = reader.ReadVector4();
            }

            m_PixelsToUnits = reader.ReadSingle();
            if (version[0] > 5
                || (version[0] == 5 && version[1] > 4)
                || (version[0] == 5 && version[1] == 4 && version[2] >= 2)) //5.4.2 and up
            {
                m_Pivot = reader.ReadVector2();
            }

            m_Extrude = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) //5.3 and up
            {
                m_IsPolygon = reader.ReadBoolean();
                reader.AlignStream();
            }

            if (version[0] >= 2017) //2017 and up
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                m_RenderDataKey = new KeyValuePair<Guid, long>(first, second);

                m_AtlasTags = reader.ReadStringArray();

                m_SpriteAtlas = new PPtr<SpriteAtlas>(reader);
            }

            m_RD = new SpriteRenderData(reader);

            if (version[0] >= 2017) //2017 and up
            {
                var m_PhysicsShapeSize = reader.ReadInt32();
                m_PhysicsShape = new Vector2[m_PhysicsShapeSize][];
                for (int i = 0; i < m_PhysicsShapeSize; i++)
                {
                    m_PhysicsShape[i] = reader.ReadVector2Array();
                }
            }

            //vector m_Bones 2018 and up
        }
    }
}
