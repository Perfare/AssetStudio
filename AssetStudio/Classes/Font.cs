using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    class UFont
    {
        public string m_Name;
        public byte[] m_FontData;

        public UFont(AssetPreloadData preloadData, bool readSwitch)
        {
            var sourceFile = preloadData.sourceFile;
            var version = sourceFile.version;
            var reader = preloadData.InitReader();

            m_Name = reader.ReadAlignedString();

            if (readSwitch)
            {
                if ((version[0] == 5 && version[1] >= 5) || version[0] > 5)//5.5 and up
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
                        reader.Position += 44;//CharacterInfo data 41
                    }
                    int m_KerningValues_size = reader.ReadInt32();
                    for (int i = 0; i < m_KerningValues_size; i++)
                    {
                        reader.Position += 8;
                    }
                    var m_PixelScale = reader.ReadSingle();
                    int m_FontData_size = reader.ReadInt32();
                    if (m_FontData_size > 0)
                    {
                        m_FontData = reader.ReadBytes(m_FontData_size);
                        if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
                        {
                            preloadData.extension = ".otf";
                        }
                        else
                        {
                            preloadData.extension = ".ttf";
                        }
                    }
                }
                else
                {
                    int m_AsciiStartOffset = reader.ReadInt32();

                    if (version[0] <= 3)
                    {
                        int m_FontCountX = reader.ReadInt32();
                        int m_FontCountY = reader.ReadInt32();
                    }

                    float m_Kerning = reader.ReadSingle();
                    float m_LineSpacing = reader.ReadSingle();

                    if (version[0] <= 3)
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

                        if (version[0] >= 4)
                        {
                            var flipped = reader.ReadBoolean();
                            reader.AlignStream(4);
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

                    if (version[0] <= 3)
                    {
                        var m_GridFont = reader.ReadBoolean();
                        reader.AlignStream(4);
                    }
                    else { float m_PixelScale = reader.ReadSingle(); }

                    int m_FontData_size = reader.ReadInt32();
                    if (m_FontData_size > 0)
                    {
                        m_FontData = reader.ReadBytes(m_FontData_size);

                        if (m_FontData[0] == 79 && m_FontData[1] == 84 && m_FontData[2] == 84 && m_FontData[3] == 79)
                        {
                            preloadData.extension = ".otf";
                        }
                        else
                        {
                            preloadData.extension = ".ttf";
                        }
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
