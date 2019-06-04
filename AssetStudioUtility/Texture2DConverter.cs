using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssetStudio
{
    public class Texture2DConverter
    {
        //Texture2D
        private int m_Width;
        private int m_Height;
        private TextureFormat m_TextureFormat;
        private int image_data_size;
        private byte[] image_data;
        private int[] version;

        //DDS Start
        private byte[] dwMagic = { 0x44, 0x44, 0x53, 0x20, 0x7c };
        private int dwFlags = 0x1 + 0x2 + 0x4 + 0x1000;
        //public int dwHeight; m_Height
        //public int dwWidth; m_Width
        private int dwPitchOrLinearSize;
        private int dwMipMapCount = 0x1;
        private int dwSize = 0x20;
        private int dwFlags2;
        private int dwFourCC;
        private int dwRGBBitCount;
        private int dwRBitMask;
        private int dwGBitMask;
        private int dwBBitMask;
        private int dwABitMask;
        private int dwCaps = 0x1000;
        private int dwCaps2 = 0x0;
        //DDS End
        //PVR Start
        private int pvrVersion = 0x03525650;
        private int pvrFlags = 0x0;
        private long pvrPixelFormat;
        private int pvrColourSpace = 0x0;
        private int pvrChannelType = 0x0;
        //public int pvrHeight; m_Height
        //public int pvrWidth; m_Width
        private int pvrDepth = 0x1;
        private int pvrNumSurfaces = 0x1; //For texture arrays
        private int pvrNumFaces = 0x1; //For cube maps
        //public int pvrMIPMapCount; dwMipMapCount
        private int pvrMetaDataSize = 0x0;
        //PVR End
        //KTX Start
        private int glType = 0;
        private int glTypeSize = 1;
        private int glFormat = 0;
        private int glInternalFormat;
        private int glBaseInternalFormat;
        //public int pixelWidth; m_Width
        //public int pixelHeight; m_Height
        private int pixelDepth = 0;
        private int numberOfArrayElements = 0;
        private int numberOfFaces = 1;
        private int numberOfMipmapLevels = 1;
        private int bytesOfKeyValueData = 0;
        //KTX End
        //TextureConverter
        private QFORMAT q_format;
        //texgenpack
        private texgenpack_texturetype texturetype;
        //astc
        private int astcBlockWidth;
        private int astcBlockHeight;


        public Texture2DConverter(Texture2D m_Texture2D)
        {
            var image_data_value = m_Texture2D.image_data.Value;
            image_data_size = image_data_value.Length;
            image_data = new byte[image_data_size];
            Buffer.BlockCopy(image_data_value, 0, image_data, 0, image_data_size);
            m_Width = m_Texture2D.m_Width;
            m_Height = m_Texture2D.m_Height;
            m_TextureFormat = m_Texture2D.m_TextureFormat;
            var mMipMap = m_Texture2D.m_MipMap;
            version = m_Texture2D.version;
            var platform = m_Texture2D.platform;

            if (version[0] < 5 || (version[0] == 5 && version[1] < 2))//5.2 down
            {
                if (mMipMap)
                {
                    dwFlags += 0x20000;
                    dwMipMapCount = Convert.ToInt32(Math.Log(Math.Max(m_Width, m_Height)) / Math.Log(2));
                    dwCaps += 0x400008;
                }
            }
            else
            {
                dwFlags += 0x20000;
                dwMipMapCount = m_Texture2D.m_MipCount;
                dwCaps += 0x400008;
            }


            switch (m_TextureFormat)
            {
                //TODO 导出到DDS容器时应该用原像素还是转换以后的像素？
                case TextureFormat.Alpha8: //test pass
                    {
                        /*dwFlags2 = 0x2;
                        dwRGBBitCount = 0x8;
                        dwRBitMask = 0x0;
                        dwGBitMask = 0x0;
                        dwBBitMask = 0x0;
                        dwABitMask = 0xFF; */

                        //转BGRA32
                        var BGRA32 = Enumerable.Repeat<byte>(0xFF, image_data_size * 4).ToArray();
                        for (var i = 0; i < image_data_size; i++)
                        {
                            BGRA32[i * 4 + 3] = image_data[i];
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.ARGB4444: //test pass
                    {
                        SwapBytesForXbox(platform);

                        /*dwFlags2 = 0x41;
                        dwRGBBitCount = 0x10;
                        dwRBitMask = 0xF00;
                        dwGBitMask = 0xF0;
                        dwBBitMask = 0xF;
                        dwABitMask = 0xF000;*/

                        //转BGRA32
                        var BGRA32 = new byte[image_data_size * 2];
                        for (var i = 0; i < image_data_size / 2; i++)
                        {
                            var pixelNew = new byte[4];
                            var pixelOldShort = BitConverter.ToUInt16(image_data, i * 2);
                            pixelNew[0] = (byte)(pixelOldShort & 0x000f);
                            pixelNew[1] = (byte)((pixelOldShort & 0x00f0) >> 4);
                            pixelNew[2] = (byte)((pixelOldShort & 0x0f00) >> 8);
                            pixelNew[3] = (byte)((pixelOldShort & 0xf000) >> 12);
                            // convert range
                            for (var j = 0; j < 4; j++)
                                pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                            pixelNew.CopyTo(BGRA32, i * 4);
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.RGB24: //test pass
                    {
                        /*dwFlags2 = 0x40;
                        dwRGBBitCount = 0x18;
                        dwRBitMask = 0xFF;
                        dwGBitMask = 0xFF00;
                        dwBBitMask = 0xFF0000;
                        dwABitMask = 0x0;*/

                        //转BGRA32
                        var BGRA32 = new byte[image_data_size / 3 * 4];
                        for (var i = 0; i < image_data_size / 3; i++)
                        {
                            BGRA32[i * 4] = image_data[i * 3 + 2];
                            BGRA32[i * 4 + 1] = image_data[i * 3 + 1];
                            BGRA32[i * 4 + 2] = image_data[i * 3 + 0];
                            BGRA32[i * 4 + 3] = 255;
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.RGBA32: //test pass
                    {
                        /*dwFlags2 = 0x41;
                        dwRGBBitCount = 0x20;
                        dwRBitMask = 0xFF;
                        dwGBitMask = 0xFF00;
                        dwBBitMask = 0xFF0000;
                        dwABitMask = -16777216;*/

                        //转BGRA32
                        var BGRA32 = new byte[image_data_size];
                        for (var i = 0; i < image_data_size; i += 4)
                        {
                            BGRA32[i] = image_data[i + 2];
                            BGRA32[i + 1] = image_data[i + 1];
                            BGRA32[i + 2] = image_data[i + 0];
                            BGRA32[i + 3] = image_data[i + 3];
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.ARGB32://test pass
                    {
                        /*dwFlags2 = 0x41;
                        dwRGBBitCount = 0x20;
                        dwRBitMask = 0xFF00;
                        dwGBitMask = 0xFF0000;
                        dwBBitMask = -16777216;
                        dwABitMask = 0xFF;*/

                        //转BGRA32
                        var BGRA32 = new byte[image_data_size];
                        for (var i = 0; i < image_data_size; i += 4)
                        {
                            BGRA32[i] = image_data[i + 3];
                            BGRA32[i + 1] = image_data[i + 2];
                            BGRA32[i + 2] = image_data[i + 1];
                            BGRA32[i + 3] = image_data[i + 0];
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.RGB565: //test pass
                    {
                        SwapBytesForXbox(platform);

                        dwFlags2 = 0x40;
                        dwRGBBitCount = 0x10;
                        dwRBitMask = 0xF800;
                        dwGBitMask = 0x7E0;
                        dwBBitMask = 0x1F;
                        dwABitMask = 0x0;
                        break;
                    }
                case TextureFormat.R16: //test pass
                    {
                        //转BGRA32
                        var BGRA32 = new byte[image_data_size * 2];
                        for (var i = 0; i < image_data_size; i += 2)
                        {
                            float f = Half.ToHalf(image_data, i);
                            BGRA32[i * 2 + 2] = (byte)Math.Ceiling(f * 255);//R
                            BGRA32[i * 2 + 3] = 255;//A
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.DXT1: //test pass
                case TextureFormat.DXT1Crunched: //test pass
                    {
                        SwapBytesForXbox(platform);

                        if (mMipMap)
                        {
                            dwPitchOrLinearSize = m_Height * m_Width / 2;
                        }
                        dwFlags2 = 0x4;
                        dwFourCC = 0x31545844;
                        dwRGBBitCount = 0x0;
                        dwRBitMask = 0x0;
                        dwGBitMask = 0x0;
                        dwBBitMask = 0x0;
                        dwABitMask = 0x0;

                        q_format = QFORMAT.Q_FORMAT_S3TC_DXT1_RGB;
                        break;
                    }
                case TextureFormat.DXT5: //test pass
                case TextureFormat.DXT5Crunched: //test pass
                    {
                        SwapBytesForXbox(platform);

                        if (mMipMap)
                        {
                            dwPitchOrLinearSize = m_Height * m_Width / 2;
                        }
                        dwFlags2 = 0x4;
                        dwFourCC = 0x35545844;
                        dwRGBBitCount = 0x0;
                        dwRBitMask = 0x0;
                        dwGBitMask = 0x0;
                        dwBBitMask = 0x0;
                        dwABitMask = 0x0;

                        q_format = QFORMAT.Q_FORMAT_S3TC_DXT5_RGBA;
                        break;
                    }
                case TextureFormat.RGBA4444: //test pass
                    {
                        /*dwFlags2 = 0x41;
                        dwRGBBitCount = 0x10;
                        dwRBitMask = 0xF000;
                        dwGBitMask = 0xF00;
                        dwBBitMask = 0xF0;
                        dwABitMask = 0xF;*/

                        //转BGRA32
                        var BGRA32 = new byte[image_data_size * 2];
                        for (var i = 0; i < image_data_size / 2; i++)
                        {
                            var pixelNew = new byte[4];
                            var pixelOldShort = BitConverter.ToUInt16(image_data, i * 2);
                            pixelNew[0] = (byte)((pixelOldShort & 0x00f0) >> 4);
                            pixelNew[1] = (byte)((pixelOldShort & 0x0f00) >> 8);
                            pixelNew[2] = (byte)((pixelOldShort & 0xf000) >> 12);
                            pixelNew[3] = (byte)(pixelOldShort & 0x000f);
                            // convert range
                            for (var j = 0; j < 4; j++)
                                pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                            pixelNew.CopyTo(BGRA32, i * 4);
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.BGRA32: //test pass
                    {
                        dwFlags2 = 0x41;
                        dwRGBBitCount = 0x20;
                        dwRBitMask = 0xFF0000;
                        dwGBitMask = 0xFF00;
                        dwBBitMask = 0xFF;
                        dwABitMask = -16777216;
                        break;
                    }
                case TextureFormat.RHalf: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_R_16F;
                        glInternalFormat = KTXHeader.GL_R16F;
                        glBaseInternalFormat = KTXHeader.GL_RED;
                        break;
                    }
                case TextureFormat.RGHalf: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_RG_HF;
                        glInternalFormat = KTXHeader.GL_RG16F;
                        glBaseInternalFormat = KTXHeader.GL_RG;
                        break;
                    }
                case TextureFormat.RGBAHalf: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_RGBA_HF;
                        glInternalFormat = KTXHeader.GL_RGBA16F;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.RFloat: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_R_F;
                        glInternalFormat = KTXHeader.GL_R32F;
                        glBaseInternalFormat = KTXHeader.GL_RED;
                        break;
                    }
                case TextureFormat.RGFloat: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_RG_F;
                        glInternalFormat = KTXHeader.GL_RG32F;
                        glBaseInternalFormat = KTXHeader.GL_RG;
                        break;
                    }
                case TextureFormat.RGBAFloat: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_RGBA_F;
                        glInternalFormat = KTXHeader.GL_RGBA32F;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.YUY2: //test pass
                    {
                        pvrPixelFormat = 17;
                        break;
                    }
                case TextureFormat.RGB9e5Float: //TODO Test failure
                    {
                        q_format = QFORMAT.Q_FORMAT_RGB9_E5;
                        break;
                    }
                case TextureFormat.BC4: //test pass
                    {
                        texturetype = texgenpack_texturetype.RGTC1;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RED_RGTC1;
                        glBaseInternalFormat = KTXHeader.GL_RED;
                        break;
                    }
                case TextureFormat.BC5: //test pass
                    {
                        texturetype = texgenpack_texturetype.RGTC2;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RG_RGTC2;
                        glBaseInternalFormat = KTXHeader.GL_RG;
                        break;
                    }
                case TextureFormat.BC6H: //test pass
                    {
                        texturetype = texgenpack_texturetype.BPTC_FLOAT;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.BC7: //test pass
                    {
                        texturetype = texgenpack_texturetype.BPTC;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGBA_BPTC_UNORM;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.PVRTC_RGB2: //test pass
                    {
                        pvrPixelFormat = 0;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGB_PVRTC_2BPPV1_IMG;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.PVRTC_RGBA2: //test pass
                    {
                        pvrPixelFormat = 1;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGBA_PVRTC_2BPPV1_IMG;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.PVRTC_RGB4: //test pass
                    {
                        pvrPixelFormat = 2;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGB_PVRTC_4BPPV1_IMG;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.PVRTC_RGBA4: //test pass
                    {
                        pvrPixelFormat = 3;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGBA_PVRTC_4BPPV1_IMG;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.ETC_RGB4Crunched: //test pass
                case TextureFormat.ETC_RGB4_3DS: //test pass
                case TextureFormat.ETC_RGB4: //test pass
                    {
                        pvrPixelFormat = 6;
                        glInternalFormat = KTXHeader.GL_ETC1_RGB8_OES;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.ATC_RGB4: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_ATITC_RGB;
                        glInternalFormat = KTXHeader.GL_ATC_RGB_AMD;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.ATC_RGBA8: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA;
                        glInternalFormat = KTXHeader.GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.EAC_R: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_EAC_R_UNSIGNED;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_R11_EAC;
                        glBaseInternalFormat = KTXHeader.GL_RED;
                        break;
                    }
                case TextureFormat.EAC_R_SIGNED: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_EAC_R_SIGNED;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_SIGNED_R11_EAC;
                        glBaseInternalFormat = KTXHeader.GL_RED;
                        break;
                    }
                case TextureFormat.EAC_RG: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_EAC_RG_UNSIGNED;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RG11_EAC;
                        glBaseInternalFormat = KTXHeader.GL_RG;
                        break;
                    }
                case TextureFormat.EAC_RG_SIGNED: //test pass
                    {
                        q_format = QFORMAT.Q_FORMAT_EAC_RG_SIGNED;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_SIGNED_RG11_EAC;
                        glBaseInternalFormat = KTXHeader.GL_RG;
                        break;
                    }
                case TextureFormat.ETC2_RGB:  //test pass
                    {
                        pvrPixelFormat = 22;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGB8_ETC2;
                        glBaseInternalFormat = KTXHeader.GL_RGB;
                        break;
                    }
                case TextureFormat.ETC2_RGBA1:  //test pass
                    {
                        pvrPixelFormat = 24;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.ETC2_RGBA8Crunched: //test pass
                case TextureFormat.ETC_RGBA8_3DS: //test pass
                case TextureFormat.ETC2_RGBA8:  //test pass
                    {
                        pvrPixelFormat = 23;
                        glInternalFormat = KTXHeader.GL_COMPRESSED_RGBA8_ETC2_EAC;
                        glBaseInternalFormat = KTXHeader.GL_RGBA;
                        break;
                    }
                case TextureFormat.ASTC_RGB_4x4: //test pass
                case TextureFormat.ASTC_RGBA_4x4: //test pass
                    {
                        astcBlockWidth = 4;
                        astcBlockHeight = 4;
                        break;
                    }
                case TextureFormat.ASTC_RGB_5x5: //test pass
                case TextureFormat.ASTC_RGBA_5x5: //test pass
                    {
                        astcBlockWidth = 5;
                        astcBlockHeight = 5;
                        break;
                    }
                case TextureFormat.ASTC_RGB_6x6: //test pass
                case TextureFormat.ASTC_RGBA_6x6: //test pass
                    {
                        astcBlockWidth = 6;
                        astcBlockHeight = 6;
                        break;
                    }
                case TextureFormat.ASTC_RGB_8x8: //test pass
                case TextureFormat.ASTC_RGBA_8x8: //test pass
                    {
                        astcBlockWidth = 8;
                        astcBlockHeight = 8;
                        break;
                    }
                case TextureFormat.ASTC_RGB_10x10: //test pass
                case TextureFormat.ASTC_RGBA_10x10: //test pass
                    {
                        astcBlockWidth = 10;
                        astcBlockHeight = 10;
                        break;
                    }
                case TextureFormat.ASTC_RGB_12x12: //test pass
                case TextureFormat.ASTC_RGBA_12x12: //test pass
                    {
                        astcBlockWidth = 12;
                        astcBlockHeight = 12;
                        break;
                    }
                case TextureFormat.RG16: //test pass
                    {
                        //转BGRA32
                        var BGRA32 = new byte[image_data_size * 2];
                        for (var i = 0; i < image_data_size; i += 2)
                        {
                            BGRA32[i * 2 + 1] = image_data[i + 1];//G
                            BGRA32[i * 2 + 2] = image_data[i];//R
                            BGRA32[i * 2 + 3] = 255;//A
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
                case TextureFormat.R8: //test pass
                    {
                        //转BGRA32
                        var BGRA32 = new byte[image_data_size * 4];
                        for (var i = 0; i < image_data_size; i++)
                        {
                            BGRA32[i * 4 + 2] = image_data[i];//R
                            BGRA32[i * 4 + 3] = 255;//A
                        }
                        SetBGRA32Info(BGRA32);
                        break;
                    }
            }
        }

        private void SetBGRA32Info(byte[] BGRA32)
        {
            image_data = BGRA32;
            image_data_size = BGRA32.Length;
            dwFlags2 = 0x41;
            dwRGBBitCount = 0x20;
            dwRBitMask = 0xFF0000;
            dwGBitMask = 0xFF00;
            dwBBitMask = 0xFF;
            dwABitMask = -16777216;
        }

        private void SwapBytesForXbox(BuildTarget platform)
        {
            if (platform == BuildTarget.XBOX360) //swap bytes for Xbox confirmed, PS3 not encountered
            {
                for (var i = 0; i < image_data_size / 2; i++)
                {
                    var b0 = image_data[i * 2];
                    image_data[i * 2] = image_data[i * 2 + 1];
                    image_data[i * 2 + 1] = b0;
                }
            }
        }

        public string GetExtensionName()
        {
            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.RGBA4444:
                case TextureFormat.BGRA32:
                case TextureFormat.RG16:
                case TextureFormat.R8:
                    return ".dds";
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGBA8Crunched:
                    return ".crn";
                case TextureFormat.YUY2:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_RGBA_12x12:
                case TextureFormat.ETC_RGB4_3DS:
                case TextureFormat.ETC_RGBA8_3DS:
                    return ".pvr";
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ATC_RGBA8:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                    return ".ktx";
                default:
                    return ".tex";
            }
        }


        public byte[] ConvertToContainer()
        {
            if (image_data == null || image_data.Length == 0)
                return null;
            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGB565:
                case TextureFormat.R16:
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.RGBA4444:
                case TextureFormat.BGRA32:
                case TextureFormat.RG16:
                case TextureFormat.R8:
                    return ConvertToDDS();
                case TextureFormat.YUY2:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_RGBA_12x12:
                case TextureFormat.ETC_RGB4_3DS:
                case TextureFormat.ETC_RGBA8_3DS:
                    return ConvertToPVR();
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ATC_RGBA8:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                    return ConvertToKTX();
                default:
                    return image_data;
            }
        }

        private byte[] ConvertToDDS()
        {
            var imageBuffer = new byte[128 + image_data_size];
            dwMagic.CopyTo(imageBuffer, 0);
            BitConverter.GetBytes(dwFlags).CopyTo(imageBuffer, 8);
            BitConverter.GetBytes(m_Height).CopyTo(imageBuffer, 12);
            BitConverter.GetBytes(m_Width).CopyTo(imageBuffer, 16);
            BitConverter.GetBytes(dwPitchOrLinearSize).CopyTo(imageBuffer, 20);
            BitConverter.GetBytes(dwMipMapCount).CopyTo(imageBuffer, 28);
            BitConverter.GetBytes(dwSize).CopyTo(imageBuffer, 76);
            BitConverter.GetBytes(dwFlags2).CopyTo(imageBuffer, 80);
            BitConverter.GetBytes(dwFourCC).CopyTo(imageBuffer, 84);
            BitConverter.GetBytes(dwRGBBitCount).CopyTo(imageBuffer, 88);
            BitConverter.GetBytes(dwRBitMask).CopyTo(imageBuffer, 92);
            BitConverter.GetBytes(dwGBitMask).CopyTo(imageBuffer, 96);
            BitConverter.GetBytes(dwBBitMask).CopyTo(imageBuffer, 100);
            BitConverter.GetBytes(dwABitMask).CopyTo(imageBuffer, 104);
            BitConverter.GetBytes(dwCaps).CopyTo(imageBuffer, 108);
            BitConverter.GetBytes(dwCaps2).CopyTo(imageBuffer, 112);
            image_data.CopyTo(imageBuffer, 128);
            return imageBuffer;
        }

        private byte[] ConvertToPVR()
        {
            var mstream = new MemoryStream();
            using (var writer = new BinaryWriter(mstream))
            {
                writer.Write(pvrVersion);
                writer.Write(pvrFlags);
                writer.Write(pvrPixelFormat);
                writer.Write(pvrColourSpace);
                writer.Write(pvrChannelType);
                writer.Write(m_Height);
                writer.Write(m_Width);
                writer.Write(pvrDepth);
                writer.Write(pvrNumSurfaces);
                writer.Write(pvrNumFaces);
                writer.Write(dwMipMapCount);
                writer.Write(pvrMetaDataSize);
                writer.Write(image_data);
                return mstream.ToArray();
            }
        }

        private byte[] ConvertToKTX()
        {
            var mstream = new MemoryStream();
            using (var writer = new BinaryWriter(mstream))
            {
                writer.Write(KTXHeader.IDENTIFIER);
                writer.Write(KTXHeader.ENDIANESS_LE);
                writer.Write(glType);
                writer.Write(glTypeSize);
                writer.Write(glFormat);
                writer.Write(glInternalFormat);
                writer.Write(glBaseInternalFormat);
                writer.Write(m_Width);
                writer.Write(m_Height);
                writer.Write(pixelDepth);
                writer.Write(numberOfArrayElements);
                writer.Write(numberOfFaces);
                writer.Write(numberOfMipmapLevels);
                writer.Write(bytesOfKeyValueData);
                writer.Write(image_data_size);
                writer.Write(image_data);
                return mstream.ToArray();
            }
        }

        public Bitmap ConvertToBitmap(bool flip)
        {
            if (image_data == null || image_data.Length == 0)
                return null;
            Bitmap bitmap;
            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.R16:
                case TextureFormat.RGBA4444:
                case TextureFormat.BGRA32:
                case TextureFormat.RG16:
                case TextureFormat.R8:
                    bitmap = BGRA32ToBitmap();
                    break;
                case TextureFormat.RGB565:
                    bitmap = RGB565ToBitmap();
                    break;
                case TextureFormat.YUY2:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC_RGB4_3DS:
                case TextureFormat.ETC_RGBA8_3DS:
                    bitmap = PVRToBitmap(ConvertToPVR());
                    break;
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGB9e5Float:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ATC_RGBA8:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                    bitmap = TextureConverter();
                    break;
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                    bitmap = TexgenPackDecode();
                    break;
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5Crunched:
                    DecompressCRN();
                    bitmap = TextureConverter();
                    break;
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGBA8Crunched:
                    DecompressCRN();
                    bitmap = PVRToBitmap(ConvertToPVR());
                    break;
                case TextureFormat.ASTC_RGB_4x4:
                case TextureFormat.ASTC_RGB_5x5:
                case TextureFormat.ASTC_RGB_6x6:
                case TextureFormat.ASTC_RGB_8x8:
                case TextureFormat.ASTC_RGB_10x10:
                case TextureFormat.ASTC_RGB_12x12:
                case TextureFormat.ASTC_RGBA_4x4:
                case TextureFormat.ASTC_RGBA_5x5:
                case TextureFormat.ASTC_RGBA_6x6:
                case TextureFormat.ASTC_RGBA_8x8:
                case TextureFormat.ASTC_RGBA_10x10:
                case TextureFormat.ASTC_RGBA_12x12:
                    bitmap = DecodeASTC();
                    break;
                default:
                    return null;
            }
            if (bitmap != null && flip)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        private Bitmap BGRA32ToBitmap()
        {
            var hObject = GCHandle.Alloc(image_data, GCHandleType.Pinned);
            var pObject = hObject.AddrOfPinnedObject();
            var bitmap = new Bitmap(m_Width, m_Height, m_Width * 4, PixelFormat.Format32bppArgb, pObject);
            hObject.Free();
            return bitmap;
        }

        private Bitmap RGB565ToBitmap()
        {
            //stride = m_Width * 2 + m_Width * 2 % 4
            //所以m_Width * 2不为4的倍数时，需要在每行补上相应的像素
            byte[] buff;
            var padding = m_Width * 2 % 4;
            var stride = m_Width * 2 + padding;
            if (padding != 0)
            {
                buff = new byte[stride * m_Height];
                for (int i = 0; i < m_Height; i++)
                {
                    Buffer.BlockCopy(image_data, i * m_Width * 2, buff, i * stride, m_Width * 2);
                }
            }
            else
            {
                buff = image_data;
            }
            var gch = GCHandle.Alloc(buff, GCHandleType.Pinned);
            var imagePtr = gch.AddrOfPinnedObject();
            var bitmap = new Bitmap(m_Width, m_Height, stride, PixelFormat.Format16bppRgb565, imagePtr);
            gch.Free();
            return bitmap;
        }

        private Bitmap PVRToBitmap(byte[] pvrData)
        {
            var imageBuff = new byte[m_Width * m_Height * 4];
            var gch = GCHandle.Alloc(imageBuff, GCHandleType.Pinned);
            var imagePtr = gch.AddrOfPinnedObject();
            if (!NativeMethods.DecompressPVR(pvrData, imagePtr))
            {
                gch.Free();
                return null;
            }
            var bitmap = new Bitmap(m_Width, m_Height, m_Width * 4, PixelFormat.Format32bppArgb, imagePtr);
            gch.Free();
            return bitmap;
        }

        private Bitmap TextureConverter()
        {
            var imageBuff = new byte[m_Width * m_Height * 4];
            var gch = GCHandle.Alloc(imageBuff, GCHandleType.Pinned);
            var imagePtr = gch.AddrOfPinnedObject();
            var fixAlpha = glBaseInternalFormat == KTXHeader.GL_RED || glBaseInternalFormat == KTXHeader.GL_RG;
            if (!NativeMethods.Ponvert(image_data, image_data_size, m_Width, m_Height, (int)q_format, fixAlpha, imagePtr))
            {
                gch.Free();
                return null;
            }
            var bitmap = new Bitmap(m_Width, m_Height, m_Width * 4, PixelFormat.Format32bppArgb, imagePtr);
            gch.Free();
            return bitmap;
        }

        private void DecompressCRN()
        {
            IntPtr uncompressedData;
            int uncompressedSize;
            bool result;
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3) //2017.3 and up
                || m_TextureFormat == TextureFormat.ETC_RGB4Crunched
                || m_TextureFormat == TextureFormat.ETC2_RGBA8Crunched)
            {
                result = NativeMethods.DecompressUnityCRN(image_data, image_data_size, out uncompressedData, out uncompressedSize);
            }
            else
            {
                result = NativeMethods.DecompressCRN(image_data, image_data_size, out uncompressedData, out uncompressedSize);
            }

            if (result)
            {
                var uncompressedBytes = new byte[uncompressedSize];
                Marshal.Copy(uncompressedData, uncompressedBytes, 0, uncompressedSize);
                Marshal.FreeHGlobal(uncompressedData);
                image_data = uncompressedBytes;
                image_data_size = uncompressedSize;
            }
        }

        private Bitmap TexgenPackDecode()
        {
            var imageBuff = new byte[m_Width * m_Height * 4];
            var gch = GCHandle.Alloc(imageBuff, GCHandleType.Pinned);
            var imagePtr = gch.AddrOfPinnedObject();
            NativeMethods.TexgenPackDecode(image_data, (int)texturetype, m_Width, m_Height, imagePtr);
            var bitmap = new Bitmap(m_Width, m_Height, m_Width * 4, PixelFormat.Format32bppArgb, imagePtr);
            gch.Free();
            return bitmap;
        }

        private Bitmap DecodeASTC()
        {
            var imageBuff = new byte[m_Width * m_Height * 4];
            var gch = GCHandle.Alloc(imageBuff, GCHandleType.Pinned);
            var imagePtr = gch.AddrOfPinnedObject();
            if (!NativeMethods.DecodeASTC(image_data, m_Width, m_Height, astcBlockWidth, astcBlockHeight, imagePtr))
            {
                gch.Free();
                return null;
            }
            var bitmap = new Bitmap(m_Width, m_Height, m_Width * 4, PixelFormat.Format32bppArgb, imagePtr);
            gch.Free();
            return bitmap;
        }
    }

    internal static class NativeMethods
    {
        [DllImport("PVRTexLibWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DecompressPVR(byte[] data, IntPtr image);

        [DllImport("TextureConverterWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Ponvert(byte[] data, int dataSize, int width, int height, int type, bool fixAlpha, IntPtr image);

        [DllImport("crunch.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DecompressCRN(byte[] data, int dataSize, out IntPtr uncompressedData, out int uncompressedSize);

        [DllImport("crunchunity.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DecompressUnityCRN(byte[] data, int dataSize, out IntPtr uncompressedData, out int uncompressedSize);

        [DllImport("texgenpack.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TexgenPackDecode(byte[] data, int textureType, int width, int height, IntPtr image);

        [DllImport("astc.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool DecodeASTC(byte[] data, int width, int height, int blockwidth, int blockheight, IntPtr image);
    }

    public static class KTXHeader
    {
        public static byte[] IDENTIFIER = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
        public static byte[] ENDIANESS_LE = { 1, 2, 3, 4 };

        // constants for glInternalFormat
        public static int GL_ETC1_RGB8_OES = 0x8D64;

        public static int GL_COMPRESSED_RGB_PVRTC_4BPPV1_IMG = 0x8C00;
        public static int GL_COMPRESSED_RGB_PVRTC_2BPPV1_IMG = 0x8C01;
        public static int GL_COMPRESSED_RGBA_PVRTC_4BPPV1_IMG = 0x8C02;
        public static int GL_COMPRESSED_RGBA_PVRTC_2BPPV1_IMG = 0x8C03;

        public static int GL_ATC_RGB_AMD = 0x8C92;
        public static int GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD = 0x87EE;

        public static int GL_COMPRESSED_RGB8_ETC2 = 0x9274;
        public static int GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276;
        public static int GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278;
        public static int GL_COMPRESSED_R11_EAC = 0x9270;
        public static int GL_COMPRESSED_SIGNED_R11_EAC = 0x9271;
        public static int GL_COMPRESSED_RG11_EAC = 0x9272;
        public static int GL_COMPRESSED_SIGNED_RG11_EAC = 0x9273;

        public static int GL_COMPRESSED_RED_RGTC1 = 0x8DBB;
        public static int GL_COMPRESSED_RG_RGTC2 = 0x8DBD;
        public static int GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8E8F;
        public static int GL_COMPRESSED_RGBA_BPTC_UNORM = 0x8E8C;

        public static int GL_R16F = 0x822D;
        public static int GL_RG16F = 0x822F;
        public static int GL_RGBA16F = 0x881A;
        public static int GL_R32F = 0x822E;
        public static int GL_RG32F = 0x8230;
        public static int GL_RGBA32F = 0x8814;

        // constants for glBaseInternalFormat
        public static int GL_RED = 0x1903;
        public static int GL_RGB = 0x1907;
        public static int GL_RGBA = 0x1908;
        public static int GL_RG = 0x8227;
    }

    //from TextureConverter.h
    public enum QFORMAT
    {
        // General formats
        Q_FORMAT_RGBA_8UI = 1,
        Q_FORMAT_RGBA_8I,
        Q_FORMAT_RGB5_A1UI,
        Q_FORMAT_RGBA_4444,
        Q_FORMAT_RGBA_16UI,
        Q_FORMAT_RGBA_16I,
        Q_FORMAT_RGBA_32UI,
        Q_FORMAT_RGBA_32I,

        Q_FORMAT_PALETTE_8_RGBA_8888,
        Q_FORMAT_PALETTE_8_RGBA_5551,
        Q_FORMAT_PALETTE_8_RGBA_4444,
        Q_FORMAT_PALETTE_4_RGBA_8888,
        Q_FORMAT_PALETTE_4_RGBA_5551,
        Q_FORMAT_PALETTE_4_RGBA_4444,
        Q_FORMAT_PALETTE_1_RGBA_8888,
        Q_FORMAT_PALETTE_8_RGB_888,
        Q_FORMAT_PALETTE_8_RGB_565,
        Q_FORMAT_PALETTE_4_RGB_888,
        Q_FORMAT_PALETTE_4_RGB_565,

        Q_FORMAT_R2_GBA10UI,
        Q_FORMAT_RGB10_A2UI,
        Q_FORMAT_RGB10_A2I,
        Q_FORMAT_RGBA_F,
        Q_FORMAT_RGBA_HF,

        Q_FORMAT_RGB9_E5,   // Last five bits are exponent bits (Read following section in GLES3 spec: "3.8.17 Shared Exponent Texture Color Conversion")
        Q_FORMAT_RGB_8UI,
        Q_FORMAT_RGB_8I,
        Q_FORMAT_RGB_565,
        Q_FORMAT_RGB_16UI,
        Q_FORMAT_RGB_16I,
        Q_FORMAT_RGB_32UI,
        Q_FORMAT_RGB_32I,

        Q_FORMAT_RGB_F,
        Q_FORMAT_RGB_HF,
        Q_FORMAT_RGB_11_11_10_F,

        Q_FORMAT_RG_F,
        Q_FORMAT_RG_HF,
        Q_FORMAT_RG_32UI,
        Q_FORMAT_RG_32I,
        Q_FORMAT_RG_16I,
        Q_FORMAT_RG_16UI,
        Q_FORMAT_RG_8I,
        Q_FORMAT_RG_8UI,
        Q_FORMAT_RG_S88,

        Q_FORMAT_R_32UI,
        Q_FORMAT_R_32I,
        Q_FORMAT_R_F,
        Q_FORMAT_R_16F,
        Q_FORMAT_R_16I,
        Q_FORMAT_R_16UI,
        Q_FORMAT_R_8I,
        Q_FORMAT_R_8UI,

        Q_FORMAT_LUMINANCE_ALPHA_88,
        Q_FORMAT_LUMINANCE_8,
        Q_FORMAT_ALPHA_8,

        Q_FORMAT_LUMINANCE_ALPHA_F,
        Q_FORMAT_LUMINANCE_F,
        Q_FORMAT_ALPHA_F,
        Q_FORMAT_LUMINANCE_ALPHA_HF,
        Q_FORMAT_LUMINANCE_HF,
        Q_FORMAT_ALPHA_HF,
        Q_FORMAT_DEPTH_16,
        Q_FORMAT_DEPTH_24,
        Q_FORMAT_DEPTH_24_STENCIL_8,
        Q_FORMAT_DEPTH_32,

        Q_FORMAT_BGR_565,
        Q_FORMAT_BGRA_8888,
        Q_FORMAT_BGRA_5551,
        Q_FORMAT_BGRX_8888,
        Q_FORMAT_BGRA_4444,
        // Compressed formats
        Q_FORMAT_ATITC_RGBA,
        Q_FORMAT_ATC_RGBA_EXPLICIT_ALPHA = Q_FORMAT_ATITC_RGBA,
        Q_FORMAT_ATITC_RGB,
        Q_FORMAT_ATC_RGB = Q_FORMAT_ATITC_RGB,
        Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA,
        Q_FORMAT_ETC1_RGB8,
        Q_FORMAT_3DC_X,
        Q_FORMAT_3DC_XY,

        Q_FORMAT_ETC2_RGB8,
        Q_FORMAT_ETC2_RGBA8,
        Q_FORMAT_ETC2_RGB8_PUNCHTHROUGH_ALPHA1,
        Q_FORMAT_ETC2_SRGB8,
        Q_FORMAT_ETC2_SRGB8_ALPHA8,
        Q_FORMAT_ETC2_SRGB8_PUNCHTHROUGH_ALPHA1,
        Q_FORMAT_EAC_R_SIGNED,
        Q_FORMAT_EAC_R_UNSIGNED,
        Q_FORMAT_EAC_RG_SIGNED,
        Q_FORMAT_EAC_RG_UNSIGNED,

        Q_FORMAT_S3TC_DXT1_RGB,
        Q_FORMAT_S3TC_DXT1_RGBA,
        Q_FORMAT_S3TC_DXT3_RGBA,
        Q_FORMAT_S3TC_DXT5_RGBA,

        // YUV formats
        Q_FORMAT_AYUV_32,
        Q_FORMAT_I444_24,
        Q_FORMAT_YUYV_16,
        Q_FORMAT_UYVY_16,
        Q_FORMAT_I420_12,
        Q_FORMAT_YV12_12,
        Q_FORMAT_NV21_12,
        Q_FORMAT_NV12_12,

        // ASTC Format
        Q_FORMAT_ASTC_8,
        Q_FORMAT_ASTC_16,
    };

    public enum texgenpack_texturetype
    {
        RGTC1,
        RGTC2,
        BPTC_FLOAT,
        BPTC
    }
}
