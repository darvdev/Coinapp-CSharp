using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace coinapplib
{
    public class HotkeyCode
    {
        private const int MOD_ALT = 0x1;
        private const int MOD_CONTROL = 0x2;
        private const int MOD_SHIFT = 0x4;

        [DllImport("user32")]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [DllImport("user32")]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);


        public void Register(IntPtr handle, int key)
        {
            RegisterHotKey(handle, 100, MOD_CONTROL | MOD_ALT | MOD_SHIFT, key);
        }

        public void Unregister(IntPtr handle)
        {
            UnregisterHotKey(handle, 100);
        }

        private const int WM_HOTKEY = 0x312;
        public event KeyDownEventHandler KeyDown;
        public delegate void KeyDownEventHandler(object sender, EventArgs e);

        public void ProcessWinMessageHotkey(int msg, IntPtr wParam)
        {
            if (msg == WM_HOTKEY)
            {
                switch ((int)wParam)
                {
                    case 100:
                        {
                            KeyDown?.Invoke(this, new EventArgs());
                            break;
                        }
                }
            }
        }

        public static string ToString(int keycode)
        {
            switch (keycode)
            {
                case 65: return "A";
                case 66: return "B";
                case 67: return "C";
                case 68: return "D";
                case 69: return "E";
                case 70: return "F";
                case 71: return "G";
                case 72: return "H";
                case 73: return "I";
                case 74: return "J";
                case 75: return "K";
                case 76: return "L";
                case 77: return "M";
                case 78: return "N";
                case 79: return "O";
                case 80: return "P";
                case 81: return "Q";
                case 82: return "R";
                case 83: return "S";
                case 84: return "T";
                case 85: return "U";
                case 86: return "V";
                case 87: return "W";
                case 88: return "X";
                case 89: return "Y";
                case 90: return "Z";
                case 48: return "0";
                case 49: return "1";
                case 50: return "2";
                case 51: return "3";
                case 52: return "4";
                case 53: return "5";
                case 54: return "6";
                case 55: return "7";
                case 56: return "8";
                case 57: return "9";
                case 192: return "`";
                case 189: return "-";
                case 187:return "=";
                case 8: return "Backspace";
                case 32: return "Space";
                case 27: return "Esc";
                case 13: return "Enter";
                case 144:return "Num Lock";
                case 106: return "Num *";
                case 107: return "Num +";
                case 109: return "Num -";
                
                case 111: return "Num /";
                case 219: return "[";
                case 220: return @"\";
                case 221: return "]";
                case 222: return "'";
                case 186: return ";";
                case 188: return ",";
                case 190: return ".";
                case 191: return "/";
                case 33: return "Page Up";
                case 34: return "Page Down";
                case 35: return "End";
                case 36: return "Home";
                case 37: return "Left Arrow";
                case 38: return "Up Arrow";
                case 39: return "Right Arrow";
                case 40: return "Down Arrow";
                case 20: return "CapsLock";
                case 93: return "Apps";
                case 45: return "Insert";
                case 46: return "Delete";
                case 145: return "Scroll Lock";
                case 112: return "F1";
                case 113: return "F2";
                case 114: return "F3";
                case 115: return "F4";
                case 116: return "F5";
                case 117: return "F6";
                case 118: return "F7";
                case 119: return "F8";
                case 120: return "F9";
                case 121: return "F10";
                case 122: return "F11";
                case 123: return "F12";

                ///// Cannot use for Hot key
                case 96: return "Num 0";
                case 97: return "Num 1";
                case 98: return "Num 2";
                case 99: return "Num 3";
                case 100: return "Num 4";
                case 101: return "Num 5";
                case 102: return "Num 6";
                case 103: return "Num 7";
                case 104: return "Num 8";
                case 105: return "Num 9";
                case 110: return "Num .";
                case 19: return "Pause Break";
                case 91: return "Left Windows Key";
                case 92: return "Right Windows Key";
                case 9: return "Tab";
                case 18: return "ALT";
                case 17: return "CTRL";
                case 16: return "SHIFT";

                default: return string.Empty;
            }
        }
    }
}
