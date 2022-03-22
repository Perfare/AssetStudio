using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AssetStudioGUI
{
    internal class OpenFolderDialog
    {
        public string InitialFolder { get; set; }
        public string DefaultFolder { get; set; }
        public string Folder { get; private set; }
        public string Title { get; set; }

        internal DialogResult ShowDialog(IWin32Window owner = null)
        {
//#if NETFRAMEWORK
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return ShowVistaDialog(owner);
            }
//#endif
            return ShowFolderBrowserDialog(owner);
        }

        private DialogResult ShowVistaDialog(IWin32Window owner)
        {
            var frm = (NativeMethods.IFileDialog)(new NativeMethods.FileOpenDialogRCW());
            frm.GetOptions(out var options);
            options |= NativeMethods.FOS_PICKFOLDERS | NativeMethods.FOS_FORCEFILESYSTEM | NativeMethods.FOS_NOVALIDATE | NativeMethods.FOS_NOTESTFILECREATE | NativeMethods.FOS_DONTADDTORECENT;
            frm.SetOptions(options);
            if (!string.IsNullOrEmpty(Title))
            {
                frm.SetTitle(Title);
            }
            if (!string.IsNullOrEmpty(InitialFolder))
            {
                var riid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); //IShellItem  
                if (NativeMethods.SHCreateItemFromParsingName(InitialFolder, IntPtr.Zero, ref riid, out var directoryShellItem) == NativeMethods.S_OK)
                {
                    frm.SetFolder(directoryShellItem);
                }
            }
            if (!string.IsNullOrEmpty(DefaultFolder))
            {
                var riid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); //IShellItem  
                if (NativeMethods.SHCreateItemFromParsingName(DefaultFolder, IntPtr.Zero, ref riid, out var directoryShellItem) == NativeMethods.S_OK)
                {
                    frm.SetDefaultFolder(directoryShellItem);
                }
            }

            if ((owner == null ? frm.Show() : frm.Show(owner.Handle)) == NativeMethods.S_OK)
            {
                if (frm.GetResult(out var shellItem) == NativeMethods.S_OK)
                {
                    if (shellItem.GetDisplayName(NativeMethods.SIGDN_FILESYSPATH, out var pszString) == NativeMethods.S_OK)
                    {
                        if (pszString != IntPtr.Zero)
                        {
                            try
                            {
                                Folder = Marshal.PtrToStringAuto(pszString);
                                return DialogResult.OK;
                            }
                            finally
                            {
                                Marshal.FreeCoTaskMem(pszString);
                            }
                        }
                    }
                }
            }
            return DialogResult.Cancel;
        }

        private DialogResult ShowFolderBrowserDialog(IWin32Window owner)
        {
            using (var frm = new FolderBrowserDialog())
            {
                if (InitialFolder != null)
                {
                    frm.SelectedPath = InitialFolder;
                }
#if !NETFRAMEWORK
                if (Title != null)
                {
                    frm.Description = Title;
                    frm.UseDescriptionForTitle = true;
                }
#endif
                var result = owner == null ? frm.ShowDialog() : frm.ShowDialog(owner);
                if (result == DialogResult.OK)
                {
                    Folder = frm.SelectedPath;
                    return result;
                }
                return result;
            }
        }
    }

    internal static class NativeMethods
    {

        #region Constants  

        public const uint FOS_PICKFOLDERS = 0x00000020;
        public const uint FOS_FORCEFILESYSTEM = 0x00000040;
        public const uint FOS_NOVALIDATE = 0x00000100;
        public const uint FOS_NOTESTFILECREATE = 0x00010000;
        public const uint FOS_DONTADDTORECENT = 0x02000000;

        public const uint S_OK = 0x0000;

        public const uint SIGDN_FILESYSPATH = 0x80058000;

        #endregion


        #region COM  

        [ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        internal class FileOpenDialogRCW { }


        [ComImport(), Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialog
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig()]
            uint Show([In, Optional] IntPtr hwndOwner); //IModalWindow   


            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileTypes([In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileTypeIndex([In] uint iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFileTypeIndex(out uint piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Advise([In, MarshalAs(UnmanagedType.Interface)] IntPtr pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Unadvise([In] uint dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetOptions([In] uint fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetOptions(out uint fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Close([MarshalAs(UnmanagedType.Error)] uint hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetClientGuid([In] ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint ClearClientData();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }


        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint BindToHandler([In] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IntPtr ppvOut);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetDisplayName([In] uint sigdnName, out IntPtr ppszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            uint Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In] uint hint, out int piOrder);
        }

        #endregion


        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);
    }
}