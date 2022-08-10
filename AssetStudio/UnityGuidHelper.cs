using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetStudio
{
    class UnityGuidHelper
    {
        public static Guid UnityGuidToGuid(byte[] data, int offset = 0)
        {
            for (int i = 0; i < 16; ++i)
            {
                data[i + offset] = (byte)(((data[i + offset] & 0xF0) >> 4) | ((data[i + offset] & 0x0F) << 4));
            }

            int d1 = BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>(data, offset, 4));
            short d2 = BinaryPrimitives.ReadInt16BigEndian(new ReadOnlySpan<byte>(data, offset + 4, 2));
            short d3 = BinaryPrimitives.ReadInt16BigEndian(new ReadOnlySpan<byte>(data, offset + 6, 2));

            return new Guid(d1, d2, d3,
                data[offset + 8], data[offset + 9], data[offset + 10], data[offset + 11],
                data[offset + 12], data[offset + 13], data[offset + 14], data[offset + 15]);
        }
    }
}
