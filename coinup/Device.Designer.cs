namespace coinup
{
    partial class Device
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Device));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.combobox_version = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.combobox_device = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.combobox_port = new System.Windows.Forms.ComboBox();
            this.statusstrip_status = new System.Windows.Forms.StatusStrip();
            this.status_label = new System.Windows.Forms.ToolStripStatusLabel();
            this.status_progress = new System.Windows.Forms.ToolStripProgressBar();
            this.richtextbox_message = new System.Windows.Forms.RichTextBox();
            this.button_flash = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.statusstrip_status.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.statusstrip_status, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.richtextbox_message, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.button_flash, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(434, 312);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.combobox_version, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.combobox_device, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.combobox_port, 2, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.40541F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 44.59459F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(428, 74);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(30, 0, 10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "FIRMWARE VERSION";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // combobox_version
            // 
            this.combobox_version.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combobox_version.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_version.Enabled = false;
            this.combobox_version.FormattingEnabled = true;
            this.combobox_version.Location = new System.Drawing.Point(30, 44);
            this.combobox_version.Margin = new System.Windows.Forms.Padding(30, 3, 10, 3);
            this.combobox_version.Name = "combobox_version";
            this.combobox_version.Size = new System.Drawing.Size(109, 21);
            this.combobox_version.TabIndex = 1;
            this.combobox_version.TabStop = false;
            this.combobox_version.SelectedIndexChanged += new System.EventHandler(this.combobox_version_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(10, 0, 10, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "DEVICE TYPE";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // combobox_device
            // 
            this.combobox_device.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combobox_device.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_device.Enabled = false;
            this.combobox_device.FormattingEnabled = true;
            this.combobox_device.Items.AddRange(new object[] {
            "Arduino Uno",
            "Arduino Nano"});
            this.combobox_device.Location = new System.Drawing.Point(159, 44);
            this.combobox_device.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.combobox_device.Name = "combobox_device";
            this.combobox_device.Size = new System.Drawing.Size(108, 21);
            this.combobox_device.TabIndex = 3;
            this.combobox_device.TabStop = false;
            this.combobox_device.SelectedIndexChanged += new System.EventHandler(this.combobox_device_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(287, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(10, 0, 30, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "SELECT COM PORT";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // combobox_port
            // 
            this.combobox_port.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.combobox_port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.combobox_port.Enabled = false;
            this.combobox_port.FormattingEnabled = true;
            this.combobox_port.Location = new System.Drawing.Point(287, 44);
            this.combobox_port.Margin = new System.Windows.Forms.Padding(10, 3, 30, 3);
            this.combobox_port.Name = "combobox_port";
            this.combobox_port.Size = new System.Drawing.Size(111, 21);
            this.combobox_port.TabIndex = 5;
            this.combobox_port.TabStop = false;
            this.combobox_port.SelectedIndexChanged += new System.EventHandler(this.combobox_port_SelectedIndexChanged);
            // 
            // statusstrip_status
            // 
            this.statusstrip_status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusstrip_status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status_label,
            this.status_progress});
            this.statusstrip_status.Location = new System.Drawing.Point(0, 290);
            this.statusstrip_status.Name = "statusstrip_status";
            this.statusstrip_status.Size = new System.Drawing.Size(434, 22);
            this.statusstrip_status.TabIndex = 5;
            this.statusstrip_status.Text = "statusStrip1";
            // 
            // status_label
            // 
            this.status_label.Margin = new System.Windows.Forms.Padding(10, 0, 0, 3);
            this.status_label.Name = "status_label";
            this.status_label.Size = new System.Drawing.Size(112, 19);
            this.status_label.Text = "toolStripStatusLabel";
            this.status_label.Visible = false;
            // 
            // status_progress
            // 
            this.status_progress.Margin = new System.Windows.Forms.Padding(1, 0, 1, 3);
            this.status_progress.Name = "status_progress";
            this.status_progress.Size = new System.Drawing.Size(100, 19);
            this.status_progress.Visible = false;
            // 
            // richtextbox_message
            // 
            this.richtextbox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richtextbox_message.Location = new System.Drawing.Point(10, 143);
            this.richtextbox_message.Margin = new System.Windows.Forms.Padding(10, 3, 10, 10);
            this.richtextbox_message.Name = "richtextbox_message";
            this.richtextbox_message.ReadOnly = true;
            this.richtextbox_message.Size = new System.Drawing.Size(414, 137);
            this.richtextbox_message.TabIndex = 6;
            this.richtextbox_message.Text = "";
            this.richtextbox_message.WordWrap = false;
            // 
            // button_flash
            // 
            this.button_flash.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.button_flash.Enabled = false;
            this.button_flash.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_flash.Location = new System.Drawing.Point(80, 88);
            this.button_flash.Margin = new System.Windows.Forms.Padding(80, 0, 80, 3);
            this.button_flash.Name = "button_flash";
            this.button_flash.Size = new System.Drawing.Size(274, 40);
            this.button_flash.TabIndex = 7;
            this.button_flash.TabStop = false;
            this.button_flash.Text = "FLASH FIRMWARE";
            this.button_flash.UseVisualStyleBackColor = true;
            this.button_flash.Click += new System.EventHandler(this.button_flash_Click);
            // 
            // Device
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 312);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(450, 350);
            this.Name = "Device";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Device_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.statusstrip_status.ResumeLayout(false);
            this.statusstrip_status.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.StatusStrip statusstrip_status;
        private System.Windows.Forms.RichTextBox richtextbox_message;
        private System.Windows.Forms.Button button_flash;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox combobox_version;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox combobox_device;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox combobox_port;
        private System.Windows.Forms.ToolStripStatusLabel status_label;
        private System.Windows.Forms.ToolStripProgressBar status_progress;
    }
}