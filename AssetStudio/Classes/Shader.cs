using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Lz4;

namespace AssetStudio
{
    public sealed class Shader : NamedObject
    {
        public byte[] m_Script;

        public Shader(AssetPreloadData preloadData) : base(preloadData)
        {
            if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 5 || sourceFile.version[0] > 5) //5.5.0 and up
            {
                var str = preloadData.Dump();
                m_Script = Encoding.UTF8.GetBytes(str ?? "Serialized Shader can't be read");
            }
            else
            {
                m_Script = reader.ReadBytes(reader.ReadInt32());
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 3) //5.3 - 5.4
                {
                    reader.AlignStream(4);
                    var m_PathName = reader.ReadAlignedString();
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
    }
}
