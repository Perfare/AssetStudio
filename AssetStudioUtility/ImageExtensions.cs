using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Tga;
using System.IO;

namespace AssetStudio
{
    public static class ImageExtensions
    {
        public static MemoryStream ConvertToStream(this Image image, ImageFormat imageFormat)
        {
            var outputStream = new MemoryStream();
            switch (imageFormat)
            {
                case ImageFormat.Jpeg:
                    image.SaveAsJpeg(outputStream);
                    break;
                case ImageFormat.Png:
                    image.SaveAsPng(outputStream);
                    break;
                case ImageFormat.Bmp:
                    image.Save(outputStream, new BmpEncoder
                    {
                        BitsPerPixel = BmpBitsPerPixel.Pixel32,
                        SupportTransparency = true
                    });
                    break;
                case ImageFormat.Tga:
                    image.Save(outputStream, new TgaEncoder
                    {
                        BitsPerPixel = TgaBitsPerPixel.Pixel32,
                        Compression = TgaCompression.None
                    });
                    break;
            }
            return outputStream;
        }
    }
}
