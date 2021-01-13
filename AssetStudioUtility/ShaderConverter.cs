using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lz4;

namespace AssetStudio
{
    public static class ShaderConverter
    {
        public static string Convert(this Shader shader)
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
                    var program = new ShaderProgram(blobReader, shader.version);
                    return header + program.Export(Encoding.UTF8.GetString(shader.m_Script));
                }
            }

            if (shader.compressedBlob != null) //5.5 and up
            {
                return header + ConvertSerializedShader(shader);
            }

            return header + Encoding.UTF8.GetString(shader.m_Script);
        }

        private static string ConvertSerializedShader(Shader shader)
        {
            var shaderPrograms = new ShaderProgram[shader.platforms.Length];
            for (var i = 0; i < shader.platforms.Length; i++)
            {
                var compressedBytes = new byte[shader.compressedLengths[i]];
                Buffer.BlockCopy(shader.compressedBlob, (int)shader.offsets[i], compressedBytes, 0, (int)shader.compressedLengths[i]);
                var decompressedBytes = new byte[shader.decompressedLengths[i]];
                using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes)))
                {
                    decoder.Read(decompressedBytes, 0, (int)shader.decompressedLengths[i]);
                }
                using (var blobReader = new BinaryReader(new MemoryStream(decompressedBytes)))
                {
                    shaderPrograms[i] = new ShaderProgram(blobReader, shader.version);
                }
            }

            return ConvertSerializedShader(shader.m_ParsedForm, shader.platforms, shaderPrograms);
        }

        private static string ConvertSerializedShader(SerializedShader m_ParsedForm, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            var sb = new StringBuilder();
            sb.Append($"Shader \"{m_ParsedForm.m_Name}\" {{\n");

            sb.Append(ConvertSerializedProperties(m_ParsedForm.m_PropInfo));

            foreach (var m_SubShader in m_ParsedForm.m_SubShaders)
            {
                sb.Append(ConvertSerializedSubShader(m_SubShader, platforms, shaderPrograms));
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.m_FallbackName))
            {
                sb.Append($"Fallback \"{m_ParsedForm.m_FallbackName}\"\n");
            }

            if (!string.IsNullOrEmpty(m_ParsedForm.m_CustomEditorName))
            {
                sb.Append($"CustomEditor \"{m_ParsedForm.m_CustomEditorName}\"\n");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private static string ConvertSerializedSubShader(SerializedSubShader m_SubShader, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            var sb = new StringBuilder();
            sb.Append("SubShader {\n");
            if (m_SubShader.m_LOD != 0)
            {
                sb.Append($" LOD {m_SubShader.m_LOD}\n");
            }

            sb.Append(ConvertSerializedTagMap(m_SubShader.m_Tags, 1));

            foreach (var m_Passe in m_SubShader.m_Passes)
            {
                sb.Append(ConvertSerializedPass(m_Passe, platforms, shaderPrograms));
            }
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string ConvertSerializedPass(SerializedPass m_Passe, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            var sb = new StringBuilder();
            switch (m_Passe.m_Type)
            {
                case PassType.kPassTypeNormal:
                    sb.Append(" Pass ");
                    break;
                case PassType.kPassTypeUse:
                    sb.Append(" UsePass ");
                    break;
                case PassType.kPassTypeGrab:
                    sb.Append(" GrabPass ");
                    break;
            }
            if (m_Passe.m_Type == PassType.kPassTypeUse)
            {
                sb.Append($"\"{m_Passe.m_UseName}\"\n");
            }
            else
            {
                sb.Append("{\n");

                if (m_Passe.m_Type == PassType.kPassTypeGrab)
                {
                    if (!string.IsNullOrEmpty(m_Passe.m_TextureName))
                    {
                        sb.Append($"  \"{m_Passe.m_TextureName}\"\n");
                    }
                }
                else
                {
                    sb.Append(ConvertSerializedShaderState(m_Passe.m_State));

                    if (m_Passe.progVertex.m_SubPrograms.Length > 0)
                    {
                        sb.Append("Program \"vp\" {\n");
                        sb.Append(ConvertSerializedSubPrograms(m_Passe.progVertex.m_SubPrograms, platforms, shaderPrograms));
                        sb.Append("}\n");
                    }

                    if (m_Passe.progFragment.m_SubPrograms.Length > 0)
                    {
                        sb.Append("Program \"fp\" {\n");
                        sb.Append(ConvertSerializedSubPrograms(m_Passe.progFragment.m_SubPrograms, platforms, shaderPrograms));
                        sb.Append("}\n");
                    }

                    if (m_Passe.progGeometry.m_SubPrograms.Length > 0)
                    {
                        sb.Append("Program \"gp\" {\n");
                        sb.Append(ConvertSerializedSubPrograms(m_Passe.progGeometry.m_SubPrograms, platforms, shaderPrograms));
                        sb.Append("}\n");
                    }

                    if (m_Passe.progHull.m_SubPrograms.Length > 0)
                    {
                        sb.Append("Program \"hp\" {\n");
                        sb.Append(ConvertSerializedSubPrograms(m_Passe.progHull.m_SubPrograms, platforms, shaderPrograms));
                        sb.Append("}\n");
                    }

                    if (m_Passe.progDomain.m_SubPrograms.Length > 0)
                    {
                        sb.Append("Program \"dp\" {\n");
                        sb.Append(ConvertSerializedSubPrograms(m_Passe.progDomain.m_SubPrograms, platforms, shaderPrograms));
                        sb.Append("}\n");
                    }
                }
                sb.Append("}\n");
            }
            return sb.ToString();
        }

        private static string ConvertSerializedSubPrograms(SerializedSubProgram[] m_SubPrograms, ShaderCompilerPlatform[] platforms, ShaderProgram[] shaderPrograms)
        {
            var sb = new StringBuilder();
            var groups = m_SubPrograms.GroupBy(x => x.m_BlobIndex);
            foreach (var group in groups)
            {
                var programs = group.GroupBy(x => x.m_GpuProgramType);
                foreach (var program in programs)
                {
                    for (int i = 0; i < platforms.Length; i++)
                    {
                        var platform = platforms[i];
                        if (CheckGpuProgramUsable(platform, program.Key))
                        {
                            var subPrograms = program.ToList();
                            var isTier = subPrograms.Count > 1;
                            foreach (var subProgram in subPrograms)
                            {
                                sb.Append($"SubProgram \"{GetPlatformString(platform)} ");
                                if (isTier)
                                {
                                    sb.Append($"hw_tier{subProgram.m_ShaderHardwareTier:00} ");
                                }
                                sb.Append("\" {\n");
                                sb.Append(shaderPrograms[i].m_SubPrograms[subProgram.m_BlobIndex].Export());
                                sb.Append("\n}\n");
                            }
                            break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private static string ConvertSerializedShaderState(SerializedShaderState m_State)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(m_State.m_Name))
            {
                sb.Append($"  Name \"{m_State.m_Name}\"\n");
            }
            if (m_State.m_LOD != 0)
            {
                sb.Append($"  LOD {m_State.m_LOD}\n");
            }

            sb.Append(ConvertSerializedTagMap(m_State.m_Tags, 2));

            sb.Append(ConvertSerializedShaderRTBlendState(m_State.rtBlend));

            if (m_State.alphaToMask.val > 0f)
            {
                sb.Append("  AlphaToMask On\n");
            }

            if (m_State.zClip?.val != 1f) //ZClip On
            {
                sb.Append("  ZClip Off\n");
            }

            if (m_State.zTest.val != 4f) //ZTest LEqual
            {
                sb.Append("  ZTest ");
                switch (m_State.zTest.val) //enum CompareFunction
                {
                    case 0f: //kFuncDisabled
                        sb.Append("Off");
                        break;
                    case 1f: //kFuncNever
                        sb.Append("Never");
                        break;
                    case 2f: //kFuncLess
                        sb.Append("Less");
                        break;
                    case 3f: //kFuncEqual
                        sb.Append("Equal");
                        break;
                    case 5f: //kFuncGreater
                        sb.Append("Greater");
                        break;
                    case 6f: //kFuncNotEqual
                        sb.Append("NotEqual");
                        break;
                    case 7f: //kFuncGEqual
                        sb.Append("GEqual");
                        break;
                    case 8f: //kFuncAlways
                        sb.Append("Always");
                        break;
                }

                sb.Append("\n");
            }

            if (m_State.zWrite.val != 1f) //ZWrite On
            {
                sb.Append("  ZWrite Off\n");
            }

            if (m_State.culling.val != 2f) //Cull Back
            {
                sb.Append("  Cull ");
                switch (m_State.culling.val) //enum CullMode
                {
                    case 0f: //kCullOff
                        sb.Append("Off");
                        break;
                    case 1f: //kCullFront
                        sb.Append("Front");
                        break;
                }
                sb.Append("\n");
            }

            if (m_State.offsetFactor.val != 0f || m_State.offsetUnits.val != 0f)
            {
                sb.Append($"  Offset {m_State.offsetFactor.val}, {m_State.offsetUnits.val}\n");
            }

            //TODO Stencil

            //TODO Fog

            if (m_State.lighting)
            {
                sb.Append($"  Lighting {(m_State.lighting ? "On" : "Off")}\n");
            }

            sb.Append($"  GpuProgramID {m_State.gpuProgramID}\n");

            return sb.ToString();
        }

        private static string ConvertSerializedShaderRTBlendState(SerializedShaderRTBlendState[] rtBlend)
        {
            //TODO Blend
            var sb = new StringBuilder();
            /*for (var i = 0; i < rtBlend.Length; i++)
            {
                var blend = rtBlend[i];
                if (!blend.srcBlend.val.Equals(1f) ||
                    !blend.destBlend.val.Equals(0f) ||
                    !blend.srcBlendAlpha.val.Equals(1f) ||
                    !blend.destBlendAlpha.val.Equals(0f))
                {
                    sb.Append("  Blend ");
                    sb.Append($"{i} ");
                    sb.Append('\n');
                }
            }*/

            return sb.ToString();
        }

        private static string ConvertSerializedTagMap(SerializedTagMap m_Tags, int intent)
        {
            var sb = new StringBuilder();
            if (m_Tags.tags.Length > 0)
            {
                sb.Append(new string(' ', intent));
                sb.Append("Tags { ");
                foreach (var pair in m_Tags.tags)
                {
                    sb.Append($"\"{pair.Key}\" = \"{pair.Value}\" ");
                }
                sb.Append("}\n");
            }
            return sb.ToString();
        }

        private static string ConvertSerializedProperties(SerializedProperties m_PropInfo)
        {
            var sb = new StringBuilder();
            sb.Append("Properties {\n");
            foreach (var m_Prop in m_PropInfo.m_Props)
            {
                sb.Append(ConvertSerializedProperty(m_Prop));
            }
            sb.Append("}\n");
            return sb.ToString();
        }

        private static string ConvertSerializedProperty(SerializedProperty m_Prop)
        {
            var sb = new StringBuilder();
            foreach (var m_Attribute in m_Prop.m_Attributes)
            {
                sb.Append($"[{m_Attribute}] ");
            }
            //TODO Flag
            sb.Append($"{m_Prop.m_Name} (\"{m_Prop.m_Description}\", ");
            switch (m_Prop.m_Type)
            {
                case SerializedPropertyType.kColor:
                    sb.Append("Color");
                    break;
                case SerializedPropertyType.kVector:
                    sb.Append("Vector");
                    break;
                case SerializedPropertyType.kFloat:
                    sb.Append("Float");
                    break;
                case SerializedPropertyType.kRange:
                    sb.Append($"Range({m_Prop.m_DefValue[1]}, {m_Prop.m_DefValue[2]})");
                    break;
                case SerializedPropertyType.kTexture:
                    switch (m_Prop.m_DefTexture.m_TexDim)
                    {
                        case TextureDimension.kTexDimAny:
                            sb.Append("any");
                            break;
                        case TextureDimension.kTexDim2D:
                            sb.Append("2D");
                            break;
                        case TextureDimension.kTexDim3D:
                            sb.Append("3D");
                            break;
                        case TextureDimension.kTexDimCUBE:
                            sb.Append("Cube");
                            break;
                        case TextureDimension.kTexDim2DArray:
                            sb.Append("2DArray");
                            break;
                        case TextureDimension.kTexDimCubeArray:
                            sb.Append("CubeArray");
                            break;
                    }
                    break;
            }
            sb.Append(") = ");
            switch (m_Prop.m_Type)
            {
                case SerializedPropertyType.kColor:
                case SerializedPropertyType.kVector:
                    sb.Append($"({m_Prop.m_DefValue[0]},{m_Prop.m_DefValue[1]},{m_Prop.m_DefValue[2]},{m_Prop.m_DefValue[3]})");
                    break;
                case SerializedPropertyType.kFloat:
                case SerializedPropertyType.kRange:
                    sb.Append(m_Prop.m_DefValue[0]);
                    break;
                case SerializedPropertyType.kTexture:
                    sb.Append($"\"{m_Prop.m_DefTexture.m_DefaultName}\" {{ }}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            sb.Append("\n");
            return sb.ToString();
        }

        private static bool CheckGpuProgramUsable(ShaderCompilerPlatform platform, ShaderGpuProgramType programType)
        {
            switch (platform)
            {
                case ShaderCompilerPlatform.kShaderCompPlatformGL:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramGLLegacy;
                case ShaderCompilerPlatform.kShaderCompPlatformD3D9:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM20
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX9VertexSM30
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM20
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX9PixelSM30;
                case ShaderCompilerPlatform.kShaderCompPlatformXbox360:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformPS3:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformD3D11:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM40
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11VertexSM50
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM40
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11PixelSM50
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM40
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11GeometrySM50
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11HullSM50
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX11DomainSM50;
                case ShaderCompilerPlatform.kShaderCompPlatformGLES20:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramGLES;
                case ShaderCompilerPlatform.kShaderCompPlatformNaCl: //Obsolete
                    throw new NotSupportedException();
                case ShaderCompilerPlatform.kShaderCompPlatformFlash: //Obsolete
                    throw new NotSupportedException();
                case ShaderCompilerPlatform.kShaderCompPlatformD3D11_9x:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramDX10Level9Vertex
                        || programType == ShaderGpuProgramType.kShaderGpuProgramDX10Level9Pixel;
                case ShaderCompilerPlatform.kShaderCompPlatformGLES3Plus:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramGLES31AEP
                        || programType == ShaderGpuProgramType.kShaderGpuProgramGLES31
                        || programType == ShaderGpuProgramType.kShaderGpuProgramGLES3;
                case ShaderCompilerPlatform.kShaderCompPlatformPSP2:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformPS4:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformXboxOne:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformPSM: //Unknown
                    throw new NotSupportedException();
                case ShaderCompilerPlatform.kShaderCompPlatformMetal:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramMetalVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramMetalFS;
                case ShaderCompilerPlatform.kShaderCompPlatformOpenGLCore:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramGLCore32
                        || programType == ShaderGpuProgramType.kShaderGpuProgramGLCore41
                        || programType == ShaderGpuProgramType.kShaderGpuProgramGLCore43;
                case ShaderCompilerPlatform.kShaderCompPlatformN3DS:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformWiiU:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformVulkan:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramSPIRV;
                case ShaderCompilerPlatform.kShaderCompPlatformSwitch:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                case ShaderCompilerPlatform.kShaderCompPlatformXboxOneD3D12:
                    return programType == ShaderGpuProgramType.kShaderGpuProgramConsoleVS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleFS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleHS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleDS
                        || programType == ShaderGpuProgramType.kShaderGpuProgramConsoleGS;
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GetPlatformString(ShaderCompilerPlatform platform)
        {
            switch (platform)
            {
                case ShaderCompilerPlatform.kShaderCompPlatformGL:
                    return "openGL";
                case ShaderCompilerPlatform.kShaderCompPlatformD3D9:
                    return "d3d9";
                case ShaderCompilerPlatform.kShaderCompPlatformXbox360:
                    return "xbox360";
                case ShaderCompilerPlatform.kShaderCompPlatformPS3:
                    return "ps3";
                case ShaderCompilerPlatform.kShaderCompPlatformD3D11:
                    return "d3d11";
                case ShaderCompilerPlatform.kShaderCompPlatformGLES20:
                    return "gles";
                case ShaderCompilerPlatform.kShaderCompPlatformNaCl:
                    return "glesdesktop";
                case ShaderCompilerPlatform.kShaderCompPlatformFlash:
                    return "flash";
                case ShaderCompilerPlatform.kShaderCompPlatformD3D11_9x:
                    return "d3d11_9x";
                case ShaderCompilerPlatform.kShaderCompPlatformGLES3Plus:
                    return "gles3";
                case ShaderCompilerPlatform.kShaderCompPlatformPSP2:
                    return "psp2";
                case ShaderCompilerPlatform.kShaderCompPlatformPS4:
                    return "ps4";
                case ShaderCompilerPlatform.kShaderCompPlatformXboxOne:
                    return "xboxone";
                case ShaderCompilerPlatform.kShaderCompPlatformPSM:
                    return "psm";
                case ShaderCompilerPlatform.kShaderCompPlatformMetal:
                    return "metal";
                case ShaderCompilerPlatform.kShaderCompPlatformOpenGLCore:
                    return "glcore";
                case ShaderCompilerPlatform.kShaderCompPlatformN3DS:
                    return "n3ds";
                case ShaderCompilerPlatform.kShaderCompPlatformWiiU:
                    return "wiiu";
                case ShaderCompilerPlatform.kShaderCompPlatformVulkan:
                    return "vulkan";
                case ShaderCompilerPlatform.kShaderCompPlatformSwitch:
                    return "switch";
                case ShaderCompilerPlatform.kShaderCompPlatformXboxOneD3D12:
                    return "xboxone_d3d12";
                default:
                    return "unknown";
            }
        }

        private static string header = "//////////////////////////////////////////\n" +
                                      "//\n" +
                                      "// NOTE: This is *not* a valid shader file\n" +
                                      "//\n" +
                                      "///////////////////////////////////////////\n";
    }

    public class ShaderProgram
    {
        public ShaderSubProgram[] m_SubPrograms;

        public ShaderProgram(BinaryReader reader, int[] version)
        {
            var subProgramsCapacity = reader.ReadInt32();
            m_SubPrograms = new ShaderSubProgram[subProgramsCapacity];
            int entrySize;
            if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
            {
                entrySize = 12;
            }
            else
            {
                entrySize = 8;
            }
            for (int i = 0; i < subProgramsCapacity; i++)
            {
                reader.BaseStream.Position = 4 + i * entrySize;
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
        private int m_Version;
        public ShaderGpuProgramType m_ProgramType;
        public string[] m_Keywords;
        public string[] m_LocalKeywords;
        public byte[] m_ProgramCode;

        public ShaderSubProgram(BinaryReader reader)
        {
            //LoadGpuProgramFromData
            //201509030 - Unity 5.3
            //201510240 - Unity 5.4
            //201608170 - Unity 5.5
            //201609010 - Unity 5.6, 2017.1 & 2017.2
            //201708220 - Unity 2017.3, Unity 2017.4 & Unity 2018.1
            //201802150 - Unity 2018.2 & Unity 2018.3
            //201806140 - Unity 2019.1~2020.1
            m_Version = reader.ReadInt32();
            m_ProgramType = (ShaderGpuProgramType)reader.ReadInt32();
            reader.BaseStream.Position += 12;
            if (m_Version >= 201608170)
            {
                reader.BaseStream.Position += 4;
            }
            var m_KeywordsSize = reader.ReadInt32();
            m_Keywords = new string[m_KeywordsSize];
            for (int i = 0; i < m_KeywordsSize; i++)
            {
                m_Keywords[i] = reader.ReadAlignedString();
            }
            if (m_Version >= 201806140)
            {
                var m_LocalKeywordsSize = reader.ReadInt32();
                m_LocalKeywords = new string[m_LocalKeywordsSize];
                for (int i = 0; i < m_LocalKeywordsSize; i++)
                {
                    m_LocalKeywords[i] = reader.ReadAlignedString();
                }
            }
            m_ProgramCode = reader.ReadUInt8Array();
            reader.AlignStream();

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
            if (m_LocalKeywords != null && m_LocalKeywords.Length > 0)
            {
                sb.Append("Local Keywords { ");
                foreach (string keyword in m_LocalKeywords)
                {
                    sb.Append($"\"{keyword}\" ");
                }
                sb.Append("}\n");
            }

            sb.Append("\"");
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
                            /*var shaderBytecode = new ShaderBytecode(m_ProgramCode);
                            sb.Append(shaderBytecode.Disassemble());*/
                            sb.Append("// shader disassembly not supported on DXBC");
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
                            /*int start = 6;
                            if (m_Version == 201509030) // 5.3
                            {
                                start = 5;
                            }
                            var buff = new byte[m_ProgramCode.Length - start];
                            Buffer.BlockCopy(m_ProgramCode, start, buff, 0, buff.Length);
                            var shaderBytecode = new ShaderBytecode(buff);
                            sb.Append(shaderBytecode.Disassemble());*/
                            sb.Append("// shader disassembly not supported on DXBC");
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
                    case ShaderGpuProgramType.kShaderGpuProgramSPIRV:
                        try
                        {
                            sb.Append(SpirVShaderConverter.Convert(m_ProgramCode));
                        }
                        catch (Exception e)
                        {
                            sb.Append($"// disassembly error {e.Message}\n");
                        }
                        break;
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleVS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleFS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleHS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleDS:
                    case ShaderGpuProgramType.kShaderGpuProgramConsoleGS:
                        sb.Append(Encoding.UTF8.GetString(m_ProgramCode));
                        break;
                    default:
                        sb.Append($"//shader disassembly not supported on {m_ProgramType}");
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
