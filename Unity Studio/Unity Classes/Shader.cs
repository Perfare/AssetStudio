using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Lz4;

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

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            if (readSwitch)
            {
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 5 || sourceFile.version[0] > 5)//5.5.0 and up
                {
                    var str = (string)ShaderResource.ResourceManager.GetObject($"Shader{sourceFile.version[0]}{sourceFile.version[1]}");
                    if (str == null)
                    {
                        str = preloadData.ViewStruct();
                        if (str == null)
                            m_Script = Encoding.UTF8.GetBytes("Serialized Shader can't be read");
                        else
                            m_Script = Encoding.UTF8.GetBytes(str);
                    }
                    else
                    {
                        a_Stream.Position = preloadData.Offset;
                        var sb = new StringBuilder();
                        var members = new JavaScriptSerializer().Deserialize<List<ClassMember>>(str);
                        ClassStructHelper.ReadClassStruct(sb, members, a_Stream);
                        m_Script = Encoding.UTF8.GetBytes(sb.ToString());
                        //m_Script = ReadSerializedShader(members, a_Stream);
                    }
                }
                else
                {
                    m_Script = a_Stream.ReadBytes(a_Stream.ReadInt32());
                    if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 3) //5.3 - 5.4
                    {
                        a_Stream.AlignStream(4);
                        a_Stream.ReadAlignedString(a_Stream.ReadInt32());//m_PathName
                        var decompressedSize = a_Stream.ReadUInt32();
                        var m_SubProgramBlob = a_Stream.ReadBytes(a_Stream.ReadInt32());
                        var decompressedBytes = new byte[decompressedSize];
                        using (var mstream = new MemoryStream(m_SubProgramBlob))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(decompressedBytes, 0, (int)decompressedSize);
                            decoder.Dispose();
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

        /*private static byte[] ReadSerializedShader(List<ClassMember> members, EndianBinaryReader a_Stream)
        {
            var offsets = new List<uint>();
            var compressedLengths = new List<uint>();
            var decompressedLengths = new List<uint>();
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var level = member.Level;
                var varTypeStr = member.Type;
                if (member.Name == "offsets")
                {
                    var offsets_size = a_Stream.ReadInt32();
                    for (int j = 0; j < offsets_size; j++)
                    {
                        offsets.Add(a_Stream.ReadUInt32());
                    }
                    var compressedLengths_size = a_Stream.ReadInt32();
                    for (int j = 0; j < compressedLengths_size; j++)
                    {
                        compressedLengths.Add(a_Stream.ReadUInt32());
                    }
                    var decompressedLengths_size = a_Stream.ReadInt32();
                    for (int j = 0; j < decompressedLengths_size; j++)
                    {
                        decompressedLengths.Add(a_Stream.ReadUInt32());
                    }
                    var compressedBlob = a_Stream.ReadBytes(a_Stream.ReadInt32());
                    var decompressedStream = new MemoryStream();
                    for (int j = 0; j < offsets.Count; j++)
                    {
                        var compressedBytes = new byte[compressedLengths[j]];
                        Array.Copy(compressedBlob, offsets[j], compressedBytes, 0, compressedLengths[j]);
                        var decompressedBytes = new byte[decompressedLengths[j]];
                        using (var mstream = new MemoryStream(compressedBytes))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(decompressedBytes, 0, (int)decompressedLengths[j]);
                            decoder.Dispose();
                        }
                        decompressedStream.Write(decompressedBytes, 0, decompressedBytes.Length);
                    }
                    var decompressedBlob = decompressedStream.ToArray();
                    return decompressedBlob;
                }
                var align = (member.Flag & 0x4000) != 0;
                if (member.alignBefore)
                    a_Stream.AlignStream(4);
                if (varTypeStr == "SInt8")//sbyte
                {
                    a_Stream.ReadSByte();
                }
                else if (varTypeStr == "UInt8")//byte
                {
                    a_Stream.ReadByte();
                }
                else if (varTypeStr == "short" || varTypeStr == "SInt16")//Int16
                {
                    a_Stream.ReadInt16();
                }
                else if (varTypeStr == "UInt16" || varTypeStr == "unsigned short")//UInt16
                {
                    a_Stream.ReadUInt16();
                }
                else if (varTypeStr == "int" || varTypeStr == "SInt32")//Int32
                {
                    a_Stream.ReadInt32();
                }
                else if (varTypeStr == "UInt32" || varTypeStr == "unsigned int" || varTypeStr == "Type*")//UInt32
                {
                    a_Stream.ReadUInt32();
                }
                else if (varTypeStr == "long long" || varTypeStr == "SInt64")//Int64
                {
                    a_Stream.ReadInt64();
                }
                else if (varTypeStr == "UInt64" || varTypeStr == "unsigned long long")//UInt64
                {
                    a_Stream.ReadUInt64();
                }
                else if (varTypeStr == "float")//float
                {
                    a_Stream.ReadSingle();
                }
                else if (varTypeStr == "double")//double
                {
                    a_Stream.ReadDouble();
                }
                else if (varTypeStr == "bool")//bool
                {
                    a_Stream.ReadBoolean();
                }
                else if (varTypeStr == "string")//string
                {
                    a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                    i += 3;//skip
                }
                else if (varTypeStr == "Array")//Array
                {
                    if ((members[i - 1].Flag & 0x4000) != 0)
                        align = true;
                    var size = a_Stream.ReadInt32();
                    var array = ClassStructHelper.ReadArray(members, level, i);
                    for (int j = 0; j < size; j++)
                    {
                        ReadSerializedShader(array, a_Stream);
                    }
                    i += array.Count + 1;//skip
                }
                else
                {
                    if (align)
                    {
                        align = false;
                        ClassStructHelper.SetAlignBefore(members, level, i + 1);
                    }
                }
                if (align)
                    a_Stream.AlignStream(4);
            }
            return null;
        }*/
    }
}
