using System;
using System.IO;
using System.Runtime.InteropServices;
using coinapplib.Properties;

namespace coinapplib
{
    public class Sounds //Implemenet NAudio here!
    {

        private const int WM_APPCOMMAND = 0x319;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;

        [DllImport("user32")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static void Unmute(IntPtr handle)
        {
            SendMessageW(handle, WM_APPCOMMAND, handle, new IntPtr(APPCOMMAND_VOLUME_UP));
            SendMessageW(handle, WM_APPCOMMAND, handle, new IntPtr(APPCOMMAND_VOLUME_MUTE));
            SendMessageW(handle, WM_APPCOMMAND, handle, new IntPtr(APPCOMMAND_VOLUME_MUTE));
        }

        public static void Mute(IntPtr handle)
        {
            SendMessageW(handle, WM_APPCOMMAND, handle, new IntPtr(APPCOMMAND_VOLUME_UP));
            SendMessageW(handle, WM_APPCOMMAND, handle, new IntPtr(APPCOMMAND_VOLUME_MUTE));
        }

        public static UnmanagedMemoryStream default_coin_drop
        {
            get
            {
                return Resources.default_coin_drop;
            }
            
        }
        public static UnmanagedMemoryStream default_time_out
        {
            get
            {
                return Resources.default_time_out;
            }

        }
        public static UnmanagedMemoryStream default_warning
        {
            get
            {
                return Resources.default_warning;
            }

        }
        public static UnmanagedMemoryStream super_mario_coin_drop
        {
            get
            {
                return Resources.super_mario_coin_drop;
            }

        }
        public static UnmanagedMemoryStream super_mario_warning
        {
            get
            {
                return Resources.super_mario_warning;
            }

        }
        public static UnmanagedMemoryStream super_mario_time_out
        {
            get
            {
                return Resources.super_mario_time_out;
            }

        }
        public static UnmanagedMemoryStream super_mario_time_pause
        {
            get
            {
                return Resources.super_mario_time_pause;
            }

        }
        public static void Play(Stream sound)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.Stream = sound;
            player.Play();
        }
    }
}
