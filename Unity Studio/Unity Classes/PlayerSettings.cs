using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    public class PlayerSettings
    {
        public string companyName;
        public string productName;

        public PlayerSettings(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;


            if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 4) || sourceFile.version[0] > 5)//5.4.0 nad up
            {
                //productGUID
                a_Stream.ReadInt32();
                a_Stream.ReadInt32();
                a_Stream.ReadInt32();
                a_Stream.ReadInt32();
            }
            if (sourceFile.version[0] >= 3)
            {
                if (sourceFile.version[0] == 3 && sourceFile.version[1] < 2) { string AndroidLicensePublicKey = a_Stream.ReadAlignedString(a_Stream.ReadInt32()); }
                else { bool AndroidProfiler = a_Stream.ReadBoolean(); a_Stream.AlignStream(4); }

                int defaultScreenOrientation = a_Stream.ReadInt32();
                int targetDevice = a_Stream.ReadInt32();

                if (sourceFile.version[0] < 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] < 1))
                { int targetGlesGraphics = a_Stream.ReadInt32(); }

                if ((sourceFile.version[0] == 5 && sourceFile.version[1] < 1) || (sourceFile.version[0] == 4 && sourceFile.version[1] == 6 && sourceFile.version[2] >= 3))
                { int targetIOSGraphics = a_Stream.ReadInt32(); }

                if (sourceFile.version[0] >= 5 || sourceFile.version[0] == 5 && (sourceFile.version[1] > 2 || (sourceFile.version[1] == 2 && sourceFile.version[2] >= 1)))
                { bool useOnDemandResources = a_Stream.ReadBoolean(); a_Stream.AlignStream(4); }

                if (sourceFile.version[0] < 5 || (sourceFile.version[0] == 5 && sourceFile.version[1] < 3))
                { int targetResolution = a_Stream.ReadInt32(); }

                if (sourceFile.version[0] == 3 && sourceFile.version[1] <= 1) { bool OverrideIPodMusic = a_Stream.ReadBoolean(); a_Stream.AlignStream(4); }
                else if (sourceFile.version[0] == 3 && sourceFile.version[1] <= 4) { }
                else { int accelerometerFrequency = a_Stream.ReadInt32(); }//3.5.0 and up
            }
            //fail in Unity 5 beta
            companyName = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            productName = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
        }
    }
}
