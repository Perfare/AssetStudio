using System;
using System.Collections.Generic;

namespace SpirV
{
    [Flags]
    public enum ImageOperands : uint
    {
        None = 0,
        Bias = 1,
        Lod = 2,
        Grad = 4,
        ConstOffset = 8,
        Offset = 16,
        ConstOffsets = 32,
        Sample = 64,
        MinLod = 128,
    }
    public class ImageOperandsParameterFactory : ParameterFactory
    {
        public class BiasParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class LodParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class GradParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), new IdRef(), };
        }

        public class ConstOffsetParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class OffsetParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class ConstOffsetsParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class SampleParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class MinLodParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public override Parameter CreateParameter(object value)
        {
            switch ((ImageOperands)value)
            {
                case ImageOperands.Bias:
                    return new BiasParameter();
                case ImageOperands.Lod:
                    return new LodParameter();
                case ImageOperands.Grad:
                    return new GradParameter();
                case ImageOperands.ConstOffset:
                    return new ConstOffsetParameter();
                case ImageOperands.Offset:
                    return new OffsetParameter();
                case ImageOperands.ConstOffsets:
                    return new ConstOffsetsParameter();
                case ImageOperands.Sample:
                    return new SampleParameter();
                case ImageOperands.MinLod:
                    return new MinLodParameter();
            }

            return null;
        }
    }
    [Flags]
    public enum FPFastMathMode : uint
    {
        None = 0,
        NotNaN = 1,
        NotInf = 2,
        NSZ = 4,
        AllowRecip = 8,
        Fast = 16,
    }
    public class FPFastMathModeParameterFactory : ParameterFactory
    {
    }
    [Flags]
    public enum SelectionControl : uint
    {
        None = 0,
        Flatten = 1,
        DontFlatten = 2,
    }
    public class SelectionControlParameterFactory : ParameterFactory
    {
    }
    [Flags]
    public enum LoopControl : uint
    {
        None = 0,
        Unroll = 1,
        DontUnroll = 2,
        DependencyInfinite = 4,
        DependencyLength = 8,
    }
    public class LoopControlParameterFactory : ParameterFactory
    {
        public class DependencyLengthParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public override Parameter CreateParameter(object value)
        {
            switch ((LoopControl)value)
            {
                case LoopControl.DependencyLength:
                    return new DependencyLengthParameter();
            }

            return null;
        }
    }
    [Flags]
    public enum FunctionControl : uint
    {
        None = 0,
        Inline = 1,
        DontInline = 2,
        Pure = 4,
        Const = 8,
    }
    public class FunctionControlParameterFactory : ParameterFactory
    {
    }
    [Flags]
    public enum MemorySemantics : uint
    {
        Relaxed = 0,
        None = 0,
        Acquire = 2,
        Release = 4,
        AcquireRelease = 8,
        SequentiallyConsistent = 16,
        UniformMemory = 64,
        SubgroupMemory = 128,
        WorkgroupMemory = 256,
        CrossWorkgroupMemory = 512,
        AtomicCounterMemory = 1024,
        ImageMemory = 2048,
    }
    public class MemorySemanticsParameterFactory : ParameterFactory
    {
    }
    [Flags]
    public enum MemoryAccess : uint
    {
        None = 0,
        Volatile = 1,
        Aligned = 2,
        Nontemporal = 4,
    }
    public class MemoryAccessParameterFactory : ParameterFactory
    {
        public class AlignedParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public override Parameter CreateParameter(object value)
        {
            switch ((MemoryAccess)value)
            {
                case MemoryAccess.Aligned:
                    return new AlignedParameter();
            }

            return null;
        }
    }
    [Flags]
    public enum KernelProfilingInfo : uint
    {
        None = 0,
        CmdExecTime = 1,
    }
    public class KernelProfilingInfoParameterFactory : ParameterFactory
    {
    }
    public enum SourceLanguage : uint
    {
        Unknown = 0,
        ESSL = 1,
        GLSL = 2,
        OpenCL_C = 3,
        OpenCL_CPP = 4,
        HLSL = 5,
    }
    public class SourceLanguageParameterFactory : ParameterFactory
    {
    }
    public enum ExecutionModel : uint
    {
        Vertex = 0,
        TessellationControl = 1,
        TessellationEvaluation = 2,
        Geometry = 3,
        Fragment = 4,
        GLCompute = 5,
        Kernel = 6,
    }
    public class ExecutionModelParameterFactory : ParameterFactory
    {
    }
    public enum AddressingModel : uint
    {
        Logical = 0,
        Physical32 = 1,
        Physical64 = 2,
    }
    public class AddressingModelParameterFactory : ParameterFactory
    {
    }
    public enum MemoryModel : uint
    {
        Simple = 0,
        GLSL450 = 1,
        OpenCL = 2,
    }
    public class MemoryModelParameterFactory : ParameterFactory
    {
    }
    public enum ExecutionMode : uint
    {
        Invocations = 0,
        SpacingEqual = 1,
        SpacingFractionalEven = 2,
        SpacingFractionalOdd = 3,
        VertexOrderCw = 4,
        VertexOrderCcw = 5,
        PixelCenterInteger = 6,
        OriginUpperLeft = 7,
        OriginLowerLeft = 8,
        EarlyFragmentTests = 9,
        PointMode = 10,
        Xfb = 11,
        DepthReplacing = 12,
        DepthGreater = 14,
        DepthLess = 15,
        DepthUnchanged = 16,
        LocalSize = 17,
        LocalSizeHint = 18,
        InputPoints = 19,
        InputLines = 20,
        InputLinesAdjacency = 21,
        Triangles = 22,
        InputTrianglesAdjacency = 23,
        Quads = 24,
        Isolines = 25,
        OutputVertices = 26,
        OutputPoints = 27,
        OutputLineStrip = 28,
        OutputTriangleStrip = 29,
        VecTypeHint = 30,
        ContractionOff = 31,
        Initializer = 33,
        Finalizer = 34,
        SubgroupSize = 35,
        SubgroupsPerWorkgroup = 36,
        SubgroupsPerWorkgroupId = 37,
        LocalSizeId = 38,
        LocalSizeHintId = 39,
        PostDepthCoverage = 4446,
        StencilRefReplacingEXT = 5027,
    }
    public class ExecutionModeParameterFactory : ParameterFactory
    {
        public class InvocationsParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class LocalSizeParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), new LiteralInteger(), new LiteralInteger(), };
        }

        public class LocalSizeHintParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), new LiteralInteger(), new LiteralInteger(), };
        }

        public class OutputVerticesParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class VecTypeHintParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class SubgroupSizeParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class SubgroupsPerWorkgroupParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class SubgroupsPerWorkgroupIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class LocalSizeIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), new IdRef(), new IdRef(), };
        }

        public class LocalSizeHintIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public override Parameter CreateParameter(object value)
        {
            switch ((ExecutionMode)value)
            {
                case ExecutionMode.Invocations:
                    return new InvocationsParameter();
                case ExecutionMode.LocalSize:
                    return new LocalSizeParameter();
                case ExecutionMode.LocalSizeHint:
                    return new LocalSizeHintParameter();
                case ExecutionMode.OutputVertices:
                    return new OutputVerticesParameter();
                case ExecutionMode.VecTypeHint:
                    return new VecTypeHintParameter();
                case ExecutionMode.SubgroupSize:
                    return new SubgroupSizeParameter();
                case ExecutionMode.SubgroupsPerWorkgroup:
                    return new SubgroupsPerWorkgroupParameter();
                case ExecutionMode.SubgroupsPerWorkgroupId:
                    return new SubgroupsPerWorkgroupIdParameter();
                case ExecutionMode.LocalSizeId:
                    return new LocalSizeIdParameter();
                case ExecutionMode.LocalSizeHintId:
                    return new LocalSizeHintIdParameter();
            }

            return null;
        }
    }
    public enum StorageClass : uint
    {
        UniformConstant = 0,
        Input = 1,
        Uniform = 2,
        Output = 3,
        Workgroup = 4,
        CrossWorkgroup = 5,
        Private = 6,
        Function = 7,
        Generic = 8,
        PushConstant = 9,
        AtomicCounter = 10,
        Image = 11,
        StorageBuffer = 12,
    }
    public class StorageClassParameterFactory : ParameterFactory
    {
    }
    public enum Dim : uint
    {
        Dim1D = 0,
        Dim2D = 1,
        Dim3D = 2,
        Cube = 3,
        Rect = 4,
        Buffer = 5,
        SubpassData = 6,
    }
    public class DimParameterFactory : ParameterFactory
    {
    }
    public enum SamplerAddressingMode : uint
    {
        None = 0,
        ClampToEdge = 1,
        Clamp = 2,
        Repeat = 3,
        RepeatMirrored = 4,
    }
    public class SamplerAddressingModeParameterFactory : ParameterFactory
    {
    }
    public enum SamplerFilterMode : uint
    {
        Nearest = 0,
        Linear = 1,
    }
    public class SamplerFilterModeParameterFactory : ParameterFactory
    {
    }
    public enum ImageFormat : uint
    {
        Unknown = 0,
        Rgba32f = 1,
        Rgba16f = 2,
        R32f = 3,
        Rgba8 = 4,
        Rgba8Snorm = 5,
        Rg32f = 6,
        Rg16f = 7,
        R11fG11fB10f = 8,
        R16f = 9,
        Rgba16 = 10,
        Rgb10A2 = 11,
        Rg16 = 12,
        Rg8 = 13,
        R16 = 14,
        R8 = 15,
        Rgba16Snorm = 16,
        Rg16Snorm = 17,
        Rg8Snorm = 18,
        R16Snorm = 19,
        R8Snorm = 20,
        Rgba32i = 21,
        Rgba16i = 22,
        Rgba8i = 23,
        R32i = 24,
        Rg32i = 25,
        Rg16i = 26,
        Rg8i = 27,
        R16i = 28,
        R8i = 29,
        Rgba32ui = 30,
        Rgba16ui = 31,
        Rgba8ui = 32,
        R32ui = 33,
        Rgb10a2ui = 34,
        Rg32ui = 35,
        Rg16ui = 36,
        Rg8ui = 37,
        R16ui = 38,
        R8ui = 39,
    }
    public class ImageFormatParameterFactory : ParameterFactory
    {
    }
    public enum ImageChannelOrder : uint
    {
        R = 0,
        A = 1,
        RG = 2,
        RA = 3,
        RGB = 4,
        RGBA = 5,
        BGRA = 6,
        ARGB = 7,
        Intensity = 8,
        Luminance = 9,
        Rx = 10,
        RGx = 11,
        RGBx = 12,
        Depth = 13,
        DepthStencil = 14,
        sRGB = 15,
        sRGBx = 16,
        sRGBA = 17,
        sBGRA = 18,
        ABGR = 19,
    }
    public class ImageChannelOrderParameterFactory : ParameterFactory
    {
    }
    public enum ImageChannelDataType : uint
    {
        SnormInt8 = 0,
        SnormInt16 = 1,
        UnormInt8 = 2,
        UnormInt16 = 3,
        UnormShort565 = 4,
        UnormShort555 = 5,
        UnormInt101010 = 6,
        SignedInt8 = 7,
        SignedInt16 = 8,
        SignedInt32 = 9,
        UnsignedInt8 = 10,
        UnsignedInt16 = 11,
        UnsignedInt32 = 12,
        HalfFloat = 13,
        Float = 14,
        UnormInt24 = 15,
        UnormInt101010_2 = 16,
    }
    public class ImageChannelDataTypeParameterFactory : ParameterFactory
    {
    }
    public enum FPRoundingMode : uint
    {
        RTE = 0,
        RTZ = 1,
        RTP = 2,
        RTN = 3,
    }
    public class FPRoundingModeParameterFactory : ParameterFactory
    {
    }
    public enum LinkageType : uint
    {
        Export = 0,
        Import = 1,
    }
    public class LinkageTypeParameterFactory : ParameterFactory
    {
    }
    public enum AccessQualifier : uint
    {
        ReadOnly = 0,
        WriteOnly = 1,
        ReadWrite = 2,
    }
    public class AccessQualifierParameterFactory : ParameterFactory
    {
    }
    public enum FunctionParameterAttribute : uint
    {
        Zext = 0,
        Sext = 1,
        ByVal = 2,
        Sret = 3,
        NoAlias = 4,
        NoCapture = 5,
        NoWrite = 6,
        NoReadWrite = 7,
    }
    public class FunctionParameterAttributeParameterFactory : ParameterFactory
    {
    }
    public enum Decoration : uint
    {
        RelaxedPrecision = 0,
        SpecId = 1,
        Block = 2,
        BufferBlock = 3,
        RowMajor = 4,
        ColMajor = 5,
        ArrayStride = 6,
        MatrixStride = 7,
        GLSLShared = 8,
        GLSLPacked = 9,
        CPacked = 10,
        BuiltIn = 11,
        NoPerspective = 13,
        Flat = 14,
        Patch = 15,
        Centroid = 16,
        Sample = 17,
        Invariant = 18,
        Restrict = 19,
        Aliased = 20,
        Volatile = 21,
        Constant = 22,
        Coherent = 23,
        NonWritable = 24,
        NonReadable = 25,
        Uniform = 26,
        SaturatedConversion = 28,
        Stream = 29,
        Location = 30,
        Component = 31,
        Index = 32,
        Binding = 33,
        DescriptorSet = 34,
        Offset = 35,
        XfbBuffer = 36,
        XfbStride = 37,
        FuncParamAttr = 38,
        FPRoundingMode = 39,
        FPFastMathMode = 40,
        LinkageAttributes = 41,
        NoContraction = 42,
        InputAttachmentIndex = 43,
        Alignment = 44,
        MaxByteOffset = 45,
        AlignmentId = 46,
        MaxByteOffsetId = 47,
        ExplicitInterpAMD = 4999,
        OverrideCoverageNV = 5248,
        PassthroughNV = 5250,
        ViewportRelativeNV = 5252,
        SecondaryViewportRelativeNV = 5256,
    }
    public class DecorationParameterFactory : ParameterFactory
    {
        public class SpecIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class ArrayStrideParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class MatrixStrideParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class BuiltInParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new EnumType<BuiltIn>(), };
        }

        public class StreamParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class LocationParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class ComponentParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class IndexParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class BindingParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class DescriptorSetParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class OffsetParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class XfbBufferParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class XfbStrideParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class FuncParamAttrParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new EnumType<FunctionParameterAttribute>(), };
        }

        public class FPRoundingModeParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new EnumType<FPRoundingMode>(), };
        }

        public class FPFastMathModeParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new EnumType<FPFastMathMode>(), };
        }

        public class LinkageAttributesParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralString(), new EnumType<LinkageType>(), };
        }

        public class InputAttachmentIndexParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class AlignmentParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class MaxByteOffsetParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public class AlignmentIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class MaxByteOffsetIdParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new IdRef(), };
        }

        public class SecondaryViewportRelativeNVParameter : Parameter
        {
            public override IReadOnlyList<OperandType> OperandTypes
            {
                get => operandTypes_;
            }

            private static readonly List<OperandType> operandTypes_ = new List<OperandType>()
        {new LiteralInteger(), };
        }

        public override Parameter CreateParameter(object value)
        {
            switch ((Decoration)value)
            {
                case Decoration.SpecId:
                    return new SpecIdParameter();
                case Decoration.ArrayStride:
                    return new ArrayStrideParameter();
                case Decoration.MatrixStride:
                    return new MatrixStrideParameter();
                case Decoration.BuiltIn:
                    return new BuiltInParameter();
                case Decoration.Stream:
                    return new StreamParameter();
                case Decoration.Location:
                    return new LocationParameter();
                case Decoration.Component:
                    return new ComponentParameter();
                case Decoration.Index:
                    return new IndexParameter();
                case Decoration.Binding:
                    return new BindingParameter();
                case Decoration.DescriptorSet:
                    return new DescriptorSetParameter();
                case Decoration.Offset:
                    return new OffsetParameter();
                case Decoration.XfbBuffer:
                    return new XfbBufferParameter();
                case Decoration.XfbStride:
                    return new XfbStrideParameter();
                case Decoration.FuncParamAttr:
                    return new FuncParamAttrParameter();
                case Decoration.FPRoundingMode:
                    return new FPRoundingModeParameter();
                case Decoration.FPFastMathMode:
                    return new FPFastMathModeParameter();
                case Decoration.LinkageAttributes:
                    return new LinkageAttributesParameter();
                case Decoration.InputAttachmentIndex:
                    return new InputAttachmentIndexParameter();
                case Decoration.Alignment:
                    return new AlignmentParameter();
                case Decoration.MaxByteOffset:
                    return new MaxByteOffsetParameter();
                case Decoration.AlignmentId:
                    return new AlignmentIdParameter();
                case Decoration.MaxByteOffsetId:
                    return new MaxByteOffsetIdParameter();
                case Decoration.SecondaryViewportRelativeNV:
                    return new SecondaryViewportRelativeNVParameter();
            }

            return null;
        }
    }
    public enum BuiltIn : uint
    {
        Position = 0,
        PointSize = 1,
        ClipDistance = 3,
        CullDistance = 4,
        VertexId = 5,
        InstanceId = 6,
        PrimitiveId = 7,
        InvocationId = 8,
        Layer = 9,
        ViewportIndex = 10,
        TessLevelOuter = 11,
        TessLevelInner = 12,
        TessCoord = 13,
        PatchVertices = 14,
        FragCoord = 15,
        PointCoord = 16,
        FrontFacing = 17,
        SampleId = 18,
        SamplePosition = 19,
        SampleMask = 20,
        FragDepth = 22,
        HelperInvocation = 23,
        NumWorkgroups = 24,
        WorkgroupSize = 25,
        WorkgroupId = 26,
        LocalInvocationId = 27,
        GlobalInvocationId = 28,
        LocalInvocationIndex = 29,
        WorkDim = 30,
        GlobalSize = 31,
        EnqueuedWorkgroupSize = 32,
        GlobalOffset = 33,
        GlobalLinearId = 34,
        SubgroupSize = 36,
        SubgroupMaxSize = 37,
        NumSubgroups = 38,
        NumEnqueuedSubgroups = 39,
        SubgroupId = 40,
        SubgroupLocalInvocationId = 41,
        VertexIndex = 42,
        InstanceIndex = 43,
        SubgroupEqMaskKHR = 4416,
        SubgroupGeMaskKHR = 4417,
        SubgroupGtMaskKHR = 4418,
        SubgroupLeMaskKHR = 4419,
        SubgroupLtMaskKHR = 4420,
        BaseVertex = 4424,
        BaseInstance = 4425,
        DrawIndex = 4426,
        DeviceIndex = 4438,
        ViewIndex = 4440,
        BaryCoordNoPerspAMD = 4992,
        BaryCoordNoPerspCentroidAMD = 4993,
        BaryCoordNoPerspSampleAMD = 4994,
        BaryCoordSmoothAMD = 4995,
        BaryCoordSmoothCentroidAMD = 4996,
        BaryCoordSmoothSampleAMD = 4997,
        BaryCoordPullModelAMD = 4998,
        FragStencilRefEXT = 5014,
        ViewportMaskNV = 5253,
        SecondaryPositionNV = 5257,
        SecondaryViewportMaskNV = 5258,
        PositionPerViewNV = 5261,
        ViewportMaskPerViewNV = 5262,
    }
    public class BuiltInParameterFactory : ParameterFactory
    {
    }
    public enum Scope : uint
    {
        CrossDevice = 0,
        Device = 1,
        Workgroup = 2,
        Subgroup = 3,
        Invocation = 4,
    }
    public class ScopeParameterFactory : ParameterFactory
    {
    }
    public enum GroupOperation : uint
    {
        Reduce = 0,
        InclusiveScan = 1,
        ExclusiveScan = 2,
    }
    public class GroupOperationParameterFactory : ParameterFactory
    {
    }
    public enum KernelEnqueueFlags : uint
    {
        NoWait = 0,
        WaitKernel = 1,
        WaitWorkGroup = 2,
    }
    public class KernelEnqueueFlagsParameterFactory : ParameterFactory
    {
    }
    public enum Capability : uint
    {
        Matrix = 0,
        Shader = 1,
        Geometry = 2,
        Tessellation = 3,
        Addresses = 4,
        Linkage = 5,
        Kernel = 6,
        Vector16 = 7,
        Float16Buffer = 8,
        Float16 = 9,
        Float64 = 10,
        Int64 = 11,
        Int64Atomics = 12,
        ImageBasic = 13,
        ImageReadWrite = 14,
        ImageMipmap = 15,
        Pipes = 17,
        Groups = 18,
        DeviceEnqueue = 19,
        LiteralSampler = 20,
        AtomicStorage = 21,
        Int16 = 22,
        TessellationPointSize = 23,
        GeometryPointSize = 24,
        ImageGatherExtended = 25,
        StorageImageMultisample = 27,
        UniformBufferArrayDynamicIndexing = 28,
        SampledImageArrayDynamicIndexing = 29,
        StorageBufferArrayDynamicIndexing = 30,
        StorageImageArrayDynamicIndexing = 31,
        ClipDistance = 32,
        CullDistance = 33,
        ImageCubeArray = 34,
        SampleRateShading = 35,
        ImageRect = 36,
        SampledRect = 37,
        GenericPointer = 38,
        Int8 = 39,
        InputAttachment = 40,
        SparseResidency = 41,
        MinLod = 42,
        Sampled1D = 43,
        Image1D = 44,
        SampledCubeArray = 45,
        SampledBuffer = 46,
        ImageBuffer = 47,
        ImageMSArray = 48,
        StorageImageExtendedFormats = 49,
        ImageQuery = 50,
        DerivativeControl = 51,
        InterpolationFunction = 52,
        TransformFeedback = 53,
        GeometryStreams = 54,
        StorageImageReadWithoutFormat = 55,
        StorageImageWriteWithoutFormat = 56,
        MultiViewport = 57,
        SubgroupDispatch = 58,
        NamedBarrier = 59,
        PipeStorage = 60,
        SubgroupBallotKHR = 4423,
        DrawParameters = 4427,
        SubgroupVoteKHR = 4431,
        StorageBuffer16BitAccess = 4433,
        StorageUniformBufferBlock16 = 4433,
        UniformAndStorageBuffer16BitAccess = 4434,
        StorageUniform16 = 4434,
        StoragePushConstant16 = 4435,
        StorageInputOutput16 = 4436,
        DeviceGroup = 4437,
        MultiView = 4439,
        VariablePointersStorageBuffer = 4441,
        VariablePointers = 4442,
        AtomicStorageOps = 4445,
        SampleMaskPostDepthCoverage = 4447,
        ImageGatherBiasLodAMD = 5009,
        FragmentMaskAMD = 5010,
        StencilExportEXT = 5013,
        ImageReadWriteLodAMD = 5015,
        SampleMaskOverrideCoverageNV = 5249,
        GeometryShaderPassthroughNV = 5251,
        ShaderViewportIndexLayerEXT = 5254,
        ShaderViewportIndexLayerNV = 5254,
        ShaderViewportMaskNV = 5255,
        ShaderStereoViewNV = 5259,
        PerViewAttributesNV = 5260,
        SubgroupShuffleINTEL = 5568,
        SubgroupBufferBlockIOINTEL = 5569,
        SubgroupImageBlockIOINTEL = 5570,
    }
    public class CapabilityParameterFactory : ParameterFactory
    {
    }
    public class OpNop : Instruction
    {
        public OpNop() : base("OpNop")
        {
        }
    }
    public class OpUndef : Instruction
    {
        public OpUndef() : base("OpUndef", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSourceContinued : Instruction
    {
        public OpSourceContinued() : base("OpSourceContinued", new List<Operand>()
    {new Operand(new LiteralString(), "Continued Source", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSource : Instruction
    {
        public OpSource() : base("OpSource", new List<Operand>()
    {new Operand(new EnumType<SourceLanguage, SourceLanguageParameterFactory>(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Version", OperandQuantifier.Default), new Operand(new IdRef(), "File", OperandQuantifier.Optional), new Operand(new LiteralString(), "Source", OperandQuantifier.Optional), })
        {
        }
    }
    public class OpSourceExtension : Instruction
    {
        public OpSourceExtension() : base("OpSourceExtension", new List<Operand>()
    {new Operand(new LiteralString(), "Extension", OperandQuantifier.Default), })
        {
        }
    }
    public class OpName : Instruction
    {
        public OpName() : base("OpName", new List<Operand>()
    {new Operand(new IdRef(), "Target", OperandQuantifier.Default), new Operand(new LiteralString(), "Name", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMemberName : Instruction
    {
        public OpMemberName() : base("OpMemberName", new List<Operand>()
    {new Operand(new IdRef(), "Type", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Member", OperandQuantifier.Default), new Operand(new LiteralString(), "Name", OperandQuantifier.Default), })
        {
        }
    }
    public class OpString : Instruction
    {
        public OpString() : base("OpString", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralString(), "String", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLine : Instruction
    {
        public OpLine() : base("OpLine", new List<Operand>()
    {new Operand(new IdRef(), "File", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Line", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Column", OperandQuantifier.Default), })
        {
        }
    }
    public class OpExtension : Instruction
    {
        public OpExtension() : base("OpExtension", new List<Operand>()
    {new Operand(new LiteralString(), "Name", OperandQuantifier.Default), })
        {
        }
    }
    public class OpExtInstImport : Instruction
    {
        public OpExtInstImport() : base("OpExtInstImport", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralString(), "Name", OperandQuantifier.Default), })
        {
        }
    }
    public class OpExtInst : Instruction
    {
        public OpExtInst() : base("OpExtInst", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Set", OperandQuantifier.Default), new Operand(new LiteralExtInstInteger(), "Instruction", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1, +Operand 2, +...", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpMemoryModel : Instruction
    {
        public OpMemoryModel() : base("OpMemoryModel", new List<Operand>()
    {new Operand(new EnumType<AddressingModel, AddressingModelParameterFactory>(), null, OperandQuantifier.Default), new Operand(new EnumType<MemoryModel, MemoryModelParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpEntryPoint : Instruction
    {
        public OpEntryPoint() : base("OpEntryPoint", new List<Operand>()
    {new Operand(new EnumType<ExecutionModel, ExecutionModelParameterFactory>(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Entry Point", OperandQuantifier.Default), new Operand(new LiteralString(), "Name", OperandQuantifier.Default), new Operand(new IdRef(), "Interface", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpExecutionMode : Instruction
    {
        public OpExecutionMode() : base("OpExecutionMode", new List<Operand>()
    {new Operand(new IdRef(), "Entry Point", OperandQuantifier.Default), new Operand(new EnumType<ExecutionMode, ExecutionModeParameterFactory>(), "Mode", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCapability : Instruction
    {
        public OpCapability() : base("OpCapability", new List<Operand>()
    {new Operand(new EnumType<Capability, CapabilityParameterFactory>(), "Capability", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeVoid : Instruction
    {
        public OpTypeVoid() : base("OpTypeVoid", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeBool : Instruction
    {
        public OpTypeBool() : base("OpTypeBool", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeInt : Instruction
    {
        public OpTypeInt() : base("OpTypeInt", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Width", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Signedness", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeFloat : Instruction
    {
        public OpTypeFloat() : base("OpTypeFloat", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Width", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeVector : Instruction
    {
        public OpTypeVector() : base("OpTypeVector", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Component Type", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Component Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeMatrix : Instruction
    {
        public OpTypeMatrix() : base("OpTypeMatrix", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Column Type", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Column Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeImage : Instruction
    {
        public OpTypeImage() : base("OpTypeImage", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Type", OperandQuantifier.Default), new Operand(new EnumType<Dim, DimParameterFactory>(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Depth", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Arrayed", OperandQuantifier.Default), new Operand(new LiteralInteger(), "MS", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Sampled", OperandQuantifier.Default), new Operand(new EnumType<ImageFormat, ImageFormatParameterFactory>(), null, OperandQuantifier.Default), new Operand(new EnumType<AccessQualifier, AccessQualifierParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpTypeSampler : Instruction
    {
        public OpTypeSampler() : base("OpTypeSampler", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeSampledImage : Instruction
    {
        public OpTypeSampledImage() : base("OpTypeSampledImage", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image Type", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeArray : Instruction
    {
        public OpTypeArray() : base("OpTypeArray", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Element Type", OperandQuantifier.Default), new Operand(new IdRef(), "Length", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeRuntimeArray : Instruction
    {
        public OpTypeRuntimeArray() : base("OpTypeRuntimeArray", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Element Type", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeStruct : Instruction
    {
        public OpTypeStruct() : base("OpTypeStruct", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Member 0 type, +member 1 type, +...", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpTypeOpaque : Instruction
    {
        public OpTypeOpaque() : base("OpTypeOpaque", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralString(), "The name of the opaque type.", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypePointer : Instruction
    {
        public OpTypePointer() : base("OpTypePointer", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new EnumType<StorageClass, StorageClassParameterFactory>(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Type", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeFunction : Instruction
    {
        public OpTypeFunction() : base("OpTypeFunction", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Return Type", OperandQuantifier.Default), new Operand(new IdRef(), "Parameter 0 Type, +Parameter 1 Type, +...", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpTypeEvent : Instruction
    {
        public OpTypeEvent() : base("OpTypeEvent", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeDeviceEvent : Instruction
    {
        public OpTypeDeviceEvent() : base("OpTypeDeviceEvent", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeReserveId : Instruction
    {
        public OpTypeReserveId() : base("OpTypeReserveId", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeQueue : Instruction
    {
        public OpTypeQueue() : base("OpTypeQueue", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypePipe : Instruction
    {
        public OpTypePipe() : base("OpTypePipe", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new EnumType<AccessQualifier, AccessQualifierParameterFactory>(), "Qualifier", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeForwardPointer : Instruction
    {
        public OpTypeForwardPointer() : base("OpTypeForwardPointer", new List<Operand>()
    {new Operand(new IdRef(), "Pointer Type", OperandQuantifier.Default), new Operand(new EnumType<StorageClass, StorageClassParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstantTrue : Instruction
    {
        public OpConstantTrue() : base("OpConstantTrue", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstantFalse : Instruction
    {
        public OpConstantFalse() : base("OpConstantFalse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstant : Instruction
    {
        public OpConstant() : base("OpConstant", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralContextDependentNumber(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstantComposite : Instruction
    {
        public OpConstantComposite() : base("OpConstantComposite", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Constituents", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpConstantSampler : Instruction
    {
        public OpConstantSampler() : base("OpConstantSampler", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new EnumType<SamplerAddressingMode, SamplerAddressingModeParameterFactory>(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Param", OperandQuantifier.Default), new Operand(new EnumType<SamplerFilterMode, SamplerFilterModeParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstantNull : Instruction
    {
        public OpConstantNull() : base("OpConstantNull", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSpecConstantTrue : Instruction
    {
        public OpSpecConstantTrue() : base("OpSpecConstantTrue", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSpecConstantFalse : Instruction
    {
        public OpSpecConstantFalse() : base("OpSpecConstantFalse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSpecConstant : Instruction
    {
        public OpSpecConstant() : base("OpSpecConstant", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralContextDependentNumber(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSpecConstantComposite : Instruction
    {
        public OpSpecConstantComposite() : base("OpSpecConstantComposite", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Constituents", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpSpecConstantOp : Instruction
    {
        public OpSpecConstantOp() : base("OpSpecConstantOp", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralSpecConstantOpInteger(), "Opcode", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFunction : Instruction
    {
        public OpFunction() : base("OpFunction", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new EnumType<FunctionControl, FunctionControlParameterFactory>(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Function Type", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFunctionParameter : Instruction
    {
        public OpFunctionParameter() : base("OpFunctionParameter", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpFunctionEnd : Instruction
    {
        public OpFunctionEnd() : base("OpFunctionEnd")
        {
        }
    }
    public class OpFunctionCall : Instruction
    {
        public OpFunctionCall() : base("OpFunctionCall", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Function", OperandQuantifier.Default), new Operand(new IdRef(), "Argument 0, +Argument 1, +...", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpVariable : Instruction
    {
        public OpVariable() : base("OpVariable", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new EnumType<StorageClass, StorageClassParameterFactory>(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Initializer", OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageTexelPointer : Instruction
    {
        public OpImageTexelPointer() : base("OpImageTexelPointer", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Sample", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLoad : Instruction
    {
        public OpLoad() : base("OpLoad", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new EnumType<MemoryAccess, MemoryAccessParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpStore : Instruction
    {
        public OpStore() : base("OpStore", new List<Operand>()
    {new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdRef(), "Object", OperandQuantifier.Default), new Operand(new EnumType<MemoryAccess, MemoryAccessParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpCopyMemory : Instruction
    {
        public OpCopyMemory() : base("OpCopyMemory", new List<Operand>()
    {new Operand(new IdRef(), "Target", OperandQuantifier.Default), new Operand(new IdRef(), "Source", OperandQuantifier.Default), new Operand(new EnumType<MemoryAccess, MemoryAccessParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpCopyMemorySized : Instruction
    {
        public OpCopyMemorySized() : base("OpCopyMemorySized", new List<Operand>()
    {new Operand(new IdRef(), "Target", OperandQuantifier.Default), new Operand(new IdRef(), "Source", OperandQuantifier.Default), new Operand(new IdRef(), "Size", OperandQuantifier.Default), new Operand(new EnumType<MemoryAccess, MemoryAccessParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpAccessChain : Instruction
    {
        public OpAccessChain() : base("OpAccessChain", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpInBoundsAccessChain : Instruction
    {
        public OpInBoundsAccessChain() : base("OpInBoundsAccessChain", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpPtrAccessChain : Instruction
    {
        public OpPtrAccessChain() : base("OpPtrAccessChain", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Element", OperandQuantifier.Default), new Operand(new IdRef(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpArrayLength : Instruction
    {
        public OpArrayLength() : base("OpArrayLength", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Structure", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Array member", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGenericPtrMemSemantics : Instruction
    {
        public OpGenericPtrMemSemantics() : base("OpGenericPtrMemSemantics", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), })
        {
        }
    }
    public class OpInBoundsPtrAccessChain : Instruction
    {
        public OpInBoundsPtrAccessChain() : base("OpInBoundsPtrAccessChain", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Element", OperandQuantifier.Default), new Operand(new IdRef(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpDecorate : Instruction
    {
        public OpDecorate() : base("OpDecorate", new List<Operand>()
    {new Operand(new IdRef(), "Target", OperandQuantifier.Default), new Operand(new EnumType<Decoration, DecorationParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpMemberDecorate : Instruction
    {
        public OpMemberDecorate() : base("OpMemberDecorate", new List<Operand>()
    {new Operand(new IdRef(), "Structure Type", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Member", OperandQuantifier.Default), new Operand(new EnumType<Decoration, DecorationParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpDecorationGroup : Instruction
    {
        public OpDecorationGroup() : base("OpDecorationGroup", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupDecorate : Instruction
    {
        public OpGroupDecorate() : base("OpGroupDecorate", new List<Operand>()
    {new Operand(new IdRef(), "Decoration Group", OperandQuantifier.Default), new Operand(new IdRef(), "Targets", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpGroupMemberDecorate : Instruction
    {
        public OpGroupMemberDecorate() : base("OpGroupMemberDecorate", new List<Operand>()
    {new Operand(new IdRef(), "Decoration Group", OperandQuantifier.Default), new Operand(new PairIdRefLiteralInteger(), "Targets", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpVectorExtractDynamic : Instruction
    {
        public OpVectorExtractDynamic() : base("OpVectorExtractDynamic", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), new Operand(new IdRef(), "Index", OperandQuantifier.Default), })
        {
        }
    }
    public class OpVectorInsertDynamic : Instruction
    {
        public OpVectorInsertDynamic() : base("OpVectorInsertDynamic", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), new Operand(new IdRef(), "Component", OperandQuantifier.Default), new Operand(new IdRef(), "Index", OperandQuantifier.Default), })
        {
        }
    }
    public class OpVectorShuffle : Instruction
    {
        public OpVectorShuffle() : base("OpVectorShuffle", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector 1", OperandQuantifier.Default), new Operand(new IdRef(), "Vector 2", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Components", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpCompositeConstruct : Instruction
    {
        public OpCompositeConstruct() : base("OpCompositeConstruct", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Constituents", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpCompositeExtract : Instruction
    {
        public OpCompositeExtract() : base("OpCompositeExtract", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Composite", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpCompositeInsert : Instruction
    {
        public OpCompositeInsert() : base("OpCompositeInsert", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Object", OperandQuantifier.Default), new Operand(new IdRef(), "Composite", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Indexes", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpCopyObject : Instruction
    {
        public OpCopyObject() : base("OpCopyObject", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTranspose : Instruction
    {
        public OpTranspose() : base("OpTranspose", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Matrix", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSampledImage : Instruction
    {
        public OpSampledImage() : base("OpSampledImage", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Sampler", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSampleImplicitLod : Instruction
    {
        public OpImageSampleImplicitLod() : base("OpImageSampleImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSampleExplicitLod : Instruction
    {
        public OpImageSampleExplicitLod() : base("OpImageSampleExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSampleDrefImplicitLod : Instruction
    {
        public OpImageSampleDrefImplicitLod() : base("OpImageSampleDrefImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSampleDrefExplicitLod : Instruction
    {
        public OpImageSampleDrefExplicitLod() : base("OpImageSampleDrefExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSampleProjImplicitLod : Instruction
    {
        public OpImageSampleProjImplicitLod() : base("OpImageSampleProjImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSampleProjExplicitLod : Instruction
    {
        public OpImageSampleProjExplicitLod() : base("OpImageSampleProjExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSampleProjDrefImplicitLod : Instruction
    {
        public OpImageSampleProjDrefImplicitLod() : base("OpImageSampleProjDrefImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSampleProjDrefExplicitLod : Instruction
    {
        public OpImageSampleProjDrefExplicitLod() : base("OpImageSampleProjDrefExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageFetch : Instruction
    {
        public OpImageFetch() : base("OpImageFetch", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageGather : Instruction
    {
        public OpImageGather() : base("OpImageGather", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Component", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageDrefGather : Instruction
    {
        public OpImageDrefGather() : base("OpImageDrefGather", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageRead : Instruction
    {
        public OpImageRead() : base("OpImageRead", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageWrite : Instruction
    {
        public OpImageWrite() : base("OpImageWrite", new List<Operand>()
    {new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Texel", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImage : Instruction
    {
        public OpImage() : base("OpImage", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQueryFormat : Instruction
    {
        public OpImageQueryFormat() : base("OpImageQueryFormat", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQueryOrder : Instruction
    {
        public OpImageQueryOrder() : base("OpImageQueryOrder", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQuerySizeLod : Instruction
    {
        public OpImageQuerySizeLod() : base("OpImageQuerySizeLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Level of Detail", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQuerySize : Instruction
    {
        public OpImageQuerySize() : base("OpImageQuerySize", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQueryLod : Instruction
    {
        public OpImageQueryLod() : base("OpImageQueryLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQueryLevels : Instruction
    {
        public OpImageQueryLevels() : base("OpImageQueryLevels", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageQuerySamples : Instruction
    {
        public OpImageQuerySamples() : base("OpImageQuerySamples", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertFToU : Instruction
    {
        public OpConvertFToU() : base("OpConvertFToU", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Float Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertFToS : Instruction
    {
        public OpConvertFToS() : base("OpConvertFToS", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Float Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertSToF : Instruction
    {
        public OpConvertSToF() : base("OpConvertSToF", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Signed Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertUToF : Instruction
    {
        public OpConvertUToF() : base("OpConvertUToF", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Unsigned Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUConvert : Instruction
    {
        public OpUConvert() : base("OpUConvert", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Unsigned Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSConvert : Instruction
    {
        public OpSConvert() : base("OpSConvert", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Signed Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFConvert : Instruction
    {
        public OpFConvert() : base("OpFConvert", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Float Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpQuantizeToF16 : Instruction
    {
        public OpQuantizeToF16() : base("OpQuantizeToF16", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertPtrToU : Instruction
    {
        public OpConvertPtrToU() : base("OpConvertPtrToU", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSatConvertSToU : Instruction
    {
        public OpSatConvertSToU() : base("OpSatConvertSToU", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Signed Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSatConvertUToS : Instruction
    {
        public OpSatConvertUToS() : base("OpSatConvertUToS", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Unsigned Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpConvertUToPtr : Instruction
    {
        public OpConvertUToPtr() : base("OpConvertUToPtr", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Integer Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpPtrCastToGeneric : Instruction
    {
        public OpPtrCastToGeneric() : base("OpPtrCastToGeneric", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGenericCastToPtr : Instruction
    {
        public OpGenericCastToPtr() : base("OpGenericCastToPtr", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGenericCastToPtrExplicit : Instruction
    {
        public OpGenericCastToPtrExplicit() : base("OpGenericCastToPtrExplicit", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new EnumType<StorageClass, StorageClassParameterFactory>(), "Storage", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitcast : Instruction
    {
        public OpBitcast() : base("OpBitcast", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSNegate : Instruction
    {
        public OpSNegate() : base("OpSNegate", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFNegate : Instruction
    {
        public OpFNegate() : base("OpFNegate", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIAdd : Instruction
    {
        public OpIAdd() : base("OpIAdd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFAdd : Instruction
    {
        public OpFAdd() : base("OpFAdd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpISub : Instruction
    {
        public OpISub() : base("OpISub", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFSub : Instruction
    {
        public OpFSub() : base("OpFSub", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIMul : Instruction
    {
        public OpIMul() : base("OpIMul", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFMul : Instruction
    {
        public OpFMul() : base("OpFMul", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUDiv : Instruction
    {
        public OpUDiv() : base("OpUDiv", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSDiv : Instruction
    {
        public OpSDiv() : base("OpSDiv", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFDiv : Instruction
    {
        public OpFDiv() : base("OpFDiv", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUMod : Instruction
    {
        public OpUMod() : base("OpUMod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSRem : Instruction
    {
        public OpSRem() : base("OpSRem", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSMod : Instruction
    {
        public OpSMod() : base("OpSMod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFRem : Instruction
    {
        public OpFRem() : base("OpFRem", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFMod : Instruction
    {
        public OpFMod() : base("OpFMod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpVectorTimesScalar : Instruction
    {
        public OpVectorTimesScalar() : base("OpVectorTimesScalar", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), new Operand(new IdRef(), "Scalar", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMatrixTimesScalar : Instruction
    {
        public OpMatrixTimesScalar() : base("OpMatrixTimesScalar", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Matrix", OperandQuantifier.Default), new Operand(new IdRef(), "Scalar", OperandQuantifier.Default), })
        {
        }
    }
    public class OpVectorTimesMatrix : Instruction
    {
        public OpVectorTimesMatrix() : base("OpVectorTimesMatrix", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), new Operand(new IdRef(), "Matrix", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMatrixTimesVector : Instruction
    {
        public OpMatrixTimesVector() : base("OpMatrixTimesVector", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Matrix", OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMatrixTimesMatrix : Instruction
    {
        public OpMatrixTimesMatrix() : base("OpMatrixTimesMatrix", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "LeftMatrix", OperandQuantifier.Default), new Operand(new IdRef(), "RightMatrix", OperandQuantifier.Default), })
        {
        }
    }
    public class OpOuterProduct : Instruction
    {
        public OpOuterProduct() : base("OpOuterProduct", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector 1", OperandQuantifier.Default), new Operand(new IdRef(), "Vector 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDot : Instruction
    {
        public OpDot() : base("OpDot", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector 1", OperandQuantifier.Default), new Operand(new IdRef(), "Vector 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIAddCarry : Instruction
    {
        public OpIAddCarry() : base("OpIAddCarry", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpISubBorrow : Instruction
    {
        public OpISubBorrow() : base("OpISubBorrow", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUMulExtended : Instruction
    {
        public OpUMulExtended() : base("OpUMulExtended", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSMulExtended : Instruction
    {
        public OpSMulExtended() : base("OpSMulExtended", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAny : Instruction
    {
        public OpAny() : base("OpAny", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAll : Instruction
    {
        public OpAll() : base("OpAll", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Vector", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsNan : Instruction
    {
        public OpIsNan() : base("OpIsNan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsInf : Instruction
    {
        public OpIsInf() : base("OpIsInf", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsFinite : Instruction
    {
        public OpIsFinite() : base("OpIsFinite", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsNormal : Instruction
    {
        public OpIsNormal() : base("OpIsNormal", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSignBitSet : Instruction
    {
        public OpSignBitSet() : base("OpSignBitSet", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLessOrGreater : Instruction
    {
        public OpLessOrGreater() : base("OpLessOrGreater", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), new Operand(new IdRef(), "y", OperandQuantifier.Default), })
        {
        }
    }
    public class OpOrdered : Instruction
    {
        public OpOrdered() : base("OpOrdered", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), new Operand(new IdRef(), "y", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUnordered : Instruction
    {
        public OpUnordered() : base("OpUnordered", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "x", OperandQuantifier.Default), new Operand(new IdRef(), "y", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLogicalEqual : Instruction
    {
        public OpLogicalEqual() : base("OpLogicalEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLogicalNotEqual : Instruction
    {
        public OpLogicalNotEqual() : base("OpLogicalNotEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLogicalOr : Instruction
    {
        public OpLogicalOr() : base("OpLogicalOr", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLogicalAnd : Instruction
    {
        public OpLogicalAnd() : base("OpLogicalAnd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLogicalNot : Instruction
    {
        public OpLogicalNot() : base("OpLogicalNot", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSelect : Instruction
    {
        public OpSelect() : base("OpSelect", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Condition", OperandQuantifier.Default), new Operand(new IdRef(), "Object 1", OperandQuantifier.Default), new Operand(new IdRef(), "Object 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIEqual : Instruction
    {
        public OpIEqual() : base("OpIEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpINotEqual : Instruction
    {
        public OpINotEqual() : base("OpINotEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUGreaterThan : Instruction
    {
        public OpUGreaterThan() : base("OpUGreaterThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSGreaterThan : Instruction
    {
        public OpSGreaterThan() : base("OpSGreaterThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUGreaterThanEqual : Instruction
    {
        public OpUGreaterThanEqual() : base("OpUGreaterThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSGreaterThanEqual : Instruction
    {
        public OpSGreaterThanEqual() : base("OpSGreaterThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpULessThan : Instruction
    {
        public OpULessThan() : base("OpULessThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSLessThan : Instruction
    {
        public OpSLessThan() : base("OpSLessThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpULessThanEqual : Instruction
    {
        public OpULessThanEqual() : base("OpULessThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSLessThanEqual : Instruction
    {
        public OpSLessThanEqual() : base("OpSLessThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdEqual : Instruction
    {
        public OpFOrdEqual() : base("OpFOrdEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordEqual : Instruction
    {
        public OpFUnordEqual() : base("OpFUnordEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdNotEqual : Instruction
    {
        public OpFOrdNotEqual() : base("OpFOrdNotEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordNotEqual : Instruction
    {
        public OpFUnordNotEqual() : base("OpFUnordNotEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdLessThan : Instruction
    {
        public OpFOrdLessThan() : base("OpFOrdLessThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordLessThan : Instruction
    {
        public OpFUnordLessThan() : base("OpFUnordLessThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdGreaterThan : Instruction
    {
        public OpFOrdGreaterThan() : base("OpFOrdGreaterThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordGreaterThan : Instruction
    {
        public OpFUnordGreaterThan() : base("OpFUnordGreaterThan", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdLessThanEqual : Instruction
    {
        public OpFOrdLessThanEqual() : base("OpFOrdLessThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordLessThanEqual : Instruction
    {
        public OpFUnordLessThanEqual() : base("OpFUnordLessThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFOrdGreaterThanEqual : Instruction
    {
        public OpFOrdGreaterThanEqual() : base("OpFOrdGreaterThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFUnordGreaterThanEqual : Instruction
    {
        public OpFUnordGreaterThanEqual() : base("OpFUnordGreaterThanEqual", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpShiftRightLogical : Instruction
    {
        public OpShiftRightLogical() : base("OpShiftRightLogical", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Shift", OperandQuantifier.Default), })
        {
        }
    }
    public class OpShiftRightArithmetic : Instruction
    {
        public OpShiftRightArithmetic() : base("OpShiftRightArithmetic", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Shift", OperandQuantifier.Default), })
        {
        }
    }
    public class OpShiftLeftLogical : Instruction
    {
        public OpShiftLeftLogical() : base("OpShiftLeftLogical", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Shift", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitwiseOr : Instruction
    {
        public OpBitwiseOr() : base("OpBitwiseOr", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitwiseXor : Instruction
    {
        public OpBitwiseXor() : base("OpBitwiseXor", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitwiseAnd : Instruction
    {
        public OpBitwiseAnd() : base("OpBitwiseAnd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand 1", OperandQuantifier.Default), new Operand(new IdRef(), "Operand 2", OperandQuantifier.Default), })
        {
        }
    }
    public class OpNot : Instruction
    {
        public OpNot() : base("OpNot", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Operand", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitFieldInsert : Instruction
    {
        public OpBitFieldInsert() : base("OpBitFieldInsert", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Insert", OperandQuantifier.Default), new Operand(new IdRef(), "Offset", OperandQuantifier.Default), new Operand(new IdRef(), "Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitFieldSExtract : Instruction
    {
        public OpBitFieldSExtract() : base("OpBitFieldSExtract", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Offset", OperandQuantifier.Default), new Operand(new IdRef(), "Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitFieldUExtract : Instruction
    {
        public OpBitFieldUExtract() : base("OpBitFieldUExtract", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), new Operand(new IdRef(), "Offset", OperandQuantifier.Default), new Operand(new IdRef(), "Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitReverse : Instruction
    {
        public OpBitReverse() : base("OpBitReverse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBitCount : Instruction
    {
        public OpBitCount() : base("OpBitCount", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Base", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdx : Instruction
    {
        public OpDPdx() : base("OpDPdx", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdy : Instruction
    {
        public OpDPdy() : base("OpDPdy", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFwidth : Instruction
    {
        public OpFwidth() : base("OpFwidth", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdxFine : Instruction
    {
        public OpDPdxFine() : base("OpDPdxFine", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdyFine : Instruction
    {
        public OpDPdyFine() : base("OpDPdyFine", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFwidthFine : Instruction
    {
        public OpFwidthFine() : base("OpFwidthFine", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdxCoarse : Instruction
    {
        public OpDPdxCoarse() : base("OpDPdxCoarse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDPdyCoarse : Instruction
    {
        public OpDPdyCoarse() : base("OpDPdyCoarse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFwidthCoarse : Instruction
    {
        public OpFwidthCoarse() : base("OpFwidthCoarse", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "P", OperandQuantifier.Default), })
        {
        }
    }
    public class OpEmitVertex : Instruction
    {
        public OpEmitVertex() : base("OpEmitVertex")
        {
        }
    }
    public class OpEndPrimitive : Instruction
    {
        public OpEndPrimitive() : base("OpEndPrimitive")
        {
        }
    }
    public class OpEmitStreamVertex : Instruction
    {
        public OpEmitStreamVertex() : base("OpEmitStreamVertex", new List<Operand>()
    {new Operand(new IdRef(), "Stream", OperandQuantifier.Default), })
        {
        }
    }
    public class OpEndStreamPrimitive : Instruction
    {
        public OpEndStreamPrimitive() : base("OpEndStreamPrimitive", new List<Operand>()
    {new Operand(new IdRef(), "Stream", OperandQuantifier.Default), })
        {
        }
    }
    public class OpControlBarrier : Instruction
    {
        public OpControlBarrier() : base("OpControlBarrier", new List<Operand>()
    {new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdScope(), "Memory", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMemoryBarrier : Instruction
    {
        public OpMemoryBarrier() : base("OpMemoryBarrier", new List<Operand>()
    {new Operand(new IdScope(), "Memory", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicLoad : Instruction
    {
        public OpAtomicLoad() : base("OpAtomicLoad", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicStore : Instruction
    {
        public OpAtomicStore() : base("OpAtomicStore", new List<Operand>()
    {new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicExchange : Instruction
    {
        public OpAtomicExchange() : base("OpAtomicExchange", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicCompareExchange : Instruction
    {
        public OpAtomicCompareExchange() : base("OpAtomicCompareExchange", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Equal", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Unequal", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), new Operand(new IdRef(), "Comparator", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicCompareExchangeWeak : Instruction
    {
        public OpAtomicCompareExchangeWeak() : base("OpAtomicCompareExchangeWeak", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Equal", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Unequal", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), new Operand(new IdRef(), "Comparator", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicIIncrement : Instruction
    {
        public OpAtomicIIncrement() : base("OpAtomicIIncrement", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicIDecrement : Instruction
    {
        public OpAtomicIDecrement() : base("OpAtomicIDecrement", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicIAdd : Instruction
    {
        public OpAtomicIAdd() : base("OpAtomicIAdd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicISub : Instruction
    {
        public OpAtomicISub() : base("OpAtomicISub", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicSMin : Instruction
    {
        public OpAtomicSMin() : base("OpAtomicSMin", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicUMin : Instruction
    {
        public OpAtomicUMin() : base("OpAtomicUMin", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicSMax : Instruction
    {
        public OpAtomicSMax() : base("OpAtomicSMax", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicUMax : Instruction
    {
        public OpAtomicUMax() : base("OpAtomicUMax", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicAnd : Instruction
    {
        public OpAtomicAnd() : base("OpAtomicAnd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicOr : Instruction
    {
        public OpAtomicOr() : base("OpAtomicOr", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicXor : Instruction
    {
        public OpAtomicXor() : base("OpAtomicXor", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpPhi : Instruction
    {
        public OpPhi() : base("OpPhi", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new PairIdRefIdRef(), "Variable, Parent, ...", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpLoopMerge : Instruction
    {
        public OpLoopMerge() : base("OpLoopMerge", new List<Operand>()
    {new Operand(new IdRef(), "Merge Block", OperandQuantifier.Default), new Operand(new IdRef(), "Continue Target", OperandQuantifier.Default), new Operand(new EnumType<LoopControl, LoopControlParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSelectionMerge : Instruction
    {
        public OpSelectionMerge() : base("OpSelectionMerge", new List<Operand>()
    {new Operand(new IdRef(), "Merge Block", OperandQuantifier.Default), new Operand(new EnumType<SelectionControl, SelectionControlParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpLabel : Instruction
    {
        public OpLabel() : base("OpLabel", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpBranch : Instruction
    {
        public OpBranch() : base("OpBranch", new List<Operand>()
    {new Operand(new IdRef(), "Target Label", OperandQuantifier.Default), })
        {
        }
    }
    public class OpBranchConditional : Instruction
    {
        public OpBranchConditional() : base("OpBranchConditional", new List<Operand>()
    {new Operand(new IdRef(), "Condition", OperandQuantifier.Default), new Operand(new IdRef(), "True Label", OperandQuantifier.Default), new Operand(new IdRef(), "False Label", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Branch weights", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpSwitch : Instruction
    {
        public OpSwitch() : base("OpSwitch", new List<Operand>()
    {new Operand(new IdRef(), "Selector", OperandQuantifier.Default), new Operand(new IdRef(), "Default", OperandQuantifier.Default), new Operand(new PairLiteralIntegerIdRef(), "Target", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpKill : Instruction
    {
        public OpKill() : base("OpKill")
        {
        }
    }
    public class OpReturn : Instruction
    {
        public OpReturn() : base("OpReturn")
        {
        }
    }
    public class OpReturnValue : Instruction
    {
        public OpReturnValue() : base("OpReturnValue", new List<Operand>()
    {new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpUnreachable : Instruction
    {
        public OpUnreachable() : base("OpUnreachable")
        {
        }
    }
    public class OpLifetimeStart : Instruction
    {
        public OpLifetimeStart() : base("OpLifetimeStart", new List<Operand>()
    {new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Size", OperandQuantifier.Default), })
        {
        }
    }
    public class OpLifetimeStop : Instruction
    {
        public OpLifetimeStop() : base("OpLifetimeStop", new List<Operand>()
    {new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Size", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupAsyncCopy : Instruction
    {
        public OpGroupAsyncCopy() : base("OpGroupAsyncCopy", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Destination", OperandQuantifier.Default), new Operand(new IdRef(), "Source", OperandQuantifier.Default), new Operand(new IdRef(), "Num Elements", OperandQuantifier.Default), new Operand(new IdRef(), "Stride", OperandQuantifier.Default), new Operand(new IdRef(), "Event", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupWaitEvents : Instruction
    {
        public OpGroupWaitEvents() : base("OpGroupWaitEvents", new List<Operand>()
    {new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Num Events", OperandQuantifier.Default), new Operand(new IdRef(), "Events List", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupAll : Instruction
    {
        public OpGroupAll() : base("OpGroupAll", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupAny : Instruction
    {
        public OpGroupAny() : base("OpGroupAny", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupBroadcast : Instruction
    {
        public OpGroupBroadcast() : base("OpGroupBroadcast", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), new Operand(new IdRef(), "LocalId", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupIAdd : Instruction
    {
        public OpGroupIAdd() : base("OpGroupIAdd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFAdd : Instruction
    {
        public OpGroupFAdd() : base("OpGroupFAdd", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFMin : Instruction
    {
        public OpGroupFMin() : base("OpGroupFMin", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupUMin : Instruction
    {
        public OpGroupUMin() : base("OpGroupUMin", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupSMin : Instruction
    {
        public OpGroupSMin() : base("OpGroupSMin", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFMax : Instruction
    {
        public OpGroupFMax() : base("OpGroupFMax", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupUMax : Instruction
    {
        public OpGroupUMax() : base("OpGroupUMax", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupSMax : Instruction
    {
        public OpGroupSMax() : base("OpGroupSMax", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReadPipe : Instruction
    {
        public OpReadPipe() : base("OpReadPipe", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpWritePipe : Instruction
    {
        public OpWritePipe() : base("OpWritePipe", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReservedReadPipe : Instruction
    {
        public OpReservedReadPipe() : base("OpReservedReadPipe", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Index", OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReservedWritePipe : Instruction
    {
        public OpReservedWritePipe() : base("OpReservedWritePipe", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Index", OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReserveReadPipePackets : Instruction
    {
        public OpReserveReadPipePackets() : base("OpReserveReadPipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Num Packets", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReserveWritePipePackets : Instruction
    {
        public OpReserveWritePipePackets() : base("OpReserveWritePipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Num Packets", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCommitReadPipe : Instruction
    {
        public OpCommitReadPipe() : base("OpCommitReadPipe", new List<Operand>()
    {new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCommitWritePipe : Instruction
    {
        public OpCommitWritePipe() : base("OpCommitWritePipe", new List<Operand>()
    {new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsValidReserveId : Instruction
    {
        public OpIsValidReserveId() : base("OpIsValidReserveId", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetNumPipePackets : Instruction
    {
        public OpGetNumPipePackets() : base("OpGetNumPipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetMaxPipePackets : Instruction
    {
        public OpGetMaxPipePackets() : base("OpGetMaxPipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupReserveReadPipePackets : Instruction
    {
        public OpGroupReserveReadPipePackets() : base("OpGroupReserveReadPipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Num Packets", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupReserveWritePipePackets : Instruction
    {
        public OpGroupReserveWritePipePackets() : base("OpGroupReserveWritePipePackets", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Num Packets", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupCommitReadPipe : Instruction
    {
        public OpGroupCommitReadPipe() : base("OpGroupCommitReadPipe", new List<Operand>()
    {new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupCommitWritePipe : Instruction
    {
        public OpGroupCommitWritePipe() : base("OpGroupCommitWritePipe", new List<Operand>()
    {new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new IdRef(), "Pipe", OperandQuantifier.Default), new Operand(new IdRef(), "Reserve Id", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Size", OperandQuantifier.Default), new Operand(new IdRef(), "Packet Alignment", OperandQuantifier.Default), })
        {
        }
    }
    public class OpEnqueueMarker : Instruction
    {
        public OpEnqueueMarker() : base("OpEnqueueMarker", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Queue", OperandQuantifier.Default), new Operand(new IdRef(), "Num Events", OperandQuantifier.Default), new Operand(new IdRef(), "Wait Events", OperandQuantifier.Default), new Operand(new IdRef(), "Ret Event", OperandQuantifier.Default), })
        {
        }
    }
    public class OpEnqueueKernel : Instruction
    {
        public OpEnqueueKernel() : base("OpEnqueueKernel", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Queue", OperandQuantifier.Default), new Operand(new IdRef(), "Flags", OperandQuantifier.Default), new Operand(new IdRef(), "ND Range", OperandQuantifier.Default), new Operand(new IdRef(), "Num Events", OperandQuantifier.Default), new Operand(new IdRef(), "Wait Events", OperandQuantifier.Default), new Operand(new IdRef(), "Ret Event", OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), new Operand(new IdRef(), "Local Size", OperandQuantifier.Varying), })
        {
        }
    }
    public class OpGetKernelNDrangeSubGroupCount : Instruction
    {
        public OpGetKernelNDrangeSubGroupCount() : base("OpGetKernelNDrangeSubGroupCount", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "ND Range", OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetKernelNDrangeMaxSubGroupSize : Instruction
    {
        public OpGetKernelNDrangeMaxSubGroupSize() : base("OpGetKernelNDrangeMaxSubGroupSize", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "ND Range", OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetKernelWorkGroupSize : Instruction
    {
        public OpGetKernelWorkGroupSize() : base("OpGetKernelWorkGroupSize", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetKernelPreferredWorkGroupSizeMultiple : Instruction
    {
        public OpGetKernelPreferredWorkGroupSizeMultiple() : base("OpGetKernelPreferredWorkGroupSizeMultiple", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpRetainEvent : Instruction
    {
        public OpRetainEvent() : base("OpRetainEvent", new List<Operand>()
    {new Operand(new IdRef(), "Event", OperandQuantifier.Default), })
        {
        }
    }
    public class OpReleaseEvent : Instruction
    {
        public OpReleaseEvent() : base("OpReleaseEvent", new List<Operand>()
    {new Operand(new IdRef(), "Event", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCreateUserEvent : Instruction
    {
        public OpCreateUserEvent() : base("OpCreateUserEvent", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpIsValidEvent : Instruction
    {
        public OpIsValidEvent() : base("OpIsValidEvent", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Event", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSetUserEventStatus : Instruction
    {
        public OpSetUserEventStatus() : base("OpSetUserEventStatus", new List<Operand>()
    {new Operand(new IdRef(), "Event", OperandQuantifier.Default), new Operand(new IdRef(), "Status", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCaptureEventProfilingInfo : Instruction
    {
        public OpCaptureEventProfilingInfo() : base("OpCaptureEventProfilingInfo", new List<Operand>()
    {new Operand(new IdRef(), "Event", OperandQuantifier.Default), new Operand(new IdRef(), "Profiling Info", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetDefaultQueue : Instruction
    {
        public OpGetDefaultQueue() : base("OpGetDefaultQueue", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpBuildNDRange : Instruction
    {
        public OpBuildNDRange() : base("OpBuildNDRange", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "GlobalWorkSize", OperandQuantifier.Default), new Operand(new IdRef(), "LocalWorkSize", OperandQuantifier.Default), new Operand(new IdRef(), "GlobalWorkOffset", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseSampleImplicitLod : Instruction
    {
        public OpImageSparseSampleImplicitLod() : base("OpImageSparseSampleImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseSampleExplicitLod : Instruction
    {
        public OpImageSparseSampleExplicitLod() : base("OpImageSparseSampleExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseSampleDrefImplicitLod : Instruction
    {
        public OpImageSparseSampleDrefImplicitLod() : base("OpImageSparseSampleDrefImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseSampleDrefExplicitLod : Instruction
    {
        public OpImageSparseSampleDrefExplicitLod() : base("OpImageSparseSampleDrefExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseSampleProjImplicitLod : Instruction
    {
        public OpImageSparseSampleProjImplicitLod() : base("OpImageSparseSampleProjImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseSampleProjExplicitLod : Instruction
    {
        public OpImageSparseSampleProjExplicitLod() : base("OpImageSparseSampleProjExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseSampleProjDrefImplicitLod : Instruction
    {
        public OpImageSparseSampleProjDrefImplicitLod() : base("OpImageSparseSampleProjDrefImplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseSampleProjDrefExplicitLod : Instruction
    {
        public OpImageSparseSampleProjDrefExplicitLod() : base("OpImageSparseSampleProjDrefExplicitLod", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseFetch : Instruction
    {
        public OpImageSparseFetch() : base("OpImageSparseFetch", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseGather : Instruction
    {
        public OpImageSparseGather() : base("OpImageSparseGather", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Component", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseDrefGather : Instruction
    {
        public OpImageSparseDrefGather() : base("OpImageSparseDrefGather", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Sampled Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "D~ref~", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpImageSparseTexelsResident : Instruction
    {
        public OpImageSparseTexelsResident() : base("OpImageSparseTexelsResident", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Resident Code", OperandQuantifier.Default), })
        {
        }
    }
    public class OpNoLine : Instruction
    {
        public OpNoLine() : base("OpNoLine")
        {
        }
    }
    public class OpAtomicFlagTestAndSet : Instruction
    {
        public OpAtomicFlagTestAndSet() : base("OpAtomicFlagTestAndSet", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpAtomicFlagClear : Instruction
    {
        public OpAtomicFlagClear() : base("OpAtomicFlagClear", new List<Operand>()
    {new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), new Operand(new IdScope(), "Scope", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpImageSparseRead : Instruction
    {
        public OpImageSparseRead() : base("OpImageSparseRead", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new EnumType<ImageOperands, ImageOperandsParameterFactory>(), null, OperandQuantifier.Optional), })
        {
        }
    }
    public class OpSizeOf : Instruction
    {
        public OpSizeOf() : base("OpSizeOf", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pointer", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypePipeStorage : Instruction
    {
        public OpTypePipeStorage() : base("OpTypePipeStorage", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpConstantPipeStorage : Instruction
    {
        public OpConstantPipeStorage() : base("OpConstantPipeStorage", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new LiteralInteger(), "Packet Size", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Packet Alignment", OperandQuantifier.Default), new Operand(new LiteralInteger(), "Capacity", OperandQuantifier.Default), })
        {
        }
    }
    public class OpCreatePipeFromPipeStorage : Instruction
    {
        public OpCreatePipeFromPipeStorage() : base("OpCreatePipeFromPipeStorage", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Pipe Storage", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetKernelLocalSizeForSubgroupCount : Instruction
    {
        public OpGetKernelLocalSizeForSubgroupCount() : base("OpGetKernelLocalSizeForSubgroupCount", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Subgroup Count", OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGetKernelMaxNumSubgroups : Instruction
    {
        public OpGetKernelMaxNumSubgroups() : base("OpGetKernelMaxNumSubgroups", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Invoke", OperandQuantifier.Default), new Operand(new IdRef(), "Param", OperandQuantifier.Default), new Operand(new IdRef(), "Param Size", OperandQuantifier.Default), new Operand(new IdRef(), "Param Align", OperandQuantifier.Default), })
        {
        }
    }
    public class OpTypeNamedBarrier : Instruction
    {
        public OpTypeNamedBarrier() : base("OpTypeNamedBarrier", new List<Operand>()
    {new Operand(new IdResult(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpNamedBarrierInitialize : Instruction
    {
        public OpNamedBarrierInitialize() : base("OpNamedBarrierInitialize", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Subgroup Count", OperandQuantifier.Default), })
        {
        }
    }
    public class OpMemoryNamedBarrier : Instruction
    {
        public OpMemoryNamedBarrier() : base("OpMemoryNamedBarrier", new List<Operand>()
    {new Operand(new IdRef(), "Named Barrier", OperandQuantifier.Default), new Operand(new IdScope(), "Memory", OperandQuantifier.Default), new Operand(new IdMemorySemantics(), "Semantics", OperandQuantifier.Default), })
        {
        }
    }
    public class OpModuleProcessed : Instruction
    {
        public OpModuleProcessed() : base("OpModuleProcessed", new List<Operand>()
    {new Operand(new LiteralString(), "Process", OperandQuantifier.Default), })
        {
        }
    }
    public class OpExecutionModeId : Instruction
    {
        public OpExecutionModeId() : base("OpExecutionModeId", new List<Operand>()
    {new Operand(new IdRef(), "Entry Point", OperandQuantifier.Default), new Operand(new EnumType<ExecutionMode, ExecutionModeParameterFactory>(), "Mode", OperandQuantifier.Default), })
        {
        }
    }
    public class OpDecorateId : Instruction
    {
        public OpDecorateId() : base("OpDecorateId", new List<Operand>()
    {new Operand(new IdRef(), "Target", OperandQuantifier.Default), new Operand(new EnumType<Decoration, DecorationParameterFactory>(), null, OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupBallotKHR : Instruction
    {
        public OpSubgroupBallotKHR() : base("OpSubgroupBallotKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupFirstInvocationKHR : Instruction
    {
        public OpSubgroupFirstInvocationKHR() : base("OpSubgroupFirstInvocationKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupAllKHR : Instruction
    {
        public OpSubgroupAllKHR() : base("OpSubgroupAllKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupAnyKHR : Instruction
    {
        public OpSubgroupAnyKHR() : base("OpSubgroupAnyKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupAllEqualKHR : Instruction
    {
        public OpSubgroupAllEqualKHR() : base("OpSubgroupAllEqualKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Predicate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupReadInvocationKHR : Instruction
    {
        public OpSubgroupReadInvocationKHR() : base("OpSubgroupReadInvocationKHR", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), new Operand(new IdRef(), "Index", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupIAddNonUniformAMD : Instruction
    {
        public OpGroupIAddNonUniformAMD() : base("OpGroupIAddNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFAddNonUniformAMD : Instruction
    {
        public OpGroupFAddNonUniformAMD() : base("OpGroupFAddNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFMinNonUniformAMD : Instruction
    {
        public OpGroupFMinNonUniformAMD() : base("OpGroupFMinNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupUMinNonUniformAMD : Instruction
    {
        public OpGroupUMinNonUniformAMD() : base("OpGroupUMinNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupSMinNonUniformAMD : Instruction
    {
        public OpGroupSMinNonUniformAMD() : base("OpGroupSMinNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupFMaxNonUniformAMD : Instruction
    {
        public OpGroupFMaxNonUniformAMD() : base("OpGroupFMaxNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupUMaxNonUniformAMD : Instruction
    {
        public OpGroupUMaxNonUniformAMD() : base("OpGroupUMaxNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpGroupSMaxNonUniformAMD : Instruction
    {
        public OpGroupSMaxNonUniformAMD() : base("OpGroupSMaxNonUniformAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdScope(), "Execution", OperandQuantifier.Default), new Operand(new EnumType<GroupOperation, GroupOperationParameterFactory>(), "Operation", OperandQuantifier.Default), new Operand(new IdRef(), "X", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFragmentMaskFetchAMD : Instruction
    {
        public OpFragmentMaskFetchAMD() : base("OpFragmentMaskFetchAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpFragmentFetchAMD : Instruction
    {
        public OpFragmentFetchAMD() : base("OpFragmentFetchAMD", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Fragment Index", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupShuffleINTEL : Instruction
    {
        public OpSubgroupShuffleINTEL() : base("OpSubgroupShuffleINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Data", OperandQuantifier.Default), new Operand(new IdRef(), "InvocationId", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupShuffleDownINTEL : Instruction
    {
        public OpSubgroupShuffleDownINTEL() : base("OpSubgroupShuffleDownINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Current", OperandQuantifier.Default), new Operand(new IdRef(), "Next", OperandQuantifier.Default), new Operand(new IdRef(), "Delta", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupShuffleUpINTEL : Instruction
    {
        public OpSubgroupShuffleUpINTEL() : base("OpSubgroupShuffleUpINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Previous", OperandQuantifier.Default), new Operand(new IdRef(), "Current", OperandQuantifier.Default), new Operand(new IdRef(), "Delta", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupShuffleXorINTEL : Instruction
    {
        public OpSubgroupShuffleXorINTEL() : base("OpSubgroupShuffleXorINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Data", OperandQuantifier.Default), new Operand(new IdRef(), "Value", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupBlockReadINTEL : Instruction
    {
        public OpSubgroupBlockReadINTEL() : base("OpSubgroupBlockReadINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Ptr", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupBlockWriteINTEL : Instruction
    {
        public OpSubgroupBlockWriteINTEL() : base("OpSubgroupBlockWriteINTEL", new List<Operand>()
    {new Operand(new IdRef(), "Ptr", OperandQuantifier.Default), new Operand(new IdRef(), "Data", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupImageBlockReadINTEL : Instruction
    {
        public OpSubgroupImageBlockReadINTEL() : base("OpSubgroupImageBlockReadINTEL", new List<Operand>()
    {new Operand(new IdResultType(), null, OperandQuantifier.Default), new Operand(new IdResult(), null, OperandQuantifier.Default), new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), })
        {
        }
    }
    public class OpSubgroupImageBlockWriteINTEL : Instruction
    {
        public OpSubgroupImageBlockWriteINTEL() : base("OpSubgroupImageBlockWriteINTEL", new List<Operand>()
    {new Operand(new IdRef(), "Image", OperandQuantifier.Default), new Operand(new IdRef(), "Coordinate", OperandQuantifier.Default), new Operand(new IdRef(), "Data", OperandQuantifier.Default), })
        {
        }
    }
    public static class Instructions
    {
        private static readonly Dictionary<int, Instruction> instructions_ = new Dictionary<int, Instruction> { { 0, new OpNop() }, { 1, new OpUndef() }, { 2, new OpSourceContinued() }, { 3, new OpSource() }, { 4, new OpSourceExtension() }, { 5, new OpName() }, { 6, new OpMemberName() }, { 7, new OpString() }, { 8, new OpLine() }, { 10, new OpExtension() }, { 11, new OpExtInstImport() }, { 12, new OpExtInst() }, { 14, new OpMemoryModel() }, { 15, new OpEntryPoint() }, { 16, new OpExecutionMode() }, { 17, new OpCapability() }, { 19, new OpTypeVoid() }, { 20, new OpTypeBool() }, { 21, new OpTypeInt() }, { 22, new OpTypeFloat() }, { 23, new OpTypeVector() }, { 24, new OpTypeMatrix() }, { 25, new OpTypeImage() }, { 26, new OpTypeSampler() }, { 27, new OpTypeSampledImage() }, { 28, new OpTypeArray() }, { 29, new OpTypeRuntimeArray() }, { 30, new OpTypeStruct() }, { 31, new OpTypeOpaque() }, { 32, new OpTypePointer() }, { 33, new OpTypeFunction() }, { 34, new OpTypeEvent() }, { 35, new OpTypeDeviceEvent() }, { 36, new OpTypeReserveId() }, { 37, new OpTypeQueue() }, { 38, new OpTypePipe() }, { 39, new OpTypeForwardPointer() }, { 41, new OpConstantTrue() }, { 42, new OpConstantFalse() }, { 43, new OpConstant() }, { 44, new OpConstantComposite() }, { 45, new OpConstantSampler() }, { 46, new OpConstantNull() }, { 48, new OpSpecConstantTrue() }, { 49, new OpSpecConstantFalse() }, { 50, new OpSpecConstant() }, { 51, new OpSpecConstantComposite() }, { 52, new OpSpecConstantOp() }, { 54, new OpFunction() }, { 55, new OpFunctionParameter() }, { 56, new OpFunctionEnd() }, { 57, new OpFunctionCall() }, { 59, new OpVariable() }, { 60, new OpImageTexelPointer() }, { 61, new OpLoad() }, { 62, new OpStore() }, { 63, new OpCopyMemory() }, { 64, new OpCopyMemorySized() }, { 65, new OpAccessChain() }, { 66, new OpInBoundsAccessChain() }, { 67, new OpPtrAccessChain() }, { 68, new OpArrayLength() }, { 69, new OpGenericPtrMemSemantics() }, { 70, new OpInBoundsPtrAccessChain() }, { 71, new OpDecorate() }, { 72, new OpMemberDecorate() }, { 73, new OpDecorationGroup() }, { 74, new OpGroupDecorate() }, { 75, new OpGroupMemberDecorate() }, { 77, new OpVectorExtractDynamic() }, { 78, new OpVectorInsertDynamic() }, { 79, new OpVectorShuffle() }, { 80, new OpCompositeConstruct() }, { 81, new OpCompositeExtract() }, { 82, new OpCompositeInsert() }, { 83, new OpCopyObject() }, { 84, new OpTranspose() }, { 86, new OpSampledImage() }, { 87, new OpImageSampleImplicitLod() }, { 88, new OpImageSampleExplicitLod() }, { 89, new OpImageSampleDrefImplicitLod() }, { 90, new OpImageSampleDrefExplicitLod() }, { 91, new OpImageSampleProjImplicitLod() }, { 92, new OpImageSampleProjExplicitLod() }, { 93, new OpImageSampleProjDrefImplicitLod() }, { 94, new OpImageSampleProjDrefExplicitLod() }, { 95, new OpImageFetch() }, { 96, new OpImageGather() }, { 97, new OpImageDrefGather() }, { 98, new OpImageRead() }, { 99, new OpImageWrite() }, { 100, new OpImage() }, { 101, new OpImageQueryFormat() }, { 102, new OpImageQueryOrder() }, { 103, new OpImageQuerySizeLod() }, { 104, new OpImageQuerySize() }, { 105, new OpImageQueryLod() }, { 106, new OpImageQueryLevels() }, { 107, new OpImageQuerySamples() }, { 109, new OpConvertFToU() }, { 110, new OpConvertFToS() }, { 111, new OpConvertSToF() }, { 112, new OpConvertUToF() }, { 113, new OpUConvert() }, { 114, new OpSConvert() }, { 115, new OpFConvert() }, { 116, new OpQuantizeToF16() }, { 117, new OpConvertPtrToU() }, { 118, new OpSatConvertSToU() }, { 119, new OpSatConvertUToS() }, { 120, new OpConvertUToPtr() }, { 121, new OpPtrCastToGeneric() }, { 122, new OpGenericCastToPtr() }, { 123, new OpGenericCastToPtrExplicit() }, { 124, new OpBitcast() }, { 126, new OpSNegate() }, { 127, new OpFNegate() }, { 128, new OpIAdd() }, { 129, new OpFAdd() }, { 130, new OpISub() }, { 131, new OpFSub() }, { 132, new OpIMul() }, { 133, new OpFMul() }, { 134, new OpUDiv() }, { 135, new OpSDiv() }, { 136, new OpFDiv() }, { 137, new OpUMod() }, { 138, new OpSRem() }, { 139, new OpSMod() }, { 140, new OpFRem() }, { 141, new OpFMod() }, { 142, new OpVectorTimesScalar() }, { 143, new OpMatrixTimesScalar() }, { 144, new OpVectorTimesMatrix() }, { 145, new OpMatrixTimesVector() }, { 146, new OpMatrixTimesMatrix() }, { 147, new OpOuterProduct() }, { 148, new OpDot() }, { 149, new OpIAddCarry() }, { 150, new OpISubBorrow() }, { 151, new OpUMulExtended() }, { 152, new OpSMulExtended() }, { 154, new OpAny() }, { 155, new OpAll() }, { 156, new OpIsNan() }, { 157, new OpIsInf() }, { 158, new OpIsFinite() }, { 159, new OpIsNormal() }, { 160, new OpSignBitSet() }, { 161, new OpLessOrGreater() }, { 162, new OpOrdered() }, { 163, new OpUnordered() }, { 164, new OpLogicalEqual() }, { 165, new OpLogicalNotEqual() }, { 166, new OpLogicalOr() }, { 167, new OpLogicalAnd() }, { 168, new OpLogicalNot() }, { 169, new OpSelect() }, { 170, new OpIEqual() }, { 171, new OpINotEqual() }, { 172, new OpUGreaterThan() }, { 173, new OpSGreaterThan() }, { 174, new OpUGreaterThanEqual() }, { 175, new OpSGreaterThanEqual() }, { 176, new OpULessThan() }, { 177, new OpSLessThan() }, { 178, new OpULessThanEqual() }, { 179, new OpSLessThanEqual() }, { 180, new OpFOrdEqual() }, { 181, new OpFUnordEqual() }, { 182, new OpFOrdNotEqual() }, { 183, new OpFUnordNotEqual() }, { 184, new OpFOrdLessThan() }, { 185, new OpFUnordLessThan() }, { 186, new OpFOrdGreaterThan() }, { 187, new OpFUnordGreaterThan() }, { 188, new OpFOrdLessThanEqual() }, { 189, new OpFUnordLessThanEqual() }, { 190, new OpFOrdGreaterThanEqual() }, { 191, new OpFUnordGreaterThanEqual() }, { 194, new OpShiftRightLogical() }, { 195, new OpShiftRightArithmetic() }, { 196, new OpShiftLeftLogical() }, { 197, new OpBitwiseOr() }, { 198, new OpBitwiseXor() }, { 199, new OpBitwiseAnd() }, { 200, new OpNot() }, { 201, new OpBitFieldInsert() }, { 202, new OpBitFieldSExtract() }, { 203, new OpBitFieldUExtract() }, { 204, new OpBitReverse() }, { 205, new OpBitCount() }, { 207, new OpDPdx() }, { 208, new OpDPdy() }, { 209, new OpFwidth() }, { 210, new OpDPdxFine() }, { 211, new OpDPdyFine() }, { 212, new OpFwidthFine() }, { 213, new OpDPdxCoarse() }, { 214, new OpDPdyCoarse() }, { 215, new OpFwidthCoarse() }, { 218, new OpEmitVertex() }, { 219, new OpEndPrimitive() }, { 220, new OpEmitStreamVertex() }, { 221, new OpEndStreamPrimitive() }, { 224, new OpControlBarrier() }, { 225, new OpMemoryBarrier() }, { 227, new OpAtomicLoad() }, { 228, new OpAtomicStore() }, { 229, new OpAtomicExchange() }, { 230, new OpAtomicCompareExchange() }, { 231, new OpAtomicCompareExchangeWeak() }, { 232, new OpAtomicIIncrement() }, { 233, new OpAtomicIDecrement() }, { 234, new OpAtomicIAdd() }, { 235, new OpAtomicISub() }, { 236, new OpAtomicSMin() }, { 237, new OpAtomicUMin() }, { 238, new OpAtomicSMax() }, { 239, new OpAtomicUMax() }, { 240, new OpAtomicAnd() }, { 241, new OpAtomicOr() }, { 242, new OpAtomicXor() }, { 245, new OpPhi() }, { 246, new OpLoopMerge() }, { 247, new OpSelectionMerge() }, { 248, new OpLabel() }, { 249, new OpBranch() }, { 250, new OpBranchConditional() }, { 251, new OpSwitch() }, { 252, new OpKill() }, { 253, new OpReturn() }, { 254, new OpReturnValue() }, { 255, new OpUnreachable() }, { 256, new OpLifetimeStart() }, { 257, new OpLifetimeStop() }, { 259, new OpGroupAsyncCopy() }, { 260, new OpGroupWaitEvents() }, { 261, new OpGroupAll() }, { 262, new OpGroupAny() }, { 263, new OpGroupBroadcast() }, { 264, new OpGroupIAdd() }, { 265, new OpGroupFAdd() }, { 266, new OpGroupFMin() }, { 267, new OpGroupUMin() }, { 268, new OpGroupSMin() }, { 269, new OpGroupFMax() }, { 270, new OpGroupUMax() }, { 271, new OpGroupSMax() }, { 274, new OpReadPipe() }, { 275, new OpWritePipe() }, { 276, new OpReservedReadPipe() }, { 277, new OpReservedWritePipe() }, { 278, new OpReserveReadPipePackets() }, { 279, new OpReserveWritePipePackets() }, { 280, new OpCommitReadPipe() }, { 281, new OpCommitWritePipe() }, { 282, new OpIsValidReserveId() }, { 283, new OpGetNumPipePackets() }, { 284, new OpGetMaxPipePackets() }, { 285, new OpGroupReserveReadPipePackets() }, { 286, new OpGroupReserveWritePipePackets() }, { 287, new OpGroupCommitReadPipe() }, { 288, new OpGroupCommitWritePipe() }, { 291, new OpEnqueueMarker() }, { 292, new OpEnqueueKernel() }, { 293, new OpGetKernelNDrangeSubGroupCount() }, { 294, new OpGetKernelNDrangeMaxSubGroupSize() }, { 295, new OpGetKernelWorkGroupSize() }, { 296, new OpGetKernelPreferredWorkGroupSizeMultiple() }, { 297, new OpRetainEvent() }, { 298, new OpReleaseEvent() }, { 299, new OpCreateUserEvent() }, { 300, new OpIsValidEvent() }, { 301, new OpSetUserEventStatus() }, { 302, new OpCaptureEventProfilingInfo() }, { 303, new OpGetDefaultQueue() }, { 304, new OpBuildNDRange() }, { 305, new OpImageSparseSampleImplicitLod() }, { 306, new OpImageSparseSampleExplicitLod() }, { 307, new OpImageSparseSampleDrefImplicitLod() }, { 308, new OpImageSparseSampleDrefExplicitLod() }, { 309, new OpImageSparseSampleProjImplicitLod() }, { 310, new OpImageSparseSampleProjExplicitLod() }, { 311, new OpImageSparseSampleProjDrefImplicitLod() }, { 312, new OpImageSparseSampleProjDrefExplicitLod() }, { 313, new OpImageSparseFetch() }, { 314, new OpImageSparseGather() }, { 315, new OpImageSparseDrefGather() }, { 316, new OpImageSparseTexelsResident() }, { 317, new OpNoLine() }, { 318, new OpAtomicFlagTestAndSet() }, { 319, new OpAtomicFlagClear() }, { 320, new OpImageSparseRead() }, { 321, new OpSizeOf() }, { 322, new OpTypePipeStorage() }, { 323, new OpConstantPipeStorage() }, { 324, new OpCreatePipeFromPipeStorage() }, { 325, new OpGetKernelLocalSizeForSubgroupCount() }, { 326, new OpGetKernelMaxNumSubgroups() }, { 327, new OpTypeNamedBarrier() }, { 328, new OpNamedBarrierInitialize() }, { 329, new OpMemoryNamedBarrier() }, { 330, new OpModuleProcessed() }, { 331, new OpExecutionModeId() }, { 332, new OpDecorateId() }, { 4421, new OpSubgroupBallotKHR() }, { 4422, new OpSubgroupFirstInvocationKHR() }, { 4428, new OpSubgroupAllKHR() }, { 4429, new OpSubgroupAnyKHR() }, { 4430, new OpSubgroupAllEqualKHR() }, { 4432, new OpSubgroupReadInvocationKHR() }, { 5000, new OpGroupIAddNonUniformAMD() }, { 5001, new OpGroupFAddNonUniformAMD() }, { 5002, new OpGroupFMinNonUniformAMD() }, { 5003, new OpGroupUMinNonUniformAMD() }, { 5004, new OpGroupSMinNonUniformAMD() }, { 5005, new OpGroupFMaxNonUniformAMD() }, { 5006, new OpGroupUMaxNonUniformAMD() }, { 5007, new OpGroupSMaxNonUniformAMD() }, { 5011, new OpFragmentMaskFetchAMD() }, { 5012, new OpFragmentFetchAMD() }, { 5571, new OpSubgroupShuffleINTEL() }, { 5572, new OpSubgroupShuffleDownINTEL() }, { 5573, new OpSubgroupShuffleUpINTEL() }, { 5574, new OpSubgroupShuffleXorINTEL() }, { 5575, new OpSubgroupBlockReadINTEL() }, { 5576, new OpSubgroupBlockWriteINTEL() }, { 5577, new OpSubgroupImageBlockReadINTEL() }, { 5578, new OpSubgroupImageBlockWriteINTEL() }, };
        public static IReadOnlyDictionary<int, Instruction> OpcodeToInstruction
        {
            get => instructions_;
        }
    }
}