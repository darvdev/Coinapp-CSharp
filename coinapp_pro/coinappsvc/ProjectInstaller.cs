using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using coinapplib;

namespace coinappsvc
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.AfterInstall += new InstallEventHandler(coinappsvc_AfterInstall);
            this.AfterUninstall += new InstallEventHandler(coinappsvc_AfterUninstall);
        }

        private void coinappsvc_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                My.SetRecoveryOptions(Program.serviceName);
            }
            catch (InvalidOperationException ex)
            {
                My.WriteEventLog("Setting service recovery failed. (" + ex.Message + ")");
            }

            if (!Program.INSTALLER_SILENT)
                MessageBox.Show("Coinapp service installed successfully.", "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void coinappsvc_AfterUninstall(object sender, InstallEventArgs e)
        {
            if (!Program.INSTALLER_SILENT)
                MessageBox.Show("Coinapp service uninstalled successfully.", "Coinapp", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

    }
}
