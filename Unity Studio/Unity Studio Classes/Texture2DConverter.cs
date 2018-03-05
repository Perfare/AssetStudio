using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Unity_Studio
{
    partial class Texture2D
    {
        [DllImport("PVRTexLibWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DecompressPVR(byte[] buffer, IntPtr bmp, int len);

        [DllImport("TextureConverterWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Ponvert(byte[] buffer, IntPtr bmp, int nWidth, int nHeight, int len, int type, int bmpsize, bool fixAlpha);

        [DllImport("crunch.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DecompressCRN(byte[] pSrc_file_data, int src_file_size, out IntPtr uncompressedData, out int uncompressedSize);

        [DllImport("crunchunity.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool DecompressUnityCRN(byte[] pSrc_file_data, int src_file_size, out IntPtr uncompressedData, out int uncompressedSize);

        [DllImport("texgenpack.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void texgenpackdecode(int texturetype, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha);


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
                    bitmap = Texgenpack();
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
                    Array.Copy(image_data, i * m_Width * 2, buff, i * stride, m_Width * 2);
                }
            }
            else
            {
                buff = image_data;
            }
            var hObject = GCHandle.Alloc(buff, GCHandleType.Pinned);
            var pObject = hObject.AddrOfPinnedObject();
            var bitmap = new Bitmap(m_Width, m_Height, stride, PixelFormat.Format16bppRgb565, pObject);
            hObject.Free();
            return bitmap;
        }

        private Bitmap PVRToBitmap(byte[] pvrdata)
        {
            var bitmap = new Bitmap(m_Width, m_Height);
            var rect = new Rectangle(0, 0, m_Width, m_Height);
            var bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var len = Math.Abs(bmd.Stride) * bmd.Height;
            if (!DecompressPVR(pvrdata, bmd.Scan0, len))
            {
                bitmap.UnlockBits(bmd);
                bitmap.Dispose();
                return null;
            }
            bitmap.UnlockBits(bmd);
            return bitmap;
        }

        private Bitmap TextureConverter()
        {
            var bitmap = new Bitmap(m_Width, m_Height);
            var rect = new Rectangle(0, 0, m_Width, m_Height);
            var bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var len = Math.Abs(bmd.Stride) * bmd.Height;
            var fixAlpha = glBaseInternalFormat == KTXHeader.GL_RED || glBaseInternalFormat == KTXHeader.GL_RG;
            if (!Ponvert(image_data, bmd.Scan0, m_Width, m_Height, image_data_size, (int)q_format, len, fixAlpha))
            {
                bitmap.UnlockBits(bmd);
                bitmap.Dispose();
                return null;
            }
            bitmap.UnlockBits(bmd);
            return bitmap;
        }

        private void DecompressCRN()
        {
            IntPtr uncompressedData;
            int uncompressedSize;
            bool result;
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                result = DecompressUnityCRN(image_data, image_data_size, out uncompressedData, out uncompressedSize);
            }
            else
            {
                result = DecompressCRN(image_data, image_data_size, out uncompressedData, out uncompressedSize);
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

        private Bitmap Texgenpack()
        {
            var bitmap = new Bitmap(m_Width, m_Height);
            var rect = new Rectangle(0, 0, m_Width, m_Height);
            var bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var fixAlpha = glBaseInternalFormat == KTXHeader.GL_RED || glBaseInternalFormat == KTXHeader.GL_RG;
            texgenpackdecode((int)texturetype, image_data, m_Width, m_Height, bmd.Scan0, fixAlpha);
            bitmap.UnlockBits(bmd);
            return bitmap;
        }
    }

    public static class KTXHeader
    {
        public static byte[] IDENTIFIER = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
        public static byte[] ENDIANESS_LE = { 1, 2, 3, 4 };
        public static byte[] ENDIANESS_BE = { 4, 3, 2, 1 };

        // constants for glInternalFormat
        public static int GL_ETC1_RGB8_OES = 0x8D64;

        public static int GL_COMPRESSED_RGB_PVRTC_4BPPV1_IMG = 0x8C00;
        public static int GL_COMPRESSED_RGB_PVRTC_2BPPV1_IMG = 0x8C01;
        public static int GL_COMPRESSED_RGBA_PVRTC_4BPPV1_IMG = 0x8C02;
        public static int GL_COMPRESSED_RGBA_PVRTC_2BPPV1_IMG = 0x8C03;

        public static int GL_ATC_RGB_AMD = 0x8C92;
        public static int GL_ATC_RGBA_EXPLICIT_ALPHA_AMD = 0x8C93;
        public static int GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD = 0x87EE;

        public static int GL_COMPRESSED_RGB8_ETC2 = 0x9274;
        public static int GL_COMPRESSED_SRGB8_ETC2 = 0x9275;
        public static int GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276;
        public static int GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9277;
        public static int GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278;
        public static int GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC = 0x9279;
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
        public static int GL_GREEN = 0x1904;
        public static int GL_BLUE = 0x1905;
        public static int GL_ALPHA = 0x1906;
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
