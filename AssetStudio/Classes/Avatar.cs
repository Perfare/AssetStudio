using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class Avatar
    {
        public string m_Name;
        public List<KeyValuePair<uint, string>> m_TOS;

        public Avatar(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var version = sourceFile.version;
            var reader = preloadData.InitReader();
            reader.Position = preloadData.Offset;

            m_Name = reader.ReadAlignedString();
            var m_AvatarSize = reader.ReadUInt32();
            //AvatarConstant m_Avatar
            //- OffsetPtr m_AvatarSkeleton
            //-- Skeleton data
            //--- vector m_Node
            var numNodes = reader.ReadInt32();
            for (int i = 0; i < numNodes; i++)
            {
                reader.Position += 8;
            }
            //--- vector m_ID
            int numIDs = reader.ReadInt32();
            for (int i = 0; i < numIDs; i++)
            {
                reader.Position += 4;
            }
            //--- vector m_AxesArray
            int numAxes = reader.ReadInt32();
            for (int i = 0; i < numAxes; i++)
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                    reader.Position += 76;
                else
                    reader.Position += 88;
            }
            //- OffsetPtr m_AvatarSkeletonPose
            //-- SkeletonPose data
            int numXforms = reader.ReadInt32();
            for (int i = 0; i < numXforms; i++)
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                    reader.Position += 40;
                else
                    reader.Position += 48;
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3))//4.3 and up
            {
                //- OffsetPtr m_DefaultPose
                //-- SkeletonPose data
                numXforms = reader.ReadInt32();
                for (int i = 0; i < numXforms; i++)
                {
                    if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                        reader.Position += 40;
                    else
                        reader.Position += 48;
                }
                //- vector m_SkeletonNameIDArray
                numIDs = reader.ReadInt32();
                for (int i = 0; i < numIDs; i++)
                {
                    reader.Position += 4;
                }
            }
            //- OffsetPtr m_Human
            //-- Human data
            //--- xform m_RootX
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                reader.Position += 40;
            else
                reader.Position += 48;
            //--- OffsetPtr m_Skeleton
            //---- Skeleton data
            numNodes = reader.ReadInt32();
            for (int i = 0; i < numNodes; i++)
            {
                reader.Position += 8;
            }
            //--- vector m_ID
            numIDs = reader.ReadInt32();
            for (int i = 0; i < numIDs; i++)
            {
                reader.Position += 4;
            }
            //--- vector m_AxesArray
            numAxes = reader.ReadInt32();
            for (int i = 0; i < numAxes; i++)
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                    reader.Position += 76;
                else
                    reader.Position += 88;
            }
            //--- OffsetPtr m_SkeletonPose
            //---- SkeletonPose data
            numXforms = reader.ReadInt32();
            for (int i = 0; i < numXforms; i++)
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                    reader.Position += 40;
                else
                    reader.Position += 48;
            }
            //--- OffsetPtr m_LeftHand
            //---- Hand data
            //----- staticvector m_HandBoneIndex
            int numIndexes = reader.ReadInt32();
            for (int i = 0; i < numIndexes; i++)
            {
                reader.Position += 4;
            }
            //--- OffsetPtr m_RightHand
            numIndexes = reader.ReadInt32();
            for (int i = 0; i < numIndexes; i++)
            {
                reader.Position += 4;
            }

            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                //--- vector m_Handles
                int numHandles = reader.ReadInt32();
                for (int i = 0; i < numHandles; i++)
                {
                    if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
                        reader.Position += 48;
                    else
                        reader.Position += 56;
                }
                //--- vector m_ColliderArray
                int numColliders = reader.ReadInt32();
                for (int i = 0; i < numColliders; i++)
                {
                    if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
                        reader.Position += 72;
                    else
                        reader.Position += 80;
                }
            }
            //--- staticvector m_HumanBoneIndex
            numIndexes = reader.ReadInt32();
            for (int i = 0; i < numIndexes; i++)
            {
                reader.Position += 4;
            }
            //--- staticvector m_HumanBoneMass
            int numMasses = reader.ReadInt32();
            for (int i = 0; i < numMasses; i++)
            {
                reader.Position += 4;
            }
            if (version[0] < 2018 || (version[0] == 2018 && version[1] < 2)) //2018.2 down
            {
                //--- staticvector m_ColliderIndex
                int numColliderIndexes = reader.ReadInt32();
                for (int i = 0; i < numColliderIndexes; i++)
                {
                    reader.Position += 4;
                }
            }
            var m_Scale = reader.ReadSingle();
            var m_ArmTwist = reader.ReadSingle();
            var m_ForeArmTwist = reader.ReadSingle();
            var m_UpperLegTwist = reader.ReadSingle();
            var m_LegTwist = reader.ReadSingle();
            var m_ArmStretch = reader.ReadSingle();
            var m_LegStretch = reader.ReadSingle();
            var m_FeetSpacing = reader.ReadSingle();
            var m_HasLeftHand = reader.ReadBoolean();
            var m_HasRightHand = reader.ReadBoolean();
            var m_HasTDoF = reader.ReadBoolean();
            reader.AlignStream(4);
            //- vector m_HumanSkeletonIndexArray
            numIndexes = reader.ReadInt32();
            for (int i = 0; i < numIndexes; i++)
            {
                reader.Position += 4;
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                //- vector m_HumanSkeletonReverseIndexArray
                int numReverseIndexes = reader.ReadInt32();
                for (int i = 0; i < numReverseIndexes; i++)
                {
                    reader.Position += 4;
                }
            }
            var m_RootMotionBoneIndex = reader.ReadInt32();
            //- xform m_RootMotionBoneX
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                reader.Position += 40;
            else
                reader.Position += 48;
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                //- OffsetPtr m_RootMotionSkeleton
                //-- Skeleton data
                //--- vector m_Node
                numNodes = reader.ReadInt32();
                for (int i = 0; i < numNodes; i++)
                {
                    reader.Position += 8;
                }
                //--- vector m_ID
                numIDs = reader.ReadInt32();
                for (int i = 0; i < numIDs; i++)
                {
                    reader.Position += 4;
                }
                //--- vector m_AxesArray
                numAxes = reader.ReadInt32();
                for (int i = 0; i < numAxes; i++)
                {
                    if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                        reader.Position += 76;
                    else
                        reader.Position += 88;
                }
                //- OffsetPtr m_RootMotionSkeletonPose
                //-- SkeletonPose data
                numXforms = reader.ReadInt32();
                for (int i = 0; i < numXforms; i++)
                {
                    if (version[0] > 5 || (version[0] == 5 && version[1] >= 4))//5.4 and up
                        reader.Position += 40;
                    else
                        reader.Position += 48;
                }
                //- vector m_RootMotionSkeletonIndexArray
                int numMotionIndexes = reader.ReadInt32();
                for (int i = 0; i < numMotionIndexes; i++)
                {
                    reader.Position += 4;
                }
            }
            //map m_TOS
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
    }
}
