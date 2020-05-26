using System;
using System.Diagnostics;
using System.IO;

namespace coinapplib
{
    public class My
    {
        public static bool SystemConfigurationExists
        {
            get
            {
                if (!Settings.SystemConfigurationExist(Settings.passFile) || !Settings.SystemConfigurationExist(Settings.emailFile))
                    return false;
                return true;
            }
        }
        public static bool SavedataPathExists(out string path)
        {

            if (!Settings.SystemConfigurationExist(Settings.savedataPath))
            {
                path = string.Empty;
                return false;
            }

            string savedataPath = Settings.ReadSysFile(Settings.savedataPath);

            if (savedataPath == string.Empty)
            {
                path = string.Empty;
                return false;
            }

            DirectoryInfo di = new DirectoryInfo(savedataPath);

            if (!di.Exists)
            {
                path = string.Empty;
                return false;
            }

            path = savedataPath.EndsWith(@"\") ? savedataPath : savedataPath + @"\";
            return true;
        }
        public static bool RestoreTimeResume(out int credits)
        {
            if (!SavedataPathExists(out string savedataPath))
            {
                credits = 0;
                return false;
            }

            FileInfo fi = new FileInfo(savedataPath + Settings.savedataFile);

            if (!fi.Exists)
            {
                credits = 0;
                return false;
            }

            string coin = Settings.ReadCoin(savedataPath, Settings.savedataFile);

            if (int.TryParse(coin, out int rt))
            {
                credits = rt;
                return true;
            }
            else
            {
                credits = 0;
                return false;
            }
        }
        public static bool WriteTimeResume(int credits)
        {
            if (!SavedataPathExists(out string savedataPath))
                return false;

            savedataPath = savedataPath.EndsWith(@"\") ? savedataPath : savedataPath + @"\";

            return Settings.WriteCoin(savedataPath, credits.ToString());
        }
        
        public static bool ClearTimeResume()
        {
            if (!SavedataPathExists(out string savedataPath))
                return false;

            savedataPath = savedataPath.EndsWith(@"\") ? savedataPath : savedataPath + @"\";

            try
            {
                File.WriteAllText(savedataPath + Settings.savedataFile, string.Empty);
                //return true;
            }
            catch
            {
                return false;
            }

            try
            {
                File.Delete(savedataPath + Settings.savedataFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string Password
        {
            get
            {
                if (!Settings.SystemConfigurationExist(Settings.passFile))
                    return string.Empty;

                try { return Settings.ReadSysFile(Settings.passFile); }
                catch { return string.Empty; }
            }
            set
            {
                Settings.WriteSysFile(Settings.passFile, value);
            }
        }
        public static string Email
        {
            get
            {
                if (!Settings.SystemConfigurationExist(Settings.emailFile))
                    return string.Empty;

                try { return Settings.ReadSysFile(Settings.emailFile); }
                catch { return string.Empty; }
            }
            set
            {
                Settings.WriteSysFile(Settings.emailFile, value);
            }
        }
        //Recovery options of this service.Sets after the service is installed.
        public static void SetRecoveryOptions(string serviceName)
        {
            int exitCode;
            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = process.StartInfo;
                startInfo.FileName = "sc"; //sc.exe
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // tell Windows that the service should restart if it fails
                startInfo.Arguments = string.Format("failure \"{0}\" reset= 0 actions= reboot/0", serviceName);
                process.Start();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
            {
                throw new InvalidOperationException();
            }
        }
        //Add log to Windows Event Log Manager
        public static void WriteEventLog(string EventMessage, EventLogEntryType EntryType = EventLogEntryType.Information)
        {
            if (!EventLog.SourceExists("Coinapp")) EventLog.CreateEventSource("Coinapp", "System");
            EventLog.WriteEntry("Coinapp", EventMessage, EntryType);
        }
    }
    public class CONFIG
    {
        private int _PORTNUMBER = 1;
        private int _BAUDRATE = 19200;
        private int _COINSLOT = 0;
        private int _COIN1 = 0;
        private int _PULSE1 = 0;
        private int _COIN2 = 0;
        private int _PULSE2 = 0;
        private int _COIN3 = 0;
        private int _PULSE3 = 0;
        private int _SHUTDOWN = 20;
        private int _LOGINATTEMPT = 3;
        private int _HOTKEYCODE = 38;
        private int _TIMER = 0;
        private int _SOUND = 0;
        private int _INTERVAL = 1000;
        private int _SAVEDATA = 0;
        private string _LOCKIMAGE = string.Empty;
        private int _LOG = 0;

        public CONFIG()
        {
            _PORTNUMBER = Settings.PortNumber;
            _BAUDRATE = Settings.BaudRate;
            _COINSLOT = Settings.CoinSlot;
            _COIN1 = Settings.Coin1;
            _PULSE1 = Settings.Pulse1;
            _COIN2 = Settings.Coin2;
            _PULSE2 = Settings.Pulse2;
            _COIN3 = Settings.Coin3;
            _PULSE3 = Settings.Pulse3;
            _SHUTDOWN = Settings.Shutdown;
            _LOGINATTEMPT = Settings.LoginAttempt;
            _HOTKEYCODE = Settings.HotkeyCode;
            _TIMER = Settings.Timer;
            _SOUND = Settings.Sound;
            _INTERVAL = Settings.Interval;
            _SAVEDATA = Settings.SaveData;
            _LOCKIMAGE = Settings.LockImage;
            _LOG = Settings.Log;
        }

        public int PORTNUMBER { get { return _PORTNUMBER; } }
        public int BAUDRATE { get { return _BAUDRATE; } }
        public int COINSLOT { get { return _COINSLOT; } }
        public int COIN1 { get { return _COIN1; } }
        public int COIN2 { get { return _COIN2; } }
        public int COIN3 { get { return _COIN3; } }
        public int PULSE1 { get { return _PULSE1; } }
        public int PULSE2 { get { return _PULSE2; } }
        public int PULSE3 { get { return _PULSE3; } }
        public int SHUTDOWN { get { return _SHUTDOWN; } }
        public int LOGINATTEMPT { get { return _LOGINATTEMPT; } }
        public int HOTKEYCODE { get { return _HOTKEYCODE; } }
        public int TIMER { get { return _TIMER; } }
        public int SOUND { get { return _SOUND; } }
        public int INTERVAL { get { return _INTERVAL; } }
        public int SAVEDATA { get { return _SAVEDATA; } }
        public string LOCKIMAGE { get { return _LOCKIMAGE; } }
        public int LOG { get { return _LOG; } }
    }
}
