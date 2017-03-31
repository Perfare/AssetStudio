using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class AssetPreloadData : ListViewItem
    {
        public long m_PathID;
        public uint Offset;
        public int Size;
        public int Type1;
        public ushort Type2;

        public string TypeString;
        public string InfoText;
        public string extension;

        public AssetsFile sourceFile;
        public string uniqueID;

        public string ViewStruct()
        {
            var a_Stream = sourceFile.a_Stream;
            a_Stream.Position = Offset;
            ClassStruct classStructure;
            if (sourceFile.ClassStructures.TryGetValue(Type1, out classStructure))
            {
                var member = classStructure.members;
                var sb = new StringBuilder();
                Read(sb, member, a_Stream);
                return sb.ToString();
            }
            return null;
        }

        //TODO 1,2长度类型对齐问题
        private static void Read(StringBuilder sb, List<ClassMember> members, EndianStream a_Stream)
        {
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var level = member.Level;
                var varTypeStr = member.Type;
                var varNameStr = member.Name;
                if (varTypeStr == "SInt8")//sbyte
                {
                    var value = a_Stream.ReadSByte();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt8")//byte
                {
                    var value = a_Stream.ReadByte();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "short" || varTypeStr == "SInt16")//Int16
                {
                    var value = a_Stream.ReadInt16();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt16" || varTypeStr == "unsigned short")//UInt16
                {
                    var value = a_Stream.ReadUInt16();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "int" || varTypeStr == "SInt32")//Int32
                {
                    var value = a_Stream.ReadInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt32" || varTypeStr == "unsigned int")//UInt32
                {
                    var value = a_Stream.ReadUInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "long long" || varTypeStr == "SInt64")//Int64
                {
                    var value = a_Stream.ReadInt64();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt64" || varTypeStr == "unsigned long long")//UInt64
                {
                    var value = a_Stream.ReadUInt64();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "float")//float
                {
                    var value = a_Stream.ReadSingle();
                    sb.AppendFormat("{0}{1} {2} = {3:f}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "double")//double
                {
                    var value = a_Stream.ReadDouble();
                    sb.AppendFormat("{0}{1} {2} = {3:f4}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "bool")//bool
                {
                    var value = a_Stream.ReadBoolean();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "string")//string
                {
                    var value = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    i += 3;//skip
                }
                else if (varTypeStr == "Array")//Array
                {
                    sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                    var size = a_Stream.ReadInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), "int", "size", size);
                    var array = ReadArray(members, level, i);
                    for (int j = 0; j < size; j++)
                    {
                        sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 1)), j);
                        Read(sb, array, a_Stream);
                    }
                    i += array.Count + 1;//skip
                }
                else
                {
                    sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                }
            }
        }

        private static List<ClassMember> ReadArray(List<ClassMember> members, int level, int index)
        {
            var member2 = new List<ClassMember>();
            for (int i = index + 2; i < members.Count; i++)//skip int size
            {
                var member = members[i];
                var level2 = member.Level;
                if (level2 <= level)
                {
                    return member2;
                }
                else
                {
                    member2.Add(member);
                }
            }
            return member2;
        }
    }
}
