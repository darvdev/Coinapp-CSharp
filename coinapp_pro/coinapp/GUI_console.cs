using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Ports;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading.Tasks;
using coinapplib;
using System.Runtime.InteropServices;
using System.IO;

namespace coinapp
{
    public partial class GUI_console : Form
    {
        //[DllImport("user32.dll", EntryPoint = "HideCaret")] private static extern long HideCaret(IntPtr hwnd);
        //[DllImport("user32.dll", EntryPoint = "ShowCaret")] private static extern long ShowCaret(IntPtr hwnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;

        private int STEPS = 0;
        private int NEXT = 0;
        private int INSTALL_TYPE = 0; //0 = install default; 1 = basic install; 2 = install advance;
        private bool ENTER_INSTALL = false;
        private bool ENTER_HOTKEY = false;
        private bool ENTER_EMAIL = false;
        private bool ENTER_PASS = false;
        private bool SKIP_PATH = false;

        private bool INPUT_DATA = false;
        private bool WORKING = false;
        private bool DEVICE_READY = false;

        private string DEVICE_VERSION = string.Empty;
        List<string> tempInput = new List<string>();
        SerialPort serialPort;
        public IntPtr iHandle;
        private struct TEMP
        {
            public static bool ValidEmail = false;
            public static string Email = string.Empty;
            public static string Pass1 = string.Empty;
            public static string Pass2 = string.Empty;
            public static int CoinSlot;
            public static int HotkeyCode;
        }
        public GUI_console()
        {
            InitializeComponent();
        }
        public GUI_console(IntPtr hWnd)
        { 
            InitializeComponent();
            iHandle = hWnd;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                const int WS_EX_TOPMOST = 0x8;
                if (c.LOCKED) cp.ExStyle |= WS_EX_TOPMOST;
                else cp.ExStyle = cp.ExStyle;
                return cp;
            }
        }

        #region CONTROL HANDLES
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab)//&& string.IsNullOrEmpty(textbox.Text))
            {
                if (ENTER_PASS)
                {
                    if (txtbx_command.UseSystemPasswordChar == true)
                        txtbx_command.UseSystemPasswordChar = false;
                    else
                        txtbx_command.UseSystemPasswordChar = true;

                }
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                if (ENTER_PASS && TEMP.Pass1 != string.Empty && TEMP.Pass2 != string.Empty)
                {
                    TEMP.Pass1 = string.Empty;
                    TEMP.Pass2 = string.Empty;
                    STEPS = 0;
                    SetConfig(NEXT, STEPS);
                }
                if (SKIP_PATH)
                {
                    SKIP_PATH = false;

                    if (NEXT == COMM.SAVEDATA)
                    {
                        if (INPUT_DATA)
                        {
                            INPUT_DATA = false;
                            btn_enter.Enabled = false;
                            Settings.SetSavedata(0);
                            Settings.WriteSysFile(Settings.savedataPath, string.Empty);
                            add("Selecting SAVEDATA canceled. Savedata cleared.");
                        }
                        else
                        {
                            Config.SAVEDATA = 0;
                            Config.SAVEDATA_PATH = string.Empty;
                            NEXT = COMM.LOCKIMAGE;
                            STEPS = 0;
                            SetConfig(NEXT, STEPS);
                        }
                        
                    } 
                    else if (NEXT == COMM.LOCKIMAGE)
                    {
                        if (INPUT_DATA)
                        {
                            INPUT_DATA = false;
                            btn_enter.Enabled = false;
                            Settings.SetLockImage(string.Empty);
                            add("Selecting LOCKIMAGE canceled. Lockimage cleared.");
                        }
                        else
                        {
                            NEXT = COMM.SHOW_SETTINGS;
                            STEPS = 0;
                            SetConfig(NEXT, STEPS);
                        }
                    }  
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void add(string text, bool addNewLine = true)
        { 
            if (InvokeRequired)
            {
                this.BeginInvoke(() =>
                {
                    rtb_details.AppendText(addNewLine ? text + "\n" : text);
                    //rtb_details.ScrollToCaret();
                });
            }
            else
            {
                rtb_details.AppendText(addNewLine ? text + "\n" : text);
                //rtb_details.ScrollToCaret();
            }

            SendMessage(rtb_details.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
        }
        private void rtb_details_LinkClicked(Object sender, LinkClickedEventArgs e)
        {
            Process.Start(e.LinkText);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (WORKING)
                {
                    e.Cancel = true;
                    add("Performing background operations. Please wait a moment...");
                    return;
                }
            }

            if (serialPort != null)
                serialPort.Dispose();
            
            this.Dispose();
            base.OnFormClosing(e);
        }
        private void GUI_console_Load(object sender, EventArgs e)
        {
            if (c.FormExists("GUI_dialog"))
                c.app.dialog.Close();
            
            Text = Application.ProductName + " Console " + Application.ProductVersion;
            rtb_details.SelectionIndent = 10;
            rtb_details.SelectionHangingIndent = 20;
            rtb_details.SelectionRightIndent = 10;
            if (c.CONNECTED && c.CURRENT_DEVICE_VERSION != string.Empty) Text += " ( Firmware device : " + c.CURRENT_DEVICE_VERSION + " )";

            if (c.STARTUP_INSTALL)
            {
                this.Height = 420;
                add(string.Empty);
                add("Welcome to " + Application.ProductName + " " + Application.ProductVersion);
                add("Thank you for downloading this wonderful application :)");
                add(string.Empty);
                add("Before we start. We need to configure some settings first; AND");
                add("You have the required device (Arduino) installed with appropriate firmware.");
                add(string.Empty);
                add("This is our official facebook page https://www.facebook.com/CoinappTechnology");
                add("Follow us by clicking the like button of our facebook page. Thank you!");
                add(string.Empty); add(string.Empty);
                add("'" + Application.StartupPath + @"\'", false); add(" <-- will be use to run the app.");
                add(string.Empty);
                add("NOW let's start. First, connect the device to this computer; then");
                add("Choose and type a command (without asterisk) then enter. (case insensitive)");
                add(string.Empty);
                add("Commands:");
                add("* INSTALL            Install default configurations;");
                add("* INSTALL BASIC      Install basic configurations;");
                add("* INSTALL ADVANCE    Install advance configurations;");
                add(string.Empty);
            }
            else
            {
                add(string.Empty);
                add(Application.ProductName + " " + Application.ProductVersion);
                add("https://www.facebook.com/CoinappTechnology\n");
                add(string.Empty);
                
                if (c.UPDATES > 0)
                {
                    add("New version of " + (c.UPDATES == 1 ? "software is available!" : (c.UPDATES == 2 ? "device is available!" : "software and device are available!")));
                    add(string.Empty);
                }

                Task.Run(async () =>
                {
                    this.txtbx_command.ReadOnly = true;
                    string msg = await c.iConsoleMessage();
                    if (msg != string.Empty)
                    {
                        add(msg);
                        add(string.Empty);
                    }
                    this.txtbx_command.ReadOnly = false;
                });

                if (c.ecode.Count != 0)
                {
                    if (c.ecode.Contains(ECODE.DEVICE_READY))
                        add("SESSION : " + (c.SESSION.ToString().Length == 1 ? " " + c.SESSION : c.SESSION.ToString()) + " [" + c.S.ToString(c.SESSION) + "]"); ;
                    
                    for (int i = 0; i < c.ecode.Count; i++)
                    {
                        add("ECODE   : ", false); add((c.ecode[i].ToString().Length == 1 ? " " + c.ecode[i] : c.ecode[i].ToString()), false); add(" [" + ECODE.ToString(c.ecode[i]) + "]");
                    }

                    add(string.Empty);

                    //Show data in save data folder to the console.
                    if (My.RestoreTimeResume(out int rem) && rem != 0 && c.SESSION != SESSION.TIME_IN && c.SESSION != SESSION.TIME_RESUME)
                    {
                        add(string.Empty);
                        add("Saved data : " + c.TimeResumeToString(rem, true));
                        add(string.Empty);
                    }

                    if (!Reg.StartupRegistryExists)
                    {
                        //c.ecode.Add(ECODE.REGISTRY_STARTUP_NOT_FOUND);
                        add("WARNING! Startup registry not exists.");
                    }
                    else
                    {
                        if (!Reg.ValidStartupValue)
                        {
                            //c.ecode.Add(ECODE.REGISTRY_STARTUP_INVALID);
                            add("WARNING! Invalid startup registry value.");
                        }
                    }
                }
                else
                {
                    if (c.CONNECTED) add("SYSTEM INITIALIZING...");
                }
            }
        }
        private void txtbx_command_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbx_command.Text) == false)
            {
                if (ENTER_EMAIL == true)
                {
                    if (c.IsValidEmail(txtbx_command.Text))
                    {
                        TEMP.ValidEmail = true;
                        btn_enter.Enabled = true;
                    }
                    else
                    {
                        TEMP.ValidEmail = false;
                        btn_enter.Enabled = false;
                    }
                }
                else
                {
                    btn_enter.Enabled = true;
                }
            }  
            else
            {
                btn_enter.Enabled = false;
            }
            txtbx_command.Focus();
        }
        private void txtbx_command_KeyDown(object sender, KeyEventArgs e)
        {
            if (ENTER_HOTKEY == true)
            {
                if (((int)e.KeyCode >= 96 && (int)e.KeyCode <= 105 || (int)e.KeyCode == 110 || (int)e.KeyCode == 19 || (int)e.KeyCode == 9 || (int)e.KeyCode == 91 || (int)e.KeyCode == 92 || (int)e.KeyCode >= 16 && (int)e.KeyCode <= 18))
                {
                    add(HotkeyCode.ToString((int)e.KeyCode) + " is not available for HOTKEY.");
                    e.Handled = true;
                }
                else
                {
                    TEMP.HotkeyCode = (int)e.KeyCode;

                    txtbx_command.Text = "CTRL + ALT + SHIFT + " + HotkeyCode.ToString((int)e.KeyCode);
                    txtbx_command.SelectionStart = txtbx_command.Text.Length;
                    //this.BeginInvoke(() =>
                    //{
                        
                    //});
                }
            }
            else if (ENTER_EMAIL == true)
            {
                if ((int)e.KeyCode == 13 && TEMP.ValidEmail)
                {
                    TEMP.Email = txtbx_command.Text;
                    txtbx_command.Clear();
                    ExecuteCommand();
                    //Task.Run(() => this.BeginInvoke(() => ExecuteCommand()));
                    e.SuppressKeyPress = true;
                }
            }
            else if (ENTER_PASS == true)
            {
                if ((int)e.KeyCode == 13 && string.IsNullOrWhiteSpace(txtbx_command.Text) == false)
                {
                    if (STEPS == 1)
                        TEMP.Pass1 = txtbx_command.Text;
                    if (STEPS == 2)
                        TEMP.Pass2 = txtbx_command.Text;
                    txtbx_command.Clear();
                    ExecuteCommand();
                    //Task.Run(() => this.BeginInvoke(() => ExecuteCommand()));
                    e.SuppressKeyPress = true;
                }
            }
            else
            {
                if ((int)e.KeyCode == 13 && string.IsNullOrWhiteSpace(txtbx_command.Text) == false)
                {
                    string input = txtbx_command.Text;
                    txtbx_command.Clear();
                    ExecuteCommand(input);
                    //Task.Run(() =>this.BeginInvoke(() => ExecuteCommand(input)));
                    e.SuppressKeyPress = true;
                }
            }
        }
        private void btn_enter_Click(object sender, EventArgs e)
        {   
            if (TEMP.ValidEmail == true)
            {
               TEMP.Email = txtbx_command.Text;
                txtbx_command.Clear();
                ExecuteCommand();
                //Task.Run(() => this.BeginInvoke(() => ExecuteCommand()));
            }
            else if (ENTER_PASS == true)
            {
                if (STEPS == 1)
                    TEMP.Pass1 = txtbx_command.Text;
                if (STEPS == 2)
                    TEMP.Pass2 = txtbx_command.Text;
                txtbx_command.Clear();
                ExecuteCommand();
                //Task.Run(() => this.BeginInvoke(() => ExecuteCommand()));
            }
            else
            {
                string input = txtbx_command.Text;
                txtbx_command.Clear();
                ExecuteCommand(input);
                //Task.Run(() => this.BeginInvoke(() => ExecuteCommand(input)));
            }
            //txtbx_command.Clear();
            txtbx_command.Focus();
        }
        #endregion

        private void ExecuteCommand(string args = "")
        {
            List<string> input = new List<string>();

            if (args != string.Empty)
            {
                input = args.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();
            }

            if (ENTER_INSTALL)
            {
                tempInput.Clear();
                tempInput = input;
                SetConfig(NEXT, STEPS);
            }
            else
            {
                if (INPUT_DATA)
                {
                    tempInput.Clear();
                    tempInput = input;
                    SetConfig(NEXT, STEPS);
                }
                else
                {
                    if (COMM.IsValidCom(input[0]) == false)
                    {
                        add(@"""" + input[0].ToUpper() + @""" is not a coinapp command-name. Type HELP for more info.");
                    }
                    else
                    {
                        switch (input[0].ToUpper())
                        {

                            case COMM.INSTALL:
                                {
                                    INSTALL_TYPE = input.Count > 1 ? (input[1].ToUpper() == "BASIC" ? 1 : (input[1].ToUpper() == "ADVANCE" ? 2 : 0)) : 0;

                                    switch (INSTALL_TYPE)
                                    {
                                        case 0:
                                            add("Default INSTALLATION begin...");
                                            break;

                                        case 1:
                                            add("Basic INSTALLATION begin...");
                                            break;

                                        case 2:
                                            add("Advance INSTALLATION begin...");
                                            break;
                                    }

                                    add(string.Empty);
                                    ENTER_INSTALL = true;
                                    NEXT = COMM.COMPORT; // 1; BEGIN STARTING INSTALLATION CONFIG.

                                    SetConfig(NEXT, STEPS);

                                    break;
                                }

                            case COMM.APP:
                                {
                                    if (input.Count > 1)
                                    {
                                        SetConfigApp(input);
                                    }
                                    else
                                    {
                                        add(string.Empty); add(string.Empty);
                                        add("[APP]   APPLICATION CONFIGURATIONS");
                                        add("-------------------------------------------------------");
                                        add("To change specific configuration type APP [config-name]");
                                        add("Or change directly by typing APP [config-name] [value]");
                                        add(string.Empty);
                                        add("List of config names:");
                                        add(" * STARTUP      value : number 0 or 1");
                                        add(" * TASKMGR      value : number 0 or 1");
                                        add(" * SHUTDOWN     value : number in seconds");
                                        add(" * ATTEMPT      value : number of login retries");
                                        add(" * EMAIL        (cannot change directly)");
                                        add(" * PASS         (cannot change directly)");
                                        add(" * TIMER        value : number 0, 1 or 2");
                                        add(" * SOUND        value : number 0, 1 or 2");
                                        add(" * LOG          value : number 0 or 1");
                                        add(" * HOTKEY       (cannot change directly)");
                                        add(" * SAVEDATA     (cannot change directly)");
                                        add(" * LOCKIMAGE    (cannot change directly)");
                                        add(string.Empty); add(string.Empty);
                                        add("These are the current application configuration.");
                                        add("-----------------------------------------------------------");
                                        add("  Start at startup :   ", false);

                                        if (Reg.StartupRegistryExists)
                                        {
                                            if (Reg.ValidStartupValue) add("1 (It will start at windows startup)");
                                            else add("0 (Invalid startup value)");
                                        }
                                        else
                                        {
                                            add("0 (No startup registry detected)");
                                        }
                                        add("  Disable taskmgr  :   ", false);

                                        if (Reg.DisableTaskmgrRegistryExists) ///////////////////iuhduiash oerroroqa jaslkdj as
                                        {
                                            if (Reg.ValidDisableTaskmgrValue) add("1 (Default task manager is disabled)");
                                            else add("0 (Invalid Disable Taskmgr value)");
                                        }
                                        else
                                        {
                                            add("0 (Default task manager will be open)");
                                        }

                                        add("  Shutdown time    :   " + c.TimeToString(Config.SHUTDOWN));
                                        add("  Login attempt    :   " + Config.LOGINATTEMPT + " retry");
                                        add("  Hotkey           :   " + "CTRL + ALT + SHIFT + " + HotkeyCode.ToString(Config.HOTKEY));
                                        add("  Recovery e-mail  :   " + Config.EMAIL);
                                        add("  Password         :   " + Config.PASS);
                                        add("  Display timer    :   " + Config.TIMER + " (" + (Config.TIMER > 0 ? "Show timer display on desktop window)" : "No timer display on desktop window)"));
                                        add("  Sound enable     :   " + Config.SOUND + " (" + (Config.SOUND > 0 ? "Play application sound effect)" : "No application sound effect)"));
                                        add("  Savedata folder  :   " + (Config.SAVEDATA_PATH == string.Empty ? "(No save data folder)" : Config.SAVEDATA_PATH));
                                        add("  Lock image       :   " + (Config.LOCKIMAGE == string.Empty ? "(No lock image file)" : Config.LOCKIMAGE));
                                        add(string.Empty); add(string.Empty);
                                    }

                                    break;
                                }

                            case COMM.DEV:
                                {
                                    if (input.Count > 1)
                                    {
                                        SetConfigDev(input);
                                    }
                                    else
                                    {
                                        add(string.Empty); add(string.Empty);
                                        add("[DEV]   DEVICE CONFIGURATIONS");
                                        add("-------------------------------------------------------");
                                        add("To change specific configuration type DEV [config-name]");
                                        add("Or change directly by typing DEV [config-name] [value]");
                                        add(string.Empty);
                                        add("List of config names:");
                                        add(" * COMPORT      (Cannot change directly)");
                                        add(" * BAUDRATE     (Cannot change directly)");
                                        add(" * COINSLOT     value : number 0, 1, 2 or 3");
                                        add(" * INTERVAL     value : number in milliseconds");
                                        add(" * COIN1        value : number in seconds");
                                        add(" * COIN2        value : number in seconds");
                                        add(" * COIN3        value : number in seconds");
                                        add(" * PULSE1       value : number in pulse");
                                        add(" * PULSE2       value : number in pulse");
                                        add(" * PULSE3       value : number in pulse");
                                        add(string.Empty); add(string.Empty);
                                        add("These are the current device configuration.");
                                        add("-------------------------------------------");
                                        add("  Device com port  :   COM" + Config.PORTNUMBER);
                                        add("  Device baud rate :   " + Config.BAUDRATE);
                                        add("  Coin-slot type   :   " + Config.COINSLOT + " [" + c.CoinslotToString(Config.COINSLOT) + "]");
                                        add("  Time interval    :   " + Config.INTERVAL + " milliseconds");
                                        add("  Coin 1           :   " + c.TimeToString(Config.COIN1));
                                        add("  Coin 2           :   " + c.TimeToString(Config.COIN2));
                                        add("  Coin 3           :   " + c.TimeToString(Config.COIN3));
                                        add("  Pulse 1          :   " + (Config.PULSE1 == 0 ? "" : Config.PULSE1 + " pulse"));
                                        add("  Pulse 2          :   " + (Config.PULSE2 == 0 ? "" : Config.PULSE2 + " pulse"));
                                        add("  Pulse 3          :   " + (Config.PULSE3 == 0 ? "" : Config.PULSE3 + " pulse"));
                                        add(string.Empty); add(string.Empty);
                                    }
                                    break;
                                }

                            case COMM.SVC:
                                {
                                    if (input.Count() > 1)
                                    {
                                        SetConfigSvc(input);
                                    }
                                    else
                                    {
                                        add(string.Empty); add(string.Empty);
                                        add("[SVC]   SERVICE CONFIGURATIONS");
                                        add("--------------------------------------------------------");
                                        add("To initiate service configuration type SVC [config-name]");
                                        add(string.Empty);
                                        add("List of config names:");
                                        add(" * INSTALL      To install if the service is not installed.");
                                        add(" * UNINSTALL    To uninstall if the service is installed.");
                                        add(" * START        To start if the service is not running.");
                                        add(" * STOP         To stop if service is running.");
                                        add(string.Empty); add(string.Empty);
                                        add("These are the current information of the service.");
                                        add("-------------------------------------------------");

                                        bool installed = false;
                                        bool valid = false;
                                        string imagePath = string.Empty;
                                        string serviceStatus = string.Empty;
                                        string serviceName = string.Empty;
                                        string displayName = string.Empty;
                                        string serviceType = string.Empty;

                                        ServiceController[] scs = ServiceController.GetServices();
                                        if (scs.Any(x => x.ServiceName == c.serviceName))
                                        {
                                            using (ServiceController sc = new ServiceController(c.serviceName))
                                            {
                                                serviceName = sc.ServiceName;
                                                displayName = sc.DisplayName;
                                                serviceStatus = sc.Status.ToString();
                                                serviceType = sc.ServiceType.ToString();
                                                installed = true;
                                            }
                                        }

                                        if (!installed)
                                        {
                                            add("-------------(SERVICE NOT INSTALLED)-------------");
                                            add("-------------------------------------------------");
                                        }
                                        else
                                        {
                                            valid = Reg.IsValidServicePath(c.serviceName, out imagePath);

                                            add("  Service name  :   " + serviceName);
                                            add("  Display name  :   " + displayName);
                                            add("  Start type    :   " + Reg.ServiceStartType(c.serviceName));
                                            add("  Service type  :   " + serviceType);
                                            add("  Status        :   " + serviceStatus);
                                            add("  ImagePath     :   " + imagePath);
                                            add("  Path valid    :   " + valid.ToString());
                                        }
                                        add(string.Empty); add(string.Empty);
                                    }
                                    break;
                                }

                            case COMM.CLS:
                                {
                                    rtb_details.Clear();
                                    add(string.Empty);
                                    add(Application.ProductName + " " + Application.ProductVersion);
                                    add("https://www.facebook.com/CoinappTechnology\n");
                                    add(string.Empty);
                                    break;
                                }

                            case COMM.MAIN:
                                {
                                    if (input.Count() > 1)
                                    {
                                        if (input.Count() > 2)
                                        {
                                            add(input[1].ToUpper() + " is not accepting value.");
                                        }
                                        else
                                        {
                                            if (input[1].ToUpper() == "STOP")
                                            {
                                                if (c.CONNECTED)
                                                {
                                                    if (c.LOCKED)
                                                    {
                                                        add("You cannot stop the app in this session.");
                                                        add("SESSION: " + c.SESSION + " [" + c.S.ToString(c.SESSION) + "]");
                                                        return;
                                                    }
                                                    add("Stopping coinapp application...");
                                                    c.mainPipe.WriteString(ARGS.STOP);
                                                }
                                                else
                                                {
                                                    add("You cannot execute this action right now.");
                                                }
                                            }
                                            else if (input[1].ToUpper() == "RESTART")
                                            {
                                                add("Sorry, this command is disabled in this version. Use MAIN STOP instead.");
                                                //if (c.CONNECTED)
                                                //{
                                                //    if (c.LOCKED)
                                                //    {
                                                //        add("You cannot restart the app in this session.");
                                                //        add("SESSION: " + c.SESSION + " [" + c.S.ToString(c.SESSION) + "]");
                                                //        return;
                                                //    }
                                                //    add("Restarting coinapp application...");
                                                //    c.mainPipe.WriteString(ARGS.RESTART);

                                                //    using (ServiceController sc = new ServiceController(c.serviceName))
                                                //    {
                                                //        if (sc.Status == ServiceControllerStatus.Running)
                                                //        {
                                                //            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(50000));
                                                //        }
                                                //        Application.Restart();
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    add("You cannot execute this action right now.");
                                                //}
                                            }
                                            else if (input[1].ToUpper() == "CREATE")
                                            {
                                                string ini = Application.StartupPath + @"\coinapp.ini";
                                                if (File.Exists(ini))
                                                {
                                                    File.Delete(ini);
                                                    add("coinapp.ini replaced with default values.");
                                                }
                                                else
                                                {
                                                    add("coinapp.ini created with default values.");
                                                }
                                                Settings.CreateDefaultConfigFile();
                                            }
                                            else if (input[1].ToUpper() == "REMOVE") 
                                            {
                                                if (Directory.Exists(Settings.sysDir))
                                                {
                                                    try
                                                    {
                                                        Directory.Delete(Settings.sysDir, true);
                                                    }
                                                    catch
                                                    {
                                                        add("Error! cannot remove system configuration. Try running this as administrator and try again.");
                                                        return;
                                                    }
                                                    add("System configuration removed successfuly.");
                                                }
                                                else
                                                {
                                                    add("Nothing to remove.");
                                                }
                                            }
                                            else
                                            {
                                                add(input[1].ToUpper() + " is not a command for main.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        add(string.Empty); add(string.Empty);
                                        add("[MAIN]   COINAPP MAIN PROGRAM");
                                        add("----------------------------------------------------------------");
                                        add("To initiate TOP-LEVEL command, type MAIN command-name and enter.");
                                        add(string.Empty);
                                        add("List of command names:");
                                        add(" * STOP        To stop this coinapp application.");
                                        add(" * RESTART     To restart this coinapp application.");
                                        add(" * REMOVE      To remove saved password and recovery email.");
                                        add(" * CREATE      To create coinapp.ini file with default values.");
                                        add("               (coinapp.ini will be replaced if existed.)");
                                        add(string.Empty); add(string.Empty);
                                    }
                                    break;
                                }

                            case COMM.HELP:
                                {
                                    add(string.Empty); add(string.Empty);
                                    add("[HELP]   COINAPP CONSOLE WINDOW");
                                    add("--------------------------------------------------------");
                                    add("For more information, type the command-name and enter.");
                                    add("NOTE: all commands, options, values are CaSe iNsenSiTive");
                                    add(string.Empty);
                                    add("List of command names:");
                                    add(" * APP        Information about aplication configuration.");
                                    add(" * DEV        Information about device configuration.");
                                    add(" * SVC        Information about the service configuration.");
                                    add(" * CLS        To clear text in the command window. (No other info)");
                                    add(" * MAIN       Sub-command (To restart app, to quit app, etc.)");
                                    add(" * HELP       This is me. The information. (No other info)");
                                    add(string.Empty);
                                    break;
                                }

                            case COMM.COINAPP:
                                {
                                    Task.Run(async () =>
                                    {
                                        this.txtbx_command.ReadOnly = true;
                                        string msg = await c.iConsoleMessage();
                                        if (msg != string.Empty)
                                        {
                                            add(msg);
                                            add(string.Empty);
                                        }
                                        this.txtbx_command.ReadOnly = false;
                                    });
                                    break;
                                }

                            case COMM.SERIAL:
                                {
                                    if (input.Count() > 1)
                                    {
                                        if (c.CONNECTED) c.mainPipe.WriteString("SERIAL=" + input[1]);
                                        add("Sent " + input[1].ToUpper() + " serial command.");
                                    }
                                    else
                                    {
                                        add("[SERIAL]   SERIAL COMMUNICATION");
                                        add("------------------------------------------------");
                                        add("Direct communication to connected serial device.");
                                    }
                                        break;
                                }
                        }
                    }





                }
            }

            ////////////////////////////////////////////////////////////////////////////

            #region BACKUP

            //if (c.STARTUP_INSTALL)
            //{
            //    if (ENTER_INSTALL)
            //    {
            //        tempInput.Clear();
            //        tempInput = input;
            //        SetConfig(NEXT, STEPS);
            //    }
            //    else
            //    {
            //        switch (input[0].ToUpper())
            //        {
            //            case COMM.INSTALL:
            //                { 
            //                    INSTALL_TYPE = input.Count > 1 ? (input[1].ToUpper() == "BASIC" ? 1 : (input[1].ToUpper() == "ADVANCE" ? 2 : 0)) : 0;

            //                    switch (INSTALL_TYPE)
            //                    {
            //                        case 0:
            //                            add("Default INSTALLATION begin...");
            //                            break;

            //                        case 1:
            //                            add("Basic INSTALLATION begin...");
            //                            break;

            //                        case 2:
            //                            add("Advance INSTALLATION begin...");
            //                            break;
            //                    }

            //                    add(string.Empty);
            //                    ENTER_INSTALL = true;
            //                    NEXT = COMM.COMPORT; // 1; BEGIN STARTING INSTALLATION CONFIG.

            //                    SetConfig(NEXT, STEPS);

            //                    break;
            //                }

            //            default:
            //                {
            //                    add("Invalid command in this stage.");

            //                }
            //                break;
            //        }
            //    }
                
            //}
            //else
            //{
            //    if (INPUT_DATA)
            //    {
            //        tempInput.Clear();
            //        tempInput = input;
            //        SetConfig(NEXT, STEPS);
            //    }
            //    else
            //    {
            //        if (COMM.IsValidCom(input[0]) == false)
            //        {
            //            add(@"""" + input[0].ToUpper() + @""" is not a coinapp command-name. Type HELP for more info.");
            //        }
            //        else
            //        {
            //            switch (input[0].ToUpper())
            //            {
            //                case COMM.APP:
            //                    {
            //                        if (input.Count > 1)
            //                        {
            //                            SetConfigApp(input);
            //                        }
            //                        else
            //                        {
            //                            add(string.Empty); add(string.Empty);
            //                            add("[APP]   APPLICATION CONFIGURATIONS");
            //                            add("-------------------------------------------------------");
            //                            add("To change specific configuration type APP [config-name]");
            //                            add("Or change directly by typing APP [config-name] [value]");
            //                            add(string.Empty);
            //                            add("List of config names:");
            //                            add(" * STARTUP      value : number 0 or 1");
            //                            add(" * TASKMGR      value : number 0 or 1");
            //                            add(" * SHUTDOWN     value : number in seconds");
            //                            add(" * ATTEMPT      value : number of login retries");
            //                            add(" * EMAIL        (cannot change directly)");
            //                            add(" * PASS         (cannot change directly)");
            //                            add(" * TIMER        value : number 0, 1 or 2");
            //                            add(" * SOUND        value : number 0, 1 or 2");
            //                            add(" * LOG          value : number 0 or 1");
            //                            add(" * HOTKEY       (cannot change directly)");
            //                            add(" * SAVEDATA     (cannot change directly)");
            //                            add(" * LOCKIMAGE    (cannot change directly)");
            //                            add(string.Empty); add(string.Empty);
            //                            add("These are the current application configuration.");
            //                            add("-----------------------------------------------------------");
            //                            add("  Start at startup :   ", false);

            //                            if (Reg.StartupRegistryExists)
            //                            {
            //                                if (Reg.ValidStartupValue) add("1 (It will start at windows startup)");
            //                                else add("0 (Invalid startup value)");
            //                            }
            //                            else
            //                            {
            //                                add("0 (No startup registry detected)");
            //                            }
            //                            add("  Disable taskmgr  :   ", false);

            //                            if (Reg.DisableTaskmgrRegistryExists) ///////////////////iuhduiash oerroroqa jaslkdj as
            //                            {
            //                                if (Reg.ValidDisableTaskmgrValue) add("1 (Default task manager is disabled)");
            //                                else add("0 (Invalid Disable Taskmgr value)");
            //                            }
            //                            else
            //                            {
            //                                add("0 (Default task manager will be open)");
            //                            }

            //                            add("  Shutdown time    :   " + c.TimeToString(Config.SHUTDOWN));
            //                            add("  Login attempt    :   " + Config.LOGINATTEMPT + " retry");
            //                            add("  Hotkey           :   " + "CTRL + ALT + SHIFT + " + HotkeyCode.ToString(Config.HOTKEY));
            //                            add("  Recovery e-mail  :   " + Config.EMAIL);
            //                            add("  Password         :   " + Config.PASS);
            //                            add("  Display timer    :   " + Config.TIMER + " (" + (Config.TIMER > 0 ? "Show timer display on desktop window)" : "No timer display on desktop window)"));
            //                            add("  Sound enable     :   " + Config.SOUND + " (" + (Config.SOUND > 0 ? "Play application sound effect)" : "No application sound effect)"));
            //                            add("  Savedata folder  :   " + Config.SAVEDATA_PATH);
            //                            add("  Lock image       :   " + Config.LOCKIMAGE);
            //                            add(string.Empty); add(string.Empty);
            //                        }

            //                        break;
            //                    }

            //                case COMM.DEV:
            //                    {
            //                        if (input.Count > 1)
            //                        {
            //                            SetConfigDev(input);
            //                        }
            //                        else
            //                        {
            //                            add(string.Empty); add(string.Empty);
            //                            add("[DEV]   DEVICE CONFIGURATIONS");
            //                            add("-------------------------------------------------------");
            //                            add("To change specific configuration type DEV [config-name]");
            //                            add("Or change directly by typing DEV [config-name] [value]");
            //                            add(string.Empty);
            //                            add("List of config names:");
            //                            add(" * COMPORT      (Cannot change directly)");
            //                            add(" * BAUDRATE     (Cannot change directly)");
            //                            add(" * COINSLOT     value : number 0, 1, 2 or 3");
            //                            add(" * INTERVAL     value : number in milliseconds");
            //                            add(" * COIN1        value : number in seconds");
            //                            add(" * COIN2        value : number in seconds");
            //                            add(" * COIN3        value : number in seconds");
            //                            add(" * PULSE1       value : number in pulse");
            //                            add(" * PULSE2       value : number in pulse");
            //                            add(" * PULSE3       value : number in pulse");
            //                            add(string.Empty); add(string.Empty);
            //                            add("These are the current device configuration.");
            //                            add("-------------------------------------------");
            //                            add("  Device com port  :   COM" + Config.PORTNUMBER);
            //                            add("  Device baud rate :   " + Config.BAUDRATE);
            //                            add("  Coin-slot type   :   " + Config.COINSLOT + " [" + c.CoinslotToString(Config.COINSLOT) + "]");
            //                            add("  Time interval    :   " + Config.INTERVAL + " milliseconds");
            //                            add("  Coin 1           :   " + c.TimeToString(Config.COIN1));
            //                            add("  Coin 2           :   " + c.TimeToString(Config.COIN2));
            //                            add("  Coin 3           :   " + c.TimeToString(Config.COIN3));
            //                            add("  Pulse 1          :   " + Config.PULSE1 + " pulse");
            //                            add("  Pulse 2          :   " + (Config.PULSE2 > 0 ? Config.PULSE2 + " pulse" : ""));
            //                            add("  Pulse 3          :   " + (Config.PULSE3 > 0 ? Config.PULSE3 + " pulse" : ""));
            //                            add(string.Empty); add(string.Empty);
            //                        }
            //                        break;
            //                    }

            //                case COMM.SVC:
            //                    {
            //                        if (input.Count() > 1)
            //                        {
            //                            SetConfigSvc(input);
            //                        }
            //                        else
            //                        {
            //                            add(string.Empty); add(string.Empty);
            //                            add("[SVC]   SERVICE CONFIGURATIONS");
            //                            add("--------------------------------------------------------");
            //                            add("To initiate service configuration type SVC [config-name]");
            //                            add(string.Empty);
            //                            add("List of config names:");
            //                            add(" * INSTALL      To install if the service is not installed.");
            //                            add(" * UNINSTALL    To uninstall if the service is installed.");
            //                            add(" * START        To start if the service is not running.");
            //                            add(" * STOP         To stop if service is running.");
            //                            add(string.Empty); add(string.Empty);
            //                            add("These are the current information of the service.");
            //                            add("-------------------------------------------------");

            //                            bool installed = false;
            //                            bool valid = false;
            //                            string imagePath = string.Empty;
            //                            string serviceStatus = string.Empty;
            //                            string serviceName = string.Empty;
            //                            string displayName = string.Empty;
            //                            string serviceType = string.Empty;

            //                            ServiceController[] scs = ServiceController.GetServices();
            //                            if (scs.Any(x => x.ServiceName == c.serviceName))
            //                            {
            //                                using (ServiceController sc = new ServiceController(c.serviceName))
            //                                {
            //                                    serviceName = sc.ServiceName;
            //                                    displayName = sc.DisplayName;
            //                                    serviceStatus = sc.Status.ToString();
            //                                    serviceType = sc.ServiceType.ToString();
            //                                    installed = true;
            //                                }
            //                            }

            //                            if (!installed)
            //                            {
            //                                add("-------------(SERVICE NOT INSTALLED)-------------");
            //                                add("-------------------------------------------------");
            //                            }
            //                            else
            //                            {
            //                                valid = Reg.IsValidServicePath(c.serviceName, out imagePath);
                                            
            //                                add("  Service name  :   " + serviceName);
            //                                add("  Display name  :   " + displayName);
            //                                add("  Start type    :   " + Reg.ServiceStartType(c.serviceName));
            //                                add("  Service type  :   " + serviceType);
            //                                add("  Status        :   " + serviceStatus);
            //                                add("  ImagePath     :   " + imagePath);
            //                                add("  Path valid    :   " + valid.ToString());
            //                            }
            //                            add(string.Empty); add(string.Empty);
            //                        }
            //                        break;
            //                    }

            //                case COMM.CLS:
            //                    {
            //                        rtb_details.Clear();
            //                        add(string.Empty);
            //                        add(Application.ProductName + " " + Application.ProductVersion);
            //                        add("https://www.facebook.com/CoinappTechnology\n");
            //                        add(string.Empty);
            //                        break;
            //                    }

            //                case COMM.MAIN:
            //                    {
            //                        if (input.Count() > 1)
            //                        {
            //                            if (input.Count() > 2)
            //                            {
            //                                add(input[1].ToUpper() + " is not accepting value.");
            //                            }
            //                            else
            //                            {
            //                                if (input[1].ToUpper() == "STOP")
            //                                {
            //                                    if (c.CONNECTED)
            //                                    {
            //                                        if (c.LOCKED)
            //                                        {
            //                                            add("You cannot stop the app in this session.");
            //                                            add("SESSION: " + c.SESSION + " [" + c.S.ToString(c.SESSION) + "]");
            //                                            return;
            //                                        }
            //                                        add("Stopping coinapp application...");
            //                                        c.mainPipe.WriteString(ARGS.STOP);
            //                                    }
            //                                    else
            //                                    {
            //                                        add("You cannot execute this action right now.");
            //                                    }
            //                                }
            //                                else if (input[1].ToUpper() == "RESTART")
            //                                {
            //                                    if (c.CONNECTED)
            //                                    {
            //                                        if (c.LOCKED)
            //                                        {
            //                                            add("You cannot restart the app in this session.");
            //                                            add("SESSION: " + c.SESSION + " [" + c.S.ToString(c.SESSION) + "]");
            //                                            return;
            //                                        }
            //                                        add("Restarting coinapp application...");
            //                                        c.mainPipe.WriteString(ARGS.RESTART);
            //                                    }
            //                                    else
            //                                    {
            //                                        add("You cannot execute this action right now.");
            //                                    }
            //                                }
            //                                else
            //                                {
            //                                    add(input[1].ToUpper() + " is not a command for main.");
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            add(string.Empty); add(string.Empty);
            //                            add("[MAIN]   COINAPP MAIN PROGRAM");
            //                            add("----------------------------------------------------------------");
            //                            add("To initiate TOP-LEVEL command, type MAIN command-name and enter.");
            //                            add(string.Empty);
            //                            add("List of command names:");
            //                            add(" * STOP        To stop this coinapp application.");
            //                            add(" * RESTART     To restart this coinapp application.");
            //                            add(string.Empty); add(string.Empty);
            //                        }
            //                        break;
            //                    }

            //                case COMM.HELP:
            //                    {
            //                        add(string.Empty); add(string.Empty);
            //                        add("[HELP]   COINAPP CONSOLE WINDOW");
            //                        add("--------------------------------------------------------");
            //                        add("For more information, type the command-name and enter.");
            //                        add("NOTE: all commands, options, values are CaSe iNsenSiTive");
            //                        add(string.Empty);
            //                        add("List of command names:");
            //                        add(" * APP        Information about aplication configuration.");
            //                        add(" * DEV        Information about device configuration.");
            //                        add(" * SVC        Information about the service configuration.");
            //                        add(" * CLS        To clear text in the command window. (No other info)");
            //                        add(" * MAIN       Sub-command (To restart app, to quit app, etc.)");
            //                        add(" * HELP       This is me. The information. (No other info)");
            //                        add(string.Empty);
            //                        break;
            //                    }

            //                case COMM.COINAPP:
            //                    {
            //                        Task.Run(async () =>
            //                        {
            //                            this.txtbx_command.ReadOnly = true;
            //                            string msg = await c.iConsoleMessage();
            //                            if (msg != string.Empty)
            //                            {
            //                                add(msg);
            //                                add(string.Empty);
            //                            }
            //                            this.txtbx_command.ReadOnly = false;
            //                        });
            //                        break;
            //                    }
                                
            //            }
            //        }
            //    }
            //}

            #endregion BACKUP
        }
        private void SetConfigSvc(List<string> data)
        {
            if (data.Count() > 2)
            {
                add("Service config-name is not accepting value.");
            }
            else
            {
                ServiceController[] scs = ServiceController.GetServices();
                ServiceController sc = scs.FirstOrDefault(s => s.ServiceName == c.serviceName);

                switch (data[1].ToUpper())
                {
                    case "INSTALL":
                        {
                            if (scs.Any(x => x.ServiceName == c.serviceName)) //Check if the service is installed.
                            {
                                add("Service is already installed.");
                            }
                            else
                            {
                                add("Installing service... ", false);
                                ProcessStartInfo svc = new ProcessStartInfo();
                                svc.FileName = "coinappSvc.exe";
                                svc.Arguments = "/i -s"; //Install parameter.
                                svc.UseShellExecute = false;
                                svc.WindowStyle = ProcessWindowStyle.Normal;
                                Process proc = Process.Start(svc);
                                proc.WaitForExit();

                                if (proc.ExitCode != 0)
                                {
                                    add(string.Empty);
                                    add("Cannot installing the service.");
                                    add("Windows system error code: " + proc.ExitCode);
                                }
                                else
                                {
                                    add("Done!"); add(string.Empty);
                                }
                            }
                            break;
                        }
                        
                    case "UNINSTALL":
                        {
                            if (!scs.Any(x => x.ServiceName == c.serviceName)) //Check if the service is installed.
                            {
                                add("Service is not installed.");
                            }
                            else
                            {
                                if (c.CONNECTED)
                                {
                                    add("You cannot execute this action right now.");
                                    return;
                                }
                                
                                if (sc != null)
                                {
                                    if (sc.Status == ServiceControllerStatus.Running)
                                    {
                                        add("Service is running. Stop the service first.");
                                        return;
                                    }
                                }



                                add("Service uninstalling...", false);
                                int exitCode;
                                using (Process process = new Process())
                                {
                                    ProcessStartInfo startInfo = process.StartInfo;
                                    startInfo.FileName = "sc";
                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                                    // tell Windows that the service should restart if it fails
                                    startInfo.Arguments = "delete " + c.serviceName;
                                    process.Start();
                                    process.WaitForExit();
                                    exitCode = process.ExitCode;
                                }

                                if (exitCode == 0)
                                {
                                    add("Done!");
                                }
                                else
                                {
                                    add(string.Empty); add("Cannot uninstalling the service.");
                                    add("Windows system error code: " + exitCode);
                                }
                            }
                            break;
                        }
                        
                    case "START":
                        {
                            if (sc == null)
                            {
                                add("You cannot start a service that not installed.");
                                return;
                            }
                            
                            if (c.CONNECTED)
                            {
                                add("You cannot execute this action right now.");
                                return;
                            }

                            if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                            {
                                add("Service is already " + sc.Status.ToString() + ".");
                                return;
                            }

                            Task.Run(() =>
                            {
                                add("Starting the service...", false);
                                sc.Start();
                                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(8000));

                                if (sc.Status != ServiceControllerStatus.Running)
                                {
                                    add("Failed!");
                                    add("Cannot start the service.");
                                    return;
                                }
                                add("Done!");
                            });
                            break;
                        }
                        
                    case "STOP":
                        {
                            if (sc == null)
                            {
                                add("You cannot stop a service that not installed.");
                                return;
                            }

                            if (c.CONNECTED)
                            {
                                add("You cannot execute this action right now.");
                                return;
                            }

                            if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
                            {
                                add("Service is already " + sc.Status.ToString() + ".");
                                return;
                            }

                            Task.Run(() =>
                            {
                                add("Stopping the service...", false);
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(8000));

                                if (sc.Status != ServiceControllerStatus.Stopped)
                                {
                                    add("Failed!");
                                    add("Cannot stop the service.");
                                    return;
                                }
                                add("Done!");
                            });

                            break;
                        }
                        
                    default: add(data[1].ToUpper() + " is not a service config-name.");
                        return;
                }
            }
        }
        private void SetConfigDev(List<string> data)
        {
            switch (data[1].ToUpper())
            {
                case COMM.PN: NEXT = COMM.COMPORT;
                    break;
                case COMM.BR: NEXT = COMM.BAUDRATE;
                    break;
                case COMM.CS: NEXT = COMM.COINSLOT;
                    break;
                case COMM.IV: NEXT = COMM.INTERVAL;
                    break;
                case COMM.C1: NEXT = COMM.COIN1;
                    break;
                case COMM.C2: NEXT = COMM.COIN2;
                    break;
                case COMM.C3: NEXT = COMM.COIN3;
                    break;
                case COMM.P1: NEXT = COMM.PULSE1;
                    break;
                case COMM.P2: NEXT = COMM.PULSE2;
                    break;
                case COMM.P3: NEXT = COMM.PULSE3;
                    break;
                default: add(data[1].ToUpper() + " is not a device config-name."); 
                    return;
            }

            if (data.Count() > 2)
            {
                if (data[1].ToUpper() == COMM.PN || data[1].ToUpper() == COMM.BR)
                {
                    add("Cannot change " + data[1].ToUpper() + " value directly. Type DEV " + data[1].ToUpper() + " instead.");
                    return;
                }

                if (tempInput != null) tempInput.Clear();
                else tempInput = new List<string>();

                tempInput.Add(data[2]);
                STEPS = 1;
            }
            else
            {
                STEPS = 0;
            }

            INPUT_DATA = true;
            SetConfig(NEXT, STEPS);

        }
        private void SetConfigApp(List<string> data)
        {
            switch (data[1].ToUpper())
            {
                case COMM.SD:
                    NEXT = COMM.SHUTDOWN;
                    break;

                case COMM.HK:
                    NEXT = COMM.HOTKEYCODE;
                    break;

                case COMM.PA:
                    NEXT = COMM.PASS;
                    break;

                case COMM.EM:
                    NEXT = COMM.EMAIL;
                    break;

                case COMM.LA:
                    NEXT = COMM.LOGINATTEMPT;
                    break;

                case COMM.LI:
                    NEXT = COMM.LOCKIMAGE;
                    break;

                case COMM.TM:
                    NEXT = COMM.TIMER;
                    break;

                case COMM.SN:
                    NEXT = COMM.SOUND;
                    break;

                case COMM.SV:
                    NEXT = COMM.SAVEDATA;
                    break;

                case COMM.LG: //LOG
                    break;

                case COMM.SU: //STARTUP
                    { 
                        if (data.Count() > 2)
                        {
                             if (data[2] == "0")
                            {
                                if (!Reg.StartupRegistryExists) add("You cannot remove a registry startup that not exists.");
                                else
                                {
                                    add("Removing startup registry...", false);
                                    if (Reg.DeleteStartupRegistry)
                                    {
                                        add("Done!");
                                        add("WARNING! This app will not run after reboot.");
                                    }
                                    else
                                    {
                                        add("Failed!");
                                        add("Cannot remove startup registy");
                                    }
                                }
                            }
                             else if (data[2] == "1")
                            {
                                if (Reg.StartupRegistryExists)
                                {
                                    add("Registry startup is already exists.");
                                    add("Removing startup registry...", false);
                                    if (Reg.DeleteStartupRegistry) add("Done!");
                                    else add("Failed!");
                                    add("Trying to add registry startup...", false);
                                }
                                else
                                {
                                    add("Adding registry startup...", false);
                                }

                                if (Reg.AddStartupRegistry) add("Done!");
                                else
                                {
                                    add("Failed!");
                                    add("Cannot add registry startup.");
                                }

                            }
                             else
                            {
                                add("Only 0 or 1 value is available for STARTUP config");
                            }
                        }
                        else
                        {
                            add(string.Empty); add(string.Empty);
                            add("[STARTUP]   Startup Registry");
                            add("--------------------------------------------------------");
                            add("To add into windows startup type APP STARTUP 1 and enter");
                            add("To remove into windows startup type APP STARTUP 0 and enter");
                            add(string.Empty);
                        }
                    }
                    return;

                case COMM.TK: //TASKMGR
                    {
                        if (data.Count() > 2)
                        {
                            if (data[2] == "0")
                            {
                                if (!Reg.DisableTaskmgrRegistryExists) add("You cannot remove a registry that not exists.");
                                else
                                {
                                    add("Removing disable taskmgr registry...", false);
                                    if (Reg.DeleteDisableTaskmgr) add("Done!");
                                    else
                                    {
                                        add("Failed!");
                                        add("Cannot remove disable taskmgr registy");
                                    }
                                }
                            }
                            else if (data[2] == "1")
                            {
                                if (Reg.DisableTaskmgrRegistryExists)
                                {
                                    add("Registry disable taskmgr is already exists.");
                                    add("Removing disable taskmgr registry...", false);
                                    if (Reg.DeleteDisableTaskmgr) add("Done!");
                                    else add("Failed!");
                                    add("Trying to add registry disable taskmgr...", false);
                                }
                                else
                                {
                                    add("Adding registry disable taskmgr...", false);
                                }

                                if (Reg.AddDisableTaskmgr) add("Done!");
                                else
                                {
                                    add("Failed!");
                                    add("Cannot add registry disable taskmgr.");
                                }

                            }
                            else
                            {
                                add("Only 0 or 1 value is available for TASKMGR config");
                            }
                        }
                        else
                        {
                            add(string.Empty); add(string.Empty);
                            add("[TASKMGR]   DisableTaskmgr Registry");
                            add("------------------------------------------------------------");
                            add("To disable default taskmanager type APP TASKMGR 1 and enter.");
                            add("To enable default taskmanger type APP TASKMGR 0 and enter.");
                            add(string.Empty);
                        }
                    }
                    return;

                default:
                    add(data[1].ToUpper() + " is not an application config-name.");
                    return;
            }

            if (data.Count() > 2)
            {
                if (data[1].ToUpper() == COMM.SV || data[1].ToUpper() == COMM.LI)
                {
                    add("Cannot change " + data[1].ToUpper() + " value directly. Type APP " + data[1].ToUpper()  + " instead.");
                    return;
                }

                if (tempInput != null ) tempInput.Clear();
                else tempInput = new List<string>();

                tempInput.Add(data[2]);
                STEPS = 1;
            }
            else
            {
                STEPS = 0;
            }

            INPUT_DATA = true;
            SetConfig(NEXT, STEPS);            
        }
        private void SetConfig(int next, int steps = 0)
        {
            switch (next)
            {
                case COMM.COMPORT: //Search for devices and ask for COM port. 
                    switch (steps)
                    {
                        case 0: //Seach for a connected device in this computer
                            {
                                add("Searching for connected devices...");
                                if (SerialPort.GetPortNames().Length <= 0) //Check if there is COM connected on computer.
                                {
                                    //No device detected on computer. Cancel installation.
                                    add("No devices found. Connect a device and try again.");
                                    //add("Type INSTALL or INSTALL ADVANCE in the command box and enter.");
                                    add(string.Empty);
                                    ENTER_INSTALL = false;
                                    INPUT_DATA = false;
                                    INSTALL_TYPE = 0;
                                    NEXT = 0; STEPS = 0;                                    
                                }
                                else //There is 1 or more connected device on computer.
                                {
                                    add(SerialPort.GetPortNames().Length > 1 ? "Found " + SerialPort.GetPortNames().Length + " devices:" : "Found 1 device:");
                                    foreach (string sp in SerialPort.GetPortNames())
                                    {
                                        add("* " + sp); //Display all COM ports connected on computer.
                                    }
                                    add("Select a COM PORT number.");
                                    add("Type COM# or number only of COM. (Example: " + SerialPort.GetPortNames()[0] + " or " + SerialPort.GetPortNames()[0].Replace("COM", string.Empty) + ")");
                                    add(string.Empty);

                                    STEPS++;
                                } 
                            }
                            break;

                        case 1: //Select a COM devices.
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted. //User inputed is empty.
                                    add("Something went wrong selecting COM PORT numbers. What did you trying do?");
                                else //User inputted detected.
                                {
                                    string inputComPort = tempInput[0];

                                    if (inputComPort.Length > 3) //Check if there are more than 3 characters in user inputted.
                                    {
                                        if (inputComPort.Substring(0, 3).ToUpper() != "COM") //Check if the first 3 characters are COM.
                                                                                             //The first 3 characters in user inputted is not COM.
                                            add(inputComPort.ToUpper() + " is not a valid COM PORT number.");
                                        else //The first 3 character in user inputted is COM.
                                        {
                                            //Then check if the subsequent of COM character is all numbers.
                                            bool isNumbers = true;
                                            for (int i = 0; i < inputComPort.Substring(3).Length; i++)
                                                if (char.IsDigit(inputComPort.Substring(3)[i]) == false) //Check all the characters subsequent by the COM.
                                                    isNumbers = false; //If found not a number in all characters subsequent by the COM. return a false.

                                            if (!isNumbers)
                                                add(inputComPort.ToUpper() + " is not a valid COM PORT number.");
                                            else
                                            {
                                                bool foundPort = false;
                                                foreach (string port in SerialPort.GetPortNames()) //
                                                    if (inputComPort.ToUpper() == port) //Compare all connected device COM PORTS to user COM# inputted.
                                                        foundPort = true;

                                                if (foundPort == false)
                                                    add(inputComPort.ToUpper() + " is not available in the connected devices.");
                                                else
                                                {

                                                    add(inputComPort.ToUpper() + " is selected.");
                                                    add(string.Empty);

                                                    if (ENTER_INSTALL)
                                                    {
                                                        Config.PORTNUMBER = Convert.ToInt32(inputComPort.Substring(3)); //Store the number only in COM in an arraylist[0] index to save in to the settings later on.
                                                        
                                                        if (INSTALL_TYPE != 2)
                                                        {
                                                            add("Testing device is now begin...");
                                                            add("Connecting to " + inputComPort.ToUpper() + " device with " + c.BAUDRATE[2] + " default baud rate...");
                                                            add(string.Empty);

                                                            TestSerialDevice(inputComPort.ToUpper(), c.BAUDRATE[2]);
                                                        }
                                                        else
                                                        {
                                                            NEXT++;
                                                            STEPS = 0;
                                                            this.SetConfig(NEXT, STEPS);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Settings.SetPortNumber(Convert.ToInt32(inputComPort.Substring(3)));
                                                        INPUT_DATA = false;
                                                        NEXT = 0; STEPS = 0;
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        bool isNumbers = true;
                                        for (int i = 0; i < inputComPort.Length; i++)
                                            if (char.IsDigit(inputComPort[i]) == false)
                                                isNumbers = false;

                                        if (!isNumbers)
                                            add(inputComPort.ToUpper() + " is not a valid COM PORT number.");
                                        else
                                        {
                                            bool foundPort = false;
                                            foreach (string port in SerialPort.GetPortNames())
                                                if ("COM" + inputComPort == port)
                                                    foundPort = true;

                                            if (foundPort == false)
                                                add(inputComPort + " is not available in the connected devices.");
                                            else
                                            {
                                                add(inputComPort + " is selected.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.PORTNUMBER = Convert.ToInt32(inputComPort); // tempConfig.Add(inputComPort); //Store it in an arraylist[0] index to save in to the settings later on.

                                                    if (INSTALL_TYPE != 2)
                                                    {
                                                        add("Testing device is now begin...");
                                                        add("Connecting to COM" + inputComPort.ToUpper() + " device with " + c.BAUDRATE[2] + " default baud rate...");
                                                        add(string.Empty);

                                                        TestSerialDevice("COM" + inputComPort, c.BAUDRATE[2]);
                                                    }
                                                    else
                                                    {
                                                        NEXT++;
                                                        STEPS = 0;
                                                        this.SetConfig(NEXT, STEPS);
                                                    }
                                                }
                                                else
                                                {
                                                    Settings.SetPortNumber(Convert.ToInt32(inputComPort));
                                                    INPUT_DATA = false;
                                                    NEXT = 0; STEPS = 0;
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    break;
                    
                case COMM.BAUDRATE: //[ADVANCE] Display a baud rate which user will select.
                    switch (steps)
                    {
                        case 0: //Display a BAUD RATE list which user will select.
                            {
                                add("[ADVANCED] Select a BAUD RATE number on the list:");
                                foreach (int i in c.BAUDRATE)
                                    add("* " + i);

                                add(string.Empty);
                                add("The default BAUD RATE of Coinapp is 19200.");
                                add("If you select other than 19200 it will not work. This is for future use only.");
                                add(string.Empty);

                                STEPS++;
                            }
                            break;

                        case 1: //Get the selected baud rate by user.
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong selecting BAUD RATE numbers. What did you trying do?");
                                else
                                {
                                    string inputBaudRate = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputBaudRate.Length; i++)
                                        if (char.IsDigit(inputBaudRate[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputBaudRate.ToUpper() + " is not a valid BAUD RATE number.");
                                    else
                                    {
                                        if (int.TryParse(inputBaudRate, out int ret) == false)
                                            add(inputBaudRate + " is not a valid BAUD RATE.");
                                        else
                                        {

                                            bool foundBaudRate = false;
                                            foreach (int br in c.BAUDRATE)
                                                if (ret == br)
                                                    foundBaudRate = true;

                                            if (foundBaudRate == false)
                                                add(inputBaudRate + " is not available in the BAUD RATE list.");
                                            else
                                            {
                                                add(inputBaudRate + " is selected for BAUD RATE device.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.BAUDRATE = ret;// tempConfig.Add(ret); //Store the selected baud of user in arraylist[1] index and save it later.                                                  

                                                    add("Testing device is now begin...");
                                                    add("Connecting to COM" + Config.PORTNUMBER + " device with " + inputBaudRate + " baud rate...");
                                                    add(Convert.ToInt32(inputBaudRate) != c.BAUDRATE[2] ? "WARNING: The default baud rate for Coinapp application is " + c.BAUDRATE[2] : string.Empty);
                                                    add(string.Empty, Convert.ToInt32(inputBaudRate) != c.BAUDRATE[2] ? true : false);
                                                    
                                                    TestSerialDevice("COM" + Config.PORTNUMBER, Convert.ToInt32(inputBaudRate));
                                                }
                                                else
                                                {
                                                    Settings.SetBaudRate(ret);
                                                    INPUT_DATA = false;
                                                    NEXT = 0; STEPS = 0;
                                                }

                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.COINSLOT: //Display a coin slot type which user will select.
                    switch (steps)
                    {
                        case 0: //List an available coin-slot type.
                            {
                                add("Select a COIN-SLOT number according to the type.");
                                foreach (int i in c.COINSLOT)
                                    add("* " + i + " [" + c.CoinslotToString(i) + "]");

                                add(string.Empty);
                                add("Type a number for COIN-SLOT type and enter. (Example: 0 or 1)");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;

                        case 1: //Get the selected user inputted for coin-slot type.
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong selecting COIN SLOT numbers. What did you trying do?");
                                else
                                {
                                    string inputCoinSlot = tempInput[0];

                                    if (inputCoinSlot.Length > 1)
                                        add(inputCoinSlot.ToUpper() + " is not valid COIN SLOT number.");
                                    else
                                    {
                                        if (int.TryParse(inputCoinSlot, out int ret) == false)
                                            add(inputCoinSlot.ToUpper() + " is not valid COIN SLOT number.");
                                        else
                                        {
                                            bool foundCoinSlot = false;
                                            foreach (int i in c.COINSLOT)
                                                if (ret == i)
                                                    foundCoinSlot = true;

                                            if (foundCoinSlot == false)
                                                add(ret + " is not in the available COIN SLOT list.");
                                            else
                                            {
                                                add(ret + " [" + c.CoinslotToString(ret) + "] is selected as COIN SLOT.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.COINSLOT = ret; // tempConfig.Add(ret); //Store coin slot type number in arraylist[2] index. save it later.
                                                    TEMP.CoinSlot = ret;

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetCoinSlot(ret);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.COIN1: //Coin 1
                    switch (steps)
                    {
                        case 0:
                            {
                                add("COIN1. Type a number (in seconds) then enter. (Example: 300)");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in COIN1 numbers. What did you trying do?");
                                else
                                {
                                    string inputCoin1 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputCoin1.Length; i++)
                                        if (char.IsDigit(inputCoin1[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputCoin1.ToUpper() + " is not a valid number for seconds.");
                                    else
                                    {
                                        if (int.TryParse(inputCoin1, out int time) == false)
                                            add("Maximum integer value is not valid for COIN1.");
                                        else
                                        {

                                            if (time == 0)
                                                add("Zero is not valid for COIN1.");
                                            else
                                            {
                                                add(c.TimeToString(time) + " for COIN1.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.COIN1 = time;// tempConfig.Add(time); //Save coin1 seconds in arraylist[3] index. Save it later.
                                                    
                                                    switch (TEMP.CoinSlot)
                                                    {
                                                        case 0:
                                                            NEXT = COMM.SHUTDOWN; // 10; //Skip to shutdown. Legacy coin slot.
                                                            break;

                                                        case 1: //Skip to shutdown. Single coin slot
                                                            NEXT = COMM.SHUTDOWN; // 10;
                                                            break;

                                                        case 2:  //Skip to coin2. Dual coin slot.
                                                            NEXT = COMM.COIN2;
                                                            break;

                                                        case 3:
                                                            {
                                                                if (INSTALL_TYPE == 2)
                                                                    NEXT++;
                                                                else
                                                                    NEXT = COMM.COIN2;
                                                            }
                                                            break;
                                                    }
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetCoin1(time);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.PULSE1: //Pulse 1 ADVANCED
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Change number for PULSE1 (Default is 1).");
                                add("Type a number then enter.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in PULSE1 numbers. What did you trying do?");
                                else
                                {
                                    string inputPulse1 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputPulse1.Length; i++)
                                        if (char.IsDigit(inputPulse1[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputPulse1.ToUpper() + " is not a valid number for PULSE1.");
                                    else
                                    {
                                        if (int.TryParse(inputPulse1, out int pulse) == false)
                                            add("Maximum integer value is not valid for PULSE1.");
                                        else
                                        {
                                            if (pulse == 0)
                                                add("Zero is not valid for PULSE1.");
                                            else
                                            {
                                                add(inputPulse1 + " pulse for PULSE1.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.PULSE1 = pulse; // tempConfig.Add(pulse); //Save Pulse1 in arraylist[4] index. Save it later.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetPulse1(pulse);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.COIN2: //Coin 2
                    switch (steps)
                    {
                        case 0:
                            {
                                add("COIN2. Type a number (in seconds) then enter. (Example: 1500)");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in COIN2 numbers. What did you trying do?");
                                else
                                {
                                    string inputCoin2 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputCoin2.Length; i++)
                                        if (char.IsDigit(inputCoin2[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputCoin2.ToUpper() + " is not a valid number for seconds.");
                                    else
                                    {
                                        if (int.TryParse(inputCoin2, out int time) == false)
                                            add("Maximum integer value is not valid for COIN2.");
                                        else
                                        {
                                            if (time == 0)
                                                add("Zero is not valid for COIN2.");
                                            else
                                            {
                                                add(c.TimeToString(time) + " for COIN2.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.COIN2 = time; // tempConfig.Add(time); //Save coin2 seconds in arraylist[5] index. Save it later.

                                                    if (TEMP.CoinSlot == 2) //Dual coin slot type.
                                                        NEXT = COMM.SHUTDOWN;// 10; //Skip to shutdown.
                                                    else //Single or multi coin slot type.
                                                    {
                                                        if (INSTALL_TYPE == 2) //Advance configurations
                                                            NEXT++; //next to pulse2
                                                        else
                                                            NEXT = COMM.COIN3; // 8; //Skip to coin3. Not advance configurations.
                                                    }
                                                    
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetCoin2(time);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.PULSE2: //Pulse 2
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Change number for PULSE2 (Default is 5).");
                                add("Type a number then enter.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in PULSE2 numbers. What did you trying do?");
                                else
                                {
                                    string inputPulse2 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputPulse2.Length; i++)
                                        if (char.IsDigit(inputPulse2[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputPulse2.ToUpper() + " is not a valid number for PULSE2.");
                                    else
                                    {
                                        if (int.TryParse(inputPulse2, out int pulse) == false)
                                            add("Maximum integer value is not valid for PULSE2.");
                                        else
                                        {
                                            if (pulse == 0)
                                                add("Zero is not valid for PULSE2.");
                                            else
                                            {
                                                add(inputPulse2 + " pulse for PULSE2.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.PULSE2 = pulse; // tempConfig.Add(pulse); //Save Pulse1 in arraylist[6] index. Save it later.
                                                    
                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetPulse2(pulse);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.COIN3: //Coin 3
                    switch (steps)
                    {
                        case 0:
                            {
                                add("COIN3. Type a number (in seconds) then enter. (Example: 3000)");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in COIN3 numbers. What did you trying do?");
                                else
                                {
                                    string inputCoin3 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputCoin3.Length; i++)
                                        if (char.IsDigit(inputCoin3[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputCoin3.ToUpper() + " is not a valid number for seconds.");
                                    else
                                    {
                                        if (int.TryParse(inputCoin3, out int time) == false)
                                            add("Maximum integer value is not valid for COIN3.");
                                        else
                                        {
                                            if (time == 0)
                                                add("Zero is not valid for COIN3.");
                                            else
                                            {
                                                add(c.TimeToString(time) + " for COIN3.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.COIN3 = time; // tempConfig.Add(time); //Save coin3 seconds in arraylist[7] index. Save it later.

                                                    if (INSTALL_TYPE == 2)
                                                        NEXT++;
                                                    else
                                                        NEXT = COMM.SHUTDOWN; // 10;

                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetCoin3(time);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.PULSE3: //Pulse 3
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Change number for PULSE3 (Default is 10).");
                                add("Type a number then enter.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in PULSE3 numbers. What did you trying do?");
                                else
                                {
                                    string inputPulse3 = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputPulse3.Length; i++)
                                        if (char.IsDigit(inputPulse3[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputPulse3.ToUpper() + " is not a valid number for PULSE3.");
                                    else
                                    {
                                        if (int.TryParse(inputPulse3, out int pulse) == false)
                                            add("Maximum integer value is not valid for PULSE3.");
                                        else
                                        {
                                            if (pulse == 0)
                                                add("Zero is not valid for PULSE3.");
                                            else
                                            {
                                                add(inputPulse3 + " pulse for PULSE3.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.PULSE3 = pulse; // tempConfig.Add(pulse); //Save Pulse3 seconds in arraylist[8] index. Save it later.
                                                    
                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetPulse3(pulse);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.SHUTDOWN: //Shutdown
                    switch (steps)
                    {
                        case 0:
                            {
                                add("Type a number (in seconds) for SHUTDOWN time. (Example: 120)");
                                add("Note: Minimum is 20 seconds and maximum of 300 seconds or 5 minutes.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong in SHUTDOWN numbers. What did you trying do?");
                                else
                                {
                                    string inputShutdown = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputShutdown.Length; i++)
                                        if (char.IsDigit(inputShutdown[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputShutdown.ToUpper() + " is not a valid number for seconds.");
                                    else
                                    {
                                        if (int.TryParse(inputShutdown, out int time) == false)
                                            add("Maximum integer value is not valid for SHUTDOWN time.");
                                        else
                                        {
                                            if (time < 20 || time > 300)
                                            {
                                                add(inputShutdown + " is not accepted for SHUTDOWN time.");
                                                add("Minimum is 20 seconds and maximum of 300 seconds or 5 minutes is valid.");
                                            }
                                            else
                                            {
                                                add(c.TimeToString(time) + " for SHUTDOWN.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.SHUTDOWN = time; // tempConfig.Add(time); //Save coin3 seconds in arraylist[9] index. Save it later.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetShutdown(time);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.LOGINATTEMPT: //Login attempt
                    switch (steps)
                    {
                        case 0:
                            {
                                add("Type a number for LOGIN ATTEMPT retry then enter. (Example: 3)");
                                add("Note: The minimum retry number is 3.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;
                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                {
                                    if (INPUT_DATA) INPUT_DATA = false;
                                    add("Something went wrong in LOGIN ATTEMPT retry numbers. What did you trying do?");
                                }
                                else
                                {
                                    string inputLoginAttempt = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputLoginAttempt.Length; i++)
                                        if (char.IsDigit(inputLoginAttempt[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputLoginAttempt.ToUpper() + " is not a valid number for LOGIN ATTEMPT.");
                                    else
                                    {
                                        if (int.TryParse(inputLoginAttempt, out int retry) == false)
                                            add("Maximum integer value is not valid for LOGIN ATTEMPT.");
                                        else
                                        {
                                            if (retry < 3)
                                                add("The minimum retry number for LOGIN ATTEMPT is 3.");
                                            else
                                            {
                                                add(inputLoginAttempt + " retry for LOGIN ATTEMPT.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.LOGINATTEMPT = retry; // tempConfig.Add(retry); //Save login attempt in arraylist[10] index. Save it later.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetLoginAttempt(retry);
                                                    add(string.Empty);
                                                    NEXT = 0; STEPS = 0;
                                                }

                                            }

                                        }

                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.HOTKEYCODE: //Hotkey code
                    switch (steps)
                    {
                        case 0:
                            {
                                add("Create your Hotkey (CTRL + ALT + SHIFT + 'TheKeyYouPressed').");
                                add("Press any key on your keyboard then click enter.");
                                add(string.Empty);
                                ENTER_HOTKEY = true;

                                Task.Run(async () =>
                                {
                                    await Task.Delay(50);
                                    txtbx_command.Text = "CTRL + ALT + SHIFT + ";
                                    txtbx_command.SelectionStart = txtbx_command.Text.Length;
                                    btn_enter.Enabled = false;
                                    txtbx_command.ReadOnly = true;
                                });

                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (TEMP.HotkeyCode == 0)
                                {
                                    if (INPUT_DATA)
                                    {
                                        INPUT_DATA = false;
                                        add("Cannot change HOTKEY value directly. Type APP HOTKEY instead.");
                                    }
                                    else add("Something went wrong in HOTKEY. What did you trying do?");
                                }
                                else
                                {
                                    ENTER_HOTKEY = false;
                                    txtbx_command.ReadOnly = false;

                                    add("This is your Hotkey CTRL + ALT + SHIFT + " + HotkeyCode.ToString(TEMP.HotkeyCode));
                                    add(string.Empty);

                                    if (ENTER_INSTALL)
                                    {
                                        Config.HOTKEY = TEMP.HotkeyCode; // tempConfig.Add(tempHotkeyCode); //Store hotkey code in arraylist[11] index. Save it later.
                                        TEMP.HotkeyCode = 0;

                                        NEXT++;
                                        STEPS = 0;
                                        this.SetConfig(NEXT, STEPS);
                                    }
                                    else
                                    {
                                        Settings.SetHotkeyCode(TEMP.HotkeyCode);
                                        INPUT_DATA = false;
                                        NEXT = 0; STEPS = 0;
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.EMAIL: //Recovery email address
                    switch (steps)
                    {
                        case 0:
                            {
                                add("Create your RECOVERY EMAIL ADDRESS.");
                                add("Note: enter a valid email address format to enable the enter button.");
                                add(string.Empty);

                                ENTER_EMAIL = true;
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (TEMP.Email == string.Empty)
                                {
                                    if (INPUT_DATA)
                                    {
                                        INPUT_DATA = false;
                                        add("Cannot change EMAIL value directly. Type APP EMAIL instead.");
                                    }
                                    else add("Something went wrong creating RECOVERY EMAIL ADDRESS.");
                                }
                                else
                                {
                                    add(TEMP.Email + " is your RECOVERY EMAIL ADDRESS.");
                                    add(string.Empty);

                                    TEMP.ValidEmail = false;
                                    ENTER_EMAIL = false;

                                    if (ENTER_INSTALL)
                                    {
                                        Config.EMAIL = TEMP.Email; // tempConfig.Add(tempEmail); //Save email address in arraylist[12] index.
                                        TEMP.Email = string.Empty;
                                        
                                        NEXT++;
                                        STEPS = 0;
                                        this.SetConfig(NEXT, STEPS);
                                    }
                                    else
                                    {
                                        Settings.WriteSysFile(Settings.emailFile, TEMP.Email);
                                        TEMP.Email = string.Empty;
                                        INPUT_DATA = false;
                                        NEXT = 0; STEPS = 0;
                                    }
                                    
                                }
                            }                       
                            break;
                    }
                    break;

                case COMM.PASS:
                    switch (steps)
                    {
                        case 0:
                            {
                                txtbx_command.UseSystemPasswordChar = true;

                                add("Create your PASSWORD.");
                                add("Press tab to show the password and press tab again to hide (vice versa).");
                                add(string.Empty);
                                
                                ENTER_PASS = true;
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (TEMP.Pass1 == string.Empty)
                                {
                                    if (INPUT_DATA)
                                    {
                                        INPUT_DATA = false;
                                        add("Cannot change PASS value directly. Type APP PASS instead.");
                                    }
                                    else add("Something wrong with the PASSWORD. What are you trying to do?");
                                }
                                else
                                {
                                    add("Password ENTERED.");
                                    add("Enter again your password to verify.");
                                    add(string.Empty);
                                    if (txtbx_command.UseSystemPasswordChar == false)
                                        txtbx_command.UseSystemPasswordChar = true;
                                    
                                    STEPS++;
                                }
                            }
                            break;
                            
                        case 2:
                            {
                                if (TEMP.Pass2 == string.Empty)
                                    add("Something wrong with the PASSWORD verification. What are you trying to do?");
                                else
                                {
                                    if (TEMP.Pass1 != TEMP.Pass2)
                                    {
                                        add("Entered password verification MISMATCH. Please try again");
                                        add("(OR press ESCAPE to creating password again)");
                                        add(string.Empty);
                                    }
                                    else
                                    {
                                        add("Password created successfully.");
                                        add(string.Empty);
                                        ENTER_PASS = false;
                                        TEMP.Pass2 = string.Empty;
                                        txtbx_command.UseSystemPasswordChar = false;
                                        
                                        if (ENTER_INSTALL)
                                        {
                                            Config.PASS = TEMP.Pass1; // tempConfig.Add(tempPass1); //Store password in arraylist[13] index.
                                            TEMP.Pass1 = string.Empty;
                                            
                                            switch (INSTALL_TYPE)
                                            {
                                                case 0: //Default configurations
                                                    NEXT = COMM.SAVE_SETTINGS; // 21 Skip to install service.
                                                    break;
                                                case 1: //Basic configurations.
                                                    NEXT = COMM.SHOW_SETTINGS; // 20 = Skip to install verification.
                                                    break;
                                                case 2: //Advance configuraitons 
                                                    NEXT++;
                                                    break;
                                            }

                                            STEPS = 0;
                                            this.SetConfig(NEXT, STEPS);
                                        }
                                        else
                                        {
                                            Settings.WriteSysFile(Settings.passFile, TEMP.Pass1);
                                            TEMP.Pass1 = string.Empty;
                                            INPUT_DATA = false;
                                            NEXT = 0; STEPS = 0;
                                        }
  
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.TIMER:
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Enable TIMER DISPLAY on desktop window.");
                                add("* 0 for NO");
                                add("* 1 for YES");
                                add("* 2 to force enable both");
                                add(string.Empty);
                                add("If NO, be sure you have TM1637 DISPLAY module connected in device.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong setting TIMER DISPLAY. What did you trying do?");
                                else
                                {
                                    string inputDisplay = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputDisplay.Length; i++)
                                        if (char.IsDigit(inputDisplay[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputDisplay.ToUpper() + " is not valid input for DISPLAY.");
                                    else
                                    {
                                        if (int.TryParse(inputDisplay, out int ret) == false)
                                        {
                                            if (INPUT_DATA) INPUT_DATA = false;
                                            add("Maximun integer value is not a valid input.");
                                        }
                                        else
                                        {
                                            if (ret < 0 || ret > 2)
                                                add("Only 0, 1 or 2 is accepted input.");
                                            else
                                            {
                                                if (ret == 0)
                                                    add("TIMER DISPLAY will not show on desktop window.");
                                                else if (ret == 1)
                                                    add("TIMER DISPLAY will be show on desktop window.");
                                                else
                                                    add("TIMER DISPLAY will be shown both.");

                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.TIMER = ret; // tempConfig.Add(ret); //Store display in arraylist[14] index.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetTimer(ret);
                                                    NEXT = 0; STEPS = 0;
                                                }

                                            }
                                        }
                                    }

                                }
                            }
                            break;
                    }
                    break;

                case COMM.SOUND:
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Enable APPLICATION SOUND effect on desktop.");
                                add("* 0 for NO");
                                add("* 1 for YES");
                                add("* 2 for Super Mario sound theme.");
                                add(string.Empty);
                                add("If NO, be sure you have BEEP SOUND module connected in device.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong setting SOUND. What did you trying do?");
                                else
                                {
                                    string inputSound = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputSound.Length; i++)
                                        if (char.IsDigit(inputSound[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputSound.ToUpper() + " is not valid input for SOUND.");
                                    else
                                    {
                                        if (int.TryParse(inputSound, out int ret) == false)
                                            add("Maximun integer value is not a valid input.");
                                        else
                                        {
                                            if (ret < 0 || ret > 2)
                                                add("Only 0, 1 or 2 is accepted input.");
                                            else
                                            {
                                                if (ret == 0)
                                                    add("APPLICATION SOUND will not play on desktop.");
                                                else if (ret == 1)
                                                    add("APPLICATION SOUND will be play on desktop.");
                                                else
                                                    add("APPLICATION SOUND plays Super Mario theme.");

                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.SOUND = ret; // tempConfig.Add(ret); //Store sound in arraylist[15] index.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetSound(ret);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.INTERVAL:
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Change TIME INTERVAL in device timer. (Default is 1000)");
                                add("Enter a number in milliseconds.");
                                add(string.Empty);
                                STEPS++;
                            }
                            break;

                        case 1:
                            {
                                if (INPUT_DATA) INPUT_DATA = false;

                                if (tempInput.Count <= 0) //Check if there is user inputted.
                                    add("Something went wrong changing TIME INTERVAL. What did you trying do?");
                                else
                                {
                                    string inputInterval = tempInput[0];
                                    bool isNumbers = true;
                                    for (int i = 0; i < inputInterval.Length; i++)
                                        if (char.IsDigit(inputInterval[i]) == false)
                                            isNumbers = false;

                                    if (!isNumbers)
                                        add(inputInterval.ToUpper() + " is not valid number for TIME INTERVAL.");
                                    else
                                    {
                                        if (int.TryParse(inputInterval, out int ret) == false)
                                            add("Maximun integer value is not valid for TIME INTERVAL.");
                                        else
                                        {
                                            if (ret <= 0)
                                                add("Zero or negative numbers is not valid for TIME INTERVAL.");
                                            else
                                            {
                                                add(inputInterval + " milliseconds for TIME INTERVAL.");
                                                add(string.Empty);

                                                if (ENTER_INSTALL)
                                                {
                                                    Config.INTERVAL = ret; // tempConfig.Add(ret); //Store time interval in arraylist[16] index.

                                                    NEXT++;
                                                    STEPS = 0;
                                                    this.SetConfig(NEXT, STEPS);
                                                }
                                                else
                                                {
                                                    Settings.SetInterval(ret);
                                                    NEXT = 0; STEPS = 0;
                                                }
                                                
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    break;

                case COMM.SAVEDATA:
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Set folder location for SAVE DATA.");

                                if (c.PreviousSaveDataFolderPath == string.Empty)
                                    goto showDialog;
                                else
                                {
                                    if (!My.RestoreTimeResume(out _))
                                        goto showDialog;
                                    else
                                    {
                                        DialogResult dialogResult = MessageBox.Show("There is a previous data in SAVE DATA folder containing the REMAINING TIME.\nDo you want to remove the data and change the SAVE DATA folder location?", "Coinapp SAVE DATA", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                        if (dialogResult != DialogResult.Yes)
                                        {
                                            add("Selecting SAVE DATA folder location canceled.");
                                            add("Click ENTER BUTTON to try again OR press ESCAPE to cancel.");
                                            add(string.Empty);
                                            SKIP_PATH = true;
                                            btn_enter.Enabled = true;
                                        }
                                        else
                                        {
                                            add("Previous remaining time deleted.");
                                            goto showDialog;
                                        }

                                    }
                                }

                                break;
                            showDialog:

                                if (c.INITIALIZED)
                                {
                                    c.app.BeginInvoke(() => SavedataFinalizer(c.SaveDataFolder(this, out string zPath), zPath));
                                }
                                else
                                {
                                    SavedataFinalizer(c.SaveDataFolder(this, out string zPath), zPath);
                                } 
                            }
                            break;
                    }
                    break;

                case COMM.LOCKIMAGE:
                    switch (steps)
                    {
                        case 0:
                            {
                                add("[ADVANCED] Select image file for LOCK IMAGE.");

                                if (c.INITIALIZED)
                                {
                                    c.app.BeginInvoke(() => LockimageFinalizer(c.LockImagePath(this, out string zfile), zfile));
                                }
                                else
                                {
                                    LockimageFinalizer(c.LockImagePath(this, out string zfile), zfile);
                                }
                            }
                            break;
                    }
                    break;

                case COMM.SHOW_SETTINGS:                  
                    switch (steps)
                    {
                        case 0:
                            {
                                add(string.Empty);
                                add("Your " + (INSTALL_TYPE > 1 ? "ADVANCE" : "BASIC") + " configuration is:");
                                add(string.Empty);
                                add("Device COM PORT  :   COM" + Config.PORTNUMBER);

                                if (INSTALL_TYPE == 1) //Basic configurations.
                                {
                                    add("COIN-SLOT type   :   " + Config.COINSLOT + " [" + c.CoinslotToString(Config.COINSLOT) + "]");
                                    add("COIN 1           :   " + c.TimeToString(Config.COIN1));

                                    if (Config.COINSLOT == 2) // Dual coin slot.
                                    {
                                        add("COIN 2           :   " + c.TimeToString(Config.COIN2));
                                    }
                                    else if (Config.COINSLOT == 3) //Multi coin slot.
                                    {
                                        add("COIN 2           :   " + c.TimeToString(Config.COIN2));
                                        add("COIN 3           :   " + c.TimeToString(Config.COIN3));
                                    }

                                    add("SHUTDOWN time    :   " + c.TimeToString(Config.SHUTDOWN));
                                    add("LOGIN ATTEMPT    :   " + Config.LOGINATTEMPT + " retry");
                                    add("HOTKEY           :   " + "CTRL + ALT + SHIFT + " + HotkeyCode.ToString(Config.HOTKEY));
                                    add("RECOVERY E-MAIL  :   " + Config.EMAIL);
                                    add("PASSWORD         :   " + Config.PASS);

                                }
                                else if (INSTALL_TYPE == 2) //Advance configurations.
                                {                                   
                                    add("Device BAUD RATE :   " + Config.BAUDRATE);
                                    add("COIN-SLOT type   :   " + Config.COINSLOT + " [" + c.CoinslotToString(Config.COINSLOT) + "]");
                                    add("COIN 1           :   " + c.TimeToString(Config.COIN1));

                                    if (Config.COINSLOT == 2) // Dual coin slot.
                                    {
                                        add("COIN 2           :   " + c.TimeToString(Config.COIN2));
                                    }
                                    else if (Config.COINSLOT == 3) //Multi coin slot.
                                    {
                                        add("PULSE 1          :   " + Config.PULSE1 + " pulse");
                                        add("COIN 2           :   " + c.TimeToString(Config.COIN2));
                                        add("PULSE 2          :   " + Config.PULSE2 + " pulse");
                                        add("COIN 3           :   " + c.TimeToString(Config.COIN3));
                                        add("PULSE 3          :   " + Config.PULSE3 + " pulse");
                                    }

                                    add("SHUTDOWN time    :   " + c.TimeToString(Config.SHUTDOWN));
                                    add("LOGIN ATTEMPT    :   " + Config.LOGINATTEMPT + " retry");
                                    add("HOTKEY           :   " + "CTRL + ALT + SHIFT + " + HotkeyCode.ToString(Config.HOTKEY));
                                    add("RECOVERY E-MAIL  :   " + Config.EMAIL);
                                    add("PASSWORD         :   " + Config.PASS);
                                    add("TIMER          :   " + Config.TIMER + " (" + (Config.TIMER > 0 ? "Show timer display on desktop window)" : "No timer display on desktop window)"));
                                    add("SOUND            :   " + Config.SOUND + " (" + (Config.SOUND > 0 ? "Play application sound effect)" : "No application sound effect)"));
                                    add("TIME INTERVAL    :   " + Config.INTERVAL + " milliseconds");
                                    add("SAVE DATA folder :   " + Config.SAVEDATA_PATH);
                                    add("LOCK IMAGE       :   " + Config.LOCKIMAGE);
                                }

                                add(string.Empty);
                                NEXT++;
                                STEPS = 0;
                                this.SetConfig(NEXT, STEPS);

                            }
                            break;

                    }
                    break;

                case COMM.SAVE_SETTINGS:
                    {
                        add("Saving configurations... ", false);

                        Settings.CreateDefaultConfigFile();
                        Settings.SetPortNumber(Config.PORTNUMBER);

                        if (INSTALL_TYPE == 0)
                        {
                            Settings.SetBaudRate(c.BAUDRATE[2]);
                            Settings.SetCoin1(300);
                            Settings.SetPulse1(1);
                            Settings.SetCoinSlot(0);
                            Settings.SetShutdown(120);
                            Settings.SetLoginAttempt(5);
                        }
                        else if (INSTALL_TYPE == 1) //Basic configurations.
                        {
                            Settings.SetBaudRate(c.BAUDRATE[2]);
                            Settings.SetCoin1(Config.COIN1);
                            Settings.SetCoinSlot(Config.COINSLOT);

                            if (Config.COINSLOT == 2) // Dual coin slot.
                            {
                                Settings.SetPulse1(1);
                                Settings.SetCoin2(Config.COIN2);
                                Settings.SetPulse2(1);
                            }
                            else if (Config.COINSLOT == 3) //Multi coin slot.
                            {
                                Settings.SetCoin1(Config.COIN1);
                                Settings.SetPulse1(1);
                                Settings.SetCoin2(Config.COIN2);
                                Settings.SetPulse2(5);
                                Settings.SetCoin3(Config.COIN3);
                                Settings.SetPulse3(10);
                            }
                            else
                            {
                                Settings.SetPulse1(1);
                            }

                            Settings.SetShutdown(Config.SHUTDOWN);
                            Settings.SetLoginAttempt(Config.LOGINATTEMPT);
                        }
                        else if (INSTALL_TYPE == 2) //Advance configurations.
                        {
                            Settings.SetBaudRate(c.BAUDRATE[2]);
                            Settings.SetCoin1(Config.COIN1);
                            Settings.SetCoinSlot(Config.COINSLOT);

                            if (Config.COINSLOT == 2) // Dual coin slot.
                            {
                                Settings.SetPulse1(1);
                                Settings.SetCoin2(Config.COIN2);
                                Settings.SetPulse2(1);
                            }
                            else if (Config.COINSLOT == 3) //Multi coin slot.
                            {
                                Settings.SetPulse1(Config.PULSE1);
                                Settings.SetCoin2(Config.COIN2);
                                Settings.SetPulse2(Config.PULSE2);
                                Settings.SetCoin3(Config.COIN3);
                                Settings.SetPulse3(Config.PULSE3);
                            }
                            Settings.SetShutdown(Config.SHUTDOWN);
                            Settings.SetLoginAttempt(Config.LOGINATTEMPT);
                            Settings.SetTimer(Config.TIMER);
                            Settings.SetSound(Config.SOUND);
                            Settings.SetInterval(Config.INTERVAL);
                            Settings.SetSavedata(Config.SAVEDATA);

                            if (Config.SAVEDATA == 1)
                                Settings.WriteSysFile(Settings.savedataPath, Config.SAVEDATA_PATH);

                            Settings.SetLockImage(Config.LOCKIMAGE);
                        }

                        Settings.SetHotkeyCode(Config.HOTKEY);
                        Settings.WriteSysFile(Settings.emailFile, Config.EMAIL);
                        Settings.WriteSysFile(Settings.passFile, Config.PASS);

                        add("Saved!");
                        NEXT++;
                        STEPS = 0;
                        this.SetConfig(NEXT, STEPS);

                    }
                    break;

                case COMM.ADD_REGISTRY:
                    {
                        if (INPUT_DATA) INPUT_DATA = false;

                        add("Adding registry startup... ", false);
                        if (Reg.StartupRegistryExists == false)
                        {
                            if (!Reg.AddStartupRegistry) add("Failed!");
                            else add("Done!");
                        }
                        else
                        {
                            add(string.Empty);
                            add("Startup registry exists. Validating... ", false);
                            if (!Reg.ValidStartupValue)
                            {
                                add(string.Empty);
                                add("Invalid startup registry value.");
                                add("Changing startup registry value... ", false);
                                if (!Reg.DeleteStartupRegistry) add("Failed!");
                                else
                                {
                                    if (Reg.AddStartupRegistry) add("Done!");
                                    else add("Failed!");
                                }    
                            }
                            else add("Done!");
                        }

                        if (ENTER_INSTALL)
                        {
                            NEXT++;
                            STEPS = 0;
                            this.SetConfig(NEXT, STEPS);
                        }    
                    }
                    break;

                case COMM.SERVICE_INSTALL:
                    {
                        add("Installing service... ", false);
                        ServiceController sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == c.serviceName);
                        if (sc != null)
                        {
                            add(string.Empty);
                            add("Service is already installed.");
                            add("Checking ImagePath value...", false);

                            if (Reg.IsValidServicePath(c.serviceName, out string path)) add("Done!");
                            else
                            {
                                add(string.Empty);
                                add("Invalid service ImagePath.");
                                add("Uninstalling...", false);

                                int exitCode;
                                using (Process process = new Process())
                                {
                                    ProcessStartInfo startInfo = process.StartInfo;
                                    startInfo.FileName = "sc";
                                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                                    // tell Windows that the service should restart if it fails
                                    startInfo.Arguments = "delete " + c.serviceName;

                                    process.Start();
                                    process.WaitForExit();

                                    exitCode = process.ExitCode;
                                }

                                //If something went wrong starting the service show a dialog dialog box to the user and show some error code.
                                if (exitCode != 0)
                                {
                                    add("Failed!");
                                    add("Something went wrong with the service. Window Error code : " + exitCode);
                                }
                                else
                                {
                                    InstallServiceFinalizer(sc);
                                }
                            }
                        }
                        else
                        {
                            InstallServiceFinalizer(sc);
                        }
                    }
                    break;
            }
        }
        private void SavedataFinalizer(bool access, string path)
        {
            if (access == false)
            {
                add(path);
                add("Click ENTER BUTTON to try again OR press ESCAPE to cancel.");
                add(string.Empty);
                SKIP_PATH = true;
                Task.Run(async () =>
                {
                    await Task.Delay(50);
                    btn_enter.Enabled = true;
                });
            }
            else
            {
                btn_enter.Enabled = false;
                add("Save data folder " + path);
                add(string.Empty);

                if (ENTER_INSTALL)
                {
                    Config.SAVEDATA = 1;
                    Config.SAVEDATA_PATH = path; // tempConfig.Add(path); //Store save data folder path in arraylist[17] index.

                    NEXT++;
                    STEPS = 0;
                    this.SetConfig(NEXT, STEPS);
                }
                else
                {
                    Settings.SetSavedata(1);
                    Settings.WriteSysFile(Settings.savedataPath, path);
                    INPUT_DATA = false;
                    NEXT = 0; STEPS = 0;
                }
            }
        }
        private void LockimageFinalizer(bool access, string file)
        {
            if (access == false)
            {
                add(file);
                add("Click ENTER BUTTON to try again OR press ESCAPE to cancel.");
                add(string.Empty);
                SKIP_PATH = true;
                Task.Run(async () =>
                {
                    await Task.Delay(50);
                    btn_enter.Enabled = true;
                });
            }
            else
            {
                btn_enter.Enabled = true;
                add("Lock image file " + file);
                add(string.Empty);

                if (ENTER_INSTALL)
                {
                    Config.LOCKIMAGE = file; // tempConfig.Add(file); //Store lock image path in arraylist[18] index.
                    NEXT++; STEPS = 0;
                    this.SetConfig(NEXT, STEPS);
                }
                else
                {
                    Settings.SetLockImage(file);
                    INPUT_DATA = false;
                    NEXT = 0; STEPS = 0;
                }
            }
        }
        private void InstallServiceFinalizer(ServiceController sc = null)
        {
            ProcessStartInfo svc = new ProcessStartInfo();
            svc.FileName = "coinappSvc.exe";
            svc.Arguments = "/i -s"; //Install parameter.
            svc.UseShellExecute = false;
            svc.WindowStyle = ProcessWindowStyle.Normal;
            Process proc = Process.Start(svc);
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                add(string.Empty);
                add("Something happen installing the service.");
                add("Windows system error code: " + proc.ExitCode);
            }
            else
            {
                sc = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == c.serviceName);
                if (sc != null)
                {
                    add("Done!");
                    add(string.Empty);
                    add("Installation successful. Re-open this application to start.");

                }
                else
                {
                    add("Something went wrong.");
                    add("Cannot install service. Contact the software developer.");
                }

                if (ENTER_INSTALL)
                {
                    NEXT = 0;
                    STEPS = 0;
                    ENTER_INSTALL = false;
                    INSTALL_TYPE = 0;
                    c.STARTUP_INSTALL = false;

                }
            }
        }
        private void TestSerialDevice(string ComPort, int BaudRate)
        {
            Task.Run(async () =>
            {
                WORKING = true;
                txtbx_command.ReadOnly = true;

                serialPort = new SerialPort();
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.PortName = ComPort;
                serialPort.BaudRate = BaudRate;
                serialPort.DtrEnable = true;

                try { serialPort.Open(); }
                catch (Exception ex)
                {
                    ENTER_INSTALL = false;
                    NEXT = 0;
                    STEPS = 0;
                    INSTALL_TYPE = 0;
                    tempInput.Clear();
                    //tempInput = null;

                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Dispose();
                    serialPort = null;

                    txtbx_command.ReadOnly = false;
                    add("Something went wrong testing the device."); add("Error message: " + ex.Message);
                    add(string.Empty); add("The installation is now cancelled.");
                    WORKING = false;
                    return;
                }

                await Task.Delay(3000);
                serialPort.Write("COINAPP;");
                await Task.Delay(1000);

                if (DEVICE_READY == false)
                {
                    add("Testing done! Disconnecting device first...");

                    try { serialPort.Close(); }
                    catch (Exception ex)
                    {
                        add(string.Empty); add("Disconnecting failed. There is a problem disconnecting the device.");
                        add("Error message: " + ex.Message); add(string.Empty);
                    }

                    if (serialPort.IsOpen == false)
                    {
                        add("Disconnected!");
                        add(string.Empty);
                    }

                    ENTER_INSTALL = false;
                    NEXT = 0;
                    STEPS = 0;
                    INSTALL_TYPE = 0;
                    tempInput.Clear();
                    tempInput = null;

                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Dispose();
                    serialPort = null;

                    add("The selected device is tested, but, this is not for Coinapp application.");
                    add("Be sure that the selected device is installed with appropriate Coinapp firmware.");
                    add(string.Empty);
                    add("The installation is now cancelled.");
                    txtbx_command.ReadOnly = false;
                }
                else
                {
                    add("Congratulations! Device tested successfully.");
                    add("Getting device information...");
                    serialPort.Write("VMJ?"); await Task.Delay(100);
                    serialPort.Write("VMN?"); await Task.Delay(100);
                    serialPort.Write("VB?"); await Task.Delay(100);
                    serialPort.Write("VR?"); await Task.Delay(100);

                    if (DEVICE_VERSION == string.Empty) add("Failed getting device information.");
                    else add("Device version: " + DEVICE_VERSION);

                    add(string.Empty); add("Testing done! Disconnecting device first...");

                    try { serialPort.Close(); }
                    catch (Exception ex)
                    {
                        add(string.Empty); add("Disconnecting failed. There is a problem disconnecting the device.");
                        add("Error message: " + ex.Message); add("You can continue the installation."); add(string.Empty);
                    }

                    if (serialPort.IsOpen == false)
                    {
                        add("Disconnected! Continue the installation.");
                        add(string.Empty);
                    }

                    tempInput.Clear();
                    serialPort.DataReceived -= SerialPort_DataReceived;
                    serialPort.Dispose();
                    serialPort = null;
                    txtbx_command.ReadOnly = false;

                    if (ENTER_INSTALL)
                    {
                        if (INSTALL_TYPE == 0) NEXT = COMM.HOTKEYCODE; // 12; //skip to Hotkey. Default configurations.;
                        else NEXT = COMM.COINSLOT; // 3; //Basic & advance configurations.
                        STEPS = 0;

                        this.SetConfig(NEXT, STEPS);
                    }
                }
                WORKING = false;
            });
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null)
                {
                    string serialData = serialPort.ReadTo(serialPort.NewLine);

                    if (DEVICE_READY)
                    {
                        //if (spReceived == "VT")
                        //    DEVICE_VERSION += sp.ReadTo(sp.NewLine);
                        if (serialData == "VMJ")
                            DEVICE_VERSION += serialPort.ReadTo(serialPort.NewLine) + ".";
                        if (serialData == "VMN")
                            DEVICE_VERSION += serialPort.ReadTo(serialPort.NewLine) + ".";
                        if (serialData == "VB")
                            DEVICE_VERSION += serialPort.ReadTo(serialPort.NewLine) + ".";
                        if (serialData == "VR")
                            DEVICE_VERSION += serialPort.ReadTo(serialPort.NewLine);
                    }
                    else if (serialData == "READY")
                    {
                        DEVICE_READY = true;
                    }
                }
            }
            catch (Exception ex)
            {
                add(ex.Message);
            }
        }
    }
}