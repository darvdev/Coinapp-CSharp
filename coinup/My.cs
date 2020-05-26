using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace coinup
{
    class My
    {
        public class WebClientEx : WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                request.Timeout = Timeout;
                return request;
            }
        }

        public static void Log(string logMessage)
        {
            string logName = Application.StartupPath + @"\coinup.log"; //Create a log file name by the day.

            try
            {
                using (var writer = new StreamWriter(logName, true))
                {
                    writer.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd") + "][" + DateTime.Now.ToString("hh:mm:ss.fff tt") + "] " + logMessage);
                    writer.Close();
                }
            }
            catch
            {
                throw new IOException();
            }
        }

        public static async Task<XDocument> GetXDocument()
        {
            const string upUrl = "https://dl.dropboxusercontent.com/s/u42shdtpsf4kfvl/up.xml";
            string upXml = string.Empty;

            #region Get XML string
            try
            {
                My.WebClientEx webClient = new My.WebClientEx();
                webClient.Timeout = 5000;
                upXml = await webClient.DownloadStringTaskAsync(upUrl);
                webClient.Dispose();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }
            #endregion  Get XML string

            #region Parse XML
            XDocument doc = new XDocument();
            string rootLocalName = string.Empty;
            string rootAttribute = string.Empty;

            try
            {
                doc = XDocument.Parse(upXml);
                rootLocalName = doc.Root.Name.LocalName;
                rootAttribute = doc.Root.Attribute("name").Value;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return null;
            }

            if (rootLocalName != "update")
            {
                Log("XML Error! Invalid root.");
                return null;
            }
            if (rootAttribute != "coinapp")
            {
                Log("XML Error! Invalid attribute.");
                return null;
            }

            if (doc.Root.Elements().Count() == 0)
            {
                Log("XML Error! No child elements.");
                return null;
            }

            return doc;
            #endregion Parse XML

        }


        public static async Task<string> GetXml()
        {
            const string upUrl = "https://dl.dropboxusercontent.com/s/u42shdtpsf4kfvl/up.xml";
            string upXml = string.Empty;

            #region Get XML string
            try
            {
                My.WebClientEx webClient = new My.WebClientEx();
                webClient.Timeout = 5000;
                upXml = await webClient.DownloadStringTaskAsync(upUrl);
                webClient.Dispose();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return string.Empty;
            }

            return upXml;
            #endregion  Get XML string
        }
        public static bool FormExists(string formName)
        {
            return Application.OpenForms.Cast<Form>().Any(form => form.Name == formName);
        }
    }
}
