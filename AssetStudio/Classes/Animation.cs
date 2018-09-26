using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class Animation : Behaviour
    {
        public List<PPtr> m_Animations;

        public Animation(AssetPreloadData preloadData) : base(preloadData)
        {
            var m_Animation = sourceFile.ReadPPtr();
            int numAnimations = reader.ReadInt32();
            m_Animations = new List<PPtr>(numAnimations);
            for (int i = 0; i < numAnimations; i++)
            {
                m_Animations.Add(sourceFile.ReadPPtr());
            }
        }
    }
}
