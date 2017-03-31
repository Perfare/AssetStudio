using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{

    class AssetBundle
    {
        public class AssetInfo
        {
            public int preloadIndex;
            public int preloadSize;
            public PPtr asset;
        }

        public class ContainerData
        {
            public string first;
            public AssetInfo second;
        }


        public List<ContainerData> m_Container = new List<ContainerData>();

        public AssetBundle(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            var m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            var size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                sourceFile.ReadPPtr();
            }
            size = a_Stream.ReadInt32();
            for (int i = 0; i < size; i++)
            {
                var temp = new ContainerData();
                temp.first = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                temp.second = new AssetInfo();
                temp.second.preloadIndex = a_Stream.ReadInt32();
                temp.second.preloadSize = a_Stream.ReadInt32();
                temp.second.asset = sourceFile.ReadPPtr();
                m_Container.Add(temp);
            }
        }
    }
}
