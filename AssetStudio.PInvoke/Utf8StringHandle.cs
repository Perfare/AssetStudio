using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AssetStudio.PInvoke
{
    // Generally the technique from Steamworks.NET
    public class Utf8StringHandle : SafeHandleZeroOrMinusOneIsInvalid
    {

        static Utf8StringHandle()
        {
            Utf8 = new UTF8Encoding(false);
        }

        public Utf8StringHandle(string str)
            : base(true)
        {
            IntPtr buffer;

            if (str == null)
            {
                buffer = IntPtr.Zero;
            }
            else
            {
                if (str.Length == 0)
                {
                    buffer = Marshal.AllocHGlobal(1);

                    unsafe
                    {
                        *(byte*)buffer = 0;
                    }
                }
                else
                {
                    var strlen = Utf8.GetByteCount(str);
                    var strBuffer = new byte[strlen + 1];

                    Utf8.GetBytes(str, 0, str.Length, strBuffer, 0);

                    buffer = Marshal.AllocHGlobal(strBuffer.Length);

                    Marshal.Copy(strBuffer, 0, buffer, strBuffer.Length);
                }
            }

            SetHandle(buffer);
        }

        public static string ReadUtf8StringFromPointer(IntPtr lpstr)
        {
            if (lpstr == IntPtr.Zero || lpstr == new IntPtr(-1))
            {
                return null;
            }

            var byteCount = 0;

            unsafe
            {
                var p = (byte*)lpstr.ToPointer();

                while (*p != 0)
                {
                    byteCount += 1;
                    p += 1;
                }
            }

            if (byteCount == 0)
            {
                return string.Empty;
            }

            var strBuffer = new byte[byteCount];

            Marshal.Copy(lpstr, strBuffer, 0, byteCount);

            var str = Utf8.GetString(strBuffer);

            return str;
        }

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                Marshal.FreeHGlobal(handle);
            }

            return true;
        }

        private static readonly UTF8Encoding Utf8;

    }
}
