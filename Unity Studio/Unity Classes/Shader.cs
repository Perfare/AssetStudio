using System.Text;

namespace Unity_Studio
{
    class Shader
    {
        public string m_Name;
        public byte[] m_Script;

        public Shader(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;
            preloadData.extension = ".txt";

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            if (readSwitch)
            {
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 5 || sourceFile.version[0] > 5)
                {
                    string str;
                    if ((str = preloadData.ViewStruct()) != null)
                    {
                        m_Script = Encoding.UTF8.GetBytes(str);
                    }
                    else
                        m_Script = Encoding.UTF8.GetBytes("Serialized Shader can't be read");
                }
                else
                {
                    m_Script = a_Stream.ReadBytes(a_Stream.ReadInt32());
                }
            }
            else
            {
                if (m_Name != "") { preloadData.Text = m_Name; }
                else { preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID; }
                preloadData.SubItems.AddRange(new[] { preloadData.TypeString, preloadData.Size.ToString() });
            }
        }
    }
}
