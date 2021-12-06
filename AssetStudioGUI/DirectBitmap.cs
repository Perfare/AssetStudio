using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AssetStudioGUI
{
    public sealed class DirectBitmap : IDisposable
    {
        public DirectBitmap(byte[] buff, int width, int height)
        {
            Width = width;
            Height = height;
            Bits = buff;
            m_handle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            m_bitmap = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppArgb, m_handle.AddrOfPinnedObject());
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_bitmap.Dispose();
                m_handle.Free();
            }
            m_bitmap = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public int Height { get; }
        public int Width { get; }
        public int Stride => Width * 4;
        public byte[] Bits { get; }
        public Bitmap Bitmap => m_bitmap;

        private Bitmap m_bitmap;
        private readonly GCHandle m_handle;
    }
}
