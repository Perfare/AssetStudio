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
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            if (readSwitch)
            {
                var m_Loop = a_Stream.ReadBoolean();
                a_Stream.AlignStream(4);
                //PPtr<AudioClip>
                sourceFile.ReadPPtr();
                var size = a_Stream.ReadInt32();
                m_MovieData = a_Stream.ReadBytes(size);
                var m_ColorSpace = a_Stream.ReadInt32();
            }
            else
            {
                preloadData.extension = ".ogv";
                preloadData.Text = m_Name;
            }
        }
    }
}
