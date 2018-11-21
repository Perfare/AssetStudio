using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class VideoClip : NamedObject
    {
        public Lazy<byte[]> m_VideoData;
        public string m_OriginalPath;
        public string m_Source;
        public ulong m_Size;

        public VideoClip(ObjectReader reader) : base(reader)
        {
            m_OriginalPath = reader.ReadAlignedString();
            var m_ProxyWidth = reader.ReadUInt32();
            var m_ProxyHeight = reader.ReadUInt32();
            var Width = reader.ReadUInt32();
            var Height = reader.ReadUInt32();
            if (version[0] >= 2017)//2017.x and up
            {
                var m_PixelAspecRatioNum = reader.ReadUInt32();
                var m_PixelAspecRatioDen = reader.ReadUInt32();
            }
            var m_FrameRate = reader.ReadDouble();
            var m_FrameCount = reader.ReadUInt64();
            var m_Format = reader.ReadInt32();
            //m_AudioChannelCount
            var size = reader.ReadInt32();
            reader.Position += size * 2;
            reader.AlignStream(4);
            //m_AudioSampleRate
            size = reader.ReadInt32();
            reader.Position += size * 4;
            //m_AudioLanguage
            size = reader.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                reader.ReadAlignedString();
            }
            //StreamedResource m_ExternalResources
            m_Source = reader.ReadAlignedString();
            var m_Offset = reader.ReadUInt64();
            m_Size = reader.ReadUInt64();
            var m_HasSplitAlpha = reader.ReadBoolean();

            ResourceReader resourceReader;
            if (!string.IsNullOrEmpty(m_Source))
            {
                resourceReader = new ResourceReader(m_Source, assetsFile, (long)m_Offset, (int)m_Size);
            }
            else
            {
                resourceReader = new ResourceReader(reader, reader.BaseStream.Position, (int)m_Size);
            }
            m_VideoData = new Lazy<byte[]>(resourceReader.GetData);
        }
    }
}
