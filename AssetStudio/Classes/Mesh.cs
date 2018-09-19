using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using SharpDX;

namespace AssetStudio
{
    public class Mesh
    {
        private EndianBinaryReader reader;
        public string m_Name;
        public List<SubMesh> m_SubMeshes = new List<SubMesh>();
        public List<uint> m_Indices = new List<uint>(); //use a list because I don't always know the facecount for triangle strips
        public List<int> m_materialIDs = new List<int>();
        private uint[] m_IndexBuffer;
        private ChannelInfo[] m_Channels;
        private StreamInfo[] m_Streams;
        public List<BoneInfluence>[] m_Skin;
        public float[][,] m_BindPose;
        public int m_VertexCount;
        public float[] m_Vertices;
        public float[] m_Normals;
        public float[] m_Colors;
        public float[] m_UV1;
        public float[] m_UV2;
        public float[] m_UV3;
        public float[] m_UV4;
        public float[] m_Tangents;
        public uint[] m_BoneNameHashes;
        public BlendShapeData m_Shapes;

        public class SubMesh
        {
            public uint firstByte;
            public uint indexCount;
            public int topology;
            public uint triangleCount;
            public uint firstVertex;
            public uint vertexCount;
        }

        public class BoneInfluence
        {
            public float weight;
            public int boneIndex;
        }

        public class ChannelInfo
        {
            public byte stream;
            public byte offset;
            public byte format;
            public byte dimension;
        }

        public class StreamInfo
        {
            public BitArray channelMask;
            public int offset;
            public int stride;
            public uint align; //3.5.0 - 3.5.7
            public byte dividerOp; //4.0.0 and later
            public ushort frequency;
        }

        public class BlendShapeData
        {
            public class BlendShapeVertex
            {
                public Vector3 vertex { get; set; }
                public Vector3 normal { get; set; }
                public Vector3 tangent { get; set; }
                public uint index { get; set; }

                public BlendShapeVertex(EndianBinaryReader reader)
                {
                    vertex = reader.ReadVector3();
                    normal = reader.ReadVector3();
                    tangent = reader.ReadVector3();
                    index = reader.ReadUInt32();
                }
            }

            public class MeshBlendShape
            {
                public uint firstVertex { get; set; }
                public uint vertexCount { get; set; }
                public bool hasNormals { get; set; }
                public bool hasTangents { get; set; }

                public MeshBlendShape(EndianBinaryReader reader)
                {
                    firstVertex = reader.ReadUInt32();
                    vertexCount = reader.ReadUInt32();
                    hasNormals = reader.ReadBoolean();
                    hasTangents = reader.ReadBoolean();
                    reader.AlignStream(4);
                }
            }

            public class MeshBlendShapeChannel
            {
                public string name { get; set; }
                public uint nameHash { get; set; }
                public int frameIndex { get; set; }
                public int frameCount { get; set; }

                public MeshBlendShapeChannel(EndianBinaryReader reader)
                {
                    name = reader.ReadAlignedString();
                    nameHash = reader.ReadUInt32();
                    frameIndex = reader.ReadInt32();
                    frameCount = reader.ReadInt32();
                }
            }

            public List<BlendShapeVertex> vertices { get; set; }
            public List<MeshBlendShape> shapes { get; set; }
            public List<MeshBlendShapeChannel> channels { get; set; }
            public List<float> fullWeights { get; set; }

            public BlendShapeData(EndianBinaryReader reader)
            {
                int numVerts = reader.ReadInt32();
                vertices = new List<BlendShapeVertex>(numVerts);
                for (int i = 0; i < numVerts; i++)
                {
                    vertices.Add(new BlendShapeVertex(reader));
                }

                int numShapes = reader.ReadInt32();
                shapes = new List<MeshBlendShape>(numShapes);
                for (int i = 0; i < numShapes; i++)
                {
                    shapes.Add(new MeshBlendShape(reader));
                }

                int numChannels = reader.ReadInt32();
                channels = new List<MeshBlendShapeChannel>(numChannels);
                for (int i = 0; i < numChannels; i++)
                {
                    channels.Add(new MeshBlendShapeChannel(reader));
                }

                int numWeights = reader.ReadInt32();
                fullWeights = new List<float>(numWeights);
                for (int i = 0; i < numWeights; i++)
                {
                    fullWeights.Add(reader.ReadSingle());
                }
            }
        }

        private float BytesToFloat(byte[] inputBytes)
        {
            float result = 0;
            if (reader.endian == EndianType.BigEndian) { Array.Reverse(inputBytes); }

            switch (inputBytes.Length)
            {
                case 1:
                    result = inputBytes[0] / 255.0f;
                    break;
                case 2:
                    result = System.Half.ToHalf(inputBytes, 0);
                    break;
                case 4:
                    result = BitConverter.ToSingle(inputBytes, 0);
                    break;
            }

            return result;
        }

        private static int GetChannelFormatSize(int format)
        {
            switch (format)
            {
                case 0: //kChannelFormatFloat
                    return 4;
                case 1: //kChannelFormatFloat16
                    return 2;
                case 2: //kChannelFormatColor, in 4.x is size 4
                    return 1;
                case 3: //kChannelFormatByte
                    return 1;
                case 11: //kChannelFormatInt32
                    return 4;
                default:
                    return 0;
            }
        }

        private static float[] BytesToFloatArray(byte[] inputBytes, int size)
        {
            var result = new float[inputBytes.Length / size];
            for (int i = 0; i < inputBytes.Length / size; i++)
            {
                float value = 0f;
                switch (size)
                {
                    case 1:
                        value = inputBytes[i] / 255.0f;
                        break;
                    case 2:
                        value = System.Half.ToHalf(inputBytes, i * 2);
                        break;
                    case 4:
                        value = BitConverter.ToSingle(inputBytes, i * 4);
                        break;
                }
                result[i] = value;
            }
            return result;
        }

        private static int[] BytesToIntArray(byte[] inputBytes)
        {
            var result = new int[inputBytes.Length / 4];
            for (int i = 0; i < inputBytes.Length / 4; i++)
            {
                result[i] = BitConverter.ToInt32(inputBytes, i * 4);
            }
            return result;
        }

        private void InitMSkin()
        {
            m_Skin = new List<BoneInfluence>[m_VertexCount];
            for (int i = 0; i < m_VertexCount; i++)
            {
                m_Skin[i] = new List<BoneInfluence>(4);
                for (int j = 0; j < 4; j++)
                {
                    m_Skin[i].Add(new BoneInfluence());
                }
            }
        }

        public Mesh(AssetPreloadData preloadData, bool readSwitch)
        {
            var version = preloadData.sourceFile.version;
            reader = preloadData.InitReader();

            bool m_Use16BitIndices = true; //3.5.0 and newer always uses 16bit indices
            uint m_MeshCompression = 0;

            if (preloadData.sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = reader.ReadUInt32();
                PPtr m_PrefabParentObject = preloadData.sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = preloadData.sourceFile.ReadPPtr();
            }

            m_Name = reader.ReadAlignedString();

            if (readSwitch)
            {

                if (version[0] < 3 || (version[0] == 3 && version[1] < 5))
                {
                    m_Use16BitIndices = reader.ReadBoolean();
                    reader.Position += 3;
                }

                #region Index Buffer for 2.5.1 and earlier
                if (version[0] == 2 && version[1] <= 5)
                {
                    int m_IndexBuffer_size = reader.ReadInt32();

                    if (m_Use16BitIndices)
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                        for (int i = 0; i < m_IndexBuffer_size / 2; i++) { m_IndexBuffer[i] = reader.ReadUInt16(); }
                        reader.AlignStream(4);
                    }
                    else
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 4];
                        for (int i = 0; i < m_IndexBuffer_size / 4; i++) { m_IndexBuffer[i] = reader.ReadUInt32(); }
                    }
                }
                #endregion

                #region subMeshes
                int m_SubMeshes_size = reader.ReadInt32();
                for (int s = 0; s < m_SubMeshes_size; s++)
                {
                    m_SubMeshes.Add(new SubMesh());
                    m_SubMeshes[s].firstByte = reader.ReadUInt32();
                    m_SubMeshes[s].indexCount = reader.ReadUInt32(); //what is this in case of triangle strips?
                    m_SubMeshes[s].topology = reader.ReadInt32(); //isTriStrip
                    if (version[0] < 4)
                    {
                        m_SubMeshes[s].triangleCount = reader.ReadUInt32();
                    }
                    if (version[0] >= 3)
                    {
                        if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3))//2017.3 and up
                        {
                            var baseVertex = reader.ReadUInt32();
                        }
                        m_SubMeshes[s].firstVertex = reader.ReadUInt32();
                        m_SubMeshes[s].vertexCount = reader.ReadUInt32();
                        reader.Position += 24; //Axis-Aligned Bounding Box
                    }
                }
                #endregion

                #region BlendShapeData for 4.1.0 to 4.2.x, excluding 4.1.0 alpha
                if (version[0] == 4 && ((version[1] == 1 && preloadData.sourceFile.buildType[0] != "a") || (version[1] > 1 && version[1] <= 2)))
                {
                    int m_Shapes_size = reader.ReadInt32();
                    if (m_Shapes_size > 0)
                    {
                        //bool stop = true;
                    }
                    for (int s = 0; s < m_Shapes_size; s++) //untested
                    {
                        string shape_name = reader.ReadAlignedString();
                        reader.Position += 36; //uint firstVertex, vertexCount; Vector3f aabbMinDelta, aabbMaxDelta; bool hasNormals, hasTangents
                    }

                    int m_ShapeVertices_size = reader.ReadInt32();
                    reader.Position += m_ShapeVertices_size * 40; //vertex positions, normals, tangents & uint index
                }
                #endregion
                #region BlendShapeData and BindPose for 4.3.0 and later
                else if (version[0] >= 5 || (version[0] == 4 && version[1] >= 3))
                {
                    m_Shapes = new BlendShapeData(reader);

                    m_BindPose = new float[reader.ReadInt32()][,];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        m_BindPose[i] = new[,] {
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() } };
                    }

                    int m_BoneNameHashes_size = reader.ReadInt32();
                    m_BoneNameHashes = new uint[m_BoneNameHashes_size];
                    for (int i = 0; i < m_BoneNameHashes_size; i++)
                    {
                        m_BoneNameHashes[i] = reader.ReadUInt32();
                    }

                    uint m_RootBoneNameHash = reader.ReadUInt32();
                }
                #endregion

                #region Index Buffer for 2.6.0 and later
                if (version[0] >= 3 || (version[0] == 2 && version[1] >= 6))
                {
                    m_MeshCompression = reader.ReadByte();
                    if (version[0] >= 4)
                    {
                        if (version[0] < 5)
                        {
                            uint m_StreamCompression = reader.ReadByte();
                        }
                        bool m_IsReadable = reader.ReadBoolean();
                        bool m_KeepVertices = reader.ReadBoolean();
                        bool m_KeepIndices = reader.ReadBoolean();
                        if (preloadData.HasStructMember("m_UsedForStaticMeshColliderOnly"))
                        {
                            var m_UsedForStaticMeshColliderOnly = reader.ReadBoolean();
                        }
                    }
                    reader.AlignStream(4);
                    //This is a bug fixed in 2017.3.1p1 and later versions
                    if ((version[0] > 2017 || (version[0] == 2017 && version[1] >= 4)) || //2017.4
                        ((version[0] == 2017 && version[1] == 3 && version[2] == 1) && preloadData.sourceFile.buildType[0] == "p") || //fixed after 2017.3.1px
                        ((version[0] == 2017 && version[1] == 3) && m_MeshCompression == 0))//2017.3.xfx with no compression
                    {
                        var m_IndexFormat = reader.ReadInt32();
                    }
                    int m_IndexBuffer_size = reader.ReadInt32();

                    if (m_Use16BitIndices)
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                        for (int i = 0; i < m_IndexBuffer_size / 2; i++) { m_IndexBuffer[i] = reader.ReadUInt16(); }
                        reader.AlignStream(4);
                    }
                    else
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 4];
                        for (int i = 0; i < m_IndexBuffer_size / 4; i++) { m_IndexBuffer[i] = reader.ReadUInt32(); }
                        reader.AlignStream(4);//untested
                    }
                }
                #endregion

                #region Vertex Buffer for 3.4.2 and earlier
                if (version[0] < 3 || (version[0] == 3 && version[1] < 5))
                {
                    m_VertexCount = reader.ReadInt32();
                    m_Vertices = new float[m_VertexCount * 3];
                    for (int v = 0; v < m_VertexCount * 3; v++) { m_Vertices[v] = reader.ReadSingle(); }

                    m_Skin = new List<BoneInfluence>[reader.ReadInt32()];
                    for (int s = 0; s < m_Skin.Length; s++)
                    {
                        m_Skin[s] = new List<BoneInfluence>();
                        for (int i = 0; i < 4; i++) { m_Skin[s].Add(new BoneInfluence() { weight = reader.ReadSingle() }); }
                        for (int i = 0; i < 4; i++) { m_Skin[s][i].boneIndex = reader.ReadInt32(); }
                    }

                    m_BindPose = new float[reader.ReadInt32()][,];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        m_BindPose[i] = new[,] {
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() } };
                    }

                    int m_UV1_size = reader.ReadInt32();
                    m_UV1 = new float[m_UV1_size * 2];
                    for (int v = 0; v < m_UV1_size * 2; v++) { m_UV1[v] = reader.ReadSingle(); }

                    int m_UV2_size = reader.ReadInt32();
                    m_UV2 = new float[m_UV2_size * 2];
                    for (int v = 0; v < m_UV2_size * 2; v++) { m_UV2[v] = reader.ReadSingle(); }

                    if (version[0] == 2 && version[1] <= 5)
                    {
                        int m_TangentSpace_size = reader.ReadInt32();
                        m_Normals = new float[m_TangentSpace_size * 3];
                        for (int v = 0; v < m_TangentSpace_size; v++)
                        {
                            m_Normals[v * 3] = reader.ReadSingle();
                            m_Normals[v * 3 + 1] = reader.ReadSingle();
                            m_Normals[v * 3 + 2] = reader.ReadSingle();
                            reader.Position += 16; //Vector3f tangent & float handedness 
                        }
                    }
                    else //2.6.0 and later
                    {
                        int m_Tangents_size = reader.ReadInt32();
                        reader.Position += m_Tangents_size * 16; //Vector4f

                        int m_Normals_size = reader.ReadInt32();
                        m_Normals = new float[m_Normals_size * 3];
                        for (int v = 0; v < m_Normals_size * 3; v++) { m_Normals[v] = reader.ReadSingle(); }
                    }
                }
                #endregion
                #region Vertex Buffer for 3.5.0 and later
                else
                {
                    #region read vertex stream

                    if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
                    {
                        m_Skin = new List<BoneInfluence>[reader.ReadInt32()];
                        for (int s = 0; s < m_Skin.Length; s++)
                        {
                            m_Skin[s] = new List<BoneInfluence>();
                            for (int i = 0; i < 4; i++)
                            {
                                m_Skin[s].Add(new BoneInfluence() { weight = reader.ReadSingle() });
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                m_Skin[s][i].boneIndex = reader.ReadInt32();
                            }
                        }
                    }

                    if (version[0] == 3 || (version[0] == 4 && version[1] <= 2))
                    {
                        m_BindPose = new float[reader.ReadInt32()][,];
                        for (int i = 0; i < m_BindPose.Length; i++)
                        {
                            m_BindPose[i] = new[,] {
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                        { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() } };
                        }
                    }

                    if (version[0] < 2018)//2018 down
                    {
                        var m_CurrentChannels = reader.ReadInt32();
                    }

                    m_VertexCount = reader.ReadInt32();
                    int streamCount = 0;

                    #region streams for 3.5.0 - 3.5.7
                    if (version[0] < 4)
                    {
                        if (m_MeshCompression != 0 && version[2] == 0) //special case not just on platform 9
                        {
                            reader.Position += 12;
                        }
                        else
                        {
                            m_Streams = new StreamInfo[4];
                            for (int s = 0; s < 4; s++)
                            {
                                m_Streams[s] = new StreamInfo();
                                m_Streams[s].channelMask = new BitArray(new int[1] { reader.ReadInt32() });
                                m_Streams[s].offset = reader.ReadInt32();
                                m_Streams[s].stride = reader.ReadInt32();
                                m_Streams[s].align = reader.ReadUInt32();
                            }
                        }
                    }
                    #endregion
                    #region channels and streams for 4.0.0 and later
                    else
                    {
                        m_Channels = new ChannelInfo[reader.ReadInt32()];
                        for (int c = 0; c < m_Channels.Length; c++)
                        {
                            m_Channels[c] = new ChannelInfo();
                            m_Channels[c].stream = reader.ReadByte();
                            m_Channels[c].offset = reader.ReadByte();
                            m_Channels[c].format = reader.ReadByte();
                            m_Channels[c].dimension = reader.ReadByte();

                            if (m_Channels[c].stream >= streamCount) { streamCount = m_Channels[c].stream + 1; }
                        }

                        if (version[0] < 5)
                        {
                            m_Streams = new StreamInfo[reader.ReadInt32()];
                            for (int s = 0; s < m_Streams.Length; s++)
                            {
                                m_Streams[s] = new StreamInfo();
                                m_Streams[s].channelMask = new BitArray(new[] { reader.ReadInt32() });
                                m_Streams[s].offset = reader.ReadInt32();
                                m_Streams[s].stride = reader.ReadByte();
                                m_Streams[s].dividerOp = reader.ReadByte();
                                m_Streams[s].frequency = reader.ReadUInt16();
                            }
                        }
                    }
                    #endregion
                    if (version[0] >= 5) //ComputeCompressedStreams
                    {
                        m_Streams = new StreamInfo[streamCount];
                        int offset = 0;
                        for (int s = 0; s < streamCount; s++)
                        {
                            int chnMask = 0;
                            int stride = 0;
                            for (int chn = 0; chn < m_Channels.Length; chn++)
                            {
                                var m_Channel = m_Channels[chn];
                                if (m_Channel.stream == s)
                                {
                                    if (m_Channel.dimension > 0)
                                    {
                                        chnMask |= 1 << chn;
                                        stride += m_Channel.dimension * GetChannelFormatSize(m_Channel.format);
                                    }
                                }
                            }
                            m_Streams[s] = new StreamInfo
                            {
                                channelMask = new BitArray(new[] { chnMask }),
                                offset = offset,
                                stride = stride,
                                dividerOp = 0,
                                frequency = 0
                            };
                            offset += m_VertexCount * stride;
                            //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                            offset = (offset + (16 - 1)) & ~(16 - 1);
                        }
                    }

                    //actual Vertex Buffer
                    var m_DataSize = reader.ReadBytes(reader.ReadInt32());
                    #endregion

                    #region compute FvF
                    #region 2018 and up
                    if (version[0] >= 2018)
                    {
                        InitMSkin();
                        foreach (var m_Channel in m_Channels)
                        {
                            if (m_Channel.dimension > 0)
                            {
                                var m_Stream = m_Streams[m_Channel.stream];

                                for (int b = 0; b < 14; b++)
                                {
                                    if (m_Stream.channelMask.Get(b))
                                    {
                                        var componentByteSize = GetChannelFormatSize(m_Channel.format);
                                        var componentBytes = new byte[m_VertexCount * m_Channel.dimension * componentByteSize];

                                        for (int v = 0; v < m_VertexCount; v++)
                                        {
                                            int vertexOffset = m_Stream.offset + m_Channel.offset + m_Stream.stride * v;
                                            for (int d = 0; d < m_Channel.dimension; d++)
                                            {
                                                int componentOffset = vertexOffset + componentByteSize * d;
                                                Buffer.BlockCopy(m_DataSize, componentOffset, componentBytes, componentByteSize * (v * m_Channel.dimension + d), componentByteSize);
                                            }
                                        }

                                        if (preloadData.sourceFile.platform == 11 && componentByteSize > 1) //swap bytes for Xbox
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
                                        if (m_Channel.format == 11)
                                            componentsIntArray = BytesToIntArray(componentBytes);
                                        else
                                            componentsFloatArray = BytesToFloatArray(componentBytes, componentByteSize);

                                        switch (b)
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
                                                m_UV1 = componentsFloatArray;
                                                break;
                                            case 5: //kShaderChannelTexCoord1
                                                m_UV2 = componentsFloatArray;
                                                break;
                                            case 6: //kShaderChannelTexCoord2
                                                m_UV3 = componentsFloatArray;
                                                break;
                                            case 7: //kShaderChannelTexCoord3
                                                m_UV4 = componentsFloatArray;
                                                break;
                                            //2018.2 and up
                                            case 12:
                                                for (int i = 0; i < m_VertexCount; i++)
                                                {
                                                    for (int j = 0; j < 4; j++)
                                                    {
                                                        m_Skin[i][j].weight = componentsFloatArray[i * 4 + j];
                                                    }
                                                }
                                                break;
                                            case 13:
                                                for (int i = 0; i < m_VertexCount; i++)
                                                {
                                                    for (int j = 0; j < 4; j++)
                                                    {
                                                        m_Skin[i][j].boneIndex = componentsIntArray[i * 4 + j];
                                                    }
                                                }
                                                break;
                                        }

                                        m_Stream.channelMask.Set(b, false);
                                        break; //go to next channel
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        #region 4.0 - 2017.x
                        if (m_Channels != null)
                        {
                            //it is better to loop channels instead of streams
                            //because channels are likely to be sorted by vertex property
                            foreach (var m_Channel in m_Channels)
                            {
                                if (m_Channel.dimension > 0)
                                {
                                    var m_Stream = m_Streams[m_Channel.stream];

                                    for (int b = 0; b < 8; b++)
                                    {
                                        if (m_Stream.channelMask.Get(b))
                                        {
                                            //in version 4.x the colors is ColorRGBA32, size 4 with 4 byte components
                                            if (b == 2 && m_Channel.format == 2)
                                            {
                                                m_Channel.dimension = 4; //set this so that don't need to convert int to 4 bytes
                                            }

                                            var componentByteSize = GetChannelFormatSize(m_Channel.format);
                                            var componentBytes = new byte[m_VertexCount * m_Channel.dimension * componentByteSize];

                                            for (int v = 0; v < m_VertexCount; v++)
                                            {
                                                int vertexOffset = m_Stream.offset + m_Channel.offset + m_Stream.stride * v;
                                                for (int d = 0; d < m_Channel.dimension; d++)
                                                {
                                                    int componentOffset = vertexOffset + componentByteSize * d;
                                                    Buffer.BlockCopy(m_DataSize, componentOffset, componentBytes, componentByteSize * (v * m_Channel.dimension + d), componentByteSize);
                                                }
                                            }

                                            if (preloadData.sourceFile.platform == 11 && componentByteSize > 1) //swap bytes for Xbox
                                            {
                                                for (var i = 0; i < componentBytes.Length / componentByteSize; i++)
                                                {
                                                    var buff = new byte[componentByteSize];
                                                    Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
                                                    buff = buff.Reverse().ToArray();
                                                    Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
                                                }
                                            }

                                            var componentsFloatArray = BytesToFloatArray(componentBytes, componentByteSize);

                                            switch (b)
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
                                                    m_UV1 = componentsFloatArray;
                                                    break;
                                                case 4: //kShaderChannelTexCoord1
                                                    m_UV2 = componentsFloatArray;
                                                    break;
                                                case 5: //kShaderChannelTangent & kShaderChannelTexCoord2
                                                    if (version[0] >= 5)
                                                    {
                                                        m_UV3 = componentsFloatArray;
                                                    }
                                                    else
                                                    {
                                                        m_Tangents = componentsFloatArray;
                                                    }
                                                    break;
                                                case 6: //kShaderChannelTexCoord3
                                                    m_UV4 = componentsFloatArray;
                                                    break;
                                                case 7: //kShaderChannelTangent
                                                    m_Tangents = componentsFloatArray;
                                                    break;
                                            }

                                            m_Stream.channelMask.Set(b, false);
                                            break; //go to next channel
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region 3.5.0 - 3.5.7
                        else if (m_Streams != null)
                        {
                            foreach (var m_Stream in m_Streams)
                            {
                                //a stream may have multiple vertex components but without channels there are no offsets, so I assume all vertex properties are in order
                                //version 3.5.x only uses floats, and that's probably why channels were introduced in version 4

                                ChannelInfo m_Channel = new ChannelInfo();//create my own channel so I can use the same methods
                                m_Channel.offset = 0;
                                int componentByteSize = 0;
                                for (int b = 0; b < 6; b++)
                                {
                                    if (m_Stream.channelMask.Get(b))
                                    {
                                        switch (b)
                                        {
                                            case 0:
                                            case 1:
                                                componentByteSize = 4;
                                                m_Channel.dimension = 3;
                                                break;
                                            case 2:
                                                componentByteSize = 1;
                                                m_Channel.dimension = 4;
                                                break;
                                            case 3:
                                            case 4:
                                                componentByteSize = 4;
                                                m_Channel.dimension = 2;
                                                break;
                                            case 5:
                                                componentByteSize = 4;
                                                m_Channel.dimension = 4;
                                                break;
                                        }

                                        var componentBytes = new byte[componentByteSize];
                                        var componentsArray = new float[m_VertexCount * m_Channel.dimension];

                                        for (int v = 0; v < m_VertexCount; v++)
                                        {
                                            int vertexOffset = m_Stream.offset + m_Channel.offset + m_Stream.stride * v;
                                            for (int d = 0; d < m_Channel.dimension; d++)
                                            {
                                                int m_DataSizeOffset = vertexOffset + componentByteSize * d;
                                                Buffer.BlockCopy(m_DataSize, m_DataSizeOffset, componentBytes, 0, componentByteSize);
                                                componentsArray[v * m_Channel.dimension + d] = BytesToFloat(componentBytes);
                                            }
                                        }

                                        switch (b)
                                        {
                                            case 0: m_Vertices = componentsArray; break;
                                            case 1: m_Normals = componentsArray; break;
                                            case 2: m_Colors = componentsArray; break;
                                            case 3: m_UV1 = componentsArray; break;
                                            case 4: m_UV2 = componentsArray; break;
                                            case 5: m_Tangents = componentsArray; break;
                                        }

                                        m_Channel.offset += (byte)(m_Channel.dimension * componentByteSize); //safe to cast as byte because strides larger than 255 are unlikely
                                        m_Stream.channelMask.Set(b, false);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion

                #region Compressed Mesh data for 2.6.0 and later - 160 bytes
                if (version[0] >= 3 || (version[0] == 2 && version[1] >= 6))
                {
                    #region m_Vertices;
                    var m_Vertices_Packed = new PackedFloatVector(reader);
                    if (m_Vertices_Packed.m_NumItems > 0)
                    {
                        m_VertexCount = (int)m_Vertices_Packed.m_NumItems / 3;
                        m_Vertices = m_Vertices_Packed.UnpackFloats(3, 4);
                    }
                    #endregion

                    #region m_UV
                    var m_UV_Packed = new PackedFloatVector(reader);
                    if (m_UV_Packed.m_NumItems > 0)
                    {
                        m_UV1 = m_UV_Packed.UnpackFloats(2, 4, 0, m_VertexCount);
                        if (m_UV_Packed.m_NumItems >= m_VertexCount * 4)
                        {
                            m_UV2 = m_UV_Packed.UnpackFloats(2, 4, m_VertexCount * 2, m_VertexCount);
                        }
                        if (m_UV_Packed.m_NumItems >= m_VertexCount * 6)
                        {
                            m_UV3 = m_UV_Packed.UnpackFloats(2, 4, m_VertexCount * 4, m_VertexCount);
                        }
                        if (m_UV_Packed.m_NumItems >= m_VertexCount * 8)
                        {
                            m_UV4 = m_UV_Packed.UnpackFloats(2, 4, m_VertexCount * 6, m_VertexCount);
                        }
                    }
                    #endregion

                    #region m_BindPoses
                    if (version[0] < 5)
                    {
                        var m_BindPoses_Packed = new PackedFloatVector(reader);
                        if (m_BindPoses_Packed.m_NumItems > 0)
                        {
                            m_BindPose = new float[m_BindPoses_Packed.m_NumItems / 16][,];
                            var m_BindPoses_Unpacked = m_BindPoses_Packed.UnpackFloats(16, 4 * 16);
                            throw new NotImplementedException();
                        }
                    }
                    #endregion

                    var m_Normals_Packed = new PackedFloatVector(reader);

                    var m_Tangents_Packed = new PackedFloatVector(reader);

                    var m_Weights = new PackedIntVector(reader);

                    #region m_Normals
                    var m_NormalSigns = new PackedIntVector(reader);
                    if (m_Normals_Packed.m_NumItems > 0)
                    {
                        var normalData = m_Normals_Packed.UnpackFloats(2, 4 * 2);
                        var signs = m_NormalSigns.UnpackInts();
                        m_Normals = new float[m_Normals_Packed.m_NumItems / 2 * 3];
                        for (int i = 0; i < m_Normals_Packed.m_NumItems / 2; ++i)
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
                    #endregion

                    #region m_Tangents
                    var m_TangentSigns = new PackedIntVector(reader);
                    if (m_Tangents_Packed.m_NumItems > 0)
                    {
                        var tangentData = m_Tangents_Packed.UnpackFloats(2, 4 * 2);
                        var signs = m_TangentSigns.UnpackInts();
                        m_Tangents = new float[m_Tangents_Packed.m_NumItems / 2 * 4];
                        for (int i = 0; i < m_Tangents_Packed.m_NumItems / 2; ++i)
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
                    #endregion

                    #region m_FloatColors
                    if (version[0] >= 5)
                    {
                        var m_FloatColors = new PackedFloatVector(reader);
                        if (m_FloatColors.m_NumItems > 0)
                        {
                            m_Colors = m_FloatColors.UnpackFloats(1, 4);
                        }
                    }
                    #endregion

                    #region m_Skin
                    var m_BoneIndices = new PackedIntVector(reader);
                    if (m_Weights.m_NumItems > 0)
                    {
                        var weights = m_Weights.UnpackInts();
                        var boneIndices = m_BoneIndices.UnpackInts();

                        InitMSkin();

                        int bonePos = 0;
                        int boneIndexPos = 0;
                        int j = 0;
                        int sum = 0;

                        for (int i = 0; i < m_Weights.m_NumItems; i++)
                        {
                            //read bone index and weight.
                            m_Skin[bonePos][j].weight = weights[i] / 31.0f;
                            m_Skin[bonePos][j].boneIndex = boneIndices[boneIndexPos++];
                            j++;
                            sum += weights[i];

                            //the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
                            if (sum >= 31)
                            {
                                for (; j < 4; j++)
                                {
                                    m_Skin[bonePos][j].weight = 0;
                                    m_Skin[bonePos][j].boneIndex = 0;
                                }
                                bonePos++;
                                j = 0;
                                sum = 0;
                            }
                            //we read three weights, but they don't add up to one. calculate the fourth one, and read
                            //missing bone index. continue with next vertex.
                            else if (j == 3)
                            {
                                m_Skin[bonePos][j].weight = (31 - sum) / 31.0f;
                                m_Skin[bonePos][j].boneIndex = boneIndices[boneIndexPos++];
                                bonePos++;
                                j = 0;
                                sum = 0;
                            }
                        }
                    }
                    #endregion

                    #region m_IndexBuffer
                    var m_Triangles = new PackedIntVector(reader);
                    if (m_Triangles.m_NumItems > 0)
                    {
                        m_IndexBuffer = Array.ConvertAll(m_Triangles.UnpackInts(), x => (uint)x);
                    }
                    #endregion
                }
                #endregion

                #region Colors & Collision triangles for 3.4.2 and earlier
                if (version[0] <= 2 || (version[0] == 3 && version[1] <= 4))
                {
                    reader.Position += 24; //Axis-Aligned Bounding Box
                    int m_Colors_size = reader.ReadInt32();
                    m_Colors = new float[m_Colors_size * 4];
                    for (int v = 0; v < m_Colors_size * 4; v++) { m_Colors[v] = (float)(reader.ReadByte()) / 0xFF; }

                    int m_CollisionTriangles_size = reader.ReadInt32();
                    reader.Position += m_CollisionTriangles_size * 4; //UInt32 indices
                    int m_CollisionVertexCount = reader.ReadInt32();
                }
                #endregion
                #region Compressed colors
                else
                {
                    if (version[0] < 5)
                    {
                        var m_Colors_Packed = new PackedIntVector(reader);
                        if (m_Colors_Packed.m_NumItems > 0)
                        {
                            m_Colors_Packed.m_NumItems *= 4;
                            m_Colors_Packed.m_BitSize /= 4;
                            var tempColors = m_Colors_Packed.UnpackInts();
                            m_Colors = new float[m_Colors_Packed.m_NumItems];
                            for (int v = 0; v < m_Colors_Packed.m_NumItems; v++)
                            {
                                m_Colors[v] = tempColors[v] / 255f;
                            }
                        }
                    }
                    else
                    {
                        var m_UVInfo = reader.ReadUInt32();
                    }

                    reader.Position += 24; //AABB m_LocalAABB
                }
                #endregion

                int m_MeshUsageFlags = reader.ReadInt32();

                if (version[0] >= 5)
                {
                    //int m_BakedConvexCollisionMesh = a_Stream.ReadInt32();
                    //a_Stream.Position += m_BakedConvexCollisionMesh;
                    //int m_BakedTriangleCollisionMesh = a_Stream.ReadInt32();
                    //a_Stream.Position += m_BakedConvexCollisionMesh;
                }

                #region Build face indices
                for (int s = 0; s < m_SubMeshes_size; s++)
                {
                    uint firstIndex = m_SubMeshes[s].firstByte / 2;
                    if (!m_Use16BitIndices) { firstIndex /= 2; }

                    if (m_SubMeshes[s].topology == 0)
                    {
                        for (int i = 0; i < m_SubMeshes[s].indexCount / 3; i++)
                        {
                            m_Indices.Add(m_IndexBuffer[firstIndex + i * 3]);
                            m_Indices.Add(m_IndexBuffer[firstIndex + i * 3 + 1]);
                            m_Indices.Add(m_IndexBuffer[firstIndex + i * 3 + 2]);
                            m_materialIDs.Add(s);
                        }
                    }
                    else
                    {
                        uint j = 0;
                        for (int i = 0; i < m_SubMeshes[s].indexCount - 2; i++)
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
                                m_materialIDs.Add(s);
                                j++;
                            }
                        }
                        //TODO just fix it
                        m_SubMeshes[s].indexCount = j * 3;
                    }
                }
                #endregion
            }
            else
            {
                preloadData.extension = ".obj";
                preloadData.Text = m_Name;
            }
        }
    }
}
