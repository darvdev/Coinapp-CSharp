using System;
using System.IO;
using System.Windows.Forms;

namespace coinapplib
{
    public class Logs
    {
        //Write a log to .txt files
        public static void Write(string logMessage, string path = null, int add = 0)
        {
            if (add == 0) //Create a log if this value is true. Else don't
                return;

            string dir = string.Empty;

            if (path != null || Directory.Exists(path)) //If there is existing directory path assign in this method and use that directory.
            {
                dir = path.EndsWith(@"\") ? path : path + @"\"; //Store the existing directory.
            }
            else //Nothing found in the assigned directory. Use the default directory for creating log files.
            {
                //Create default directory for log files if none.
                if (!Directory.Exists(Application.StartupPath + @"\Logs\"))
                {
                    Directory.CreateDirectory(Application.StartupPath + @"\Logs\");
                }

                dir = Application.StartupPath + @"\Logs\";

                //throw new Exception("Directory for logs not exists. Used the default directory.");
            }

            string logName = DateTime.Now.ToString("yyyyMMdd") + ".log"; //Create a log file name by the day.

            try
            {
                using (var writer = new StreamWriter(dir + logName, true))
                {
                    writer.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] " + logMessage);
                    writer.Close();
                }
            }
            catch
            {
                //throw new IOException();
            }
        }
        
        public static void System(string logMessage, string path = null, bool add = false)
        {
            if (!add)
                return;
            
            try
            {
                string dir = string.Empty;

                if (path == null || !Directory.Exists(path))
                {
                    dir = Application.StartupPath + @"\coinapp_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                }
                else
                {
                    if (path.EndsWith(@"\"))
                    {
                        dir = path + "coinapp_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    }
                    else
                    {
                        dir = path + @"\coinapp_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    }
                }

                using (StreamWriter writer = new StreamWriter(dir, true))
                {
                    writer.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] " + logMessage);
                    writer.Close();
                }
            }
            catch
            {
                //throw new IOException();
            }
        }

        //public static void App(string logMessage, string path = null, bool add = false)
        //{
        //    if (!add)
        //        return;

        //    try
        //    {
        //        string dir = string.Empty;

        //        if (path == null || !Directory.Exists(path))
        //        {
        //            dir = Application.StartupPath + @"\coinapp.exe_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        //        }
        //        else
        //        {
        //            if (path.EndsWith(@"\"))
        //            {
        //                dir = path + "coinapp.exe_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        //            }
        //            else
        //            {
        //                dir = path + @"\coinapp.exe_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        //            }
        //        }

        //        using (StreamWriter writer = new StreamWriter(dir, true))
        //        {
        //            writer.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] " + logMessage);
        //            writer.Close();
        //        }
        //    }
        //    catch
        //    {
        //        throw new IOException();
        //    }
        //}

    }
}
