using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class BuildSettings
    {
        public string m_Version;

        public BuildSettings(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.InitReader();

            int levels = reader.ReadInt32();
            for (int l = 0; l < levels; l++) { string level = reader.ReadAlignedString(); }

            if (sourceFile.version[0] == 5)
            {
                int preloadedPlugins = reader.ReadInt32();
                for (int l = 0; l < preloadedPlugins; l++) { string preloadedPlugin = reader.ReadAlignedString(); }
            }

            reader.Position += 4; //bool flags
            if (sourceFile.fileGen >= 8) { reader.Position += 4; } //bool flags
            if (sourceFile.fileGen >= 9) { reader.Position += 4; } //bool flags
            if (sourceFile.version[0] == 5 || 
                (sourceFile.version[0] == 4 && (sourceFile.version[1] >= 3 || 
                                                (sourceFile.version[1] == 2 && sourceFile.buildType[0] != "a"))))
                                                { reader.Position += 4; } //bool flags

            m_Version = reader.ReadAlignedString();
        }
    }
}
