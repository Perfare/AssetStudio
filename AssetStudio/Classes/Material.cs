using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class TexEnv
    {
        public string name;
        public PPtr m_Texture;
        public float[] m_Scale;
        public float[] m_Offset;
    }

    public class strFloatPair
    {
        public string first;
        public float second;
    }

    public class strColorPair
    {
        public string first;
        public float[] second;
    }

    public sealed class Material : NamedObject
    {
        public PPtr m_Shader;
        public string[] m_ShaderKeywords;
        public int m_CustomRenderQueue;
        public TexEnv[] m_TexEnvs;
        public strFloatPair[] m_Floats;
        public strColorPair[] m_Colors;

        public Material(ObjectReader reader) : base(reader)
        {
            m_Shader = reader.ReadPPtr();

            if (version[0] == 4 && (version[1] >= 2 || (version[1] == 1 && buildType[0] != "a")))
            {
                m_ShaderKeywords = new string[reader.ReadInt32()];
                for (int i = 0; i < m_ShaderKeywords.Length; i++)
                {
                    m_ShaderKeywords[i] = reader.ReadAlignedString();
                }
            }
            else if (version[0] >= 5)//5.0 and up
            {
                m_ShaderKeywords = new[] { reader.ReadAlignedString() };
                uint m_LightmapFlags = reader.ReadUInt32();
                if (version[0] == 5 && version[1] >= 6 || version[0] > 5)//5.6.0 and up
                {
                    var m_EnableInstancingVariants = reader.ReadBoolean();
                    //var m_DoubleSidedGI = a_Stream.ReadBoolean();//2017.x
                    reader.AlignStream(4);
                }
            }

            if (version[0] > 4 || version[0] == 4 && version[1] >= 3) { m_CustomRenderQueue = reader.ReadInt32(); }

            if (version[0] == 5 && version[1] >= 1 || version[0] > 5)//5.1 and up
            {
                string[][] stringTagMap = new string[reader.ReadInt32()][];
                for (int i = 0; i < stringTagMap.Length; i++)
                {
                    stringTagMap[i] = new[] { reader.ReadAlignedString(), reader.ReadAlignedString() };
                }
            }
            //disabledShaderPasses
            if ((version[0] == 5 && version[1] >= 6) || version[0] > 5)//5.6.0 and up
            {
                var size = reader.ReadInt32();
                for (int i = 0; i < size; i++)
                {
                    reader.ReadAlignedString();
                }
            }
            //m_SavedProperties
            m_TexEnvs = new TexEnv[reader.ReadInt32()];
            for (int i = 0; i < m_TexEnvs.Length; i++)
            {
                TexEnv m_TexEnv = new TexEnv()
                {
                    name = reader.ReadAlignedString(),
                    m_Texture = reader.ReadPPtr(),
                    m_Scale = new[] { reader.ReadSingle(), reader.ReadSingle() },
                    m_Offset = new[] { reader.ReadSingle(), reader.ReadSingle() }
                };
                m_TexEnvs[i] = m_TexEnv;
            }

            m_Floats = new strFloatPair[reader.ReadInt32()];
            for (int i = 0; i < m_Floats.Length; i++)
            {
                strFloatPair m_Float = new strFloatPair()
                {
                    first = reader.ReadAlignedString(),
                    second = reader.ReadSingle()
                };
                m_Floats[i] = m_Float;
            }

            m_Colors = new strColorPair[reader.ReadInt32()];
            for (int i = 0; i < m_Colors.Length; i++)
            {
                strColorPair m_Color = new strColorPair()
                {
                    first = reader.ReadAlignedString(),
                    second = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() }
                };
                m_Colors[i] = m_Color;
            }
        }
    }
}
