using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class SerializedFileHeader
    {
        public uint m_MetadataSize;
        public long m_FileSize;
        public uint m_Version;
        public long m_DataOffset;
        public byte m_Endianess;
        public byte[] m_Reserved;
    }
}
