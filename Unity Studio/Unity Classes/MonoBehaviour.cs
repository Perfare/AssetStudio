using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class MonoBehaviour
    {
        public string serializedText;

        public MonoBehaviour(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            var m_GameObject = sourceFile.ReadPPtr();
            var m_Enabled = a_Stream.ReadByte();
            a_Stream.AlignStream(4);
            var m_Script = sourceFile.ReadPPtr();
            var m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            if (readSwitch)
            {
                preloadData.extension = ".txt";
                a_Stream.Position = preloadData.Offset;
                ClassStrStruct classStructure;
                if (sourceFile.ClassStructures.TryGetValue(preloadData.Type1, out classStructure))
                {
                    var member = classStructure.members;
                    var strs = member.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var sb = new StringBuilder();
                    Read(sb, strs, a_Stream);
                    serializedText = sb.ToString();
                }
                else
                {
                    var str = "PPtr<GameObject> m_GameObject\r\n";
                    str += "\tint m_FileID = " + m_GameObject.m_FileID + "\r\n";
                    str += "\tint64 m_PathID = " + m_GameObject.m_PathID + "\r\n";
                    str += "UInt8 m_Enabled = " + m_Enabled + "\r\n";
                    str += "PPtr<MonoScript> m_Script\r\n";
                    str += "\tint m_FileID = " + m_Script.m_FileID + "\r\n";
                    str += "\tint64 m_PathID = " + m_Script.m_PathID + "\r\n";
                    str += "string m_Name = \"" + m_Name + "\"\r\n";
                    serializedText = str;
                }
            }
            else
            {
                if (m_Name != "")
                {
                    preloadData.Text = m_Name;
                }
                else
                {
                    preloadData.Text = preloadData.TypeString + " #" + preloadData.uniqueID;
                }
                preloadData.SubItems.AddRange(new string[] { preloadData.TypeString, preloadData.Size.ToString() });
            }
        }

        private void Read(StringBuilder sb, string[] strs, EndianStream a_Stream)
        {
            for (int i = 0; i < strs.Length; i++)
            {
                var strs2 = strs[i].Split(' ');
                var str = strs2[0].Split('\t');
                var level = str.Length - 1;
                var varTypeStr = str.Last();
                var varNameStr = strs2[1];
                if (varTypeStr == "int")
                {
                    var value = a_Stream.ReadInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt")
                {
                    var value = a_Stream.ReadUInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "int64")
                {
                    var value = a_Stream.ReadInt64();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt64")
                {
                    var value = a_Stream.ReadUInt64();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt16")
                {
                    var value = a_Stream.ReadUInt16();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "SInt16")
                {
                    var value = a_Stream.ReadInt16();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "UInt8")
                {
                    var value = a_Stream.ReadByte();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "float")
                {
                    var value = a_Stream.ReadSingle();
                    sb.AppendFormat("{0}{1} {2} = {3:f}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "bool")
                {
                    var value = a_Stream.ReadBoolean();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    a_Stream.AlignStream(4);
                }
                else if (varTypeStr == "string")
                {
                    var value = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                    sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", (new string('\t', level)), varTypeStr, varNameStr, value);
                    i += 3;//skip
                }
                else if (varTypeStr == "Array")
                {
                    sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                    var size = a_Stream.ReadInt32();
                    sb.AppendFormat("{0}{1} {2} = {3}\r\n", (new string('\t', level)), "int", "size", size);
                    var array = ReadArray(strs, level, i);
                    for (int j = 0; j < size; j++)
                    {
                        sb.AppendFormat("{0}[{1}]\r\n", (new string('\t', level + 1)), j);
                        Read(sb, array, a_Stream);
                    }
                    i += array.Length + 1;//skip
                }
                else
                {
                    sb.AppendFormat("{0}{1} {2}\r\n", (new string('\t', level)), varTypeStr, varNameStr);
                }
            }
        }

        private string[] ReadArray(string[] strs, int level, int index)
        {
            List<string> strs3 = new List<string>();
            for (int i = index + 2; i < strs.Length; i++)//skip int size
            {
                var strs2 = strs[i].Split(' ');
                var str = strs2[0].Split('\t');
                var level2 = str.Length - 1;
                if (level2 <= level)
                {
                    return strs3.ToArray();
                }
                else
                {
                    strs3.Add(strs[i]);
                }
            }
            return strs3.ToArray();
        }
    }
}
