namespace coinup
{
    partial class Software
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Software));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.downloadProgress = new System.Windows.Forms.ProgressBar();
            this.label_download = new System.Windows.Forms.Label();
            this.label_message = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.downloadProgress, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label_download, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label_message, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 123);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // downloadProgress
            // 
            this.downloadProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadProgress.Location = new System.Drawing.Point(20, 66);
            this.downloadProgress.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.downloadProgress.Name = "downloadProgress";
            this.downloadProgress.Size = new System.Drawing.Size(410, 24);
            this.downloadProgress.TabIndex = 0;
            this.downloadProgress.Visible = false;
            // 
            // label_download
            // 
            this.label_download.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_download.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_download.Location = new System.Drawing.Point(30, 96);
            this.label_download.Margin = new System.Windows.Forms.Padding(30, 3, 30, 8);
            this.label_download.Name = "label_download";
            this.label_download.Size = new System.Drawing.Size(390, 19);
            this.label_download.TabIndex = 1;
            this.label_download.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_message.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.label_message.Location = new System.Drawing.Point(10, 10);
            this.label_message.Margin = new System.Windows.Forms.Padding(10);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(430, 43);
            this.label_message.TabIndex = 2;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // Software
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(450, 123);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(470, 165);
            this.MinimumSize = new System.Drawing.Size(470, 165);
            this.Name = "Software";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Main_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ProgressBar downloadProgress;
        private System.Windows.Forms.Label label_download;
        private System.Windows.Forms.Label label_message;
    }
}

