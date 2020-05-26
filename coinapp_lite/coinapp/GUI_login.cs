using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using coinapplib;

namespace coinapp
{
    public partial class GUI_login : Form
    {
        public IntPtr iHandle;
        private bool working = false;
        const int WM_CONTEXTMENU = 0x007B;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CONTEXTMENU)
                m.Result = IntPtr.Zero;
            else
                base.WndProc(ref m);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOPMOST = 0x8;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOPMOST;
                return cp;
            }
        }
        public GUI_login()
        {
            InitializeComponent();
        }
        public GUI_login(IntPtr hWnd)
        {
            InitializeComponent();
            iHandle = hWnd;
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (working == true)
                {
                    label_message.ForeColor = Color.OrangeRed;
                    label_message.Text = "Background operation is in process. Please wait a moment...";
                    this.Height = 180;
                    e.Cancel = true;
                    return;
                }
            }
            this.Dispose();
            base.OnFormClosing(e);
        }
        
        private void button_enter_Click(object sender, EventArgs e)
        {
            BeginEnter();
        }

        private void button_switch_Click(object sender, EventArgs e)
        {
            if (button_enter.Text == "LOGIN")
            {
                this.Text = "Enter recovery email";
                button_enter.Text = "RECOVER";
                textbox_login.UseSystemPasswordChar = false;
            }
            else
            {
                this.Text = "Enter password";
                textbox_login.UseSystemPasswordChar = true;
                button_enter.Text = "LOGIN";
            }

            textbox_login.Text = string.Empty;
            label_message.Text = string.Empty;
            button_enter.Enabled = false;
            if (this.Height > 135)
                this.Height = 135;
            this.ActiveControl = textbox_login;
        }

        private void textbox_login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && button_enter.Enabled == true)
            {
                BeginEnter();
            }           
            else
            {
                if (this.Height > 135)
                    this.Height = 135;
            }
        }

        private void BeginEnter()
        {
            if (button_enter.Text == "RECOVER")
            {
                if (textbox_login.Text.ToLower() == Config.EMAIL.ToLower())
                {
                    textbox_login.Enabled = false;
                    button_switch.Enabled = false;
                    label_message.Text = "Sending password...";
                    label_message.ForeColor = Color.Blue;
                    this.Height = 160;

                    Task.Run( async() =>
                    {
                        working = true;
                        await c.EmailData();
                        if (c.RecoverPassword(Config.EMAIL, Config.PASS, out string message))
                            label_message.ForeColor = Color.Green;
                        else
                            label_message.ForeColor = Color.Red;
                        label_message.Text = message;
                        this.Height = 200;
                        textbox_login.Enabled = true;
                        button_switch.Enabled = true;
                        working = false;
                    });
                }
                else
                {
                    label_message.Text = "Please type the registered recovery email address";
                    label_message.ForeColor = Color.Red;
                    this.Height = 180;
                }
            }
            else
            {
                if (textbox_login.Text == Config.PASS)
                {
                    textbox_login.Enabled = false;
                    button_switch.Enabled = false;
                    button_enter.Enabled = false;
                    label_message.Text = "Please wait a moment...";
                    label_message.ForeColor = Color.Green;
                    this.Height = 160;
                    c.ATTEMPT = 0;
                    
                    if (!c.LOCKED)
                    {
                        Close();
                        c.app.ShowConsoleWindow();
                    }
                    else
                    {
                        working = true;
                        Hide();

                        if (c.FormExists("GUI_dialog"))
                            c.app.dialog.Close();

                        c.app.dialog = new GUI_dialog();
                        DialogResult result = c.app.dialog.ShowDialog("Unlock the dekstop?\n\nclick NO to open Coinapp console window here.", "Unlock desktop", MessageBoxButtons.YesNoCancel, Images.initialize);

                        if (result == DialogResult.Yes)
                        {
                            working = false;
                            c.app.UnlockDesktop(true);
                        }
                        else if (DialogResult.No == result)
                        {
                            c.app.ShowConsoleWindow();
                        }
                        working = false;
                        Close();
                    }
                }
                else
                {
                    label_message.Text = "Incorrent password";
                    label_message.ForeColor = Color.Red;
                    this.Height = 160;
                    
                    c.ATTEMPT++;

                    if (c.ATTEMPT >= Config.LOGINATTEMPT)
                    {
                        label_message.Text = "Shutting down...";
                        if (c.CONNECTED) c.mainPipe.WriteString(ARGS.SHUTDOWN);
                        else Shutdown.Force();
                    }
                }
            }

            button_enter.Enabled = false;
            textbox_login.Text = string.Empty;
        }

        private void GUI_login_Deactivate(object sender, EventArgs e)
        {
            if (working == false)
                this.Close();
        }

        private void textbox_login_TextChanged(object sender, EventArgs e)
        {
            if (button_enter.Text == "RECOVER")
            {
                if (c.IsValidEmail(textbox_login.Text))
                    button_enter.Enabled = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textbox_login.Text) == false)
                    button_enter.Enabled = true;
                else
                    button_enter.Enabled = false;
            }
        }

        private void GUI_login_Load(object sender, EventArgs e)
        {
            if (c.FormExists("GUI_dialog"))
                c.app.dialog.Close();

            this.Text = "Enter password";
            this.Height = 135;
            textbox_login.Enabled = true;
            button_switch.Enabled = true;
            this.Activate();
        }
    }
}
