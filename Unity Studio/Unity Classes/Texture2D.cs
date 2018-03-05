using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Unity_Studio
{
    partial class Texture2D
    {
        public string m_Name;
        public int m_Width;
        public int m_Height;
        public int m_CompleteImageSize;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap;
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

        private int[] version;

        public Texture2D(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.Reader;
            version = sourceFile.version;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = reader.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = reader.ReadAlignedString(reader.ReadInt32());
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3))//2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                reader.AlignStream(4);
            }
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            m_CompleteImageSize = reader.ReadInt32();
            m_TextureFormat = (TextureFormat)reader.ReadInt32();

            if (version[0] < 5 || (version[0] == 5 && version[1] < 2))
            { m_MipMap = reader.ReadBoolean(); }
            else
            {
                dwFlags += 0x20000;
                dwMipMapCount = reader.ReadInt32();//is this with or without main image?
                dwCaps += 0x400008;
            }

            m_IsReadable = reader.ReadBoolean(); //2.6.0 and up
            m_ReadAllowed = reader.ReadBoolean(); //3.0.0 - 5.4
            reader.AlignStream(4);

            m_ImageCount = reader.ReadInt32();
            m_TextureDimension = reader.ReadInt32();
            //m_TextureSettings
            m_FilterMode = reader.ReadInt32();
            m_Aniso = reader.ReadInt32();
            m_MipBias = reader.ReadSingle();
            m_WrapMode = reader.ReadInt32();
            if (version[0] >= 2017)//2017.x and up
            {
                int m_WrapV = reader.ReadInt32();
                int m_WrapW = reader.ReadInt32();
            }
            if (version[0] >= 3)
            {
                m_LightmapFormat = reader.ReadInt32();
                if (version[0] >= 4 || version[1] >= 5) { m_ColorSpace = reader.ReadInt32(); } //3.5.0 and up
            }

            image_data_size = reader.ReadInt32();

            if (m_MipMap)
            {
                dwFlags += 0x20000;
                dwMipMapCount = Convert.ToInt32(Math.Log(Math.Max(m_Width, m_Height)) / Math.Log(2));
                dwCaps += 0x400008;
            }

            if (image_data_size == 0 && ((version[0] == 5 && version[1] >= 3) || version[0] > 5))//5.3.0 and up
            {
                offset = reader.ReadUInt32();
                size = reader.ReadUInt32();
                image_data_size = (int)size;
                path = reader.ReadAlignedString(reader.ReadInt32());
            }

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var resourceFileName = Path.GetFileName(path);
                    var resourceFilePath = Path.GetDirectoryName(sourceFile.filePath) + "\\" + resourceFileName;
                    if (!File.Exists(resourceFilePath))
                    {
                        var findFiles = Directory.GetFiles(Path.GetDirectoryName(sourceFile.filePath), resourceFileName, SearchOption.AllDirectories);
                        if (findFiles.Length > 0)
                        {
                            resourceFilePath = findFiles[0];
                        }
                    }
                    if (File.Exists(resourceFilePath))
                    {
                        using (var resourceReader = new BinaryReader(File.OpenRead(resourceFilePath)))
                        {
                            resourceReader.BaseStream.Position = offset;
                            image_data = resourceReader.ReadBytes(image_data_size);
                        }
                    }
                    else
                    {
                        if (UnityStudio.resourceFileReaders.TryGetValue(resourceFileName.ToUpper(), out var resourceReader))
                        {
                            resourceReader.Position = offset;
                            image_data = resourceReader.ReadBytes(image_data_size);
                        }
                        else
                        {
                            MessageBox.Show($"can't find the resource file {resourceFileName}");
                        }
                    }
                }
                else
                {
                    image_data = reader.ReadBytes(image_data_size);
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
                            SwapBytesForXbox(sourceFile.platform);

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
                            SwapBytesForXbox(sourceFile.platform);

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
                            SwapBytesForXbox(sourceFile.platform);

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
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
                            SwapBytesForXbox(sourceFile.platform);

                            if (m_MipMap) { dwPitchOrLinearSize = m_Height * m_Width / 2; }
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
                    case TextureFormat.RGB9e5Float:
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
                    case TextureFormat.ETC_RGB4Crunched:
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
                    case TextureFormat.ETC2_RGBA8Crunched:
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
            else
            {
                preloadData.InfoText = $"Width: {m_Width}\nHeight: {m_Height}\nFormat: ";

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
                    case TextureFormat.RG16:
                    case TextureFormat.R8:
                        preloadData.extension = ".dds"; break;
                    case TextureFormat.DXT1Crunched:
                    case TextureFormat.DXT5Crunched:
                    case TextureFormat.ETC_RGB4Crunched:
                    case TextureFormat.ETC2_RGBA8Crunched:
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
                    case TextureFormat.ETC_RGB4_3DS:
                    case TextureFormat.ETC_RGBA8_3DS:
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
                        preloadData.extension = ".tex"; break;
                }

                switch (m_FilterMode)
                {
                    case 0: preloadData.InfoText += "\nFilter Mode: Point "; break;
                    case 1: preloadData.InfoText += "\nFilter Mode: Bilinear "; break;
                    case 2: preloadData.InfoText += "\nFilter Mode: Trilinear "; break;

                }

                preloadData.InfoText += $"\nAnisotropic level: {m_Aniso}\nMip map bias: {m_MipBias}";

                switch (m_WrapMode)
                {
                    case 0: preloadData.InfoText += "\nWrap mode: Repeat"; break;
                    case 1: preloadData.InfoText += "\nWrap mode: Clamp"; break;
                }

                preloadData.Text = m_Name;
                if (!string.IsNullOrEmpty(path))
                    preloadData.fullSize = preloadData.Size + (int)size;
            }
        }

        private void SwapBytesForXbox(int platform)
        {
            if (platform == 11) //swap bytes for Xbox confirmed, PS3 not encountered
            {
                for (var i = 0; i < image_data_size / 2; i++)
                {
                    var b0 = image_data[i * 2];
                    image_data[i * 2] = image_data[i * 2 + 1];
                    image_data[i * 2 + 1] = b0;
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
    }
}