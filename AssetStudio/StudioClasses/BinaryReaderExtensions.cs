using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;

namespace AssetStudio
{
    public static class BinaryReaderExtensions
    {
        public static void AlignStream(this BinaryReader reader, int alignment)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                reader.BaseStream.Position += alignment - mod;
            }
        }

        public static string ReadAlignedString(this BinaryReader reader)
        {
            return ReadAlignedString(reader, reader.ReadInt32());
        }

        public static string ReadAlignedString(this BinaryReader reader, int length)
        {
            if (length > 0 && length < (reader.BaseStream.Length - reader.BaseStream.Position))
            {
                var stringData = reader.ReadBytes(length);
                var result = Encoding.UTF8.GetString(stringData);
                reader.AlignStream(4);
                return result;
            }
            return "";
        }

        public static string ReadStringToNull(this BinaryReader reader)
        {
            var bytes = new List<byte>();
            byte b;
            while (reader.BaseStream.Position != reader.BaseStream.Length && (b = reader.ReadByte()) != 0)
                bytes.Add(b);
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            var q = new Quaternion
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                W = reader.ReadSingle()
            };
            return q;
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            Vector2 v = new Vector2
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle()
            };
            return v;
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            var v = new Vector3
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
            return v;
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            var v = new Vector4
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                W = reader.ReadSingle()
            };
            return v;
        }

        private static T[] ReadArray<T>(Func<T> del, int length)
        {
            var array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = del();
            }
            return array;
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadInt32, length);
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadUInt32, length);
        }

        public static float[] ReadSingleArray(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadSingle, length);
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadVector2, length);
        }

        public static Vector4[] ReadVector4Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadVector4, length);
        }
    }
}
