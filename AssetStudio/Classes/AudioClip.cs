using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class AudioClip : NamedObject
    {
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

        public AudioClip(AssetPreloadData preloadData, bool readData) : base(preloadData)
        {
            if (version[0] < 5)
            {
                m_Format = reader.ReadInt32();
                m_Type = (AudioType)reader.ReadInt32();
                m_3D = reader.ReadBoolean();
                m_UseHardware = reader.ReadBoolean();
                reader.AlignStream(4);

                if (version[0] >= 4 || (version[0] == 3 && version[1] >= 2)) //3.2.0 to 5
                {
                    int m_Stream = reader.ReadInt32();
                    m_Size = reader.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
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
                m_LoadType = reader.ReadInt32();
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

            if (readData)
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
