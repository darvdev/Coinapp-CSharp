using System;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;

namespace coinappsvc
{
    public class Program
    {
        public static bool INSTALLER_SILENT = false;
        public const string serviceName = "Coinapp";
        public static ServiceController[] serviceControllers = ServiceController.GetServices();
        public static string exeLocation = Assembly.GetExecutingAssembly().Location;
        
        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    bool isExist = serviceControllers.Any(x => x.ServiceName == serviceName);

                    switch (args[0])
                    {
                        case "/i":
                            {
                                if (isExist)
                                {
                                    MessageBox.Show("Coinapp service is already installed.", "Coinapp Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    if (args.Length > 1)
                                    {
                                        if (args[1] == "-s")
                                            INSTALLER_SILENT = true;
                                    }

                                    try
                                    {
                                        ManagedInstallerClass.InstallHelper(new string[] { exeLocation });
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.Message, "Coinapp Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                            break;

                        case "/u":
                            {
                                if (isExist == false)
                                {
                                    MessageBox.Show("Coinapp service is not installed.", "Coinapp Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    using (ServiceController sc = new ServiceController(serviceName))
                                    {
                                        if (sc.Status == ServiceControllerStatus.Running)
                                        {
                                            sc.Stop();
                                            sc.WaitForStatus(ServiceControllerStatus.Stopped);
                                        }

                                        try
                                        {
                                            if (args.Length > 1)
                                            {
                                                if (args[1] == "-s")
                                                    INSTALLER_SILENT = true;
                                            }
                                            
                                            ManagedInstallerClass.InstallHelper(new string[] { "/u", exeLocation });
                                        }
                                        catch (Exception e)
                                        {
                                            MessageBox.Show(e.Message, "Coinapp Service", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }

                                    }
                                }
                            }
                            break;
                    }
                }
                return;
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new coinappsvc()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }

    public class Win32
    {
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

        public const int SERVICE_CONTROL_STOP = 1;
        public const int SERVICE_CONTROL_DEVICEEVENT = 11;
        public const int SERVICE_CONTROL_SHUTDOWN = 5;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;

        //public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEQUERYREMOVE = 0x8001;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        //public const int WM_DEVICECHANGE = 0x219;

        public delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr IntPtr, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            public byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] dbcc_name;
        }
    }
}
