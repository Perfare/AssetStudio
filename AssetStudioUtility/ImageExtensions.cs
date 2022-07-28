using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Runtime.InteropServices;

namespace AssetStudio
{
    public static class ImageExtensions
    {
        public static void WriteToStream(this Image image, Stream stream, ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormat.Jpeg:
                    image.SaveAsJpeg(stream);
                    break;
                case ImageFormat.Png:
                    image.SaveAsPng(stream);
                    break;
                case ImageFormat.Bmp:
                    image.Save(stream, new BmpEncoder
                    {
                        BitsPerPixel = BmpBitsPerPixel.Pixel32,
                        SupportTransparency = true
                    });
                    break;
                case ImageFormat.Tga:
                    image.Save(stream, new TgaEncoder
                    {
                        BitsPerPixel = TgaBitsPerPixel.Pixel32,
                        Compression = TgaCompression.None
                    });
                    break;
            }
        }

        public static MemoryStream ConvertToStream(this Image image, ImageFormat imageFormat)
        {
            var stream = new MemoryStream();
            image.WriteToStream(stream, imageFormat);
            return stream;
        }

        public static byte[] ConvertToBytes<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (image.TryGetSinglePixelSpan(out var pixelSpan))
            {
                return MemoryMarshal.AsBytes(pixelSpan).ToArray();
            }
            return null;
        }
    }
}
