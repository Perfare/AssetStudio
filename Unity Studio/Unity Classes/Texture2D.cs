using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Tao.DevIl;

namespace Unity_Studio
{
    class Texture2D
    {
        public string m_Name;
        public int m_Width;
        public int m_Height;
        public int m_CompleteImageSize;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap = false;
        public bool m_IsReadable;
        public bool m_ReadAllowed;
        public int m_ImageCount;
        public int m_TextureDimension;
        //m_TextureSettings
        public int m_FilterMode;
        public int m_Aniso;
        public float m_MipBias;
        public int m_WrapMode;
        public int m_LightmapFormat;
        public int m_ColorSpace;
        //image dataa
        public int image_data_size;
        public byte[] image_data;
        //m_StreamData
        public uint offset;
        public uint size;
        public string path;

        //DDS Start
        public byte[] dwMagic = { 0x44, 0x44, 0x53, 0x20, 0x7c };
        public int dwFlags = 0x1 + 0x2 + 0x4 + 0x1000;
        //public int dwHeight; m_Height
        //public int dwWidth; m_Width
        public int dwPitchOrLinearSize = 0x0;
        public int dwMipMapCount = 0x1;
        public int dwSize = 0x20;
        public int dwFlags2;
        public int dwFourCC = 0x0;
        public int dwRGBBitCount;
        public int dwRBitMask;
        public int dwGBitMask;
        public int dwBBitMask;
        public int dwABitMask;
        public int dwCaps = 0x1000;
        public int dwCaps2 = 0x0;
        //DDS End
        //PVR Start
        public int pvrVersion = 0x03525650;
        public int pvrFlags = 0x0;
        public long pvrPixelFormat;
        public int pvrColourSpace = 0x0;
        public int pvrChannelType = 0x0;
        //public int pvrHeight; m_Height
        //public int pvrWidth; m_Width
        public int pvrDepth = 0x1;
        public int pvrNumSurfaces = 0x1; //For texture arrays
        public int pvrNumFaces = 0x1; //For cube maps
        //public int pvrMIPMapCount; dwMipMapCount
        public int pvrMetaDataSize = 0x0;
        //PVR End
        //KTX Start
        public int glType = 0;
        public int glTypeSize = 1;
        public int glFormat = 0;
        public int glInternalFormat;
        public int glBaseInternalFormat;
        //public int pixelWidth; m_Width
        //public int pixelHeight; m_Height
        public int pixelDepth = 0;
        public int numberOfArrayElements = 0;
        public int numberOfFaces = 1;
        public int numberOfMipmapLevels = 1;
        public int bytesOfKeyValueData = 0;
        //KTX End
        //TextureConverter
        public int q_format;

        [DllImport("PVRTexLibWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressPVR(byte[] buffer, IntPtr bmp, int len);

        [DllImport("TextureConverterWrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Ponvert(byte[] buffer, IntPtr bmp, int nWidth, int nHeight, int len, int type);

        public Texture2D(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            m_Width = a_Stream.ReadInt32();
            m_Height = a_Stream.ReadInt32();
            m_CompleteImageSize = a_Stream.ReadInt32();
            m_TextureFormat = (TextureFormat)a_Stream.ReadInt32();

            if (sourceFile.version[0] < 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] < 2))
            { m_MipMap = a_Stream.ReadBoolean(); }
            else
            {
                dwFlags += 0x20000;
                dwMipMapCount = a_Stream.ReadInt32();//is this with or without main image?
                dwCaps += 0x400008;
            }

            m_IsReadable = a_Stream.ReadBoolean(); //2.6.0 and up
            m_ReadAllowed = a_Stream.ReadBoolean(); //3.0.0 and up
            a_Stream.AlignStream(4);

            m_ImageCount = a_Stream.ReadInt32();
            m_TextureDimension = a_Stream.ReadInt32();
            //m_TextureSettings
            m_FilterMode = a_Stream.ReadInt32();
            m_Aniso = a_Stream.ReadInt32();
            m_MipBias = a_Stream.ReadSingle();
            m_WrapMode = a_Stream.ReadInt32();

            if (sourceFile.version[0] >= 3)
            {
                m_LightmapFormat = a_Stream.ReadInt32();
                if (sourceFile.version[0] >= 4 || sourceFile.version[1] >= 5) { m_ColorSpace = a_Stream.ReadInt32(); } //3.5.0 and up
            }

            image_data_size = a_Stream.ReadInt32();

            if (m_MipMap)
            {
                dwFlags += 0x20000;
                dwMipMapCount = Convert.ToInt32(Math.Log(Math.Max(m_Width, m_Height)) / Math.Log(2));
                dwCaps += 0x400008;
            }

            if (image_data_size == 0 && ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 3) || sourceFile.version[0] > 5))//5.3.0 and up
            {
                offset = a_Stream.ReadUInt32();
                size = a_Stream.ReadUInt32();
                image_data_size = (int)size;
                path = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            }

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.Combine(Path.GetDirectoryName(sourceFile.filePath), path.Replace("archive:/", ""));
                    if (File.Exists(path) ||
                    File.Exists(path = Path.Combine(Path.GetDirectoryName(sourceFile.filePath), Path.GetFileName(path))))
                    {
                        image_data = new byte[image_data_size];
                        BinaryReader reader = new BinaryReader(File.OpenRead(path));
                        reader.BaseStream.Position = offset;
                        reader.Read(image_data, 0, image_data_size);
                        reader.Close();
                    }
                    else
                    {
                        EndianStream estream = null;
                        if (UnityStudioForm.assetsfileandstream.TryGetValue(Path.GetFileName(path), out estream))
                        {
                            estream.Position = offset;
                            image_data = estream.ReadBytes(image_data_size);
                        }
                    }
                }
                else
                {
                    image_data = new byte[image_data_size];
                    a_Stream.Read(image_data, 0, image_data_size);
                }

                switch (m_TextureFormat)
                {
                    case TextureFormat.Alpha8: //test pass
                        {
                            /*dwFlags2 = 0x2;
                            dwRGBBitCount = 0x8;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0xFF; *///透明通道丢失?
                            //转ARGB32
                            var bytes = Enumerable.Repeat<byte>(0xFF, image_data_size * 4).ToArray();
                            for (int i = 0; i < image_data_size; i++)
                            {
                                bytes[i * 4] = image_data[i];
                            }
                            image_data = bytes;
                            image_data_size = image_data_size * 4;
                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF00;
                            dwGBitMask = 0xFF0000;
                            dwBBitMask = -16777216;
                            dwABitMask = 0xFF;
                            break;
                        }
                    case TextureFormat.ARGB4444: //test pass
                        {
                            if (sourceFile.platform == 11) //swap bytes for Xbox confirmed, PS3 not encountered
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF00;
                            dwGBitMask = 0xF0;
                            dwBBitMask = 0xF;
                            dwABitMask = 0xF000;
                            break;
                        }
                    case TextureFormat.RGB24: //test pass
                        {
                            dwFlags2 = 0x40;
                            dwRGBBitCount = 0x18;
                            dwRBitMask = 0xFF;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF0000;
                            dwABitMask = 0x0;
                            break;
                        }
                    case TextureFormat.RGBA32: //test pass
                        {
                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF0000;
                            dwABitMask = -16777216;
                            break;
                        }
                    case TextureFormat.ARGB32://test pass
                        {
                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF00;
                            dwGBitMask = 0xFF0000;
                            dwBBitMask = -16777216;
                            dwABitMask = 0xFF;
                            break;
                        }
                    case TextureFormat.RGB565: //test pass
                        {
                            if (sourceFile.platform == 11)
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            dwFlags2 = 0x40;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF800;
                            dwGBitMask = 0x7E0;
                            dwBBitMask = 0x1F;
                            dwABitMask = 0x0;
                            break;
                        }
                    case TextureFormat.R16:
                        {
                            //转BGRA32
                            var BGRA32 = new byte[image_data.Length * 2];
                            for (var i = 0; i < image_data.Length; i += 2)
                            {
                                float f = Half.ToHalf(image_data, i);
                                BGRA32[i * 2 + 2] = (byte)Math.Ceiling(f * 255);//R
                                BGRA32[i * 2 + 3] = 255;//A
                            }
                            image_data = BGRA32;
                            image_data_size *= 2;
                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x20;
                            dwRBitMask = 0xFF0000;
                            dwGBitMask = 0xFF00;
                            dwBBitMask = 0xFF;
                            dwABitMask = -16777216;
                            break;
                        }
                    case TextureFormat.DXT1: //test pass
                        {
                            if (sourceFile.platform == 11) //X360 only, PS3 not
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
                            dwFlags2 = 0x4;
                            dwFourCC = 0x31545844;
                            dwRGBBitCount = 0x0;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0x0;
                            break;
                        }
                    case TextureFormat.DXT5: //test pass
                        {
                            if (sourceFile.platform == 11) //X360, PS3 not
                            {
                                for (int i = 0; i < (image_data_size / 2); i++)
                                {
                                    byte b0 = image_data[i * 2];
                                    image_data[i * 2] = image_data[i * 2 + 1];
                                    image_data[i * 2 + 1] = b0;
                                }
                            }

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
                            dwFlags2 = 0x4;
                            dwFourCC = 0x35545844;
                            dwRGBBitCount = 0x0;
                            dwRBitMask = 0x0;
                            dwGBitMask = 0x0;
                            dwBBitMask = 0x0;
                            dwABitMask = 0x0;
                            break;
                        }
                    case TextureFormat.RGBA4444: //test pass
                        {
                            dwFlags2 = 0x41;
                            dwRGBBitCount = 0x10;
                            dwRBitMask = 0xF000;
                            dwGBitMask = 0xF00;
                            dwBBitMask = 0xF0;
                            dwABitMask = 0xF;
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
                    case TextureFormat.RHalf://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_R_16F;
                            glInternalFormat = KTXHeader.GL_R16F;
                            glBaseInternalFormat = KTXHeader.GL_RED;
                            break;
                        }
                    case TextureFormat.RGHalf://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_RG_HF;
                            glInternalFormat = KTXHeader.GL_RG16F;
                            glBaseInternalFormat = KTXHeader.GL_RG;
                            break;
                        }
                    case TextureFormat.RGBAHalf://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_RGBA_HF;
                            glInternalFormat = KTXHeader.GL_RGBA16F;
                            glBaseInternalFormat = KTXHeader.GL_RGBA;
                            break;
                        }
                    case TextureFormat.RFloat://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_R_F;
                            glInternalFormat = KTXHeader.GL_R32F;
                            glBaseInternalFormat = KTXHeader.GL_RED;
                            break;
                        }
                    case TextureFormat.RGFloat://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_RG_F;
                            glInternalFormat = KTXHeader.GL_RG32F;
                            glBaseInternalFormat = KTXHeader.GL_RG;
                            break;
                        }
                    case TextureFormat.RGBAFloat://Not sure
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_RGBA_F;
                            glInternalFormat = KTXHeader.GL_RGBA32F;
                            glBaseInternalFormat = KTXHeader.GL_RGBA;
                            break;
                        }
                    case TextureFormat.YUY2://Not sure
                        {
                            pvrPixelFormat = 17;
                            q_format = (int)QFORMAT.Q_FORMAT_YUYV_16;
                            break;
                        }
                    case TextureFormat.BC4:
                        {
                            glInternalFormat = KTXHeader.GL_COMPRESSED_RED_RGTC1;
                            glBaseInternalFormat = KTXHeader.GL_RED;
                            break;
                        }
                    case TextureFormat.BC5:
                        {
                            glInternalFormat = KTXHeader.GL_COMPRESSED_RG_RGTC2;
                            glBaseInternalFormat = KTXHeader.GL_RG;
                            break;
                        }
                    case TextureFormat.BC6H:
                        {
                            glInternalFormat = KTXHeader.GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT;
                            glBaseInternalFormat = KTXHeader.GL_RGB;
                            break;
                        }
                    case TextureFormat.BC7:
                        {
                            glInternalFormat = KTXHeader.GL_COMPRESSED_RGBA_BPTC_UNORM;
                            glBaseInternalFormat = KTXHeader.GL_RGBA;
                            break;
                        }
                    case TextureFormat.DXT1Crunched: //DXT1 Crunched
                    case TextureFormat.DXT5Crunched: //DXT1 Crunched
                        break;
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
                    case TextureFormat.ETC_RGB4: //test pass
                        {
                            pvrPixelFormat = 6;
                            glInternalFormat = KTXHeader.GL_ETC1_RGB8_OES;
                            glBaseInternalFormat = KTXHeader.GL_RGB;
                            break;
                        }
                    case TextureFormat.ATC_RGB4: //test pass
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_ATITC_RGB;
                            glInternalFormat = KTXHeader.GL_ATC_RGB_AMD;
                            glBaseInternalFormat = KTXHeader.GL_RGB;
                            break;
                        }
                    case TextureFormat.ATC_RGBA8: //test pass
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA;
                            glInternalFormat = KTXHeader.GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD;
                            glBaseInternalFormat = KTXHeader.GL_RGBA;
                            break;
                        }
                    case TextureFormat.EAC_R:
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_EAC_R_UNSIGNED;
                            glInternalFormat = KTXHeader.GL_COMPRESSED_R11_EAC;
                            glBaseInternalFormat = KTXHeader.GL_RED;
                            break;
                        }
                    case TextureFormat.EAC_R_SIGNED:
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_EAC_R_SIGNED;
                            glInternalFormat = KTXHeader.GL_COMPRESSED_SIGNED_R11_EAC;
                            glBaseInternalFormat = KTXHeader.GL_RED;
                            break;
                        }
                    case TextureFormat.EAC_RG:
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_EAC_RG_UNSIGNED;
                            glInternalFormat = KTXHeader.GL_COMPRESSED_RG11_EAC;
                            glBaseInternalFormat = KTXHeader.GL_RG;
                            break;
                        }
                    case TextureFormat.EAC_RG_SIGNED:
                        {
                            q_format = (int)QFORMAT.Q_FORMAT_EAC_RG_SIGNED;
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
                            pvrPixelFormat = 27;
                            break;
                        }
                    case TextureFormat.ASTC_RGB_5x5: //test pass
                    case TextureFormat.ASTC_RGBA_5x5: //test pass
                        {
                            pvrPixelFormat = 29;
                            break;
                        }
                    case TextureFormat.ASTC_RGB_6x6: //test pass
                    case TextureFormat.ASTC_RGBA_6x6: //test pass
                        {
                            pvrPixelFormat = 31;
                            break;
                        }
                    case TextureFormat.ASTC_RGB_8x8: //test pass
                    case TextureFormat.ASTC_RGBA_8x8: //test pass
                        {
                            pvrPixelFormat = 34;
                            break;
                        }
                    case TextureFormat.ASTC_RGB_10x10: //test pass
                    case TextureFormat.ASTC_RGBA_10x10: //test pass
                        {
                            pvrPixelFormat = 38;
                            break;
                        }
                    case TextureFormat.ASTC_RGB_12x12: //test pass
                    case TextureFormat.ASTC_RGBA_12x12: //test pass
                        {
                            pvrPixelFormat = 40;
                            break;
                        }
                    case TextureFormat.ETC_RGB4_3DS:
                    case TextureFormat.ETC_RGBA8_3DS:
                        break;
                }
            }
            else
            {
                preloadData.InfoText = "Width: " + m_Width.ToString() + "\nHeight: " + m_Height.ToString() + "\nFormat: ";

                string type = m_TextureFormat.ToString();
                preloadData.InfoText += type;

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
                        preloadData.extension = ".dds"; break;
                    case TextureFormat.DXT1Crunched:
                    case TextureFormat.DXT5Crunched:
                        preloadData.extension = ".crn"; break;
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
                        preloadData.extension = ".pvr"; break;
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
                        preloadData.extension = ".ktx"; break;
                    default:
                        preloadData.extension = "_" + type + ".tex"; break;
                }

                switch (m_FilterMode)
                {
                    case 0: preloadData.InfoText += "\nFilter Mode: Point "; break;
                    case 1: preloadData.InfoText += "\nFilter Mode: Bilinear "; break;
                    case 2: preloadData.InfoText += "\nFilter Mode: Trilinear "; break;

                }

                preloadData.InfoText += "\nAnisotropic level: " + m_Aniso.ToString() + "\nMip map bias: " + m_MipBias.ToString();

                switch (m_WrapMode)
                {
                    case 0: preloadData.InfoText += "\nWrap mode: Repeat"; break;
                    case 1: preloadData.InfoText += "\nWrap mode: Clamp"; break;
                }

                if (m_Name != "") { preloadData.Text = m_Name; }
                else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                preloadData.SubItems.AddRange(new string[] { preloadData.TypeString, preloadData.Size.ToString() });
            }
        }

        public byte[] ConvertToContainer()
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
            Bitmap bitmap = null;
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
                    bitmap = DDSToBitmap(ConvertToDDS());
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
                    bitmap = PVRToBitmap(ConvertToPVR());
                    break;
                case TextureFormat.RHalf:
                case TextureFormat.RGHalf:
                case TextureFormat.RGBAHalf:
                case TextureFormat.RFloat:
                case TextureFormat.RGFloat:
                case TextureFormat.RGBAFloat:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ATC_RGBA8:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                    bitmap = TextureConverter();
                    break;
            }
            if (bitmap != null && flip)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        private static Bitmap DDSToBitmap(byte[] DDSData)
        {
            // Create a DevIL image "name" (which is actually a number)
            int img_name;
            Il.ilGenImages(1, out img_name);
            Il.ilBindImage(img_name);

            // Load the DDS file into the bound DevIL image
            Il.ilLoadL(Il.IL_DDS, DDSData, DDSData.Length);

            // Set a few size variables that will simplify later code

            var ImgWidth = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            var ImgHeight = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            var rect = new Rectangle(0, 0, ImgWidth, ImgHeight);

            // Convert the DevIL image to a pixel byte array to copy into Bitmap
            Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);

            // Create a Bitmap to copy the image into, and prepare it to get data
            var bmp = new Bitmap(ImgWidth, ImgHeight);
            var bmd = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Copy the pixel byte array from the DevIL image to the Bitmap
            Il.ilCopyPixels(0, 0, 0,
              Il.ilGetInteger(Il.IL_IMAGE_WIDTH),
              Il.ilGetInteger(Il.IL_IMAGE_HEIGHT),
              1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE,
              bmd.Scan0);

            // Clean up and return Bitmap
            Il.ilDeleteImages(1, ref img_name);
            bmp.UnlockBits(bmd);
            return bmp;
        }

        private Bitmap PVRToBitmap(byte[] pvrdata)
        {
            var bitmap = new Bitmap(m_Width, m_Height);
            var rect = new Rectangle(0, 0, m_Width, m_Height);
            var bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            var len = Math.Abs(bmd.Stride) * bmd.Height;
            DecompressPVR(pvrdata, bmd.Scan0, len);
            bitmap.UnlockBits(bmd);
            return bitmap;
        }

        private Bitmap TextureConverter()
        {
            var bitmap = new Bitmap(m_Width, m_Height);
            var rect = new Rectangle(0, 0, m_Width, m_Height);
            var bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Ponvert(image_data, bmd.Scan0, m_Width, m_Height, image_data_size, q_format);
            bitmap.UnlockBits(bmd);
            //if (glBaseInternalFormat == KTXHeader.GL_RED || glBaseInternalFormat == KTXHeader.GL_RG)
            //    FixAlpha(bitmap);
            return bitmap;
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
        ETC_RGBA8_3DS
    }
}

public class KTXHeader
{
    public static byte[] IDENTIFIER = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
    public static byte[] ENDIANESS_LE = new byte[] { 1, 2, 3, 4 };
    public static byte[] ENDIANESS_BE = new byte[] { 4, 3, 2, 1 };

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