using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class MonoScript : NamedObject
    {
        public string m_ClassName;
        public string m_Namespace;
        public string m_AssemblyName;

        public MonoScript(ObjectReader reader) : base(reader)
        {
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 4)) //3.4 and up
            {
                var m_ExecutionOrder = reader.ReadInt32();
            }
            if (version[0] < 5) //5.0 down
            {
                var m_PropertiesHash = reader.ReadUInt32();
            }
            else
            {
                var m_PropertiesHash = reader.ReadBytes(16);
            }
            if (version[0] < 3) //3.0 down
            {
                var m_PathName = reader.ReadAlignedString();
            }
            m_ClassName = reader.ReadAlignedString();
            if (version[0] >= 3) //3.0 and up
            {
                m_Namespace = reader.ReadAlignedString();
            }
            m_AssemblyName = reader.ReadAlignedString();
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                var m_IsEditorScript = reader.ReadBoolean();
            }
        }
    }
}
