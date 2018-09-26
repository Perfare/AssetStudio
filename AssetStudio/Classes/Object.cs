using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public abstract class Object
    {
        public AssetPreloadData preloadData;
        public AssetsFile sourceFile;
        protected EndianBinaryReader reader;
        public int[] version;
        protected string[] buildType;
        protected BuildTarget platform;

        protected Object(AssetPreloadData preloadData)
        {
            this.preloadData = preloadData;
            sourceFile = preloadData.sourceFile;
            reader = preloadData.InitReader();
            version = sourceFile.version;
            buildType = sourceFile.buildType;
            platform = sourceFile.m_TargetPlatform;

            if (platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }
    }
}
