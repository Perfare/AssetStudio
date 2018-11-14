using System;
using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public class StructParameter
    {
        public List<MatrixParameter> m_MatrixParams { get; set; }
        public List<VectorParameter> m_VectorParams { get; set; }

        public StructParameter(BinaryReader reader)
        {
            var m_NameIndex = reader.ReadInt32();
            var m_Index = reader.ReadInt32();
            var m_ArraySize = reader.ReadInt32();
            var m_StructSize = reader.ReadInt32();

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>(numVectorParams);
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>(numMatrixParams);
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }
        }
    }

    public class SamplerParameter
    {
        public uint sampler { get; set; }
        public int bindPoint { get; set; }

        public SamplerParameter(BinaryReader reader)
        {
            sampler = reader.ReadUInt32();
            bindPoint = reader.ReadInt32();
        }
    }

    public class SerializedTextureProperty
    {
        public string m_DefaultName { get; set; }
        public int m_TexDim { get; set; }

        public SerializedTextureProperty(BinaryReader reader)
        {
            m_DefaultName = reader.ReadAlignedString();
            m_TexDim = reader.ReadInt32();
        }
    }

    public class SerializedProperty
    {
        public string m_Name { get; set; }
        public string m_Description { get; set; }
        public List<string> m_Attributes { get; set; }
        public int m_Type { get; set; }
        public uint m_Flags { get; set; }
        public List<float> m_DefValue { get; set; }
        public SerializedTextureProperty m_DefTexture { get; set; }

        public SerializedProperty(BinaryReader reader)
        {
            m_Name = reader.ReadAlignedString();
            m_Description = reader.ReadAlignedString();

            int numAttributes = reader.ReadInt32();
            m_Attributes = new List<string>(numAttributes);
            for (int i = 0; i < numAttributes; i++)
            {
                m_Attributes.Add(reader.ReadAlignedString());
            }

            m_Type = reader.ReadInt32();
            m_Flags = reader.ReadUInt32();

            int numValues = 4;
            m_DefValue = new List<float>(numValues);
            for (int i = 0; i < numValues; i++)
            {
                m_DefValue.Add(reader.ReadSingle());
            }

            m_DefTexture = new SerializedTextureProperty(reader);
        }
    }

    public class SerializedProperties
    {
        public List<SerializedProperty> m_Props { get; set; }

        public SerializedProperties(BinaryReader reader)
        {
            int numProps = reader.ReadInt32();
            m_Props = new List<SerializedProperty>(numProps);
            for (int i = 0; i < numProps; i++)
            {
                m_Props.Add(new SerializedProperty(reader));
            }
        }
    }

    public class SerializedShaderFloatValue
    {
        public float val { get; set; }
        public string name { get; set; }

        public SerializedShaderFloatValue(BinaryReader reader)
        {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }

    public class SerializedShaderRTBlendState
    {
        public SerializedShaderFloatValue srcBlend { get; set; }
        public SerializedShaderFloatValue destBlend { get; set; }
        public SerializedShaderFloatValue srcBlendAlpha { get; set; }
        public SerializedShaderFloatValue destBlendAlpha { get; set; }
        public SerializedShaderFloatValue blendOp { get; set; }
        public SerializedShaderFloatValue blendOpAlpha { get; set; }
        public SerializedShaderFloatValue colMask { get; set; }

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
        public SerializedShaderFloatValue pass { get; set; }
        public SerializedShaderFloatValue fail { get; set; }
        public SerializedShaderFloatValue zFail { get; set; }
        public SerializedShaderFloatValue comp { get; set; }

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
        public SerializedShaderFloatValue x { get; set; }
        public SerializedShaderFloatValue y { get; set; }
        public SerializedShaderFloatValue z { get; set; }
        public SerializedShaderFloatValue w { get; set; }
        public string name { get; set; }

        public SerializedShaderVectorValue(BinaryReader reader)
        {
            x = new SerializedShaderFloatValue(reader);
            y = new SerializedShaderFloatValue(reader);
            z = new SerializedShaderFloatValue(reader);
            w = new SerializedShaderFloatValue(reader);
            name = reader.ReadAlignedString();
        }
    }

    public class SerializedShaderState
    {
        public string m_Name { get; set; }
        public SerializedShaderRTBlendState rtBlend0 { get; set; }
        public SerializedShaderRTBlendState rtBlend1 { get; set; }
        public SerializedShaderRTBlendState rtBlend2 { get; set; }
        public SerializedShaderRTBlendState rtBlend3 { get; set; }
        public SerializedShaderRTBlendState rtBlend4 { get; set; }
        public SerializedShaderRTBlendState rtBlend5 { get; set; }
        public SerializedShaderRTBlendState rtBlend6 { get; set; }
        public SerializedShaderRTBlendState rtBlend7 { get; set; }
        public bool rtSeparateBlend { get; set; }
        public SerializedShaderFloatValue zClip { get; set; }
        public SerializedShaderFloatValue zTest { get; set; }
        public SerializedShaderFloatValue zWrite { get; set; }
        public SerializedShaderFloatValue culling { get; set; }
        public SerializedShaderFloatValue offsetFactor { get; set; }
        public SerializedShaderFloatValue offsetUnits { get; set; }
        public SerializedShaderFloatValue alphaToMask { get; set; }
        public SerializedStencilOp stencilOp { get; set; }
        public SerializedStencilOp stencilOpFront { get; set; }
        public SerializedStencilOp stencilOpBack { get; set; }
        public SerializedShaderFloatValue stencilReadMask { get; set; }
        public SerializedShaderFloatValue stencilWriteMask { get; set; }
        public SerializedShaderFloatValue stencilRef { get; set; }
        public SerializedShaderFloatValue fogStart { get; set; }
        public SerializedShaderFloatValue fogEnd { get; set; }
        public SerializedShaderFloatValue fogDensity { get; set; }
        public SerializedShaderVectorValue fogColor { get; set; }
        public int fogMode { get; set; }
        public int gpuProgramID { get; set; }
        public SerializedTagMap m_Tags { get; set; }
        public int m_LOD { get; set; }
        public bool lighting { get; set; }

        public SerializedShaderState(ObjectReader reader)
        {
            var version = reader.version;

            m_Name = reader.ReadAlignedString();
            rtBlend0 = new SerializedShaderRTBlendState(reader);
            rtBlend1 = new SerializedShaderRTBlendState(reader);
            rtBlend2 = new SerializedShaderRTBlendState(reader);
            rtBlend3 = new SerializedShaderRTBlendState(reader);
            rtBlend4 = new SerializedShaderRTBlendState(reader);
            rtBlend5 = new SerializedShaderRTBlendState(reader);
            rtBlend6 = new SerializedShaderRTBlendState(reader);
            rtBlend7 = new SerializedShaderRTBlendState(reader);
            rtSeparateBlend = reader.ReadBoolean();
            reader.AlignStream(4);
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
            fogMode = reader.ReadInt32();
            gpuProgramID = reader.ReadInt32();
            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
            lighting = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class ShaderBindChannel
    {
        public sbyte source { get; set; }
        public sbyte target { get; set; }

        public ShaderBindChannel(BinaryReader reader)
        {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }

    public class ParserBindChannels
    {
        public List<ShaderBindChannel> m_Channels { get; set; }
        public uint m_SourceMap { get; set; }

        public ParserBindChannels(BinaryReader reader)
        {
            int numChannels = reader.ReadInt32();
            m_Channels = new List<ShaderBindChannel>(numChannels);
            for (int i = 0; i < numChannels; i++)
            {
                m_Channels.Add(new ShaderBindChannel(reader));
            }
            reader.AlignStream(4);

            m_SourceMap = reader.ReadUInt32();
        }
    }

    public class VectorParameter
    {
        public int m_NameIndex { get; set; }
        public int m_Index { get; set; }
        public int m_ArraySize { get; set; }
        public sbyte m_Type { get; set; }
        public sbyte m_Dim { get; set; }

        public VectorParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_Dim = reader.ReadSByte();
            reader.AlignStream(4);
        }
    }

    public class MatrixParameter
    {
        public int m_NameIndex { get; set; }
        public int m_Index { get; set; }
        public int m_ArraySize { get; set; }
        public sbyte m_Type { get; set; }
        public sbyte m_RowCount { get; set; }

        public MatrixParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_RowCount = reader.ReadSByte();
            reader.AlignStream(4);
        }
    }

    public class TextureParameter
    {
        public int m_NameIndex { get; set; }
        public int m_Index { get; set; }
        public int m_SamplerIndex { get; set; }
        public sbyte m_Dim { get; set; }

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
            reader.AlignStream(4);
        }
    }

    public class BufferBinding
    {
        public int m_NameIndex { get; set; }
        public int m_Index { get; set; }

        public BufferBinding(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
        }
    }

    public class ConstantBuffer
    {
        public int m_NameIndex { get; set; }
        public List<MatrixParameter> m_MatrixParams { get; set; }
        public List<VectorParameter> m_VectorParams { get; set; }
        public List<StructParameter> m_StructParams { get; set; }
        public int m_Size { get; set; }

        public ConstantBuffer(ObjectReader reader)
        {
            var version = reader.version;

            m_NameIndex = reader.ReadInt32();

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>(numMatrixParams);
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>(numVectorParams);
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }
            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                int numStructParams = reader.ReadInt32();
                m_StructParams = new List<StructParameter>(numStructParams);
                for (int i = 0; i < numStructParams; i++)
                {
                    m_StructParams.Add(new StructParameter(reader));
                }
            }
            m_Size = reader.ReadInt32();
        }
    }

    public class UAVParameter
    {
        public int m_NameIndex { get; set; }
        public int m_Index { get; set; }
        public int m_OriginalIndex { get; set; }

        public UAVParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_OriginalIndex = reader.ReadInt32();
        }
    }

    public class SerializedSubProgram
    {
        public uint m_BlobIndex { get; set; }
        public ParserBindChannels m_Channels { get; set; }
        public List<ushort> m_KeywordIndices { get; set; }
        public sbyte m_ShaderHardwareTier { get; set; }
        public sbyte m_GpuProgramType { get; set; }
        public List<VectorParameter> m_VectorParams { get; set; }
        public List<MatrixParameter> m_MatrixParams { get; set; }
        public List<TextureParameter> m_TextureParams { get; set; }
        public List<BufferBinding> m_BufferParams { get; set; }
        public List<ConstantBuffer> m_ConstantBuffers { get; set; }
        public List<BufferBinding> m_ConstantBufferBindings { get; set; }
        public List<UAVParameter> m_UAVParams { get; set; }
        public List<SamplerParameter> m_Samplers { get; set; }

        public SerializedSubProgram(ObjectReader reader)
        {
            var version = reader.version;

            m_BlobIndex = reader.ReadUInt32();
            m_Channels = new ParserBindChannels(reader);

            int numIndices = reader.ReadInt32();
            m_KeywordIndices = new List<ushort>(numIndices);
            for (int i = 0; i < numIndices; i++)
            {
                m_KeywordIndices.Add(reader.ReadUInt16());
            }
            if (version[0] >= 2017) //2017 and up
            {
                reader.AlignStream(4);
            }
            m_ShaderHardwareTier = reader.ReadSByte();
            m_GpuProgramType = reader.ReadSByte();
            reader.AlignStream(4);

            int numVectorParams = reader.ReadInt32();
            m_VectorParams = new List<VectorParameter>(numVectorParams);
            for (int i = 0; i < numVectorParams; i++)
            {
                m_VectorParams.Add(new VectorParameter(reader));
            }

            int numMatrixParams = reader.ReadInt32();
            m_MatrixParams = new List<MatrixParameter>(numMatrixParams);
            for (int i = 0; i < numMatrixParams; i++)
            {
                m_MatrixParams.Add(new MatrixParameter(reader));
            }

            int numTextureParams = reader.ReadInt32();
            m_TextureParams = new List<TextureParameter>(numTextureParams);
            for (int i = 0; i < numTextureParams; i++)
            {
                m_TextureParams.Add(new TextureParameter(reader));
            }

            int numBufferParams = reader.ReadInt32();
            m_BufferParams = new List<BufferBinding>(numBufferParams);
            for (int i = 0; i < numBufferParams; i++)
            {
                m_BufferParams.Add(new BufferBinding(reader));
            }

            int numConstantBuffers = reader.ReadInt32();
            m_ConstantBuffers = new List<ConstantBuffer>(numConstantBuffers);
            for (int i = 0; i < numConstantBuffers; i++)
            {
                m_ConstantBuffers.Add(new ConstantBuffer(reader));
            }

            int numConstantBufferBindings = reader.ReadInt32();
            m_ConstantBufferBindings = new List<BufferBinding>(numConstantBufferBindings);
            for (int i = 0; i < numConstantBufferBindings; i++)
            {
                m_ConstantBufferBindings.Add(new BufferBinding(reader));
            }

            int numUAVParams = reader.ReadInt32();
            m_UAVParams = new List<UAVParameter>(numUAVParams);
            for (int i = 0; i < numUAVParams; i++)
            {
                m_UAVParams.Add(new UAVParameter(reader));
            }

            if (version[0] >= 2017) //2017 and up
            {
                int numSamplers = reader.ReadInt32();
                m_Samplers = new List<SamplerParameter>(numSamplers);
                for (int i = 0; i < numSamplers; i++)
                {
                    m_Samplers.Add(new SamplerParameter(reader));
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
        public List<SerializedSubProgram> m_SubPrograms { get; set; }

        public SerializedProgram(ObjectReader reader)
        {
            int numSubPrograms = reader.ReadInt32();
            m_SubPrograms = new List<SerializedSubProgram>(numSubPrograms);
            for (int i = 0; i < numSubPrograms; i++)
            {
                m_SubPrograms.Add(new SerializedSubProgram(reader));
            }
        }
    }

    public class SerializedPass
    {
        public List<KeyValuePair<string, int>> m_NameIndices { get; set; }
        public int m_Type { get; set; }
        public SerializedShaderState m_State { get; set; }
        public uint m_ProgramMask { get; set; }
        public SerializedProgram progVertex { get; set; }
        public SerializedProgram progFragment { get; set; }
        public SerializedProgram progGeometry { get; set; }
        public SerializedProgram progHull { get; set; }
        public SerializedProgram progDomain { get; set; }
        public bool m_HasInstancingVariant { get; set; }
        public string m_UseName { get; set; }
        public string m_Name { get; set; }
        public string m_TextureName { get; set; }
        public SerializedTagMap m_Tags { get; set; }

        public SerializedPass(ObjectReader reader)
        {
            var version = reader.version;

            int numIndices = reader.ReadInt32();
            m_NameIndices = new List<KeyValuePair<string, int>>(numIndices);
            for (int i = 0; i < numIndices; i++)
            {
                m_NameIndices.Add(new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32()));
            }

            m_Type = reader.ReadInt32();
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
            reader.AlignStream(4);
            m_UseName = reader.ReadAlignedString();
            m_Name = reader.ReadAlignedString();
            m_TextureName = reader.ReadAlignedString();
            m_Tags = new SerializedTagMap(reader);
        }
    }

    public class SerializedTagMap
    {
        public List<KeyValuePair<string, string>> tags { get; set; }

        public SerializedTagMap(BinaryReader reader)
        {
            int numTags = reader.ReadInt32();
            tags = new List<KeyValuePair<string, string>>(numTags);
            for (int i = 0; i < numTags; i++)
            {
                tags.Add(new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString()));
            }
        }
    }

    public class SerializedSubShader
    {
        public List<SerializedPass> m_Passes { get; set; }
        public SerializedTagMap m_Tags { get; set; }
        public int m_LOD { get; set; }

        public SerializedSubShader(ObjectReader reader)
        {
            int numPasses = reader.ReadInt32();
            m_Passes = new List<SerializedPass>(numPasses);
            for (int i = 0; i < numPasses; i++)
            {
                m_Passes.Add(new SerializedPass(reader));
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
        }
    }

    public class SerializedShaderDependency
    {
        public string from { get; set; }
        public string to { get; set; }

        public SerializedShaderDependency(BinaryReader reader)
        {
            from = reader.ReadAlignedString();
            to = reader.ReadAlignedString();
        }
    }

    public class SerializedShader
    {
        public SerializedProperties m_PropInfo { get; set; }
        public List<SerializedSubShader> m_SubShaders { get; set; }
        public string m_Name { get; set; }
        public string m_CustomEditorName { get; set; }
        public string m_FallbackName { get; set; }
        public List<SerializedShaderDependency> m_Dependencies { get; set; }
        public bool m_DisableNoSubshadersMessage { get; set; }

        public SerializedShader(ObjectReader reader)
        {
            m_PropInfo = new SerializedProperties(reader);

            int numSubShaders = reader.ReadInt32();
            m_SubShaders = new List<SerializedSubShader>(numSubShaders);
            for (int i = 0; i < numSubShaders; i++)
            {
                m_SubShaders.Add(new SerializedSubShader(reader));
            }

            m_Name = reader.ReadAlignedString();
            m_CustomEditorName = reader.ReadAlignedString();
            m_FallbackName = reader.ReadAlignedString();

            int numDependencies = reader.ReadInt32();
            m_Dependencies = new List<SerializedShaderDependency>(numDependencies);
            for (int i = 0; i < numDependencies; i++)
            {
                m_Dependencies.Add(new SerializedShaderDependency(reader));
            }

            m_DisableNoSubshadersMessage = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class Shader : NamedObject
    {
        public byte[] m_Script;
        //5.3 - 5.4
        public uint decompressedSize;
        public byte[] m_SubProgramBlob;
        //5.5 and up
        public SerializedShader m_ParsedForm;
        public List<uint> platforms;
        public List<uint> offsets;
        public List<uint> compressedLengths;
        public List<uint> decompressedLengths;
        public byte[] compressedBlob;

        public Shader(ObjectReader reader) : base(reader)
        {
            if (version[0] == 5 && version[1] >= 5 || version[0] > 5) //5.5 and up
            {
                m_ParsedForm = new SerializedShader(reader);
                int numPlatforms = reader.ReadInt32();
                platforms = new List<uint>(numPlatforms);
                for (int i = 0; i < numPlatforms; i++)
                {
                    platforms.Add(reader.ReadUInt32());
                }

                int numOffsets = reader.ReadInt32();
                offsets = new List<uint>(numOffsets);
                for (int i = 0; i < numOffsets; i++)
                {
                    offsets.Add(reader.ReadUInt32());
                }

                int numCompressedLengths = reader.ReadInt32();
                compressedLengths = new List<uint>(numCompressedLengths);
                for (int i = 0; i < numCompressedLengths; i++)
                {
                    compressedLengths.Add(reader.ReadUInt32());
                }

                int numDecompressedLengths = reader.ReadInt32();
                decompressedLengths = new List<uint>(numDecompressedLengths);
                for (int i = 0; i < numDecompressedLengths; i++)
                {
                    decompressedLengths.Add(reader.ReadUInt32());
                }

                compressedBlob = reader.ReadBytes(reader.ReadInt32());
            }
            else
            {
                m_Script = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream(4);
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
