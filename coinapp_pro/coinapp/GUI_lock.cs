using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using coinapplib;

namespace coinapp
{
    public partial class GUI_lock : Form
    {
        public IntPtr iHandle;
        public GUI_lock()
        {
            InitializeComponent();
        }
        public GUI_lock(IntPtr hWnd)
        {
            InitializeComponent();
            iHandle = hWnd;
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = 80;
            this.Location = new Point(0, Screen.PrimaryScreen.Bounds.Height);
            //this.Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - this.Height);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            return;
        }
        private void button_shutdown_Click(object sender, EventArgs e)
        {
            button_shutdown.Cursor = Cursors.Default;
            button_shutdown.Image = Images.button_shutdown_x;

            if (c.FormExists("GUI_dialog"))
                c.app.dialog.Close();

            c.app.dialog = new GUI_dialog();

            DialogResult result = c.app.dialog.ShowDialog("Do you really want to shutdown?", "Shutdown", MessageBoxButtons.OKCancel, Images.button_shutdown);
            
            if (result == DialogResult.OK)
            {
                if (c.CONNECTED)
                {
                    c.SESSION = SESSION.SHUTTING_DOWN;
                    button_status.Image = null;
                    label_messages.Text = c.S.ToString(c.SESSION);
                    c.mainPipe.WriteString(ARGS.SHUTDOWN);
                }
                else
                {
                    MessageBox.Show("Something went wrong to the application. Shutdown proceed.", "Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Shutdown.Force();
                }
            }
            else
            {
                button_shutdown.Image = Images.button_shutdown;
                button_shutdown.Cursor = Cursors.Hand;
            }
        }
        public void button_status_Click(object sender, EventArgs e)
        {
            if (!c.CONNECTED)
                return;

            if (!c.INITIALIZED)
                return;

            if (c.REMAINING_TIME > 60)
            {
                this.button_status.Cursor = Cursors.Default;
                this.button_status.Image = Images.button_resume_x;

                if (c.FormExists("GUI_dialog"))
                    c.app.dialog.Close();

                c.app.dialog = new GUI_dialog();
                DialogResult result = c.app.dialog.ShowDialog(c.TimeResumeToString(c.REMAINING_TIME), "Time resumable", MessageBoxButtons.YesNo, Images.button_resume);
                c.app.dialog.Close();
                
                if (result == DialogResult.Yes)
                {
                    if (c.CONNECTED)
                    {
                        c.SESSION = SESSION.TIME_RESUME;
                        this.button_status.Image = Images.ready;
                        this.button_status.Cursor = Cursors.Default;
                        c.mainPipe.WriteString(ARGS.RESUME);
                    }
                    else
                    {
                        this.button_status.Image = Images.button_resume;
                        this.button_status.Cursor = Cursors.Hand;
                        MessageBox.Show("Cannot use the remaining time right now .", "Time resume", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    this.button_status.Image = Images.button_resume;
                    this.button_status.Cursor = Cursors.Hand;
                }
            }
        }
        private void GUI_lock_Load(object sender, EventArgs e)
        {
            LockImage LockImage = new LockImage();
            LockImage.Show();
            LockImage.Enabled = false;
            LockImage.BackgroundImage = Images.logo;
            Task.Run( async() =>
            {
                await Task.Delay(100);
                this.BeginInvoke(() =>
                {
                    if (c.LOCKIMAGE != null)
                    {
                        LockImage.BackgroundImage = null;
                        LockImage.Reinit(c.LOCKIMAGE);
                    }
                });
            });

            //this.BeginInvoke(() => Location = new Point(0, 600));

            Task.Run(() => LockAnimation());
        }

        private void LockAnimation()
        {
            int LocY = Screen.PrimaryScreen.Bounds.Height - Height;
            int moveY = Screen.PrimaryScreen.Bounds.Height;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);

                    if (c.LOCKED)
                    {
                        await Task.Delay(100);
                        while ((moveY - 5) >= LocY)
                        {
                            await Task.Delay(5);
                            this.BeginInvoke(() => Location = new Point(0, moveY));
                            moveY -= 3;
                        }

                        if (Location.Y <= Screen.PrimaryScreen.Bounds.Height - Height)
                        {
                            this.BeginInvoke(() => Location = new Point(0, Screen.PrimaryScreen.Bounds.Height - Height));
                        }
                    }
                    else
                    {
                        if (moveY <= Screen.PrimaryScreen.Bounds.Height) moveY = Screen.PrimaryScreen.Bounds.Height;
                        if (Location.Y <= Screen.PrimaryScreen.Bounds.Height) this.BeginInvoke(() => Location = new Point(0, Screen.PrimaryScreen.Bounds.Height));
                    }
                }
            });
        }
    }

    public class LockImage : Form
    {
        public LockImage()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            e.Cancel = true;
            return;
        }
        public void Reinit(Bitmap image)
        {
            this.Width = SystemInformation.VirtualScreen.Width;
            this.Height = SystemInformation.VirtualScreen.Height;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.BackgroundImage = image;
            this.CenterToScreen();
        }
        private void InitializeComponent()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TransparencyKey = SystemColors.Control;
            this.BackColor = SystemColors.Control;
            this.FormBorderStyle = FormBorderStyle.None;
            //this.BackgroundImage = Images.logo;
            this.BackgroundImageLayout = ImageLayout.Zoom;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LockImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(300, 300);
            this.TopMost = false;

            this.ResumeLayout(false);
        }
    }
}
