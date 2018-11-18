using System.IO;

namespace AssetStudio
{
    public static class StreamExtensions
    {
        private const int BufferSize = 81920;

        public static void CopyTo(this Stream source, Stream destination, long size)
        {
            var buffer = new byte[BufferSize];
            for (var left = size; left > 0; left -= BufferSize)
            {
                int toRead = BufferSize < left ? BufferSize : (int)left;
                int read = source.Read(buffer, 0, toRead);
                destination.Write(buffer, 0, read);
                if (read != toRead)
                {
                    return;
                }
            }
        }
    }
}
