using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.CecilTools;
using Unity.SerializationLogic;

namespace AssetStudio
{
    public class TypeDefinitionConverter
    {
        private readonly TypeDefinition TypeDef;
        private readonly TypeResolver TypeResolver;
        private readonly SerializedTypeHelper Helper;
        private readonly int Indent;

        public TypeDefinitionConverter(TypeDefinition typeDef, SerializedTypeHelper helper, int indent)
        {
            TypeDef = typeDef;
            TypeResolver = new TypeResolver(null);
            Helper = helper;
            Indent = indent;
        }

        public List<TypeTreeNode> ConvertToTypeTreeNodes()
        {
            var nodes = new List<TypeTreeNode>();

            var baseTypes = new Stack<TypeReference>();
            var lastBaseType = TypeDef.BaseType;
            while (!UnitySerializationLogic.IsNonSerialized(lastBaseType))
            {
                var genericInstanceType = lastBaseType as GenericInstanceType;
                if (genericInstanceType != null)
                {
                    TypeResolver.Add(genericInstanceType);
                }
                baseTypes.Push(lastBaseType);
                lastBaseType = lastBaseType.Resolve().BaseType;
            }
            while (baseTypes.Count > 0)
            {
                var typeReference = baseTypes.Pop();
                var typeDefinition = typeReference.Resolve();
                foreach (var fieldDefinition in typeDefinition.Fields.Where(WillUnitySerialize))
                {
                    if (!IsHiddenByParentClass(baseTypes, fieldDefinition, TypeDef))
                    {
                        nodes.AddRange(ProcessingFieldRef(ResolveGenericFieldReference(fieldDefinition)));
                    }
                }

                var genericInstanceType = typeReference as GenericInstanceType;
                if (genericInstanceType != null)
                {
                    TypeResolver.Remove(genericInstanceType);
                }
            }
            foreach (var field in FilteredFields())
            {
                nodes.AddRange(ProcessingFieldRef(field));
            }

            return nodes;
        }

        private bool WillUnitySerialize(FieldDefinition fieldDefinition)
        {
            try
            {
                var resolvedFieldType = TypeResolver.Resolve(fieldDefinition.FieldType);
                if (UnitySerializationLogic.ShouldNotTryToResolve(resolvedFieldType))
                {
                    return false;
                }
                if (!UnityEngineTypePredicates.IsUnityEngineObject(resolvedFieldType))
                {
                    if (resolvedFieldType.FullName == fieldDefinition.DeclaringType.FullName)
                    {
                        return false;
                    }
                }
                return UnitySerializationLogic.WillUnitySerialize(fieldDefinition, TypeResolver);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception while processing {0} {1}, error {2}", fieldDefinition.FieldType.FullName, fieldDefinition.FullName, ex.Message));
            }
        }

        private static bool IsHiddenByParentClass(IEnumerable<TypeReference> parentTypes, FieldDefinition fieldDefinition, TypeDefinition processingType)
        {
            return processingType.Fields.Any(f => f.Name == fieldDefinition.Name) || parentTypes.Any(t => t.Resolve().Fields.Any(f => f.Name == fieldDefinition.Name));
        }

        private IEnumerable<FieldDefinition> FilteredFields()
        {
            return TypeDef.Fields.Where(WillUnitySerialize).Where(f =>
                UnitySerializationLogic.IsSupportedCollection(f.FieldType) ||
                !f.FieldType.IsGenericInstance ||
                UnitySerializationLogic.ShouldImplementIDeserializable(f.FieldType.Resolve()));
        }

        private FieldReference ResolveGenericFieldReference(FieldReference fieldRef)
        {
            var field = new FieldReference(fieldRef.Name, fieldRef.FieldType, ResolveDeclaringType(fieldRef.DeclaringType));
            return TypeDef.Module.ImportReference(field);
        }

        private TypeReference ResolveDeclaringType(TypeReference declaringType)
        {
            var typeDefinition = declaringType.Resolve();
            if (typeDefinition == null || !typeDefinition.HasGenericParameters)
            {
                return typeDefinition;
            }
            var genericInstanceType = new GenericInstanceType(typeDefinition);
            foreach (var genericParameter in typeDefinition.GenericParameters)
            {
                genericInstanceType.GenericArguments.Add(genericParameter);
            }
            return TypeResolver.Resolve(genericInstanceType);
        }

        private List<TypeTreeNode> ProcessingFieldRef(FieldReference fieldDef)
        {
            var typeRef = TypeResolver.Resolve(fieldDef.FieldType);
            return TypeRefToTypeTreeNodes(typeRef, fieldDef.Name, Indent, false);
        }

        private static bool IsStruct(TypeReference typeRef)
        {
            return typeRef.IsValueType && !IsEnum(typeRef) && !typeRef.IsPrimitive;
        }

        private static bool IsEnum(TypeReference typeRef)
        {
            return !typeRef.IsArray && typeRef.Resolve().IsEnum;
        }

        private static bool RequiresAlignment(TypeReference typeRef)
        {
            switch (typeRef.MetadataType)
            {
                case MetadataType.Boolean:
                case MetadataType.Char:
                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Int16:
                case MetadataType.UInt16:
                    return true;
                default:
                    return UnitySerializationLogic.IsSupportedCollection(typeRef) && RequiresAlignment(CecilUtils.ElementTypeOfCollection(typeRef));
            }
        }

        private static bool IsSystemString(TypeReference typeRef)
        {
            return typeRef.FullName == "System.String";
        }

        private List<TypeTreeNode> TypeRefToTypeTreeNodes(TypeReference typeRef, string name, int indent, bool isElement)
        {
            var align = false;

            if (!IsStruct(TypeDef) || !UnityEngineTypePredicates.IsUnityEngineValueType(TypeDef))
            {
                if (IsStruct(typeRef) || RequiresAlignment(typeRef))
                {
                    align = true;
                }
            }

            var nodes = new List<TypeTreeNode>();
            if (typeRef.IsPrimitive)
            {
                var primitiveName = typeRef.Name;
                switch (primitiveName)
                {
                    case "Boolean":
                        primitiveName = "bool";
                        break;
                    case "Byte":
                        primitiveName = "UInt8";
                        break;
                    case "SByte":
                        primitiveName = "SInt8";
                        break;
                    case "Int16":
                        primitiveName = "SInt16";
                        break;
                    case "UInt16":
                        primitiveName = "UInt16";
                        break;
                    case "Int32":
                        primitiveName = "SInt32";
                        break;
                    case "UInt32":
                        primitiveName = "UInt32";
                        break;
                    case "Int64":
                        primitiveName = "SInt64";
                        break;
                    case "UInt64":
                        primitiveName = "UInt64";
                        break;
                    case "Char":
                        primitiveName = "char";
                        break;
                    case "Double":
                        primitiveName = "double";
                        break;
                    case "Single":
                        primitiveName = "float";
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (isElement)
                {
                    align = false;
                }
                nodes.Add(new TypeTreeNode(primitiveName, name, indent, align));
            }
            else if (IsSystemString(typeRef))
            {
                Helper.AddString(nodes, name, indent);
            }
            else if (IsEnum(typeRef))
            {
                nodes.Add(new TypeTreeNode("SInt32", name, indent, align));
            }
            else if (CecilUtils.IsGenericList(typeRef))
            {
                var elementRef = CecilUtils.ElementTypeOfCollection(typeRef);
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                Helper.AddArray(nodes, indent + 1);
                nodes.AddRange(TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true));
            }
            else if (typeRef.IsArray)
            {
                var elementRef = typeRef.GetElementType();
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                Helper.AddArray(nodes, indent + 1);
                nodes.AddRange(TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true));
            }
            else if (UnityEngineTypePredicates.IsUnityEngineObject(typeRef))
            {
                Helper.AddPPtr(nodes, typeRef.Name, name, indent);
            }
            else if (UnityEngineTypePredicates.IsSerializableUnityClass(typeRef) || UnityEngineTypePredicates.IsSerializableUnityStruct(typeRef))
            {
                switch (typeRef.FullName)
                {
                    case "UnityEngine.AnimationCurve":
                        Helper.AddAnimationCurve(nodes, name, indent);
                        break;
                    case "UnityEngine.Gradient":
                        Helper.AddGradient(nodes, name, indent);
                        break;
                    case "UnityEngine.GUIStyle":
                        Helper.AddGUIStyle(nodes, name, indent);
                        break;
                    case "UnityEngine.RectOffset":
                        Helper.AddRectOffset(nodes, name, indent);
                        break;
                    case "UnityEngine.Color32":
                        Helper.AddColor32(nodes, name, indent);
                        break;
                    case "UnityEngine.Matrix4x4":
                        Helper.AddMatrix4x4(nodes, name, indent);
                        break;
                    case "UnityEngine.Rendering.SphericalHarmonicsL2":
                        Helper.AddSphericalHarmonicsL2(nodes, name, indent);
                        break;
                    case "UnityEngine.PropertyName":
                        Helper.AddPropertyName(nodes, name, indent);
                        break;
                }
            }
            else
            {
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                var typeDef = typeRef.Resolve();
                var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, Helper, indent + 1);
                nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
            }

            return nodes;
        }
    }
}
