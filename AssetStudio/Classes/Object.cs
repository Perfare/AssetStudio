using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Object
    {
        protected SerializedFile sourceFile;
        public ObjectReader reader;
        public int[] version;
        protected BuildType buildType;
        public BuildTarget platform;

        protected Object(ObjectReader reader)
        {
            this.reader = reader;
            reader.Reset();
            sourceFile = reader.assetsFile;
            version = reader.version;
            buildType = reader.buildType;
            platform = reader.platform;

            if (platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }
    }
}
