using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace coinapplib
{
    public class Window
    {

        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_SHOWWINDOW = 0x40;
        private const int FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        private const uint SW_RESTORE = 0x09;

        private const int SWP_NOSIZE = 0x1;
        private const int SWP_NOMOVE = 0x2;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

        [DllImport("User32.dll")]
        private static extern Int32 SetForegroundWindow(IntPtr hWnd);


        public static void SetForeground(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
        }

        public static void Show(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_RESTORE);
        }

        public static void Top(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        public static void Normal(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        public static bool DesktopLoaded
        {
            get
            {
                IntPtr lHwnd = IntPtr.Zero;
                while (lHwnd == IntPtr.Zero)
                {
                    lHwnd = FindWindow("Shell_TrayWnd", null);
                    Task.Delay(10);
                }

                return true;

            }
        }
    }
}
