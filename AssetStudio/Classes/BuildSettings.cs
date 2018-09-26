using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class BuildSettings : Object
    {
        public string m_Version;

        public BuildSettings(AssetPreloadData preloadData) : base(preloadData)
        {
            int levels = reader.ReadInt32();
            for (int l = 0; l < levels; l++)
            {
                var level = reader.ReadAlignedString();
            }

            if (version[0] >= 5)
            {
                int preloadedPlugins = reader.ReadInt32();
                for (int l = 0; l < preloadedPlugins; l++)
                {
                    var preloadedPlugin = reader.ReadAlignedString();
                }
            }

            reader.Position += 4;
            if (version[0] >= 3) //3.0 and up
            {
                reader.Position += 4;
            }
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 5))//3.5 and up
            {
                reader.Position += 4;
            }
            if (version[0] >= 5 || (version[0] == 4 && (version[1] >= 3 || (version[1] == 2 && buildType[0] != "a"))))
            {
                reader.Position += 4;
            }
            m_Version = reader.ReadAlignedString();
        }
    }
}
