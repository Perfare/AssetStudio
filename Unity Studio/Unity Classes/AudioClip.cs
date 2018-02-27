using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    class AudioClip
    {
        public string m_Name;
        public int m_Format;
        public AudioType m_Type;
        public bool m_3D;
        public bool m_UseHardware;

        //Unity 5
        public int m_LoadType;
        public int m_Channels;
        public int m_Frequency;
        public int m_BitsPerSample;
        public float m_Length;
        public bool m_IsTrackerFormat;
        public int m_SubsoundIndex;
        public bool m_PreloadAudioData;
        public bool m_LoadInBackground;
        public bool m_Legacy3D;
        public AudioCompressionFormat m_CompressionFormat;

        public string m_Source;
        public long m_Offset;
        public long m_Size;
        public byte[] m_AudioData;

        public bool version5;

        public AudioClip(AssetPreloadData preloadData, bool readSwitch)
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
            version5 = sourceFile.version[0] >= 5;
            if (sourceFile.version[0] < 5)
            {

                m_Format = a_Stream.ReadInt32(); //channels?
                m_Type = (AudioType)a_Stream.ReadInt32();
                m_3D = a_Stream.ReadBoolean();
                m_UseHardware = a_Stream.ReadBoolean();
                a_Stream.Position += 2; //4 byte alignment

                if (sourceFile.version[0] >= 4 || (sourceFile.version[0] == 3 && sourceFile.version[1] >= 2)) //3.2.0 to 5
                {
                    int m_Stream = a_Stream.ReadInt32();
                    m_Size = a_Stream.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
                    //TODO: Need more test
                    if (preloadData.Size + preloadData.Offset - a_Stream.Position != tsize)
                    {
                        m_Offset = a_Stream.ReadInt32();
                        m_Source = sourceFile.filePath + ".resS";
                    }
                }
                else
                {
                    m_Size = a_Stream.ReadInt32();
                }
            }
            else
            {
                m_LoadType = a_Stream.ReadInt32(); //Decompress on load, Compressed in memory, Streaming
                m_Channels = a_Stream.ReadInt32();
                m_Frequency = a_Stream.ReadInt32();
                m_BitsPerSample = a_Stream.ReadInt32();
                m_Length = a_Stream.ReadSingle();
                m_IsTrackerFormat = a_Stream.ReadBoolean();
                a_Stream.Position += 3;
                m_SubsoundIndex = a_Stream.ReadInt32();
                m_PreloadAudioData = a_Stream.ReadBoolean();
                m_LoadInBackground = a_Stream.ReadBoolean();
                m_Legacy3D = a_Stream.ReadBoolean();
                a_Stream.Position += 1;
                m_3D = m_Legacy3D;

                m_Source = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                m_Offset = a_Stream.ReadInt64();
                m_Size = a_Stream.ReadInt64();
                m_CompressionFormat = (AudioCompressionFormat)a_Stream.ReadInt32();
            }

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(m_Source))
                {
                    var resourceFileName = Path.GetFileName(m_Source);
                    var resourceFilePath = Path.GetDirectoryName(sourceFile.filePath) + "\\" + resourceFileName;
                    if (!File.Exists(resourceFilePath))
                    {
                        var findFiles = Directory.GetFiles(Path.GetDirectoryName(sourceFile.filePath), resourceFileName, SearchOption.AllDirectories);
                        if (findFiles.Length > 0) { resourceFilePath = findFiles[0]; }
                    }
                    if (File.Exists(resourceFilePath))
                    {
                        using (var reader = new BinaryReader(File.OpenRead(resourceFilePath)))
                        {
                            reader.BaseStream.Position = m_Offset;
                            m_AudioData = reader.ReadBytes((int)m_Size);
                        }
                    }
                    else
                    {
                        if (UnityStudio.assetsfileandstream.TryGetValue(resourceFileName, out var reader))
                        {
                            reader.Position = m_Offset;
                            m_AudioData = reader.ReadBytes((int)m_Size);
                        }
                        else
                        {
                            MessageBox.Show($"can't find the resource file {resourceFileName}");
                        }
                    }
                }
                else
                {
                    if (m_Size > 0)
                        m_AudioData = a_Stream.ReadBytes((int)m_Size);
                }
            }
            else
            {
                preloadData.InfoText = "Compression format: ";

                if (sourceFile.version[0] < 5)
                {
                    switch (m_Type)
                    {
                        case AudioType.ACC:
                            preloadData.extension = ".m4a";
                            preloadData.InfoText += "Acc";
                            break;
                        case AudioType.AIFF:
                            preloadData.extension = ".aif";
                            preloadData.InfoText += "AIFF";
                            break;
                        case AudioType.IT:
                            preloadData.extension = ".it";
                            preloadData.InfoText += "Impulse tracker";
                            break;
                        case AudioType.MOD:
                            preloadData.extension = ".mod";
                            preloadData.InfoText += "Protracker / Fasttracker MOD";
                            break;
                        case AudioType.MPEG:
                            preloadData.extension = ".mp3";
                            preloadData.InfoText += "MP2/MP3 MPEG";
                            break;
                        case AudioType.OGGVORBIS:
                            preloadData.extension = ".ogg";
                            preloadData.InfoText += "Ogg vorbis";
                            break;
                        case AudioType.S3M:
                            preloadData.extension = ".s3m";
                            preloadData.InfoText += "ScreamTracker 3";
                            break;
                        case AudioType.WAV:
                            preloadData.extension = ".wav";
                            preloadData.InfoText += "Microsoft WAV";
                            break;
                        case AudioType.XM:
                            preloadData.extension = ".xm";
                            preloadData.InfoText += "FastTracker 2 XM";
                            break;
                        case AudioType.XMA:
                            preloadData.extension = ".wav";
                            preloadData.InfoText += "Xbox360 XMA";
                            break;
                        case AudioType.VAG:
                            preloadData.extension = ".vag";
                            preloadData.InfoText += "PlayStation Portable ADPCM";
                            break;
                        case AudioType.AUDIOQUEUE:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "iPhone";
                            break;
                    }

                }
                else
                {
                    switch (m_CompressionFormat)
                    {
                        case AudioCompressionFormat.PCM:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "PCM";
                            break;
                        case AudioCompressionFormat.Vorbis:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "Vorbis";
                            break;
                        case AudioCompressionFormat.ADPCM:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "ADPCM";
                            break;
                        case AudioCompressionFormat.MP3:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "MP3";
                            break;
                        case AudioCompressionFormat.VAG:
                            preloadData.extension = ".vag";
                            preloadData.InfoText += "PlayStation Portable ADPCM";
                            break;
                        case AudioCompressionFormat.HEVAG:
                            preloadData.extension = ".vag";
                            preloadData.InfoText += "PSVita ADPCM";
                            break;
                        case AudioCompressionFormat.XMA:
                            preloadData.extension = ".wav";
                            preloadData.InfoText += "Xbox360 XMA";
                            break;
                        case AudioCompressionFormat.AAC:
                            preloadData.extension = ".m4a";
                            preloadData.InfoText += "Acc";
                            break;
                        case AudioCompressionFormat.GCADPCM:
                            preloadData.extension = ".fsb";
                            preloadData.InfoText += "Nintendo 3DS/Wii DSP";
                            break;
                        case AudioCompressionFormat.ATRAC9:
                            preloadData.extension = ".at9";
                            preloadData.InfoText += "PSVita ATRAC9";
                            break;
                    }
                }

                if (preloadData.extension == null)
                {
                    preloadData.extension = ".AudioClip";
                    preloadData.InfoText += "Unknown";
                }

                preloadData.InfoText += "\n3D: " + m_3D;

                preloadData.Text = m_Name;
                if (m_Source != null)
                    preloadData.fullSize = preloadData.Size + (int)m_Size;
            }
        }

        public bool IsFMODSupport
        {
            get
            {
                if (!version5)
                {
                    switch (m_Type)
                    {
                        case AudioType.AIFF:
                        case AudioType.IT:
                        case AudioType.MOD:
                        case AudioType.S3M:
                        case AudioType.XM:
                        case AudioType.XMA:
                        case AudioType.VAG:
                        case AudioType.AUDIOQUEUE:
                            return true;
                        default:
                            return false;
                    }
                }
                else
                {
                    switch (m_CompressionFormat)
                    {
                        case AudioCompressionFormat.PCM:
                        case AudioCompressionFormat.Vorbis:
                        case AudioCompressionFormat.ADPCM:
                        case AudioCompressionFormat.MP3:
                        case AudioCompressionFormat.VAG:
                        case AudioCompressionFormat.HEVAG:
                        case AudioCompressionFormat.XMA:
                        case AudioCompressionFormat.GCADPCM:
                        case AudioCompressionFormat.ATRAC9:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }
    }
}
