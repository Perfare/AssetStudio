using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr asset;

        public AssetInfo(ObjectReader reader)
        {
            preloadIndex = reader.ReadInt32();
            preloadSize = reader.ReadInt32();
            asset = reader.ReadPPtr();
        }
    }

    public sealed class AssetBundle : NamedObject
    {
        public List<PPtr> m_PreloadTable;
        public List<KeyValuePair<string, AssetInfo>> m_Container;

        public AssetBundle(ObjectReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = new List<PPtr>(m_PreloadTableSize);
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                m_PreloadTable.Add(reader.ReadPPtr());
            }

            var m_ContainerSize = reader.ReadInt32();
            m_Container = new List<KeyValuePair<string, AssetInfo>>(m_ContainerSize);
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container.Add(new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader)));
            }
        }
    }
}
