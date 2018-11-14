using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Lz4;
using SharpDX.D3DCompiler;

namespace AssetStudio
{
    public static class ShaderConverter
    {
        public static string Convert(Shader shader)
        {
            if (shader.m_SubProgramBlob != null) //5.3 - 5.4
            {
                var decompressedBytes = new byte[shader.decompressedSize];
                using (var decoder = new Lz4DecoderStream(new MemoryStream(shader.m_SubProgramBlob)))
                {
                    decoder.Read(decompressedBytes, 0, (int)shader.decompressedSize);
                }
                using (var blobReader = new BinaryReader(new MemoryStream(decompressedBytes)))
                {
                    var program = new ShaderProgram(blobReader);
                    return program.Export(Encoding.UTF8.GetString(shader.m_Script));
                }
            }

            if (shader.compressedBlob != null) //5.5 and up
            {
                //TODO
                /*for (var i = 0; i < shader.platforms.Count; i++)
                {
                    var compressedBytes = new byte[shader.compressedLengths[i]];
                    Array.Copy(shader.compressedBlob, shader.offsets[i], compressedBytes, 0, shader.compressedLengths[i]);
                    var decompressedBytes = new byte[shader.decompressedLengths[i]];
                    using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes)))
                    {
                        decoder.Read(decompressedBytes, 0, (int)shader.decompressedLengths[i]);
                    }
                    using (var blobReader = new BinaryReader(new MemoryStream(decompressedBytes)))
                    {
                        new ShaderProgram(blobReader);
                    }
                }*/
                return shader.reader.Dump();
            }
            return Encoding.UTF8.GetString(shader.m_Script);
        }
    }

    public class ShaderProgram
    {
        public ShaderSubProgram[] m_SubPrograms;

        public ShaderProgram(BinaryReader reader)
        {
            var subProgramsCapacity = reader.ReadInt32();
            m_SubPrograms = new ShaderSubProgram[subProgramsCapacity];
            for (int i = 0; i < subProgramsCapacity; i++)
            {
                reader.BaseStream.Position = 4 + i * 8;
                var offset = reader.ReadInt32();
                reader.BaseStream.Position = offset;
                m_SubPrograms[i] = new ShaderSubProgram(reader);
            }
        }

        public string Export(string shader)
        {
            var evaluator = new MatchEvaluator(match =>
            {
                var index = int.Parse(match.Groups[1].Value);
                return m_SubPrograms[index].Export();
            });
            shader = Regex.Replace(shader, "GpuProgramIndex (.+)", evaluator);
            return shader;
        }
    }

    public class ShaderSubProgram
    {
        private int magic;
        public ShaderGpuProgramType m_ProgramType;
        public string[] m_Keywords;
        public byte[] m_ProgramCode;

        public ShaderSubProgram(BinaryReader reader)
        {
            //LoadGpuProgramFromData
            // 201509030 - Unity 5.3
            // 201510240 - Unity 5.4
            // 201608170 - Unity 5.5
            // 201609010 - Unity 5.6, 2017.1 & 2017.2
            // 201708220 - Unity 2017.3, Unity 2017.4 & Unity 2018.1
            // 201802150 - Unity 2018.2
            magic = reader.ReadInt32();
            m_ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
            reader.BaseStream.Position += 12;
            if (magic >= 201608170) //5.5.0 and up
            {
                reader.BaseStream.Position += 4;
            }
            var keywordCount = reader.ReadInt32();
            m_Keywords = new string[keywordCount];
            for (int i = 0; i < keywordCount; i++)
            {
                m_Keywords[i] = reader.ReadAlignedString();
            }
            m_ProgramCode = reader.ReadBytes(reader.ReadInt32());
            reader.AlignStream(4);

            //TODO
        }

        public string Export()
        {
            var sb = new StringBuilder();
            if (m_Keywords.Length > 0)
            {
                sb.Append("Keywords { ");
                foreach (string keyword in m_Keywords)
                {
                    sb.Append($"\"{keyword}\" ");
                }
                sb.Append("}\n");
            }

            sb.Append("\"\n");
            if (m_ProgramCode.Length > 0)
            {
                switch (m_ProgramType)
                {
                    case ShaderGpuProgramType.kShaderGpuProgramGLLegacy:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES31AEP:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES31:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES3:
                    case ShaderGpuProgramType.kShaderGpuProgramGLES:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore32:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore41:
                    case ShaderGpuProgramType.kShaderGpuProgramGLCore43:
                        sb.Append(Encoding.UTF8.GetString(m_ProgramCode));
                        break;
                    case ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM20:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM30:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM20:
                    case ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM30:
                        {
                            var shaderBytecode = new ShaderBytecode(m_ProgramCode);
                            sb.Append(shaderBytecode.Disassemble());
                            break;
                        }
                    case ShaderGpuProgramType.kShaderGpuProgramDX10Level9Vertex:
                    case ShaderGpuProgramType.kShaderGpuProgramDX10Level9Pixel:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM40:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11HullSM50:
                    case ShaderGpuProgramType.kShaderGpuProgramDX11DomainSM50:
                        {
                            int start = 6;
                            if (magic == 201509030) // 5.3
                            {
                                start = 5;
                            }
                            var buff = new byte[m_ProgramCode.Length - start];
                            Buffer.BlockCopy(m_ProgramCode, start, buff, 0, buff.Length);
                            var shaderBytecode = new ShaderBytecode(buff);
                            sb.Append(shaderBytecode.Disassemble());
                            break;
                        }
                    case ShaderGpuProgramType.kShaderGpuProgramMetalVS:
                    case ShaderGpuProgramType.kShaderGpuProgramMetalFS:
                        using (var reader = new BinaryReader(new MemoryStream(m_ProgramCode)))
                        {
                            var fourCC = reader.ReadUInt32();
                            if (fourCC == 0xf00dcafe)
                            {
                                int offset = reader.ReadInt32();
                                reader.BaseStream.Position = offset;
                            }
                            var entryName = reader.ReadStringToNull();
                            var buff = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                            sb.Append(Encoding.UTF8.GetString(buff));
                        }
                        break;
                    default:
                        sb.Append($"/*Unsupported program data {m_ProgramType}*/");
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }

    public enum ShaderGpuProgramType
    {
        kShaderGpuProgramUnknown = 0,
        kShaderGpuProgramGLLegacy = 1,
        kShaderGpuProgramGLES31AEP = 2,
        kShaderGpuProgramGLES31 = 3,
        kShaderGpuProgramGLES3 = 4,
        kShaderGpuProgramGLES = 5,
        kShaderGpuProgramGLCore32 = 6,
        kShaderGpuProgramGLCore41 = 7,
        kShaderGpuProgramGLCore43 = 8,
        kShaderGpuProgramDX9VertexSM20 = 9,
        kShaderGpuProgramDX9VertexSM30 = 10,
        kShaderGpuProgramDX9PixelSM20 = 11,
        kShaderGpuProgramDX9PixelSM30 = 12,
        kShaderGpuProgramDX10Level9Vertex = 13,
        kShaderGpuProgramDX10Level9Pixel = 14,
        kShaderGpuProgramDX11VertexSM40 = 15,
        kShaderGpuProgramDX11VertexSM50 = 16,
        kShaderGpuProgramDX11PixelSM40 = 17,
        kShaderGpuProgramDX11PixelSM50 = 18,
        kShaderGpuProgramDX11GeometrySM40 = 19,
        kShaderGpuProgramDX11GeometrySM50 = 20,
        kShaderGpuProgramDX11HullSM50 = 21,
        kShaderGpuProgramDX11DomainSM50 = 22,
        kShaderGpuProgramMetalVS = 23,
        kShaderGpuProgramMetalFS = 24,
        kShaderGpuProgramSPIRV = 25,
        kShaderGpuProgramConsole = 26,
    };
}
