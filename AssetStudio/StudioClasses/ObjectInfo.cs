using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectInfo
    {
        public uint byteStart;
        public uint byteSize;
        public int typeID;
        public int classID;
        public ushort isDestroyed;

        //custom
        public long m_PathID;
        public SerializedType serializedType;
    }
}
