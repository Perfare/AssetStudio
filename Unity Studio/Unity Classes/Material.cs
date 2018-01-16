using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
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
            var a_Stream = preloadData.sourceFile.a_Stream;
            a_Stream.Position = preloadData.Offset;

            if (sourceFile.platform == -2)
            {
                uint m_ObjectHideFlags = a_Stream.ReadUInt32();
                PPtr m_PrefabParentObject = sourceFile.ReadPPtr();
                PPtr m_PrefabInternal = sourceFile.ReadPPtr();
            }

            m_Name = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
            m_Shader = sourceFile.ReadPPtr();

            if (sourceFile.version[0] == 4 && (sourceFile.version[1] >= 2 || (sourceFile.version[1] == 1 && sourceFile.buildType[0] != "a")))
            {
                m_ShaderKeywords = new string[a_Stream.ReadInt32()];
                for (int i = 0; i < m_ShaderKeywords.Length; i++)
                {
                    m_ShaderKeywords[i] = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                }
            }
            else if (sourceFile.version[0] >= 5)//5.0 and up
            {
                m_ShaderKeywords = new[] { a_Stream.ReadAlignedString(a_Stream.ReadInt32()) };
                uint m_LightmapFlags = a_Stream.ReadUInt32();
                if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 6 || sourceFile.version[0] > 5)//5.6.0 and up
                {
                    var m_EnableInstancingVariants = a_Stream.ReadBoolean();
                    //var m_DoubleSidedGI = a_Stream.ReadBoolean();//2017.x
                    a_Stream.AlignStream(4);
                }
            }

            if (sourceFile.version[0] > 4 || sourceFile.version[0] == 4 && sourceFile.version[1] >= 3) { m_CustomRenderQueue = a_Stream.ReadInt32(); }

            if (sourceFile.version[0] == 5 && sourceFile.version[1] >= 1 || sourceFile.version[0] > 5)//5.1 and up
            {
                string[][] stringTagMap = new string[a_Stream.ReadInt32()][];
                for (int i = 0; i < stringTagMap.Length; i++)
                {
                    stringTagMap[i] = new[] { a_Stream.ReadAlignedString(a_Stream.ReadInt32()), a_Stream.ReadAlignedString(a_Stream.ReadInt32()) };
                }
            }
            //disabledShaderPasses
            if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 6) || sourceFile.version[0] > 5)//5.6.0 and up
            {
                var size = a_Stream.ReadInt32();
                for (int i = 0; i < size; i++)
                {
                    a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                }
            }
            //m_SavedProperties
            m_TexEnvs = new TexEnv[a_Stream.ReadInt32()];
            for (int i = 0; i < m_TexEnvs.Length; i++)
            {
                TexEnv m_TexEnv = new TexEnv()
                {
                    name = a_Stream.ReadAlignedString(a_Stream.ReadInt32()),
                    m_Texture = sourceFile.ReadPPtr(),
                    m_Scale = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle() },
                    m_Offset = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle() }
                };
                m_TexEnvs[i] = m_TexEnv;
            }

            m_Floats = new strFloatPair[a_Stream.ReadInt32()];
            for (int i = 0; i < m_Floats.Length; i++)
            {
                strFloatPair m_Float = new strFloatPair()
                {
                    first = a_Stream.ReadAlignedString(a_Stream.ReadInt32()),
                    second = a_Stream.ReadSingle()
                };
                m_Floats[i] = m_Float;
            }

            m_Colors = new strColorPair[a_Stream.ReadInt32()];
            for (int i = 0; i < m_Colors.Length; i++)
            {
                strColorPair m_Color = new strColorPair()
                {
                    first = a_Stream.ReadAlignedString(a_Stream.ReadInt32()),
                    second = new[] { a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle(), a_Stream.ReadSingle() }
                };
                m_Colors[i] = m_Color;
            }
        }
    }
}
