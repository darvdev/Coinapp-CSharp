using System;
using System.Runtime.InteropServices;

namespace coinapplib
{
    public class Comctl32
    {

        public const string DLL = "comctl32.dll";

        public const uint WM_CLOSE = 0x0010;
        public const uint WM_NCDESTROY = 0x0082;

        public delegate IntPtr SubclassWndProc(IntPtr hWnd, uint uMsg, UIntPtr wParam, UIntPtr lParam, UIntPtr uIdSubclass, UIntPtr dwRefData);

        [DllImport(DLL, CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetWindowSubclass(
            [param: In]
            IntPtr hWnd,
            [param: In]
            SubclassWndProc pfnSubclass,
            [param: In]
            UIntPtr uIdSubclass,
            [param: In]
            UIntPtr dwRefData);

        [DllImport(DLL, CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool RemoveWindowSubclass(
            [param: In]
            IntPtr hWnd,
            [param: In]
            SubclassWndProc pfnSubclass,
            [param: In]
            UIntPtr uIdSubclass);

        [DllImport(DLL, CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr DefSubclassProc(
            [param: In]
            IntPtr hWnd,
            [param: In, MarshalAs(UnmanagedType.U4)]
            uint uMsg,
            [param: In]
            UIntPtr WPARAM,
            [param: In]
            UIntPtr LPARAM);
    }
}
