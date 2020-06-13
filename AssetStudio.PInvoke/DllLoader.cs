using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace AssetStudio.PInvoke
{
    public static class DllLoader
    {

        public static void PreloadDll(string dllName)
        {
            var dllDir = GetDirectedDllDirectory();

            // Not using OperatingSystem.Platform.
            // See: https://www.mono-project.com/docs/faq/technical/#how-to-detect-the-execution-platform
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Win32.LoadDll(dllDir, dllName);
            }
            else
            {
                Posix.LoadDll(dllDir, dllName);
            }
        }

        private static string GetDirectedDllDirectory()
        {
            var localPath = new Uri(typeof(DllLoader).Assembly.CodeBase).LocalPath;
            var localDir = Path.GetDirectoryName(localPath);

            var subDir = Environment.Is64BitProcess ? "x64" : "x86";

            var directedDllDir = Path.Combine(localDir, subDir);

            return directedDllDir;
        }

        private static class Win32
        {

            internal static void LoadDll(string dllDir, string dllName)
            {
                var dllFileName = $"{dllName}.dll";
                var directedDllPath = Path.Combine(dllDir, dllFileName);

                // Specify SEARCH_DLL_LOAD_DIR to load dependent libraries located in the same platform-specific directory.
                var hLibrary = LoadLibraryEx(directedDllPath, IntPtr.Zero, LOAD_LIBRARY_SEARCH_DEFAULT_DIRS | LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR);

                if (hLibrary == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var exception = new Win32Exception(errorCode);

                    throw new DllNotFoundException(exception.Message, exception);
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

            private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x1000;
            private const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x100;

        }

        private static class Posix
        {

            internal static void LoadDll(string dllDir, string dllName)
            {
                string dllExtension;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    dllExtension = ".so";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    dllExtension = ".dylib";
                }
                else
                {
                    throw new NotSupportedException();
                }

                var dllFileName = $"lib{dllName}{dllExtension}";
                var directedDllPath = Path.Combine(dllDir, dllFileName);

                var hLibrary = DlOpen(directedDllPath, RTLD_NOW | RTLD_GLOBAL);

                if (hLibrary == IntPtr.Zero)
                {
                    var pErrStr = DlError();
                    var errorMessage = Marshal.PtrToStringAnsi(pErrStr);

                    throw new DllNotFoundException(errorMessage);
                }
            }

            [DllImport("libdl", EntryPoint = "dlopen")]
            private static extern IntPtr DlOpen([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

            [DllImport("libdl", EntryPoint = "dlerror")]
            private static extern IntPtr DlError();

            private const int RTLD_LAZY = 0x1;
            private const int RTLD_NOW = 0x2;
            private const int RTLD_GLOBAL = 0x100;

        }

    }
}
