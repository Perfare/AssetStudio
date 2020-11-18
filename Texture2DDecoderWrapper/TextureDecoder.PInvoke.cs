using System.Runtime.InteropServices;

namespace Texture2DDecoder
{
    unsafe partial class TextureDecoder
    {

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeDXT1(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeDXT5(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodePVRTC(void* data, int width, int height, void* image, [MarshalAs(UnmanagedType.Bool)] bool is2bpp);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeETC1(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeETC2(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeETC2A1(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeETC2A8(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeEACR(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeEACRSigned(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeEACRG(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeEACRGSigned(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeBC4(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeBC5(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeBC6(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeBC7(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeATCRGB4(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeATCRGBA8(void* data, int width, int height, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DecodeASTC(void* data, int width, int height, int blockWidth, int blockHeight, void* image);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void DisposeBuffer(ref void* ppBuffer);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void UnpackCrunch(void* data, uint dataSize, out void* result, out uint resultSize);

        [DllImport(T2DDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void UnpackUnityCrunch(void* data, uint dataSize, out void* result, out uint resultSize);

    }
}
