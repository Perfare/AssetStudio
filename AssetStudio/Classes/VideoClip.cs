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
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                var m_PixelAspecRatioNum = reader.ReadUInt32();
                var m_PixelAspecRatioDen = reader.ReadUInt32();
            }
            var m_FrameRate = reader.ReadDouble();
            var m_FrameCount = reader.ReadUInt64();
            var m_Format = reader.ReadInt32();
            var m_AudioChannelCount = reader.ReadUInt16Array();
            reader.AlignStream();
            var m_AudioSampleRate = reader.ReadUInt32Array();
            var m_AudioLanguage = reader.ReadStringArray();
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
