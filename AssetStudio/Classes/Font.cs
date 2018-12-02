using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class Font : NamedObject
    {
        public byte[] m_FontData;

        public Font(ObjectReader reader) : base(reader)
        {
            if ((version[0] == 5 && version[1] >= 5) || version[0] > 5)//5.5 and up
            {
                var m_LineSpacing = reader.ReadSingle();
                var m_DefaultMaterial = new PPtr<Material>(reader);
                var m_FontSize = reader.ReadSingle();
                var m_Texture = new PPtr<Texture>(reader);
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
                var m_DefaultMaterial = new PPtr<Material>(reader);

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
                        reader.AlignStream();
                    }
                }

                var m_Texture = new PPtr<Texture>(reader);

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
                    reader.AlignStream();
                }
                else { float m_PixelScale = reader.ReadSingle(); }

                int m_FontData_size = reader.ReadInt32();
                if (m_FontData_size > 0)
                {
                    m_FontData = reader.ReadBytes(m_FontData_size);
                }
            }
        }
    }
}
