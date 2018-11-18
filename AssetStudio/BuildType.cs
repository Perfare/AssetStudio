using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class BuildType
    {
        private string buildType;

        public BuildType(string type)
        {
            buildType = type;
        }

        public bool IsAlpha => buildType == "a";
        public bool IsPatch => buildType == "p";
    }
}
