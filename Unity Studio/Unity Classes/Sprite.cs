using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class Sprite
    {
        public string m_Name;
        public PPtr texture;
        public PPtr m_SpriteAtlas;
        public RectangleF textureRect;

        public Sprite(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            if (readSwitch)
            {
                //Rectf m_Rect
                var m_Rect = new RectangleF(a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle());
                //Vector2f m_Offset
                a_Stream.Position += 8;
                if (sourceFile.version[0] > 4 || (sourceFile.version[0] == 4 && sourceFile.version[1] >= 2)) //4.2 and up
                {
                    //Vector4f m_Border
                    a_Stream.Position += 16;
                }

                var m_PixelsToUnits = a_Stream.ReadSingle();
                if (sourceFile.version[0] > 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] >= 5)) //5.5 and up
                {
                    //Vector2f m_Pivot
                    a_Stream.Position += 8;
                }

                var m_Extrude = a_Stream.ReadUInt32();
                if (sourceFile.version[0] > 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] >= 1)) //5.1 and up
                {
                    var m_IsPolygon = a_Stream.ReadBoolean();
                    a_Stream.AlignStream(4);
                }

                if (sourceFile.version[0] >= 2017) //2017 and up
                {
                    //pair m_RenderDataKey
                    a_Stream.Position += 24;
                    //vector m_AtlasTags
                    var size = a_Stream.ReadInt32();
                    for (int i = 0; i < size; i++)
                    {
                        var data = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                    }

                    //PPtr<SpriteAtlas> m_SpriteAtlas
                    m_SpriteAtlas = sourceFile.ReadPPtr();
                }

                //SpriteRenderData m_RD
                //  PPtr<Texture2D> texture
                texture = sourceFile.ReadPPtr();
                //  PPtr<Texture2D> alphaTexture
                if (sourceFile.version[0] >= 5) //5.0 and up
                {
                    var alphaTexture = sourceFile.ReadPPtr();
                }

                if (sourceFile.version[0] > 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] >= 6)) //5.6 and up
                {
                    //  vector m_SubMeshes
                    var size = a_Stream.ReadInt32();
                    //      SubMesh data
                    if (sourceFile.version[0] > 2017 || (sourceFile.version[0] == 2017 && sourceFile.version[1] >= 3)) //2017.3 and up
                    {
                        a_Stream.Position += 48 * size;
                    }
                    else
                    {
                        a_Stream.Position += 44 * size;
                    }

                    //  vector m_IndexBuffer
                    size = a_Stream.ReadInt32();
                    a_Stream.Position += size; //UInt8 data   
                    a_Stream.AlignStream(4);
                    //  VertexData m_VertexData
                    var m_CurrentChannels = a_Stream.ReadInt32();
                    var m_VertexCount = a_Stream.ReadUInt32();
                    //      vector m_Channels
                    size = a_Stream.ReadInt32();
                    a_Stream.Position += size * 4; //ChannelInfo data
                    //      TypelessData m_DataSize
                    size = a_Stream.ReadInt32();
                    a_Stream.Position += size; //UInt8 data   
                    a_Stream.AlignStream(4);
                }
                else
                {
                    //  vector vertices
                    var size = a_Stream.ReadInt32();
                    for (int i = 0; i < size; i++)
                    {
                        //SpriteVertex data
                        a_Stream.Position += 12; //Vector3f pos
                        if (sourceFile.version[0] < 4 || (sourceFile.version[0] == 4 && sourceFile.version[1] <= 1)) //4.1 and down
                            a_Stream.Position += 8; //Vector2f uv
                    }

                    //  vector indices
                    size = a_Stream.ReadInt32();
                    a_Stream.Position += 2 * size; //UInt16 data
                    a_Stream.AlignStream(4);
                }

                //  Rectf textureRect
                textureRect = new RectangleF(a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle());
                //  Vector2f textureRectOffset
                //  Vector2f atlasRectOffset - 5.6 and up
                //  unsigned int settingsRaw
                //  Vector4f uvTransform - 4.2 and up
                //  float downscaleMultiplier - 2017 and up
                //vector m_PhysicsShape - 2017 and up
            }
            else
            {
                preloadData.extension = ".png";
                preloadData.Text = m_Name;
            }
        }
    }
}
