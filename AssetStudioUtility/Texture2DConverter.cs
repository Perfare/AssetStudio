using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Texture2DDecoder;

namespace AssetStudio
{
    public class Texture2DConverter
    {
        private int m_Width;
        private int m_Height;
        private TextureFormat m_TextureFormat;
        private int image_data_size;
        private byte[] image_data;
        private int[] version;
        private BuildTarget platform;

        public Texture2DConverter(Texture2D m_Texture2D)
        {
            image_data = m_Texture2D.image_data.GetData();
            image_data_size = image_data.Length;
            m_Width = m_Texture2D.m_Width;
            m_Height = m_Texture2D.m_Height;
            m_TextureFormat = m_Texture2D.m_TextureFormat;
            version = m_Texture2D.version;
            platform = m_Texture2D.platform;
        }

        public Bitmap ConvertToBitmap(bool flip)
        {
            if (image_data == null || image_data.Length == 0)
                return null;
            var buff = DecodeTexture2D();
            if (buff == null)
            {
                return null;
            }
            var bitmap = new Bitmap(m_Width, m_Height, PixelFormat.Format32bppArgb);
            var bmpData = bitmap.LockBits(new Rectangle(0, 0, m_Width, m_Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(buff, 0, bmpData.Scan0, buff.Length);
            bitmap.UnlockBits(bmpData);
            if (flip)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return bitmap;
        }

        public byte[] DecodeTexture2D()
        {
            byte[] bytes = null;
            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8: //test pass
                    bytes = DecodeAlpha8();
                    break;
                case TextureFormat.ARGB4444: //test pass
                    SwapBytesForXbox();
                    bytes = DecodeARGB4444();
                    break;
                case TextureFormat.RGB24: //test pass
                    bytes = DecodeRGB24();
                    break;
                case TextureFormat.RGBA32: //test pass
                    bytes = DecodeRGBA32();
                    break;
                case TextureFormat.ARGB32: //test pass
                    bytes = DecodeARGB32();
                    break;
                case TextureFormat.RGB565: //test pass
                    SwapBytesForXbox();
                    bytes = DecodeRGB565();
                    break;
                case TextureFormat.R16: //test pass
                    bytes = DecodeR16();
                    break;
                case TextureFormat.DXT1: //test pass
                    SwapBytesForXbox();
                    bytes = DecodeDXT1();
                    break;
                case TextureFormat.DXT5: //test pass
                    SwapBytesForXbox();
                    bytes = DecodeDXT5();
                    break;
                case TextureFormat.RGBA4444: //test pass
                    bytes = DecodeRGBA4444();
                    break;
                case TextureFormat.BGRA32: //test pass
                    bytes = DecodeBGRA32();
                    break;
                case TextureFormat.RHalf:
                    bytes = DecodeRHalf();
                    break;
                case TextureFormat.RGHalf:
                    bytes = DecodeRGHalf();
                    break;
                case TextureFormat.RGBAHalf: //test pass
                    bytes = DecodeRGBAHalf();
                    break;
                case TextureFormat.RFloat:
                    bytes = DecodeRFloat();
                    break;
                case TextureFormat.RGFloat:
                    bytes = DecodeRGFloat();
                    break;
                case TextureFormat.RGBAFloat:
                    bytes = DecodeRGBAFloat();
                    break;
                case TextureFormat.YUY2: //test pass
                    bytes = DecodeYUY2();
                    break;
                case TextureFormat.RGB9e5Float: //test pass
                    bytes = DecodeRGB9e5Float();
                    break;
                case TextureFormat.BC4: //test pass
                    bytes = DecodeBC4();
                    break;
                case TextureFormat.BC5: //test pass
                    bytes = DecodeBC5();
                    break;
                case TextureFormat.BC6H: //test pass
                    bytes = DecodeBC6H();
                    break;
                case TextureFormat.BC7: //test pass
                    bytes = DecodeBC7();
                    break;
                case TextureFormat.DXT1Crunched: //test pass
                    if (UnpackCrunch())
                    {
                        bytes = DecodeDXT1();
                    }
                    break;
                case TextureFormat.DXT5Crunched: //test pass
                    if (UnpackCrunch())
                    {
                        bytes = DecodeDXT5();
                    }
                    break;
                case TextureFormat.PVRTC_RGB2: //test pass
                case TextureFormat.PVRTC_RGBA2: //test pass
                    bytes = DecodePVRTC(true);
                    break;
                case TextureFormat.PVRTC_RGB4: //test pass
                case TextureFormat.PVRTC_RGBA4: //test pass
                    bytes = DecodePVRTC(false);
                    break;
                case TextureFormat.ETC_RGB4: //test pass
                case TextureFormat.ETC_RGB4_3DS:
                    bytes = DecodeETC1();
                    break;
                case TextureFormat.ATC_RGB4: //test pass
                    bytes = DecodeATCRGB4();
                    break;
                case TextureFormat.ATC_RGBA8: //test pass
                    bytes = DecodeATCRGBA8();
                    break;
                case TextureFormat.EAC_R: //test pass
                    bytes = DecodeEACR();
                    break;
                case TextureFormat.EAC_R_SIGNED:
                    bytes = DecodeEACRSigned();
                    break;
                case TextureFormat.EAC_RG: //test pass
                    bytes = DecodeEACRG();
                    break;
                case TextureFormat.EAC_RG_SIGNED:
                    bytes = DecodeEACRGSigned();
                    break;
                case TextureFormat.ETC2_RGB: //test pass
                    bytes = DecodeETC2();
                    break;
                case TextureFormat.ETC2_RGBA1: //test pass
                    bytes = DecodeETC2A1();
                    break;
                case TextureFormat.ETC2_RGBA8: //test pass
                case TextureFormat.ETC_RGBA8_3DS:
                    bytes = DecodeETC2A8();
                    break;
                case TextureFormat.ASTC_RGB_4x4: //test pass
                case TextureFormat.ASTC_RGBA_4x4: //test pass
                case TextureFormat.ASTC_HDR_4x4: //test pass
                    bytes = DecodeASTC(4);
                    break;
                case TextureFormat.ASTC_RGB_5x5: //test pass
                case TextureFormat.ASTC_RGBA_5x5: //test pass
                case TextureFormat.ASTC_HDR_5x5: //test pass
                    bytes = DecodeASTC(5);
                    break;
                case TextureFormat.ASTC_RGB_6x6: //test pass
                case TextureFormat.ASTC_RGBA_6x6: //test pass
                case TextureFormat.ASTC_HDR_6x6: //test pass
                    bytes = DecodeASTC(6);
                    break;
                case TextureFormat.ASTC_RGB_8x8: //test pass
                case TextureFormat.ASTC_RGBA_8x8: //test pass
                case TextureFormat.ASTC_HDR_8x8: //test pass
                    bytes = DecodeASTC(8);
                    break;
                case TextureFormat.ASTC_RGB_10x10: //test pass
                case TextureFormat.ASTC_RGBA_10x10: //test pass
                case TextureFormat.ASTC_HDR_10x10: //test pass
                    bytes = DecodeASTC(10);
                    break;
                case TextureFormat.ASTC_RGB_12x12: //test pass
                case TextureFormat.ASTC_RGBA_12x12: //test pass
                case TextureFormat.ASTC_HDR_12x12: //test pass
                    bytes = DecodeASTC(12);
                    break;
                case TextureFormat.RG16: //test pass
                    bytes = DecodeRG16();
                    break;
                case TextureFormat.R8: //test pass
                    bytes = DecodeR8();
                    break;
                case TextureFormat.ETC_RGB4Crunched: //test pass
                    if (UnpackCrunch())
                    {
                        bytes = DecodeETC1();
                    }
                    break;
                case TextureFormat.ETC2_RGBA8Crunched: //test pass
                    if (UnpackCrunch())
                    {
                        bytes = DecodeETC2A8();
                    }
                    break;
            }
            return bytes;
        }

        private void SwapBytesForXbox()
        {
            if (platform == BuildTarget.XBOX360)
            {
                for (var i = 0; i < image_data_size / 2; i++)
                {
                    var b = image_data[i * 2];
                    image_data[i * 2] = image_data[i * 2 + 1];
                    image_data[i * 2 + 1] = b;
                }
            }
        }

        private byte[] DecodeAlpha8()
        {
            var buff = Enumerable.Repeat<byte>(0xFF, m_Width * m_Height * 4).ToArray();
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                buff[i * 4 + 3] = image_data[i];
            }
            return buff;
        }

        private byte[] DecodeARGB4444()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                var pixelNew = new byte[4];
                var pixelOldShort = BitConverter.ToUInt16(image_data, i * 2);
                pixelNew[0] = (byte)(pixelOldShort & 0x000f);
                pixelNew[1] = (byte)((pixelOldShort & 0x00f0) >> 4);
                pixelNew[2] = (byte)((pixelOldShort & 0x0f00) >> 8);
                pixelNew[3] = (byte)((pixelOldShort & 0xf000) >> 12);
                for (var j = 0; j < 4; j++)
                    pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                pixelNew.CopyTo(buff, i * 4);
            }
            return buff;
        }

        private byte[] DecodeRGB24()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                buff[i * 4] = image_data[i * 3 + 2];
                buff[i * 4 + 1] = image_data[i * 3 + 1];
                buff[i * 4 + 2] = image_data[i * 3 + 0];
                buff[i * 4 + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeRGBA32()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = image_data[i + 2];
                buff[i + 1] = image_data[i + 1];
                buff[i + 2] = image_data[i + 0];
                buff[i + 3] = image_data[i + 3];
            }
            return buff;
        }

        private byte[] DecodeARGB32()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = image_data[i + 3];
                buff[i + 1] = image_data[i + 2];
                buff[i + 2] = image_data[i + 1];
                buff[i + 3] = image_data[i + 0];
            }
            return buff;
        }

        private byte[] DecodeRGB565()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                var p = BitConverter.ToUInt16(image_data, i * 2);
                buff[i * 4] = (byte)((p << 3) | (p >> 2 & 7));
                buff[i * 4 + 1] = (byte)((p >> 3 & 0xfc) | (p >> 9 & 3));
                buff[i * 4 + 2] = (byte)((p >> 8 & 0xf8) | (p >> 13));
                buff[i * 4 + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeR16()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                buff[i * 4 + 2] = image_data[i * 2 + 1]; //r
                buff[i * 4 + 3] = 255; //a
            }
            return buff;
        }

        private byte[] DecodeDXT1()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeDXT1(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeDXT5()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeDXT5(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeRGBA4444()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                var pixelNew = new byte[4];
                var pixelOldShort = BitConverter.ToUInt16(image_data, i * 2);
                pixelNew[0] = (byte)((pixelOldShort & 0x00f0) >> 4);
                pixelNew[1] = (byte)((pixelOldShort & 0x0f00) >> 8);
                pixelNew[2] = (byte)((pixelOldShort & 0xf000) >> 12);
                pixelNew[3] = (byte)(pixelOldShort & 0x000f);
                for (var j = 0; j < 4; j++)
                    pixelNew[j] = (byte)((pixelNew[j] << 4) | pixelNew[j]);
                pixelNew.CopyTo(buff, i * 4);
            }
            return buff;
        }

        private byte[] DecodeBGRA32()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = image_data[i];
                buff[i + 1] = image_data[i + 1];
                buff[i + 2] = image_data[i + 2];
                buff[i + 3] = image_data[i + 3];
            }
            return buff;
        }

        private byte[] DecodeRHalf()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = 0;
                buff[i + 1] = 0;
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(image_data, i / 2) * 255f);
                buff[i + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeRGHalf()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = 0;
                buff[i + 1] = (byte)Math.Round(Half.ToHalf(image_data, i + 2) * 255f);
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(image_data, i) * 255f);
                buff[i + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeRGBAHalf()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = (byte)Math.Round(Half.ToHalf(image_data, i * 2 + 4) * 255f);
                buff[i + 1] = (byte)Math.Round(Half.ToHalf(image_data, i * 2 + 2) * 255f);
                buff[i + 2] = (byte)Math.Round(Half.ToHalf(image_data, i * 2) * 255f);
                buff[i + 3] = (byte)Math.Round(Half.ToHalf(image_data, i * 2 + 6) * 255f);
            }
            return buff;
        }

        private byte[] DecodeRFloat()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = 0;
                buff[i + 1] = 0;
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(image_data, i) * 255f);
                buff[i + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeRGFloat()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = 0;
                buff[i + 1] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 2 + 4) * 255f);
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 2) * 255f);
                buff[i + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeRGBAFloat()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                buff[i] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 4 + 8) * 255f);
                buff[i + 1] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 4 + 4) * 255f);
                buff[i + 2] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 4) * 255f);
                buff[i + 3] = (byte)Math.Round(BitConverter.ToSingle(image_data, i * 4 + 12) * 255f);
            }
            return buff;
        }

        private static byte ClampByte(int x)
        {
            return (byte)(byte.MaxValue < x ? byte.MaxValue : (x > byte.MinValue ? x : byte.MinValue));
        }

        private byte[] DecodeYUY2()
        {
            var buff = new byte[m_Width * m_Height * 4];
            int p = 0;
            int o = 0;
            int halfWidth = m_Width / 2;
            for (int j = 0; j < m_Height; j++)
            {
                for (int i = 0; i < halfWidth; ++i)
                {
                    int y0 = image_data[p++];
                    int u0 = image_data[p++];
                    int y1 = image_data[p++];
                    int v0 = image_data[p++];
                    int c = y0 - 16;
                    int d = u0 - 128;
                    int e = v0 - 128;
                    buff[o++] = ClampByte((298 * c + 516 * d + 128) >> 8);            // b
                    buff[o++] = ClampByte((298 * c - 100 * d - 208 * e + 128) >> 8);  // g
                    buff[o++] = ClampByte((298 * c + 409 * e + 128) >> 8);            // r
                    buff[o++] = 255;
                    c = y1 - 16;
                    buff[o++] = ClampByte((298 * c + 516 * d + 128) >> 8);            // b
                    buff[o++] = ClampByte((298 * c - 100 * d - 208 * e + 128) >> 8);  // g
                    buff[o++] = ClampByte((298 * c + 409 * e + 128) >> 8);            // r
                    buff[o++] = 255;
                }
            }
            return buff;
        }

        private byte[] DecodeRGB9e5Float()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < buff.Length; i += 4)
            {
                var n = BitConverter.ToInt32(image_data, i);
                var scale = n >> 27 & 0x1f;
                var scalef = Math.Pow(2, scale - 24);
                var b = n >> 18 & 0x1ff;
                var g = n >> 9 & 0x1ff;
                var r = n & 0x1ff;
                buff[i] = (byte)Math.Round(b * scalef * 255f);
                buff[i + 1] = (byte)Math.Round(g * scalef * 255f);
                buff[i + 2] = (byte)Math.Round(r * scalef * 255f);
                buff[i + 3] = 255;
            }
            return buff;
        }

        private byte[] DecodeBC4()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeBC4(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeBC5()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeBC5(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeBC6H()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeBC6(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeBC7()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeBC7(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodePVRTC(bool is2bpp)
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodePVRTC(image_data, m_Width, m_Height, buff, is2bpp))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeETC1()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeETC1(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeATCRGB4()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeATCRGB4(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeATCRGBA8()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeATCRGBA8(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeEACR()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeEACR(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeEACRSigned()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeEACRSigned(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeEACRG()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeEACRG(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeEACRGSigned()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeEACRGSigned(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeETC2()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeETC2(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeETC2A1()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeETC2A1(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeETC2A8()
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeETC2A8(image_data, m_Width, m_Height, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeASTC(int blocksize)
        {
            var buff = new byte[m_Width * m_Height * 4];
            if (!TextureDecoder.DecodeASTC(image_data, m_Width, m_Height, blocksize, blocksize, buff))
            {
                return null;
            }
            return buff;
        }

        private byte[] DecodeRG16()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i += 2)
            {
                buff[i * 2 + 1] = image_data[i + 1];//G
                buff[i * 2 + 2] = image_data[i];//R
                buff[i * 2 + 3] = 255;//A
            }
            return buff;
        }

        private byte[] DecodeR8()
        {
            var buff = new byte[m_Width * m_Height * 4];
            for (var i = 0; i < m_Width * m_Height; i++)
            {
                buff[i * 4 + 2] = image_data[i];//R
                buff[i * 4 + 3] = 255;//A
            }
            return buff;
        }

        private bool UnpackCrunch()
        {
            byte[] result;
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3) //2017.3 and up
                || m_TextureFormat == TextureFormat.ETC_RGB4Crunched
                || m_TextureFormat == TextureFormat.ETC2_RGBA8Crunched)
            {
                result = TextureDecoder.UnpackUnityCrunch(image_data);
            }
            else
            {
                result = TextureDecoder.UnpackCrunch(image_data);
            }
            if (result != null)
            {
                image_data = result;
                image_data_size = result.Length;
                return true;
            }
            return false;
        }
    }
}
