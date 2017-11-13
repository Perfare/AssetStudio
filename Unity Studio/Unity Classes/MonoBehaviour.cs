using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class MonoBehaviour
    {
        public string serializedText;

        public MonoBehaviour(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            var m_GameObject = sourceFile.ReadPPtr();
            var m_Enabled = a_Stream.ReadByte();
            a_Stream.AlignStream(4);
            var m_Script = sourceFile.ReadPPtr();
            var m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            if (readSwitch)
            {
                if ((serializedText = preloadData.ViewStruct()) == null)
                {
                    var str = "PPtr<GameObject> m_GameObject\r\n";
                    str += "\tint m_FileID = " + m_GameObject.m_FileID + "\r\n";
                    str += "\tint64 m_PathID = " + m_GameObject.m_PathID + "\r\n";
                    str += "UInt8 m_Enabled = " + m_Enabled + "\r\n";
                    str += "PPtr<MonoScript> m_Script\r\n";
                    str += "\tint m_FileID = " + m_Script.m_FileID + "\r\n";
                    str += "\tint64 m_PathID = " + m_Script.m_PathID + "\r\n";
                    str += "string m_Name = \"" + m_Name + "\"\r\n";
                    serializedText = str;
                }
            }
            else
            {
                preloadData.extension = ".txt";
                preloadData.Text = m_Name;
            }
        }
    }
}
