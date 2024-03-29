﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace coinapplib
{
    public class DT
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        private static DateTime GetNetworkTime()
        {
            const string ntpServer = "time.windows.com";// "pool.ntp.org";
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
        public static void Update()
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (short)GetNetworkTime().Year; // must be short
            st.wMonth = (short)GetNetworkTime().Month;
            st.wDay = (short)GetNetworkTime().Day;
            st.wHour = (short)GetNetworkTime().Hour;
            st.wMinute = (short)GetNetworkTime().Minute;
            st.wSecond = (short)GetNetworkTime().Second;
            st.wMilliseconds = (short)GetNetworkTime().Millisecond;

            SetSystemTime(ref st); // invoke this method.
        }
    }
}
