using SharpDX;
using System.Collections.Generic;

namespace AssetStudio
{
    public class Node
    {
        public int m_ParentId;
        public int m_AxesId;

        public Node(ObjectReader reader)
        {
            m_ParentId = reader.ReadInt32();
            m_AxesId = reader.ReadInt32();
        }
    }

    public class Limit
    {
        public object m_Min;
        public object m_Max;

        public Limit(ObjectReader reader)
        {
            var version = reader.version;
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
        public Vector4 m_PreQ;
        public Vector4 m_PostQ;
        public object m_Sgn;
        public Limit m_Limit;
        public float m_Length;
        public uint m_Type;

        public Axes(ObjectReader reader)
        {
            var version = reader.version;
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
            m_Limit = new Limit(reader);
            m_Length = reader.ReadSingle();
            m_Type = reader.ReadUInt32();
        }
    }

    public class Skeleton
    {
        public List<Node> m_Node;
        public List<uint> m_ID;
        public List<Axes> m_AxesArray;


        public Skeleton(ObjectReader reader)
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
                m_AxesArray.Add(new Axes(reader));
            }
        }
    }

    public class SkeletonPose
    {
        public List<xform> m_X;

        public SkeletonPose()
        {
            m_X = new List<xform>();
        }

        public SkeletonPose(ObjectReader reader)
        {
            int numXforms = reader.ReadInt32();
            m_X = new List<xform>(numXforms);
            for (int i = 0; i < numXforms; i++)
            {
                m_X.Add(new xform(reader));
            }
        }
    }

    public class Hand
    {
        public List<int> m_HandBoneIndex;

        public Hand(ObjectReader reader)
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
        public xform m_X;
        public uint m_ParentHumanIndex;
        public uint m_ID;

        public Handle(ObjectReader reader)
        {
            m_X = new xform(reader);
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }

    public class Collider
    {
        public xform m_X;
        public uint m_Type;
        public uint m_XMotionType;
        public uint m_YMotionType;
        public uint m_ZMotionType;
        public float m_MinLimitX;
        public float m_MaxLimitX;
        public float m_MaxLimitY;
        public float m_MaxLimitZ;

        public Collider(ObjectReader reader)
        {
            m_X = new xform(reader);
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
        public xform m_RootX;
        public Skeleton m_Skeleton;
        public SkeletonPose m_SkeletonPose;
        public Hand m_LeftHand;
        public Hand m_RightHand;
        public List<Handle> m_Handles;
        public List<Collider> m_ColliderArray;
        public List<int> m_HumanBoneIndex;
        public List<float> m_HumanBoneMass;
        public List<int> m_ColliderIndex;
        public float m_Scale;
        public float m_ArmTwist;
        public float m_ForeArmTwist;
        public float m_UpperLegTwist;
        public float m_LegTwist;
        public float m_ArmStretch;
        public float m_LegStretch;
        public float m_FeetSpacing;
        public bool m_HasLeftHand;
        public bool m_HasRightHand;
        public bool m_HasTDoF;

        public Human(ObjectReader reader)
        {
            var version = reader.version;
            m_RootX = new xform(reader);
            m_Skeleton = new Skeleton(reader);
            m_SkeletonPose = new SkeletonPose(reader);
            m_LeftHand = new Hand(reader);
            m_RightHand = new Hand(reader);

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                int numHandles = reader.ReadInt32();
                m_Handles = new List<Handle>(numHandles);
                for (int i = 0; i < numHandles; i++)
                {
                    m_Handles.Add(new Handle(reader));
                }

                int numColliders = reader.ReadInt32();
                m_ColliderArray = new List<Collider>(numColliders);
                for (int i = 0; i < numColliders; i++)
                {
                    m_ColliderArray.Add(new Collider(reader));
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
        public Skeleton m_AvatarSkeleton;
        public SkeletonPose m_AvatarSkeletonPose;
        public SkeletonPose m_DefaultPose;
        public List<uint> m_SkeletonNameIDArray;
        public Human m_Human;
        public List<int> m_HumanSkeletonIndexArray;
        public List<int> m_HumanSkeletonReverseIndexArray;
        public int m_RootMotionBoneIndex;
        public xform m_RootMotionBoneX;
        public Skeleton m_RootMotionSkeleton;
        public SkeletonPose m_RootMotionSkeletonPose;
        public List<int> m_RootMotionSkeletonIndexArray;

        public AvatarConstant(ObjectReader reader)
        {
            var version = reader.version;
            m_AvatarSkeleton = new Skeleton(reader);
            m_AvatarSkeletonPose = new SkeletonPose(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_DefaultPose = new SkeletonPose(reader);
                int numIDs = reader.ReadInt32();
                m_SkeletonNameIDArray = new List<uint>(numIDs);
                for (int i = 0; i < numIDs; i++)
                {
                    m_SkeletonNameIDArray.Add(reader.ReadUInt32());
                }
            }

            m_Human = new Human(reader);

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
            m_RootMotionBoneX = new xform(reader);

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_RootMotionSkeleton = new Skeleton(reader);
                m_RootMotionSkeletonPose = new SkeletonPose(reader);

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
        public uint m_AvatarSize;
        public AvatarConstant m_Avatar;
        public List<KeyValuePair<uint, string>> m_TOS;

        public Avatar(ObjectReader reader) : base(reader)
        {
            m_AvatarSize = reader.ReadUInt32();
            m_Avatar = new AvatarConstant(reader);

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
