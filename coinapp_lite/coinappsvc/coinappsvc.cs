using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Win32;
using coinapplib;
using System.Runtime.InteropServices;

namespace coinappsvc
{
    public partial class coinappsvc : ServiceBase
    {
        private IntPtr deviceNotifyHandle;
        private IntPtr directoryHandle;
        private IntPtr deviceEventHandle;
        private Win32.ServiceControlHandlerEx myCallback;
        
        private ServerPipe mainPipe;// = null;
        private SerialPort serialPort;// = null;
        private Process mainProcess;// = null;
        private CONFIG Config;// = null;
        private System.Timers.Timer shutdownTimer;// = null;        
        
        private readonly string XmlPath = Application.StartupPath + @"\coinapp.xml";
        private readonly string appName = Application.StartupPath + @"\coinapp.exe";
        
        private const string mainPipeName = "coinapp";
        private string SYSTEM_PASSWORD = string.Empty;
        private string SYSTEM_EMAIL = string.Empty;
        private string DEVICE_VERSION = string.Empty;

        private bool DEVICE_DISCONNECT = false;
        private bool DEVICE_READY = false;
        private bool SYSTEM_LOCKED = false;
        private bool CONNECTED = false;
        private bool INITIALIZED = false;
        private bool DEVICE_INIT = false;
        private bool LOCKIMAGE = false;
        private bool STOP = false;
        private int CREDITS = 0;
        private int REMAINING_TIME = 0; 
        private List<int> ecode = new List<int>();
        

        //Default values
        public static string LOGPATH { get; set; } = Application.StartupPath + @"\";
        public static bool ADDLOG { get; set; } = false;
        public static int UPDATEINTERVAL { get; set; } = 60;

        private int SHUTDOWN = 20;
        private int SHUTDOWN_SAFE { get; set; } = 330;
        private int REINITIALIZE { get; set; } = 3;
        private int SHUTDOWN_ERROR { get; set; } = 0;

        #region Service methods
        public coinappsvc()
        {
            InitializeComponent();
            this.CanShutdown = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanHandlePowerEvent = true;
        }
        protected override void OnStart(string[] args)
        {
            SvcXmlParser();
            Logs.System("Service starting...", LOGPATH, ADDLOG);

            StartPipeCommunication(mainPipeName); //Start communication to GUI logo app.
            Task.Run(() => SoftwareUpdateChecker());

            if (My.SystemConfigurationExists == false) //Check system configurations.
            {
                ecode.Add(ECODE.SYSTEM_CONFIGURATION_FILE_NOT_FOUND);
                Logs.System("ERROR! System configuration not found.", LOGPATH, ADDLOG);
            } 
            else
            {
                SYSTEM_PASSWORD = My.Password;
                SYSTEM_EMAIL = My.Email;            
            }

            if (Settings.ConfigurationExists == false) //Check if coinapp.ini is exists.
            {
                ecode.Add(ECODE.CONFIGURATION_FILE_NOT_FOUND);
                Logs.System("ERROR! Application configuration not found. Default config will be use.", LOGPATH, ADDLOG);
                Config = new CONFIG();
            }
            else
            {
                Config = new CONFIG(); 
                if (File.Exists(Config.LOCKIMAGE))
                {
                    Bitmap bmp = null;
                    try { bmp = new Bitmap(Config.LOCKIMAGE); }
                    catch (Exception ex) { Logs.System(ex.Message, LOGPATH, ADDLOG); }

                    if (bmp != null)
                    {
                        bmp.Dispose();
                        LOCKIMAGE = true;
                    }
                }
            }
            
            Config = Config ?? new CONFIG();
            serialPort = new SerialPort();
            serialPort.DataReceived += SerialPort_DataReceived;
            Task.Run(async () => await StartSerialDevice());

            shutdownTimer = new System.Timers.Timer(1000);
            shutdownTimer.Elapsed += ShutdownTimer_Elapsed;
            SHUTDOWN = SHUTDOWN_SAFE;
            shutdownTimer.Start(); //Shutdown in 10 minutes if no connection to the main application.
            Logs.System("Safe shutdown is started in " + SHUTDOWN_SAFE + " seconds", LOGPATH, ADDLOG);
            Logs.System("Service started.", LOGPATH, ADDLOG);     
        }
        protected override void OnStop()
        {
            if (STOP) { Logs.System("REQUESTED! Service stopped.", LOGPATH, ADDLOG); this.Dispose(); }
            else
            {
                Logs.System("WARNING! Service stopped unexpectedly.", LOGPATH, ADDLOG);
                if (mainProcess != null)
                {
                    foreach (Process proc in Process.GetProcesses())
                    {
                        if (mainProcess.Id == proc.Id)
                        {
                            mainProcess.Kill();
                            Logs.System("! Application process killed by the service.", LOGPATH, ADDLOG);
                        }
                    }
                }
            }
        }
        protected override void OnShutdown()
        {
            if (serialPort.IsOpen)
            {
                if (DEVICE_READY) serialPort.Write("DC;");
                serialPort.Close();
                serialPort.Dispose();
            }
            Logs.System("WARNING!  System detected OnShutdown event.", LOGPATH, ADDLOG);
            base.OnShutdown();
        }
        #endregion Service methods

        #region System methods
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    Logs.System("Session LOGON.", LOGPATH, ADDLOG);
                    this.Stop();
                    break;
                case SessionChangeReason.SessionLogoff:
                    Logs.System("Session LOGOFF.", LOGPATH, ADDLOG);
                    break;
                case SessionChangeReason.SessionLock:
                    Logs.System("Session LOCK.", LOGPATH, ADDLOG);
                    //Logs.S("Session ID : " + changeDescription.SessionId + " LOCK");
                    break;
                case SessionChangeReason.SessionUnlock:
                    Logs.System("Session UNLOCK.", LOGPATH, ADDLOG);
                    break;
            }

            base.OnSessionChange(changeDescription);
        }
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            if (powerStatus.HasFlag(PowerBroadcastStatus.QuerySuspend))
            {
                Logs.System("PowerStatus : QuerySuspended.", LOGPATH, ADDLOG);
            }

            if (powerStatus.HasFlag(PowerBroadcastStatus.ResumeSuspend))
            {
                Logs.System("PowerStatus : ResumeSuspend.", LOGPATH, ADDLOG);
            }
            return base.OnPowerEvent(powerStatus);
        }
        private void SystemEvents_SessionEnded(object sender, SessionEndedEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    Logs.System("SESSION ENDED: System is logging off.", LOGPATH, ADDLOG);
                    break;
                case SessionEndReasons.SystemShutdown:
                    Logs.System("SESSION ENDED: System is shutting down.", LOGPATH, ADDLOG);
                    break;
            }
        }
        private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            if (control == Win32.SERVICE_CONTROL_STOP || control == Win32.SERVICE_CONTROL_SHUTDOWN)
            {
                UnregisterHandles();
                Win32.UnregisterDeviceNotification(deviceEventHandle);
                base.Stop();
            }
            else if (control == Win32.SERVICE_CONTROL_DEVICEEVENT)
            {
                if (eventType == Win32.DBT_DEVICEQUERYREMOVE || eventType == Win32.DBT_DEVICEREMOVECOMPLETE)
                {
                    Logs.System("WARNING! A device from machine was removed completely.", LOGPATH, ADDLOG);
                    UnregisterHandles();
                    Win32.UnregisterDeviceNotification(deviceEventHandle);
                    Task.Run(async () => {
                        if (DEVICE_READY)
                        {
                            if (!serialPort.IsOpen)
                            {
                                if (!CONNECTED)
                                {
                                    Logs.System("System restart initiated because of fail application.", LOGPATH, ADDLOG);
                                    Shutdown.Restart();
                                }
                                else
                                {
                                    Logs.System("WARNING! Device is disconnected.", LOGPATH, ADDLOG);
                                    await Send(ARGS.DEVICE_DC);
                                }
                            }
                            else
                            {
                                Logs.System("Checking if the device associated by application is disconnected...", LOGPATH, ADDLOG);
                                DEVICE_DISCONNECT = true;
                                serialPort.Write("LIVE;");
                                await Task.Delay(1000);
                                if (DEVICE_DISCONNECT)
                                {
                                    Logs.System("WARNING! Device is disconnected.", LOGPATH, ADDLOG);
                                    if (!CONNECTED)
                                    {
                                        Logs.System("System restart initiated because of fail application.", LOGPATH, ADDLOG);
                                        Shutdown.Restart();
                                    }
                                    else
                                    {
                                        await Send(ARGS.DEVICE_DC);
                                    }
                                }
                                else
                                {
                                    Logs.System("OK! device is alive.", LOGPATH, ADDLOG);
                                    RegisterDeviceNotification();
                                }

                            }
                        }
                    });

                }
            }

            return 0;
        }
        private void RegisterDeviceNotification()
        {
            myCallback = new Win32.ServiceControlHandlerEx(ServiceControlHandler);
            Win32.RegisterServiceCtrlHandlerEx(this.ServiceName, myCallback, IntPtr.Zero);

            if (this.ServiceHandle == IntPtr.Zero)
            {
                // TODO handle error
                Logs.System("RegisterDeviceNotification service handle is 0.", LOGPATH, ADDLOG);
            }

            Win32.DEV_BROADCAST_DEVICEINTERFACE deviceInterface = new Win32.DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(deviceInterface);
            deviceInterface.dbcc_size = size;
            deviceInterface.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
            IntPtr buffer = default(IntPtr);
            buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(deviceInterface, buffer, true);
            deviceEventHandle = Win32.RegisterDeviceNotification(this.ServiceHandle, buffer, Win32.DEVICE_NOTIFY_SERVICE_HANDLE | Win32.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
            if (deviceEventHandle == IntPtr.Zero)
            {
                // TODO handle error
                Logs.System("RegisterDeviceNotification device event handle is 0.", LOGPATH, ADDLOG);
            }
        }
        private void UnregisterHandles()
        {
            if (directoryHandle != IntPtr.Zero)
            {
                Win32.CloseHandle(directoryHandle);
                directoryHandle = IntPtr.Zero;
            }
            if (deviceNotifyHandle != IntPtr.Zero)
            {
                Win32.UnregisterDeviceNotification(deviceNotifyHandle);
                deviceNotifyHandle = IntPtr.Zero;
            }
        }
        #endregion System methods

        #region Applicatoin communication
        private void MainProcessExited(object sender, EventArgs e)
        {
            if (STOP) { CONNECTED = false; Logs.System("REQUESTED! Main application closed.", LOGPATH, ADDLOG); return; }
            
            Logs.System("WARNING Application process terminated unexpectedly. Exit code : " + mainProcess.ExitCode, LOGPATH, ADDLOG);
            CONNECTED = false;       
            try
            {
                mainPipe.Flush();
                mainPipe.Close();
                mainPipe.DataReceived -= MainPipe_DataReceived;
                mainPipe.Connected -= MainPipe_Connected;
                mainPipe.PipeClosed -= MainPipe_Closed;
                mainPipe = null;
            }
            catch (Exception ex) { Logs.System(ex.Message, LOGPATH, ADDLOG); }

            try
            {
                mainProcess.Dispose();
                mainProcess.Close();
                mainProcess = null;
            }
            catch (Exception ex) { Logs.System(ex.Message, LOGPATH, ADDLOG); }

            #region Run process again ang again if terminated.
            //Task.Run(async () =>
            //{
            //    StartServerPipeCommunication(mainPipeName);

            //    //Run MAIN APPLICATION from default desktop.
            //    if (RunProcess.Start(mainAppName, out RunProcess.PROCESS_INFORMATION procInfo) == false)
            //        Logs.Svc("ERROR! Failed creating application process.", s.LOGPATH, s.ADDLOG);
            //    else
            //    {
            //        Logs.Svc("SUCCESS! Main application process started.", s.LOGPATH, s.ADDLOG);
            //        mainProcess = Process.GetProcessById((int)procInfo.dwProcessId);

            //        if (mainProcess != null)
            //        {
            //            mainProcess.EnableRaisingEvents = true;
            //            mainProcess.Exited += MainProcessExited;
            //        }
            //    }
            //});
            #endregion
            //or
            #region Start XML seconds shutdown if process is terminated.
            if (SHUTDOWN_ERROR > 0)
            {
                if (!SYSTEM_LOCKED)
                {
                    SHUTDOWN = SHUTDOWN_ERROR;
                    RunProcess.Start(appName + " " + ARGS.START_MSG + " &Sytem error.\nSystem is shutting down.", out RunProcess.PROCESS_INFORMATION procInfo);
                    Logs.System("WARNING Shutdown initiated within " + SHUTDOWN_ERROR + " seconds, due to main application exited.", LOGPATH, ADDLOG);
                    if (!shutdownTimer.Enabled) shutdownTimer.Start();
                }
            }
            else
            {
                Logs.System("WARNING! System is restarting imidiately due to main application communication closed.", LOGPATH, ADDLOG);
                Shutdown.Restart();
            }
            #endregion
        }
        private void MainPipe_Closed(object sender, EventArgs e)
        {
            CONNECTED = false;
            
            if (STOP) { Logs.System("REQUESTED! Communication to main application closed.", LOGPATH, ADDLOG); return; }

            Logs.System("WARNING! Communication to main application closed unexpectedly.", LOGPATH, ADDLOG);

            if (mainProcess != null)
            {
                Logs.System("WARNING! Process is not properly disposed.", LOGPATH, ADDLOG);
                Process proc = Process.GetProcessById(mainProcess.Id);

                if (proc != null)
                {
                    Logs.System("WARNING! Process is running. Process kill initiated.", LOGPATH, ADDLOG);
                    proc.Kill();


                    if (SHUTDOWN_ERROR > 0)
                    {
                        if (!SYSTEM_LOCKED)
                        {
                            SHUTDOWN = SHUTDOWN_ERROR;
                            RunProcess.Start(appName + " " + ARGS.START_MSG + " &Sytem error.\nSystem is shutting down.", out RunProcess.PROCESS_INFORMATION procInfo);
                            Logs.System("WARNING Shutdown initiated within " + SHUTDOWN_ERROR + " seconds, due to main application communication closed.", LOGPATH, ADDLOG);
                            if (!shutdownTimer.Enabled) shutdownTimer.Start();
                        }
                    }
                    else
                    {
                        Logs.System("WARNING! System is restarting imidiately due to main application communication closed.", LOGPATH, ADDLOG);
                        Shutdown.Restart();
                    }

                }
            }
            

            //if (!SYSTEM_LOCKED)
            //{
            //    CONNECTED = false;
            //    //Check process here if alive.
            //    if (mainProcess != null)
            //    {
            //        foreach (Process p in Process.GetProcessesByName("coinapp"))
            //        {
            //            if (p.Id == mainProcess.Id)
            //            {
            //                p.Kill();
            //            }
            //        }

            //    }


            //}
        }
        private void MainPipe_Connected(object sender, EventArgs e)
        {
            if (shutdownTimer.Enabled) shutdownTimer.Stop();
            SHUTDOWN = Config.SHUTDOWN > 20 ? Config.SHUTDOWN : 20;

            Logs.System("Safe shutdown stopped.", LOGPATH, ADDLOG);
            Logs.System("Main application communication started. Sending data...", LOGPATH, ADDLOG);
            CONNECTED = true;
            Task.Run(async () =>
            {
                try
                {
                    await Send("HK", Config.HOTKEYCODE);
                    await Send("SD", SHUTDOWN);
                    await Send("PA", SYSTEM_PASSWORD);
                    await Send("EM", SYSTEM_EMAIL);
                    await Send("LA", Config.LOGINATTEMPT);
                    await Send("LI", Config.LOCKIMAGE, LOCKIMAGE);
                    await Send("TM", Config.TIMER);
                    await Send("PN", Config.PORTNUMBER);
                    await Send("BR", Config.BAUDRATE);
                    await Send("CS", Config.COINSLOT);
                    await Send("C1", Config.COIN1);
                    await Send("C2", Config.COIN2, Config.COINSLOT == 2 || Config.COINSLOT == 3);
                    await Send("C3", Config.COIN3, Config.COINSLOT == 3);
                    await Send("P1", Config.PULSE1, Config.COINSLOT == 3);
                    await Send("P2", Config.PULSE2, Config.COINSLOT == 3);
                    await Send("P3", Config.PULSE3, Config.COINSLOT == 3);
                    await Send("SN", Config.SOUND);
                    await Send("IV", Config.INTERVAL);
                    await Send("SV", Config.SAVEDATA);
                    if (My.SavedataPathExists(out string path)) await Send("SP", path, Config.SAVEDATA > 0);
                    await Send("LG", Config.LOG);
                    await Send(ARGS.CONNECTED);
                    Logs.System("Successfuly sent all data.", LOGPATH, ADDLOG);

                    if (CREDITS <= 0 || !INITIALIZED)
                    {
                        await Send(ARGS.LOCK);
                    }
                    else
                    {
                        if (ecode.Count > 0)
                            foreach (int ec in ecode)
                                await Send("EC", ec);

                        await Send(ARGS.TIMEIN);
                    }

                    while (!DEVICE_INIT)
                        await Task.Delay(10);

                    if (ecode.Count > 0)
                        foreach (int ec in ecode)
                            await Send("EC", ec);

                    if (My.RestoreTimeResume(out int rem) && rem > 60 && CREDITS == 0)
                    {
                        REMAINING_TIME = rem;
                        await Send("RT", rem);
                    }

                    if (CREDITS <= 0) await Send(ARGS.INITIALIZE);
                    else await Send(ARGS.TIMEIN);
                }
                catch (Exception ex)
                {
                    Logs.System("SENDIN DATA : " + ex.Message, LOGPATH, ADDLOG);

                }
            });
        }
        private void MainPipe_DataReceived(object sender, PipeEventArgs e)
        { 
            try
            {
                string mainData = e.String;
                //Logs.System("Received from Main : " + mainData, LOGPATH, ADDLOG);
                switch (mainData)
                {
                    case ARGS.LOGOUT:
                        {
                            SYSTEM_LOCKED = true;
                            if (serialPort.IsOpen)
                            {
                                serialPort.Write("AD=0"); //Make device user mode. (accepting coins)
                                serialPort.Write("CT=0"); //Turn off the counting on device.
                                serialPort.Write("CD=0");//Make credits 0;
                                if (Config.SAVEDATA > 0) if (!My.ClearTimeResume()) Logs.System("Failed clearing time resume.", LOGPATH, ADDLOG);//Erase saved coins in encypted text.
                            }
                            CREDITS = 0;
                            SHUTDOWN = Config.SHUTDOWN;
                            if (!shutdownTimer.Enabled) shutdownTimer.Start();
                            Logs.System("WARNING! Shutdown started in " + SHUTDOWN + " seconds", LOGPATH, ADDLOG);
                            break;
                        }

                    case ARGS.LOCKED:
                        {
                            SYSTEM_LOCKED = true;
                            if (serialPort.IsOpen && DEVICE_READY)
                            {
                                serialPort.Write("AD=0"); //Make device user mode. (accepting coins)
                                serialPort.Write("CT=0"); //Turn off the counting on device.
                                serialPort.Write("CD=0");//Make credits 0;
                            }

                            SHUTDOWN = Config.SHUTDOWN;
                            if (!shutdownTimer.Enabled) shutdownTimer.Start();
                            Logs.System("WARNING! Shutdown started in " + SHUTDOWN + " seconds", LOGPATH, ADDLOG); ///////////////////
                            break;
                        }

                    case ARGS.UNLOCKED:
                        {
                            SYSTEM_LOCKED = false;
                            if (shutdownTimer.Enabled) shutdownTimer.Stop();
                            Logs.System("Shutdown stopped.", LOGPATH, ADDLOG);
                            Logs.System("Desktop unlocked", LOGPATH, ADDLOG);
                            break;
                        }
                    case ARGS.DEVICE_DC:
                        {
                            SYSTEM_LOCKED = true;
                            break;
                        }
                        
                    case ARGS.LOGIN:
                        {
                            SYSTEM_LOCKED = false;
                            if (shutdownTimer.Enabled) shutdownTimer.Stop();
                            Logs.System("Shutdown stopped.", LOGPATH, ADDLOG);
                            Logs.System("Admin login", LOGPATH, ADDLOG);
                            Task.Run(async () =>
                            {
                                while (!INITIALIZED)
                                    await Task.Delay(10);
                                if (serialPort.IsOpen && DEVICE_READY) serialPort.Write("AD=1");
                            });
                            break;
                        }

                    case ARGS.RESUME:
                        {
                            if (serialPort.IsOpen && CONNECTED)
                            {
                                Send(ARGS.UNLOCK);
                                serialPort.Write("CD=" + REMAINING_TIME);
                                serialPort.Write("CT=1");
                                Send("AN", "1");
                            }
                            break;
                        }

                    case ARGS.INITIALIZED:
                        {
                            INITIALIZED = true;
                            
                            if (UPDATEINTERVAL >= 60)
                            {
                                UPDATEINTERVAL *= 1000;
                                Task.Run(async () =>
                                {
                                    while (true)
                                    {
                                        await SoftwareUpdateChecker();
                                        await DeviceUpdateChecker(DEVICE_VERSION);
                                        await Task.Delay(UPDATEINTERVAL);
                                    }
                                });
                            }
                            
                            if (!DEVICE_READY && REINITIALIZE != 0) //Reconnect device if failed.
                            {
                                #region SELFT RECONNECT SERIAL DEVICE
                                //Self reconnection for serial device
                                try { if (serialPort.IsOpen) serialPort.Close(); }
                                catch (IOException ex)
                                {
                                    Logs.System(ex.Message, LOGPATH, ADDLOG);
                                    break;
                                }

                                Task.Run(async () =>
                                {
                                    Logs.System("Self reconnection serial device begin in 10 seconds.", LOGPATH, ADDLOG);
                                    await Task.Delay(5000);
                                    await Send("RC", "DEVICE WILL RESTART IN A MOMENT");
                                    await Task.Delay(5000);
                                    int WAIT = 5;

                                    while (WAIT > -1)
                                    {
                                        if (!SYSTEM_LOCKED)
                                        {
                                            Logs.System("Self reconnection canceled.", LOGPATH, ADDLOG);
                                            return;
                                        }
                                        Logs.System("RE-INITIALIZE DEVICE IN " + WAIT, LOGPATH, ADDLOG);
                                        await Send("RC", "RESTARTING DEVICE IN " + WAIT, CONNECTED && SYSTEM_LOCKED);
                                        WAIT--;
                                        await Task.Delay(1000);
                                    }
                                    await Send("RC", "DEVICE INITIALIZING...", CONNECTED);
                                    if (!SYSTEM_LOCKED) return;
                                    //Remove all stored ecode.
                                    await Send(ARGS.REINIT);

                                    if (ecode.Count != 0)
                                    {
                                        int index = 0;
                                        for (int ec = 0; ec < ecode.Count; ec++)
                                        {
                                            if (ecode[ec] == ECODE.DEVICE_NOT_COMPATIBLE || ecode[ec] == ECODE.DEVICE_NOT_DETECTED || ecode[ec] == ECODE.DEVICE_INFO_ERROR) index = ec;
                                        } 
                                        ecode.RemoveAt(index);
                                    }

                                    await StartSerialDevice();

                                    if (ecode.Count > 0)
                                        foreach (int ec in ecode)
                                            await Send("EC", ec);

                                    await Send(ARGS.INITIALIZE);
                                    REINITIALIZE--;

                                    if (REINITIALIZE <= 0)
                                    {
                                        if (!SYSTEM_LOCKED) return;
                                        await Send("RC","FAILED INITIALIZING DEVICE", CONNECTED);
                                    }
                                });
                                #endregion SELFT RECONNECT SERIAL DEVICE
                            }
                            break;
                        }

                    case ARGS.SHUTDOWN:
                        {
                            Logs.System("Application request for shutdown. Shutdown immidiately.", LOGPATH, ADDLOG);
                            STOP = true;
                            Send(ARGS.STOPPED);
                            mainPipe.Flush();
                            mainPipe.Close();
                            if (serialPort.IsOpen)
                            {
                                if (DEVICE_READY) serialPort.Write("DC;");
                                serialPort.Close();
                                serialPort.Dispose();
                            }
                            Shutdown.Force();
                            break;
                        }

                    case ARGS.STOP:
                        {
                            STOP = true;
                            Send(ARGS.STOPPED);
                            mainPipe.Flush();
                            mainPipe.Close();
                            if (serialPort.IsOpen)
                            {
                                if (DEVICE_READY) serialPort.Write("DC;");
                                serialPort.Close();
                                serialPort.Dispose();
                            }
                            this.Stop();
                            break;
                        }

                    case ARGS.RESTART:
                        {
                            STOP = true;
                            //Send(ARGS.STOPPED);
                            mainPipe.Flush();
                            mainPipe.Close();
                            if (serialPort.IsOpen)
                            {
                                if (DEVICE_READY) serialPort.Write("DC;");
                                serialPort.Close();
                                serialPort.Dispose();
                            }
                            //RunProcess.Start(appName + " " + ARGS.START_BOOT, out RunProcess.PROCESS_INFORMATION procInfo);
                            this.Stop();
                            break;
                        }

                    default:
                        {
                            string sent = mainData.Substring(0, mainData.IndexOf("="));
                            string dataString = mainData.Substring(mainData.IndexOf("=") + 1);
                            int dataInt = int.TryParse(dataString, out dataInt) ? dataInt : 0;

                            switch (sent)
                            {
                                case "PID":
                                    mainProcess = Process.GetProcessById(dataInt);
                                    if (mainProcess != null)
                                    {
                                        mainProcess.EnableRaisingEvents = true;
                                        mainProcess.Exited += MainProcessExited;
                                    }
                                    break;

                                case "SERIAL":
                                    {
                                        if (serialPort.IsOpen && DEVICE_READY) serialPort.Write(dataString);
                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                Logs.System("COMM : " + ex.Message, LOGPATH, ADDLOG);
            }
        }
        #endregion Main applicatoin communication

        #region Custom methods
        private async Task<bool> Send(string comm, object value, bool condition = true)
        {
            if (condition == false)
                return false;

            if (CONNECTED) 
            {
                await mainPipe.WriteString(comm + "=" + value);
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task<bool> Send(string args, bool condition = true)
        {
            if (!condition)
                return false;
           
            if (CONNECTED)
            {
                await mainPipe.WriteString(args);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void StartPipeCommunication(string pipeName)
        {
            Logs.System("Communication started. Waiting for connection.");
            if (mainPipe != null) //Clear the previous pipe communication.
            {
                mainPipe.DataReceived -= MainPipe_DataReceived;
                mainPipe.Connected -= MainPipe_Connected;
                mainPipe.PipeClosed -= MainPipe_Closed;
                mainPipe = null;
            }

            mainPipe = new ServerPipe(pipeName);
            mainPipe.DataReceived += MainPipe_DataReceived;
            mainPipe.Connected += MainPipe_Connected;
            mainPipe.PipeClosed += MainPipe_Closed;
        }
        private async Task StartSerialDevice()
        {
            Logs.System("Initializing serial device...");
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                ecode.Add(ECODE.DEVICE_NOT_DETECTED);
                DEVICE_INIT = true;
                Logs.System("No device connected.");
                return;
            }
            
            if (!ports.Contains("COM" + Config.PORTNUMBER))
            {
                ecode.Add(ECODE.DEVICE_INFO_ERROR);
                DEVICE_INIT = true;
                Logs.System("Device information error.");
                return;
            }

            
            serialPort.PortName = "COM" + Config.PORTNUMBER;
            serialPort.BaudRate = Config.BAUDRATE;
            serialPort.DtrEnable = true;

            try { serialPort.Open(); }
            catch (Exception ex)
            {
                Logs.System("ERROR! " + ex.Message, LOGPATH, ADDLOG);
                ecode.Add(ECODE.DEVICE_PORT_CANNOT_OPEN);
                DEVICE_INIT = true;
                return;
            }

            await Task.Delay(3000);
            serialPort.Write("COINAPP;");
            await Task.Delay(1000);

            if (!DEVICE_READY)
            {
                ecode.Add(ECODE.DEVICE_NOT_COMPATIBLE);
                Logs.System("ERROR! Device is not compatible.", LOGPATH, ADDLOG);
            }
            else
            {
                ecode.Add(ECODE.DEVICE_READY);
                RegisterDeviceNotification();
                Logs.System("SUCCESS! Device initialized.", LOGPATH, ADDLOG);
            }

            DEVICE_INIT = true;
        }
        private async void ShutdownTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (CONNECTED && !STOP)
            {
                await mainPipe.WriteString("SD=" + SHUTDOWN.ToString());
                if (SHUTDOWN <= 0)
                {
                    if (shutdownTimer.Enabled) shutdownTimer.Stop();
                    if (serialPort.IsOpen && DEVICE_READY) serialPort.Write("DC;");
                    Logs.System("WARNING! The deskttop is now shutting down...", LOGPATH, ADDLOG);
                    Shutdown.Force();
                }
                SHUTDOWN--;
            }
            else ///Safe shutdown here
            {
                if (SHUTDOWN <= 0)
                {
                    if (shutdownTimer.Enabled) shutdownTimer.Stop();
                    Logs.System("WARNING! System restarting unexpextedly.", LOGPATH, ADDLOG);
                    Shutdown.Restart();
                }
                SHUTDOWN--;
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort != null) //Check if serialPort if not null.
                {
                    string serialData = serialPort.ReadTo(serialPort.NewLine); //Received serial port data sent by device.
                    //Logs.System("Serial data : " + serialData, LOGPATH, ADDLOG);
                    if (DEVICE_READY)
                    {
                        if (int.TryParse(serialData, out int credits))
                        {
                            CREDITS = credits;

                            if (SYSTEM_LOCKED && INITIALIZED) mainPipe.WriteString(ARGS.UNLOCK);

                            ///// add if display is > 1 send this.
                            if (CONNECTED) mainPipe.WriteString("CD=" + CREDITS);

                            if (Config.SAVEDATA > 0)
                                if (!My.WriteTimeResume(CREDITS)) //Save credits to text file.
                                    Logs.System("ERROR! Failed saving credits : " + CREDITS, LOGPATH, ADDLOG);

                            if (Config.SOUND > 0 && CONNECTED)
                            {
                                //Play sounds here when reaching warning time.
                                if (CREDITS == 60)
                                {
                                    if (Config.SOUND == 2) mainPipe.WriteString("PS=SMW");
                                    else mainPipe.WriteString("PS=WARN");
                                }

                                if (CREDITS == 5)
                                {
                                    if (Config.SOUND == 2) mainPipe.WriteString("PS=SMO");
                                    else mainPipe.WriteString("PS=SMO");
                                }

                            }
                        }
                        else
                        {
                            switch (serialData)
                            {
                                case "COIN1":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin1 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN2":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin2 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN3":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin3 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN4":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin4 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN5":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin5 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN6":
                                    {
                                        InsertCoin();
                                        Logs.System("Coin6 inserted", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "COIN":
                                    {
                                        InsertCoin();
                                        Logs.System("Unknown coin detected.", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "ZERO":
                                    {
                                        Logs.System("User time consumed.", LOGPATH, ADDLOG);
                                        if (CONNECTED) mainPipe.WriteString(ARGS.TIMEOUT);
                                        else
                                        {
                                            ///Check main process. If not found create a process
                                            ///to lock the computer.
                                        }

                                        if (Config.SAVEDATA > 0) if (!My.ClearTimeResume()) Logs.System("Cannot clear save data file.", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "ALIVE":
                                    {
                                        DEVICE_DISCONNECT = false;
                                        Logs.System("Device is alive and kicking!", LOGPATH, ADDLOG);
                                        break;
                                    }
                                case "M": // multi
                                    {
                                        SetCoinSlot("M");
                                        break;
                                    }
                                case "D": // dual
                                    {
                                        SetCoinSlot("D");
                                        break;
                                    }
                                case "S": // single
                                    {
                                        SetCoinSlot("S");
                                        break;
                                    }
                                case "L": // Legacy
                                    {
                                        SetCoinSlot("L");
                                        break;
                                    }
                                case "VERSION":
                                    {
                                        DEVICE_VERSION = serialPort.ReadTo(serialPort.NewLine);
                                        break;
                                    }
                            }
                        }
                    }
                    else if (serialData == "READY")
                    {
                        DEVICE_READY = true;
                        serialPort.Write("CS=" + Config.COINSLOT);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.System("SERIAL : " + ex.Message, LOGPATH, ADDLOG);
            }
        }
        private void SetCoinSlot(string coinslot)
        {
            serialPort.Write("C1=" + Config.COIN1);

            if (coinslot == "D")
            {
                serialPort.Write("C2=" + Config.COIN2);
            }
            else if (coinslot == "M")
            {
                serialPort.Write("C2=" + Config.COIN2);
                serialPort.Write("C3=" + Config.COIN3);
                serialPort.Write("P1=" + Config.PULSE1);
                serialPort.Write("P2=" + Config.PULSE2);
                serialPort.Write("P3=" + Config.PULSE3);
            }

            serialPort.Write("TI=" + Config.INTERVAL);
            if (Config.SOUND == 2) serialPort.Write("S=1");
            else serialPort.Write("S=" + Config.SOUND);
            if (Config.TIMER == 2) serialPort.Write("D=0");
            else serialPort.Write("D=" + Config.TIMER);

            Task.Run(async () =>
            {
               await Task.Delay(1000);
               serialPort.Write("VERSION?"); await Task.Delay(50);
               await mainPipe.WriteString("CV=" + DEVICE_VERSION);
               await DeviceUpdateChecker(DEVICE_VERSION);
            });
        }
        private void InsertCoin()
        {
            if (REMAINING_TIME != 0) REMAINING_TIME = 0; //Prevent when not using resume time then the user logout and try to use again.

            if (CONNECTED)
            {
                if (SYSTEM_LOCKED) mainPipe.WriteString(ARGS.UNLOCK);

                //If OS is windows 10. Show a notification toast display.

                if (Config.SOUND == 2) mainPipe.WriteString("PS=SMC");
                else mainPipe.WriteString("PS=COIN"); //Sounds.Play(Sounds.coin_drop); //Sa windows 10 lang ito gumagana.

                //if (Config.DISPLAY > 0)
                mainPipe.WriteString("AN=1");
            }

            if (serialPort.IsOpen) serialPort.Write("CT=1"); //Send a start counting command (of credits) to device.
        }
        private void SvcXmlParser()
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
            catch { }

            try
            {
                if (int.TryParse(doc.Root.Element("system").Element("SafeShutdown").Value , out int ret))
                {
                    SHUTDOWN_SAFE = ret;
                }
            }
            catch { }

            try
            {
                if (int.TryParse(doc.Root.Element("system").Element("ErrorShutdown").Value, out int ret))
                {
                    SHUTDOWN_ERROR = ret;
                }
            }
            catch { }

            try
            {
                if (int.TryParse(doc.Root.Element("system").Element("ReInitialize").Value, out int ret))
                {
                    REINITIALIZE = ret;
                }
            }
            catch { }

            try
            {
                if (int.TryParse(doc.Root.Element("system").Element("UpdateInterval").Value, out int ret))
                {
                    UPDATEINTERVAL = ret;
                }
            }
            catch { }

        }
        private async Task SoftwareUpdateChecker()
        {
            XDocument doc = await GetXDocument();

            if (doc == null )
            {
                Logs.System("Server not available at this time.");
                return;
            }

            string upVer = string.Empty;
            string upLnk = string.Empty;
            string upSizeString = string.Empty;
            string upInfo = string.Empty;
  
            try { upVer = doc.Root.Element("updater").Element("ver").Value; }
            catch (Exception ex) { Logs.System("UPDATE ver!" + ex.Message, LOGPATH, ADDLOG); }

            try { upLnk = doc.Root.Element("updater").Element("lnk").Value; }
            catch (Exception ex) { Logs.System("UPDATE lnk!" + ex.Message, LOGPATH, ADDLOG); }

            try { upSizeString = doc.Root.Element("updater").Element("size").Value; }
            catch (Exception ex) { Logs.System("UPDATE size!" + ex.Message, LOGPATH, ADDLOG); }

            try { upInfo = doc.Root.Element("updater").Element("info").Value; }
            catch (Exception ex) { Logs.System("UPDATE info!" + ex.Message, LOGPATH, ADDLOG); }

            //coinup.exe
            string upFilePath = Application.StartupPath + @"\coinup.exe";
            string upVersionString = string.Empty;
            Version upCurrent;
            Version upOnline;
            //int mode = 0; //0 = direct download . 1 = corrupted fle. 2 = move file to old folder.
           
            int mode = 0; //0 = latest version . 1 = direct download. 2 = corrupted file (delete and download) 3 = old version (move to old folder then download lated).

            if (!File.Exists(upFilePath))
            {
                mode = 1;
                goto download;
            }

            try { upVersionString = FileVersionInfo.GetVersionInfo(upFilePath).ProductVersion; }
            catch { mode = 2; }
            if (!Version.TryParse(upVersionString, out upCurrent)) mode = 2;

            if (mode == 2) goto download; //Skip and go to download

            if (Version.TryParse(upVer, out upOnline))
            {
                int versionResult = upOnline.CompareTo(upCurrent);
                if (versionResult > 0)
                {
                    mode = 3;
                    Logs.System("There are new coinapp updater version.", LOGPATH, ADDLOG);
                    goto download;
                }
            }

            #region CHECK SOFTWARE UPDATE.
            if (doc.Root.Element("main").Elements().Count() == 0) return;

            string newVersion = string.Empty;

            try { newVersion = doc.Root.Element("main").Element("ver").Value; }
            catch (Exception ex) { Logs.System("NEW VERSION! " + ex.Message, LOGPATH, ADDLOG); return; }

            if (Version.TryParse(newVersion, out Version onlineVersion))
            {
                int versionResult = onlineVersion.CompareTo(Version.Parse(Application.ProductVersion));
                if (versionResult > 0)
                {
                    Logs.System("There are new software version available.", LOGPATH, ADDLOG);
                    if (CONNECTED) await mainPipe.WriteString("NV=" + newVersion);
                }
            }

            #endregion CHECK SOFTWARE UPDATE.


            return;

        download:

            if (mode == 2)
            {
                if (File.Exists(upFilePath)) File.Delete(upFilePath);
            }
            else if (mode == 3)
            {
                if (File.Exists(upFilePath))
                {
                    try
                    {
                        Process[] proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(upLnk));
                        while (proc.Length > 0 && proc != null)
                        {
                            foreach (Process p in proc)
                            { p.Kill(); }
                            proc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(upLnk));
                        }
                    }

                    catch (Exception ex) { Logs.System(ex.Message, LOGPATH, ADDLOG); return;  }
                    await Task.Delay(500);
                    try
                    {
                        File.Move(upFilePath, Application.StartupPath + @"\old\" + Path.GetFileName(upFilePath));
                    }
                    catch (Exception ex) { Logs.System(ex.Message, LOGPATH, ADDLOG); return; }
                }
            }

            Logs.System("Downloading coinup.exe...", LOGPATH, ADDLOG);
            bool err = false;
            try
            {
                WebClientEx webClient = new WebClientEx();
                webClient.Timeout = 5000;
                webClient.DownloadFileCompleted += (s, e) => {
                    webClient.Dispose();
                    if (err) Logs.System("Downloading coinapp updater failed.", LOGPATH, ADDLOG);
                    else
                    {
                        Logs.System("Verifying coinapp updater...", LOGPATH, ADDLOG);

                        long newSize = new FileInfo(Application.StartupPath + @"\" + Path.GetFileName(upLnk)).Length;
                        
                        if (!long.TryParse(upSizeString, out long upSize) || newSize != upSize)
                        {
                            Logs.System("Coinapp updater verification failed.", LOGPATH, ADDLOG);
                        }
                        else
                        {
                            Logs.System("Coinapp updated download completed.", LOGPATH, ADDLOG);
                        }
                    }
                };
                await webClient.DownloadFileTaskAsync(new Uri(upLnk), Application.StartupPath + @"\" + Path.GetFileName(upLnk)); //webClient.DownloadData(consoleUrl);                   
            }
            catch (Exception ex) { err = true ; Logs.System("DOWNLOAD FAILED! " + ex.Message, LOGPATH, ADDLOG); return; }

        }
        private async Task DeviceUpdateChecker(string deviceVersion)
        {
            if (!Version.TryParse(deviceVersion, out Version currentVersion)) return;
            Version onlineVersion;

            XDocument doc = await GetXDocument();
            if (doc == null)
            {
                Logs.System("Server not available at this time.");
                return;
            }

            string devVerString = string.Empty;

            try { devVerString = doc.Root.Element("device").Element("ver").Value; }
            catch (Exception ex) { Logs.System("DEV VERSION ver!" + ex.Message, LOGPATH, ADDLOG); }

            if (Version.TryParse(devVerString, out onlineVersion))
            {
                int versionResult = onlineVersion.CompareTo(currentVersion);
                if (versionResult > 0)
                {
                    Logs.System("There are new device version update.", LOGPATH, ADDLOG);
                    if (CONNECTED) await mainPipe.WriteString("DV=" + devVerString);
                }
            }

        }
        private async Task<XDocument> GetXDocument()
        {
            const string upUrl = "https://dl.dropboxusercontent.com/s/u42shdtpsf4kfvl/up.xml";
            string upXml = string.Empty;
            
            try
            {
                var webClient = new WebClientEx();
                webClient.Timeout = 5000;
                upXml = await webClient.DownloadStringTaskAsync(upUrl); //webClient.DownloadData(consoleUrl);                   
                webClient.Dispose();
            }
            catch (Exception ex) { Logs.System("DOWNL XML : " + ex.Message, LOGPATH, ADDLOG); return null; }

            XDocument doc = new XDocument();
            string rootLocalName = string.Empty;
            string rootAttribute = string.Empty;
            try
            {
                doc = XDocument.Parse(upXml);
                rootLocalName = doc.Root.Name.LocalName;
                rootAttribute = doc.Root.Attribute("name").Value;
            }
            catch (Exception ex) { Logs.System("PARSE XML : " + ex.Message, LOGPATH, ADDLOG); return null; }

            if (rootLocalName != "update")
            {
                Logs.System("XML Error! Invalid root.");
                return null; 
            }

            if (rootAttribute != "coinapp") 
            {
                Logs.System("XML Error! Invalid attribute.");
                return null; 
            }

            if (doc.Root.Elements().Count() == 0) 
            {
                Logs.System("XML Error! No child element.");
                return null; 
            }

            return doc;
        }
        #endregion Custom methods
    }
}