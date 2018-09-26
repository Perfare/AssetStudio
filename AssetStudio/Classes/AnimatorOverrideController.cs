using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class AnimatorOverrideController : NamedObject
    {
        public PPtr m_Controller;
        public PPtr[][] m_Clips;

        public AnimatorOverrideController(AssetPreloadData preloadData) : base(preloadData)
        {
            m_Controller = sourceFile.ReadPPtr();

            int numOverrides = reader.ReadInt32();
            m_Clips = new PPtr[numOverrides][];
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips[i] = new PPtr[2];
                m_Clips[i][0] = sourceFile.ReadPPtr();
                m_Clips[i][1] = sourceFile.ReadPPtr();
            }
        }
    }
}
