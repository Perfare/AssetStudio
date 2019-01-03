using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class HumanPoseMask
    {
        public uint word0;
        public uint word1;
        public uint word2;

        public HumanPoseMask(ObjectReader reader)
        {
            var version = reader.version;

            word0 = reader.ReadUInt32();
            word1 = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 2)) //5.2 and up
            {
                word2 = reader.ReadUInt32();
            }
        }
    }

    public class SkeletonMaskElement
    {
        public uint m_PathHash;
        public float m_Weight;

        public SkeletonMaskElement(ObjectReader reader)
        {
            m_PathHash = reader.ReadUInt32();
            m_Weight = reader.ReadSingle();
        }
    }

    public class SkeletonMask
    {
        public SkeletonMaskElement[] m_Data;

        public SkeletonMask(ObjectReader reader)
        {
            int numElements = reader.ReadInt32();
            m_Data = new SkeletonMaskElement[numElements];
            for (int i = 0; i < numElements; i++)
            {
                m_Data[i] = new SkeletonMaskElement(reader);
            }
        }
    }

    public class LayerConstant
    {
        public uint m_StateMachineIndex;
        public uint m_StateMachineMotionSetIndex;
        public HumanPoseMask m_BodyMask;
        public SkeletonMask m_SkeletonMask;
        public uint m_Binding;
        public int m_LayerBlendingMode;
        public float m_DefaultWeight;
        public bool m_IKPass;
        public bool m_SyncedLayerAffectsTiming;

        public LayerConstant(ObjectReader reader)
        {
            var version = reader.version;

            m_StateMachineIndex = reader.ReadUInt32();
            m_StateMachineMotionSetIndex = reader.ReadUInt32();
            m_BodyMask = new HumanPoseMask(reader);
            m_SkeletonMask = new SkeletonMask(reader);
            m_Binding = reader.ReadUInt32();
            m_LayerBlendingMode = reader.ReadInt32();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 2)) //4.2 and up
            {
                m_DefaultWeight = reader.ReadSingle();
            }
            m_IKPass = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 2)) //4.2 and up
            {
                m_SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
        }
    }

    public class ConditionConstant
    {
        public uint m_ConditionMode;
        public uint m_EventID;
        public float m_EventThreshold;
        public float m_ExitTime;

        public ConditionConstant(ObjectReader reader)
        {
            m_ConditionMode = reader.ReadUInt32();
            m_EventID = reader.ReadUInt32();
            m_EventThreshold = reader.ReadSingle();
            m_ExitTime = reader.ReadSingle();
        }
    }

    public class TransitionConstant
    {
        public ConditionConstant[] m_ConditionConstantArray;
        public uint m_DestinationState;
        public uint m_FullPathID;
        public uint m_ID;
        public uint m_UserID;
        public float m_TransitionDuration;
        public float m_TransitionOffset;
        public float m_ExitTime;
        public bool m_HasExitTime;
        public bool m_HasFixedDuration;
        public int m_InterruptionSource;
        public bool m_OrderedInterruption;
        public bool m_Atomic;
        public bool m_CanTransitionToSelf;

        public TransitionConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = new ConditionConstant[numConditions];
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray[i] = new ConditionConstant(reader);
            }

            m_DestinationState = reader.ReadUInt32();
            if (version[0] >= 5) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_ID = reader.ReadUInt32();
            m_UserID = reader.ReadUInt32();
            m_TransitionDuration = reader.ReadSingle();
            m_TransitionOffset = reader.ReadSingle();
            if (version[0] >= 5) //5.0 and up
            {
                m_ExitTime = reader.ReadSingle();
                m_HasExitTime = reader.ReadBoolean();
                m_HasFixedDuration = reader.ReadBoolean();
                reader.AlignStream();
                m_InterruptionSource = reader.ReadInt32();
                m_OrderedInterruption = reader.ReadBoolean();
            }
            else
            {
                m_Atomic = reader.ReadBoolean();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                m_CanTransitionToSelf = reader.ReadBoolean();
            }

            reader.AlignStream();
        }
    }

    public class LeafInfoConstant
    {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(ObjectReader reader)
        {
            m_IDArray = reader.ReadUInt32Array();
            m_IndexOffset = reader.ReadUInt32();
        }
    }

    public class MotionNeighborList
    {
        public uint[] m_NeighborArray;

        public MotionNeighborList(ObjectReader reader)
        {
            m_NeighborArray = reader.ReadUInt32Array();
        }
    }

    public class Blend2dDataConstant
    {
        public Vector2[] m_ChildPositionArray;
        public float[] m_ChildMagnitudeArray;
        public Vector2[] m_ChildPairVectorArray;
        public float[] m_ChildPairAvgMagInvArray;
        public MotionNeighborList[] m_ChildNeighborListArray;

        public Blend2dDataConstant(ObjectReader reader)
        {
            m_ChildPositionArray = reader.ReadVector2Array();
            m_ChildMagnitudeArray = reader.ReadSingleArray();
            m_ChildPairVectorArray = reader.ReadVector2Array();
            m_ChildPairAvgMagInvArray = reader.ReadSingleArray();

            int numNeighbours = reader.ReadInt32();
            m_ChildNeighborListArray = new MotionNeighborList[numNeighbours];
            for (int i = 0; i < numNeighbours; i++)
            {
                m_ChildNeighborListArray[i] = new MotionNeighborList(reader);
            }
        }
    }

    public class Blend1dDataConstant // wrong labeled
    {
        public float[] m_ChildThresholdArray;

        public Blend1dDataConstant(ObjectReader reader)
        {
            m_ChildThresholdArray = reader.ReadSingleArray();
        }
    }

    public class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray;
        public bool m_NormalizedBlendValues;

        public BlendDirectDataConstant(ObjectReader reader)
        {
            m_ChildBlendEventIDArray = reader.ReadUInt32Array();
            m_NormalizedBlendValues = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public class BlendTreeNodeConstant
    {
        public uint m_BlendType;
        public uint m_BlendEventID;
        public uint m_BlendEventYID;
        public uint[] m_ChildIndices;
        public float[] m_ChildThresholdArray;
        public Blend1dDataConstant m_Blend1dData;
        public Blend2dDataConstant m_Blend2dData;
        public BlendDirectDataConstant m_BlendDirectData;
        public uint m_ClipID;
        public uint m_ClipIndex;
        public float m_Duration;
        public float m_CycleOffset;
        public bool m_Mirror;

        public BlendTreeNodeConstant(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_BlendType = reader.ReadUInt32();
            }
            m_BlendEventID = reader.ReadUInt32();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_BlendEventYID = reader.ReadUInt32();
            }
            m_ChildIndices = reader.ReadUInt32Array();
            if (version[0] < 4 || (version[0] == 4 && version[1] < 1)) //4.1 down
            {
                m_ChildThresholdArray = reader.ReadSingleArray();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_Blend1dData = new Blend1dDataConstant(reader);
                m_Blend2dData = new Blend2dDataConstant(reader);
            }

            if (version[0] >= 5) //5.0 and up
            {
                m_BlendDirectData = new BlendDirectDataConstant(reader);
            }

            m_ClipID = reader.ReadUInt32();
            if (version[0] == 4 && version[1] >= 5) //4.5 - 5.0
            {
                m_ClipIndex = reader.ReadUInt32();
            }

            m_Duration = reader.ReadSingle();

            if (version[0] > 4
                || (version[0] == 4 && version[1] > 1)
                || (version[0] == 4 && version[1] == 1 && version[2] >= 3)) //4.1.3 and up
            {
                m_CycleOffset = reader.ReadSingle();
                m_Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }

    public class BlendTreeConstant
    {
        public BlendTreeNodeConstant[] m_NodeArray;
        public ValueArrayConstant m_BlendEventArrayConstant;

        public BlendTreeConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numNodes = reader.ReadInt32();
            m_NodeArray = new BlendTreeNodeConstant[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                m_NodeArray[i] = new BlendTreeNodeConstant(reader);
            }

            if (version[0] < 4 || (version[0] == 4 && version[1] < 5)) //4.5 down
            {
                m_BlendEventArrayConstant = new ValueArrayConstant(reader);
            }
        }
    }


    public class StateConstant
    {
        public TransitionConstant[] m_TransitionConstantArray;
        public int[] m_BlendTreeConstantIndexArray;
        public LeafInfoConstant[] m_LeafInfoArray;
        public BlendTreeConstant[] m_BlendTreeConstantArray;
        public uint m_NameID;
        public uint m_PathID;
        public uint m_FullPathID;
        public uint m_TagID;
        public uint m_SpeedParamID;
        public uint m_MirrorParamID;
        public uint m_CycleOffsetParamID;
        public float m_Speed;
        public float m_CycleOffset;
        public bool m_IKOnFeet;
        public bool m_WriteDefaultValues;
        public bool m_Loop;
        public bool m_Mirror;

        public StateConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numTransistions = reader.ReadInt32();
            m_TransitionConstantArray = new TransitionConstant[numTransistions];
            for (int i = 0; i < numTransistions; i++)
            {
                m_TransitionConstantArray[i] = new TransitionConstant(reader);
            }

            m_BlendTreeConstantIndexArray = reader.ReadInt32Array();

            if (version[0] < 5 || (version[0] == 5 && version[1] < 2)) //5.2 down
            {
                int numInfos = reader.ReadInt32();
                m_LeafInfoArray = new LeafInfoConstant[numInfos];
                for (int i = 0; i < numInfos; i++)
                {
                    m_LeafInfoArray[i] = new LeafInfoConstant(reader);
                }
            }

            int numBlends = reader.ReadInt32();
            m_BlendTreeConstantArray = new BlendTreeConstant[numBlends];
            for (int i = 0; i < numBlends; i++)
            {
                m_BlendTreeConstantArray[i] = new BlendTreeConstant(reader);
            }

            m_NameID = reader.ReadUInt32();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_PathID = reader.ReadUInt32();
            }
            if (version[0] >= 5) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_TagID = reader.ReadUInt32();
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 1)) //5.1 and up
            {
                m_SpeedParamID = reader.ReadUInt32();
                m_MirrorParamID = reader.ReadUInt32();
                m_CycleOffsetParamID = reader.ReadUInt32();
            }

            if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
            {
                var m_TimeParamID = reader.ReadUInt32();
            }

            m_Speed = reader.ReadSingle();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_CycleOffset = reader.ReadSingle();
            }
            m_IKOnFeet = reader.ReadBoolean();
            if (version[0] >= 5) //5.0 and up
            {
                m_WriteDefaultValues = reader.ReadBoolean();
            }

            m_Loop = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
            {
                m_Mirror = reader.ReadBoolean();
            }

            reader.AlignStream();
        }
    }

    public class SelectorTransitionConstant
    {
        public uint m_Destination;
        public ConditionConstant[] m_ConditionConstantArray;

        public SelectorTransitionConstant(ObjectReader reader)
        {
            m_Destination = reader.ReadUInt32();

            int numConditions = reader.ReadInt32();
            m_ConditionConstantArray = new ConditionConstant[numConditions];
            for (int i = 0; i < numConditions; i++)
            {
                m_ConditionConstantArray[i] = new ConditionConstant(reader);
            }
        }
    }

    public class SelectorStateConstant
    {
        public SelectorTransitionConstant[] m_TransitionConstantArray;
        public uint m_FullPathID;
        public bool m_isEntry;

        public SelectorStateConstant(ObjectReader reader)
        {
            int numTransitions = reader.ReadInt32();
            m_TransitionConstantArray = new SelectorTransitionConstant[numTransitions];
            for (int i = 0; i < numTransitions; i++)
            {
                m_TransitionConstantArray[i] = new SelectorTransitionConstant(reader);
            }

            m_FullPathID = reader.ReadUInt32();
            m_isEntry = reader.ReadBoolean();
            reader.AlignStream();
        }
    }

    public class StateMachineConstant
    {
        public StateConstant[] m_StateConstantArray;
        public TransitionConstant[] m_AnyStateTransitionConstantArray;
        public SelectorStateConstant[] m_SelectorStateConstantArray;
        public uint m_DefaultState;
        public uint m_MotionSetCount;

        public StateMachineConstant(ObjectReader reader)
        {
            var version = reader.version;

            int numStates = reader.ReadInt32();
            m_StateConstantArray = new StateConstant[numStates];
            for (int i = 0; i < numStates; i++)
            {
                m_StateConstantArray[i] = new StateConstant(reader);
            }

            int numAnyStates = reader.ReadInt32();
            m_AnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
            for (int i = 0; i < numAnyStates; i++)
            {
                m_AnyStateTransitionConstantArray[i] = new TransitionConstant(reader);
            }

            if (version[0] >= 5) //5.0 and up
            {
                int numSelectors = reader.ReadInt32();
                m_SelectorStateConstantArray = new SelectorStateConstant[numSelectors];
                for (int i = 0; i < numSelectors; i++)
                {
                    m_SelectorStateConstantArray[i] = new SelectorStateConstant(reader);
                }
            }

            m_DefaultState = reader.ReadUInt32();
            m_MotionSetCount = reader.ReadUInt32();
        }
    }

    public class ValueArray
    {
        public bool[] m_BoolValues;
        public int[] m_IntValues;
        public float[] m_FloatValues;
        public Vector4[] m_VectorValues;
        public Vector3[] m_PositionValues;
        public Vector4[] m_QuaternionValues;
        public Vector3[] m_ScaleValues;

        public ValueArray(ObjectReader reader)
        {
            var version = reader.version;

            if (version[0] < 5 || (version[0] == 5 && version[1] < 5)) //5.5 down
            {
                m_BoolValues = reader.ReadBooleanArray();
                reader.AlignStream();
                m_IntValues = reader.ReadInt32Array();
                m_FloatValues = reader.ReadSingleArray();
            }

            if (version[0] < 4 || (version[0] == 4 && version[1] < 3)) //4.3 down
            {
                m_VectorValues = reader.ReadVector4Array();
            }
            else
            {
                int numPosValues = reader.ReadInt32();
                m_PositionValues = new Vector3[numPosValues];
                for (int i = 0; i < numPosValues; i++)
                {
                    m_PositionValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4(); //5.4 and up
                }

                m_QuaternionValues = reader.ReadVector4Array();

                int numScaleValues = reader.ReadInt32();
                m_ScaleValues = new Vector3[numScaleValues];
                for (int i = 0; i < numScaleValues; i++)
                {
                    m_ScaleValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? reader.ReadVector3() : (Vector3)reader.ReadVector4(); //5.4 and up
                }

                if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) //5.5 and up
                {
                    m_FloatValues = reader.ReadSingleArray();
                    m_IntValues = reader.ReadInt32Array();
                    m_BoolValues = reader.ReadBooleanArray();
                    reader.AlignStream();
                }
            }
        }
    }

    public class ControllerConstant
    {
        public LayerConstant[] m_LayerArray;
        public StateMachineConstant[] m_StateMachineArray;
        public ValueArrayConstant m_Values;
        public ValueArray m_DefaultValues;

        public ControllerConstant(ObjectReader reader)
        {
            int numLayers = reader.ReadInt32();
            m_LayerArray = new LayerConstant[numLayers];
            for (int i = 0; i < numLayers; i++)
            {
                m_LayerArray[i] = new LayerConstant(reader);
            }

            int numStates = reader.ReadInt32();
            m_StateMachineArray = new StateMachineConstant[numStates];
            for (int i = 0; i < numStates; i++)
            {
                m_StateMachineArray[i] = new StateMachineConstant(reader);
            }

            m_Values = new ValueArrayConstant(reader);
            m_DefaultValues = new ValueArray(reader);
        }
    }

    public sealed class AnimatorController : RuntimeAnimatorController
    {
        public PPtr<AnimationClip>[] m_AnimationClips;

        public AnimatorController(ObjectReader reader) : base(reader)
        {
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = new ControllerConstant(reader);

            int tosSize = reader.ReadInt32();
            var m_TOS = new KeyValuePair<uint, string>[tosSize];
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            int numClips = reader.ReadInt32();
            m_AnimationClips = new PPtr<AnimationClip>[numClips];
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips[i] = new PPtr<AnimationClip>(reader);
            }
        }
    }
}
