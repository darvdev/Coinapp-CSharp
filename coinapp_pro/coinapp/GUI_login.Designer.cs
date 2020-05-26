using coinapplib;

namespace coinapp
{
    partial class GUI_login
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI_login));
            this.textbox_login = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_message = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button_enter = new System.Windows.Forms.Button();
            this.button_switch = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textbox_login
            // 
            this.textbox_login.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textbox_login.Enabled = false;
            this.textbox_login.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textbox_login.Location = new System.Drawing.Point(13, 13);
            this.textbox_login.Margin = new System.Windows.Forms.Padding(13, 13, 13, 5);
            this.textbox_login.Name = "textbox_login";
            this.textbox_login.Size = new System.Drawing.Size(234, 23);
            this.textbox_login.TabIndex = 0;
            this.textbox_login.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textbox_login.UseSystemPasswordChar = true;
            this.textbox_login.TextChanged += new System.EventHandler(this.textbox_login_TextChanged);
            this.textbox_login.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textbox_login_KeyDown);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label_message, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.textbox_login, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(260, 93);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label_message
            // 
            this.label_message.AutoSize = true;
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_message.Location = new System.Drawing.Point(5, 48);
            this.label_message.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(250, 1);
            this.label_message.TabIndex = 5;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.Controls.Add(this.button_enter, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_switch, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 46);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.Padding = new System.Windows.Forms.Padding(5, 0, 5, 5);
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(254, 44);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // button_enter
            // 
            this.button_enter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_enter.Enabled = false;
            this.button_enter.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_enter.Location = new System.Drawing.Point(8, 3);
            this.button_enter.Name = "button_enter";
            this.button_enter.Size = new System.Drawing.Size(164, 33);
            this.button_enter.TabIndex = 3;
            this.button_enter.TabStop = false;
            this.button_enter.Text = "LOGIN";
            this.button_enter.UseVisualStyleBackColor = true;
            this.button_enter.Click += new System.EventHandler(this.button_enter_Click);
            // 
            // button_switch
            // 
            this.button_switch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button_switch.Enabled = false;
            this.button_switch.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_switch.Location = new System.Drawing.Point(178, 3);
            this.button_switch.Name = "button_switch";
            this.button_switch.Size = new System.Drawing.Size(68, 33);
            this.button_switch.TabIndex = 4;
            this.button_switch.TabStop = false;
            this.button_switch.Text = "SWITCH";
            this.button_switch.UseVisualStyleBackColor = true;
            this.button_switch.Click += new System.EventHandler(this.button_switch_Click);
            // 
            // GUI_login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 93);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(280, 200);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(280, 135);
            this.Name = "GUI_login";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.GUI_login_Deactivate);
            this.Load += new System.EventHandler(this.GUI_login_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textbox_login;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button button_enter;
        private System.Windows.Forms.Button button_switch;
        private System.Windows.Forms.Label label_message;
    }
}