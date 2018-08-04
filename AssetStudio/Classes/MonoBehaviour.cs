using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class MonoBehaviour
    {
        public PPtr m_GameObject;
        public byte m_Enabled;
        public PPtr m_Script;
        public string m_Name;

        public MonoBehaviour(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();

            m_GameObject = sourceFile.ReadPPtr();
            m_Enabled = reader.ReadByte();
            reader.AlignStream(4);
            m_Script = sourceFile.ReadPPtr();
            m_Name = reader.ReadAlignedString();
        }
    }
}
