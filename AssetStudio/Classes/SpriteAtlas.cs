using System;
using System.Collections.Generic;

namespace AssetStudio
{
    public class SpriteAtlasData
    {
        public PPtr<Texture2D> texture;
        public PPtr<Texture2D> alphaTexture;
        public Rectf textureRect;
        public Vector2 textureRectOffset;
        public Vector2 atlasRectOffset;
        public Vector4 uvTransform;
        public float downscaleMultiplier;
        public SpriteSettings settingsRaw;
        public SecondarySpriteTexture[] secondaryTextures;

        public SpriteAtlasData(ObjectReader reader)
        {
            var version = reader.version;
            texture = new PPtr<Texture2D>(reader);
            alphaTexture = new PPtr<Texture2D>(reader);
            textureRect = new Rectf(reader);
            textureRectOffset = reader.ReadVector2();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                atlasRectOffset = reader.ReadVector2();
            }
            uvTransform = reader.ReadVector4();
            downscaleMultiplier = reader.ReadSingle();
            settingsRaw = new SpriteSettings(reader);
            if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
            {
                var secondaryTexturesSize = reader.ReadInt32();
                secondaryTextures = new SecondarySpriteTexture[secondaryTexturesSize];
                for (int i = 0; i < secondaryTexturesSize; i++)
                {
                    secondaryTextures[i] = new SecondarySpriteTexture(reader);
                }
                reader.AlignStream();
            }
        }
    }

    public sealed class SpriteAtlas : NamedObject
    {
        public PPtr<Sprite>[] m_PackedSprites;
        public Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData> m_RenderDataMap;

        public SpriteAtlas(ObjectReader reader) : base(reader)
        {
            var m_PackedSpritesSize = reader.ReadInt32();
            m_PackedSprites = new PPtr<Sprite>[m_PackedSpritesSize];
            for (int i = 0; i < m_PackedSpritesSize; i++)
            {
                m_PackedSprites[i] = new PPtr<Sprite>(reader);
            }

            var m_PackedSpriteNamesToIndex = reader.ReadStringArray();

            var m_RenderDataMapSize = reader.ReadInt32();
            m_RenderDataMap = new Dictionary<KeyValuePair<Guid, long>, SpriteAtlasData>(m_RenderDataMapSize);
            for (int i = 0; i < m_RenderDataMapSize; i++)
            {
                var first = new Guid(reader.ReadBytes(16));
                var second = reader.ReadInt64();
                var value = new SpriteAtlasData(reader);
                m_RenderDataMap.Add(new KeyValuePair<Guid, long>(first, second), value);
            }
            //string m_Tag
            //bool m_IsVariant
        }
    }
}
