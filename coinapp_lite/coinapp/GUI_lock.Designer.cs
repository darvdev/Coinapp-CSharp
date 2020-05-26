using coinapplib;

namespace coinapp
{
    partial class GUI_lock
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI_lock));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.button_shutdown = new System.Windows.Forms.PictureBox();
            this.button_status = new System.Windows.Forms.PictureBox();
            this.label_messages = new System.Windows.Forms.Label();
            this.label_timer = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_shutdown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_status)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tableLayoutPanel1.Controls.Add(this.button_shutdown, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.button_status, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_messages, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_timer, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.ForeColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 74);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // button_shutdown
            // 
            this.button_shutdown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_shutdown.BackColor = System.Drawing.Color.Transparent;
            this.button_shutdown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_shutdown.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button_shutdown.Image = ((System.Drawing.Image)(resources.GetObject("button_shutdown.Image")));
            this.button_shutdown.Location = new System.Drawing.Point(10, 10);
            this.button_shutdown.Margin = new System.Windows.Forms.Padding(10);
            this.button_shutdown.Name = "button_shutdown";
            this.button_shutdown.Size = new System.Drawing.Size(55, 54);
            this.button_shutdown.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.button_shutdown.TabIndex = 0;
            this.button_shutdown.TabStop = false;
            this.button_shutdown.Click += new System.EventHandler(this.button_shutdown_Click);
            // 
            // button_status
            // 
            this.button_status.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.button_status.BackColor = System.Drawing.Color.Transparent;
            this.button_status.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.button_status.Cursor = System.Windows.Forms.Cursors.Default;
            this.button_status.Location = new System.Drawing.Point(85, 10);
            this.button_status.Margin = new System.Windows.Forms.Padding(10);
            this.button_status.Name = "button_status";
            this.button_status.Size = new System.Drawing.Size(55, 54);
            this.button_status.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.button_status.TabIndex = 1;
            this.button_status.TabStop = false;
            this.button_status.Click += new System.EventHandler(this.button_status_Click);
            // 
            // label_messages
            // 
            this.label_messages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label_messages.AutoSize = true;
            this.label_messages.Font = new System.Drawing.Font("Consolas", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_messages.ForeColor = System.Drawing.Color.White;
            this.label_messages.Location = new System.Drawing.Point(153, 15);
            this.label_messages.Name = "label_messages";
            this.label_messages.Size = new System.Drawing.Size(424, 43);
            this.label_messages.TabIndex = 2;
            this.label_messages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_timer
            // 
            this.label_timer.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label_timer.AutoSize = true;
            this.label_timer.BackColor = System.Drawing.Color.Transparent;
            this.label_timer.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_timer.ForeColor = System.Drawing.Color.White;
            this.label_timer.Location = new System.Drawing.Point(590, 0);
            this.label_timer.Name = "label_timer";
            this.label_timer.Size = new System.Drawing.Size(207, 74);
            this.label_timer.TabIndex = 0;
            this.label_timer.Text = "xx:xx";
            this.label_timer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // GUI_lock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(800, 80);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 80);
            this.Name = "GUI_lock";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Load += new System.EventHandler(this.GUI_lock_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.button_shutdown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.button_status)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox button_shutdown;
        public System.Windows.Forms.Label label_timer;
        public System.Windows.Forms.Label label_messages;
        public System.Windows.Forms.PictureBox button_status;
    }
}