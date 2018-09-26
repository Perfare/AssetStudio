using SharpDX;
using System.Collections.Generic;

namespace AssetStudio
{
    public class Node
    {
        public int m_ParentId { get; set; }
        public int m_AxesId { get; set; }

        public Node(EndianBinaryReader reader)
        {
            m_ParentId = reader.ReadInt32();
            m_AxesId = reader.ReadInt32();
        }
    }

    public class Limit
    {
        public object m_Min { get; set; }
        public object m_Max { get; set; }

        public Limit(EndianBinaryReader reader, int[] version)
        {
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
            {
                m_Min = reader.ReadVector3();
                m_Max = reader.ReadVector3();
            }
            else
            {
                m_Min = reader.ReadVector4();
                m_Max = reader.ReadVector4();
            }
        }
    }

    public class Axes
    {
        public Vector4 m_PreQ { get; set; }
        public Vector4 m_PostQ { get; set; }
        public object m_Sgn { get; set; }
        public Limit m_Limit { get; set; }
        public float m_Length { get; set; }
        public uint m_Type { get; set; }

        public Axes(EndianBinaryReader reader, int[] version)
        {
            m_PreQ = reader.ReadVector4();
            m_PostQ = reader.ReadVector4();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
            {
                m_Sgn = reader.ReadVector3();
            }
            else
            {
                m_Sgn = reader.ReadVector4();
            }
            m_Limit = new Limit(reader, version);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }

    public class Skeleton
    {
        public List<Node> m_Node { get; set; }
        public List<uint> m_ID { get; set; }
        public List<Axes> m_AxesArray { get; set; }


        public Skeleton(EndianBinaryReader reader, int[] version)
        {
            int numNodes = reader.ReadInt32();
            m_Node = new List<Node>(numNodes);
            for (int i = 0; i < numNodes; i++)
            {
                m_Node.Add(new Node(reader));
            }

            int numIDs = reader.ReadInt32();
            m_ID = new List<uint>(numIDs);
            for (int i = 0; i < numIDs; i++)
            {
                m_ID.Add(reader.ReadUInt32());
            }

            int numAxes = reader.ReadInt32();
            m_AxesArray = new List<Axes>(numAxes);
            for (int i = 0; i < numAxes; i++)
            {
                m_AxesArray.Add(new Axes(reader, version));
            }
        }
    }

    public class SkeletonPose
    {
        public List<xform> m_X { get; set; }

        public SkeletonPose()
        {
            m_X = new List<xform>();
        }

        public SkeletonPose(EndianBinaryReader reader, int[] version)
        {
            int numXforms = reader.ReadInt32();
            m_X = new List<xform>(numXforms);
            for (int i = 0; i < numXforms; i++)
            {
                m_X.Add(new xform(reader, version));
            }
        }
    }

    public class Hand
    {
        public List<int> m_HandBoneIndex { get; set; }

        public Hand(EndianBinaryReader reader)
        {
            int numIndexes = reader.ReadInt32();
            m_HandBoneIndex = new List<int>(numIndexes);
            for (int i = 0; i < numIndexes; i++)
            {
                m_HandBoneIndex.Add(reader.ReadInt32());
            }
        }
    }

    public class Handle
    {
        public xform m_X { get; set; }
        public uint m_ParentHumanIndex { get; set; }
        public uint m_ID { get; set; }

        public Handle(EndianBinaryReader reader, int[] version)
        {
            m_X = new xform(reader, version);
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }

    public class Collider
    {
        public xform m_X { get; set; }
        public uint m_Type { get; set; }
        public uint m_XMotionType { get; set; }
        public uint m_YMotionType { get; set; }
        public uint m_ZMotionType { get; set; }
        public float m_MinLimitX { get; set; }
        public float m_MaxLimitX { get; set; }
        public float m_MaxLimitY { get; set; }
        public float m_MaxLimitZ { get; set; }

        public Collider(EndianBinaryReader reader, int[] version)
        {
            m_X = new xform(reader, version);
            m_Type = reader.ReadUInt32();
            m_XMotionType = reader.ReadUInt32();
            m_YMotionType = reader.ReadUInt32();
            m_ZMotionType = reader.ReadUInt32();
            m_MinLimitX = reader.ReadSingle();
            m_MaxLimitX = reader.ReadSingle();
            m_MaxLimitY = reader.ReadSingle();
            m_MaxLimitZ = reader.ReadSingle();
        }
    }

    public class Human
    {
        public xform m_RootX { get; set; }
        public Skeleton m_Skeleton { get; set; }
        public SkeletonPose m_SkeletonPose { get; set; }
        public Hand m_LeftHand { get; set; }
        public Hand m_RightHand { get; set; }
        public List<Handle> m_Handles { get; set; }
        public List<Collider> m_ColliderArray { get; set; }
        public List<int> m_HumanBoneIndex { get; set; }
        public List<float> m_HumanBoneMass { get; set; }
        public List<int> m_ColliderIndex { get; set; }
        public float m_Scale { get; set; }
        public float m_ArmTwist { get; set; }
        public float m_ForeArmTwist { get; set; }
        public float m_UpperLegTwist { get; set; }
        public float m_LegTwist { get; set; }
        public float m_ArmStretch { get; set; }
        public float m_LegStretch { get; set; }
        public float m_FeetSpacing { get; set; }
        public bool m_HasLeftHand { get; set; }
        public bool m_HasRightHand { get; set; }
        public bool m_HasTDoF { get; set; }

        public Human(EndianBinaryReader reader, int[] version)
        {
            m_RootX = new xform(reader, version);
            m_Skeleton = new Skeleton(reader, version);
            m_SkeletonPose = new SkeletonPose(reader, version);
            m_LeftHand = new Hand(reader);
            m_RightHand = new Hand(reader);

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                int numHandles = reader.ReadInt32();
                m_Handles = new List<Handle>(numHandles);
                for (int i = 0; i < numHandles; i++)
                {
                    m_Handles.Add(new Handle(reader, version));
                }

                int numColliders = reader.ReadInt32();
                m_ColliderArray = new List<Collider>(numColliders);
                for (int i = 0; i < numColliders; i++)
                {
                    m_ColliderArray.Add(new Collider(reader, version));
                }
            }

            int numIndexes = reader.ReadInt32();
            m_HumanBoneIndex = new List<int>(numIndexes);
            for (int i = 0; i < numIndexes; i++)
            {
                m_HumanBoneIndex.Add(reader.ReadInt32());
            }

            int numMasses = reader.ReadInt32();
            m_HumanBoneMass = new List<float>(numMasses);
            for (int i = 0; i < numMasses; i++)
            {
                m_HumanBoneMass.Add(reader.ReadSingle());
            }

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                int numColliderIndexes = reader.ReadInt32();
                m_ColliderIndex = new List<int>(numColliderIndexes);
                for (int i = 0; i < numColliderIndexes; i++)
                {
                    m_ColliderIndex.Add(reader.ReadInt32());
                }
            }

            m_Scale = reader.ReadSingle();
            m_ArmTwist = reader.ReadSingle();
            m_ForeArmTwist = reader.ReadSingle();
            m_UpperLegTwist = reader.ReadSingle();
            m_LegTwist = reader.ReadSingle();
            m_ArmStretch = reader.ReadSingle();
            m_LegStretch = reader.ReadSingle();
            m_FeetSpacing = reader.ReadSingle();
            m_HasLeftHand = reader.ReadBoolean();
            m_HasRightHand = reader.ReadBoolean();
            m_HasTDoF = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class AvatarConstant
    {
        public Skeleton m_AvatarSkeleton { get; set; }
        public SkeletonPose m_AvatarSkeletonPose { get; set; }
        public SkeletonPose m_DefaultPose { get; set; }
        public List<uint> m_SkeletonNameIDArray { get; set; }
        public Human m_Human { get; set; }
        public List<int> m_HumanSkeletonIndexArray { get; set; }
        public List<int> m_HumanSkeletonReverseIndexArray { get; set; }
        public int m_RootMotionBoneIndex { get; set; }
        public xform m_RootMotionBoneX { get; set; }
        public Skeleton m_RootMotionSkeleton { get; set; }
        public SkeletonPose m_RootMotionSkeletonPose { get; set; }
        public List<int> m_RootMotionSkeletonIndexArray { get; set; }

        public AvatarConstant(EndianBinaryReader reader, int[] version)
        {
            m_AvatarSkeleton = new Skeleton(reader, version);
            m_AvatarSkeletonPose = new SkeletonPose(reader, version);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_DefaultPose = new SkeletonPose(reader, version);
                int numIDs = reader.ReadInt32();
                m_SkeletonNameIDArray = new List<uint>(numIDs);
                for (int i = 0; i < numIDs; i++)
                {
                    m_SkeletonNameIDArray.Add(reader.ReadUInt32());
                }
            }

            m_Human = new Human(reader, version);

            int numIndexes = reader.ReadInt32();
            m_HumanSkeletonIndexArray = new List<int>(numIndexes);
            for (int i = 0; i < numIndexes; i++)
            {
                m_HumanSkeletonIndexArray.Add(reader.ReadInt32());
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numReverseIndexes = reader.ReadInt32();
                m_HumanSkeletonReverseIndexArray = new List<int>(numReverseIndexes);
                for (int i = 0; i < numReverseIndexes; i++)
                {
                    m_HumanSkeletonReverseIndexArray.Add(reader.ReadInt32());
                }
            }

            m_RootMotionBoneIndex = reader.ReadInt32();
            m_RootMotionBoneX = new xform(reader, version);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_RootMotionSkeleton = new Skeleton(reader, version);
                m_RootMotionSkeletonPose = new SkeletonPose(reader, version);

                int numMotionIndexes = reader.ReadInt32();
                m_RootMotionSkeletonIndexArray = new List<int>(numMotionIndexes);
                for (int i = 0; i < numMotionIndexes; i++)
                {
                    m_RootMotionSkeletonIndexArray.Add(reader.ReadInt32());
                }
            }
        }
    }

    public sealed class Avatar : NamedObject
    {
        public uint m_AvatarSize { get; set; }
        public AvatarConstant m_Avatar { get; set; }
        public List<KeyValuePair<uint, string>> m_TOS { get; set; }

        public Avatar(AssetPreloadData preloadData) : base(preloadData)
        {
            m_AvatarSize = reader.ReadUInt32();
            m_Avatar = new AvatarConstant(reader, version);

            int numTOS = reader.ReadInt32();
            m_TOS = new List<KeyValuePair<uint, string>>(numTOS);
            for (int i = 0; i < numTOS; i++)
            {
                m_TOS.Add(new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString()));
            }
        }

        public string FindBoneName(uint hash)
        {
            foreach (var pair in m_TOS)
            {
                if (pair.Key == hash)
                {
                    return pair.Value.Substring(pair.Value.LastIndexOf('/') + 1);
                }
            }
            return null;
        }

        public string FindBonePath(uint hash)
        {
            return m_TOS.Find(pair => pair.Key == hash).Value;
        }
    }
}
