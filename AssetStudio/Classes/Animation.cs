using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class Animation : Behaviour
    {
        public PPtr<AnimationClip>[] m_Animations;

        public Animation(ObjectReader reader) : base(reader)
        {
            var m_Animation = new PPtr<AnimationClip>(reader);
            int numAnimations = reader.ReadInt32();
            m_Animations = new PPtr<AnimationClip>[numAnimations];
            for (int i = 0; i < numAnimations; i++)
            {
                m_Animations[i] = new PPtr<AnimationClip>(reader);
            }
        }
    }
}
