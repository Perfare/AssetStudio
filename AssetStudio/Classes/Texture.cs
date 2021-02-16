using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Texture : NamedObject
    {
        protected Texture(ObjectReader reader) : base(reader)
        {
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                if (version[0] > 2020 || (version[0] == 2020 && version[1] >= 2)) //2020.2 and up
                {
                    var m_IsAlphaChannelOptional = reader.ReadBoolean();
                }
                reader.AlignStream();
            }
        }
    }

    public enum TextureFormat
    {
        Alpha8 = 1,
        ARGB4444,
        RGB24,
        RGBA32,
        ARGB32,
        RGB565 = 7,
        R16 = 9,
        DXT1,
        DXT5 = 12,
        RGBA4444,
        BGRA32,
        RHalf,
        RGHalf,
        RGBAHalf,
        RFloat,
        RGFloat,
        RGBAFloat,
        YUY2,
        RGB9e5Float,
        BC4 = 26,
        BC5,
        BC6H = 24,
        BC7,
        DXT1Crunched = 28,
        DXT5Crunched,
        PVRTC_RGB2,
        PVRTC_RGBA2,
        PVRTC_RGB4,
        PVRTC_RGBA4,
        ETC_RGB4,
        ATC_RGB4,
        ATC_RGBA8,
        EAC_R = 41,
        EAC_R_SIGNED,
        EAC_RG,
        EAC_RG_SIGNED,
        ETC2_RGB,
        ETC2_RGBA1,
        ETC2_RGBA8,
        ASTC_RGB_4x4,
        ASTC_RGB_5x5,
        ASTC_RGB_6x6,
        ASTC_RGB_8x8,
        ASTC_RGB_10x10,
        ASTC_RGB_12x12,
        ASTC_RGBA_4x4,
        ASTC_RGBA_5x5,
        ASTC_RGBA_6x6,
        ASTC_RGBA_8x8,
        ASTC_RGBA_10x10,
        ASTC_RGBA_12x12,
        ETC_RGB4_3DS,
        ETC_RGBA8_3DS,
        RG16,
        R8,
        ETC_RGB4Crunched,
        ETC2_RGBA8Crunched,
        ASTC_HDR_4x4,
        ASTC_HDR_5x5,
        ASTC_HDR_6x6,
        ASTC_HDR_8x8,
        ASTC_HDR_10x10,
        ASTC_HDR_12x12,
    }
}
