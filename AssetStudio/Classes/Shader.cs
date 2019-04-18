using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public class StructParameter
    {
        public MatrixParameter[] m_MatrixParams;
        public VectorParameter[] m_VectorParams;

        public StructParameter(BinaryReader reader)
        {
            var m_NameIndex = reader.ReadInt32();
            var m_Index = reader.ReadInt32();
            var m_ArraySize = reader.ReadInt32();
            var m_StructSize = reader.ReadInt32();

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams[i] = new VectorParameter(reader);
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }
        }
    }

    public class SamplerParameter
    {
        public uint sampler;
        public int bindPoint;

        public SamplerParameter(BinaryReader reader)
        {
            sampler = reader.ReadUInt32();
            bindPoint = reader.ReadInt32();
        }
    }
    public enum TextureDimension
    {
        kTexDimUnknown = -1,
        kTexDimNone = 0,
        kTexDimAny = 1,
        kTexDim2D = 2,
        kTexDim3D = 3,
        kTexDimCUBE = 4,
        kTexDim2DArray = 5,
        kTexDimCubeArray = 6,
        kTexDimForce32Bit = 2147483647
    };

    public class SerializedTextureProperty
    {
        public string m_DefaultName;
        public TextureDimension m_TexDim;

        public SerializedTextureProperty(BinaryReader reader)
        {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = (TextureDimension)reader.ReadInt32();
        }
    }

    public enum SerializedPropertyType
    {
        kColor = 0,
        kVector = 1,
        kFloat = 2,
        kRange = 3,
        kTexture = 4
    };

    public class SerializedProperty
    {
        public string m_Name;
        public string m_Description;
        public string[] m_Attributes;
        public SerializedPropertyType m_Type;
        public uint m_Flags;
        public float[] m_DefValue;
        public SerializedTextureProperty m_DefTexture;

        public SerializedProperty(BinaryReader reader)
        {
            m_Name = reader.ReadAlignedString();
            m_Description = reader.ReadAlignedString();
            m_Attributes = reader.ReadStringArray();
            m_Type = (SerializedPropertyType)reader.ReadInt32();
            m_Flags = reader.ReadUInt32();
            m_DefValue = reader.ReadSingleArray(4);
            m_DefTexture = new SerializedTextureProperty(reader);
        }
    }

    public class SerializedProperties
    {
        public SerializedProperty[] m_Props;

        public SerializedProperties(BinaryReader reader)
        {
            int numProps = reader.ReadInt32();
            m_Props = new SerializedProperty[numProps];
            for (int i = 0; i < numProps; i++)
            {
                m_Props[i] = new SerializedProperty(reader);
            }
        }
    }

    public class SerializedShaderFloatValue
    {
        public float val;
        public string name;

        public SerializedShaderFloatValue(BinaryReader reader)
        {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }

    public class SerializedShaderRTBlendState
    {
        public SerializedShaderFloatValue srcBlend;
        public SerializedShaderFloatValue destBlend;
        public SerializedShaderFloatValue srcBlendAlpha;
        public SerializedShaderFloatValue destBlendAlpha;
        public SerializedShaderFloatValue blendOp;
        public SerializedShaderFloatValue blendOpAlpha;
        public SerializedShaderFloatValue colMask;

        public SerializedShaderRTBlendState(BinaryReader reader)
        {
            srcBlend = new SerializedShaderFloatValue(reader);
            destBlend = new SerializedShaderFloatValue(reader);
            srcBlendAlpha = new SerializedShaderFloatValue(reader);
            destBlendAlpha = new SerializedShaderFloatValue(reader);
            blendOp = new SerializedShaderFloatValue(reader);
            blendOpAlpha = new SerializedShaderFloatValue(reader);
            colMask = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedStencilOp
    {
        public SerializedShaderFloatValue pass;
        public SerializedShaderFloatValue fail;
        public SerializedShaderFloatValue zFail;
        public SerializedShaderFloatValue comp;

        public SerializedStencilOp(BinaryReader reader)
        {
            pass = new SerializedShaderFloatValue(reader);
            fail = new SerializedShaderFloatValue(reader);
            zFail = new SerializedShaderFloatValue(reader);
            comp = new SerializedShaderFloatValue(reader);
        }
    }

    public class SerializedShaderVectorValue
    {
        public SerializedShaderFloatValue x;
        public SerializedShaderFloatValue y;
        public SerializedShaderFloatValue z;
        public SerializedShaderFloatValue w;
        public string name;

        public SerializedShaderVectorValue(BinaryReader reader)
        {
            x = new SerializedShaderFloatValue(reader);
            y = new SerializedShaderFloatValue(reader);
            z = new SerializedShaderFloatValue(reader);
            w = new SerializedShaderFloatValue(reader);
            name = reader.ReadAlignedString();
        }
    }

    public enum FogMode
    {
        kFogUnknown = -1,
        kFogDisabled = 0,
        kFogLinear = 1,
        kFogExp = 2,
        kFogExp2 = 3
    };

    public class SerializedShaderState
    {
        public string m_Name;
        public SerializedShaderRTBlendState[] rtBlend;
        public bool rtSeparateBlend;
        public SerializedShaderFloatValue zClip;
        public SerializedShaderFloatValue zTest;
        public SerializedShaderFloatValue zWrite;
        public SerializedShaderFloatValue culling;
        public SerializedShaderFloatValue offsetFactor;
        public SerializedShaderFloatValue offsetUnits;
        public SerializedShaderFloatValue alphaToMask;
        public SerializedStencilOp stencilOp;
        public SerializedStencilOp stencilOpFront;
        public SerializedStencilOp stencilOpBack;
        public SerializedShaderFloatValue stencilReadMask;
        public SerializedShaderFloatValue stencilWriteMask;
        public SerializedShaderFloatValue stencilRef;
        public SerializedShaderFloatValue fogStart;
        public SerializedShaderFloatValue fogEnd;
        public SerializedShaderFloatValue fogDensity;
        public SerializedShaderVectorValue fogColor;
        public FogMode fogMode;
        public int gpuProgramID;
        public SerializedTagMap m_Tags;
        public int m_LOD;
        public bool lighting;

        public SerializedShaderState(ObjectReader reader)
        {
            var version = reader.version;

            m_Name = reader.ReadAlignedString();
            rtBlend = new SerializedShaderRTBlendState[8];
            for (int i = 0; i < 8; i++)
            {
                rtBlend[i] = new SerializedShaderRTBlendState(reader);
            }
            rtSeparateBlend = reader.ReadBoolean();
            reader.AlignStream();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                zClip = new SerializedShaderFloatValue(reader);
            }
            zTest = new SerializedShaderFloatValue(reader);
            zWrite = new SerializedShaderFloatValue(reader);
            culling = new SerializedShaderFloatValue(reader);
            offsetFactor = new SerializedShaderFloatValue(reader);
            offsetUnits = new SerializedShaderFloatValue(reader);
            alphaToMask = new SerializedShaderFloatValue(reader);
            stencilOp = new SerializedStencilOp(reader);
            stencilOpFront = new SerializedStencilOp(reader);
            stencilOpBack = new SerializedStencilOp(reader);
            stencilReadMask = new SerializedShaderFloatValue(reader);
            stencilWriteMask = new SerializedShaderFloatValue(reader);
            stencilRef = new SerializedShaderFloatValue(reader);
            fogStart = new SerializedShaderFloatValue(reader);
            fogEnd = new SerializedShaderFloatValue(reader);
            fogDensity = new SerializedShaderFloatValue(reader);
            fogColor = new SerializedShaderVectorValue(reader);
            fogMode = (FogMode)reader.ReadInt32();
            gpuProgramID = reader.ReadInt32();
            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
            lighting = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public class ShaderBindChannel
    {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(BinaryReader reader)
        {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }

    public class ParserBindChannels
    {
        public ShaderBindChannel[] m_Channels;
        public uint m_SourceMap;

        public ParserBindChannels(BinaryReader reader)
        {
            int numChannels = reader.ReadInt32();
            m_Channels = new ShaderBindChannel[numChannels];
            for (int i = 0; i < numChannels; i++)
            {
                m_Channels[i] = new ShaderBindChannel(reader);
            }
            reader.AlignStream();

            m_SourceMap = reader.ReadUInt32();
        }
    }

    public class VectorParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_Dim;

        public VectorParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_Dim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class MatrixParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_RowCount;

        public MatrixParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_RowCount = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class TextureParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_SamplerIndex;
        public sbyte m_Dim;

        public TextureParameter(ObjectReader reader)
        {
            var version = reader.version;

            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_SamplerIndex = reader.ReadInt32();
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                var m_MultiSampled = reader.ReadBoolean();
            }
            m_Dim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

    public class BufferBinding
    {
        public int m_NameIndex;
        public int m_Index;

        public BufferBinding(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
        }
    }

    public class ConstantBuffer
    {
        public int m_NameIndex;
        public MatrixParameter[] m_MatrixParams;
        public VectorParameter[] m_VectorParams;
        public StructParameter[] m_StructParams;
        public int m_Size;

        public ConstantBuffer(ObjectReader reader)
        {
            var version = reader.version;

            m_NameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams[i] = new VectorParameter(reader);
            }
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                int numStructParams = reader.ReadInt32();
                m_StructParams = new StructParameter[numStructParams];
                for (int i = 0; i < numStructParams; i++)
                {
                    m_StructParams[i] = new StructParameter(reader);
                }
            }
            m_Size = reader.ReadInt32();
        }
    }

    public class UAVParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_OriginalIndex;

        public UAVParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_OriginalIndex = reader.ReadInt32();
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
        kShaderGpuProgramConsole = 26
    };

    public class SerializedSubProgram
    {
        public uint m_BlobIndex;
        public ParserBindChannels m_Channels;
        public ushort[] m_KeywordIndices;
        public sbyte m_ShaderHardwareTier;
        public ShaderGpuProgramType m_GpuProgramType;
        public VectorParameter[] m_VectorParams;
        public MatrixParameter[] m_MatrixParams;
        public TextureParameter[] m_TextureParams;
        public BufferBinding[] m_BufferParams;
        public ConstantBuffer[] m_ConstantBuffers;
        public BufferBinding[] m_ConstantBufferBindings;
        public UAVParameter[] m_UAVParams;
        public SamplerParameter[] m_Samplers;

        public SerializedSubProgram(ObjectReader reader)
        {
            var version = reader.version;

            m_BlobIndex = reader.ReadUInt32();
            m_Channels = new ParserBindChannels(reader);

            if (version[0] >= 2019) //2019 and up
            {
                var m_GlobalKeywordIndices = reader.ReadUInt16Array();
                reader.AlignStream();
                var m_LocalKeywordIndices = reader.ReadUInt16Array();
                reader.AlignStream();
            }
            else
            {
                m_KeywordIndices = reader.ReadUInt16Array();
                if (version[0] >= 2017) //2017 and up
                {
                    reader.AlignStream();
                }
            }

            m_ShaderHardwareTier = reader.ReadSByte();
            m_GpuProgramType = (ShaderGpuProgramType)reader.ReadSByte();
            reader.AlignStream();

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new VectorParameter[numVectorParams];
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams[i] = new VectorParameter(reader);
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new MatrixParameter[numMatrixParams];
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams[i] = new MatrixParameter(reader);
            }

            int numTextureParams = reader.ReadInt32();
            m_TextureParams = new TextureParameter[numTextureParams];
            for (int i = 0; i < numTextureParams; i++)
            {
                m_TextureParams[i] = new TextureParameter(reader);
            }

            int numBufferParams = reader.ReadInt32();
            m_BufferParams = new BufferBinding[numBufferParams];
            for (int i = 0; i < numBufferParams; i++)
            {
                m_BufferParams[i] = new BufferBinding(reader);
            }

            int numConstantBuffers = reader.ReadInt32();
            m_ConstantBuffers = new ConstantBuffer[numConstantBuffers];
            for (int i = 0; i < numConstantBuffers; i++)
            {
                m_ConstantBuffers[i] = new ConstantBuffer(reader);
            }

            int numConstantBufferBindings = reader.ReadInt32();
            m_ConstantBufferBindings = new BufferBinding[numConstantBufferBindings];
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                m_ConstantBufferBindings[i] = new BufferBinding(reader);
            }

            int numUAVParams = reader.ReadInt32();
            m_UAVParams = new UAVParameter[numUAVParams];
            for (int i = 0; i < numUAVParams; i++)
            {
                m_UAVParams[i] = new UAVParameter(reader);
            }

            if (version[0] >= 2017) //2017 and up
            {
                int numSamplers = reader.ReadInt32();
                m_Samplers = new SamplerParameter[numSamplers];
                for (int i = 0; i < numSamplers; i++)
                {
                    m_Samplers[i] = new SamplerParameter(reader);
                }
            }
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                var m_ShaderRequirements = reader.ReadInt32();
            }
        }
    }

    public class SerializedProgram
    {
        public SerializedSubProgram[] m_SubPrograms;

        public SerializedProgram(ObjectReader reader)
        {
            int numSubPrograms = reader.ReadInt32();
            m_SubPrograms = new SerializedSubProgram[numSubPrograms];
            for (int i = 0; i < numSubPrograms; i++)
            {
                m_SubPrograms[i] = new SerializedSubProgram(reader);
            }
        }
    }

    public enum PassType
    {
        kPassTypeNormal = 0,
        kPassTypeUse = 1,
        kPassTypeGrab = 2
    };

    public class SerializedPass
    {
        public KeyValuePair<string, int>[] m_NameIndices;
        public PassType m_Type;
        public SerializedShaderState m_State;
        public uint m_ProgramMask;
        public SerializedProgram progVertex;
        public SerializedProgram progFragment;
        public SerializedProgram progGeometry;
        public SerializedProgram progHull;
        public SerializedProgram progDomain;
        public bool m_HasInstancingVariant;
        public string m_UseName;
        public string m_Name;
        public string m_TextureName;
        public SerializedTagMap m_Tags;

        public SerializedPass(ObjectReader reader)
        {
            var version = reader.version;

            int numIndices = reader.ReadInt32();
            m_NameIndices = new KeyValuePair<string, int>[numIndices];
            for (int i = 0; i < numIndices; i++)
            {
                m_NameIndices[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
            }

            m_Type = (PassType)reader.ReadInt32();
            m_State = new SerializedShaderState(reader);
            m_ProgramMask = reader.ReadUInt32();
            progVertex = new SerializedProgram(reader);
            progFragment = new SerializedProgram(reader);
            progGeometry = new SerializedProgram(reader);
            progHull = new SerializedProgram(reader);
            progDomain = new SerializedProgram(reader);
            m_HasInstancingVariant = reader.ReadBoolean();
            if (version[0] >= 2018) //2018 and up
            {
                var m_HasProceduralInstancingVariant = reader.ReadBoolean();
            }
            reader.AlignStream();
            m_UseName = reader.ReadAlignedString();
            m_Name = reader.ReadAlignedString();
            m_TextureName = reader.ReadAlignedString();
            m_Tags = new SerializedTagMap(reader);
        }
    }

    public class SerializedTagMap
    {
        public KeyValuePair<string, string>[] tags;

        public SerializedTagMap(BinaryReader reader)
        {
            int numTags = reader.ReadInt32();
            tags = new KeyValuePair<string, string>[numTags];
            for (int i = 0; i < numTags; i++)
            {
                tags[i] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
            }
        }
    }

    public class SerializedSubShader
    {
        public SerializedPass[] m_Passes;
        public SerializedTagMap m_Tags;
        public int m_LOD;

        public SerializedSubShader(ObjectReader reader)
        {
            int numPasses = reader.ReadInt32();
            m_Passes = new SerializedPass[numPasses];
            for (int i = 0; i < numPasses; i++)
            {
                m_Passes[i] = new SerializedPass(reader);
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
        }
    }

    public class SerializedShaderDependency
    {
        public string from;
        public string to;

        public SerializedShaderDependency(BinaryReader reader)
        {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }

    public class SerializedShader
    {
        public SerializedProperties m_PropInfo;
        public SerializedSubShader[] m_SubShaders;
        public string m_Name;
        public string m_CustomEditorName;
        public string m_FallbackName;
        public SerializedShaderDependency[] m_Dependencies;
        public bool m_DisableNoSubshadersMessage;

        public SerializedShader(ObjectReader reader)
        {
            m_PropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.ReadInt32();
            m_SubShaders = new SerializedSubShader[numSubShaders];
            for (int i = 0; i < numSubShaders; i++)
            {
                m_SubShaders[i] = new SerializedSubShader(reader);
            }

            m_Name = reader.ReadAlignedString();
            m_CustomEditorName = reader.ReadAlignedString();
            m_FallbackName = reader.ReadAlignedString();

            int numDependencies = reader.ReadInt32();
            m_Dependencies = new SerializedShaderDependency[numDependencies];
            for (int i = 0; i < numDependencies; i++)
            {
                m_Dependencies[i] = new SerializedShaderDependency(reader);
            }

            m_DisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public enum ShaderCompilerPlatform
    {
        kShaderCompPlatformNone = -1,
        kShaderCompPlatformGL = 0,
        kShaderCompPlatformD3D9 = 1,
        kShaderCompPlatformXbox360 = 2,
        kShaderCompPlatformPS3 = 3,
        kShaderCompPlatformD3D11 = 4,
        kShaderCompPlatformGLES20 = 5,
        kShaderCompPlatformNaCl = 6,
        kShaderCompPlatformFlash = 7,
        kShaderCompPlatformD3D11_9x = 8,
        kShaderCompPlatformGLES3Plus = 9,
        kShaderCompPlatformPSP2 = 10,
        kShaderCompPlatformPS4 = 11,
        kShaderCompPlatformXboxOne = 12,
        kShaderCompPlatformPSM = 13,
        kShaderCompPlatformMetal = 14,
        kShaderCompPlatformOpenGLCore = 15,
        kShaderCompPlatformN3DS = 16,
        kShaderCompPlatformWiiU = 17,
        kShaderCompPlatformVulkan = 18,
        kShaderCompPlatformSwitch = 19,
        kShaderCompPlatformXboxOneD3D12 = 20
    };

    public class Shader : NamedObject
    {
        public byte[] m_Script;
        //5.3 - 5.4
        public uint decompressedSize;
        public byte[] m_SubProgramBlob;
        //5.5 and up
        public SerializedShader m_ParsedForm;
        public ShaderCompilerPlatform[] platforms;
        public uint[] offsets;
        public uint[] compressedLengths;
        public uint[] decompressedLengths;
        public byte[] compressedBlob;

        public Shader(ObjectReader reader) : base(reader)
        {
            if (version[0] == 5 && version[1] >= 5 || version[0] > 5) //5.5 and up
            {
                m_ParsedForm = new SerializedShader(reader);
                platforms = reader.ReadUInt32Array().Select(x => (ShaderCompilerPlatform)x).ToArray();
                offsets = reader.ReadUInt32Array();
                compressedLengths = reader.ReadUInt32Array();
                decompressedLengths = reader.ReadUInt32Array();
                compressedBlob = reader.ReadBytes(reader.ReadInt32());
            }
            else
            {
                m_Script = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream();
                var m_PathName = reader.ReadAlignedString();
                if (version[0] == 5 && version[1] >= 3) //5.3 - 5.4
                {
                    decompressedSize = reader.ReadUInt32();
                    m_SubProgramBlob = reader.ReadBytes(reader.ReadInt32());
                }
            }
        }
    }
}
