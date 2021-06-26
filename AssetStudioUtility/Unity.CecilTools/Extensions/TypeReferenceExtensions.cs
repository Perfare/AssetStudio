// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using Mono.Cecil;

namespace Unity.CecilTools.Extensions
{
    public static class TypeReferenceExtensions
    {
        public static string SafeNamespace(this TypeReference type)
        {
            if (type.IsGenericInstance)
                return ((GenericInstanceType)type).ElementType.SafeNamespace();
            if (type.IsNested)
                return type.DeclaringType.SafeNamespace();
            return type.Namespace;
        }

        public static bool IsAssignableTo(this TypeReference typeRef, string typeName)
        {
            try
            {
                if (typeRef.IsGenericInstance)
                    return ElementType.For(typeRef).IsAssignableTo(typeName);

                if (typeRef.FullName == typeName)
                    return true;

                return typeRef.CheckedResolve().IsSubclassOf(typeName);
            }
            catch (AssemblyResolutionException) // If we can't resolve our typeref or one of its base types,
            {                                   // let's assume it is not assignable to our target type
                return false;
            }
        }

        public static bool IsEnum(this TypeReference type)
        {
            return type.IsValueType && !type.IsPrimitive && type.CheckedResolve().IsEnum;
        }

        public static bool IsStruct(this TypeReference type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum() && !IsSystemDecimal(type);
        }

        private static bool IsSystemDecimal(TypeReference type)
        {
            return type.FullName == "System.Decimal";
        }
    }
}
