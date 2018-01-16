using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    class VideoClip
    {
        public string m_Name;
        public byte[] m_VideoData;

        public VideoClip(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            var m_OriginalPath = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            var m_ProxyWidth = a_Stream.ReadUInt32();
            var m_ProxyHeight = a_Stream.ReadUInt32();
            var Width = a_Stream.ReadUInt32();
            var Height = a_Stream.ReadUInt32();
            if (sourceFile.version[0] >= 2017)//2017.x and up
            {
                var m_PixelAspecRatioNum = a_Stream.ReadUInt32();
                var m_PixelAspecRatioDen = a_Stream.ReadUInt32();
            }
            var m_FrameRate = a_Stream.ReadDouble();
            var m_FrameCount = a_Stream.ReadUInt64();
            var m_Format = a_Stream.ReadInt32();
            //m_AudioChannelCount
            var size = a_Stream.ReadInt32();
            a_Stream.Position += size * 2;
            a_Stream.AlignStream(4);
            //m_AudioSampleRate
            size = a_Stream.ReadInt32();
            a_Stream.Position += size * 4;
            //m_AudioLanguage
            size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            }
            //StreamedResource m_ExternalResources
            var m_Source = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            if (m_Source != "")
                m_Source = Path.Combine(Path.GetDirectoryName(sourceFile.filePath), m_Source.Replace("archive:/", ""));
            var m_Offset = a_Stream.ReadUInt64();
            var m_Size = a_Stream.ReadUInt64();
            var m_HasSplitAlpha = a_Stream.ReadBoolean();

            if (readSwitch)
            {
                if (string.IsNullOrEmpty(m_Source))
                {
                    if (m_Size > 0)
                        m_VideoData = a_Stream.ReadBytes((int)m_Size);
                }
                else if (File.Exists(m_Source) || File.Exists(m_Source = Path.Combine(Path.GetDirectoryName(sourceFile.filePath), Path.GetFileName(m_Source))))
                {
                    using (var reader = new BinaryReader(File.OpenRead(m_Source)))
                    {
                        reader.BaseStream.Position = (long)m_Offset;
                        m_VideoData = reader.ReadBytes((int)m_Size);
                    }
                }
                else
                {
                    if (UnityStudio.assetsfileandstream.TryGetValue(Path.GetFileName(m_Source), out var reader))
                    {
                        reader.Position = (long)m_Offset;
                        m_VideoData = reader.ReadBytes((int)m_Size);
                    }
                    else
                    {
                        MessageBox.Show($"can't find the resource file {Path.GetFileName(m_Source)}");
                    }
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
