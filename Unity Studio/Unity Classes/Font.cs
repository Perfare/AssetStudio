using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unity_Studio
{
    class UnityFont
    {
        public string m_Name;
        public byte[] m_FontData;

        public UnityFont(AssetPreloadData preloadData, bool readSwitch)
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

            if (readSwitch)
            {
                if ((sourceFile.version[0] == 5 && sourceFile.version[1] >= 5) || sourceFile.version[0] > 5)
                {
                    var m_LineSpacing = reader.ReadSingle();
                    var m_DefaultMaterial = sourceFile.ReadPPtr();
                    var m_FontSize = reader.ReadSingle();
                    var m_Texture = sourceFile.ReadPPtr();
                    int m_AsciiStartOffset = reader.ReadInt32();
                    var m_Tracking = reader.ReadSingle();
                    var m_CharacterSpacing = reader.ReadInt32();
                    var m_CharacterPadding = reader.ReadInt32();
                    var m_ConvertCase = reader.ReadInt32();
                    int m_CharacterRects_size = reader.ReadInt32();
                    for (int i = 0; i < m_CharacterRects_size; i++)
                    {
                        int index = reader.ReadInt32();
                        //Rectf uv
                        float uvx = reader.ReadSingle();
                        float uvy = reader.ReadSingle();
                        float uvwidth = reader.ReadSingle();
                        float uvheight = reader.ReadSingle();
                        //Rectf vert
                        float vertx = reader.ReadSingle();
                        float verty = reader.ReadSingle();
                        float vertwidth = reader.ReadSingle();
                        float vertheight = reader.ReadSingle();
                        float width = reader.ReadSingle();

                        if (sourceFile.version[0] >= 4)
                        {
                            bool flipped = reader.ReadBoolean();
                            reader.Position += 3;
                        }
                    }
                    int m_KerningValues_size = reader.ReadInt32();
                    for (int i = 0; i < m_KerningValues_size; i++)
                    {
                        int pairfirst = reader.ReadInt16();
                        int pairsecond = reader.ReadInt16();
                        float second = reader.ReadSingle();
                    }
                    var m_PixelScale = reader.ReadSingle();
                    int m_FontData_size = reader.ReadInt32();
                    if (m_FontData_size > 0)
                    {
                        m_FontData = reader.ReadBytes(m_FontData_size);

                        if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
                        { preloadData.extension = ".otf"; }
                        else { preloadData.extension = ".ttf"; }
                    }
                }
                else
                {
                    int m_AsciiStartOffset = reader.ReadInt32();

                    if (sourceFile.version[0] <= 3)
                    {
                        int m_FontCountX = reader.ReadInt32();
                        int m_FontCountY = reader.ReadInt32();
                    }

                    float m_Kerning = reader.ReadSingle();
                    float m_LineSpacing = reader.ReadSingle();

                    if (sourceFile.version[0] <= 3)
                    {
                        int m_PerCharacterKerning_size = reader.ReadInt32();
                        for (int i = 0; i < m_PerCharacterKerning_size; i++)
                        {
                            int first = reader.ReadInt32();
                            float second = reader.ReadSingle();
                        }
                    }
                    else
                    {
                        int m_CharacterSpacing = reader.ReadInt32();
                        int m_CharacterPadding = reader.ReadInt32();
                    }

                    int m_ConvertCase = reader.ReadInt32();
                    PPtr m_DefaultMaterial = sourceFile.ReadPPtr();

                    int m_CharacterRects_size = reader.ReadInt32();
                    for (int i = 0; i < m_CharacterRects_size; i++)
                    {
                        int index = reader.ReadInt32();
                        //Rectf uv
                        float uvx = reader.ReadSingle();
                        float uvy = reader.ReadSingle();
                        float uvwidth = reader.ReadSingle();
                        float uvheight = reader.ReadSingle();
                        //Rectf vert
                        float vertx = reader.ReadSingle();
                        float verty = reader.ReadSingle();
                        float vertwidth = reader.ReadSingle();
                        float vertheight = reader.ReadSingle();
                        float width = reader.ReadSingle();

                        if (sourceFile.version[0] >= 4)
                        {
                            bool flipped = reader.ReadBoolean();
                            reader.Position += 3;
                        }
                    }

                    PPtr m_Texture = sourceFile.ReadPPtr();

                    int m_KerningValues_size = reader.ReadInt32();
                    for (int i = 0; i < m_KerningValues_size; i++)
                    {
                        int pairfirst = reader.ReadInt16();
                        int pairsecond = reader.ReadInt16();
                        float second = reader.ReadSingle();
                    }

                    if (sourceFile.version[0] <= 3)
                    {
                        bool m_GridFont = reader.ReadBoolean();
                        reader.Position += 3; //4 byte alignment
                    }
                    else { float m_PixelScale = reader.ReadSingle(); }

                    int m_FontData_size = reader.ReadInt32();
                    if (m_FontData_size > 0)
                    {
                        m_FontData = reader.ReadBytes(m_FontData_size);

                        if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
                        { preloadData.extension = ".otf"; }
                        else { preloadData.extension = ".ttf"; }

                    }

                    float m_FontSize = reader.ReadSingle();//problem here in minifootball
                    float m_Ascent = reader.ReadSingle();
                    uint m_DefaultStyle = reader.ReadUInt32();

                    int m_FontNames = reader.ReadInt32();
                    for (int i = 0; i < m_FontNames; i++)
                    {
                        string m_FontName = reader.ReadAlignedString(reader.ReadInt32());
                    }

                    if (sourceFile.version[0] >= 4)
                    {
                        int m_FallbackFonts = reader.ReadInt32();
                        for (int i = 0; i < m_FallbackFonts; i++)
                        {
                            PPtr m_FallbackFont = sourceFile.ReadPPtr();
                        }

                        int m_FontRenderingMode = reader.ReadInt32();
                    }
                }
            }
            else
            {
                preloadData.Text = m_Name;
            }
        }
    }
}
