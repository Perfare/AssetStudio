using System.Collections.Generic;

namespace AssetStudio
{
    public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2 m_Scale;
        public Vector2 m_Offset;

        public UnityTexEnv(ObjectReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2();
            m_Offset = reader.ReadVector2();
        }
    }

    public class UnityPropertySheet
    {
        public KeyValuePair<string, UnityTexEnv>[] m_TexEnvs;
        public KeyValuePair<string, int>[] m_Ints;
        public KeyValuePair<string, float>[] m_Floats;
        public KeyValuePair<string, Color>[] m_Colors;

        public UnityPropertySheet(ObjectReader reader)
        {
            var version = reader.version;

            int m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = new KeyValuePair<string, UnityTexEnv>[m_TexEnvsSize];
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                m_TexEnvs[i] = new KeyValuePair<string, UnityTexEnv>(reader.ReadAlignedString(), new UnityTexEnv(reader));
            }

            if (version[0] >= 2021) //2021.1 and up
            {
                int m_IntsSize = reader.ReadInt32();
                m_Ints = new KeyValuePair<string, int>[m_IntsSize];
                for (int i = 0; i < m_IntsSize; i++)
                {
                    m_Ints[i] = new KeyValuePair<string, int>(reader.ReadAlignedString(), reader.ReadInt32());
                }
            }

            int m_FloatsSize = reader.ReadInt32();
            m_Floats = new KeyValuePair<string, float>[m_FloatsSize];
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats[i] = new KeyValuePair<string, float>(reader.ReadAlignedString(), reader.ReadSingle());
            }

            int m_ColorsSize = reader.ReadInt32();
            m_Colors = new KeyValuePair<string, Color>[m_ColorsSize];
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors[i] = new KeyValuePair<string, Color>(reader.ReadAlignedString(), reader.ReadColor4());
            }
        }
    }

    public sealed class Material : NamedObject
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;

        public Material(ObjectReader reader) : base(reader)
        {
            m_Shader = new PPtr<Shader>(reader);

            if (version[0] == 4 && version[1] >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadStringArray();
            }

            if (version[0] > 2021 || (version[0] == 2021 && version[1] >= 3)) //2021.3 and up
            {
                var m_ValidKeywords = reader.ReadStringArray();
                var m_InvalidKeywords = reader.ReadStringArray();
            }
            else if (version[0] >= 5) //5.0 ~ 2021.2
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
            }

            if (version[0] >= 5) //5.0 and up
            {
                var m_LightmapFlags = reader.ReadUInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                //var m_DoubleSidedGI = a_Stream.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.ReadInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                }
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadStringArray();
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }
    }
}
