﻿namespace coinappsvc
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.coinappsvcProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.coinappsvcInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // coinappsvcProcessInstaller
            // 
            this.coinappsvcProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.coinappsvcProcessInstaller.Password = null;
            this.coinappsvcProcessInstaller.Username = null;
            // 
            // coinappsvcInstaller
            // 
            this.coinappsvcInstaller.Description = "Windows kiosk system";
            this.coinappsvcInstaller.DisplayName = "Coinapp";
            this.coinappsvcInstaller.ServiceName = "Coinapp";
            this.coinappsvcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.coinappsvcProcessInstaller,
            this.coinappsvcInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller coinappsvcProcessInstaller;
        private System.ServiceProcess.ServiceInstaller coinappsvcInstaller;
    }
}