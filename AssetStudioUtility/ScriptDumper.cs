using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;

namespace AssetStudio
{
    //TODO to json file
    public sealed class ScriptDumper : IDisposable
    {
        private Dictionary<string, ModuleDef> moduleDic = new Dictionary<string, ModuleDef>();

        public ScriptDumper() { }

        public ScriptDumper(string path)
        {
            var files = Directory.GetFiles(path, "*.dll");
            var moduleContext = new ModuleContext();
            var asmResolver = new AssemblyResolver(moduleContext);
            var resolver = new Resolver(asmResolver);
            moduleContext.AssemblyResolver = asmResolver;
            moduleContext.Resolver = resolver;
            try
            {
                foreach (var file in files)
                {
                    var module = ModuleDefMD.Load(file, moduleContext);
                    asmResolver.AddToCache(module);
                    moduleDic.Add(Path.GetFileName(file), module);
                }
            }
            catch
            {
                // ignored
            }
        }

        public string DumpScript(ObjectReader reader)
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
                        var readed = reader.Position - reader.byteStart;
                        if (readed != reader.byteSize)
                        {
                            Logger.Error($"Error while dump type, read {readed} bytes but expected {reader.byteSize} bytes");
                        }
                    }
                    catch
                    {
                        sb = CreateMonoBehaviourHeader(m_MonoBehaviour);
                        Logger.Error("Error while dump type");
                    }
                }
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            if (moduleDic != null)
            {
                foreach (var pair in moduleDic)
                {
                    pair.Value.Dispose();
                }
                moduleDic.Clear();
                moduleDic = null;
            }
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

        private static void DumpType(TypeSig typeSig, StringBuilder sb, ObjectReader reader, string name, int indent, bool isRoot = false, bool align = true)
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
                if (align)
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
                    var genericType = genericInstSig.GenericType.ToTypeDefOrRef().ResolveTypeDefThrow();
                    var type = genericInstSig.GenericArguments[0].ToTypeDefOrRef().ResolveTypeDefThrow();
                    if (genericInstSig.GenericArguments[0] is ArraySigBase)
                    {
                        return;
                    }
                    if (!type.IsEnum && !IsBaseType(type) && !IsAssignFromUnityObject(type) && !IsEngineType(type) && !type.IsSerializable)
                    {
                        return;
                    }
                    sb.AppendLine($"{new string('\t', indent)}{typeSig.TypeName} {name}");
                    if (genericType.Interfaces.Any(x => x.Interface.FullName == "System.Collections.Generic.ICollection`1<T>")) //System.Collections.Generic.IEnumerable`1<T>
                    {
                        var size = reader.ReadInt32();
                        sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                        for (int i = 0; i < size; i++)
                        {
                            sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                            DumpType(genericInstSig.GenericArguments[0], sb, reader, "data", indent + 2, false, false);
                        }
                        reader.AlignStream();
                    }
                    else
                    {
                        DumpType(genericType.ToTypeSig(), sb, reader, "data", indent + 1);
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
            if (!isRoot && !IsEngineType(typeDef) && !typeDef.IsSerializable)
            {
                return;
            }
            if (typeDef.FullName == "UnityEngine.AnimationCurve")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}<truncated>");
                var animationCurve = new AnimationCurve<float>(reader, reader.ReadSingle);
                return;
            }
            if (typeDef.FullName == "UnityEngine.Bounds")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}<truncated>");
                new AABB(reader);
                return;
            }
            if (typeDef.FullName == "UnityEngine.BoundsInt")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}<truncated>");
                reader.Position += 24;
                return;
            }
            if (typeDef.FullName == "UnityEngine.Color32")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}byte r = {reader.ReadByte()}");
                sb.AppendLine($"{prefix}byte g = {reader.ReadByte()}");
                sb.AppendLine($"{prefix}byte b = {reader.ReadByte()}");
                sb.AppendLine($"{prefix}byte a = {reader.ReadByte()}");
                reader.AlignStream();
                return;
            }
            if (typeDef.FullName == "UnityEngine.Gradient")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}<truncated>");
                if (reader.version[0] == 5 && reader.version[1] < 5)
                    reader.Position += 68;
                else if (reader.version[0] == 5 && reader.version[1] < 6)
                    reader.Position += 72;
                else
                    reader.Position += 168;
                return;
            }
            if (typeDef.FullName == "UnityEngine.GUIStyle") //TODO
            {
                throw new NotSupportedException();
            }
            if (typeDef.FullName == "UnityEngine.LayerMask")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}uint m_Bits = {reader.ReadUInt32()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.PropertyName")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                sb.AppendLine($"{new string('\t', indent + 1)}int id = {reader.ReadInt32()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.Rect")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}float x = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float y = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float width = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float height = {reader.ReadSingle()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.RectInt")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}int x = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int y = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int width = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int height = {reader.ReadInt32()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.RectOffset")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}float left = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float right = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float top = {reader.ReadSingle()}");
                sb.AppendLine($"{prefix}float bottom = {reader.ReadSingle()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.Vector2Int")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}int x = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int y = {reader.ReadInt32()}");
                return;
            }
            if (typeDef.FullName == "UnityEngine.Vector3Int")
            {
                sb.AppendLine($"{new string('\t', indent)}{typeDef.Name} {name}");
                var prefix = new string('\t', indent + 1);
                sb.AppendLine($"{prefix}int x = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int y = {reader.ReadInt32()}");
                sb.AppendLine($"{prefix}int z = {reader.ReadInt32()}");
                return;
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
                    var flag = false;
                    var access = fieldDef.Access & FieldAttributes.FieldAccessMask;
                    if (access != FieldAttributes.Public)
                    {
                        if (fieldDef.CustomAttributes.Any(x => x.TypeFullName == "UnityEngine.SerializeField"))
                        {
                            flag = true;
                        }
                    }
                    else if ((fieldDef.Attributes & FieldAttributes.Static) == 0 && (fieldDef.Attributes & FieldAttributes.InitOnly) == 0 && (fieldDef.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        flag = true;
                    }

                    if (flag)
                    {
                        if (fieldDef.FieldType.IsGenericParameter)
                        {
                            for (var i = 0; i < typeDef.GenericParameters.Count; i++)
                            {
                                var g = typeDef.GenericParameters[i];
                                if (g.FullName == fieldDef.FieldType.FullName)
                                {
                                    var type = ((GenericInstSig)typeSig).GenericArguments[i];
                                    DumpType(type, sb, reader, fieldDef.Name, indent + 1);
                                    break;
                                }
                            }
                        }
                        else if (fieldDef.FieldType is GenericInstSig genericSig && genericSig.GenericArguments.Count == 1 && genericSig.GenericArguments[0].IsGenericParameter)
                        {
                            for (var i = 0; i < typeDef.GenericParameters.Count; i++)
                            {
                                var g = typeDef.GenericParameters[i];
                                if (g.FullName == genericSig.GenericArguments[0].FullName)
                                {
                                    var type = ((GenericInstSig)typeSig).GenericArguments[i];
                                    var fieldTypeDef = fieldDef.FieldType.ToTypeDefOrRef().ResolveTypeDefThrow();
                                    if (fieldTypeDef.Interfaces.Any(x => x.Interface.FullName == "System.Collections.Generic.ICollection`1<T>")) //System.Collections.Generic.IEnumerable`1<T>
                                    {
                                        var size = reader.ReadInt32();
                                        sb.AppendLine($"{new string('\t', indent + 1)}int size = {size}");
                                        for (int j = 0; j < size; j++)
                                        {
                                            sb.AppendLine($"{new string('\t', indent + 2)}[{i}]");
                                            DumpType(type, sb, reader, "data", indent + 2, false, false);
                                        }
                                    }
                                    else
                                    {
                                        DumpType(fieldDef.FieldType, sb, reader, fieldDef.Name, indent + 1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            DumpType(fieldDef.FieldType, sb, reader, fieldDef.Name, indent + 1);
                        }
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
                case "UnityEngine.PropertyName":
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
