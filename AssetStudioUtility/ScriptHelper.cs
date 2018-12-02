using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;

namespace AssetStudio
{
    //TODO unfinished
    public static class ScriptHelper
    {
        public static string GetScriptString(ObjectReader reader, Dictionary<string, ModuleDef> moduleDic)
        {
            var m_MonoBehaviour = new MonoBehaviour(reader);
            var sb = CreateMonoBehaviourHeader(m_MonoBehaviour);
            if (m_MonoBehaviour.m_Script.TryGet(out var m_Script))
            {
                if (!moduleDic.TryGetValue(m_Script.m_AssemblyName, out var module))
                {
                    return sb.ToString();
                }
                var typeDef = module.Assembly.Find(m_Script.m_Namespace != "" ? $"{m_Script.m_Namespace}.{m_Script.m_ClassName}" : m_Script.m_ClassName, false);
                if (typeDef != null)
                {
                    try
                    {
                        DumpType(typeDef.ToTypeSig(), sb, reader, null, -1, true);
                    }
                    catch
                    {
                        sb = CreateMonoBehaviourHeader(m_MonoBehaviour);
                    }
                }
            }
            return sb.ToString();
        }

        private static StringBuilder CreateMonoBehaviourHeader(MonoBehaviour m_MonoBehaviour)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PPtr<GameObject> m_GameObject");
            sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_GameObject.m_FileID}");
            sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_GameObject.m_PathID}");
            sb.AppendLine($"UInt8 m_Enabled = {m_MonoBehaviour.m_Enabled}");
            sb.AppendLine("PPtr<MonoScript> m_Script");
            sb.AppendLine($"\tint m_FileID = {m_MonoBehaviour.m_Script.m_FileID}");
            sb.AppendLine($"\tint64 m_PathID = {m_MonoBehaviour.m_Script.m_PathID}");
            sb.AppendLine($"string m_Name = \"{m_MonoBehaviour.m_Name}\"");
            return sb;
        }

        private static void DumpType(TypeSig typeSig, StringBuilder sb, ObjectReader reader, string name, int indent, bool isRoot = false)
        {
            var typeDef = typeSig.ToTypeDefOrRef().ResolveTypeDefThrow();
            if (typeSig.IsPrimitive)
            {
                object value = null;
                switch (typeSig.TypeName)
                {
                    case "Boolean":
                        value = reader.ReadBoolean();
                        break;
                    case "Byte":
                        value = reader.ReadByte();
                        break;
                    case "SByte":
                        value = reader.ReadSByte();
                        break;
                    case "Int16":
                        value = reader.ReadInt16();
                        break;
                    case "UInt16":
                        value = reader.ReadUInt16();
                        break;
                    case "Int32":
                        value = reader.ReadInt32();
                        break;
                    case "UInt32":
                        value = reader.ReadUInt32();
                        break;
                    case "Int64":
                        value = reader.ReadInt64();
                        break;
                    case "UInt64":
                        value = reader.ReadUInt64();
                        break;
                    case "Single":
                        value = reader.ReadSingle();
                        break;
                    case "Double":
                        value = reader.ReadDouble();
                        break;
                    case "Char":
                        value = reader.ReadChar();
                        break;
                }
                reader.AlignStream();
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = {value}");
                return;
            }
            if (typeSig.FullName == "System.String")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = \"{reader.ReadAlignedString()}\"");
                return;
            }
            if (typeSig.FullName == "System.Object")
            {
                return;
            }
            if (typeDef.IsDelegate)
            {
                return;
            }
            if (typeSig is ArraySigBase)
            {
                if (!typeDef.IsEnum && !IsBaseType(typeDef) && !IsAssignFromUnityObject(typeDef) && !IsEngineType(typeDef) && !typeDef.IsSerializable)
                {
                    return;
                }
                var size = reader.ReadInt32();
                sb.AppendLine($"{new string('\t', indent)}{typeSig.TypeName} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                for (int i = 0; i < size; i++)
                {
                    sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                    DumpType(typeDef.ToTypeSig(), sb, reader, "data", indent + 2);
                }
                return;
            }
            if (!isRoot && typeSig is GenericInstSig genericInstSig)
            {
                if (genericInstSig.GenericArguments.Count == 1)
                {
                    var type = genericInstSig.GenericArguments[0].ToTypeDefOrRef().ResolveTypeDefThrow();
                    if (!type.IsEnum && !IsBaseType(type) && !IsAssignFromUnityObject(type) && !IsEngineType(type) && !type.IsSerializable)
                    {
                        return;
                    }
                    var size = reader.ReadInt32();
                    sb.AppendLine($"{new string('\t', indent)}{typeSig.TypeName} {name}");
                    sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                    for (int i = 0; i < size; i++)
                    {
                        sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                        DumpType(genericInstSig.GenericArguments[0], sb, reader, "data", indent + 2);
                    }
                }
                return;
            }
            if (indent != -1 && IsAssignFromUnityObject(typeDef))
            {
                var pptr = new PPtr<Object>(reader);
                sb.AppendLine($"{new string('\t', indent)}PPtr<{typeDef.Name}> {name} = {{fileID: {pptr.m_FileID}, pathID: {pptr.m_PathID}}}");
                return;
            }
            if (typeDef.IsEnum)
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name} = {reader.ReadUInt32()}");
                return;
            }
            if (indent != -1 && !IsEngineType(typeDef) && !typeDef.IsSerializable)
            {
                return;
            }
            if (typeDef.FullName == "UnityEngine.Rect")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var rect = reader.ReadSingleArray(4);
                return;
            }
            if (typeDef.FullName == "UnityEngine.LayerMask")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var value = reader.ReadInt32();
                return;
            }
            if (typeDef.FullName == "UnityEngine.AnimationCurve")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var animationCurve = new AnimationCurve<float>(reader, reader.ReadSingle);
                return;
            }
            if (typeDef.FullName == "UnityEngine.Gradient")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                if (reader.version[0] == 5 && reader.version[1] < 5)
                    reader.Position += 68;
                else if (reader.version[0] == 5 && reader.version[1] < 6)
                    reader.Position += 72;
                else
                    reader.Position += 168;
                return;
            }
            if (typeDef.FullName == "UnityEngine.RectOffset")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var left = reader.ReadSingle();
                var right = reader.ReadSingle();
                var top = reader.ReadSingle();
                var bottom = reader.ReadSingle();
                return;
            }
            if (typeDef.FullName == "UnityEngine.GUIStyle") //TODO
            {
                throw new NotSupportedException();
            }
            if (typeDef.IsClass || typeDef.IsValueType)
            {
                if (name != null && indent != -1)
                {
                    sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                }
                if (indent == -1 && typeDef.BaseType.FullName != "UnityEngine.Object")
                {
                    DumpType(typeDef.BaseType.ToTypeSig(), sb, reader, null, indent, true);
                }
                if (indent != -1 && typeDef.BaseType.FullName != "System.Object")
                {
                    DumpType(typeDef.BaseType.ToTypeSig(), sb, reader, null, indent, true);
                }
                foreach (var fieldDef in typeDef.Fields)
                {
                    var access = fieldDef.Access & FieldAttributes.FieldAccessMask;
                    if (access != FieldAttributes.Public)
                    {
                        if (fieldDef.CustomAttributes.Any(x => x.TypeFullName.Contains("SerializeField")))
                        {
                            DumpType(fieldDef.FieldType, sb, reader, fieldDef.Name, indent + 1);
                        }
                    }
                    else if ((fieldDef.Attributes & FieldAttributes.Static) == 0 && (fieldDef.Attributes & FieldAttributes.InitOnly) == 0 && (fieldDef.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        DumpType(fieldDef.FieldType, sb, reader, fieldDef.Name, indent + 1);
                    }
                }
            }
        }

        private static bool IsAssignFromUnityObject(TypeDef typeDef)
        {
            if (typeDef.FullName == "UnityEngine.Object")
            {
                return true;
            }
            if (typeDef.BaseType != null)
            {
                if (typeDef.BaseType.FullName == "UnityEngine.Object")
                {
                    return true;
                }
                while (true)
                {
                    typeDef = typeDef.BaseType.ResolveTypeDefThrow();
                    if (typeDef.BaseType == null)
                    {
                        break;
                    }
                    if (typeDef.BaseType.FullName == "UnityEngine.Object")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsBaseType(IFullName typeDef)
        {
            switch (typeDef.FullName)
            {
                case "System.Boolean":
                case "System.Byte":
                case "System.SByte":
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                case "System.Int64":
                case "System.UInt64":
                case "System.Single":
                case "System.Double":
                case "System.String":
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsEngineType(IFullName typeDef)
        {
            switch (typeDef.FullName)
            {
                case "UnityEngine.AnimationCurve":
                case "UnityEngine.Bounds":
                case "UnityEngine.BoundsInt":
                case "UnityEngine.Color":
                case "UnityEngine.Color32":
                case "UnityEngine.Gradient":
                case "UnityEngine.GUIStyle":
                case "UnityEngine.LayerMask":
                case "UnityEngine.Matrix4x4":
                case "UnityEngine.Quaternion":
                case "UnityEngine.Rect":
                case "UnityEngine.RectInt":
                case "UnityEngine.RectOffset":
                case "UnityEngine.Vector2":
                case "UnityEngine.Vector2Int":
                case "UnityEngine.Vector3":
                case "UnityEngine.Vector3Int":
                case "UnityEngine.Vector4":
                    return true;
                default:
                    return false;
            }
        }
    }
}
