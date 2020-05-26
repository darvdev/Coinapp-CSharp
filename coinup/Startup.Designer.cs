namespace coinup
{
    partial class Startup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Startup));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_copyright = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.picturebox_device = new System.Windows.Forms.PictureBox();
            this.picturebox_software = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_device)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_software)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label_copyright, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(360, 178);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label_copyright
            // 
            this.label_copyright.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label_copyright.AutoSize = true;
            this.label_copyright.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_copyright.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_copyright.Location = new System.Drawing.Point(50, 148);
            this.label_copyright.Name = "label_copyright";
            this.label_copyright.Size = new System.Drawing.Size(259, 26);
            this.label_copyright.TabIndex = 0;
            this.label_copyright.Text = "Coinapp © 2018-2020\r\nhttps://www.facebook.com/CoinappTechnology";
            this.label_copyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_copyright.MouseClick += new System.Windows.Forms.MouseEventHandler(this.label_copyright_MouseClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.picturebox_device, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.picturebox_software, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 76.05634F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23.94366F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(354, 142);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // picturebox_device
            // 
            this.picturebox_device.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picturebox_device.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picturebox_device.Image = ((System.Drawing.Image)(resources.GetObject("picturebox_device.Image")));
            this.picturebox_device.Location = new System.Drawing.Point(207, 10);
            this.picturebox_device.Margin = new System.Windows.Forms.Padding(30, 10, 50, 3);
            this.picturebox_device.Name = "picturebox_device";
            this.picturebox_device.Size = new System.Drawing.Size(97, 95);
            this.picturebox_device.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picturebox_device.TabIndex = 1;
            this.picturebox_device.TabStop = false;
            this.picturebox_device.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picturebox_device_MouseClick);
            // 
            // picturebox_software
            // 
            this.picturebox_software.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picturebox_software.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picturebox_software.Image = ((System.Drawing.Image)(resources.GetObject("picturebox_software.Image")));
            this.picturebox_software.Location = new System.Drawing.Point(50, 10);
            this.picturebox_software.Margin = new System.Windows.Forms.Padding(50, 10, 30, 3);
            this.picturebox_software.Name = "picturebox_software";
            this.picturebox_software.Size = new System.Drawing.Size(97, 95);
            this.picturebox_software.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picturebox_software.TabIndex = 0;
            this.picturebox_software.TabStop = false;
            this.picturebox_software.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picturebox_software_MouseClick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 113);
            this.label2.Margin = new System.Windows.Forms.Padding(30, 5, 15, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "COINAPP SOFTWARE";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(192, 113);
            this.label3.Margin = new System.Windows.Forms.Padding(15, 5, 30, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(132, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "COINAPP DEVICE";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Startup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 178);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(380, 220);
            this.MinimumSize = new System.Drawing.Size(380, 220);
            this.Name = "Startup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_device)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox_software)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label_copyright;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox picturebox_device;
        private System.Windows.Forms.PictureBox picturebox_software;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}