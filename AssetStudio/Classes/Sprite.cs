using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

    public sealed class Sprite : NamedObject
    {
        public RectangleF m_Rect;
        public float m_PixelsToUnits;
        public Vector2 m_Pivot;
        public Tuple<Guid, long> m_RenderDataKey;
        public PPtr texture;
        public PPtr m_SpriteAtlas;
        public RectangleF textureRect;
        public SpriteSettings settingsRaw;
        public PointF[][] m_PhysicsShape; //Vector2[][]

        public Sprite(ObjectReader reader) : base(reader)
        {
            //Rectf m_Rect
            m_Rect = reader.ReadRectangleF();
            //Vector2f m_Offset
            reader.Position += 8;
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                //Vector4f m_Border
                reader.Position += 16;
            }

            m_PixelsToUnits = reader.ReadSingle();
            if (version[0] > 5
                || (version[0] == 5 && version[1] > 4)
                || (version[0] == 5 && version[1] == 4 && version[2] >= 2)) //5.4.2 and up
            {
                //Vector2f m_Pivot
                m_Pivot = reader.ReadVector2();
            }

            var m_Extrude = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) //5.3 and up
            {
                var m_IsPolygon = reader.ReadBoolean();
                reader.AlignStream(4);
            }

            if (version[0] >= 2017) //2017 and up
            {
                //pair m_RenderDataKey
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                m_RenderDataKey = new Tuple<Guid, long>(first, second);
                //vector m_AtlasTags
                var size = reader.ReadInt32();
                for (int i = 0; i < size; i++)
                {
                    var data = reader.ReadAlignedString();
                }

                //PPtr<SpriteAtlas> m_SpriteAtlas
                m_SpriteAtlas = reader.ReadPPtr();
            }

            //SpriteRenderData m_RD
            //  PPtr<Texture2D> texture
            texture = reader.ReadPPtr();
            //  PPtr<Texture2D> alphaTexture
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                var alphaTexture = reader.ReadPPtr();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                //  vector m_SubMeshes
                var size = reader.ReadInt32();
                //      SubMesh data
                if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
                {
                    reader.Position += 48 * size;
                }
                else
                {
                    reader.Position += 44 * size;
                }

                //  vector m_IndexBuffer
                size = reader.ReadInt32();
                reader.Position += size; //UInt8 data   
                reader.AlignStream(4);
                //  VertexData m_VertexData
                if (version[0] < 2018)//2018 down
                {
                    var m_CurrentChannels = reader.ReadInt32();
                }
                var m_VertexCount = reader.ReadUInt32();
                //      vector m_Channels
                size = reader.ReadInt32();
                reader.Position += size * 4; //ChannelInfo data
                                             //      TypelessData m_DataSize
                size = reader.ReadInt32();
                reader.Position += size; //UInt8 data   
                reader.AlignStream(4);

                if (version[0] >= 2018)//2018 and up
                {
                    //	vector m_Bindpose
                    //			Matrix4x4f data
                    size = reader.ReadInt32();
                    reader.Position += size * 64;
                    if (version[0] == 2018 && version[1] < 2) //2018.2 down
                    {
                        //	vector m_SourceSkin
                        //			BoneWeights4 data
                        size = reader.ReadInt32();
                        reader.Position += size * 32;
                    }
                }
            }
            else
            {
                //  vector vertices
                var size = reader.ReadInt32();
                for (int i = 0; i < size; i++)
                {
                    //SpriteVertex data
                    reader.Position += 12; //Vector3f pos
                    if (version[0] < 4 || (version[0] == 4 && version[1] <= 3)) //4.3 and down
                        reader.Position += 8; //Vector2f uv
                }

                //  vector indices
                size = reader.ReadInt32();
                reader.Position += 2 * size; //UInt16 data
                reader.AlignStream(4);
            }

            //  Rectf textureRect
            textureRect = reader.ReadRectangleF();
            //  Vector2f textureRectOffset
            reader.Position += 8;
            //  Vector2f atlasRectOffset - 5.6 and up
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                reader.Position += 8;
            }
            //  unsigned int settingsRaw
            settingsRaw = new SpriteSettings(reader);
            //  Vector4f uvTransform - 4.5 and up
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                reader.Position += 16;
            }
            if (version[0] >= 2017) //2017 and up
            {
                //  float downscaleMultiplier - 2017 and up
                reader.Position += 4;
                //vector m_PhysicsShape - 2017 and up
                var m_PhysicsShape_size = reader.ReadInt32();
                m_PhysicsShape = new PointF[m_PhysicsShape_size][];
                for (int i = 0; i < m_PhysicsShape_size; i++)
                {
                    var data_size = reader.ReadInt32();
                    //Vector2f
                    m_PhysicsShape[i] = new PointF[data_size];
                    for (int j = 0; j < data_size; j++)
                    {
                        m_PhysicsShape[i][j] = new PointF(reader.ReadSingle(), reader.ReadSingle());
                    }
                }
            }
            //vector m_Bones 2018 and up
        }
    }
}
