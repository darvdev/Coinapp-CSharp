using System;
using System.Windows.Forms;

namespace coinup
{
    public static class Program
    {
        public static Startup startup;
        public static bool SILENT = false;
        public static bool VERBOSE = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "/app":
                        {
                            if (args.Length > 1) if (args[1].ToLower() == "-s") SILENT = true;
                            Application.Run(new Software());
                            break; 
                        }
                        
                    case "/dev":
                        {
                            if (args.Length > 1) if (args[1].ToLower() == "-v") VERBOSE = true;
                            Application.Run(new Device());
                            break;
                        }

                    default:
                        {

                            startup = new Startup();
                            Application.Run(startup);
                            break;
                        }
                }
            }
            else
            {
                startup = new Startup();
                Application.Run(startup);
            }
        }
    }
}
