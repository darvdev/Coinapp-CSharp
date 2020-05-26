using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace coinup
{
    public partial class Software : Form
    {
        private bool working = false;
        private int downloads = 0;
        private int updates = 0;

        private readonly string appFilePath = Application.StartupPath + @"\coinapp.exe";
        private readonly string dllFilePath = Application.StartupPath + @"\coinapplib.dll";
        private readonly string svcFilePath = Application.StartupPath + @"\coinappsvc.exe";

        public Software()
        {
            InitializeComponent();
            Text = "Coinapp Software Updater v" + Application.ProductVersion;
        }

        private void Message(string text)
        {
            if (InvokeRequired) { this.BeginInvoke((Action)(() => label_message.Text = text)); }
            else { label_message.Text = text; }
        }

        private void Label(string text)
        {
            if (InvokeRequired) { this.BeginInvoke((Action)(() => label_download.Text = text)); }
            else { label_download.Text = text; }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (working == true)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            Program.SILENT = false;

            if (My.FormExists("Startup"))
            {
                Program.startup.Show();
            }

            this.Dispose();
            base.OnFormClosing(e);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Message("Checking internet connection...");

            Task.Run(async() =>
            {
                working = true;
                My.Log("Started Coinapp software updater.");

                #region Check internet
                try
                {
                    My.WebClientEx webClient = new My.WebClientEx();
                    webClient.Timeout = 5000;
                    webClient.OpenRead(new Uri("https://www.dropbox.com"));
                    webClient.Dispose();
                    Message("Connecting to the server...");
                }
                catch (Exception ex)
                {
                    Message("You are not connected to the internet.");
                    My.Log(ex.Message);
                    working = false;
                    return;
                }
                #endregion Check internet

                XDocument doc = await My.GetXDocument();

                if (doc == null)
                {
                    Message("Server not available at this time.");
                    working = false;
                    return;
                }

                #region Main version
                string mainVersionString;
                string mainVersionInfo = string.Empty;
                
                try { mainVersionString = doc.Root.Element("main").Element("ver").Value; }
                catch (Exception ex)
                {
                    Message("Cannot get latest  software version details.");
                    My.Log(ex.Message);
                    working = false;
                    return;
                }

                if (!Version.TryParse(mainVersionString, out Version mainVersion))
                {
                    Message("Latest software version details error.");
                    working = false;
                    return;
                }

                try { mainVersionInfo = doc.Root.Element("main").Element("info").Value; }
                catch (Exception ex)
                {
                    Message("Cannot get latest software version information.");
                    My.Log(ex.Message);
                }
                #endregion Main info


                #region The main program

                string appUrl;
                string dllUrl;
                string svcUrl;

                string appSize;
                string dllSize;
                string svcSize;

                #region App
                try
                {
                    appUrl = doc.Root.Element("main").Elements("lnk")
                    .First(x => x.Attribute("type").Value == "app").Value;
                }
                catch (Exception ex)
                { 
                    Message("coinapp.exe link error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }

                try
                {
                    appSize = doc.Root.Element("main").Elements("size")
                    .First(x => x.Attribute("type").Value == "app").Value;

                }
                catch (Exception ex)
                {
                    Message("coinapp.exe size error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }
                #endregion App

                #region Dll
                try
                {
                    dllUrl = doc.Root.Element("main").Elements("lnk")
                    .First(x => x.Attribute("type").Value == "dll").Value;
                }
                catch (Exception ex)
                {
                    Message("coinapplib.dll link error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }

                try
                {
                    dllSize = doc.Root.Element("main").Elements("size")
                    .First(x => x.Attribute("type").Value == "dll").Value;
                }
                catch (Exception ex)
                {
                    Message("coinapplib.dll size error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }
                #endregion Dll

                #region Svc
                try
                {
                    svcUrl = doc.Root.Element("main").Elements("lnk")
                    .First(x => x.Attribute("type").Value == "svc").Value;
                }
                catch (Exception ex)
                {
                    Message("coinappsvc.exe link error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }

                try
                {
                    svcSize = doc.Root.Element("main").Elements("size")
                    .First(x => x.Attribute("type").Value == "svc").Value;
                }
                catch (Exception ex)
                {
                    Message("coinappsvc.exe size error."); My.Log(ex.Message);
                    working = false;
                    return; 
                }
                #endregion Svc


                //Modes : 0 = latest version; 1 = File not exists (download without promt),
                //2 = corrupted (delete file & download without promt), 3 = Old version need to update (ask to update and move to old folder)  
                int appMode = 0;
                int dllMode = 0;
                int svcMode = 0;
                
                string appVersion = string.Empty;
                string dllVersion = string.Empty;
                string svcVersion = string.Empty;

                //coinapp.exe
                if (!File.Exists(appFilePath)) appMode = 1;
                else
                {
                    try { appVersion = FileVersionInfo.GetVersionInfo(appFilePath).ProductVersion; }
                    catch (Exception ex) { My.Log(ex.Message); appMode = 2; }

                    if (!Version.TryParse(appVersion, out Version version)) appMode = 2;
                    else
                    {
                        int result = mainVersion.CompareTo(version);
                        if (result > 0) appMode = 3;
                    }
                }

                //coinapplib.dll
                if (!File.Exists(dllFilePath)) dllMode = 1;
                else
                {
                    try { dllVersion = FileVersionInfo.GetVersionInfo(dllFilePath).ProductVersion; }
                    catch (Exception ex) { My.Log(ex.Message); dllMode = 2; }

                    if (!Version.TryParse(dllVersion, out Version version)) dllMode = 2;
                    else
                    {
                        int result = mainVersion.CompareTo(version);
                        if (result > 0) dllMode = 3;
                    }
                }

                //coinappsvc.exe
                if (!File.Exists(svcFilePath)) svcMode = 1;
                else
                {
                    try { svcVersion = FileVersionInfo.GetVersionInfo(svcFilePath).ProductVersion; }
                    catch (Exception ex) { My.Log(ex.Message); svcMode = 2; }

                    if (!Version.TryParse(svcVersion, out Version version)) svcMode = 2;
                    else
                    {
                        int result = mainVersion.CompareTo(version);
                        if (result > 0) svcMode = 3;
                    }
                }
                #endregion The Main program

                working = false;

                await Switcher(appMode, appFilePath, appUrl, appSize, mainVersionString, appVersion);
                await Switcher(dllMode, dllFilePath, dllUrl, dllSize, mainVersionString, dllVersion);
                await Switcher(svcMode, svcFilePath, svcUrl, svcSize, mainVersionString, svcVersion);

                Message(downloads == 0 && updates == 0? "You are updated to the latest version." : string.Format("Latest software version {0} downloaded", mainVersionString));
                Label(downloads == 0 && updates == 0 ? string.Format("Your coinapp software version {0} is up-to-date. ", mainVersionString) : (downloads == 0 ? "" : downloads + " downloaded" + (updates == 0 ? "" : ", ")) + (updates == 0 ? "" : updates + " updated" ));
            });
        }

        private async Task Switcher(int mode, string path, string url, string size, string mVersion, string cVersion)
        {
            switch (mode)
            {
                case 0: { My.Log(Path.GetFileName(url) + " is in latest version."); break; }
                case 1:
                    {
                        downloads += await DownloadFile(url, size, mVersion);
                        while (working)
                            await Task.Delay(10);
                        break;
                    }
                case 2:
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch (Exception ex)
                            {
                                Message("Cannot delete " + Path.GetFileName(path) + " file.");
                                Label(ex.Message); My.Log(ex.Message);
                                return;
                            }
                        }

                        downloads += await DownloadFile(url, size, mVersion);
                        while (working)
                            await Task.Delay(10);
                        break;
                    }

                case 3:
                    {
                        if (!Program.SILENT)
                        {
                            DialogResult result = MessageBox.Show(this, string.Format("{0} version {1} needs to update to version {2} continue update?", Path.GetFileName(url), cVersion, mVersion), "Coinapp updates", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                            if (result == DialogResult.No)
                            {
                                Message("Update canceled.");
                                Label(string.Format("Update for {0} version {1} is canceled.", Path.GetFileName(url), mVersion));
                                My.Log(string.Format("Update for {0} version {1} is canceled.", Path.GetFileName(url), mVersion));
                                return;
                            }
                        }

                        foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(url)))
                        {
                            if (Path.GetDirectoryName(p.MainModule.FileName) == Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName))
                            {
                                Message(string.Format("Please close {0} and try again.", Path.GetFileName(url)));
                                Label(string.Format("File {0} is currently opened. Cannot proceed.", Path.GetFileName(url)));
                                My.Log(string.Format("File {0} is currently opened. Cannot proceed.", Path.GetFileName(url)));
                                return;
                            }
                        }

                        updates += await DownloadFile(url, size, mVersion);
                        while (working)
                            await Task.Delay(10);
                        break;
                    }
            }
        }

        private async Task<int> DownloadFile(string url, string fileSize, string version = null)
        {
            working = true;
            My.Log("Downloading " + Path.GetFileName(url) + " version " + version + "...");

            string path = Application.StartupPath + @"\";
            string file = Path.GetFileName(url);
            bool error = false;
            //bool move = false;

            if (File.Exists(path + file))
            {
                try
                {
                    if (!Directory.Exists(path + "old"))
                    {
                        Directory.CreateDirectory(path + "old"); 
                    }
                    else
                    {
                        if (File.Exists(path + @"old\" + file))
                        {
                            File.Delete(path + @"old\" + file); 
                        } 
                    } 
                }
                catch (Exception ex)
                {
                    Message(string.Format("{0} is currently opened in the folder. Close it and try again.", file));
                    Label(path + @"old\"); My.Log(ex.Message); working = false;
                    return 0;
                }

                try
                {
                    File.Move(path + file, path + @"old\" + file);
                }
                catch (Exception ex)
                {
                    Message(string.Format("File {0} is currently opened. Close it and try again.", file));
                    Label(path); My.Log(ex.Message); working = false;
                    return 0;
                }
            }
            
            try
            {
                My.WebClientEx webClient = new My.WebClientEx();
                webClient.Timeout = 5000;
                webClient.DownloadProgressChanged += (s, e) =>
                {
                    this.BeginInvoke((Action)(() =>
                    {
                        if (!downloadProgress.Visible) downloadProgress.Visible = true;
                        downloadProgress.Value = e.ProgressPercentage;
                        label_download.Text =  e.ProgressPercentage + "%";
                    }));
                };

                webClient.DownloadFileCompleted += (s, e) =>
                {
                    webClient.Dispose();

                    if (error)
                    {
                        Message("Something went wrong."); Label("Failed to download " + file);
                        My.Log("Downloading " + file + " failed.");
                        working = false; return;
                    }
                    else
                    {
                        Message(string.Format("Validating {0}...", file));
                        Label(file + " " + version + " downloaded"); My.Log(string.Format("Validating {0} version {1}...", file, version));
                         
                        long newSize = new FileInfo(path + file).Length;

                        if (!long.TryParse(fileSize, out long newFileSize) || newSize != newFileSize) 
                        { 
                            Message(string.Format("Invalid {0} file.", file));
                            My.Log(string.Format("Invalid {0} file.", file));
                            File.Delete(path + file);
                            working = false; return;
                        }
                        else
                        {
                            Message(file + " verification completed.");
                            My.Log(string.Format("{0} version {1} verification complete.", file, version));
                            working = false; return;
                        }
                    }
                 };

                Message("Downloading " + file + "...");
                await webClient.DownloadFileTaskAsync(new Uri(url), path + file);
            }
            catch (Exception ex)
            {
                error = true; My.Log(ex.Message); working = false;
                return 0;
            }

            return 1;
        }

    }
}
