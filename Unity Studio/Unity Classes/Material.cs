using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class TexEnv
    {
        public string name;
        public PPtr m_Texture;
        public float[] m_Scale;
        public float[] m_Offset;
    }

    class strFloatPair
    {
        public string first;
        public float second;
    }

    class strColorPair
    {
        public string first;
        public float[] second;
    }

    class Material
    {
        public string m_Name;
        public PPtr m_Shader;
        public string[] m_ShaderKeywords;
        public int m_CustomRenderQueue;
        public TexEnv[] m_TexEnvs;
        public strFloatPair[] m_Floats;
        public strColorPair[] m_Colors;

        public Material(AssetPreloadData preloadData)
        {
            var sourceFile = preloadData.sourceFile;
            var reader = preloadData.Reader;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = reader.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = reader.ReadAlignedString(reader.ReadInt32());
            m_Shader = sourceFile.ReadPPtr();

            if (sourceFile.version[0] == 4 && (sourceFile.version[1] >= 2 || (sourceFile.version[1] == 1 && sourceFile.buildType[0] != "a")))
            {
                m_ShaderKeywords = new string[reader.ReadInt32()];
                for (int i = 0; i < m_ShaderKeywords.Length; i++)
                {
                    m_ShaderKeywords[i] = reader.ReadAlignedString(reader.ReadInt32());
                }
            }
            else if (sourceFile.version[0] >= 5)//5.0 and up
            {
                m_ShaderKeywords = new[] { reader.ReadAlignedString(reader.ReadInt32()) };
                uint m_LightmapFlags = reader.ReadUInt32();
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 6 || sourceFile.version[0] > 5)//5.6.0 and up
                {
                    var m_EnableInstancingVariants = reader.ReadBoolean();
                    //var m_DoubleSidedGI = a_Stream.ReadBoolean();//2017.x
                    reader.AlignStream(4);
                }
            }

            if (sourceFile.version[0] > 4 || sourceFile.version[0] == 4 && sourceFile.version[1] >= 3) { m_CustomRenderQueue = reader.ReadInt32(); }

            if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 1 || sourceFile.version[0] > 5)//5.1 and up
            {
                string[][] stringTagMap = new string[reader.ReadInt32()][];
                for (int i = 0; i < stringTagMap.Length; i++)
                {
                    stringTagMap[i] = new[] { reader.ReadAlignedString(reader.ReadInt32()), reader.ReadAlignedString(reader.ReadInt32()) };
                }
            }
            //disabledShaderPasses
            if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 6) || sourceFile.version[0] > 5)//5.6.0 and up
            {
                var size = reader.ReadInt32();
                for (int i = 0; i < size; i++)
                {
                    reader.ReadAlignedString(reader.ReadInt32());
                }
            }
            //m_SavedProperties
            m_TexEnvs = new TexEnv[reader.ReadInt32()];
            for (int i = 0; i < m_TexEnvs.Length; i++)
            {
                TexEnv m_TexEnv = new TexEnv()
                {
                    name = reader.ReadAlignedString(reader.ReadInt32()),
                    m_Texture = sourceFile.ReadPPtr(),
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
                    first = reader.ReadAlignedString(reader.ReadInt32()),
                    second = reader.ReadSingle()
                };
                m_Floats[i] = m_Float;
            }

            m_Colors = new strColorPair[reader.ReadInt32()];
            for (int i = 0; i < m_Colors.Length; i++)
            {
                strColorPair m_Color = new strColorPair()
                {
                    first = reader.ReadAlignedString(reader.ReadInt32()),
                    second = new[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() }
                };
                m_Colors[i] = m_Color;
            }
        }
    }
}
