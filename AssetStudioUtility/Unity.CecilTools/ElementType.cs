// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using Mono.Cecil;

namespace Unity.CecilTools
{
    static public class ElementType
    {
        public static TypeReference For(TypeReference byRefType)
        {
            var refType = byRefType as TypeSpecification;
            if (refType != null)
                return refType.ElementType;

            throw new ArgumentException(string.Format("TypeReference isn't a TypeSpecification {0} ", byRefType));
        }
    }
}
