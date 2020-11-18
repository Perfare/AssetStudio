using System.Collections.Generic;

namespace AssetStudio
{
    public static class MonoBehaviourConverter
    {
        public static List<TypeTreeNode> ConvertToTypeTreeNodes(this MonoBehaviour m_MonoBehaviour, AssemblyLoader assemblyLoader)
        {
            var nodes = new List<TypeTreeNode>();
            var helper = new SerializedTypeHelper(m_MonoBehaviour.version);
            helper.AddMonoBehaviour(nodes, 0);
            if (m_MonoBehaviour.m_Script.TryGet(out var m_Script))
            {
                var typeDef = assemblyLoader.GetTypeDefinition(m_Script.m_AssemblyName, string.IsNullOrEmpty(m_Script.m_Namespace) ? m_Script.m_ClassName : $"{m_Script.m_Namespace}.{m_Script.m_ClassName}");
                if (typeDef != null)
                {
                    var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, helper, 1);
                    nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
                }
            }
            return nodes;
        }
    }
}
