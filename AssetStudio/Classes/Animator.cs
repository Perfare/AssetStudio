using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class Animator : Behaviour
    {
        public PPtr<Avatar> m_Avatar;
        public PPtr<RuntimeAnimatorController> m_Controller;
        public bool m_HasTransformHierarchy = true;

        public Animator(ObjectReader reader) : base(reader)
        {
            m_Avatar = new PPtr<Avatar>(reader);
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);
            var m_CullingMode = reader.ReadInt32();

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                var m_UpdateMode = reader.ReadInt32();
            }

            var m_ApplyRootMotion = reader.ReadBoolean();
            if (version[0] == 4 && version[1] >= 5) //4.5 and up - 5.0 down
            {
                reader.AlignStream();
            }

            if (version[0] >= 5) //5.0 and up
            {
                var m_LinearVelocityBlending = reader.ReadBoolean();
                if (version[0] > 2021 || (version[0] == 2021 && version[1] >= 2)) //2021.2 and up
                {
                    var m_StabilizeFeet = reader.ReadBoolean();
                }
                reader.AlignStream();
            }

            if (version[0] < 4 || (version[0] == 4 && version[1] < 5)) //4.5 down
            {
                var m_AnimatePhysics = reader.ReadBoolean();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_HasTransformHierarchy = reader.ReadBoolean();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 5)) //4.5 and up
            {
                var m_AllowConstantClipSamplingOptimization = reader.ReadBoolean();
            }
            if (version[0] >= 5 && version[0] < 2018) //5.0 and up - 2018 down
            {
                reader.AlignStream();
            }

            if (version[0] >= 2018) //2018 and up
            {
                var m_KeepAnimatorControllerStateOnDisable = reader.ReadBoolean();
                reader.AlignStream();
            }
        }
    }
}
