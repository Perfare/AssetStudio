using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public class MinMaxAABB
    {
        public Vector3 m_Min;
        public Vector3 m_Max;

        public MinMaxAABB(BinaryReader reader)
        {
            m_Min = reader.ReadVector3();
            m_Max = reader.ReadVector3();
        }
    }

    public class CompressedMesh
    {
        public PackedFloatVector m_Vertices;
        public PackedFloatVector m_UV;
        public PackedFloatVector m_BindPoses;
        public PackedFloatVector m_Normals;
        public PackedFloatVector m_Tangents;
        public PackedIntVector m_Weights;
        public PackedIntVector m_NormalSigns;
        public PackedIntVector m_TangentSigns;
        public PackedFloatVector m_FloatColors;
        public PackedIntVector m_BoneIndices;
        public PackedIntVector m_Triangles;
        public PackedIntVector m_Colors;
        public uint m_UVInfo;

        public CompressedMesh(ObjectReader reader)
        {
            var version = reader.version;

            m_Vertices = new PackedFloatVector(reader);
            m_UV = new PackedFloatVector(reader);
            if (version[0] < 5)
            {
                m_BindPoses = new PackedFloatVector(reader);
            }
            m_Normals = new PackedFloatVector(reader);
            m_Tangents = new PackedFloatVector(reader);
            m_Weights = new PackedIntVector(reader);
            m_NormalSigns = new PackedIntVector(reader);
            m_TangentSigns = new PackedIntVector(reader);
            if (version[0] >= 5)
            {
                m_FloatColors = new PackedFloatVector(reader);
            }
            m_BoneIndices = new PackedIntVector(reader);
            m_Triangles = new PackedIntVector(reader);
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5 and up
            {
                if (version[0] < 5)
                {
                    m_Colors = new PackedIntVector(reader);
                }
                else
                {
                    m_UVInfo = reader.ReadUInt32();
                }
            }
        }
    }

    public class StreamInfo
    {
        public uint channelMask;
        public uint offset;
        public uint stride;
        public uint align;
        public byte dividerOp;
        public ushort frequency;

        public StreamInfo() { }

        public StreamInfo(ObjectReader reader)
        {
            var version = reader.version;

            channelMask = reader.ReadUInt32();
            offset = reader.ReadUInt32();

            if (version[0] < 4) //4.0 down
            {
                stride = reader.ReadUInt32();
                align = reader.ReadUInt32();
            }
            else
            {
                stride = reader.ReadByte();
                dividerOp = reader.ReadByte();
                frequency = reader.ReadUInt16();
            }
        }
    }

    public class ChannelInfo
    {
        public byte stream;
        public byte offset;
        public byte format;
        public byte dimension;

        public ChannelInfo() { }

        public ChannelInfo(ObjectReader reader)
        {
            stream = reader.ReadByte();
            offset = reader.ReadByte();
            format = reader.ReadByte();
            dimension = (byte)(reader.ReadByte() & 0xF);
        }
    }

    public class VertexData
    {
        public uint m_CurrentChannels;
        public uint m_VertexCount;
        public ChannelInfo[] m_Channels;
        public StreamInfo[] m_Streams;
        public byte[] m_DataSize;

        public VertexData(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] < 2018)//2018 down
            {
                m_CurrentChannels = reader.ReadUInt32();
            }

            m_VertexCount = reader.ReadUInt32();

            if (version[0] >= 4) //4.0 and up
            {
                var m_ChannelsSize = reader.ReadInt32();
                m_Channels = new ChannelInfo[m_ChannelsSize];
                for (int i = 0; i < m_ChannelsSize; i++)
                {
                    m_Channels[i] = new ChannelInfo(reader);
                }
            }

            if (version[0] < 5) //5.0 down
            {
                if (version[0] < 4)
                {
                    m_Streams = new StreamInfo[4];
                }
                else
                {
                    m_Streams = new StreamInfo[reader.ReadInt32()];
                }

                for (int i = 0; i < m_Streams.Length; i++)
                {
                    m_Streams[i] = new StreamInfo(reader);
                }

                if (version[0] < 4) //4.0 down
                {
                    GetChannels(version);
                }
            }
            else //5.0 and up
            {
                GetStreams(version);
            }

            m_DataSize = reader.ReadBytes(reader.ReadInt32());
            reader.AlignStream();
        }

        private void GetStreams(int[] version)
        {
            var streamCount = m_Channels.Max(x => x.stream) + 1;
            m_Streams = new StreamInfo[streamCount];
            uint offset = 0;
            for (int s = 0; s < streamCount; s++)
            {
                uint chnMask = 0;
                uint stride = 0;
                for (int chn = 0; chn < m_Channels.Length; chn++)
                {
                    var m_Channel = m_Channels[chn];
                    if (m_Channel.stream == s)
                    {
                        if (m_Channel.dimension > 0)
                        {
                            chnMask |= 1u << chn;
                            stride += m_Channel.dimension * MeshHelper.GetFormatSize(version, m_Channel.format);
                        }
                    }
                }
                m_Streams[s] = new StreamInfo
                {
                    channelMask = chnMask,
                    offset = offset,
                    stride = stride,
                    dividerOp = 0,
                    frequency = 0
                };
                offset += m_VertexCount * stride;
                //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                offset = (offset + (16u - 1u)) & ~(16u - 1u);
            }
        }

        private void GetChannels(int[] version)
        {
            m_Channels = new ChannelInfo[6];
            for (int i = 0; i < 6; i++)
            {
                m_Channels[i] = new ChannelInfo();
            }
            for (var s = 0; s < m_Streams.Length; s++)
            {
                var m_Stream = m_Streams[s];
                var channelMask = new BitArray(new[] { (int)m_Stream.channelMask });
                byte offset = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (channelMask.Get(i))
                    {
                        var m_Channel = m_Channels[i];
                        m_Channel.stream = (byte)s;
                        m_Channel.offset = offset;
                        switch (i)
                        {
                            case 0: //kShaderChannelVertex
                            case 1: //kShaderChannelNormal
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 3;
                                break;
                            case 2: //kShaderChannelColor
                                m_Channel.format = 2; //kChannelFormatColor
                                m_Channel.dimension = 4;
                                break;
                            case 3: //kShaderChannelTexCoord0
                            case 4: //kShaderChannelTexCoord1
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 2;
                                break;
                            case 5: //kShaderChannelTangent
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 4;
                                break;
                        }
                        offset += (byte)(m_Channel.dimension * MeshHelper.GetFormatSize(version, m_Channel.format));
                    }
                }
            }
        }
    }

    public class BoneWeights4
    {
        public float[] weight;
        public int[] boneIndex;

        public BoneWeights4()
        {
            weight = new float[4];
            boneIndex = new int[4];
        }

        public BoneWeights4(ObjectReader reader)
        {
            weight = reader.ReadSingleArray(4);
            boneIndex = reader.ReadInt32Array(4);
        }
    }

    public class BlendShapeVertex
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector3 tangent;
        public uint index;

        public BlendShapeVertex(ObjectReader reader)
        {
            vertex = reader.ReadVector3();
            normal = reader.ReadVector3();
            tangent = reader.ReadVector3();
            index = reader.ReadUInt32();
        }
    }

    public class MeshBlendShape
    {
        public uint firstVertex;
        public uint vertexCount;
        public bool hasNormals;
        public bool hasTangents;

        public MeshBlendShape(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] == 4 && version[1] < 3) //4.3 down
            {
                var name = reader.ReadAlignedString();
            }
            firstVertex = reader.ReadUInt32();
            vertexCount = reader.ReadUInt32();
            if (version[0] == 4 && version[1] < 3) //4.3 down
            {
                var aabbMinDelta = reader.ReadVector3();
                var aabbMaxDelta = reader.ReadVector3();
            }
            hasNormals = reader.ReadBoolean();
            hasTangents = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                reader.AlignStream();
            }
        }
    }

    public class MeshBlendShapeChannel
    {
        public string name;
        public uint nameHash;
        public int frameIndex;
        public int frameCount;

        public MeshBlendShapeChannel(ObjectReader reader)
        {
            name = reader.ReadAlignedString();
            nameHash = reader.ReadUInt32();
            frameIndex = reader.ReadInt32();
            frameCount = reader.ReadInt32();
        }
    }

    public class BlendShapeData
    {
        public BlendShapeVertex[] vertices;
        public MeshBlendShape[] shapes;
        public MeshBlendShapeChannel[] channels;
        public float[] fullWeights;

        public BlendShapeData(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numVerts = reader.ReadInt32();
                vertices = new BlendShapeVertex[numVerts];
                for (int i = 0; i < numVerts; i++)
                {
                    vertices[i] = new BlendShapeVertex(reader);
                }

                int numShapes = reader.ReadInt32();
                shapes = new MeshBlendShape[numShapes];
                for (int i = 0; i < numShapes; i++)
                {
                    shapes[i] = new MeshBlendShape(reader);
                }

                int numChannels = reader.ReadInt32();
                channels = new MeshBlendShapeChannel[numChannels];
                for (int i = 0; i < numChannels; i++)
                {
                    channels[i] = new MeshBlendShapeChannel(reader);
                }

                fullWeights = reader.ReadSingleArray();
            }
            else
            {
                var m_ShapesSize = reader.ReadInt32();
                var m_Shapes = new MeshBlendShape[m_ShapesSize];
                for (int i = 0; i < m_ShapesSize; i++)
                {
                    m_Shapes[i] = new MeshBlendShape(reader);
                }
                reader.AlignStream();
                var m_ShapeVerticesSize = reader.ReadInt32();
                var m_ShapeVertices = new BlendShapeVertex[m_ShapeVerticesSize]; //MeshBlendShapeVertex
                for (int i = 0; i < m_ShapeVerticesSize; i++)
                {
                    m_ShapeVertices[i] = new BlendShapeVertex(reader);
                }
            }
        }
    }

    public class SubMesh
    {
        public uint firstByte;
        public uint indexCount;
        public int topology;
        public uint triangleCount;
        public uint baseVertex;
        public uint firstVertex;
        public uint vertexCount;
        public AABB localAABB;

        public SubMesh(ObjectReader reader)
        {
            var version = reader.version;

            firstByte = reader.ReadUInt32();
            indexCount = reader.ReadUInt32();
            topology = reader.ReadInt32();

            if (version[0] < 4) //4.0 down
            {
                triangleCount = reader.ReadUInt32();
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3)) //2017.3 and up
            {
                baseVertex = reader.ReadUInt32();
            }

            if (version[0] >= 3) //3.0 and up
            {
                firstVertex = reader.ReadUInt32();
                vertexCount = reader.ReadUInt32();
                localAABB = new AABB(reader);
            }
        }
    }

    public sealed class Mesh : NamedObject
    {
        private bool m_Use16BitIndices = true; //3.5.0 and newer always uses 16bit indices;
        public SubMesh[] m_SubMeshes;
        private uint[] m_IndexBuffer;
        public BlendShapeData m_Shapes;
        public Matrix4x4[] m_BindPose;
        public uint[] m_BoneNameHashes;
        public int m_VertexCount;
        public float[] m_Vertices;
        public BoneWeights4[] m_Skin;
        public float[] m_Normals;
        public float[] m_Colors;
        public float[] m_UV0;
        public float[] m_UV1;
        public float[] m_UV2;
        public float[] m_UV3;
        public float[] m_UV4;
        public float[] m_UV5;
        public float[] m_UV6;
        public float[] m_UV7;
        public float[] m_Tangents;
        private VertexData m_VertexData;
        private CompressedMesh m_CompressedMesh;
        private StreamingInfo m_StreamData;

        public List<uint> m_Indices = new List<uint>(); //use a list because I don't always know the facecount for triangle strips

        public Mesh(ObjectReader reader) : base(reader)
        {
            if (version[0] < 3 || (version[0] == 3 && version[1] < 5)) //3.5 down
            {
                m_Use16BitIndices = reader.ReadInt32() > 0;
            }

            if (version[0] == 2 && version[1] <= 5) //2.5 and down
            {
                int m_IndexBuffer_size = reader.ReadInt32();

                if (m_Use16BitIndices)
                {
                    m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                    for (int i = 0; i < m_IndexBuffer_size / 2; i++)
                    {
                        m_IndexBuffer[i] = reader.ReadUInt16();
                    }
                    reader.AlignStream();
                }
                else
                {
                    m_IndexBuffer = reader.ReadUInt32Array(m_IndexBuffer_size / 4);
                }
            }

            int m_SubMeshesSize = reader.ReadInt32();
            m_SubMeshes = new SubMesh[m_SubMeshesSize];
            for (int i = 0; i < m_SubMeshesSize; i++)
            {
                m_SubMeshes[i] = new SubMesh(reader);
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_Shapes = new BlendShapeData(reader);
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_BindPose = reader.ReadMatrixArray();
                m_BoneNameHashes = reader.ReadUInt32Array();
                var m_RootBoneNameHash = reader.ReadUInt32();
            }

            if (version[0] > 2 || (version[0] == 2 && version[1] >= 6)) //2.6.0 and up
            {
                if (version[0] >= 2019) //2019 and up
                {
                    var m_BonesAABBSize = reader.ReadInt32();
                    var m_BonesAABB = new MinMaxAABB[m_BonesAABBSize];
                    for (int i = 0; i < m_BonesAABBSize; i++)
                    {
                        m_BonesAABB[i] = new MinMaxAABB(reader);
                    }

                    var m_VariableBoneCountWeights = reader.ReadUInt32Array();
                }

                var m_MeshCompression = reader.ReadByte();
                if (version[0] >= 4)
                {
                    if (version[0] < 5)
                    {
                        var m_StreamCompression = reader.ReadByte();
                    }
                    var m_IsReadable = reader.ReadBoolean();
                    var m_KeepVertices = reader.ReadBoolean();
                    var m_KeepIndices = reader.ReadBoolean();
                }
                reader.AlignStream();

                //Unity fixed it in 2017.3.1p1 and later versions
                if ((version[0] > 2017 || (version[0] == 2017 && version[1] >= 4)) || //2017.4
                    ((version[0] == 2017 && version[1] == 3 && version[2] == 1) && buildType.IsPatch) || //fixed after 2017.3.1px
                    ((version[0] == 2017 && version[1] == 3) && m_MeshCompression == 0))//2017.3.xfx with no compression
                {
                    var m_IndexFormat = reader.ReadInt32();
                }

                int m_IndexBuffer_size = reader.ReadInt32();
                if (m_Use16BitIndices)
                {
                    m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                    for (int i = 0; i < m_IndexBuffer_size / 2; i++)
                    {
                        m_IndexBuffer[i] = reader.ReadUInt16();
                    }
                    reader.AlignStream();
                }
                else
                {
                    m_IndexBuffer = reader.ReadUInt32Array(m_IndexBuffer_size / 4);
                }
            }

            if (version[0] < 3 || (version[0] == 3 && version[1] < 5)) //3.4.2 and earlier
            {
                m_VertexCount = reader.ReadInt32();
                m_Vertices = reader.ReadSingleArray(m_VertexCount * 3); //Vector3

                m_Skin = new BoneWeights4[reader.ReadInt32()];
                for (int s = 0; s < m_Skin.Length; s++)
                {
                    m_Skin[s] = new BoneWeights4(reader);
                }

                m_BindPose = reader.ReadMatrixArray();

                m_UV0 = reader.ReadSingleArray(reader.ReadInt32() * 2); //Vector2

                m_UV1 = reader.ReadSingleArray(reader.ReadInt32() * 2); //Vector2

                if (version[0] == 2 && version[1] <= 5) //2.5 and down
                {
                    int m_TangentSpace_size = reader.ReadInt32();
                    m_Normals = new float[m_TangentSpace_size * 3];
                    m_Tangents = new float[m_TangentSpace_size * 4];
                    for (int v = 0; v < m_TangentSpace_size; v++)
                    {
                        m_Normals[v * 3] = reader.ReadSingle();
                        m_Normals[v * 3 + 1] = reader.ReadSingle();
                        m_Normals[v * 3 + 2] = reader.ReadSingle();
                        m_Tangents[v * 3] = reader.ReadSingle();
                        m_Tangents[v * 3 + 1] = reader.ReadSingle();
                        m_Tangents[v * 3 + 2] = reader.ReadSingle();
                        m_Tangents[v * 3 + 3] = reader.ReadSingle(); //handedness
                    }
                }
                else //2.6.0 and later
                {
                    m_Tangents = reader.ReadSingleArray(reader.ReadInt32() * 4); //Vector4

                    m_Normals = reader.ReadSingleArray(reader.ReadInt32() * 3); //Vector3
                }
            }
            else
            {
                if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
                {
                    m_Skin = new BoneWeights4[reader.ReadInt32()];
                    for (int s = 0; s < m_Skin.Length; s++)
                    {
                        m_Skin[s] = new BoneWeights4(reader);
                    }
                }

                if (version[0] == 3 || (version[0] == 4 && version[1] <= 2)) //4.2 and down
                {
                    m_BindPose = reader.ReadMatrixArray();
                }

                m_VertexData = new VertexData(reader);
            }

            if (version[0] > 2 || (version[0] == 2 && version[1] >= 6)) //2.6.0 and later
            {
                m_CompressedMesh = new CompressedMesh(reader);
            }

            reader.Position += 24; //AABB m_LocalAABB

            if (version[0] < 3 || (version[0] == 3 && version[1] <= 4)) //3.4.2 and earlier
            {
                int m_Colors_size = reader.ReadInt32();
                m_Colors = new float[m_Colors_size * 4];
                for (int v = 0; v < m_Colors_size * 4; v++)
                {
                    m_Colors[v] = (float)reader.ReadByte() / 0xFF;
                }

                int m_CollisionTriangles_size = reader.ReadInt32();
                reader.Position += m_CollisionTriangles_size * 4; //UInt32 indices
                int m_CollisionVertexCount = reader.ReadInt32();
            }

            int m_MeshUsageFlags = reader.ReadInt32();

            if (version[0] >= 5) //5.0 and up
            {
                var m_BakedConvexCollisionMesh = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream();
                var m_BakedTriangleCollisionMesh = reader.ReadBytes(reader.ReadInt32());
                reader.AlignStream();
            }

            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 2)) //2018.2 and up
            {
                var m_MeshMetrics = new float[2];
                m_MeshMetrics[0] = reader.ReadSingle();
                m_MeshMetrics[1] = reader.ReadSingle();
            }

            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 3)) //2018.3 and up
            {
                reader.AlignStream();
                m_StreamData = new StreamingInfo(reader);
            }

            ProcessData();
        }

        private void ProcessData()
        {
            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                if (m_VertexData.m_VertexCount > 0)
                {
                    var resourceReader = new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, (int)m_StreamData.size);
                    m_VertexData.m_DataSize = resourceReader.GetData();
                }
            }
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5 and up
            {
                ReadVertexData();
            }

            if (version[0] > 2 || (version[0] == 2 && version[1] >= 6)) //2.6.0 and later
            {
                DecompressCompressedMesh();
            }

            BuildFaces();
        }

        private void ReadVertexData()
        {
            m_VertexCount = (int)m_VertexData.m_VertexCount;

            for (var chn = 0; chn < m_VertexData.m_Channels.Length; chn++)
            {
                var m_Channel = m_VertexData.m_Channels[chn];
                if (m_Channel.dimension > 0)
                {
                    var m_Stream = m_VertexData.m_Streams[m_Channel.stream];
                    var channelMask = new BitArray(new[] { (int)m_Stream.channelMask });
                    if (channelMask.Get(chn))
                    {
                        if (version[0] < 2018 && chn == 2 && m_Channel.format == 2)
                        {
                            m_Channel.dimension = 4;
                        }

                        var componentByteSize = (int)MeshHelper.GetFormatSize(version, m_Channel.format);
                        var componentBytes = new byte[m_VertexCount * m_Channel.dimension * componentByteSize];
                        for (int v = 0; v < m_VertexCount; v++)
                        {
                            var vertexOffset = (int)m_Stream.offset + m_Channel.offset + (int)m_Stream.stride * v;
                            for (int d = 0; d < m_Channel.dimension; d++)
                            {
                                var componentOffset = vertexOffset + componentByteSize * d;
                                Buffer.BlockCopy(m_VertexData.m_DataSize, componentOffset, componentBytes, componentByteSize * (v * m_Channel.dimension + d), componentByteSize);
                            }
                        }

                        if (reader.endian == EndianType.BigEndian && componentByteSize > 1) //swap bytes
                        {
                            for (var i = 0; i < componentBytes.Length / componentByteSize; i++)
                            {
                                var buff = new byte[componentByteSize];
                                Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
                                buff = buff.Reverse().ToArray();
                                Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
                            }
                        }

                        int[] componentsIntArray = null;
                        float[] componentsFloatArray = null;
                        if (MeshHelper.IsIntFormat(version, m_Channel.format))
                            componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, componentByteSize);
                        else
                            componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, componentByteSize);

                        if (version[0] >= 2018)
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    m_Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    m_Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelTangent
                                    m_Tangents = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelColor
                                    m_Colors = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord0
                                    m_UV0 = componentsFloatArray;
                                    break;
                                case 5: //kShaderChannelTexCoord1
                                    m_UV1 = componentsFloatArray;
                                    break;
                                case 6: //kShaderChannelTexCoord2
                                    m_UV2 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTexCoord3
                                    m_UV3 = componentsFloatArray;
                                    break;
                                case 8: //kShaderChannelTexCoord4
                                    m_UV4 = componentsFloatArray;
                                    break;
                                case 9: //kShaderChannelTexCoord5
                                    m_UV5 = componentsFloatArray;
                                    break;
                                case 10: //kShaderChannelTexCoord6
                                    m_UV6 = componentsFloatArray;
                                    break;
                                case 11: //kShaderChannelTexCoord7
                                    m_UV7 = componentsFloatArray;
                                    break;
                                //2018.2 and up
                                case 12: //kShaderChannelBlendWeight
                                    if (m_Skin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < m_VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.dimension; j++)
                                        {
                                            m_Skin[i].weight[j] = componentsFloatArray[i * m_Channel.dimension + j];
                                        }
                                    }
                                    break;
                                case 13: //kShaderChannelBlendIndices
                                    if (m_Skin == null)
                                    {
                                        InitMSkin();
                                    }
                                    for (int i = 0; i < m_VertexCount; i++)
                                    {
                                        for (int j = 0; j < m_Channel.dimension; j++)
                                        {
                                            m_Skin[i].boneIndex[j] = componentsIntArray[i * m_Channel.dimension + j];
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (chn)
                            {
                                case 0: //kShaderChannelVertex
                                    m_Vertices = componentsFloatArray;
                                    break;
                                case 1: //kShaderChannelNormal
                                    m_Normals = componentsFloatArray;
                                    break;
                                case 2: //kShaderChannelColor
                                    m_Colors = componentsFloatArray;
                                    break;
                                case 3: //kShaderChannelTexCoord0
                                    m_UV0 = componentsFloatArray;
                                    break;
                                case 4: //kShaderChannelTexCoord1
                                    m_UV1 = componentsFloatArray;
                                    break;
                                case 5:
                                    if (version[0] >= 5) //kShaderChannelTexCoord2
                                    {
                                        m_UV2 = componentsFloatArray;
                                    }
                                    else //kShaderChannelTangent
                                    {
                                        m_Tangents = componentsFloatArray;
                                    }
                                    break;
                                case 6: //kShaderChannelTexCoord3
                                    m_UV3 = componentsFloatArray;
                                    break;
                                case 7: //kShaderChannelTangent
                                    m_Tangents = componentsFloatArray;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void DecompressCompressedMesh()
        {
            //Vertex
            if (m_CompressedMesh.m_Vertices.m_NumItems > 0)
            {
                m_VertexCount = (int)m_CompressedMesh.m_Vertices.m_NumItems / 3;
                m_Vertices = m_CompressedMesh.m_Vertices.UnpackFloats(3, 3 * 4);
            }
            //UV
            if (m_CompressedMesh.m_UV.m_NumItems > 0)
            {
                var m_UVInfo = m_CompressedMesh.m_UVInfo;
                if (m_UVInfo != 0)
                {
                    const int kInfoBitsPerUV = 4;
                    const int kUVDimensionMask = 3;
                    const int kUVChannelExists = 4;
                    const int kMaxTexCoordShaderChannels = 8;

                    int uvSrcOffset = 0;
                    for (int uv = 0; uv < kMaxTexCoordShaderChannels; uv++)
                    {
                        var texCoordBits = m_UVInfo >> (uv * kInfoBitsPerUV);
                        texCoordBits &= (1u << kInfoBitsPerUV) - 1u;
                        if ((texCoordBits & kUVChannelExists) != 0)
                        {
                            var uvDim = 1 + (int)(texCoordBits & kUVDimensionMask);
                            var m_UV = m_CompressedMesh.m_UV.UnpackFloats(uvDim, uvDim * 4, uvSrcOffset, m_VertexCount);
                            SetUV(uv, m_UV);
                            uvSrcOffset += uvDim * m_VertexCount;
                        }
                    }
                }
                else
                {
                    m_UV0 = m_CompressedMesh.m_UV.UnpackFloats(2, 2 * 4, 0, m_VertexCount);
                    if (m_CompressedMesh.m_UV.m_NumItems >= m_VertexCount * 4)
                    {
                        m_UV1 = m_CompressedMesh.m_UV.UnpackFloats(2, 2 * 4, m_VertexCount * 2, m_VertexCount);
                    }
                }
            }
            //BindPose
            if (version[0] < 5)
            {
                if (m_CompressedMesh.m_BindPoses.m_NumItems > 0)
                {
                    m_BindPose = new Matrix4x4[m_CompressedMesh.m_BindPoses.m_NumItems / 16];
                    var m_BindPoses_Unpacked = m_CompressedMesh.m_BindPoses.UnpackFloats(16, 4 * 16);
                    var buffer = new float[16];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        Array.Copy(m_BindPoses_Unpacked, i * 16, buffer, 0, 16);
                        m_BindPose[i] = new Matrix4x4(buffer);
                    }
                }
            }
            //Normal
            if (m_CompressedMesh.m_Normals.m_NumItems > 0)
            {
                var normalData = m_CompressedMesh.m_Normals.UnpackFloats(2, 4 * 2);
                var signs = m_CompressedMesh.m_NormalSigns.UnpackInts();
                m_Normals = new float[m_CompressedMesh.m_Normals.m_NumItems / 2 * 3];
                for (int i = 0; i < m_CompressedMesh.m_Normals.m_NumItems / 2; ++i)
                {
                    var x = normalData[i * 2 + 0];
                    var y = normalData[i * 2 + 1];
                    var zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        var normal = new Vector3(x, y, z);
                        normal.Normalize();
                        x = normal.X;
                        y = normal.Y;
                        z = normal.Z;
                    }
                    if (signs[i] == 0)
                        z = -z;
                    m_Normals[i * 3] = x;
                    m_Normals[i * 3 + 1] = y;
                    m_Normals[i * 3 + 2] = z;
                }
            }
            //Tangent
            if (m_CompressedMesh.m_Tangents.m_NumItems > 0)
            {
                var tangentData = m_CompressedMesh.m_Tangents.UnpackFloats(2, 4 * 2);
                var signs = m_CompressedMesh.m_TangentSigns.UnpackInts();
                m_Tangents = new float[m_CompressedMesh.m_Tangents.m_NumItems / 2 * 4];
                for (int i = 0; i < m_CompressedMesh.m_Tangents.m_NumItems / 2; ++i)
                {
                    var x = tangentData[i * 2 + 0];
                    var y = tangentData[i * 2 + 1];
                    var zsqr = 1 - x * x - y * y;
                    float z;
                    if (zsqr >= 0f)
                        z = (float)Math.Sqrt(zsqr);
                    else
                    {
                        z = 0;
                        var vector3f = new Vector3(x, y, z);
                        vector3f.Normalize();
                        x = vector3f.X;
                        y = vector3f.Y;
                        z = vector3f.Z;
                    }
                    if (signs[i * 2 + 0] == 0)
                        z = -z;
                    var w = signs[i * 2 + 1] > 0 ? 1.0f : -1.0f;
                    m_Tangents[i * 4] = x;
                    m_Tangents[i * 4 + 1] = y;
                    m_Tangents[i * 4 + 2] = z;
                    m_Tangents[i * 4 + 3] = w;
                }
            }
            //FloatColor
            if (version[0] >= 5)
            {
                if (m_CompressedMesh.m_FloatColors.m_NumItems > 0)
                {
                    m_Colors = m_CompressedMesh.m_FloatColors.UnpackFloats(1, 4);
                }
            }
            //Skin
            if (m_CompressedMesh.m_Weights.m_NumItems > 0)
            {
                var weights = m_CompressedMesh.m_Weights.UnpackInts();
                var boneIndices = m_CompressedMesh.m_BoneIndices.UnpackInts();

                InitMSkin();

                int bonePos = 0;
                int boneIndexPos = 0;
                int j = 0;
                int sum = 0;

                for (int i = 0; i < m_CompressedMesh.m_Weights.m_NumItems; i++)
                {
                    //read bone index and weight.
                    m_Skin[bonePos].weight[j] = weights[i] / 31.0f;
                    m_Skin[bonePos].boneIndex[j] = boneIndices[boneIndexPos++];
                    j++;
                    sum += weights[i];

                    //the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
                    if (sum >= 31)
                    {
                        for (; j < 4; j++)
                        {
                            m_Skin[bonePos].weight[j] = 0;
                            m_Skin[bonePos].boneIndex[j] = 0;
                        }
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                    //we read three weights, but they don't add up to one. calculate the fourth one, and read
                    //missing bone index. continue with next vertex.
                    else if (j == 3)
                    {
                        m_Skin[bonePos].weight[j] = (31 - sum) / 31.0f;
                        m_Skin[bonePos].boneIndex[j] = boneIndices[boneIndexPos++];
                        bonePos++;
                        j = 0;
                        sum = 0;
                    }
                }
            }
            //IndexBuffer
            if (m_CompressedMesh.m_Triangles.m_NumItems > 0)
            {
                m_IndexBuffer = Array.ConvertAll(m_CompressedMesh.m_Triangles.UnpackInts(), x => (uint)x);
            }
            //Color
            if (m_CompressedMesh.m_Colors?.m_NumItems > 0)
            {
                m_CompressedMesh.m_Colors.m_NumItems *= 4;
                m_CompressedMesh.m_Colors.m_BitSize /= 4;
                var tempColors = m_CompressedMesh.m_Colors.UnpackInts();
                m_Colors = new float[m_CompressedMesh.m_Colors.m_NumItems];
                for (int v = 0; v < m_CompressedMesh.m_Colors.m_NumItems; v++)
                {
                    m_Colors[v] = tempColors[v] / 255f;
                }
            }
        }

        private void BuildFaces()
        {
            foreach (var m_SubMesh in m_SubMeshes)
            {
                var firstIndex = m_SubMesh.firstByte / 2;
                if (!m_Use16BitIndices)
                {
                    firstIndex /= 2;
                }

                if (m_SubMesh.topology == 0)
                {
                    for (int i = 0; i < m_SubMesh.indexCount / 3; i++)
                    {
                        m_Indices.Add(m_IndexBuffer[firstIndex + i * 3]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + i * 3 + 1]);
                        m_Indices.Add(m_IndexBuffer[firstIndex + i * 3 + 2]);
                    }
                }
                else
                {
                    uint j = 0;
                    for (int i = 0; i < m_SubMesh.indexCount - 2; i++)
                    {
                        uint fa = m_IndexBuffer[firstIndex + i];
                        uint fb = m_IndexBuffer[firstIndex + i + 1];
                        uint fc = m_IndexBuffer[firstIndex + i + 2];

                        if ((fa != fb) && (fa != fc) && (fc != fb))
                        {
                            m_Indices.Add(fa);
                            if ((i % 2) == 0)
                            {
                                m_Indices.Add(fb);
                                m_Indices.Add(fc);
                            }
                            else
                            {
                                m_Indices.Add(fc);
                                m_Indices.Add(fb);
                            }
                            j++;
                        }
                    }
                    //fix indexCount
                    m_SubMesh.indexCount = j * 3;
                }
            }
        }

        private void InitMSkin()
        {
            m_Skin = new BoneWeights4[m_VertexCount];
            for (int i = 0; i < m_VertexCount; i++)
            {
                m_Skin[i] = new BoneWeights4();
            }
        }

        private void SetUV(int uv, float[] m_UV)
        {
            switch (uv)
            {
                case 0:
                    m_UV0 = m_UV;
                    break;
                case 1:
                    m_UV1 = m_UV;
                    break;
                case 2:
                    m_UV2 = m_UV;
                    break;
                case 3:
                    m_UV3 = m_UV;
                    break;
                case 4:
                    m_UV4 = m_UV;
                    break;
                case 5:
                    m_UV5 = m_UV;
                    break;
                case 6:
                    m_UV6 = m_UV;
                    break;
                case 7:
                    m_UV7 = m_UV;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public static class MeshHelper
    {
        private enum VertexChannelFormat
        {
            kChannelFormatFloat,
            kChannelFormatFloat16,
            kChannelFormatColor,
            kChannelFormatByte,
            kChannelFormatUInt32
        }

        private enum VertexFormat
        {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatColor,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        private enum VertexFormatV2019
        {
            kVertexFormatFloat,
            kVertexFormatFloat16,
            kVertexFormatUNorm8,
            kVertexFormatSNorm8,
            kVertexFormatUNorm16,
            kVertexFormatSNorm16,
            kVertexFormatUInt8,
            kVertexFormatSInt8,
            kVertexFormatUInt16,
            kVertexFormatSInt16,
            kVertexFormatUInt32,
            kVertexFormatSInt32
        }

        public static uint GetFormatSize(int[] version, int format)
        {
            if (version[0] < 2017)
            {
                switch ((VertexChannelFormat)format)
                {
                    case VertexChannelFormat.kChannelFormatFloat:
                        return 4u;
                    case VertexChannelFormat.kChannelFormatFloat16:
                        return 2u;
                    case VertexChannelFormat.kChannelFormatColor: //in 4.x is size 4
                        return 1u;
                    case VertexChannelFormat.kChannelFormatByte:
                        return 1u;
                    case VertexChannelFormat.kChannelFormatUInt32: //in 5.x
                        return 4u;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else if (version[0] < 2019)
            {
                switch ((VertexFormat)format)
                {
                    case VertexFormat.kVertexFormatFloat:
                        return 4u;
                    case VertexFormat.kVertexFormatFloat16:
                        return 2u;
                    case VertexFormat.kVertexFormatColor:
                        return 1u;
                    case VertexFormat.kVertexFormatUNorm8:
                        return 1u;
                    case VertexFormat.kVertexFormatSNorm8:
                        return 1u;
                    case VertexFormat.kVertexFormatUNorm16:
                        return 2u;
                    case VertexFormat.kVertexFormatSNorm16:
                        return 2u;
                    case VertexFormat.kVertexFormatUInt8:
                        return 1u;
                    case VertexFormat.kVertexFormatSInt8:
                        return 1u;
                    case VertexFormat.kVertexFormatUInt16:
                        return 2u;
                    case VertexFormat.kVertexFormatSInt16:
                        return 2u;
                    case VertexFormat.kVertexFormatUInt32:
                        return 4u;
                    case VertexFormat.kVertexFormatSInt32:
                        return 4u;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
            else
            {
                switch ((VertexFormatV2019)format)
                {
                    case VertexFormatV2019.kVertexFormatFloat:
                        return 4u;
                    case VertexFormatV2019.kVertexFormatFloat16:
                        return 2u;
                    case VertexFormatV2019.kVertexFormatUNorm8:
                        return 1u;
                    case VertexFormatV2019.kVertexFormatSNorm8:
                        return 1u;
                    case VertexFormatV2019.kVertexFormatUNorm16:
                        return 2u;
                    case VertexFormatV2019.kVertexFormatSNorm16:
                        return 2u;
                    case VertexFormatV2019.kVertexFormatUInt8:
                        return 1u;
                    case VertexFormatV2019.kVertexFormatSInt8:
                        return 1u;
                    case VertexFormatV2019.kVertexFormatUInt16:
                        return 2u;
                    case VertexFormatV2019.kVertexFormatSInt16:
                        return 2u;
                    case VertexFormatV2019.kVertexFormatUInt32:
                        return 4u;
                    case VertexFormatV2019.kVertexFormatSInt32:
                        return 4u;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(format), format, null);
                }
            }
        }

        public static bool IsIntFormat(int[] version, int format)
        {
            if (version[0] < 2017)
            {
                return format == 4;
            }
            else if (version[0] < 2019)
            {
                return format >= 7;
            }
            else
            {
                return format >= 6;
            }
        }

        public static float[] BytesToFloatArray(byte[] inputBytes, int size)
        {
            var result = new float[inputBytes.Length / size];
            for (int i = 0; i < inputBytes.Length / size; i++)
            {
                var value = 0f;
                switch (size)
                {
                    case 1:
                        value = inputBytes[i] / 255.0f;
                        break;
                    case 2:
                        value = Half.ToHalf(inputBytes, i * 2);
                        break;
                    case 4:
                        value = BitConverter.ToSingle(inputBytes, i * 4);
                        break;
                }
                result[i] = value;
            }
            return result;
        }

        public static int[] BytesToIntArray(byte[] inputBytes, int size)
        {
            var result = new int[inputBytes.Length / size];
            for (int i = 0; i < inputBytes.Length / size; i++)
            {
                switch (size)
                {
                    case 1:
                        result[i] = inputBytes[i];
                        break;
                    case 2:
                        result[i] = BitConverter.ToInt16(inputBytes, i * 2);
                        break;
                    case 4:
                        result[i] = BitConverter.ToInt32(inputBytes, i * 4);
                        break;
                }
            }
            return result;
        }
    }
}
