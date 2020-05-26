using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using coinapplib;

namespace coinapp
{
    public partial class GUI_dialog : Form
    {
        private bool TOPMOST = false;
        private ClientPipe clientPipe;
        private string clientPipeName = "coinapp_msg";
        private static DialogResult _result;
        private static DialogResult result { get { return _result; } }
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
                const int WS_EX_TOOLWINDOW = 0x80;
                const int CS_NOCLOSE = 0x200;
                const int WS_EX_TOPMOST = 0x8;
                CreateParams cp = base.CreateParams;
                if (TOPMOST)
                {
                    cp.ClassStyle |= CS_NOCLOSE;
                    cp.ExStyle |= WS_EX_TOPMOST;
                }
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }
        public GUI_dialog()
        {
            InitializeComponent();
        }

        public GUI_dialog(string message, Bitmap image = null, int width = 380, int height = 160, bool connect = false, bool button = true)
        {
            TOPMOST = true;
            InitializeComponent();
            label_message.Text = message;
            imageBox.Image = image;
            Width = width;
            Height = height; 

            if (!button)
            {
                button_ok.Visible = false;
                button_yes.Visible = false;
                button_cancel.Visible = false;
            }

            if (connect)
            {
                clientPipe = new ClientPipe(".", clientPipeName);
                Task.Run(() => clientPipe.Connect());
            }
            
        }

        public DialogResult ShowDialog(string text, string title = "", MessageBoxButtons button = MessageBoxButtons.OK, Bitmap icon = null, int width = 390, int height = 170)
        {
            this.Width = width;
            this.Height = height;
            this.Text = title;
            this.label_message.Text = text;
            if (icon != null) imageBox.Image = icon;
            else imageBox.Image = Images.initialize;

            switch (button)
            {
                case MessageBoxButtons.OK:
                    {
                        button_cancel.Text = "OK";
                        button_ok.Visible = false;
                        button_yes.Visible = false;
                        break;
                    }
                case MessageBoxButtons.OKCancel:
                    {
                        //button_cancel.Text = "CANCEL";
                        //button_ok.Visible = false;
                        button_yes.Visible = false;
                        break;
                    }
                case MessageBoxButtons.YesNo:
                    {
                        button_cancel.Text = "NO";
                        button_ok.Text = "YES";
                        button_yes.Visible = false;
                        break;
                    }
                case MessageBoxButtons.YesNoCancel:
                    {
                        //button_cancel.Text = "CANCEL";
                        button_ok.Text = "NO";
                        //button_yes.Visible = false;
                        break;
                    }
            }

            System.Media.SystemSounds.Exclamation.Play();
            base.ShowDialog();
            return result;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //if (e.CloseReason == CloseReason.UserClosing)
            //{
               
            //}

            if (TOPMOST)
            {
                e.Cancel = true;
                return;
            }

            this.Dispose();
            base.OnFormClosing(e);
        }
        
        private void GUI_dialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }
        private void button_yes_Click(object sender, EventArgs e)
        {
            switch (button_yes.Text)
            {
                case "YES":
                    {
                        _result = DialogResult.Yes;
                        break;
                    }
            }

            this.Close();
        }
        private void button_ok_Click(object sender, EventArgs e)
        {
            switch (button_ok.Text)
            {
                case "OK":
                    {

                        _result = DialogResult.OK;
                        break;
                    }
                case "YES":
                    {

                        _result = DialogResult.Yes;
                        break;
                    }

                case "NO":
                    {

                        _result = DialogResult.No;
                        break;
                    }
            }

            this.Close();
        }
        private void button_cancel_Click(object sender, EventArgs e)
        {
            switch (button_cancel.Text)
            {
                case "OK":
                    {

                        _result = DialogResult.OK;
                        break;
                    }

                case "CANCEL":
                    {

                        _result = DialogResult.Cancel;
                        break;
                    }
                case "NO":
                    {

                        _result = DialogResult.No;
                        break;
                    }
            }

            this.Close();
        }
    }
}
