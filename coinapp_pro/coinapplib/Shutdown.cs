﻿using System;
using System.Runtime.InteropServices;

namespace coinapplib
{
    public class Shutdown
    {


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
            ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool ExitWindowsEx(int flg, int rea);

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        private const int EWX_LOGOFF = 0x00000000;
        private const int EWX_SHUTDOWN = 0x00000001;
        private const int EWX_REBOOT = 0x00000002;
        private const int EWX_FORCE = 0x00000004;
        private const int EWX_POWEROFF = 0x00000008;
        private const int EWX_FORCEIFHUNG = 0x00000010;

        private static void DoExitWin(int flg)
        {
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            ExitWindowsEx(flg, 0);
        }

        public static void Force()
        {
            //DoExitWin(EWX_SHUTDOWN | EWX_FORCE);
            DoExitWin(EWX_SHUTDOWN | EWX_FORCEIFHUNG);
        }

        public static void Restart()
        {
            DoExitWin(EWX_REBOOT | EWX_FORCEIFHUNG);
        }
    
    }
}
