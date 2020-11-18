using System.Collections.Generic;

namespace AssetStudio
{
    public class SerializedTypeHelper
    {
        private readonly int[] version;

        public SerializedTypeHelper(int[] version)
        {
            this.version = version;
        }

        public void AddMonoBehaviour(List<TypeTreeNode> nodes, int indent)
        {
            nodes.Add(new TypeTreeNode("MonoBehaviour", "Base", indent, false));
            AddPPtr(nodes, "GameObject", "m_GameObject", indent + 1);
            nodes.Add(new TypeTreeNode("UInt8", "m_Enabled", indent + 1, true));
            AddPPtr(nodes, "MonoScript", "m_Script", indent + 1);
            AddString(nodes, "m_Name", indent + 1);
        }

        public void AddPPtr(List<TypeTreeNode> nodes, string type, string name, int indent)
        {
            nodes.Add(new TypeTreeNode($"PPtr<{type}>", name, indent, false));
            nodes.Add(new TypeTreeNode("int", "m_FileID", indent + 1, false));
            if (version[0] >= 5) //5.0 and up
            {
                nodes.Add(new TypeTreeNode("SInt64", "m_PathID", indent + 1, false));
            }
            else
            {
                nodes.Add(new TypeTreeNode("int", "m_PathID", indent + 1, false));
            }
        }

        public void AddString(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("string", name, indent, false));
            nodes.Add(new TypeTreeNode("Array", "Array", indent + 1, true));
            nodes.Add(new TypeTreeNode("int", "size", indent + 2, false));
            nodes.Add(new TypeTreeNode("char", "data", indent + 2, false));
        }

        public void AddArray(List<TypeTreeNode> nodes, int indent)
        {
            nodes.Add(new TypeTreeNode("Array", "Array", indent, false));
            nodes.Add(new TypeTreeNode("int", "size", indent + 1, false));
        }

        public void AddAnimationCurve(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("AnimationCurve", name, indent, false));
            nodes.Add(new TypeTreeNode("vector", "m_Curve", indent + 1, false));
            AddArray(nodes, indent + 2); //TODO 2017 and up Array align but no effect 
            nodes.Add(new TypeTreeNode("Keyframe", "data", indent + 3, false));
            nodes.Add(new TypeTreeNode("float", "time", indent + 4, false));
            nodes.Add(new TypeTreeNode("float", "value", indent + 4, false));
            nodes.Add(new TypeTreeNode("float", "inSlope", indent + 4, false));
            nodes.Add(new TypeTreeNode("float", "outSlope", indent + 4, false));
            if (version[0] >= 2018) //2018 and up
            {
                nodes.Add(new TypeTreeNode("int", "weightedMode", indent + 4, false));
                nodes.Add(new TypeTreeNode("float", "inWeight", indent + 4, false));
                nodes.Add(new TypeTreeNode("float", "outWeight", indent + 4, false));
            }
            nodes.Add(new TypeTreeNode("int", "m_PreInfinity", indent + 1, false));
            nodes.Add(new TypeTreeNode("int", "m_PostInfinity", indent + 1, false));
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3)) //5.3 and up
            {
                nodes.Add(new TypeTreeNode("int", "m_RotationOrder", indent + 1, false));
            }
        }

        public void AddGradient(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("Gradient", name, indent, false));
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                AddColorRGBA(nodes, "key0", indent + 1);
                AddColorRGBA(nodes, "key1", indent + 1);
                AddColorRGBA(nodes, "key2", indent + 1);
                AddColorRGBA(nodes, "key3", indent + 1);
                AddColorRGBA(nodes, "key4", indent + 1);
                AddColorRGBA(nodes, "key5", indent + 1);
                AddColorRGBA(nodes, "key6", indent + 1);
                AddColorRGBA(nodes, "key7", indent + 1);
            }
            else
            {
                AddColor32(nodes, "key0", indent + 1);
                AddColor32(nodes, "key1", indent + 1);
                AddColor32(nodes, "key2", indent + 1);
                AddColor32(nodes, "key3", indent + 1);
                AddColor32(nodes, "key4", indent + 1);
                AddColor32(nodes, "key5", indent + 1);
                AddColor32(nodes, "key6", indent + 1);
                AddColor32(nodes, "key7", indent + 1);
            }
            nodes.Add(new TypeTreeNode("UInt16", "ctime0", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime1", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime2", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime3", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime4", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime5", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime6", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "ctime7", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime0", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime1", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime2", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime3", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime4", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime5", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime6", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt16", "atime7", indent + 1, false));
            if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) //5.5 and up
            {
                nodes.Add(new TypeTreeNode("int", "m_Mode", indent + 1, false));
            }
            nodes.Add(new TypeTreeNode("UInt8", "m_NumColorKeys", indent + 1, false));
            nodes.Add(new TypeTreeNode("UInt8", "m_NumAlphaKeys", indent + 1, true));
        }

        public void AddGUIStyle(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("GUIStyle", name, indent, false));
            AddString(nodes, "m_Name", indent + 1);
            AddGUIStyleState(nodes, "m_Normal", indent + 1);
            AddGUIStyleState(nodes, "m_Hover", indent + 1);
            AddGUIStyleState(nodes, "m_Active", indent + 1);
            AddGUIStyleState(nodes, "m_Focused", indent + 1);
            AddGUIStyleState(nodes, "m_OnNormal", indent + 1);
            AddGUIStyleState(nodes, "m_OnHover", indent + 1);
            AddGUIStyleState(nodes, "m_OnActive", indent + 1);
            AddGUIStyleState(nodes, "m_OnFocused", indent + 1);
            AddRectOffset(nodes, "m_Border", indent + 1);
            if (version[0] >= 4) //4 and up
            {
                AddRectOffset(nodes, "m_Margin", indent + 1);
                AddRectOffset(nodes, "m_Padding", indent + 1);
            }
            else
            {
                AddRectOffset(nodes, "m_Padding", indent + 1);
                AddRectOffset(nodes, "m_Margin", indent + 1);
            }
            AddRectOffset(nodes, "m_Overflow", indent + 1);
            AddPPtr(nodes, "Font", "m_Font", indent + 1);
            if (version[0] >= 4) //4 and up
            {
                nodes.Add(new TypeTreeNode("int", "m_FontSize", indent + 1, false));
                nodes.Add(new TypeTreeNode("int", "m_FontStyle", indent + 1, false));
                nodes.Add(new TypeTreeNode("int", "m_Alignment", indent + 1, false));
                nodes.Add(new TypeTreeNode("bool", "m_WordWrap", indent + 1, false));
                nodes.Add(new TypeTreeNode("bool", "m_RichText", indent + 1, true));
                nodes.Add(new TypeTreeNode("int", "m_TextClipping", indent + 1, false));
                nodes.Add(new TypeTreeNode("int", "m_ImagePosition", indent + 1, false));
                AddVector2f(nodes, "m_ContentOffset", indent + 1);
                nodes.Add(new TypeTreeNode("float", "m_FixedWidth", indent + 1, false));
                nodes.Add(new TypeTreeNode("float", "m_FixedHeight", indent + 1, false));
                nodes.Add(new TypeTreeNode("bool", "m_StretchWidth", indent + 1, false));
                nodes.Add(new TypeTreeNode("bool", "m_StretchHeight", indent + 1, true));
            }
            else
            {
                nodes.Add(new TypeTreeNode("int", "m_ImagePosition", indent + 1, false));
                nodes.Add(new TypeTreeNode("int", "m_Alignment", indent + 1, false));
                nodes.Add(new TypeTreeNode("bool", "m_WordWrap", indent + 1, true));
                nodes.Add(new TypeTreeNode("int", "m_TextClipping", indent + 1, false));
                AddVector2f(nodes, "m_ContentOffset", indent + 1);
                AddVector2f(nodes, "m_ClipOffset", indent + 1);
                nodes.Add(new TypeTreeNode("float", "m_FixedWidth", indent + 1, false));
                nodes.Add(new TypeTreeNode("float", "m_FixedHeight", indent + 1, false));
                if (version[0] >= 3) //3 and up
                {
                    nodes.Add(new TypeTreeNode("int", "m_FontSize", indent + 1, false));
                    nodes.Add(new TypeTreeNode("int", "m_FontStyle", indent + 1, false));
                }
                nodes.Add(new TypeTreeNode("bool", "m_StretchWidth", indent + 1, true));
                nodes.Add(new TypeTreeNode("bool", "m_StretchHeight", indent + 1, true));
            }
        }

        public void AddGUIStyleState(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("GUIStyleState", name, indent, false));
            AddPPtr(nodes, "Texture2D", "m_Background", indent + 1);
            AddColorRGBA(nodes, "m_TextColor", indent + 1);
        }

        public void AddVector2f(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("Vector2f", name, indent, false));
            nodes.Add(new TypeTreeNode("float", "x", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "y", indent + 1, false));
        }

        public void AddRectOffset(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("RectOffset", name, indent, false));
            nodes.Add(new TypeTreeNode("int", "m_Left", indent + 1, false));
            nodes.Add(new TypeTreeNode("int", "m_Right", indent + 1, false));
            nodes.Add(new TypeTreeNode("int", "m_Top", indent + 1, false));
            nodes.Add(new TypeTreeNode("int", "m_Bottom", indent + 1, false));
        }

        public void AddColorRGBA(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("ColorRGBA", name, indent, false));
            nodes.Add(new TypeTreeNode("float", "r", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "g", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "b", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "a", indent + 1, false));
        }

        public void AddColor32(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("ColorRGBA", name, indent, false));
            nodes.Add(new TypeTreeNode("unsigned int", "rgba", indent + 1, false));
        }

        public void AddMatrix4x4(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("Matrix4x4f", name, indent, false));
            nodes.Add(new TypeTreeNode("float", "e00", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e01", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e02", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e03", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e10", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e11", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e12", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e13", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e20", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e21", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e22", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e23", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e30", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e31", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e32", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "e33", indent + 1, false));
        }

        public void AddSphericalHarmonicsL2(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("SphericalHarmonicsL2", name, indent, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 0]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 1]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 2]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 3]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 4]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 5]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 6]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 7]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 8]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[ 9]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[10]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[11]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[12]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[13]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[14]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[15]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[16]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[17]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[18]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[19]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[20]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[21]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[22]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[23]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[24]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[25]", indent + 1, false));
            nodes.Add(new TypeTreeNode("float", "sh[26]", indent + 1, false));
        }

        public void AddPropertyName(List<TypeTreeNode> nodes, string name, int indent)
        {
            nodes.Add(new TypeTreeNode("PropertyName", name, indent, false));
            AddString(nodes, "id", indent + 1);
        }
    }
}
