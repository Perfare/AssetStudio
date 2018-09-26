using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MovieTexture : Texture
    {
        public byte[] m_MovieData;

        public MovieTexture(AssetPreloadData preloadData) : base(preloadData)
        {
            var m_Loop = reader.ReadBoolean();
            reader.AlignStream(4);
            //PPtr<AudioClip>
            sourceFile.ReadPPtr();
            m_MovieData = reader.ReadBytes(reader.ReadInt32());
        }
    }
}
