using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;

namespace coinapplib
{
    public class Settings
    {
        #region Variables

        public const string savedataPath = "sp";
        public const string savedataFile = "sv";
        public const string emailFile = "em";
        public const string passFile = "pw";

        public const string sysDir = @"C:\Windows\ppanioc\";
        private const string file = "coinapp.ini";
        private static readonly string dir = Application.StartupPath + @"\";
        private static readonly StringBuilder sb = new StringBuilder(500);
        
        #endregion

        #region DLLImports

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        [DllImport("kernel32")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        #endregion

        #region Public Functions

        //Check the coinapp.ini in the application folder and return a boolean value.
        public static bool ConfigurationExists { get { return File.Exists(dir + file); } }

        //Check the file from the system folder of the application (c:\windows\ppanioc\) and return a boolean value.
        public static bool SystemConfigurationExist(string file) { return File.Exists(sysDir + file); }
        public static string ReadSysFile(string file)
        {
            try
            {
                StreamReader reader = File.OpenText(sysDir + file);
                string stringReader = reader.ReadLine();
                reader.Close();
                return stringReader;
            }
            catch (Exception e)
            {
                Logs.System("ReadSysFile : " + e.Message, Application.ExecutablePath + @"\", true);
                return string.Empty;
                //throw new Exception();
            }
        }
        public static string ReadCoin(string path, string file)
        {
            try
            {
                //StreamReader reader = File.OpenText(path + file);
                //string stringReader = reader.ReadLine();
                //reader.Close();
                //return stringReader;
                string lastLine = File.ReadLines(path + file).Last();

                return lastLine;
            }
            catch (Exception e)
            {
                Logs.System("ReadCoin : " + e.Message, Application.ExecutablePath + @"\", true);
                return string.Empty;
                //throw new Exception();
            }
        }
        public static bool WriteSysFile(string file, string text)
        {
            if (!Directory.Exists(sysDir))
            {
                try { Directory.CreateDirectory(sysDir); }
                catch{ return false; }
            }
            
            try
            {
                using (StreamWriter writer = new StreamWriter(sysDir + file))
                {
                    writer.WriteLine(text);
                    writer.Close();
                    return true;
                }
            }
            catch { return false; }
        }
        public static bool WriteCoin(string path, string coin)
        {
            path = path.EndsWith(@"\") ? path : path + @"\";

            try
            {
                using (StreamWriter writer = new StreamWriter(path + savedataFile, true))
                {
                    writer.WriteLine(coin);
                    writer.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                Logs.System("WriteCoin : " + e.Message, Application.ExecutablePath + @"\", true);
                return false;
                //throw new Exception();
            }
        }
        public static void CreateDefaultConfigFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(dir + file, true))
                {
                    writer.WriteLine("[device]");
                    writer.WriteLine("portnumber=1");
                    writer.WriteLine("baudrate=19200");
                    writer.WriteLine("coinslot=0");
                    writer.WriteLine("interval=1000");
                    writer.WriteLine("");
                    writer.WriteLine("[coins]");
                    writer.WriteLine("coin1=240");
                    writer.WriteLine("coin2=0");
                    writer.WriteLine("coin3=0");
                    writer.WriteLine("pulse1=0");
                    writer.WriteLine("pulse2=0");
                    writer.WriteLine("pulse3=0");
                    writer.WriteLine("");
                    writer.WriteLine("[apps]");
                    writer.WriteLine("shutdown=120");
                    writer.WriteLine("loginattempt=3");
                    writer.WriteLine("hotkeycode=38");
                    writer.WriteLine("timer=1");
                    writer.WriteLine("sound=1");
                    writer.WriteLine("savedata=0");
                    writer.WriteLine("log=0");
                    writer.WriteLine("lockimage=");
                    writer.Close();
                }
            }
            catch
            {
                throw new IOException();
            }
        }

        #endregion

        #region Device configurations

        public static int PortNumber
        {
            get
            {
                GetPrivateProfileString("device", "portnumber", "1", sb, sb.Capacity, dir + file).ToString();

                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 1;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 1;
                    }
                    return Convert.ToInt32(sb.ToString());
                }

            }
            
        }

        public static void SetPortNumber(int portnumber = 1)
        {
            WritePrivateProfileString("device", "portnumber", portnumber.ToString(), dir + file);
        }

        public static int BaudRate
        {
            get
            {
                GetPrivateProfileString("device", "baudrate", "9600", sb, sb.Capacity, dir + file);

                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 9600;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 9600;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetBaudRate(int baudrate = 9600)
        {
            WritePrivateProfileString("device", "baudrate", baudrate.ToString(), dir + file);
        }

        public static int CoinSlot
        {
            get
            {
                GetPrivateProfileString("device", "coinslot", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" )
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    else if (Convert.ToInt32(sb.ToString()) > 3)
                    {
                        return 3;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetCoinSlot(int coinslot = 0)
        {
            WritePrivateProfileString("device", "coinslot", coinslot.ToString(), dir + file);
        }

        public static int Interval
        {
            get
            {
                GetPrivateProfileString("device", "interval", "1000", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 1000;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 1000;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetInterval(int milliseconds = 1000)
        {
            WritePrivateProfileString("device", "interval", milliseconds.ToString(), dir + file);
        }

        #endregion

        #region Coins & Pulse configurations

        public static int Coin1
        {
            get
            {
                GetPrivateProfileString("coins", "coin1", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetCoin1(int seconds = 0)
        {
            WritePrivateProfileString("coins", "coin1", seconds.ToString(), dir + file);
        }

        public static int Coin2
        {
            get
            {
                GetPrivateProfileString("coins", "coin2", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetCoin2(int seconds = 0)
        {
            WritePrivateProfileString("coins", "coin2", seconds.ToString(), dir + file);
        }

        public static int Coin3
        {
            get
            {
                GetPrivateProfileString("coins", "coin3", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetCoin3(int seconds = 0)
        {
            WritePrivateProfileString("coins", "coin3", seconds.ToString(), dir + file);
        }

        public static int Pulse1
        {
            get
            {
                GetPrivateProfileString("coins", "pulse1", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetPulse1(int pulse = 0)
        {
            WritePrivateProfileString("coins", "pulse1", pulse.ToString(), dir + file);
        }

        public static int Pulse2
        {
            get
            {
                GetPrivateProfileString("coins", "pulse2", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetPulse2(int pulse = 0)
        {
            WritePrivateProfileString("coins", "pulse2", pulse.ToString(), dir + file);
        }

        public static int Pulse3
        {
            get
            {
                GetPrivateProfileString("coins", "pulse3", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetPulse3(int pulse = 0)
        {
            WritePrivateProfileString("coins", "pulse3", pulse.ToString(), dir + file);
        }

        #endregion

        #region Application configurations

        public static int HotkeyCode
        {
            get
            {
                GetPrivateProfileString("apps", "hotkeycode", "38", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 38;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 38;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetHotkeyCode(int keycode = 38)
        {
            WritePrivateProfileString("apps", "hotkeycode", keycode.ToString(), dir + file);
        }

        public static int Shutdown
        {
            get
            {
                GetPrivateProfileString("apps", "shutdown", "20", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 20;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 20;
                    }
                    else if (Convert.ToInt32(sb.ToString()) > 300)
                    {
                        return 300;

                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
            
        }

        public static void SetShutdown(int seconds = 20)
        {
            WritePrivateProfileString("apps", "shutdown", seconds.ToString(), dir + file);
        }

        public static int LoginAttempt
        {
            get
            {
                GetPrivateProfileString("apps", "loginattempt", "3", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 3;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 3;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetLoginAttempt(int retry = 3)
        {
            WritePrivateProfileString("apps", "loginattempt", retry.ToString(), dir + file);
        }

        public static int Timer
        {
            get
            {
                GetPrivateProfileString("apps", "timer", "1", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "0" || sb.ToString() == "")
                    return 0;
                else
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 1;
                    }
                    return Convert.ToInt32(sb.ToString());
            }
        }

        public static void SetTimer(int val = 1)
        {
            WritePrivateProfileString("apps", "timer", val.ToString(), dir + file);
        }

        public static int Sound
        {
            get
            {
                GetPrivateProfileString("apps", "sound", "1", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "0" || sb.ToString() == "")
                    return 0;
                else
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 1;
                    }
                    return Convert.ToInt32(sb.ToString());
            }
        }

        public static void SetSound(int val = 1)
        {
            WritePrivateProfileString("apps", "sound", val.ToString(), dir + file);
        }

        public static string LockImage
        {
            get
            {
                GetPrivateProfileString("apps", "lockimage", string.Empty, sb, sb.Capacity, dir + file);
                return sb.ToString();
            }
        }

        public static void SetLockImage(string path = "")
        {
            WritePrivateProfileString("apps", "lockimage", path, dir + file);
        }

        public static int SaveData
        {
            get
            {
                GetPrivateProfileString("apps", "savedata", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetSavedata(int val = 0)
        {
            WritePrivateProfileString("apps", "savedata", val.ToString(), dir + file);
        }

        public static int Log
        {
            get
            {
                GetPrivateProfileString("apps", "log", "0", sb, sb.Capacity, dir + file);
                if (sb.ToString() == "" || sb.ToString() == "0")
                    return 0;
                else
                {
                    if (!int.TryParse(sb.ToString(), out _))
                    {
                        return 0;
                    }
                    return Convert.ToInt32(sb.ToString());
                }
            }
        }

        public static void SetLog(int val = 0)
        {
            WritePrivateProfileString("apps", "log", val.ToString(), dir + file);
        }

        #endregion

    }
}
