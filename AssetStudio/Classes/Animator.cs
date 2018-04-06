using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class Animator
    {
        public PPtr m_GameObject;
        public PPtr m_Avatar;
        public PPtr m_Controller;

        public Animator(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();
            reader.Position = preloadData.Offset;

            m_GameObject = sourceFile.ReadPPtr();
            var m_Enabled = reader.ReadByte();
            reader.AlignStream(4);
            m_Avatar = sourceFile.ReadPPtr();
            m_Controller = sourceFile.ReadPPtr();
        }
    }
}
