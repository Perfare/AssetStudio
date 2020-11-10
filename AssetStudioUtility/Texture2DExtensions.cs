using System.Drawing;

namespace AssetStudio
{
    public static class Texture2DExtensions
    {
        public static Bitmap ConvertToBitmap(this Texture2D m_Texture2D, bool flip)
        {
            var converter = new Texture2DConverter(m_Texture2D);
            return converter.ConvertToBitmap(flip);
        }

        public static Bitmap ConvertToBitmap(this Texture2DArray m_Texture2DArray, bool flip, int layer)
        {
            var converter = new Texture2DConverter(m_Texture2DArray, layer);
            return converter.ConvertToBitmap(flip);
        }
    }
}
