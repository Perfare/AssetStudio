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
            var reader = preloadData.Reader;

            m_Name = reader.ReadAlignedString(reader.ReadInt32());
            var m_OriginalPath = reader.ReadAlignedString(reader.ReadInt32());
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
                reader.ReadAlignedString(reader.ReadInt32());
            }
            //StreamedResource m_ExternalResources
            var m_Source = reader.ReadAlignedString(reader.ReadInt32());
            var m_Offset = reader.ReadUInt64();
            var m_Size = reader.ReadUInt64();
            var m_HasSplitAlpha = reader.ReadBoolean();

            if (readSwitch)
            {
                if (!string.IsNullOrEmpty(m_Source))
                {
                    var resourceFileName = Path.GetFileName(m_Source);
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
                            resourceReader.BaseStream.Position = (long)m_Offset;
                            m_VideoData = resourceReader.ReadBytes((int)m_Size);
                        }
                    }
                    else
                    {
                        if (UnityStudio.resourceFileReaders.TryGetValue(resourceFileName.ToUpper(), out var resourceReader))
                        {
                            resourceReader.Position = (long)m_Offset;
                            m_VideoData = resourceReader.ReadBytes((int)m_Size);
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
