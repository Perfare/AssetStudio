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
        public FMODSoundType m_Type;
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
        public long m_Offset; //ulong
        public long m_Size; //ulong
        public ResourceReader m_AudioData;

        public AudioClip(ObjectReader reader) : base(reader)
        {
            if (version[0] < 5)
            {
                m_Format = reader.ReadInt32();
                m_Type = (FMODSoundType)reader.ReadInt32();
                m_3D = reader.ReadBoolean();
                m_UseHardware = reader.ReadBoolean();
                reader.AlignStream();

                if (version[0] >= 4 || (version[0] == 3 && version[1] >= 2)) //3.2.0 to 5
                {
                    int m_Stream = reader.ReadInt32();
                    m_Size = reader.ReadInt32();
                    var tsize = m_Size % 4 != 0 ? m_Size + 4 - m_Size % 4 : m_Size;
                    if (reader.byteSize + reader.byteStart - reader.Position != tsize)
                    {
                        m_Offset = reader.ReadUInt32();
                        m_Source = assetsFile.fullName + ".resS";
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
                reader.AlignStream();
                m_SubsoundIndex = reader.ReadInt32();
                m_PreloadAudioData = reader.ReadBoolean();
                m_LoadInBackground = reader.ReadBoolean();
                m_Legacy3D = reader.ReadBoolean();
                reader.AlignStream();

                //StreamedResource m_Resource
                m_Source = reader.ReadAlignedString();
                m_Offset = reader.ReadInt64();
                m_Size = reader.ReadInt64();
                m_CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
            }

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(m_Source))
            {
                resourceReader = new ResourceReader(m_Source, assetsFile, m_Offset, m_Size);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, m_Size);
            }
            m_AudioData = resourceReader;
        }
    }

    public enum FMODSoundType
    {
        UNKNOWN = 0,
        ACC = 1,
        AIFF = 2,
        ASF = 3,
        AT3 = 4,
        CDDA = 5,
        DLS = 6,
        FLAC = 7,
        FSB = 8,
        GCADPCM = 9,
        IT = 10,
        MIDI = 11,
        MOD = 12,
        MPEG = 13,
        OGGVORBIS = 14,
        PLAYLIST = 15,
        RAW = 16,
        S3M = 17,
        SF2 = 18,
        USER = 19,
        WAV = 20,
        XM = 21,
        XMA = 22,
        VAG = 23,
        AUDIOQUEUE = 24,
        XWMA = 25,
        BCWAV = 26,
        AT9 = 27,
        VORBIS = 28,
        MEDIA_FOUNDATION = 29
    }

    public enum AudioCompressionFormat
    {
        PCM = 0,
        Vorbis = 1,
        ADPCM = 2,
        MP3 = 3,
        PSMVAG = 4,
        HEVAG = 5,
        XMA = 6,
        AAC = 7,
        GCADPCM = 8,
        ATRAC9 = 9
    }
}
