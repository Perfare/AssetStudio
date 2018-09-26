using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class SpriteAtlas : NamedObject
    {
        public List<PPtr> textures = new List<PPtr>();
        public List<RectangleF> textureRects = new List<RectangleF>();
        public List<Guid> guids = new List<Guid>();


        public SpriteAtlas(AssetPreloadData preloadData) : base(preloadData)
        {
            //vector m_PackedSprites
            var size = reader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                //PPtr<Sprite> data
                sourceFile.ReadPPtr();
            }
            //vector m_PackedSpriteNamesToIndex
            size = reader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                var data = reader.ReadAlignedString();
            }
            //map m_RenderDataMap
            size = reader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                //pair first
                guids.Add(new Guid(reader.ReadBytes(16)));
                var second = reader.ReadInt64();
                //SpriteAtlasData second
                //  PPtr<Texture2D> texture
                textures.Add(sourceFile.ReadPPtr());
                // PPtr<Texture2D> alphaTexture
                var alphaTexture = sourceFile.ReadPPtr();
                //  Rectf textureRect
                textureRects.Add(new RectangleF(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                //  Vector2f textureRectOffset
                reader.Position += 8;
                if (sourceFile.version[0] > 2017 || (sourceFile.version[0] == 2017 && sourceFile.version[1] >= 2))//2017.2 and up
                {
                    //  Vector2f atlasRectOffset
                    reader.Position += 8;
                }
                //  Vector4f uvTransform
                //  float downscaleMultiplier
                //  unsigned int settingsRaw
                reader.Position += 24;
            }
            //string m_Tag
            //bool m_IsVariant
        }
    }
}
