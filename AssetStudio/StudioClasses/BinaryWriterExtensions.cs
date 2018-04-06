using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class BinaryWriterExtensions
    {
        private static void WriteArray<T>(Action<T> del, T[] array)
        {
            foreach (var item in array)
            {
                del(item);
            }
        }

        public static void Write(this BinaryWriter writer, uint[] array)
        {
            WriteArray(writer.Write, array);
        }
    }
}
