using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace coinapplib
{
    public class SESSION
    {
        public const int INSERT_COIN = 1;
        public const int TIME_IN = 2;
        public const int TIME_OUT = 3;
        public const int TIME_PAUSE = 4;
        public const int TIME_SAVED = 5;
        public const int TIME_RESUME = 6;
        public const int USER_LOGIN = 7;
        public const int USER_LOGOUT = 8;
        public const int ADMIN_LOGIN = 9;
        public const int ADMIN_LOGOUT = 10;
        public const int SHUTTING_DOWN = 11;

        private string _INSERT_COIN { get; set; } = "INSERT_COIN";
        private string _TIME_IN { get; set; } = "TIME_IN";
        private string _TIME_OUT { get; set; } = "TIME_OUT";
        private string _TIME_PAUSE { get; set; } = "TIME_PAUSE";
        private string _TIME_SAVE { get; set; } = "TIME_SAVED";
        private string _TIME_RESUME { get; set; } = "TIME_RESUME";
        private string _USER_LOGIN { get; set; } = "USER_LOGIN";
        private string _USER_LOGOUT { get; set; } = "USER_LOGOUT";
        private string _ADMIN_LOGIN { get; set; } = "ADMIN_LOGIN";
        private string _ADMIN_LOGOUT { get; set; } = "ADMIN_LOGOUT";
        private string _SHUTTING_DOWN { get; set; } = "SHUTTING_DOWN";

        private string _INVALID_SESSION = "ERROR_SESSION";

        public string ToString(int session)
        {
            switch (session)
            {
                case 1: return _INSERT_COIN;
                case 2: return _TIME_IN;
                case 3: return _TIME_OUT;
                case 4: return _TIME_PAUSE;
                case 5: return _TIME_SAVE;
                case 6: return _TIME_RESUME;
                case 7: return _USER_LOGIN;
                case 8: return _USER_LOGOUT;
                case 9: return _ADMIN_LOGIN;
                case 10: return _ADMIN_LOGOUT;
                case 11: return _SHUTTING_DOWN;
                default: return _INVALID_SESSION;
            }
        }
        public SESSION(string XmlPath)
        {
            if (File.Exists(XmlPath))
            {
                //string xml = File.ReadAllText(XmlPath);
                XDocument doc;
                string rootLocalName;
                string rootAttribute;

                try
                {
                    doc = XDocument.Load(XmlPath);
                    rootLocalName = doc.Root.Name.LocalName;
                    rootAttribute = doc.Root.Attribute("name").Value;
                }
                catch { return; }

                if (rootLocalName != "configuration") { return; }
                if (rootAttribute != "coinapp") { return; }

                if (doc.Root.Elements("sessions").Count() == 0) { return; }
                if (doc.Root.Element("sessions").Elements().Count() == 0) { return; }

                try { _INSERT_COIN = doc.Root.Element("sessions").Element("InsertCoin").Value; } catch { }
                try { _TIME_IN = doc.Root.Element("sessions").Element("TimeIn").Value; } catch { }
                try { _TIME_OUT = doc.Root.Element("sessions").Element("TimeOut").Value; } catch { }
                try { _TIME_PAUSE = doc.Root.Element("sessions").Element("TimePause").Value; } catch { }
                try { _TIME_SAVE = doc.Root.Element("sessions").Element("TimeSave").Value; } catch { }
                try { _TIME_RESUME = doc.Root.Element("sessions").Element("TimeResume").Value; } catch { }
                try { _USER_LOGIN = doc.Root.Element("sessions").Element("UserLogin").Value; } catch { }
                try { _USER_LOGOUT = doc.Root.Element("sessions").Element("UserLogout").Value; } catch { }
                try { _ADMIN_LOGIN = doc.Root.Element("sessions").Element("AdminLogin").Value; } catch { }
                try { _ADMIN_LOGOUT = doc.Root.Element("sessions").Element("AdminLogout").Value; } catch { }
                try { _SHUTTING_DOWN = doc.Root.Element("sessions").Element("Shutdown").Value; } catch { }
            }
        }

    }
}
