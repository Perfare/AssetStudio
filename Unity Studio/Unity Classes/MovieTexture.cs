using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class MovieTexture
    {
        public string m_Name;
        public byte[] m_MovieData;

        public MovieTexture(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.Reader;

            m_Name = reader.ReadAlignedString(reader.ReadInt32());
            if (readSwitch)
            {
                var m_Loop = reader.ReadBoolean();
                reader.AlignStream(4);
                //PPtr<AudioClip>
                sourceFile.ReadPPtr();
                var size = reader.ReadInt32();
                m_MovieData = reader.ReadBytes(size);
                var m_ColorSpace = reader.ReadInt32();
            }
            else
            {
                preloadData.extension = ".ogv";
                preloadData.Text = m_Name;
            }
        }
    }
}
