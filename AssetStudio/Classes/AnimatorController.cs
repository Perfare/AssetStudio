using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace AssetStudio
{
    public class HumanPoseMask
    {
        public uint word0 { get; set; }
        public uint word1 { get; set; }
        public uint word2 { get; set; }

        public HumanPoseMask(EndianBinaryReader reader, int[] version)
        {
            word0 = reader.ReadUInt32();
            word1 = reader.ReadUInt32();
            if (version[0] >= 5) //5.0 and up
            {
                word2 = reader.ReadUInt32();
            }
        }
    }

    public class SkeletonMaskElement
    {
        public uint m_PathHash { get; set; }
        public float m_Weight { get; set; }

        public SkeletonMaskElement(EndianBinaryReader reader)
        {
            m_PathHash = reader.ReadUInt32();
            m_Weight = reader.ReadSingle();
        }
    }

    public class SkeletonMask
    {
        public SkeletonMaskElement[] m_Data { get; set; }

        public SkeletonMask(EndianBinaryReader reader)
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
        public uint m_StateMachineIndex { get; set; }
        public uint m_StateMachineMotionSetIndex { get; set; }
        public HumanPoseMask m_BodyMask { get; set; }
        public SkeletonMask m_SkeletonMask { get; set; }
        public uint m_Binding { get; set; }
        public int m_LayerBlendingMode { get; set; }
        public float m_DefaultWeight { get; set; }
        public bool m_IKPass { get; set; }
        public bool m_SyncedLayerAffectsTiming { get; set; }

        public LayerConstant(EndianBinaryReader reader, int[] version)
        {
            m_StateMachineIndex = reader.ReadUInt32();
            m_StateMachineMotionSetIndex = reader.ReadUInt32();
            m_BodyMask = new HumanPoseMask(reader, version);
            m_SkeletonMask = new SkeletonMask(reader);
            m_Binding = reader.ReadUInt32();
            m_LayerBlendingMode = reader.ReadInt32();
            m_DefaultWeight = reader.ReadSingle();
            m_IKPass = reader.ReadBoolean();
            m_SyncedLayerAffectsTiming = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class ConditionConstant
    {
        public uint m_ConditionMode { get; set; }
        public uint m_EventID { get; set; }
        public float m_EventThreshold { get; set; }
        public float m_ExitTime { get; set; }

        public ConditionConstant(EndianBinaryReader reader)
        {
            m_ConditionMode = reader.ReadUInt32();
            m_EventID = reader.ReadUInt32();
            m_EventThreshold = reader.ReadSingle();
            m_ExitTime = reader.ReadSingle();
        }
    }

    public class TransitionConstant
    {
        public ConditionConstant[] m_ConditionConstantArray { get; set; }
        public uint m_DestinationState { get; set; }
        public uint m_FullPathID { get; set; }
        public uint m_ID { get; set; }
        public uint m_UserID { get; set; }
        public float m_TransitionDuration { get; set; }
        public float m_TransitionOffset { get; set; }
        public float m_ExitTime { get; set; }
        public bool m_HasExitTime { get; set; }
        public bool m_HasFixedDuration { get; set; }
        public int m_InterruptionSource { get; set; }
        public bool m_OrderedInterruption { get; set; }
        public bool m_Atomic { get; set; }
        public bool m_CanTransitionToSelf { get; set; }

        public TransitionConstant(EndianBinaryReader reader, int[] version)
        {
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
                reader.AlignStream(4);
                m_InterruptionSource = reader.ReadInt32();
                m_OrderedInterruption = reader.ReadBoolean();
            }
            else
            {
                m_Atomic = reader.ReadBoolean();
            }

            m_CanTransitionToSelf = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class LeafInfoConstant
    {
        public uint[] m_IDArray { get; set; }
        public uint m_IndexOffset { get; set; }

        public LeafInfoConstant(EndianBinaryReader reader)
        {
            m_IDArray = reader.ReadUInt32Array(reader.ReadInt32());
            m_IndexOffset = reader.ReadUInt32();
        }
    }

    public class MotionNeighborList
    {
        public uint[] m_NeighborArray { get; set; }

        public MotionNeighborList(EndianBinaryReader reader)
        {
            m_NeighborArray = reader.ReadUInt32Array(reader.ReadInt32());
        }
    }

    public class Blend2dDataConstant
    {
        public Vector2[] m_ChildPositionArray { get; set; }
        public float[] m_ChildMagnitudeArray { get; set; }
        public Vector2[] m_ChildPairVectorArray { get; set; }
        public float[] m_ChildPairAvgMagInvArray { get; set; }
        public MotionNeighborList[] m_ChildNeighborListArray { get; set; }

        public Blend2dDataConstant(EndianBinaryReader reader)
        {
            m_ChildPositionArray = reader.ReadVector2Array(reader.ReadInt32());
            m_ChildMagnitudeArray = reader.ReadSingleArray(reader.ReadInt32());
            m_ChildPairVectorArray = reader.ReadVector2Array(reader.ReadInt32());
            m_ChildPairAvgMagInvArray = reader.ReadSingleArray(reader.ReadInt32());

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
        public float[] m_ChildThresholdArray { get; set; }

        public Blend1dDataConstant(EndianBinaryReader reader)
        {
            m_ChildThresholdArray = reader.ReadSingleArray(reader.ReadInt32());
        }
    }

    public class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray { get; set; }
        public bool m_NormalizedBlendValues { get; set; }

        public BlendDirectDataConstant(EndianBinaryReader reader)
        {
            m_ChildBlendEventIDArray = reader.ReadUInt32Array(reader.ReadInt32());
            m_NormalizedBlendValues = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class BlendTreeNodeConstant
    {
        public uint m_BlendType { get; set; }
        public uint m_BlendEventID { get; set; }
        public uint m_BlendEventYID { get; set; }
        public uint[] m_ChildIndices { get; set; }
        public Blend1dDataConstant m_Blend1dData { get; set; }
        public Blend2dDataConstant m_Blend2dData { get; set; }
        public BlendDirectDataConstant m_BlendDirectData { get; set; }
        public uint m_ClipID { get; set; }
        public uint m_ClipIndex { get; set; }
        public float m_Duration { get; set; }
        public float m_CycleOffset { get; set; }
        public bool m_Mirror { get; set; }

        public BlendTreeNodeConstant(EndianBinaryReader reader, int[] version)
        {
            m_BlendType = reader.ReadUInt32();
            m_BlendEventID = reader.ReadUInt32();
            m_BlendEventYID = reader.ReadUInt32();
            m_ChildIndices = reader.ReadUInt32Array(reader.ReadInt32());
            m_Blend1dData = new Blend1dDataConstant(reader);
            m_Blend2dData = new Blend2dDataConstant(reader);
            if (version[0] >= 5) //5.0 and up
            {
                m_BlendDirectData = new BlendDirectDataConstant(reader);
            }

            m_ClipID = reader.ReadUInt32();
            if (version[0] < 5) //5.0 down
            {
                m_ClipIndex = reader.ReadUInt32();
            }

            m_Duration = reader.ReadSingle();
            m_CycleOffset = reader.ReadSingle();
            m_Mirror = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class BlendTreeConstant
    {
        public BlendTreeNodeConstant[] m_NodeArray { get; set; }

        public BlendTreeConstant(EndianBinaryReader reader, int[] version)
        {
            int numNodes = reader.ReadInt32();
            m_NodeArray = new BlendTreeNodeConstant[numNodes];
            for (int i = 0; i < numNodes; i++)
            {
                m_NodeArray[i] = new BlendTreeNodeConstant(reader, version);
            }
        }
    }


    public class StateConstant
    {
        public TransitionConstant[] m_TransitionConstantArray { get; set; }
        public int[] m_BlendTreeConstantIndexArray { get; set; }
        public LeafInfoConstant[] m_LeafInfoArray { get; set; }
        public BlendTreeConstant[] m_BlendTreeConstantArray { get; set; }
        public uint m_NameID { get; set; }
        public uint m_PathID { get; set; }
        public uint m_FullPathID { get; set; }
        public uint m_TagID { get; set; }
        public uint m_SpeedParamID { get; set; }
        public uint m_MirrorParamID { get; set; }
        public uint m_CycleOffsetParamID { get; set; }
        public float m_Speed { get; set; }
        public float m_CycleOffset { get; set; }
        public bool m_IKOnFeet { get; set; }
        public bool m_WriteDefaultValues { get; set; }
        public bool m_Loop { get; set; }
        public bool m_Mirror { get; set; }

        public StateConstant(EndianBinaryReader reader, int[] version)
        {
            int numTransistions = reader.ReadInt32();
            m_TransitionConstantArray = new TransitionConstant[numTransistions];
            for (int i = 0; i < numTransistions; i++)
            {
                m_TransitionConstantArray[i] = new TransitionConstant(reader, version);
            }

            int numBlendIndices = reader.ReadInt32();
            m_BlendTreeConstantIndexArray = new int[numBlendIndices];
            for (int i = 0; i < numBlendIndices; i++)
            {
                m_BlendTreeConstantIndexArray[i] = reader.ReadInt32();
            }

            if (version[0] < 5) //5.0 down
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
                m_BlendTreeConstantArray[i] = new BlendTreeConstant(reader, version);
            }

            m_NameID = reader.ReadUInt32();
            m_PathID = reader.ReadUInt32();
            if (version[0] >= 5) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_TagID = reader.ReadUInt32();
            if (version[0] >= 5) //5.0 and up
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
            m_CycleOffset = reader.ReadSingle();
            m_IKOnFeet = reader.ReadBoolean();
            if (version[0] >= 5) //5.0 and up
            {
                m_WriteDefaultValues = reader.ReadBoolean();
            }

            m_Loop = reader.ReadBoolean();
            m_Mirror = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class SelectorTransitionConstant
    {
        public uint m_Destination { get; set; }
        public ConditionConstant[] m_ConditionConstantArray { get; set; }

        public SelectorTransitionConstant(EndianBinaryReader reader)
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
        public SelectorTransitionConstant[] m_TransitionConstantArray { get; set; }
        public uint m_FullPathID { get; set; }
        public bool m_isEntry { get; set; }

        public SelectorStateConstant(EndianBinaryReader reader)
        {
            int numTransitions = reader.ReadInt32();
            m_TransitionConstantArray = new SelectorTransitionConstant[numTransitions];
            for (int i = 0; i < numTransitions; i++)
            {
                m_TransitionConstantArray[i] = new SelectorTransitionConstant(reader);
            }

            m_FullPathID = reader.ReadUInt32();
            m_isEntry = reader.ReadBoolean();
            reader.AlignStream(4);
        }
    }

    public class StateMachineConstant
    {
        public StateConstant[] m_StateConstantArray { get; set; }
        public TransitionConstant[] m_AnyStateTransitionConstantArray { get; set; }
        public SelectorStateConstant[] m_SelectorStateConstantArray { get; set; }
        public uint m_DefaultState { get; set; }
        public uint m_MotionSetCount { get; set; }

        public StateMachineConstant(EndianBinaryReader reader, int[] version)
        {
            int numStates = reader.ReadInt32();
            m_StateConstantArray = new StateConstant[numStates];
            for (int i = 0; i < numStates; i++)
            {
                m_StateConstantArray[i] = new StateConstant(reader, version);
            }

            int numAnyStates = reader.ReadInt32();
            m_AnyStateTransitionConstantArray = new TransitionConstant[numAnyStates];
            for (int i = 0; i < numAnyStates; i++)
            {
                m_AnyStateTransitionConstantArray[i] = new TransitionConstant(reader, version);
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
        public bool[] m_BoolValues { get; set; }
        public int[] m_IntValues { get; set; }
        public float[] m_FloatValues { get; set; }
        public object[] m_PositionValues { get; set; }
        public Vector4[] m_QuaternionValues { get; set; }
        public object[] m_ScaleValues { get; set; }

        public ValueArray(EndianBinaryReader reader, int[] version)
        {
            if (version[0] < 5 || (version[0] == 5 && version[1] < 5)) //5.5 down
            {
                int numBools = reader.ReadInt32();
                m_BoolValues = new bool[numBools];
                for (int i = 0; i < numBools; i++)
                {
                    m_BoolValues[i] = reader.ReadBoolean();
                }

                reader.AlignStream(4);

                m_IntValues = reader.ReadInt32Array(reader.ReadInt32());
                m_FloatValues = reader.ReadSingleArray(reader.ReadInt32());
            }

            int numPosValues = reader.ReadInt32();
            m_PositionValues = new object[numPosValues];
            for (int i = 0; i < numPosValues; i++)
            {
                m_PositionValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? (object)reader.ReadVector3() : (object)reader.ReadVector4(); //5.4 and up
            }

            m_QuaternionValues = reader.ReadVector4Array(reader.ReadInt32());

            int numScaleValues = reader.ReadInt32();
            m_ScaleValues = new object[numScaleValues];
            for (int i = 0; i < numScaleValues; i++)
            {
                m_ScaleValues[i] = version[0] > 5 || (version[0] == 5 && version[1] >= 4) ? (object)reader.ReadVector3() : (object)reader.ReadVector4(); //5.4 adn up
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) //5.5 and up
            {
                m_FloatValues = reader.ReadSingleArray(reader.ReadInt32());
                m_IntValues = reader.ReadInt32Array(reader.ReadInt32());

                int numBools = reader.ReadInt32();
                m_BoolValues = new bool[numBools];
                for (int i = 0; i < numBools; i++)
                {
                    m_BoolValues[i] = reader.ReadBoolean();
                }

                reader.AlignStream(4);
            }
        }
    }

    public class ControllerConstant
    {
        public LayerConstant[] m_LayerArray { get; set; }
        public StateMachineConstant[] m_StateMachineArray { get; set; }
        public ValueArrayConstant m_Values { get; set; }
        public ValueArray m_DefaultValues { get; set; }

        public ControllerConstant(EndianBinaryReader reader, int[] version)
        {
            int numLayers = reader.ReadInt32();
            m_LayerArray = new LayerConstant[numLayers];
            for (int i = 0; i < numLayers; i++)
            {
                m_LayerArray[i] = new LayerConstant(reader, version);
            }

            int numStates = reader.ReadInt32();
            m_StateMachineArray = new StateMachineConstant[numStates];
            for (int i = 0; i < numStates; i++)
            {
                m_StateMachineArray[i] = new StateMachineConstant(reader, version);
            }

            m_Values = new ValueArrayConstant(reader, version);
            m_DefaultValues = new ValueArray(reader, version);
        }
    }

    public sealed class AnimatorController : NamedObject
    {
        public PPtr[] m_AnimationClips;

        public AnimatorController(AssetPreloadData preloadData) : base(preloadData)
        {
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = new ControllerConstant(reader, version);

            int tosSize = reader.ReadInt32();
            var m_TOS = new List<KeyValuePair<uint, string>>(tosSize);
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS.Add(new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString()));
            }

            int numClips = reader.ReadInt32();
            m_AnimationClips = new PPtr[numClips];
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips[i] = sourceFile.ReadPPtr();
            }
        }
    }
}
