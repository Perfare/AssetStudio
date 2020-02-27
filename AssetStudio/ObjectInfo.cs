using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectInfo
    {
        public long byteStart;
        public uint byteSize;
        public int typeID;
        public int classID;

        public long m_PathID;
        public SerializedType serializedType;
    }
}
