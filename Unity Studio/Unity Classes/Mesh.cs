using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

/*Notes about handedness
Converting from left-handed to right-handed and vice versa requires either:
a) swapping any 2 axes
b) flipping any 1 axis
c) any odd combinations of a&b

An even number of combinations will result in the same handedness, but in a different perspective.
Also, rotating the axes is just that, the same handedness in a different system.

Y-up or Z-up are not requirements OR defining characteristics of a handedness, they are just common that way.

Unity is left-handed with Y-up
Aircraft              View
Y Top                          Y Up
|                              |
|    Z Nose                    |
|   /                          |
| /                            |
+--------X           X--------+
		Right       to the       \
					left           \
									 Z towards viewer

3ds Max is right-handed with Z up
Aircraft              View
Z Top                          Z Up (Viewcube Top)
|                              |
|    (-Y) Nose       away Y    |
|   /                from  \   |
| /                  viewer  \ |
+--------(-X)       (VC Back)  +--------X to the right
		 Right                         (Viewcube Right)



FBX and Maya are both right-handed but with Y up! (90 degree rotation on X = samme handedness)
Aircraft              View
Y Top                 Y Up (Viewcube Top)
|                     |
|    Z Nose           |
|   /                 |
| /                   |
+--------(-X)         +--------X to the right
		 Right          \        (Viewcube Right)
						  \
							Z towards viewer
							  (Viewcube Front)

Exporting FBX from Max, vertex components are ALWAYS written as they are in max: X Y Z.
The Axis option only affects GlobalSettings and PreRotation in the FBX file.
"Z-up" option: 
UpAxis=2,Sign=1     <=> ViewCube Up = FBX vertex[2] <=> Max Z = FBX Z <=> Maya Y = FBX Z
FrontAxis=1,Sign=-1 <=> ViewCube -Front = FBX v[1] <=> Max -(-Y) = FBX Y <=> Maya -Z = FBX Y
CoordAxis=0,Sign=1  <=> ViewCube Right = FBX v[0] <=> Max X = FBX X <=> Maya X = FBX X
no PreRotation

"Y-up" option:
UpAxis=1,Sign=1    <=> ViewCube Up = FBX vertex[1] <=> Max Z = FBX Y <=> Maya Y = FBX Y
FrontAxis=2,Sign=1 <=> ViewCube Front = FBX v[2] <=> Max -Y = FBX Z <=> Maya Z = FBX Z
CoordAxis=0,Sign=1 <=> ViewCube Right = FBX v[0] <=> Max X = FBX X <=> Maya X = FBX X
PreRotation -90,0,0 to bring Original Up (FBX Z) to ViewCube Up when importing
PreRotation means only the geometry is rotated locally around pivot before applying other modifiers. It is "invisible" to the user.


Importing FBX in Unity, Axis settings and PreRotations are ignored.
They probably ignore them because the order of vertex components is always the same in FBX, and Unity axes never change orientation (as opposed to Max-Maya).
Vertex components are loaded as follows:
Unity Up(Y) = FBX Y
Unity Front(Z) = FBX Z
Unity Left(X) = FBX -X

Technically, this is a correct handedness conversion, but to a different system, because the model is not properly oriented (plane nose is down).
So Unity adds a -90 degree rotation, similar to the FBX PreRotation, to bring the nose to Front(Z).
Except it does it as a regular rotation, and combines it with any other rotations in the Transform asset.

Converting from Unity back to FBX, the same vertex conversion cannot be applied because we have to take into account the rotation.
Option 0: export vertices and transformations as -X,Y,Z and set FBX option to Y-up without PreRotation!
the result will be Max Z = FBX Y, Max -Y = FBX Z, Max X = FBX X => final order -X -Z Y
Option 1: export vertices and transformations as -X,-Z,Y and set FBX options as "Z-up".
The -90 rotation exported from Unity will bring the model in correct orientation.
Option 2: export vertices and transformations as -X,-Y,-Z, add -90 PreRotation to every Mesh Node and set FBX options as "Y-up".
The -90 rotation from Unity plus the -90 PreRotation will bring the model in correct orientation.
Remember though that the PreRotation is baked into the Geometry.

But since the -90 rotation from Unity is a regular type, it will show up in the modifier in both cases.
Also, re-importing this FBX in Unity will now produce a (-90)+(-90)=-180 rotation.
This is an unfortunate eyesore, but nothing more, the orientation will be fine.

In theory, one could add +90 degrees rotation to GameObjects that link to Mesh assets (but not other types) to cancel out the Unity rotation.
The problem is you can never know where the Unity mesh originated from. If it came from a left-handed format such as OBJ, there wouldn't have been any conversion and it wouldn't have that -90 degree adjustment.
So it would "fix" meshes that were originally sourced form FBX, but would still have the "extra" rotation in mehses sourced from left-handed formats.
*/

namespace Unity_Studio
{
    public class Mesh
    {
        private EndianBinaryReader a_Stream;
        public string m_Name;
        public List<SubMesh> m_SubMeshes = new List<SubMesh>();
        public List<uint> m_Indices = new List<uint>(); //use a list because I don't always know the facecount for triangle strips
        public List<int> m_materialIDs = new List<int>();
        private uint[] m_IndexBuffer;
        private ChannelInfo[] m_Channels;
        private StreamInfo[] m_Streams;
        public List<BoneInfluence>[] m_Skin;
        //public Dictionary<int, float>[] m_Skin;
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

        public class PackedBitVector
        {
            public int m_NumItems;
            public float m_Range = 1.0f;
            public float m_Start;
            public byte[] m_Data;
            public byte m_BitSize;
        }

        public float bytesToFloat(byte[] inputBytes)
        {
            float result = 0;
            if (a_Stream.endian == EndianType.BigEndian) { Array.Reverse(inputBytes); }

            switch (inputBytes.Length)
            {
                case 1:
                    result = inputBytes[0] / 255.0f;
                    break;
                case 2:
                    result = Half.ToHalf(inputBytes, 0);
                    break;
                case 4:
                    result = BitConverter.ToSingle(inputBytes, 0);
                    break;
            }

            return result;
        }

        public uint[] UnpackBitVector(PackedBitVector pakData)
        {
            uint[] unpackedVectors = new uint[pakData.m_NumItems];
            //int bitmax = 0;//used to convert int value to float
            //for (int b = 0; b < pakData.m_BitSize; b++) { bitmax |= (1 << b); }

            //the lazy way
            //split data into groups of "aligned" bytes i.e. 8 packed values per group
            //I could calculate optimized group size based on BitSize, but this is the lazy way

            if (pakData.m_BitSize == 0)
            {
                pakData.m_BitSize = (byte)((pakData.m_Data.Length * 8) / pakData.m_NumItems);
                //don't know, don't care
            }
            int groupSize = pakData.m_BitSize; //bitSize * 8 values / 8 bits
            byte[] group = new byte[groupSize];
            int groupCount = pakData.m_NumItems / 8;

            for (int g = 0; g < groupCount; g++)
            {
                Buffer.BlockCopy(pakData.m_Data, g * groupSize, group, 0, groupSize);
                BitArray groupBits = new BitArray(group);

                for (int v = 0; v < 8; v++)
                {
                    BitArray valueBits = new BitArray(new Boolean[pakData.m_BitSize]);
                    for (int b = 0; b < pakData.m_BitSize; b++)
                    {
                        valueBits.Set(b, groupBits.Get(b + v * pakData.m_BitSize));
                    }

                    var valueArr = new int[1];
                    valueBits.CopyTo(valueArr, 0);
                    //unpackedVectors[v + g * 8] = (float)(valueArr[0] / bitmax) * pakData.m_Range + pakData.m_Start;
                    //valueBits.CopyTo(unpackedVectors, v + g * 8);//doesn't work with uint[]
                    unpackedVectors[v + g * 8] = (uint)valueArr[0];
                }
            }

            //m_NumItems is not necessarily a multiple of 8, so there can be one extra group with fewer values
            int endBytes = pakData.m_Data.Length - groupCount * groupSize;
            int endVal = pakData.m_NumItems - groupCount * 8;

            if (endBytes > 0)
            {
                Buffer.BlockCopy(pakData.m_Data, groupCount * groupSize, group, 0, endBytes);
                BitArray groupBits = new BitArray(group);

                for (int v = 0; v < endVal; v++)
                {
                    BitArray valueBits = new BitArray(new Boolean[pakData.m_BitSize]);
                    for (int b = 0; b < pakData.m_BitSize; b++)
                    {
                        valueBits.Set(b, groupBits.Get(b + v * pakData.m_BitSize));
                    }

                    var valueArr = new int[1];
                    valueBits.CopyTo(valueArr, 0);
                    //unpackedVectors[v + groupCount * 8] = (float)(valueArr[0] / bitmax) * pakData.m_Range + pakData.m_Start;
                    //valueBits.CopyTo(unpackedVectors, v + groupCount * 8);
                    unpackedVectors[v + groupCount * 8] = (uint)valueArr[0];
                }
            }


            //the hard way
            //compute bit position in m_Data for each value
            /*byte[] value = new byte[4] { 0, 0, 0, 0 };

			int byteCount = pakData.m_BitSize / 8;//bytes in single value
			int bitCount = pakData.m_BitSize % 8;

			for (int v = 0; v < pakData.m_NumItems; v++)
			{
				if ((bitCount * v) % 8 == 0) //bitstream is "aligned"
				{//does this make sense if I'm gonna compute unaligned anywhay?
					for (int b = 0; b < byteCount; b++)
					{
						value[b] = pakData.m_Data[b + v * (byteCount+1)];
					}

					if (byteCount < 4) //shouldn't it be always?
					{
						byte lastByte = pakData.m_Data[bitCount * v / 8];

						for (int b = 0; b < bitCount; b++)//no
						{
							//set bit in val[byteCount+1]
						}
					}
				}
				else
				{
					//god knows
				}

				unpackedVectors[v] = BitConverter.ToSingle(value, 0);
			}*/


            //first I split the data into byte-aligned arrays
            //too complicated to calculate group size each time
            //then no point in dividing?
            /*int groupSize = byteCount + (bitCount + 7)/8;

			int groups = pakData.m_Data.Length / groupSize;
			int valPerGr = (int)(pakData.m_NumItems / groups);
			byte[] group = new byte[groupSize];

			for (int g = 0; g < groups; g++)
			{
				Buffer.BlockCopy(pakData.m_Data, g * groupSize, group, 0, groupSize);

				for (int v = 0; v < valPerGr; v++)
				{

					unpackedVectors[v + g * valPerGr] = BitConverter.ToSingle(value, 0);
				}
			}

			//m_Data size is not necessarily a multiple of align, so there can be one extra group with fewer values
			int lastBytes = pakData.m_Data.Length % groupSize;
			int lastVal = (int)(pakData.m_NumItems - groups * valPerGr);

			if (lastBytes > 0)
			{
				Buffer.BlockCopy(pakData.m_Data, groups * groupSize, group, 0, lastBytes);

				for (int v = 0; v < lastVal; v++)
				{

					unpackedVectors[v + groups * valPerGr] = BitConverter.ToSingle(value, 0);
				}
			}*/

            return unpackedVectors;
        }

        public Mesh(AssetPreloadData MeshPD, bool readSwitch)
        {
            //Stream = new EndianStream(File.OpenRead(sourceFile.filePath), sourceFile.endianType);
            //Stream.endian = sourceFile.endianType;
            var version = MeshPD.sourceFile.version;
            a_Stream = MeshPD.sourceFile.a_Stream;
            a_Stream.Position = MeshPD.Offset;
            bool m_Use16BitIndices = true; //3.5.0 and newer always uses 16bit indices
            uint m_MeshCompression = 0;

            if (MeshPD.sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = MeshPD.sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = MeshPD.sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());

            if (readSwitch)
            {

                if (version[0] < 3 || (version[0] == 3 && version[1] < 5))
                {
                    m_Use16BitIndices = a_Stream.ReadBoolean();
                    a_Stream.Position += 3;
                }

                #region Index Buffer for 2.5.1 and earlier
                if (version[0] == 2 && version[1] <= 5)
                {
                    int m_IndexBuffer_size = a_Stream.ReadInt32();

                    if (m_Use16BitIndices)
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                        for (int i = 0; i < m_IndexBuffer_size / 2; i++) { m_IndexBuffer[i] = a_Stream.ReadUInt16(); }
                        a_Stream.AlignStream(4);
                    }
                    else
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 4];
                        for (int i = 0; i < m_IndexBuffer_size / 4; i++) { m_IndexBuffer[i] = a_Stream.ReadUInt32(); }
                    }
                }
                #endregion

                #region subMeshes
                int m_SubMeshes_size = a_Stream.ReadInt32();
                for (int s = 0; s < m_SubMeshes_size; s++)
                {
                    m_SubMeshes.Add(new SubMesh());
                    m_SubMeshes[s].firstByte = a_Stream.ReadUInt32();
                    m_SubMeshes[s].indexCount = a_Stream.ReadUInt32(); //what is this in case of triangle strips?
                    m_SubMeshes[s].topology = a_Stream.ReadInt32(); //isTriStrip
                    if (version[0] < 4)
                    {
                        m_SubMeshes[s].triangleCount = a_Stream.ReadUInt32();
                    }
                    if (version[0] >= 3)
                    {
                        if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3))//2017.3 and up
                        {
                            var baseVertex = a_Stream.ReadUInt32();
                        }
                        m_SubMeshes[s].firstVertex = a_Stream.ReadUInt32();
                        m_SubMeshes[s].vertexCount = a_Stream.ReadUInt32();
                        a_Stream.Position += 24; //Axis-Aligned Bounding Box
                    }
                }
                #endregion

                #region BlendShapeData for 4.1.0 to 4.2.x, excluding 4.1.0 alpha
                if (version[0] == 4 && ((version[1] == 1 && MeshPD.sourceFile.buildType[0] != "a") ||
                                        (version[1] > 1 && version[1] <= 2)))
                {
                    int m_Shapes_size = a_Stream.ReadInt32();
                    if (m_Shapes_size > 0)
                    {
                        //bool stop = true;
                    }
                    for (int s = 0; s < m_Shapes_size; s++) //untested
                    {
                        string shape_name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                        a_Stream.Position += 36; //uint firstVertex, vertexCount; Vector3f aabbMinDelta, aabbMaxDelta; bool hasNormals, hasTangents
                    }

                    int m_ShapeVertices_size = a_Stream.ReadInt32();
                    a_Stream.Position += m_ShapeVertices_size * 40; //vertex positions, normals, tangents & uint index
                }
                #endregion
                #region BlendShapeData and BindPose for 4.3.0 and later
                else if (version[0] >= 5 || (version[0] == 4 && version[1] >= 3))
                {
                    int m_ShapeVertices_size = a_Stream.ReadInt32();
                    if (m_ShapeVertices_size > 0)
                    {
                        //bool stop = true;
                    }
                    a_Stream.Position += m_ShapeVertices_size * 40; //vertex positions, normals, tangents & uint index

                    int shapes_size = a_Stream.ReadInt32();
                    a_Stream.Position += shapes_size * 12; //uint firstVertex, vertexCount; bool hasNormals, hasTangents

                    int channels_size = a_Stream.ReadInt32();
                    for (int c = 0; c < channels_size; c++)
                    {
                        string channel_name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                        a_Stream.Position += 12; //uint nameHash; int frameIndex, frameCount
                    }

                    int fullWeights_size = a_Stream.ReadInt32();
                    a_Stream.Position += fullWeights_size * 4; //floats

                    m_BindPose = new float[a_Stream.ReadInt32()][,];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        m_BindPose[i] = new[,] {
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() } };
                    }

                    int m_BoneNameHashes_size = a_Stream.ReadInt32();
                    a_Stream.Position += m_BoneNameHashes_size * 4; //uints

                    uint m_RootBoneNameHash = a_Stream.ReadUInt32();
                }
                #endregion

                #region Index Buffer for 2.6.0 and later
                if (version[0] >= 3 || (version[0] == 2 && version[1] >= 6))
                {
                    m_MeshCompression = a_Stream.ReadByte();
                    if (version[0] >= 4)
                    {
                        if (version[0] < 5) { uint m_StreamCompression = a_Stream.ReadByte(); }
                        bool m_IsReadable = a_Stream.ReadBoolean();
                        bool m_KeepVertices = a_Stream.ReadBoolean();
                        bool m_KeepIndices = a_Stream.ReadBoolean();
                    }
                    a_Stream.AlignStream(4);
                    if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 3))//2017.3 and up
                    {
						if(m_MeshCompression == 0)
						{
							var m_IndexFormat = a_Stream.ReadInt32();	
						}
                    }
                    int m_IndexBuffer_size = a_Stream.ReadInt32();

                    if (m_Use16BitIndices)
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 2];
                        for (int i = 0; i < m_IndexBuffer_size / 2; i++) { m_IndexBuffer[i] = a_Stream.ReadUInt16(); }
                        a_Stream.AlignStream(4);
                    }
                    else
                    {
                        m_IndexBuffer = new uint[m_IndexBuffer_size / 4];
                        for (int i = 0; i < m_IndexBuffer_size / 4; i++) { m_IndexBuffer[i] = a_Stream.ReadUInt32(); }
                        a_Stream.AlignStream(4);//untested
                    }
                }
                #endregion

                #region Vertex Buffer for 3.4.2 and earlier
                if (version[0] < 3 || (version[0] == 3 && version[1] < 5))
                {
                    m_VertexCount = a_Stream.ReadInt32();
                    m_Vertices = new float[m_VertexCount * 3];
                    for (int v = 0; v < m_VertexCount * 3; v++) { m_Vertices[v] = a_Stream.ReadSingle(); }

                    m_Skin = new List<BoneInfluence>[a_Stream.ReadInt32()];
                    //m_Skin = new Dictionary<int, float>[a_Stream.ReadInt32()];
                    for (int s = 0; s < m_Skin.Length; s++)
                    {
                        m_Skin[s] = new List<BoneInfluence>();
                        for (int i = 0; i < 4; i++) { m_Skin[s].Add(new BoneInfluence() { weight = a_Stream.ReadSingle() }); }
                        for (int i = 0; i < 4; i++) { m_Skin[s][i].boneIndex = a_Stream.ReadInt32(); }

                        /*m_Skin[s] = new Dictionary<int, float>();
						float[] weights = new float[4] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
						for (int i = 0; i < 4; i++)
						{
							int boneIndex = a_Stream.ReadInt32();
							m_Skin[s][boneIndex] = weights[i];
						}*/
                    }

                    m_BindPose = new float[a_Stream.ReadInt32()][,];
                    for (int i = 0; i < m_BindPose.Length; i++)
                    {
                        m_BindPose[i] = new[,] {
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() } };
                    }

                    int m_UV1_size = a_Stream.ReadInt32();
                    m_UV1 = new float[m_UV1_size * 2];
                    for (int v = 0; v < m_UV1_size * 2; v++) { m_UV1[v] = a_Stream.ReadSingle(); }

                    int m_UV2_size = a_Stream.ReadInt32();
                    m_UV2 = new float[m_UV2_size * 2];
                    for (int v = 0; v < m_UV2_size * 2; v++) { m_UV2[v] = a_Stream.ReadSingle(); }

                    if (version[0] == 2 && version[1] <= 5)
                    {
                        int m_TangentSpace_size = a_Stream.ReadInt32();
                        m_Normals = new float[m_TangentSpace_size * 3];
                        for (int v = 0; v < m_TangentSpace_size; v++)
                        {
                            m_Normals[v * 3] = a_Stream.ReadSingle();
                            m_Normals[v * 3 + 1] = a_Stream.ReadSingle();
                            m_Normals[v * 3 + 2] = a_Stream.ReadSingle();
                            a_Stream.Position += 16; //Vector3f tangent & float handedness 
                        }
                    }
                    else //2.6.0 and later
                    {
                        int m_Tangents_size = a_Stream.ReadInt32();
                        a_Stream.Position += m_Tangents_size * 16; //Vector4f

                        int m_Normals_size = a_Stream.ReadInt32();
                        m_Normals = new float[m_Normals_size * 3];
                        for (int v = 0; v < m_Normals_size * 3; v++) { m_Normals[v] = a_Stream.ReadSingle(); }
                    }
                }
                #endregion
                #region Vertex Buffer for 3.5.0 and later
                else
                {
                    #region read vertex stream
                    m_Skin = new List<BoneInfluence>[a_Stream.ReadInt32()];
                    //m_Skin = new Dictionary<int, float>[a_Stream.ReadInt32()];
                    for (int s = 0; s < m_Skin.Length; s++)
                    {
                        m_Skin[s] = new List<BoneInfluence>();
                        for (int i = 0; i < 4; i++) { m_Skin[s].Add(new BoneInfluence() { weight = a_Stream.ReadSingle() }); }
                        for (int i = 0; i < 4; i++) { m_Skin[s][i].boneIndex = a_Stream.ReadInt32(); }

                        /*m_Skin[s] = new Dictionary<int, float>();
						float[] weights = new float[4] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() };
						for (int i = 0; i < 4; i++)
						{
							int boneIndex = a_Stream.ReadInt32();
							m_Skin[s][boneIndex] = weights[i];
						}*/
                    }


                    if (version[0] == 3 || (version[0] == 4 && version[1] <= 2))
                    {
                        m_BindPose = new float[a_Stream.ReadInt32()][,];
                        for (int i = 0; i < m_BindPose.Length; i++)
                        {
                            m_BindPose[i] = new[,] {
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                        { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() } };
                        }
                    }

                    BitArray m_CurrentChannels = new BitArray(new int[1] { a_Stream.ReadInt32() });
                    m_VertexCount = a_Stream.ReadInt32();
                    //int singleStreamStride = 0;//used tor unity 5
                    int streamCount = 0;

                    #region streams for 3.5.0 - 3.5.7
                    if (version[0] < 4)
                    {
                        if (m_MeshCompression != 0 && version[2] == 0) //special case not just on platform 9
                        {
                            a_Stream.Position += 12;
                        }
                        else
                        {
                            m_Streams = new StreamInfo[4];
                            for (int s = 0; s < 4; s++)
                            {
                                m_Streams[s] = new StreamInfo();
                                m_Streams[s].channelMask = new BitArray(new int[1] { a_Stream.ReadInt32() });
                                m_Streams[s].offset = a_Stream.ReadInt32();
                                m_Streams[s].stride = a_Stream.ReadInt32();
                                m_Streams[s].align = a_Stream.ReadUInt32();
                            }
                        }
                    }
                    #endregion
                    #region channels and streams for 4.0.0 and later
                    else
                    {
                        m_Channels = new ChannelInfo[a_Stream.ReadInt32()];
                        for (int c = 0; c < m_Channels.Length; c++)
                        {
                            m_Channels[c] = new ChannelInfo();
                            m_Channels[c].stream = a_Stream.ReadByte();
                            m_Channels[c].offset = a_Stream.ReadByte();
                            m_Channels[c].format = a_Stream.ReadByte();
                            m_Channels[c].dimension = a_Stream.ReadByte();

                            //calculate stride for Unity 5
                            //singleStreamStride += m_Channels[c].dimension * (4 / (int)Math.Pow(2, m_Channels[c].format));

                            if (m_Channels[c].stream >= streamCount) { streamCount = m_Channels[c].stream + 1; }
                        }

                        if (version[0] < 5)
                        {
                            m_Streams = new StreamInfo[a_Stream.ReadInt32()];
                            for (int s = 0; s < m_Streams.Length; s++)
                            {
                                m_Streams[s] = new StreamInfo();
                                m_Streams[s].channelMask = new BitArray(new int[1] { a_Stream.ReadInt32() });
                                m_Streams[s].offset = a_Stream.ReadInt32();
                                m_Streams[s].stride = a_Stream.ReadByte();
                                m_Streams[s].dividerOp = a_Stream.ReadByte();
                                m_Streams[s].frequency = a_Stream.ReadUInt16();
                            }
                        }
                    }
                    #endregion

                    //actual Vertex Buffer
                    byte[] m_DataSize = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_DataSize, 0, m_DataSize.Length);

                    if (version[0] >= 5) //create streams
                    {
                        m_Streams = new StreamInfo[streamCount];
                        for (int s = 0; s < streamCount; s++)
                        {
                            m_Streams[s] = new StreamInfo();
                            m_Streams[s].channelMask = new BitArray(new int[1] { 0 });
                            m_Streams[s].offset = 0;
                            m_Streams[s].stride = 0;

                            foreach (var m_Channel in m_Channels)
                            {
                                if (m_Channel.stream == s) { m_Streams[s].stride += m_Channel.dimension * (4 / (int)Math.Pow(2, m_Channel.format)); }
                            }

                            if (s > 0)
                            {
                                m_Streams[s].offset = m_Streams[s - 1].offset + m_Streams[s - 1].stride * m_VertexCount;
                                //sometimes there are 8 bytes between streams
                                //this is NOT an alignment, even if sometimes it may seem so

                                if (streamCount == 2) { m_Streams[s].offset = m_DataSize.Length - m_Streams[s].stride * m_VertexCount; }
                                else
                                {
                                    m_VertexCount = 0;
                                    return;
                                }

                                /*var absoluteOffset = a_Stream.Position + 4 + m_Streams[s].offset;
								if ((absoluteOffset % m_Streams[s].stride) != 0)
								{
									m_Streams[s].offset += m_Streams[s].stride - (int)(absoluteOffset % m_Streams[s].stride);
								}*/
                            }
                        }
                    }
                    #endregion

                    #region compute FvF
                    int componentByteSize = 0;
                    byte[] componentBytes;
                    float[] componentsArray;

                    #region 4.0.0 and later
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
                                    //in the future, try to use only m_CurrentChannels
                                    if ((version[0] < 5 && m_Stream.channelMask.Get(b)) || (version[0] >= 5 && m_CurrentChannels.Get(b)))
                                    {
                                        // in Unity 4.x the colors channel has 1 dimension, as in 1 color with 4 components
                                        if (b == 2 && m_Channel.format == 2) { m_Channel.dimension = 4; }

                                        componentByteSize = 4 / (int)Math.Pow(2, m_Channel.format);

                                        /*switch (m_Channel.format)
										{
											case 0: //32bit
												valueBufferSize = 4;
												break;
											case 1: //16bit
												valueBufferSize = 2;
												break;
											case 2: //8bit
												valueBufferSize = 1;
												m_Channel.dimension = 4;//in older versions this is 1, as in 1 color with 4 components
												break;
										}*/

                                        componentBytes = new byte[componentByteSize];
                                        componentsArray = new float[m_VertexCount * m_Channel.dimension];

                                        for (int v = 0; v < m_VertexCount; v++)
                                        {
                                            int vertexOffset = m_Stream.offset + m_Channel.offset + m_Stream.stride * v;
                                            for (int d = 0; d < m_Channel.dimension; d++)
                                            {
                                                int componentOffset = vertexOffset + componentByteSize * d;
                                                Buffer.BlockCopy(m_DataSize, componentOffset, componentBytes, 0, componentByteSize);
                                                componentsArray[v * m_Channel.dimension + d] = bytesToFloat(componentBytes);
                                            }
                                        }

                                        switch (b)
                                        {
                                            case 0: m_Vertices = componentsArray; break;
                                            case 1: m_Normals = componentsArray; break;
                                            case 2: m_Colors = componentsArray; break;
                                            case 3: m_UV1 = componentsArray; break;
                                            case 4: m_UV2 = componentsArray; break;
                                            case 5:
                                                if (version[0] == 5) { m_UV3 = componentsArray; }
                                                else { m_Tangents = componentsArray; }
                                                break;
                                            case 6: m_UV4 = componentsArray; break;
                                            case 7: m_Tangents = componentsArray; break;
                                        }

                                        m_Stream.channelMask.Set(b, false);
                                        m_CurrentChannels.Set(b, false);
                                        componentBytes = null;
                                        componentsArray = null;
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
                            //Unity 3.5.x only uses floats, and that's probably why channels were introduced in Unity 4

                            ChannelInfo m_Channel = new ChannelInfo();//create my own channel so I can use the same methods
                            m_Channel.offset = 0;

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

                                    componentBytes = new byte[componentByteSize];
                                    componentsArray = new float[m_VertexCount * m_Channel.dimension];

                                    for (int v = 0; v < m_VertexCount; v++)
                                    {
                                        int vertexOffset = m_Stream.offset + m_Channel.offset + m_Stream.stride * v;
                                        for (int d = 0; d < m_Channel.dimension; d++)
                                        {
                                            int m_DataSizeOffset = vertexOffset + componentByteSize * d;
                                            Buffer.BlockCopy(m_DataSize, m_DataSizeOffset, componentBytes, 0, componentByteSize);
                                            componentsArray[v * m_Channel.dimension + d] = bytesToFloat(componentBytes);
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
                                    componentBytes = null;
                                    componentsArray = null;
                                }
                            }
                        }
                    }
                    #endregion
                    #endregion
                }
                #endregion

                #region Compressed Mesh data for 2.6.0 and later - 160 bytes
                if (version[0] >= 3 || (version[0] == 2 && version[1] >= 6))
                {
                    //remember there can be combinations of packed and regular vertex properties

                    #region m_Vertices
                    PackedBitVector m_Vertices_Packed = new PackedBitVector();
                    m_Vertices_Packed.m_NumItems = a_Stream.ReadInt32();
                    m_Vertices_Packed.m_Range = a_Stream.ReadSingle();
                    m_Vertices_Packed.m_Start = a_Stream.ReadSingle();
                    m_Vertices_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_Vertices_Packed.m_Data, 0, m_Vertices_Packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_Vertices_Packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_Vertices_Packed.m_NumItems > 0)
                    {
                        m_VertexCount = m_Vertices_Packed.m_NumItems / 3;
                        uint[] m_Vertices_Unpacked = UnpackBitVector(m_Vertices_Packed);
                        int bitmax = 0;//used to convert int value to float
                        for (int b = 0; b < m_Vertices_Packed.m_BitSize; b++) { bitmax |= (1 << b); }
                        m_Vertices = new float[m_Vertices_Packed.m_NumItems];
                        for (int v = 0; v < m_Vertices_Packed.m_NumItems; v++)
                        {
                            m_Vertices[v] = (float)((double)m_Vertices_Unpacked[v] / bitmax) * m_Vertices_Packed.m_Range + m_Vertices_Packed.m_Start;
                        }
                    }
                    #endregion

                    #region m_UV
                    PackedBitVector m_UV_Packed = new PackedBitVector(); //contains all channels
                    m_UV_Packed.m_NumItems = a_Stream.ReadInt32();
                    m_UV_Packed.m_Range = a_Stream.ReadSingle();
                    m_UV_Packed.m_Start = a_Stream.ReadSingle();
                    m_UV_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_UV_Packed.m_Data, 0, m_UV_Packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_UV_Packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_UV_Packed.m_NumItems > 0 && (bool)Properties.Settings.Default["exportUVs"])
                    {
                        uint[] m_UV_Unpacked = UnpackBitVector(m_UV_Packed);
                        int bitmax = 0;
                        for (int b = 0; b < m_UV_Packed.m_BitSize; b++) { bitmax |= (1 << b); }

                        m_UV1 = new float[m_VertexCount * 2];

                        for (int v = 0; v < m_VertexCount * 2; v++)
                        {
                            m_UV1[v] = (float)((double)m_UV_Unpacked[v] / bitmax) * m_UV_Packed.m_Range + m_UV_Packed.m_Start;
                        }

                        if (m_UV_Packed.m_NumItems >= m_VertexCount * 4)
                        {
                            m_UV2 = new float[m_VertexCount * 2];
                            for (uint v = 0; v < m_VertexCount * 2; v++)
                            {
                                m_UV2[v] = (float)((double)m_UV_Unpacked[v + m_VertexCount * 2] / bitmax) * m_UV_Packed.m_Range + m_UV_Packed.m_Start;
                            }

                            if (m_UV_Packed.m_NumItems >= m_VertexCount * 6)
                            {
                                m_UV3 = new float[m_VertexCount * 2];
                                for (uint v = 0; v < m_VertexCount * 2; v++)
                                {
                                    m_UV3[v] = (float)((double)m_UV_Unpacked[v + m_VertexCount * 4] / bitmax) * m_UV_Packed.m_Range + m_UV_Packed.m_Start;
                                }

                                if (m_UV_Packed.m_NumItems == m_VertexCount * 8)
                                {
                                    m_UV4 = new float[m_VertexCount * 2];
                                    for (uint v = 0; v < m_VertexCount * 2; v++)
                                    {
                                        m_UV4[v] = (float)((double)m_UV_Unpacked[v + m_VertexCount * 6] / bitmax) * m_UV_Packed.m_Range + m_UV_Packed.m_Start;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region m_BindPose
                    if (version[0] < 5)
                    {
                        PackedBitVector m_BindPoses_Packed = new PackedBitVector();
                        m_BindPoses_Packed.m_NumItems = a_Stream.ReadInt32();
                        m_BindPoses_Packed.m_Range = a_Stream.ReadSingle();
                        m_BindPoses_Packed.m_Start = a_Stream.ReadSingle();
                        m_BindPoses_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                        a_Stream.Read(m_BindPoses_Packed.m_Data, 0, m_BindPoses_Packed.m_Data.Length);
                        a_Stream.AlignStream(4);
                        m_BindPoses_Packed.m_BitSize = a_Stream.ReadByte();
                        a_Stream.Position += 3; //4 byte alignment

                        if (m_BindPoses_Packed.m_NumItems > 0 && (bool)Properties.Settings.Default["exportDeformers"])
                        {
                            uint[] m_BindPoses_Unpacked = UnpackBitVector(m_BindPoses_Packed);
                            int bitmax = 0;//used to convert int value to float
                            for (int b = 0; b < m_BindPoses_Packed.m_BitSize; b++) { bitmax |= (1 << b); }

                            m_BindPose = new float[m_BindPoses_Packed.m_NumItems / 16][,];

                            for (int i = 0; i < m_BindPose.Length; i++)
                            {
                                m_BindPose[i] = new float[4, 4];
                                for (int j = 0; j < 4; j++)
                                {
                                    for (int k = 0; k < 4; k++)
                                    {
                                        m_BindPose[i][j, k] = (float)((double)m_BindPoses_Unpacked[i * 16 + j * 4 + k] / bitmax) * m_BindPoses_Packed.m_Range + m_BindPoses_Packed.m_Start;
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    PackedBitVector m_Normals_Packed = new PackedBitVector();
                    m_Normals_Packed.m_NumItems = a_Stream.ReadInt32();
                    m_Normals_Packed.m_Range = a_Stream.ReadSingle();
                    m_Normals_Packed.m_Start = a_Stream.ReadSingle();
                    m_Normals_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_Normals_Packed.m_Data, 0, m_Normals_Packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_Normals_Packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    PackedBitVector m_Tangents_Packed = new PackedBitVector();
                    m_Tangents_Packed.m_NumItems = a_Stream.ReadInt32();
                    m_Tangents_Packed.m_Range = a_Stream.ReadSingle();
                    m_Tangents_Packed.m_Start = a_Stream.ReadSingle();
                    m_Tangents_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_Tangents_Packed.m_Data, 0, m_Tangents_Packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_Tangents_Packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    PackedBitVector m_Weights = new PackedBitVector();
                    m_Weights.m_NumItems = a_Stream.ReadInt32();
                    m_Weights.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_Weights.m_Data, 0, m_Weights.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_Weights.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    #region m_Normals
                    PackedBitVector m_NormalSigns_packed = new PackedBitVector();
                    m_NormalSigns_packed.m_NumItems = a_Stream.ReadInt32();
                    m_NormalSigns_packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_NormalSigns_packed.m_Data, 0, m_NormalSigns_packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_NormalSigns_packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_Normals_Packed.m_NumItems > 0 && (bool)Properties.Settings.Default["exportNormals"])
                    {
                        uint[] m_Normals_Unpacked = UnpackBitVector(m_Normals_Packed);
                        uint[] m_NormalSigns = UnpackBitVector(m_NormalSigns_packed);
                        int bitmax = 0;
                        for (int b = 0; b < m_Normals_Packed.m_BitSize; b++) { bitmax |= (1 << b); }
                        m_Normals = new float[m_Normals_Packed.m_NumItems / 2 * 3];
                        for (int v = 0; v < m_Normals_Packed.m_NumItems / 2; v++)
                        {
                            m_Normals[v * 3] = (float)((double)m_Normals_Unpacked[v * 2] / bitmax) * m_Normals_Packed.m_Range + m_Normals_Packed.m_Start;
                            m_Normals[v * 3 + 1] = (float)((double)m_Normals_Unpacked[v * 2 + 1] / bitmax) * m_Normals_Packed.m_Range + m_Normals_Packed.m_Start;
                            m_Normals[v * 3 + 2] = (float)Math.Sqrt(1 - m_Normals[v * 3] * m_Normals[v * 3] - m_Normals[v * 3 + 1] * m_Normals[v * 3 + 1]);
                            if (m_NormalSigns[v] == 0) { m_Normals[v * 3 + 2] *= -1; }
                        }
                    }
                    #endregion

                    #region m_Tangents
                    PackedBitVector m_TangentSigns_packed = new PackedBitVector();
                    m_TangentSigns_packed.m_NumItems = a_Stream.ReadInt32();
                    m_TangentSigns_packed.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_TangentSigns_packed.m_Data, 0, m_TangentSigns_packed.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_TangentSigns_packed.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_Tangents_Packed.m_NumItems > 0 && (bool)Properties.Settings.Default["exportTangents"])
                    {
                        uint[] m_Tangents_Unpacked = UnpackBitVector(m_Tangents_Packed);
                        uint[] m_TangentSigns = UnpackBitVector(m_TangentSigns_packed);
                        int bitmax = 0;
                        for (int b = 0; b < m_Tangents_Packed.m_BitSize; b++) { bitmax |= (1 << b); }
                        m_Tangents = new float[m_Tangents_Packed.m_NumItems / 2 * 3];
                        for (int v = 0; v < m_Tangents_Packed.m_NumItems / 2; v++)
                        {
                            m_Tangents[v * 3] = (float)((double)m_Tangents_Unpacked[v * 2] / bitmax) * m_Tangents_Packed.m_Range + m_Tangents_Packed.m_Start;
                            m_Tangents[v * 3 + 1] = (float)((double)m_Tangents_Unpacked[v * 2 + 1] / bitmax) * m_Tangents_Packed.m_Range + m_Tangents_Packed.m_Start;
                            m_Tangents[v * 3 + 2] = (float)Math.Sqrt(1 - m_Tangents[v * 3] * m_Tangents[v * 3] - m_Tangents[v * 3 + 1] * m_Tangents[v * 3 + 1]);
                            if (m_TangentSigns[v] == 0) { m_Tangents[v * 3 + 2] *= -1; }
                        }
                    }
                    #endregion

                    #region m_FloatColors
                    if (version[0] >= 5)
                    {
                        PackedBitVector m_FloatColors = new PackedBitVector();
                        m_FloatColors.m_NumItems = a_Stream.ReadInt32();
                        m_FloatColors.m_Range = a_Stream.ReadSingle();
                        m_FloatColors.m_Start = a_Stream.ReadSingle();
                        m_FloatColors.m_Data = new byte[a_Stream.ReadInt32()];
                        a_Stream.Read(m_FloatColors.m_Data, 0, m_FloatColors.m_Data.Length);
                        a_Stream.AlignStream(4);
                        m_FloatColors.m_BitSize = a_Stream.ReadByte();
                        a_Stream.Position += 3; //4 byte alignment

                        if (m_FloatColors.m_NumItems > 0 && (bool)Properties.Settings.Default["exportColors"])
                        {
                            uint[] m_FloatColors_Unpacked = UnpackBitVector(m_FloatColors);
                            int bitmax = 0;
                            for (int b = 0; b < m_FloatColors.m_BitSize; b++) { bitmax |= (1 << b); }

                            m_Colors = new float[m_FloatColors.m_NumItems];

                            for (int v = 0; v < m_FloatColors.m_NumItems; v++)
                            {
                                m_Colors[v] = (float)m_FloatColors_Unpacked[v] / bitmax * m_FloatColors.m_Range + m_FloatColors.m_Start;
                            }
                        }
                    }
                    #endregion

                    #region m_Skin
                    PackedBitVector m_BoneIndices = new PackedBitVector();
                    m_BoneIndices.m_NumItems = a_Stream.ReadInt32();
                    m_BoneIndices.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_BoneIndices.m_Data, 0, m_BoneIndices.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_BoneIndices.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_BoneIndices.m_NumItems > 0 && (bool)Properties.Settings.Default["exportDeformers"])
                    {
                        uint[] m_Weights_Unpacked = UnpackBitVector(m_Weights);
                        int bitmax = 0;
                        for (int b = 0; b < m_Weights.m_BitSize; b++) { bitmax |= (1 << b); }

                        uint[] m_BoneIndices_Unpacked = UnpackBitVector(m_BoneIndices);

                        m_Skin = new List<BoneInfluence>[m_VertexCount];
                        for (int s = 0; s < m_Skin.Length; s++)
                        {
                            m_Skin[s] = new List<BoneInfluence>(4);
                        }

                        int inflCount = m_Weights.m_NumItems;
                        int vertIndex = 0;
                        int weightIndex = 0;
                        int bonesIndex = 0;
                        for (weightIndex = 0; weightIndex < inflCount; vertIndex++)
                        {
                            int inflSum = 0;
                            int j;
                            for (j = 0; j < 4; j++)
                            {
                                int curWeight = 0;
                                if (j == 3)
                                {
                                    curWeight = 31 - inflSum;
                                }
                                else
                                {
                                    curWeight = (int)m_Weights_Unpacked[weightIndex];
                                    weightIndex++;
                                    inflSum += curWeight;
                                }
                                double curWeightDouble = (double)curWeight;
                                float realCurWeight = (float)(curWeightDouble / bitmax);

                                int boneIndex = (int)m_BoneIndices_Unpacked[bonesIndex];
                                bonesIndex++;
                                if (boneIndex < 0)
                                {
                                    throw new Exception($"Invalid bone index {boneIndex}");
                                }
                                BoneInfluence boneInfl = new BoneInfluence()
                                {
                                    weight = realCurWeight,
                                    boneIndex = boneIndex,
                                };
                                m_Skin[vertIndex].Add(boneInfl);

                                if (inflSum == 31)
                                {
                                    break;
                                }
                                if (inflSum > 31)
                                {
                                    throw new Exception("Influence sum " + inflSum + " greater than 31");
                                }
                            }
                            for (; j < 4; j++)
                            {
                                BoneInfluence boneInfl = new BoneInfluence()
                                {
                                    weight = 0.0f,
                                    boneIndex = 0,
                                };
                                m_Skin[vertIndex].Add(boneInfl);
                            }
                        }

                        bool isFine = vertIndex == m_VertexCount;
                        if (!isFine)
                        {
                            throw new Exception("Vertecies aint equals");
                        }
                    }
                    #endregion

                    PackedBitVector m_Triangles = new PackedBitVector();
                    m_Triangles.m_NumItems = a_Stream.ReadInt32();
                    m_Triangles.m_Data = new byte[a_Stream.ReadInt32()];
                    a_Stream.Read(m_Triangles.m_Data, 0, m_Triangles.m_Data.Length);
                    a_Stream.AlignStream(4);
                    m_Triangles.m_BitSize = a_Stream.ReadByte();
                    a_Stream.Position += 3; //4 byte alignment

                    if (m_Triangles.m_NumItems > 0) { m_IndexBuffer = UnpackBitVector(m_Triangles); }
                }
                #endregion

                #region Colors & Collision triangles for 3.4.2 and earlier
                if (version[0] <= 2 || (version[0] == 3 && version[1] <= 4)) //
                {
                    a_Stream.Position += 24; //Axis-Aligned Bounding Box
                    int m_Colors_size = a_Stream.ReadInt32();
                    m_Colors = new float[m_Colors_size * 4];
                    for (int v = 0; v < m_Colors_size * 4; v++) { m_Colors[v] = (float)(a_Stream.ReadByte()) / 0xFF; }

                    int m_CollisionTriangles_size = a_Stream.ReadInt32();
                    a_Stream.Position += m_CollisionTriangles_size * 4; //UInt32 indices
                    int m_CollisionVertexCount = a_Stream.ReadInt32();
                }
                #endregion
                #region Compressed colors & Local AABB for 3.5.0 to 4.x.x
                else //vertex colors are either in streams or packed bits
                {
                    if (version[0] < 5)
                    {
                        PackedBitVector m_Colors_Packed = new PackedBitVector();
                        m_Colors_Packed.m_NumItems = a_Stream.ReadInt32();
                        m_Colors_Packed.m_Data = new byte[a_Stream.ReadInt32()];
                        a_Stream.Read(m_Colors_Packed.m_Data, 0, m_Colors_Packed.m_Data.Length);
                        a_Stream.AlignStream(4);
                        m_Colors_Packed.m_BitSize = a_Stream.ReadByte();
                        a_Stream.Position += 3; //4 byte alignment

                        if (m_Colors_Packed.m_NumItems > 0)
                        {
                            if (m_Colors_Packed.m_BitSize == 32)
                            {
                                //4 x 8bit color channels
                                m_Colors = new float[m_Colors_Packed.m_Data.Length];
                                for (int v = 0; v < m_Colors_Packed.m_Data.Length; v++)
                                {
                                    m_Colors[v] = (float)m_Colors_Packed.m_Data[v] / 0xFF;
                                }
                            }
                            else //not tested
                            {
                                uint[] m_Colors_Unpacked = UnpackBitVector(m_Colors_Packed);
                                int bitmax = 0;//used to convert int value to float
                                for (int b = 0; b < m_Colors_Packed.m_BitSize; b++) { bitmax |= (1 << b); }
                                m_Colors = new float[m_Colors_Packed.m_NumItems];
                                for (int v = 0; v < m_Colors_Packed.m_NumItems; v++)
                                {
                                    m_Colors[v] = (float)m_Colors_Unpacked[v] / bitmax;
                                }
                            }
                        }
                    }
                    else { uint m_UVInfo = a_Stream.ReadUInt32(); }

                    a_Stream.Position += 24; //Axis-Aligned Bounding Box
                }
                #endregion

                int m_MeshUsageFlags = a_Stream.ReadInt32();

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
                            }
                        }
                    }
                }
                #endregion
            }
            else
            {
                MeshPD.extension = ".obj";
                MeshPD.Text = m_Name;
            }
        }
    }
}
