// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System.Collections.Generic;
using Unity.CecilTools.Extensions;
using Mono.Cecil;

namespace Unity.SerializationLogic
{
    public class UnityEngineTypePredicates
    {
        private static readonly HashSet<string> TypesThatShouldHaveHadSerializableAttribute = new HashSet<string>
        {
            "Vector3",
            "Vector2",
            "Vector4",
            "Rect",
            "RectInt",
            "Quaternion",
            "Matrix4x4",
            "Color",
            "Color32",
            "LayerMask",
            "Bounds",
            "BoundsInt",
            "Vector3Int",
            "Vector2Int",
        };

        private const string Gradient = "UnityEngine.Gradient";
        private const string GUIStyle = "UnityEngine.GUIStyle";
        private const string RectOffset = "UnityEngine.RectOffset";
        protected const string UnityEngineObject = "UnityEngine.Object";
        public const string MonoBehaviour = "UnityEngine.MonoBehaviour";
        public const string ScriptableObject = "UnityEngine.ScriptableObject";
        protected const string Matrix4x4 = "UnityEngine.Matrix4x4";
        protected const string Color32 = "UnityEngine.Color32";
        private const string SerializeFieldAttribute = "UnityEngine.SerializeField";
        private const string SerializeReferenceAttribute = "UnityEngine.SerializeReference";

        private static string[] serializableClasses = new[]
        {
            "UnityEngine.AnimationCurve",
            "UnityEngine.Gradient",
            "UnityEngine.GUIStyle",
            "UnityEngine.RectOffset"
        };

        private static string[] serializableStructs = new[]
        {
            // NOTE: assumes all types here are NOT interfaces
            "UnityEngine.Color32",
            "UnityEngine.Matrix4x4",
            "UnityEngine.Rendering.SphericalHarmonicsL2",
            "UnityEngine.PropertyName",
        };

        public static bool IsMonoBehaviour(TypeReference type)
        {
            return IsMonoBehaviour(type.CheckedResolve());
        }

        private static bool IsMonoBehaviour(TypeDefinition typeDefinition)
        {
            return typeDefinition.IsSubclassOf(MonoBehaviour);
        }

        public static bool IsScriptableObject(TypeReference type)
        {
            return IsScriptableObject(type.CheckedResolve());
        }

        private static bool IsScriptableObject(TypeDefinition temp)
        {
            return temp.IsSubclassOf(ScriptableObject);
        }

        public static bool IsColor32(TypeReference type)
        {
            return type.IsAssignableTo(Color32);
        }

        //Do NOT remove these, cil2as still depends on these in 4.x
        public static bool IsMatrix4x4(TypeReference type)
        {
            return type.IsAssignableTo(Matrix4x4);
        }

        public static bool IsGradient(TypeReference type)
        {
            return type.IsAssignableTo(Gradient);
        }

        public static bool IsGUIStyle(TypeReference type)
        {
            return type.IsAssignableTo(GUIStyle);
        }

        public static bool IsRectOffset(TypeReference type)
        {
            return type.IsAssignableTo(RectOffset);
        }

        public static bool IsSerializableUnityClass(TypeReference type)
        {
            foreach (var unityClasses in serializableClasses)
            {
                if (type.IsAssignableTo(unityClasses))
                    return true;
            }
            return false;
        }

        public static bool IsSerializableUnityStruct(TypeReference type)
        {
            foreach (var unityStruct in serializableStructs)
            {
                // NOTE: structs cannot inherit from structs, and can only inherit from interfaces
                //       since we know all types in serializableStructs are not interfaces,
                //       we can just do a direct comparison.
                if (type.FullName == unityStruct)
                    return true;
            }

            if (type.FullName.IndexOf("UnityEngine.LazyLoadReference`1") == 0)
                return true;

            return false;
        }

        public static bool IsUnityEngineObject(TypeReference type)
        {
            //todo: somehow solve this elegantly. CheckedResolve() drops the [] of a type.
            if (type.IsArray)
                return false;

            if (type.FullName == UnityEngineObject)
                return true;

            var typeDefinition = type.Resolve();
            if (typeDefinition == null)
                return false;

            return typeDefinition.IsSubclassOf(UnityEngineObject);
        }

        public static bool ShouldHaveHadSerializableAttribute(TypeReference type)
        {
            return IsUnityEngineValueType(type);
        }

        public static bool IsUnityEngineValueType(TypeReference type)
        {
            return type.SafeNamespace() == "UnityEngine" && TypesThatShouldHaveHadSerializableAttribute.Contains(type.Name);
        }

        public static bool IsSerializeFieldAttribute(TypeReference attributeType)
        {
            return attributeType.FullName == SerializeFieldAttribute;
        }

        public static bool IsSerializeReferenceAttribute(TypeReference attributeType)
        {
            return attributeType.FullName == SerializeReferenceAttribute;
        }
    }
}
