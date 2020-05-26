using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using coinup.Properties;


namespace coinup
{
    public partial class Device : Form
    {
        private bool working = false;

        private readonly string DIR_AVR = Application.StartupPath + @"\avr\";
        private readonly string DIR_FW = Application.StartupPath + @"\fw\";
        
        private const string CONF = "avrdude.conf";
        private const string EXE = "avrdude.exe";
        private const string DLL = "libusb0.dll";
        
        private const string BAUDRATE_UNO = "115200";
        private const string BAUDRATE_NANO = "57600";
        private const string avrdude = "-C{0} -v -patmega328p -carduino -P{1} -b{2} -D -Uflash:w:{3}:i";
        
        private const long SIZE_CONF = 480891;
        private const long SIZE_EXE = 465422;
        private const long SIZE_DLL = 67680;
        
        private int CURRENT_INDEX = 0;
        private string COMPORT = string.Empty;
        private string BAUDRATE = string.Empty;

        private List<List<string>> firmwares;
        private Process process;

        public Device()
        {
            InitializeComponent();
            Text = "Coinapp Firmware Updater v" + Application.ProductVersion;
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
            
            Program.VERBOSE = false;

            if (Directory.Exists(DIR_FW))
            {
                try { Directory.Delete(DIR_FW, true); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            if (Directory.Exists(DIR_AVR))
            {
                try { Directory.Delete(DIR_AVR, true); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

            if (My.FormExists("Startup")) { Program.startup.Show(); }

            this.Dispose();
            base.OnFormClosing(e);
        }
        private void addText(string text)
        {
            if (InvokeRequired) { this.BeginInvoke((Action)(() => richtextbox_message.AppendText(text + "\n"))); }
            else { richtextbox_message.AppendText(text + "\n"); }
        }
        private void addVer(string text)
        {
            if (InvokeRequired) { this.BeginInvoke((Action)(() => combobox_version.Items.Add(text))); }
            else { combobox_version.Items.Add(text); }
        }
        private void Device_Load(object sender, EventArgs e)
        {
            My.Log("Started Coinapp firmware updater.");
            addText(string.Empty);
            Task.Run(async () =>
            {
                working = true;
                #region Check internet
                addText("Connecting to the server...");
                try
                {
                    My.WebClientEx webClient = new My.WebClientEx();
                    webClient.Timeout = 5000;
                    webClient.OpenRead(new Uri("https://www.dropbox.com"));
                    webClient.Dispose();
                    addText("Checking firmware versions online...");
                }
                catch (Exception ex)
                {
                    addText("You are not connected to the internet.");
                    My.Log(ex.Message);
                    working = false;
                    return;
                }
                #endregion Check internet

                XDocument doc = await My.GetXDocument();
                if (doc == null)
                {
                    addText("Server not available at this time.");
                    My.Log("Server not available at this time.");
                    working = false;
                    return;
                }

                firmwares = new List<List<string>>();
                List<string> temp = new List<string>();
                Dictionary<XName, string>[] firms;

                try
                {
                    temp.Add(doc.Root.Element("device").Element("ver").Value);
                    temp.Add(doc.Root.Element("device").Element("date").Value);
                    temp.Add(doc.Root.Element("device").Element("info").Value);
                    temp.Add(doc.Root.Element("device").Element("size").Value);
                    temp.Add(doc.Root.Element("device").Element("lnk").Value);
                    firms = doc.Root.Element("device").Element("lite").Elements().Select(y => y.Elements().ToDictionary(x => x.Name, x => x.Value)).ToArray();
                }
                catch (Exception ex)
                {
                    My.Log(ex.Message);
                    MessageBox.Show(this, ex.Message, "Coinapp updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                firmwares.Add(temp);

                foreach (var firm in firms)
                {
                    temp = new List<string>();
                    temp.Add(firm["ver"]);
                    temp.Add(firm["date"]);
                    temp.Add(firm["info"]);
                    temp.Add(firm["size"]);
                    temp.Add(firm["lnk"]);
                    firmwares.Add(temp);
                }

                #region other methods
                //try
                //{

                //    XmlDocument xdoc = new XmlDocument();
                //    xdoc.LoadXml(await My.GetXml());
                //    XmlNodeList xnode = xdoc.SelectNodes("update/device/lite/fw");

                //    MessageBox.Show(xnode.Count + "");


                //    foreach (XmlNode node in xnode)
                //    {
                //        add(node["ver"].InnerText);
                //        add(node["size"].InnerText);
                //        add(node["date"].InnerText);
                //        add(node["lnk"].InnerText);
                //        add(node["info"].InnerText);
                //    }

                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}
                #endregion other methods

                for (int i = 0; i < firmwares.Count; i++)
                    addVer(firmwares[i][0]);

                addText(string.Empty);
                addText("Latest version : " + firmwares[0][0]);
                addText("Release date   : " + firmwares[0][1]);
                addText("Version info   : " + firmwares[0][2]);
                addText(string.Empty);

                status_label.Visible = true;
                status_label.Text = "There are " + firmwares.Count + " firmware available." + (Program.VERBOSE ? " ( Verbose output : ON )" : "");

                this.BeginInvoke((Action)(() =>
                {
                    combobox_version.SelectedIndex = 0;
                    CURRENT_INDEX = 0;
                    combobox_version.Enabled = true;
                    combobox_device.Enabled = true;
                }));

                working = false;
            });
        }
        private void combobox_version_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combobox_version.SelectedIndex != CURRENT_INDEX)
            {
                richtextbox_message.Clear();
                addText(string.Empty);
                CURRENT_INDEX = combobox_version.SelectedIndex;
                if (CURRENT_INDEX == 0)
                {
                    addText("Latest version : " + firmwares[CURRENT_INDEX][0]);
                    addText("Release date   : " + firmwares[CURRENT_INDEX][1]);
                    addText("Version info   : " + firmwares[CURRENT_INDEX][2]);
                }
                else
                {
                    addText("Firmware version : " + firmwares[CURRENT_INDEX][0]);
                    addText("Release date     : " + firmwares[CURRENT_INDEX][1]);
                    addText("Version info     : " + firmwares[CURRENT_INDEX][2]);
                }

                addText(string.Empty);
                status_label.Text = "Selected version : " + firmwares[CURRENT_INDEX][0];
            }

            ActiveControl = null;
        }
        private void combobox_device_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combobox_device.SelectedIndex != -1)
            {

                string[] ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    foreach (string port in ports)
                        if (!combobox_port.Items.Contains(port)) combobox_port.Items.Add(port);

                    if (!combobox_port.Enabled) combobox_port.Enabled = true;

                    BAUDRATE = combobox_device.SelectedIndex == 0 ? BAUDRATE_UNO : BAUDRATE_NANO;
                    combobox_port.SelectedIndex = 0;
                    COMPORT = combobox_port.SelectedItem.ToString();

                }
                else
                {
                    if (combobox_port.Enabled) combobox_port.Enabled = false;
                    if (button_flash.Enabled) button_flash.Enabled = false;
                    combobox_port.Items.Clear();
                    COMPORT = string.Empty;
                    addText("No available device. Please connect and try again.");
                    status_label.Text = "No connected device";
                }

            }
            ActiveControl = null;
        }
        private void combobox_port_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combobox_port.SelectedIndex < 0)
            {
                COMPORT = string.Empty;
                button_flash.Enabled = false;
            }
            else
            {
                if (COMPORT != combobox_port.SelectedItem.ToString()) COMPORT = combobox_port.SelectedItem.ToString();
                button_flash.Enabled = true;
            }

            ActiveControl = null;
        }
        private void button_flash_Click(object sender, EventArgs e)
        {
            ActiveControl = null;
            if (button_flash.Text == "ABORT")
            {
                DialogResult result = MessageBox.Show(this, "Aborting firmware flashing may damage your device.", "Coinapp Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;

                if (process != null)
                {
                    try
                    {
                        process.Kill();
                        addText("Flash firmware aborted.");
                        status_label.Text = "Flash firmware aborted.";
                        My.Log("Flash Firmware aborted.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Coinapp Updated", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        My.Log(ex.Message);
                    }
                }
            }
            else
            {
                DialogResult result = MessageBox.Show(this, "Flashing a firmware may brick your device.\n\n" +
                    "By clicking YES you agree that you are responsible for the result of flashing firmware by this program.", "Coinapp Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                {
                    status_label.Text = "Flash firmware canceled.";
                    return;
                }

                My.Log("Flash firmware agreement accepted.");
                working = true;
                button_flash.Enabled = false;
                combobox_version.Enabled = false;
                combobox_device.Enabled = false;
                combobox_port.Enabled = false;
                status_label.Text = "Flash firmware started.";
                addText("Checking flash tool...");
                My.Log("Flash firmware started.");

                if (!Directory.Exists(DIR_AVR))
                {
                    try { Directory.CreateDirectory(DIR_AVR); }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Coinapp Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        My.Log(ex.Message);
                        working = false;
                        return;
                    }
                }

                if (!(File.Exists(DIR_AVR + CONF) && File.Exists(DIR_AVR + EXE) && File.Exists(DIR_AVR + DLL)))
                {
                    addText("Extracting flash tool...");
                    My.Log("Extacting flash tool...");
                    try
                    {
                        File.WriteAllBytes(DIR_AVR + CONF, Resources.conf);
                        File.WriteAllBytes(DIR_AVR + EXE, Resources.exe);
                        File.WriteAllBytes(DIR_AVR + DLL, Resources.dll);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Coinapp Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        My.Log(ex.Message);
                        working = false;
                        return;
                    }
                }

                addText("Verifying flash tool...");
                My.Log("Verifying flash tool...");
                long confSize = new FileInfo(DIR_AVR + CONF).Length;
                long exeSize = new FileInfo(DIR_AVR + EXE).Length;
                long dllSize = new FileInfo(DIR_AVR + DLL).Length;

                if (confSize != SIZE_CONF && exeSize != SIZE_EXE && dllSize != SIZE_DLL)
                {
                    addText("Bad flash tool. Flash firmware cannot proceed.");
                    status_label.Text = "Bad flash tool.";
                    My.Log("Bad flash tool.");
                    working = false;
                    return;
                }

                Task.Run(async () =>
                {
                    string url = firmwares[CURRENT_INDEX][4];
                    string size = firmwares[CURRENT_INDEX][3];
                    string ver = firmwares[CURRENT_INDEX][0];

                    bool downloaded = await DownloadFirmware(url, size, ver, DIR_FW);
                    while (working)
                        await Task.Delay(10);

                    if (downloaded)
                    {
                        string file = DIR_AVR + EXE;
                        string conf = DIR_AVR + CONF;
                        string firm = DIR_FW + Path.GetFileNameWithoutExtension(url) + ".cfw";

                        StartFlashFirmware(file, string.Format(avrdude, conf, COMPORT, BAUDRATE, firm));
                    }
                    else
                    {
                        addText("Downloading firmware failed.");
                        status_label.Text = "Download failed.";
                        My.Log("Dowloading firmware failed.");
                    }
                });
            }
        }

        private async Task<bool> DownloadFirmware(string url, string size, string version, string path)
        {
            working = true;
            bool error = false;
            string file = Path.GetFileNameWithoutExtension(url) + ".cfw";

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    addText(ex.Message);
                    return false;
                }
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles("*.cfw").Where(p => p.Extension == ".cfw").ToArray();

                foreach (FileInfo f in files)
                    try
                    {
                        f.Attributes = FileAttributes.Normal;
                        File.Delete(f.FullName);
                    }
                    catch (Exception ex) { addText(ex.Message);return false; }

                if (File.Exists(path + file))
                {
                    try
                    {
                        File.Delete(path + file);
                    }
                    catch (Exception ex)
                    {
                        addText(ex.Message);
                        return false;
                    }
                }
            }

            try
            {
                My.WebClientEx webClient = new My.WebClientEx();
                webClient.Timeout = 5000;
                webClient.DownloadFileCompleted += (s, e) =>
                {
                    webClient.Dispose();
                    if (error)
                    {
                        addText("Failed to download firmware " + version);
                        My.Log("Failed to download firmware " + version);
                        working = false; return;
                    }
                    else
                    {
                        addText(string.Format("Validating firmware {0}...", version));
                        My.Log(string.Format("Validating firmware {0}...", version));
                        long newSize = new FileInfo(path + file).Length;

                        if (!long.TryParse(size, out long fileSize) || newSize != fileSize)
                        {
                            addText("Invalid firmware file.");
                            My.Log("Invalid firmware file.");
                            File.Delete(path + file);
                            working = false; return;
                        }
                        else
                        {
                            addText("Firmware " + version + " verification completed.");
                            My.Log("Firmware " + version + " verification completed.");
                            working = false; return;
                        }
                    }
                };
                addText("Downloading firmware " + version + "...");
                My.Log("Downloading firmware " + version + "...");
                await webClient.DownloadFileTaskAsync(new Uri(url), path + file);
            }
            catch (Exception ex)
            {
                error = true;
                My.Log(ex.Message); working = false;
                MessageBox.Show(this, ex.Message, "Coinapp Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void StartFlashFirmware(string exe, string args)
        {
            working = true;
            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            if (Program.VERBOSE)
            {
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;
            }

            process = new Process()
            {
                StartInfo = processInfo,
                EnableRaisingEvents = true,
                SynchronizingObject = this,
            };
            if (Program.VERBOSE)
            {
                process.OutputDataReceived += (s, e) => { if (e.Data != null) addText(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) addText(e.Data); };
            }

            process.Exited += (s, e) => 
            {
                this.BeginInvoke((Action)(() =>
                {
                    combobox_port.Items.Clear();
                    COMPORT = string.Empty;
                    combobox_version.Enabled = true;
                    combobox_device.SelectedIndex = -1;
                    combobox_device.Enabled = true;
                    button_flash.Enabled = false;
                    button_flash.Text = "FLASH FIRMWARE";
                }));

                addText(string.Empty);
                if (process.ExitCode != 0)
                {
                    if (process.ExitCode != -1)
                    {
                        addText("Flashing firmware failed.");
                        addText("Error code : " + process.ExitCode);
                        status_label.Text = "Flash firmware failed.";
                        My.Log("Flashing firmware failed.");
                        My.Log("Error code : " + process.ExitCode);
                    } 
                }
                else
                {
                    addText("Flash firmware completed.");
                    status_label.Text = string.Format("Flashed {0} bytes of firmware.", firmwares[CURRENT_INDEX][3]);
                    My.Log(string.Format("Flashed {0} bytes of firmware.", firmwares[CURRENT_INDEX][3]));
                }
                
                working = false;
            };

            process.Start();
            if (Program.VERBOSE)
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }
            
            this.BeginInvoke((Action)(() =>
            {
                addText(string.Format("Flashing firmware version {0} please wait a moment...", firmwares[CURRENT_INDEX][0]));
                status_label.Text = string.Format("Flashing firmware started at {0} ...", DateTime.Now);
                My.Log(string.Format("Flashing firmware Version: {0}, Size: {1} bytes, Device: {2}, Port: {3}...", firmwares[CURRENT_INDEX][0], firmwares[CURRENT_INDEX][3], combobox_device.SelectedItem, combobox_port.SelectedItem));
                button_flash.Text = "ABORT";
                button_flash.Enabled = true;
            }));
        }
    }

}
            

