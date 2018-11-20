using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class AnimationClipOverride
    {
        public PPtr m_OriginalClip;
        public PPtr m_OverrideClip;

        public AnimationClipOverride(ObjectReader reader)
        {
            m_OriginalClip = reader.ReadPPtr();
            m_OverrideClip = reader.ReadPPtr();
        }
    }

    public class AnimatorOverrideController : NamedObject
    {
        public PPtr m_Controller;
        public List<AnimationClipOverride> m_Clips;

        public AnimatorOverrideController(ObjectReader reader) : base(reader)
        {
            m_Controller = reader.ReadPPtr();

            int numOverrides = reader.ReadInt32();
            m_Clips = new List<AnimationClipOverride>(numOverrides);
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips.Add(new AnimationClipOverride(reader));
            }
        }
    }
}
