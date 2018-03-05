using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Unity_Studio
{
    public class ClassMember
    {
        public int Level;
        public string Type;
        public string Name;
        public int Size;
        public int Flag;

        //use for read
        public bool alignBefore;
    }

    public class ClassStruct : ListViewItem
    {
        public int ID;
        public List<ClassMember> members;

        public string membersstr
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var i in members)
                {
                    sb.AppendFormat("{0}{1} {2} {3} {4}\r\n", new string('\t', i.Level), i.Type, i.Name, i.Size, (i.Flag & 0x4000) != 0);
                }
                return sb.ToString();
            }
        }
    }

    public static class ClassStructHelper
    {
        public static string ViewStruct(this AssetPreloadData asset)
        {
            var reader = asset.Reader;
            if (asset.sourceFile.ClassStructures.TryGetValue(asset.Type1, out var classStructure))
            {
                var sb = new StringBuilder();
                ReadClassStruct(sb, classStructure.members, reader);
                return sb.ToString();
            }
            return null;
        }

        public static void ReadClassStruct(StringBuilder sb, List<ClassMember> members, EndianBinaryReader reader)
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
                if (member.alignBefore)
                    reader.AlignStream(4);
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
                        var str = reader.ReadAlignedString(reader.ReadInt32());
                        sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, str);
                        i += 3;//skip
                        break;
                    case "Array":
                        {
                            append = false;
                            if ((members[i - 1].Flag & 0x4000) != 0)
                                align = true;
                            sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                            var size = reader.ReadInt32();
                            sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), "int", "size", size);
                            var array = ReadArray(members, level, i);
                            for (int j = 0; j < size; j++)
                            {
                                sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 1)), j);
                                ReadClassStruct(sb, array, reader);
                            }
                            i += array.Count + 1;//skip
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
                        append = false;
                        if (align)
                        {
                            align = false;
                            SetAlignBefore(members, level, i + 1);
                        }
                        sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                        break;
                }
                if (append)
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                if (align)
                    reader.AlignStream(4);
            }
        }

        public static List<ClassMember> ReadArray(List<ClassMember> members, int level, int index)
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

        public static void SetAlignBefore(List<ClassMember> members, int level, int index)
        {
            for (int i = index; i < members.Count; i++)
            {
                var member = members[i];
                var level2 = member.Level;
                if (level2 <= level)
                {
                    member.alignBefore = true;
                    return;
                }
            }
        }
    }
}
