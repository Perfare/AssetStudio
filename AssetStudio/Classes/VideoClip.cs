using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class VideoClip
    {
        public string m_Name;
        public byte[] m_VideoData;

        public VideoClip(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();

            m_Name = reader.ReadAlignedString();
            var m_OriginalPath = reader.ReadAlignedString();
            var m_ProxyWidth = reader.ReadUInt32();
            var m_ProxyHeight = reader.ReadUInt32();
            var Width = reader.ReadUInt32();
            var Height = reader.ReadUInt32();
            if (sourceFile.version[0] >= 2017)//2017.x and up
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
            var m_Source = reader.ReadAlignedString();
            var m_Offset = reader.ReadUInt64();
            var m_Size = reader.ReadUInt64();
            var m_HasSplitAlpha = reader.ReadBoolean();

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(m_Source))
                {
                    m_VideoData = ResourcesHelper.GetData(m_Source, sourceFile.filePath, (long)m_Offset, (int)m_Size);
                }
                else
                {
                    if (m_Size > 0)
                        m_VideoData = reader.ReadBytes((int)m_Size);
                }
            }
            else
            {
                preloadData.extension = Path.GetExtension(m_OriginalPath);
                preloadData.Text = m_Name;
                preloadData.fullSize = preloadData.Size + (int)m_Size;
            }
        }
    }
}
