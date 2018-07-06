using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Lz4;

namespace AssetStudio
{
    class Shader
    {
        public string m_Name;
        public byte[] m_Script;

        public Shader(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();

            m_Name = reader.ReadAlignedString();

            if (readSwitch)
            {
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 5 || sourceFile.version[0] > 5)//5.5.0 and up
                {
                    var str = (string)ShaderResource.ResourceManager.GetObject($"Shader{sourceFile.version[0]}{sourceFile.version[1]}");
                    if (str == null)
                    {
                        str = preloadData.GetClassString();
                        if (str == null)
                            m_Script = Encoding.UTF8.GetBytes("Serialized Shader can't be read");
                        else
                            m_Script = Encoding.UTF8.GetBytes(str);
                    }
                    else
                    {
                        reader.Position = preloadData.Offset;
                        var sb = new StringBuilder();
                        var members = new JavaScriptSerializer().Deserialize<List<ClassMember>>(str);
                        ClassStructHelper.ReadClassString(sb, members, reader);
                        m_Script = Encoding.UTF8.GetBytes(sb.ToString());
                    }
                }
                else
                {
                    m_Script = reader.ReadBytes(reader.ReadInt32());
                    if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 3) //5.3 - 5.4
                    {
                        reader.AlignStream(4);
                        reader.ReadAlignedString();//m_PathName
                        var decompressedSize = reader.ReadUInt32();
                        var m_SubProgramBlob = reader.ReadBytes(reader.ReadInt32());
                        var decompressedBytes = new byte[decompressedSize];
                        using (var decoder = new Lz4DecoderStream(new MemoryStream(m_SubProgramBlob)))
                        {
                            decoder.Read(decompressedBytes, 0, (int)decompressedSize);
                        }
                        m_Script = m_Script.Concat(decompressedBytes.ToArray()).ToArray();
                    }
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
