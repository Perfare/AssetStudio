using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class MonoScript
    {
        public string m_Name;
        public string m_ClassName;
        public string m_Namespace = string.Empty;
        public string m_AssemblyName;

        public MonoScript(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();
            var version = sourceFile.version;

            m_Name = reader.ReadAlignedString();
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 4))
            {
                var m_ExecutionOrder = reader.ReadAlignedString();
            }
            if (version[0] < 5)
            {
                var m_PropertiesHash = reader.ReadUInt32();
            }
            else
            {
                var m_PropertiesHash = reader.ReadBytes(16);
            }
            if (version[0] < 3)
            {
                var m_PathName = reader.ReadAlignedString();
            }
            m_ClassName = reader.ReadAlignedString();
            if (version[0] >= 3)
            {
                m_Namespace = reader.ReadAlignedString();
            }
            m_AssemblyName = reader.ReadAlignedString();
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2))
            {
                var m_IsEditorScript = reader.ReadBoolean();
            }
        }
    }
}
