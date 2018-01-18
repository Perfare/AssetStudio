using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class SpriteAtlas
    {
        public List<PPtr> m_PackedSprites = new List<PPtr>();
        public List<PPtr> textures = new List<PPtr>();
        public List<RectangleF> textureRects = new List<RectangleF>();


        public SpriteAtlas(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            var m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            //vector m_PackedSprites
            var size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                //PPtr<Sprite> data
                m_PackedSprites.Add(sourceFile.ReadPPtr());
            }
            //vector m_PackedSpriteNamesToIndex
            size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                var data = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            }
            //map m_RenderDataMap
            size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                //pair first
                a_Stream.Position += 24;
                //SpriteAtlasData second
                //  PPtr<Texture2D> texture
                textures.Add(sourceFile.ReadPPtr());
                // PPtr<Texture2D> alphaTexture
                var alphaTexture = sourceFile.ReadPPtr();
                //  Rectf textureRect
                textureRects.Add(new RectangleF(a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle()));
                //  Vector2f textureRectOffset
                a_Stream.Position += 8;
                if (sourceFile.version[0] > 2017 || (sourceFile.version[0] == 2017 && sourceFile.version[1] >= 2))//2017.2 and up
                {
                    //  Vector2f atlasRectOffset
                    a_Stream.Position += 8;
                }
                //  Vector4f uvTransform
                //  float downscaleMultiplier
                //  unsigned int settingsRaw
                a_Stream.Position += 24;
            }
            //string m_Tag
            //bool m_IsVariant
        }
    }
}
