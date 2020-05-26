using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace coinapplib
{
    public class Reg
    {
        #region Startup configurations

        public static bool StartupRegistryExists
        {
            get
            {
                if (StartupRegkey.GetValueNames().Length != 0)
                {
                    for (int i = 0; i <= StartupRegkey.GetValueNames().Length - 1; i++)
                    {
                        if (StartupRegkey.GetValueNames()[i] == Path.GetFileNameWithoutExtension(Application.ExecutablePath))
                            return true;
                    }
                }
                return false;
            }
        }

        public static bool ValidStartupValue
        {
            get
            {
                string value = Convert.ToString(StartupRegkey.GetValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath)));

                for (int i = 0; i <= StartupRegkey.GetValueNames().Length - 1; i++)
                {
                    if (StartupRegkey.GetValueNames()[i] == Path.GetFileNameWithoutExtension(Application.ExecutablePath))
                    {
                        if (value == Application.ExecutablePath + " /boot")
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private static RegistryKey StartupRegkey
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                    return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", true);
                else
                    return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            }
        }

        private static string StartupSubkey
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                    return @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Run";
                else
                    return @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            }
            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run
            //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
            //Computer\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
        }

        public static bool AddStartupRegistry
        {
            get
            {
                try
                {
                    StartupRegkey.SetValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath), Application.ExecutablePath + " /boot");
                }
                catch
                {
                    return false;
                    //throw new Exception();
                }
                return true;
            }
        }

        public static bool DeleteStartupRegistry
        {
            get
            {
                try
                {
                    StartupRegkey.DeleteValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath));
                }
                catch
                {
                    return false;
                    //throw new Exception();
                }
                return true;
            }
        }

        #endregion

        #region Registry taskmanager configurations
        public static bool ValidDisableTaskmgrValue
        {
            get
            {
                if (TaskmgrRegistryKey.GetValue("DisableTaskMgr").ToString() == null) return false;

                if (!int.TryParse(TaskmgrRegistryKey.GetValue("DisableTaskMgr").ToString(), out int value)) return false;

                for (int i = 0; i <= TaskmgrRegistryKey.GetValueNames().Length - 1; i++)
                {
                    if (TaskmgrRegistryKey.GetValueNames()[i] == "DisableTaskMgr")
                    {
                        if (value < 10)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public static bool DisableTaskmgrRegistryExists //////////////////////////ERROR OCCUREDS
        {
            get
            {
                try
                {
                    if (TaskmgrRegistryKey.GetValueNames().Length != 0)
                    {
                        for (int i = 0; i <= TaskmgrRegistryKey.GetValueNames().Length - 1; i++)
                        {
                            if (TaskmgrRegistryKey.GetValueNames()[i] == "DisableTaskMgr") return true;
                            else return false;
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool AddDisableTaskmgr
        {
            get
            {
                try
                {
                    TaskmgrRegistryKey.SetValue("DisableTaskMgr", "1");
                }
                catch
                {
                    return false;
                    //throw new Exception();
                }

                return true;
            }
        }

        public static bool DeleteDisableTaskmgr
        {
            get
            {
                try
                {
                    TaskmgrRegistryKey.DeleteValue("DisableTaskMgr");
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }

        private static RegistryKey TaskmgrRegistryKey
        {
            get
            {
                RegistryKey userkey = Registry.Users;
                RegistryKey regkey = userkey.OpenSubKey(Users.CurrentLoggedSidUser + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\", true);

                bool found = false;

                for (int i = 0; i <= regkey.GetSubKeyNames().Length - 1; i++)
                {
                    if (regkey.GetSubKeyNames()[i] == "System")
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    regkey.CreateSubKey("System");
                }

                return userkey.OpenSubKey(Users.CurrentLoggedSidUser + @"\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\", true);
            }
        }
        #endregion


        #region Registry service
        public static bool IsValidServicePath(string serviceName, out string imagePath)
        {
            RegistryKey regkey = serviceKey(serviceName);
            //regkey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName));

            if (regkey.GetValue("ImagePath") == null) //Check if there is a service "coinapp" installed (coinappSvc.exe)
            {
                imagePath = string.Empty;
                return false; //No service found exit and return false.
            }
            else //Service found. Check the path if its valid.
            {
                //Delete the quotation mark at start and end of the value of the ImagePath of the service and compare to the path of this coinappSvc.exe.
                if (regkey.GetValue("ImagePath").ToString().Trim('"').ToLower() == (AppDomain.CurrentDomain.BaseDirectory + "coinappSvc.exe").ToLower())
                {
                    imagePath = regkey.GetValue("ImagePath").ToString();
                    return true;
                }

                //Delete the quotation mark at start and end of the value of the ImagePath of the service and compare the starting string until to the path of this coinappSvc.exe (removes the arguments that have got).
                if (regkey.GetValue("ImagePath").ToString().Trim('"').ToLower().StartsWith((AppDomain.CurrentDomain.BaseDirectory + "coinappSvc.exe").ToLower()))
                {
                    imagePath = regkey.GetValue("ImagePath").ToString();
                    return true;
                }

                imagePath = regkey.GetValue("ImagePath").ToString();
                return false;
            }

        }

        public static string ServiceStartType(string serviceName)
        {
            RegistryKey regkey = serviceKey(serviceName);
            if (regkey.GetValue("Start") == null) return string.Empty;
            string value = regkey.GetValue("Start").ToString();

            switch (value)
            {
                case "1": return "AutomaticDelayed";
                case "2": return "Automatic";
                case "3": return "Manual";
                case "4": return "Disabled";
                default: return "Undefined";
            }
        }
        
        private static RegistryKey serviceKey(string serviceName)
        {
            return Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName));
        }
        #endregion


        #region Registry wallpaper
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tile,
            Fill,
            Fit,
            Span,
            Stretch,
            Center
        }

        public static string Get
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.Users;
                    key = key.OpenSubKey(Users.CurrentLoggedSidUser + @"\Control Panel\Desktop", false);
                    return key.GetValue(@"Wallpaper").ToString();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }

        }

        public static void Set(string imgPath, Style style = Style.Fill)
        {
            RegistryKey key = Registry.Users;
            key = key.OpenSubKey(Users.CurrentLoggedSidUser + @"\Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", 2.ToString());
            key.SetValue(@"TileWallpaper", 0.ToString());


            //if (style == Style.Fill)
            //{
            //    key.SetValue(@"WallpaperStyle", 10.ToString());
            //    key.SetValue(@"TileWallpaper", 0.ToString());
            //}
            //if (style == Style.Fit)
            //{
            //    key.SetValue(@"WallpaperStyle", 6.ToString());
            //    key.SetValue(@"TileWallpaper", 0.ToString());
            //}
            //if (style == Style.Span) // Windows 8 or newer only!
            //{
            //    key.SetValue(@"WallpaperStyle", 22.ToString());
            //    key.SetValue(@"TileWallpaper", 0.ToString());
            //}
            //if (style == Style.Stretch)
            //{
            //    key.SetValue(@"WallpaperStyle", 2.ToString());
            //    key.SetValue(@"TileWallpaper", 0.ToString());
            //}
            //if (style == Style.Tile)
            //{
            //    key.SetValue(@"WallpaperStyle", 0.ToString());
            //    key.SetValue(@"TileWallpaper", 1.ToString());
            //}
            //if (style == Style.Center)
            //{
            //    key.SetValue(@"WallpaperStyle", 0.ToString());
            //    key.SetValue(@"TileWallpaper", 0.ToString());
            //}

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                imgPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        #endregion

    }

}
