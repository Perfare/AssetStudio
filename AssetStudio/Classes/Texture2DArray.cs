namespace AssetStudio
{
    public sealed class Texture2DArray : Texture
    {
        public int m_Width;
        public int m_Height;
        public int m_Depth;
        public TextureFormat m_TextureFormat;
        public int m_MipCount;
        public uint m_DataSize;
        public GLTextureSettings m_TextureSettings;
        public int m_ColorSpace;
        public bool m_IsReadable;
        public ResourceReader image_data;
        public StreamingInfo m_StreamData;

        public Texture2DArray(ObjectReader reader) : base(reader)
        {
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            m_Depth = reader.ReadInt32();
            m_TextureFormat = (TextureFormat)reader.ReadInt32();
            m_MipCount = reader.ReadInt32();
            m_DataSize = reader.ReadUInt32();
            m_TextureSettings = new GLTextureSettings(reader);
            m_ColorSpace = reader.ReadInt32();
            m_IsReadable = reader.ReadBoolean();
            reader.AlignStream();

            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0)
            {
                m_StreamData = new StreamingInfo(reader);
            }

            if (!string.IsNullOrEmpty(m_StreamData?.path))
            {
                image_data = new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, (int)m_StreamData.size);
            }
            else
            {
                image_data = new ResourceReader(reader, reader.BaseStream.Position, image_data_size);
            }
        }
    }
}
