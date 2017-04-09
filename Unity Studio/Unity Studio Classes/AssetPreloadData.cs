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

        private static void Read(StringBuilder sb, List<ClassMember> members, EndianStream a_Stream)
        {
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                var level = member.Level;
                var varTypeStr = member.Type;
                var varNameStr = member.Name;
                object value = null;
                var align = (member.Flag & 0x4000) != 0;
                var append = true;
                if (varTypeStr == "SInt8")//sbyte
                {
                    value = a_Stream.ReadSByte();
                }
                else if (varTypeStr == "UInt8")//byte
                {
                    value = a_Stream.ReadByte();
                }
                else if (varTypeStr == "short" || varTypeStr == "SInt16")//Int16
                {
                    value = a_Stream.ReadInt16();
                }
                else if (varTypeStr == "UInt16" || varTypeStr == "unsigned short")//UInt16
                {
                    value = a_Stream.ReadUInt16();
                }
                else if (varTypeStr == "int" || varTypeStr == "SInt32")//Int32
                {
                    value = a_Stream.ReadInt32();
                }
                else if (varTypeStr == "UInt32" || varTypeStr == "unsigned int")//UInt32
                {
                    value = a_Stream.ReadUInt32();
                }
                else if (varTypeStr == "long long" || varTypeStr == "SInt64")//Int64
                {
                    value = a_Stream.ReadInt64();
                }
                else if (varTypeStr == "UInt64" || varTypeStr == "unsigned long long")//UInt64
                {
                    value = a_Stream.ReadUInt64();
                }
                else if (varTypeStr == "float")//float
                {
                    value = a_Stream.ReadSingle();
                }
                else if (varTypeStr == "double")//double
                {
                    value = a_Stream.ReadDouble();
                }
                else if (varTypeStr == "bool")//bool
                {
                    value = a_Stream.ReadBoolean();
                }
                else if (varTypeStr == "string")//string
                {
                    append = false;
                    var str = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, str);
                    i += 3;//skip
                }
                else if (varTypeStr == "Array")//Array
                {
                    append = false;
                    if ((members[i - 1].Flag & 0x4000) != 0)
                        align = true;
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
                    append = false;
                    align = false;
                    sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                }
                if (append)
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                if (align)
                    a_Stream.AlignStream(4);
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
                member2.Add(member);
            }
            return member2;
        }
    }
}
