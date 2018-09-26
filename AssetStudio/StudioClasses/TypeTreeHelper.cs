using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class TypeTreeHelper
    {
        public static void ReadTypeString(StringBuilder sb, List<TypeTree> members, EndianBinaryReader reader)
        {
            for (int i = 0; i < members.Count; i++)
            {
                ReadStringValue(sb, members, reader, ref i);
            }
        }

        private static void ReadStringValue(StringBuilder sb, List<TypeTree> members, EndianBinaryReader reader, ref int i)
        {
            var member = members[i];
            var level = member.m_Depth;
            var varTypeStr = member.m_Type;
            var varNameStr = member.m_Name;
            object value = null;
            var append = true;
            var align = (member.m_MetaFlag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    append = false;
                    var str = reader.ReadAlignedString();
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, str);
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 1)), "Array", "Array");
                        var size = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level + 1)), "int", "size", size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 2)), j);
                            int tmp = 0;
                            ReadStringValue(sb, vector, reader, ref tmp);
                        }
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 1)), "Array", "Array");
                        var size = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level + 1)), "int", "size", size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].m_Depth, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 2)), j);
                            sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level + 2)), "pair", "data");
                            int tmp1 = 0;
                            int tmp2 = 0;
                            ReadStringValue(sb, first, reader, ref tmp1);
                            ReadStringValue(sb, second, reader, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        append = false;
                        var size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        i += 2;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), "int", "size", size);
                        break;
                    }
                default:
                    {
                        if (i != members.Count && members[i + 1].m_Type == "Array")
                        {
                            goto case "vector";
                        }
                        append = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        var @class = GetMembers(members, level, i);
                        @class.RemoveAt(0);
                        i += @class.Count;
                        for (int j = 0; j < @class.Count; j++)
                        {
                            ReadStringValue(sb, @class, reader, ref j);
                        }
                        break;
                    }
            }
            if (append)
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
            if (align)
                reader.AlignStream(4);
        }

        public static ExpandoObject ReadDynamicType(List<TypeTree> members, EndianBinaryReader reader)
        {
            var obj = new ExpandoObject();
            var objdic = (IDictionary<string, object>)obj;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var varNameStr = member.m_Name;
                objdic[varNameStr] = ReadValue(members, reader, ref i);
            }
            return obj;
        }

        private static object ReadValue(List<TypeTree> members, EndianBinaryReader reader, ref int i)
        {
            var member = members[i];
            var level = member.m_Depth;
            var varTypeStr = member.m_Type;
            object value;
            var align = (member.m_MetaFlag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    value = reader.ReadSByte();
                    break;
                case "UInt8":
                    value = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    value = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    value = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    value = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    value = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    value = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    value = reader.ReadUInt64();
                    break;
                case "float":
                    value = reader.ReadSingle();
                    break;
                case "double":
                    value = reader.ReadDouble();
                    break;
                case "bool":
                    value = reader.ReadBoolean();
                    break;
                case "string":
                    value = reader.ReadAlignedString();
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        var size = reader.ReadInt32();
                        var list = new List<object>(size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            int tmp = 0;
                            list.Add(ReadValue(vector, reader, ref tmp));
                        }
                        value = list;
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        var size = reader.ReadInt32();
                        var dic = new List<KeyValuePair<object, object>>(size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].m_Depth, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            int tmp1 = 0;
                            int tmp2 = 0;
                            dic.Add(new KeyValuePair<object, object>(ReadValue(first, reader, ref tmp1), ReadValue(second, reader, ref tmp2)));
                        }
                        value = dic;
                        break;
                    }
                case "TypelessData":
                    {
                        var size = reader.ReadInt32();
                        value = reader.ReadBytes(size);
                        i += 2;
                        break;
                    }
                default:
                    {
                        if (i != members.Count && members[i + 1].m_Type == "Array")
                        {
                            goto case "vector";
                        }
                        var @class = GetMembers(members, level, i);
                        @class.RemoveAt(0);
                        i += @class.Count;
                        var obj = new ExpandoObject();
                        var objdic = (IDictionary<string, object>)obj;
                        for (int j = 0; j < @class.Count; j++)
                        {
                            var classmember = @class[j];
                            var name = classmember.m_Name;
                            objdic[name] = ReadValue(@class, reader, ref j);
                        }
                        value = obj;
                        break;
                    }
            }
            if (align)
                reader.AlignStream(4);
            return value;
        }

        private static List<TypeTree> GetMembers(List<TypeTree> members, int level, int index)
        {
            var member2 = new List<TypeTree>();
            member2.Add(members[0]);
            for (int i = index + 1; i < members.Count; i++)
            {
                var member = members[i];
                var level2 = member.m_Depth;
                if (level2 <= level)
                {
                    return member2;
                }
                member2.Add(member);
            }
            return member2;
        }

        public static byte[] WriteDynamicType(ExpandoObject obj, List<TypeTree> members)
        {
            var stream = new MemoryStream();
            var write = new BinaryWriter(stream);
            var objdic = (IDictionary<string, object>)obj;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var varNameStr = member.m_Name;
                WriteValue(objdic[varNameStr], members, write, ref i);
            }
            return stream.ToArray();
        }

        private static void WriteValue(object value, List<TypeTree> members, BinaryWriter write, ref int i)
        {
            var member = members[i];
            var level = member.m_Depth;
            var varTypeStr = member.m_Type;
            var align = (member.m_MetaFlag & 0x4000) != 0;
            switch (varTypeStr)
            {
                case "SInt8":
                    write.Write((sbyte)value);
                    break;
                case "UInt8":
                    write.Write((byte)value);
                    break;
                case "short":
                case "SInt16":
                    write.Write((short)value);
                    break;
                case "UInt16":
                case "unsigned short":
                    write.Write((ushort)value);
                    break;
                case "int":
                case "SInt32":
                    write.Write((int)value);
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    write.Write((uint)value);
                    break;
                case "long long":
                case "SInt64":
                    write.Write((long)value);
                    break;
                case "UInt64":
                case "unsigned long long":
                    write.Write((ulong)value);
                    break;
                case "float":
                    write.Write((float)value);
                    break;
                case "double":
                    write.Write((double)value);
                    break;
                case "bool":
                    write.Write((bool)value);
                    break;
                case "string":
                    write.WriteAlignedString((string)value);
                    i += 3;
                    break;
                case "vector":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        var list = (List<object>)value;
                        var size = list.Count;
                        write.Write(size);
                        var vector = GetMembers(members, level, i);
                        i += vector.Count - 1;
                        vector.RemoveRange(0, 3);
                        for (int j = 0; j < size; j++)
                        {
                            int tmp = 0;
                            WriteValue(list[j], vector, write, ref tmp);
                        }
                        break;
                    }
                case "map":
                    {
                        if ((members[i + 1].m_MetaFlag & 0x4000) != 0)
                            align = true;
                        var dic = (List<KeyValuePair<object, object>>)value;
                        var size = dic.Count;
                        write.Write(size);
                        var map = GetMembers(members, level, i);
                        i += map.Count - 1;
                        map.RemoveRange(0, 4);
                        var first = GetMembers(map, map[0].m_Depth, 0);
                        map.RemoveRange(0, first.Count);
                        var second = map;
                        for (int j = 0; j < size; j++)
                        {
                            int tmp1 = 0;
                            int tmp2 = 0;
                            WriteValue(dic[j].Key, first, write, ref tmp1);
                            WriteValue(dic[j].Value, second, write, ref tmp2);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        var bytes = ((object[])value).Cast<byte>().ToArray();
                        var size = bytes.Length;
                        write.Write(size);
                        write.Write(bytes);
                        i += 2;
                        break;
                    }
                default:
                    {
                        if (i != members.Count && members[i + 1].m_Type == "Array")
                        {
                            goto case "vector";
                        }
                        var @class = GetMembers(members, level, i);
                        @class.RemoveAt(0);
                        i += @class.Count;
                        var obj = (ExpandoObject)value;
                        var objdic = (IDictionary<string, object>)obj;
                        for (int j = 0; j < @class.Count; j++)
                        {
                            var classmember = @class[j];
                            var name = classmember.m_Name;
                            WriteValue(objdic[name], @class, write, ref j);
                        }
                        break;
                    }
            }
            if (align)
                write.AlignStream(4);
        }
    }
}
