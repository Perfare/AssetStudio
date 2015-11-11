using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class BuildSettings
    {
        public string m_Version;

        public BuildSettings(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            int levels = a_Stream.ReadInt32();
            for (int l = 0; l < levels; l++) { string level = a_Stream.ReadAlignedString(a_Stream.ReadInt32()); }

            if (sourceFile.version[0] == 5)
            {
                int preloadedPlugins = a_Stream.ReadInt32();
                for (int l = 0; l < preloadedPlugins; l++) { string preloadedPlugin = a_Stream.ReadAlignedString(a_Stream.ReadInt32()); }
            }

            a_Stream.Position += 4; //bool flags
            if (sourceFile.fileGen >= 8) { a_Stream.Position += 4; } //bool flags
            if (sourceFile.fileGen >= 9) { a_Stream.Position += 4; } //bool flags
            if (sourceFile.version[0] == 5 || 
                (sourceFile.version[0] == 4 && (sourceFile.version[1] >= 3 || 
                                                (sourceFile.version[1] == 2 && sourceFile.buildType[0] != "a"))))
                                                { a_Stream.Position += 4; } //bool flags

            m_Version = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
        }
    }
}
