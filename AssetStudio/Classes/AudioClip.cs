using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class AudioClip
    {
        public string m_Name;
        public int m_Format;
        public AudioType m_Type;
        public bool m_3D;
        public bool m_UseHardware;

        //version 5
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
            var reader = preloadData.InitReader();

            m_Name = reader.ReadAlignedString();
            version5 = sourceFile.version[0] >= 5;
            if (sourceFile.version[0] < 5)
            {

                m_Format = reader.ReadInt32(); //channels?
                m_Type = (AudioType)reader.ReadInt32();
                m_3D = reader.ReadBoolean();
                m_UseHardware = reader.ReadBoolean();
                reader.AlignStream(4);

                if (sourceFile.version[0] >= 4 || (sourceFile.version[0] == 3 && sourceFile.version[1] >= 2)) //3.2.0 to 5
                {
                    int m_Stream = reader.ReadInt32();
                    m_Size = reader.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
                    //TODO: Need more test
                    if (preloadData.Size + preloadData.Offset - reader.Position != tsize)
                    {
                        m_Offset = reader.ReadInt32();
                        m_Source = sourceFile.filePath + ".resS";
                    }
                }
                else
                {
                    m_Size = reader.ReadInt32();
                }
            }
            else
            {
                m_LoadType = reader.ReadInt32(); //Decompress on load, Compressed in memory, Streaming
                m_Channels = reader.ReadInt32();
                m_Frequency = reader.ReadInt32();
                m_BitsPerSample = reader.ReadInt32();
                m_Length = reader.ReadSingle();
                m_IsTrackerFormat = reader.ReadBoolean();
                reader.AlignStream(4);
                m_SubsoundIndex = reader.ReadInt32();
                m_PreloadAudioData = reader.ReadBoolean();
                m_LoadInBackground = reader.ReadBoolean();
                m_Legacy3D = reader.ReadBoolean();
                reader.AlignStream(4);
                m_3D = m_Legacy3D;

                m_Source = reader.ReadAlignedString();
                m_Offset = reader.ReadInt64();
                m_Size = reader.ReadInt64();
                m_CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
            }

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(m_Source))
                {
                    m_AudioData = ResourcesHelper.GetData(m_Source, sourceFile.filePath, m_Offset, (int)m_Size);
                }
                else
                {
                    if (m_Size > 0)
                        m_AudioData = reader.ReadBytes((int)m_Size);
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
                            preloadData.InfoText += "AAC";
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

    public enum AudioType
    {
        UNKNOWN,
        ACC,
        AIFF,
        IT = 10,
        MOD = 12,
        MPEG,
        OGGVORBIS,
        S3M = 17,
        WAV = 20,
        XM,
        XMA,
        VAG,
        AUDIOQUEUE
    }

    public enum AudioCompressionFormat
    {
        PCM,
        Vorbis,
        ADPCM,
        MP3,
        VAG,
        HEVAG,
        XMA,
        AAC,
        GCADPCM,
        ATRAC9
    }
}
