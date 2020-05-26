using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net.Mail;
using coinapplib;
using System.Xml.Linq;
using System.Net;
using System.Threading.Tasks;

namespace coinapp
{
    public class c
    {
        public static Main app;
        public static ClientPipe mainPipe;

        public static List<int> ecode = new List<int>(); //Storage of all device/application error codes.
        public static readonly int[] BAUDRATE = { 4800, 9600, 19200, 38400, 57600, 74880, 115200, 230400, 250000 };
        public static readonly int[] COINSLOT = { 0, 1, 2, 3 };
        public static int SESSION = 0;
        public static int ATTEMPT = 0;
        public static int UPDATES = 0;
        public static int REMAINING_TIME = 0;

        public static bool STARTUP_INSTALL = false;
        public static bool ADMINISTRATOR = false;
        //public static bool DESKTOP_READY = false;
        public static bool INITIALIZED = false;
        public static bool CONNECTED = false;
        public static bool LOCKED = false;
        public static bool STOP = false;
        
        public static string CURRENT_DEVICE_VERSION = string.Empty;
        public static string NEW_DEVICE_VERSION = string.Empty;
        public static string NEW_SOFTWARE_VERSION = string.Empty;

        public const string serviceName = "Coinapp";
        public readonly static string XmlPath = Application.StartupPath + @"\coinapp.xml";
        public static SESSION S;
        public static Bitmap LOCKIMAGE = null;
        public static bool ADDLOG { get; set; } = false;
        public static string LOGPATH { get; set; } = Application.StartupPath + @"\";
        public static void AppXmlParser()
        {
            XDocument doc = new XDocument();
            string rootLocalName = string.Empty;
            string rootAttribute = string.Empty;
            try
            {
                doc = XDocument.Load(XmlPath);
                rootLocalName = doc.Root.Name.LocalName;
                rootAttribute = doc.Root.Attribute("name").Value;
            }
            catch { }

            if (rootLocalName != "configuration") return;
            if (rootAttribute != "coinapp") return;
            if (doc.Root.Elements("system").Count() == 0) return;
            if (doc.Root.Element("system").Elements().Count() == 0) return;

            try
            {
                if (doc.Root.Element("system").Element("AddLog").Value == "true")
                    ADDLOG = true;
                string logPath = doc.Root.Element("system").Element("LogPath").Value;

                if (logPath != string.Empty || Directory.Exists(logPath))
                    LOGPATH = logPath.EndsWith(@"\") ? logPath : logPath + @"\";
            }
            catch {}
        } 
        public static string CoinslotToString(int coinslot)
        {
            switch (coinslot)
            {
                case 0: return "LEGACY";
                //This is advanced coin-slot configuration.
                case 1: return "SINGLE";
                case 2: return "DUAL";
                case 3: return "MULTI"; 
                default: return "INVALID";
            }
        }

        public static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
        }

        public static string PreviousSaveDataFolderPath
        {
            get
            {
                try { return Settings.ReadSysFile(Settings.savedataPath); }
                catch { return string.Empty; }
            }

        }

        public static bool SaveDataFolder(IWin32Window form ,out string folderPath)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog
            {

                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = Application.ExecutablePath,
                Description = "[ADVANCED] Set a folder location for SAVE DATA"
            };
            
            if (folderDialog.ShowDialog(form) != DialogResult.OK)
            {
                folderDialog.Dispose();
                folderPath = "Selecting SAVE DATA folder location canceled.";
                return false;
            }
            else
            {
                folderPath = folderDialog.SelectedPath.EndsWith(@"\") ? folderDialog.SelectedPath : folderDialog.SelectedPath + @"\";
                folderDialog.Dispose();
                return true;
            }
        }

        public static bool LockImagePath(IWin32Window form, out string filePath)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                InitialDirectory = Application.StartupPath, // Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                Title = "[ADVANCED] Select image file for LOCK IMAGE",
                Filter = "ALL|*.jpg;*.jpeg;*.png;*.bmp|JPG|*.jpg|JPEG|*.jpeg|PNG|*.png|BMP|*.bmp",
            };

            Bitmap bmp;

            if (fileDialog.ShowDialog(form) != DialogResult.OK)
            {
                filePath = "Selecting LOCK IMAGE file canceled.";
                return false;
            }
            else
            {
                try { bmp = new Bitmap(fileDialog.FileName); }
                catch
                {
                    filePath = "Selected image file is not a valid JPEG file.";
                    return false;
                }
                filePath = Path.GetFullPath(fileDialog.FileName);
                bmp.Dispose();
                fileDialog.Dispose();
                return true;
            }
        }

        public static string TimeToString(int time)
        {
            int s = time % 60;
            int m = (int)((time - s) / (double)60 % 60);
            int h = (int)((time - (s + m * 60)) / (double)3600 % 60);
            return (h == 0 ? "" : h + (h > 1 ? " hours " : " hour ")) + (m == 0 ? "" : m + (m > 1 ? " minutes " : " minute ")) + (s == 0 ? "" : s + (s > 1 ? " seconds" : " second"));
        }

        public static bool FormExists(string formName)
        {
            //FormCollection fc = Application.OpenForms;
            //foreach (Form frm in fc)
            //{
            //    if (frm.Name == formname)
            //    {
            //        return true;
            //    }
            //}
            //return false;

            return Application.OpenForms.Cast<Form>().Any(form => form.Name == formName);
        }

        public static bool RecoverPassword(string email, string pass, out string message)
        {
            MailMessage Mail = new MailMessage();
            SmtpClient SMTP = new SmtpClient(Recovery.host, Recovery.port);
            SMTP.EnableSsl = true;
            SMTP.Timeout = 10000;
            SMTP.Credentials = new NetworkCredential(Recovery.email, Recovery.pass);
            Mail.Subject = Recovery.subject;
            Mail.IsBodyHtml = true;

            if (Recovery.body == string.Empty)
            {
                Mail.Body = Application.ProductName + " " + Application.ProductVersion + "<br />Your password: " + pass + "<br />" + DateTime.Now;
            }
            else
            {
                Mail.Body = string.Format(Recovery.body, Application.ProductName, Application.ProductVersion, pass, DateTime.Now);
            }
            

            Mail.From = new MailAddress(Recovery.email);
            Mail.To.Add(email);
            try
            {
                SMTP.Send(Mail);
            }
            catch (Exception ex)
            {
                message =  ex.Message;
                Mail.Dispose();
                return false;
            }

            Mail.Dispose();
            message = "We've sent your password to "+ email + "\nPlease check your email now.";
            return true;
        }

        public static string TimeResumeToString(int time, bool show = false)
        {
            int s = time % 60;
            int m = (time - s) / 60 % 60;
            int h = (time - (s + m * 60)) / 3600 % 60;
            if (!show) return "Yippee! There are " + (h > 0 ? (h).ToString() + " " + (h == 1 ? "hour " : "hours ") : "") + (m > 0 ? (m).ToString() + " " + (m == 1 ? "minute " : "minutes ") : "") + (s > 0 ? (s).ToString() + " " + (s == 1 ? "second " : "seconds ") : "") + "time to be resume.\n\nDO YOU WANT TO USE IT NOW?";
            else return (h > 0 ? (h).ToString() + " " + (h == 1 ? "hour " : "hours ") : "") + (m > 0 ? (m).ToString() + " " + (m == 1 ? "minute " : "minutes ") : "") + (s > 0 ? (s).ToString() + " " + (s == 1 ? "second " : "seconds") : "");

        }

        public async static Task<string> iConsoleMessage()
        {
            const string csUrl = "https://dl.dropboxusercontent.com/s/cud29003kcmftec/cs.xml";
            string csXml = string.Empty;

            try {
                var webClient = new WebClientEx();
                webClient.Timeout = 3000;
                csXml = await webClient.DownloadStringTaskAsync(csUrl); //webClient.DownloadData(consoleUrl);                   
                webClient.Dispose();
            }
            catch { return string.Empty; }

            XDocument doc = new XDocument();
            string rootLocalName = string.Empty;
            string rootAttribute = string.Empty;
            try {
                doc = XDocument.Parse(csXml);
                rootLocalName = doc.Root.Name.LocalName;
                rootAttribute = doc.Root.Attribute("name").Value;
            } catch { return string.Empty; }

            if (rootLocalName != "console") return string.Empty;
            if (rootAttribute != "coinapp") return string.Empty;

            if (doc.Root.Elements("message").Count() == 0) return string.Empty;

            string message = string.Empty;

            try { message = doc.Root.Element("message").Value; } catch { return string.Empty; }

            return message;
        }
    
        public static async Task EmailData()
        {
            const string emUrl = "https://dl.dropboxusercontent.com/s/a47b69cirgoxxpl/em.xml";
            string emXml = string.Empty;

            try
            {
                var webClient = new WebClientEx();
                webClient.Timeout = 5000;
                emXml = await webClient.DownloadStringTaskAsync(emUrl); //webClient.DownloadData(consoleUrl);                   
                webClient.Dispose();
            }
            catch { return; }

            XDocument doc = new XDocument();
            string rootLocalName = string.Empty;
            string rootAttribute = string.Empty;
            try
            {
                doc = XDocument.Parse(emXml);
                rootLocalName = doc.Root.Name.LocalName;
                rootAttribute = doc.Root.Attribute("name").Value;
            }
            catch { return; }

            if (rootLocalName != "email") return;
            if (rootAttribute != "coinapp") return;

            if (doc.Root.Elements().Count() == 0) return;

            string host;
            int port;
            string email;
            string pass;
            string subject;
            string body;

            try { host = doc.Root.Element("host").Value; } catch { return; }
            try { port = Convert.ToInt32(doc.Root.Element("port").Value); } catch { return; }
            try { email = doc.Root.Element("userName").Value; } catch { return; }
            try { pass = doc.Root.Element("password").Value; } catch { return; }
            try { subject = doc.Root.Element("subject").Value; } catch { return; }
            try { body = doc.Root.Element("body").Value; } catch { return; }

            Recovery.host = host;
            Recovery.port = port;
            Recovery.email = email;
            Recovery.pass = pass;
            Recovery.subject = subject;
            Recovery.body = body;
        }
    }

    public class COMM
    {
        #region INSTALL SEQUENCE
        public const int COMPORT = 1;
        public const int BAUDRATE = 2; //Advance config
        public const int COINSLOT = 3;
        public const int COIN1 = 4;
        public const int PULSE1 = 5; //Advance config
        public const int COIN2 = 6;
        public const int PULSE2 = 7; //Advance config
        public const int COIN3 = 8;
        public const int PULSE3 = 9; //Advance config
        public const int SHUTDOWN = 10;
        public const int LOGINATTEMPT = 11;
        public const int HOTKEYCODE = 12;
        public const int EMAIL = 13;
        public const int PASS = 14;

        //This is ADVANCE CONFIGURATIONS
        public const int TIMER = 15;
        public const int SOUND = 16;
        public const int INTERVAL = 17;

        public const int SAVEDATA = 18;
        public const int LOCKIMAGE = 19;
        //public const int UPDATE = 20;
        public const int SHOW_SETTINGS = 20;
        public const int SAVE_SETTINGS = 21;

        public const int ADD_REGISTRY = 22;
        public const int SERVICE_INSTALL = 23;
        #endregion

        //Installation commands
        public const string INSTALL = "INSTALL";
        
        //Main commands
        public const string APP = "APP";
        public const string DEV = "DEV";
        public const string SVC = "SVC";
        public const string CLS = "CLS";
        public const string MAIN = "MAIN";
        public const string HELP = "HELP";
        public const string COINAPP = "COINAPP";
        public const string SERIAL = "SERIAL";
        //configs
        public const string
            SD = "SHUTDOWN",
            HK = "HOTKEY",
            PA = "PASS",
            EM = "EMAIL",
            LA = "ATTEMPT",
            LI = "LOCKIMAGE",
            TM = "TIMER",
            SN = "SOUND",
            SV = "SAVEDATA",
            UP = "UPDATE",
            LG = "LOG",

            SU = "STARTUP",
            TK = "TASKMGR",

            PN = "COMPORT",
            BR = "BAUDRATE",
            CS = "COINSLOT",
            IV = "INTERVAL",
            C1 = "COIN1",
            C2 = "COIN2",
            C3 = "COIN3",
            P1 = "PULSE1",
            P2 = "PULSE2",
            P3 = "PULSE3";

        public static bool IsValidCom(string comm)
        {
            switch (comm.ToUpper())
            {
                case INSTALL: return true;
                case APP: return true;
                case DEV: return true;
                case SVC: return true;
                case CLS: return true;
                case MAIN: return true;
                case HELP: return true;
                case COINAPP: return true;
                case SERIAL: return true;
                default: return false;
            }
        }

    }

    public struct Config
    {
        public static int PORTNUMBER = 1;
        public static int BAUDRATE = 19200;
        public static int COINSLOT = 0;
        public static int COIN1 = 0;
        public static int PULSE1 = 0;
        public static int COIN2 = 0;
        public static int PULSE2 = 0;
        public static int COIN3 = 0;
        public static int PULSE3 = 0;
        public static int SHUTDOWN = 20;
        public static int LOGINATTEMPT = 3;
        public static int HOTKEY = 38;
        public static string EMAIL = string.Empty;
        public static string PASS = string.Empty;
        public static int TIMER = 0;
        public static int SOUND = 0;
        public static int INTERVAL = 1000;
        public static int SAVEDATA = 0;
        public static int LOG = 0;
        public static string SAVEDATA_PATH = string.Empty;
        public static string LOCKIMAGE = string.Empty;
    }

    public struct Recovery
    {
        public static string host { get; set; } = "smtp.gmail.com";
        public static int port { get; set; } = 587;
        public static string email { get; set; } = "coinapptechnology@gmail.com";
        public static string pass { get; set; } = "coinapp2018";
        public static string subject { get; set; } = "COINAPP PASSWORD RECOVERY";
        public static string body { get; set; } = string.Empty;
    }

}