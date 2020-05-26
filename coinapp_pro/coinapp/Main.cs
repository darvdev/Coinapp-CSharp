using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using coinapplib;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace coinapp
{
    #region START
    public class Start
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            c.AppXmlParser();
            
            if (args.Length > 0) //Check arguments when the application is started.
            {
                switch (args[0])
                {
                    //case "set":
                    //    {
                    //        Application.Run(new GUI_settings());
                    //        break;
                    //    }
                    case ARGS.START_MSG:
                        {
                            string arg = string.Empty;
                            for (int i = 0; i < args.Length - 1; i++)
                                arg += args[i] + " ";
                            
                            //string sent = arg.Substring(5, arg.IndexOf("/"));
                            string message = arg.Substring(arg.IndexOf("&") + 1);
                            GUI_dialog dialog = new GUI_dialog(message, Images.error, button: false);
                            Application.Run(dialog); 
                            return;
                        }

                    case ARGS.START_BOOT:
                        {
                            Logs.System("Starting from windows startup...", c.LOGPATH, c.ADDLOG);

                            using (ServiceController sc = new ServiceController(c.serviceName))
                            {
                                if (sc.Status != ServiceControllerStatus.Running)
                                {
                                    Logs.System("WARNING! Service is not running. Starting the service...", c.LOGPATH, c.ADDLOG);

                                    if (sc.Status == ServiceControllerStatus.StartPending)
                                    {
                                        Logs.System("WARNING! Service is in the pending running state.", c.LOGPATH, c.ADDLOG);
                                        goto RunProcess;
                                    }

                                    try
                                    {
                                        sc.Start();
                                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(10000));
                                    }
                                    catch (Exception e)
                                    {
                                        Logs.System(e.Message, c.LOGPATH, c.ADDLOG);
                                        MessageBox.Show(e.Message, "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        Environment.Exit(0);
                                    }

                                    if (sc.Status != ServiceControllerStatus.Running)
                                    {
                                        //Shutdown here.
                                        Logs.System("ERROR! Cannot start the service.", c.LOGPATH, c.ADDLOG);
                                        MessageBox.Show("Error: Cannot start the service", "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        Environment.Exit(0);
                                    }
                                    else
                                    {
                                        Logs.System("SUCCESS! Service started.", c.LOGPATH, c.ADDLOG);
                                    }
                                }
                            }

                            RunProcess:

                            Logs.System("Started from windows startup.", c.LOGPATH, c.ADDLOG);
                            c.S = new SESSION(c.XmlPath);
                            c.app = new Main();
                            Application.Run(c.app);

                            return;
                        }
                }
            }
            else //Application is started with no arguments option or user directly open.
            {
                //First, check all the configuration of the application if exists or settled before starting the application.
                //If not, open the application installer (gui_console).

                //Check service if installed.
                ServiceController[] scs = ServiceController.GetServices(); //Get all the service from Service Controller Manager.
                if ((scs.Any(x => x.ServiceName == c.serviceName)) == false) //Check if the service is installed.
                {
                    //Service is not install.
                    c.ecode.Add(ECODE.SERVICE_NOT_INSTALLED);

                    if (Settings.ConfigurationExists == false)
                        c.ecode.Add(ECODE.CONFIGURATION_FILE_NOT_FOUND);

                    if (My.SystemConfigurationExists == false)
                    {
                        c.ecode.Add(ECODE.SYSTEM_CONFIGURATION_FILE_NOT_FOUND);
                        c.STARTUP_INSTALL = true;
                    }

                    //c.STARTUP_INSTALL = true;
                    Application.Run(new GUI_console());
                }
                else //Service is installed.
                {
                    using (ServiceController sc = new ServiceController(c.serviceName))
                    {
                        if (sc.Status == ServiceControllerStatus.Running) //Check service if running.
                        {
                            //Service is running.
                            ///////////////////////////////////////////////////////////////////////////////////////////////////
                            //Check the application if running and healhty. And if not Lock the desktop and throw error code.//
                            ///////////////////////////////////////////////////////////////////////////////////////////////////
                            ///
                            return;
                        }
                        else //Service is not running.
                        {
                            if (!Reg.IsValidServicePath(c.serviceName, out _)) //Check if the service install ImagePath from registry is valid.
                                c.ecode.Add(ECODE.SERVICE_INVALID_PATH);

                            //Check if email and password file exists in system folder.
                            if (My.SystemConfigurationExists == false)
                                c.ecode.Add(ECODE.SYSTEM_CONFIGURATION_FILE_NOT_FOUND);

                            if (Settings.ConfigurationExists == false) //Check if coinapp.ini is exists.
                                c.ecode.Add(ECODE.CONFIGURATION_FILE_NOT_FOUND);

                            if (c.ecode.Count != 0) //Check if there is an error codes.
                            {
                                //Error codes occured. Run the console window.
                                Application.Run(new GUI_console());
                            }
                            else //No error found.
                            {

                                if (Reg.StartupRegistryExists)
                                {
                                    if (!Reg.ValidStartupValue)
                                        if (Reg.DeleteStartupRegistry)
                                            if (Reg.AddStartupRegistry)
                                                Logs.System("Success: Startup registry value changed to valid value.", c.LOGPATH, c.ADDLOG);
                                }
                                else
                                {
                                    if (!Reg.AddStartupRegistry)
                                        Logs.System("Error: Adding startup registry failed.", c.LOGPATH, c.ADDLOG);
                                }

                                //Ask user if he/she want to run the application by starting the service using the "sc".
                                DialogResult dialog = MessageBox.Show("Do you want to start the application and lock the desktop?", Application.ProductName + " " + Application.ProductVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                                if (dialog == DialogResult.No) return;

                                if (sc.Status == ServiceControllerStatus.StartPending)
                                {
                                    Logs.System("WARNING! Service is pending running.", c.LOGPATH, c.ADDLOG);
                                    goto RunProcess;
                                }

                                try
                                {
                                    sc.Start();
                                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(10000));
                                }
                                catch (Exception e)
                                {
                                    Logs.System(e.Message, c.LOGPATH, c.ADDLOG);
                                    MessageBox.Show(e.Message, "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Environment.Exit(0);
                                }

                                if (sc.Status != ServiceControllerStatus.Running)
                                {
                                    //Shutdown here.
                                    Logs.System("ERROR! Cannot start the service.", c.LOGPATH, c.ADDLOG);
                                    MessageBox.Show("Error: Cannot start the service", "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    Logs.System("SUCCESS! Service started manually.", c.LOGPATH, c.ADDLOG);
                                }

                            RunProcess:
                                Logs.System("Started from windows startup.", c.LOGPATH, c.ADDLOG);

                                c.S = new SESSION(c.XmlPath);
                                c.app = new Main();
                                Application.Run(c.app);

                                #region USING SC process
                                //int exitCode;
                                //using (var process = new Process())
                                //{
                                //    var startInfo = process.StartInfo;
                                //    startInfo.FileName = "sc";
                                //    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                                //    // tell Windows that the service should restart if it fails
                                //    startInfo.Arguments = "start " + c.serviceName;

                                //    process.Start();
                                //    process.WaitForExit();

                                //    exitCode = process.ExitCode;
                                //}

                                ////If something went wrong starting the service show a dialog dialog box to the user and show some error code.
                                //if (exitCode != 0)
                                //    MessageBox.Show("Failed to start the application.\nWindows system error code: " + exitCode, Application.ProductName + " v" + Application.ProductVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                #endregion

                                #region OPEN CONSOLE
                                //else if (dialog == DialogResult.No)
                                //{
                                //    CONFIG conf = new CONFIG();
                                //    Config.PORTNUMBER = conf.PORTNUMBER;
                                //    Config.BAUDRATE = conf.BAUDRATE;
                                //    Config.COINSLOT = conf.COINSLOT;
                                //    Config.COIN1 = conf.COIN1;
                                //    Config.COIN2 = conf.COIN2;
                                //    Config.COIN3 = conf.COIN3;
                                //    Config.PULSE1 = conf.PULSE1;
                                //    Config.PULSE2 = conf.PULSE2;
                                //    Config.PULSE3 = conf.PULSE3;
                                //    Config.INTERVAL = conf.INTERVAL;

                                //    Config.EMAIL = My.Email;
                                //    Config.PASS = My.Password;
                                //    Config.SHUTDOWN = conf.SHUTDOWN;
                                //    Config.LOGINATTEMPT = conf.LOGINATTEMPT;
                                //    Config.HOTKEY = conf.HOTKEYCODE;
                                //    Config.TIMER = conf.TIMER;
                                //    Config.SOUND = conf.SOUND;
                                //    Config.SAVEDATA = conf.SAVEDATA;
                                //    if (conf.SAVEDATA > 0 && My.SavedataPathExists(out string path)) Config.SAVEDATA_PATH = path;
                                //    Config.LOCKIMAGE = conf.LOCKIMAGE;
                                //    Config.LOG = conf.LOG;

                                //    Application.Run(new GUI_console());
                                //}
                                #endregion OPEN CONSOLE
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    public class Main : Form
    {
        private const string mainPipeName = "coinapp";
        private const string coinappDeskName = "Coinapp";

        private Desktop coinappDesk;
        private Desktop defaultDesk;

        private int ICON = 0;
        private int SHUTDOWN = 0;

        private bool ANIMATE = false;
        private bool TIMER = false;
        private HotkeyCode hotkey = new HotkeyCode();

        private GUI_lock gui_lock;
        private GUI_login gui_login;
        private GUI_console gui_console;
        public GUI_dialog dialog;
        private GUI_timer gui_timer;

        private Thread thread_lock;
        private Thread thread_login;
        private Thread thread_console;

        private System.Timers.Timer shutdownTimer;
        private Comctl32.SubclassWndProc subclassWndProc;
        
        private enum Display { UNLOCK, HIDE, COUNT, TIMEOUT, UPDATE, VISIBLE, }
        private struct Tray
        {
            public static NotifyIcon Notify;
            public static ContextMenuStrip Context;
            public static ToolStripMenuItem Console;
            public static ToolStripMenuItem Logout;
            public static ToolStripMenuItem Visible;
            public static ToolStripMenuItem Lock;
            public static ToolStripMenuItem Update;
        }
        public Main()
        {
            SetDesktops();
            gui_lock = new GUI_lock(coinappDesk.DesktopHandle);
            thread_lock = new Thread(LockThread);
            thread_lock.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread_lock.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            thread_lock.Start(gui_lock);
            Task.Run(() =>
            {
                thread_lock.Join();
                c.ecode.Add(ECODE.LOCK_SCREEN_TERMINATED);
            });

            c.mainPipe = new ClientPipe(".", mainPipeName);
            c.mainPipe.DataReceived += MainPipe_DataReceived;
            c.mainPipe.PipeClosed += MainPipe_Closed;
            Logs.System("Waiting for the service connection...", c.LOGPATH, c.ADDLOG);
            Task.Run(() =>
            {
                try { c.mainPipe.Connect(); }
                catch (Exception e) { Logs.System(e.Message, c.LOGPATH, c.ADDLOG); }
            });

            hotkey.KeyDown += Hotkey_KeyDown;
            //Task.Run(async () =>
            //{
            //    while (string.Equals(Desktop.OpenInputDesktop().DesktopName, defaultDeskName, StringComparison.OrdinalIgnoreCase) == false)
            //        await Task.Delay(10);

            //    //while (!Window.DesktopLoaded)
            //    //    await Task.Delay(10);

            //    c.DESKTOP_READY = true;
            //});

        }
        
        #region System Tray Icon handles
        private IntPtr TrayWndProc(IntPtr hWnd, uint uMsg, UIntPtr wParam, UIntPtr lParam, UIntPtr uIdSubclass, UIntPtr dwRefData)
        {
            switch (uMsg)
            {
                // Ignore the close message to avoid Alt+F4 killing the tray icon
                case Comctl32.WM_CLOSE:
                    return IntPtr.Zero;

                // Clean up subclassing
                case Comctl32.WM_NCDESTROY:
                    Comctl32.RemoveWindowSubclass(hWnd, this.subclassWndProc, UIntPtr.Zero);
                break;
            }

            // Invoke the default window proc
            return Comctl32.DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
        private void InitializeSystemTrayIcon()
        {
            Tray.Notify = new NotifyIcon();
            Tray.Notify.Icon = Images.coinapp;
            Tray.Context = new ContextMenuStrip();
            Tray.Notify.ContextMenuStrip = Tray.Context;
            //Tray.Notify.Visible = true;

            Tray.Console = new ToolStripMenuItem("Console");
            Tray.Lock = new ToolStripMenuItem("Lock");
            Tray.Visible = new ToolStripMenuItem("Hide");
            Tray.Logout = new ToolStripMenuItem("Logout");
            Tray.Update = new ToolStripMenuItem("Updates");

            // Get the HWND from the notify icon
            Type notifyIconType = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            var window = notifyIconType.GetField("window", hidden).GetValue(Tray.Notify) as NativeWindow;

            // Inject our window proc to intercept window messages
            this.subclassWndProc = this.TrayWndProc;
            Comctl32.SetWindowSubclass(window.Handle, this.subclassWndProc, UIntPtr.Zero, UIntPtr.Zero);

            Tray.Context.Items.Add(Tray.Update);
            Tray.Context.Items.Add(Tray.Console);
            Tray.Context.Items.Add(Tray.Lock);
            Tray.Context.Items.Add(Tray.Visible);
            Tray.Context.Items.Add(Tray.Logout);

            //Tray.Notify
            Tray.Notify.DoubleClick += Visible_and_Double_Click;
            Tray.Notify.MouseMove += Notify_MouseMove;
            Tray.Console.Click += Console_Click;
            Tray.Lock.Click += Lock_Click;
            Tray.Visible.Click += Visible_and_Double_Click;
            Tray.Logout.Click += Logout_Click;
            Tray.Update.Click += Update_Click;
        }
        private void Logout_Click(object sender, EventArgs e)
        {
            Tray.Logout.Enabled = false;

            if (c.FormExists("GUI_dialog"))
                dialog.Close();

            dialog = new GUI_dialog();

            DialogResult result = dialog.ShowDialog("Cancel all remaining time and lock the desktop?", "Logout and lock", MessageBoxButtons.OKCancel, Images.error);
            dialog.Close();
            
            if (result == DialogResult.OK)
            {
                c.SESSION = SESSION.USER_LOGOUT;
                Timer(Display.HIDE);
                Task.Run(async () =>
                {
                    await Task.Delay(300);
                    LockDesktop();
                });
            }

            Tray.Logout.Enabled = true;
        }
        private void Visible_and_Double_Click(object sender, EventArgs e) { Timer(Display.VISIBLE); }
        public void UpdateTimerVisible() { Timer(Display.UPDATE); }
        private void Console_Click(object sender, EventArgs e)
        {
            if (!c.FormExists("GUI_console"))
            {
                c.app.ShowConsoleWindow();
            }
            else
            {
                Window.Show(gui_console.Handle);
                Window.SetForeground(gui_console.Handle);
            }
        }
        private void Lock_Click(object sender, EventArgs e)
        {
            if (c.REMAINING_TIME > 60) c.SESSION = SESSION.TIME_RESUME;
            else c.SESSION = SESSION.ADMIN_LOGOUT;
            LockDesktop();
        }
        private void Update_Click(object sender, EventArgs e)
        {
            if (c.FormExists("GUI_dialog"))
                dialog.Close();
            
            if (c.UPDATES > 0)
            {
                string dialogMessage = "Do you want to close this Coinapp application and open Coinapp Updater to update the " + (c.UPDATES == 1 ? "software?" : (c.UPDATES == 2 ? "device firmware?" : "software and device firmware?"));
                dialog = new GUI_dialog();
                DialogResult result = dialog.ShowDialog(dialogMessage, "Coinapp updates", MessageBoxButtons.YesNo, Images.initialize);
                dialog.Close();
                if (result == DialogResult.No) return;
                if (c.CONNECTED)
                {
                    string file = Application.StartupPath + @"\coinup.exe";
                    if (File.Exists(file))
                    {
                        string args = c.UPDATES == 1 ? " /app -s" : c.UPDATES == 2 ? " /dev" : "";
                        defaultDesk.CreateProcess(file + args);
                        c.mainPipe.WriteString(ARGS.STOP);
                    }
                }
            }
        }
        private void Notify_MouseMove(object sender, MouseEventArgs e) 
        {
            string notify = Application.ProductName + " " + Application.ProductVersion + "\n";
            if (c.CURRENT_DEVICE_VERSION != string.Empty) notify += "Device " + c.CURRENT_DEVICE_VERSION + "\n";
            if (c.NEW_SOFTWARE_VERSION != string.Empty || c.NEW_DEVICE_VERSION != string.Empty) notify += "(Updates are available!)\n";

            if (c.ecode.Count == 0)
            {
                notify += "System initializing...";
            }
            else
            {
                notify += "Session: " + c.S.ToString(c.SESSION) + "\n";
                
                if (c.ecode.Count > 1)
                {
                    notify += "Ecodes : ";

                    for (int i = 0; i < c.ecode.Count; i++)
                    {
                        if (!c.ADMINISTRATOR)
                        {
                            //Remove ECODE.SYSTEM_INITIALIZE in time in session.
                            notify += c.ecode[i] != ECODE.DEVICE_READY ? c.ecode[i] + ", " : "";
                        }
                        else
                        {
                            notify += c.ecode[i] + ", ";
                        }
                    }
                }
                else
                {
                    //Remove ECODE.SYSTEM_INITIALIZE when not in administrator session.
                    notify += c.ADMINISTRATOR ? "Ecode: " + ECODE.ToString(c.ecode[0]) : (c.ecode[0] != ECODE.DEVICE_READY ? "ECODE: " + ECODE.ToString(c.ecode[0]) : "" ) ;                  
                } 
            }

            TextNotify(notify);
        }
        #endregion

        #region OVERRIDES
        private static int WM_QUERYENDSESSION = 0x11;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_QUERYENDSESSION)
            {
                c.STOP = true;
                if (c.CONNECTED) { c.mainPipe.WriteString(ARGS.STOP); }
                else { Shutdown.Force(); Application.Exit(); }
            }
            base.WndProc(ref m);

        } //WndProc   
        protected override void DefWndProc(ref Message m)
        {
            if (hotkey != null) hotkey.ProcessWinMessageHotkey(m.Msg, m.WParam);
            base.DefWndProc(ref m);
        }
        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
                value = false;
            }
            base.SetVisibleCore(false);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            return;
        }
        #endregion

        #region GUIs THREADS
        private void LockThread(object obj)
        {
            GUI_lock form = (GUI_lock)obj;
            Desktop.SetThreadDesktop(form.iHandle);
            Application.Run(form);
            //form.ShowDialog();
        }
        private void LoginThread(object obj)
        {
            GUI_login form = (GUI_login)obj;
            Desktop.SetThreadDesktop(form.iHandle);
            Application.Run(form);
            //form.ShowDialog();
        }
        private void ConsoleThread(object obj)
        {
            GUI_console form = (GUI_console)obj;
            Desktop.SetThreadDesktop(form.iHandle);
            Application.Run(form);
            //form.ShowDialog();
        }
        #endregion

        #region COMMUNICATIONS
        private void MainPipe_Closed(object sender, EventArgs e)
        {
            c.CONNECTED = false;
            if (c.STOP) return;
            
            using (ServiceController sc = new ServiceController(c.serviceName))
            {
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    Logs.System("WARNING! Service process terminated unexpectedly.", c.LOGPATH, c.ADDLOG);
                    c.ecode.Add(ECODE.SERVICE_PROCESS_TERMINATED);
                }
                else
                {
                    Logs.System("WARNING! Communication to the service closed!", c.LOGPATH, c.ADDLOG);
                    c.ecode.Add(ECODE.SERVICE_DISCONNECTED);
                }
            }


            if (!c.ADMINISTRATOR)
            {
                shutdownTimer = new System.Timers.Timer(1000);
                shutdownTimer.Elapsed += ShutdownTimer_Elapsed;
                SHUTDOWN = 30;
                shutdownTimer.Start();
                Logs.System("WARNING! Shutdown initiated within " + SHUTDOWN, c.LOGPATH, c.ADDLOG);
                LockDesktop();
            }
            else
            {
                if (c.ecode.Contains(ECODE.DEVICE_READY))
                {
                    int index = 0;
                    for (int i = 0; i < c.ecode.Count(); i++)
                        if (c.ecode[i] == ECODE.DEVICE_READY) index = i;
                    c.ecode.RemoveAt(index);
                }

                this.BeginInvoke(() =>
                {
                    Tray.Lock.Enabled = false;
                    Tray.Notify.Icon = Images.admin_x;
                    if (c.ecode.Count() > 0 ) Tray.Notify.BalloonTipText = ECODE.ToString(c.ecode[0]);
                    Tray.Notify.ShowBalloonTip(3000);
                });
            }

        }
        private void MainPipe_DataReceived(object sender, PipeEventArgs e)
        {
            try
            {
                string mainData = e.String;
                //Logs.System("Received from service : " + mainData, c.LOGPATH, c.ADDLOG);
                switch (mainData)
                {
                    case ARGS.LOCK: { LockDesktop(); break; }
                    case ARGS.UNLOCK: { UnlockDesktop(); break; }
                    case ARGS.INITIALIZE: { Task.Run(() => InitializedEcodes()); break; }
                    case ARGS.TIMEIN: { InitializeTimeIn(); break; }
                    case ARGS.CONNECTED:
                        {
                            c.mainPipe.WriteString("PID=" + Process.GetCurrentProcess().Id);
                            this.BeginInvoke(() => hotkey.Register(this.Handle, Config.HOTKEY));
                            InitializeSystemTrayIcon();
                            gui_lock.label_timer.Text = TimeToString(SHUTDOWN.ToString());
                            Config.SHUTDOWN = SHUTDOWN;
                            c.CONNECTED = true;
                            break;
                        }

                    case ARGS.REINIT:
                        {
                            //Get index number for DEVICE_NOT_COMPATIBLE or DEVICE_NOT_CONNECTED  or DEVICE_INFO_ERROR and remove it.

                            if (c.ecode.Count != 0)
                            {
                                int index = 0;
                                for (int ec = 0; ec < c.ecode.Count; ec++)
                                {
                                    if (c.ecode[ec] == ECODE.DEVICE_NOT_COMPATIBLE || c.ecode[ec] == ECODE.DEVICE_NOT_DETECTED || c.ecode[ec] == ECODE.DEVICE_INFO_ERROR) index = ec;
                                }
                                c.ecode.RemoveAt(index);
                            }


                            break;
                        }

                    case ARGS.TIMEOUT:
                        {
                            c.SESSION = SESSION.TIME_OUT;
                            Timer(Display.TIMEOUT);
                            LockDesktop();
                            break;
                        }

                    case ARGS.STOPPED:
                        {
                            c.STOP = true;
                            this.BeginInvoke(() => Tray.Notify.Visible = false);
                            c.mainPipe.Flush();
                            c.mainPipe.Close();
                            Environment.Exit(0);
                            break;
                        }

                    case ARGS.DEVICE_DC:
                        {
                            c.ecode.Add(ECODE.DEVICE_DISCONNECT);

                            if (c.LOCKED)
                            {
                                gui_lock.button_status.Image = Images.error;
                                gui_lock.label_messages.Text =ECODE.ToString(ECODE.DEVICE_DISCONNECT);
                                if (c.CONNECTED) { c.mainPipe.WriteString(ARGS.DEVICE_DC);}
                            }
                            else
                            {
                                if (c.ADMINISTRATOR)
                                {
                                    int index = -1;
                                    for (int i = 0; i < c.ecode.Count(); i++)
                                    {
                                        if (c.ecode[i] == ECODE.DEVICE_READY)
                                        {
                                            index = i;
                                            break;
                                        }
                                    }
                                    c.ecode.RemoveAt(index);
                                    
                                    this.BeginInvoke(() =>
                                    {
                                        Tray.Notify.Icon = Images.admin_x;
                                        Tray.Lock.Enabled = false;
                                        Tray.Notify.BalloonTipText = ECODE.ToString(ECODE.DEVICE_DISCONNECT);
                                        Tray.Notify.ShowBalloonTip(3000);
                                    });
                                }
                                else
                                {
                                    LockDesktop();
                                    if (c.CONNECTED)
                                        c.mainPipe.WriteString(ARGS.DEVICE_DC);
                                    else
                                        Shutdown.Restart();
                                }
                            }
                            break;
                        }

                    #region TCP COMMANDS
                    case ARGS.TCPLOCK:
                        {
                            c.SESSION = SESSION.SERVER_LOCK;
                            LockDesktop();
                            break;
                        }
                    case ARGS.TCPUNLOCK:
                        {
                            c.SESSION = SESSION.SERVER_UNLOCK;
                            UnlockDesktop();
                            break;
                        }
                    case ARGS.TCPLOGOUT:
                        {
                            c.SESSION = SESSION.SERVER_LOGOUT;
                            Timer(Display.TIMEOUT);
                            LockDesktop();
                            break;
                        }
                    case ARGS.TCPMUTE:
                        {
                            Sounds.Mute(Handle);
                            break;
                        }
                    case ARGS.TCPUNMUTE:
                        {
                            Sounds.Unmute(Handle);
                            break;
                        }
                    #endregion TCP COMMANDS

                    default:
                        {
                            Task.Run(() => {

                                string sent = mainData.Substring(0, mainData.IndexOf("="));
                                string dataString = mainData.Substring(mainData.IndexOf("=") + 1);
                                int dataInt = int.TryParse(dataString, out dataInt) ? dataInt : 0;

                                switch (sent)
                                {
                                    case "HK": //Hotkeycode
                                        {
                                            Config.HOTKEY = dataInt;
                                            break;
                                        }

                                    case "SD"://Shutdown
                                        {
                                            SHUTDOWN = dataInt;

                                            if (c.CONNECTED)
                                            {
                                                this.BeginInvoke(() =>
                                                {
                                                    if (dataInt > 0)
                                                    {
                                                        if (dataInt <= 10) gui_lock.BackColor = Color.Red;
                                                        else gui_lock.BackColor = Color.Black;
                                                        gui_lock.label_timer.Text = TimeToString(dataString);
                                                    }
                                                    else
                                                    {
                                                        c.SESSION = SESSION.SHUTTING_DOWN;
                                                        gui_lock.label_messages.Text = c.S.ToString(c.SESSION);
                                                        gui_lock.button_status.Image = null;
                                                        gui_lock.label_timer.Text = "00";
                                                    }
                                                });
                                            }
                                            break;
                                        }

                                    case "CD": //Credits
                                        {
                                            Timer(Display.COUNT, dataInt); /////////////////////////////////////////////////////////
                                            break;
                                        }

                                    case "AN": //Animation ////////////////////////////////Problem somethimes
                                        {
                                            if (!ANIMATE && dataInt > 0)
                                            {
                                                ANIMATE = true;
                                                Task.Run(() => TrayAnimation());
                                            }
                                            break;
                                        }

                                    case "PA": //Password
                                        {
                                            Config.PASS = dataString;
                                            break;
                                        }

                                    case "EM": //Email
                                        {
                                            Config.EMAIL = dataString;
                                            break;
                                        }

                                    case "LA": //Login attempt
                                        {
                                            Config.LOGINATTEMPT = dataInt > 3 ? dataInt : 3;
                                            break;
                                        }

                                    case "TM": //Timer
                                        {
                                            TIMER = dataInt > 0 ? true : false;
                                            Config.TIMER = dataInt;
                                            break;
                                        }

                                    case "PS": //Play sound;
                                        {
                                            switch (dataString)
                                            {
                                                case "COIN":
                                                    Sounds.Play(Sounds.default_coin_drop);
                                                    break;
                                                case "WARN":
                                                    Sounds.Play(Sounds.default_warning);
                                                    break;
                                                case "OUT":
                                                    Sounds.Play(Sounds.default_time_out);
                                                    break;
                                                case "SMC":
                                                    Sounds.Play(Sounds.super_mario_coin_drop);
                                                    break;
                                                case "SMW":
                                                    Sounds.Play(Sounds.super_mario_warning);
                                                    break;
                                                case "SMO":
                                                    Sounds.Play(Sounds.super_mario_time_out);
                                                    break;
                                                case "SMP":
                                                    Sounds.Play(Sounds.super_mario_time_pause);
                                                    break;
                                            }
                                            break;
                                        }

                                    case "RT": //Remaining time
                                        {
                                            c.REMAINING_TIME = dataInt;
                                            break;
                                        }

                                    case "RC": //Reconnect device
                                        {
                                            gui_lock.label_messages.Text = dataString;
                                            break;
                                        }
                                    case "LI": //Lock image
                                        {
                                            Task.Run(() =>
                                            {
                                                try { c.LOCKIMAGE = new Bitmap(dataString); }
                                                catch (Exception ex) { Logs.System(ex.Message, c.LOGPATH, c.ADDLOG); return; }
                                                Config.LOCKIMAGE = dataString;
                                            });

                                            break;
                                        }
                                    case "NV": //New software version
                                        {
                                            if (c.NEW_SOFTWARE_VERSION == string.Empty || c.NEW_SOFTWARE_VERSION != dataString) c.NEW_SOFTWARE_VERSION = dataString;
                                            if (c.UPDATES == 0) c.UPDATES = 1;
                                            else if (c.UPDATES == 2) c.UPDATES = 3;
                                            if (c.ADMINISTRATOR && !Tray.Update.Visible) this.BeginInvoke(() => Tray.Update.Visible = true);
                                            break;
                                        }
                                    case "DV": //New device version
                                        {
                                            if (c.NEW_DEVICE_VERSION == string.Empty || c.NEW_DEVICE_VERSION != dataString) c.NEW_DEVICE_VERSION = dataString;
                                            if (c.UPDATES == 0) c.UPDATES = 2;
                                            else if (c.UPDATES == 1) c.UPDATES = 3;
                                            if (c.ADMINISTRATOR && !Tray.Update.Visible) this.BeginInvoke(() => Tray.Update.Visible = true);
                                            break;
                                        }
                                    case "PN": { Config.PORTNUMBER = dataInt; break; } //Portnumber
                                    case "BR": { Config.BAUDRATE = dataInt; break; } //Baudrate
                                    case "CS": { Config.COINSLOT = dataInt; break; } //Coinslot
                                    case "C1": { Config.COIN1 = dataInt; break; } //Coin1
                                    case "C2": { Config.COIN2 = dataInt; break; } //Coin2
                                    case "C3": { Config.COIN3 = dataInt; break; } //Coin3
                                    case "P1": { Config.PULSE1 = dataInt; break; } //Pulse
                                    case "P2": { Config.PULSE2 = dataInt; break; } //Pulse2
                                    case "P3": { Config.PULSE3 = dataInt; break; } //Pulse3
                                    case "SN": { Config.SOUND = dataInt; break; } //Sound
                                    case "IV": { Config.INTERVAL = dataInt; break; } //Interval
                                    case "SV": { Config.SAVEDATA = dataInt; break; } //Savedata
                                    case "LG": { Config.LOG = dataInt; break; } //Log
                                    case "SP": { Config.SAVEDATA_PATH = dataString; break; } //Save data directory
                                    case "CV": { c.CURRENT_DEVICE_VERSION = dataString; break; } //Current device version
                                    case "EC": //error codes
                                        {
                                            c.ecode.Add(dataInt);
                                            break;
                                        }
                                }
                            });

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                c.CONNECTED = false;
                Logs.System("Main communication error : " + ex.Message, c.LOGPATH, c.ADDLOG);
            }
            
        }
        #endregion

        #region MY METHODS
        private void ShutdownTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!c.CONNECTED && !c.ADMINISTRATOR && !c.STOP)
            {
                if (SHUTDOWN <= 0)
                {
                    if (shutdownTimer.Enabled) shutdownTimer.Stop();
                    c.SESSION = SESSION.SHUTTING_DOWN;

                    gui_lock.button_status.Image = null;
                    gui_lock.label_timer.Text = "00";
                    gui_lock.label_messages.Text = c.S.ToString(c.SESSION);      
                    Logs.System("Warning: Initiate shutdown from main app.", c.LOGPATH, c.ADDLOG);
                    Shutdown.Force();
                }

                SHUTDOWN--;
                gui_lock.label_timer.Text = TimeToString(SHUTDOWN.ToString());

            }
        }
        private void Hotkey_KeyDown(object sender, EventArgs e)
        {
            if (!c.ADMINISTRATOR)
            {
                if (!c.FormExists("GUI_console"))
                {
                    if (!c.FormExists("GUI_login"))
                    {
                        if (c.LOCKED) gui_login = new GUI_login(coinappDesk.DesktopHandle);
                        else gui_login = new GUI_login(defaultDesk.DesktopHandle);

                        thread_login = new Thread(LoginThread);
                        thread_login.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                        thread_login.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                        thread_login.Start(gui_login);
                        Task.Run(() =>
                        {
                            thread_login.Join();
                            gui_login.Dispose();
                        });
                    }
                    else
                    {
                        Window.Top(gui_login.Handle);
                    }
                }
                else
                {
                    Window.Show(gui_console.Handle);
                    Window.SetForeground(gui_console.Handle);
                }
            }
        }       
        private void TextNotify(string text)
        {
            if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");

            var t = typeof(NotifyIcon);
            var hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(Tray.Notify, text);
            if (Convert.ToBoolean(t.GetField("added", hidden).GetValue(Tray.Notify)))
                t.GetMethod("UpdateIcon", hidden).Invoke(Tray.Notify, new object[] { true });
        }
        private void SetDesktops()
        {
            defaultDesk = Desktop.OpenDefaultDesktop();
            coinappDesk = new Desktop();
            if (Desktop.Exists(coinappDeskName))
            {
                coinappDesk = Desktop.OpenDesktop(coinappDeskName);
                if (coinappDesk.DesktopHandle == IntPtr.Zero)
                    coinappDesk.Create(coinappDeskName);
            }
            else
                coinappDesk.Create(coinappDeskName);
        }
        public void ShowConsoleWindow()
        {
            if (c.LOCKED) gui_console = new GUI_console(coinappDesk.DesktopHandle);
            else gui_console = new GUI_console(defaultDesk.DesktopHandle);

            thread_console = new Thread(ConsoleThread);
            thread_console.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            thread_console.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            thread_console.Start(gui_console);

            Task.Run(() =>
            {
                thread_console.Join();
                gui_console.Dispose();
            });
        }
        private string TimeToString(string data, bool timer = false)
        {
            int time = Convert.ToInt32(data);
            int seconds = time % 60;
            int minutes = (time - time % 60) / 60 % 60;
            int hours = (int)((time - (seconds + minutes * 60)) / (double)3600 % 60);

            if (!timer)
            {
                if (time >= 60)
                    return minutes.ToString("00") + ":" + seconds.ToString("00");
                else
                    return seconds.ToString("00");
            }
            else
            {
                return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            }

        }
        private void InitializedEcodes()
        {
            Task.Run( async() =>
            {
                //while (!c.DESKTOP_READY)
                //    await Task.Delay(10);

                if (c.ecode.Count == 0)
                {
                    gui_lock.label_messages.Text = "SYSTEM NOT INITIZALIZED";
                    gui_lock.button_status.Image = Images.error;
                }
                else
                {
                    InitializedGuiLock(true);
                }

                if (c.CONNECTED) await c.mainPipe.WriteString(ARGS.INITIALIZED);
                c.INITIALIZED = true;
            });
        }
        private void InitializedGuiLock(bool init = false)
        {
            if (c.ecode.Count > 1)
            {
                gui_lock.button_status.Image = Images.error;

                if (c.ecode.Contains(ECODE.DEVICE_READY))
                {
                    int index = 0;
                    for (int i = 0; i < c.ecode.Count(); i++)
                        if (c.ecode[i] == ECODE.DEVICE_READY) index = i;
                    c.ecode.RemoveAt(index);
                }
                
                if (c.ecode.Count() == 1) gui_lock.label_messages.Text = ECODE.ToString(c.ecode[0]);
                else
                {
                    gui_lock.label_messages.Text = "SYSTEM ERRORS FOUND";
                    Task.Run(async () =>
                    {
                        await Task.Delay(3500);
                        while (c.LOCKED)
                        {
                            foreach (int e in c.ecode)
                            {
                                gui_lock.label_messages.Text = ECODE.ToString(e);
                                await Task.Delay(3500);
                            }
                        }
                    });
                }
                
            }
            else
            {
                if (c.ecode[0] != ECODE.DEVICE_READY)
                {
                    gui_lock.label_messages.Text = ECODE.ToString(c.ecode[0]);
                    gui_lock.button_status.Image = Images.error;
                }
                else
                {
                    if (!init)
                    {
                        if (c.SESSION != 0)
                        {
                            gui_lock.label_messages.Text = c.S.ToString(c.SESSION);
                            if (c.SESSION == SESSION.TIME_RESUME) gui_lock.button_status.Image = Images.button_resume; 
                            else gui_lock.button_status.Image = Images.ready;
                        }
                    }
                    else
                    {
                        c.SESSION = SESSION.INSERT_COIN;

                        if (c.REMAINING_TIME <= 60)
                        {
                            gui_lock.label_messages.Text = c.S.ToString(SESSION.INSERT_COIN);
                            gui_lock.button_status.Image = Images.ready;
                        }
                        else
                        {
                            gui_lock.label_messages.Text = c.S.ToString(SESSION.TIME_RESUME);
                            if (c.LOCKED) gui_lock.BeginInvoke(() => gui_lock.button_status_Click(gui_lock, MouseEventArgs.Empty));
                        }
                    }
                }
            }
        }
        private void InitializeTimeIn() //Initiate when serial device is already initialized and the desktop is not locked.
        {
            c.SESSION = SESSION.TIME_IN;
            Timer(Display.UNLOCK);
            ArrangeSystemTrayIcon(false);
            ANIMATE = true;
            Task.Run(() => TrayAnimation());
            if (c.CONNECTED) c.mainPipe.WriteString(ARGS.INITIALIZED);
            c.INITIALIZED = true;
        }
        private void Timer(Display mode, int time = 0)
        {
            if (!TIMER) return;
            if (c.ADMINISTRATOR) return;

            Task.Run(() =>
            {
                this.BeginInvoke(() =>
                {
                    if (c.FormExists("GUI_timer"))
                    {

                        if (gui_timer.IsDisposed)
                        {
                            gui_timer = new GUI_timer();
                            gui_timer.Show();
                        }

                        if (!gui_timer.Visible) gui_timer.Show();

                        switch (mode)
                        {
                            case Display.UNLOCK:
                                {
                                    Tray.Visible.Text = "Hide";
                                    gui_timer.Text = "00:00:00";
                                    gui_timer.Opacity = 1.00;
                                    if (!gui_timer.Visible && !gui_timer.IsDisposed && gui_timer != null)
                                        gui_timer.Show();
                                    break;
                                }

                            case Display.HIDE:
                                {
                                    gui_timer.Opacity = 0.00;
                                    gui_timer.label_timer.Text = "00:00:00";
                                    break;
                                }

                            case Display.COUNT:
                                {
                                    if (c.SESSION == SESSION.TIME_IN || c.SESSION == SESSION.TIME_RESUME)
                                    {
                                        if (time == 60 || time == 5)
                                        {
                                            gui_timer.Opacity = 1.00;
                                            Tray.Visible.Text = "Hide";
                                            Window.Top(gui_timer.Handle);
                                        }
                                        gui_timer.label_timer.Text = TimeToString(time.ToString(), true);
                                    }
                                    break;
                                }

                            case Display.TIMEOUT:
                                {
                                    gui_timer.Opacity = 0.00;
                                    gui_timer.label_timer.Text = "00:00:00";
                                    break;
                                }

                            case Display.UPDATE:
                                {
                                    gui_timer.Opacity = 0.00;
                                    Tray.Visible.Text = "Show";
                                    break;
                                }

                            case Display.VISIBLE:
                                {
                                    if (c.CONNECTED)
                                    {
                                        if (gui_timer.Opacity <= 0.00)
                                        {
                                            gui_timer.Opacity = 1.00;
                                            Tray.Visible.Text = "Hide";
                                        }
                                        else
                                        {
                                            gui_timer.Opacity = 0.00;
                                            Tray.Visible.Text = "Show";
                                        }
                                    }
                                    break;
                                }

                        }
                    }
                    else
                    {
                        if (gui_timer == null || gui_timer.IsDisposed) gui_timer = new GUI_timer();
                        gui_timer.Show();
                    }
                });
            });
        }
        private void TrayAnimation()
        {
            Task.Run(async () =>
           {
               while (ANIMATE && c.CONNECTED)
               {
                   try { this.BeginInvoke(() => Tray.Notify.Icon = Images.icons[ICON]); }
                   catch (Exception ex) { Logs.System("Animation error : " + ex.Message, c.LOGPATH, c.ADDLOG); return; }
                   ICON++;
                   if (ICON >= 8) ICON = 0;
                   await Task.Delay(1500);
               }
               ICON = 0;
           });
        }
        public void UnlockDesktop(bool ADMIN = false)
        {
            Task.Run(async () =>
            {
                //LOCKCHECKER = false;

                if (c.FormExists("GUI_login"))
                    gui_login.Close();

                if (c.FormExists("GUI_dialog"))
                    dialog.Close();

                if (c.FormExists("GUI_console"))
                    gui_console.Close();

                while (!defaultDesk.Show())
                    await Task.Delay(10);

                while (!Desktop.SetCurrent(defaultDesk))
                    await Task.Delay(10);

                c.LOCKED = false;
                Sounds.Unmute(Handle);

                if (c.CONNECTED)
                {
                    if (ADMIN) await c.mainPipe.WriteString(ARGS.LOGIN);
                    else await c.mainPipe.WriteString(ARGS.UNLOCKED);
                }
                else
                {
                    if (ADMIN && shutdownTimer.Enabled) shutdownTimer.Stop();
                }

                while (!c.INITIALIZED)
                    await Task.Delay(10);

                c.ADMINISTRATOR = ADMIN;
                //c.SESSION = ADMIN ? SESSION.ADMIN_LOGIN : (c.SESSION == SESSION.RESUME_TIME ? SESSION.RESUME_TIME : SESSION.TIME_IN);
                if (ADMIN)
                {
                    if (Power.Enabled) Power.Enabled = false; //Awake all the time
                    c.SESSION = SESSION.ADMIN_LOGIN;
                }
                else
                {
                    Power.Enabled = true; //Awake all the time;
                    if (c.REMAINING_TIME != 0)
                        c.REMAINING_TIME = 0;

                    if (c.SESSION != SESSION.TIME_RESUME)
                    {
                        c.SESSION = SESSION.TIME_IN;
                        this.BeginInvoke(() =>
                        {
                            gui_lock.button_status.Image = Images.ready;
                            gui_lock.button_status.Cursor = Cursors.Default;
                        });
                    }

                    Timer(Display.UNLOCK);
                }

                this.BeginInvoke(() =>
                {
                    if (c.FormExists("GUI_lock"))
                    {
                        if (gui_lock.BackColor != Color.Black) gui_lock.BackColor = Color.Black;
                        gui_lock.label_timer.Text = TimeToString(Config.SHUTDOWN.ToString()); //Clear or change back the data in lock form.
                        gui_lock.label_messages.Text = c.S.ToString(c.SESSION);
                    }

                    ArrangeSystemTrayIcon(ADMIN);
                });
            });
        }
        private void ArrangeSystemTrayIcon(bool ADMIN)
        {
            if (ADMIN) //admin logged
            {
                if (c.ecode.Count > 0)
                {
                    if (c.ecode.Count() > 1)
                    {
                        Tray.Notify.Icon = Images.admin_x;
                        Tray.Lock.Enabled = false;
                        
                        if (c.ecode.Contains(ECODE.DEVICE_DISCONNECT))
                        {
                            int index = -1;
                            for (int i=0; i < c.ecode.Count(); i++)
                            {
                                if (c.ecode[i] == ECODE.DEVICE_READY)
                                {
                                    index = i;
                                    break;
                                }
                            }
                            c.ecode.RemoveAt(index);
                        }
                    }
                    else
                    {
                        if (c.ecode[0] == ECODE.DEVICE_READY)
                            Tray.Notify.Icon = Images.admin;
                        else
                        {
                            Tray.Notify.Icon = Images.admin_x;
                            Tray.Lock.Enabled = false;
                        }
                            
                    }
                }
                else
                {
                    Tray.Notify.Icon = Images.admin_x;
                    Tray.Lock.Enabled = false;
                }

                Tray.Visible.Visible = false;
                Tray.Logout.Visible = false;
                Tray.Console.Visible = true;
                Tray.Lock.Visible = true;
                Tray.Notify.Visible = true;
                
                string balloonText = string.Empty;

                if (c.UPDATES > 0)
                {
                    Tray.Update.Visible = true;
                    balloonText = "New version of " + (c.UPDATES == 1 ? "software is available!" : (c.UPDATES == 2 ? "device is available!" : "software and device are available!"));
                }
                else
                {
                    Tray.Update.Visible = false;
                    balloonText = "Session: " + c.S.ToString(c.SESSION);
                }
                
                Tray.Notify.BalloonTipText = balloonText;
                Tray.Notify.ShowBalloonTip(3000);
            }
            else //user logged
            {
                Tray.Notify.Icon = Images.coinapp;
                Tray.Update.Visible = false;
                Tray.Console.Visible = false;
                Tray.Lock.Visible = false;
                Tray.Visible.Visible = true;
                Tray.Logout.Visible = true;
                Tray.Notify.Visible = true;
            }
        }
        public void LockDesktop()
        {
            Task.Run(async () =>
            {
                //while (!c.DESKTOP_READY)
                //    await Task.Delay(10);

                if (ANIMATE) ANIMATE = false;

                if (c.FormExists("GUI_login"))
                    gui_login.Close();

                if (c.FormExists("GUI_dialog"))
                    dialog.Close();

                if (c.FormExists("GUI_console"))
                    gui_console.Close();

                await Task.Delay(100);

                while (!coinappDesk.Show())
                    await Task.Delay(10);

                while (!Desktop.SetCurrent(coinappDesk))
                    await Task.Delay(10);
                
                c.LOCKED = true;
                Sounds.Mute(Handle);

                if (!Power.Enabled) Power.Enabled = true;

                c.ADMINISTRATOR = false;

                if (c.FormExists("GUI_lock"))
                {
                    if (c.ecode.Count == 0) //Startup of the program.
                    {
                        gui_lock.label_messages.Text = "SYSTEM INITIALIZING...";
                        gui_lock.button_status.Image = Images.initialize;
                    }
                    else
                    {
                        InitializedGuiLock();
                    }
                }
                else
                {
                    if (!c.ecode.Contains(ECODE.LOCK_SCREEN_TERMINATED)) c.ecode.Add(ECODE.LOCK_SCREEN_TERMINATED);
                }

                if (c.CONNECTED)
                {
                    if (c.SESSION == SESSION.USER_LOGOUT) await c.mainPipe.WriteString(ARGS.LOGOUT);
                    else await c.mainPipe.WriteString(ARGS.LOCKED);
                }
            });

            LockDesktopChecker();
        }
        private void LockDesktopChecker()
        {
            Task.Run(async () =>
            {
                while (c.SESSION == SESSION.INSERT_COIN || c.SESSION == SESSION.ADMIN_LOGOUT || c.SESSION == SESSION.TIME_OUT || c.SESSION == SESSION.TIME_PAUSE || c.SESSION == SESSION.TIME_SAVED || c.SESSION == SESSION.USER_LOGOUT)
                {
                    if (!c.LOCKED) return;
                    if (string.Equals(Desktop.OpenInputDesktop().DesktopName, coinappDeskName, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        coinappDesk.Show();
                        Desktop.SetCurrent(coinappDesk);
                    }

                    await Task.Delay(10);
                }
            });
        }
        #endregion
    }

}