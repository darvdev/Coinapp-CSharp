using System;
using System.Drawing;
using System.Windows.Forms;
using coinapplib;

namespace coinapp
{
    public partial class GUI_timer : Form
    {

        private bool mouseDown;
        private Point lastLocation;
        private ContextMenuStrip contextOpacity;

        public GUI_timer()
        {
            InitializeComponent();
            contextOpacity = new ContextMenuStrip();
            contextOpacity.Items.Add("100%");
            contextOpacity.Items.Add("75%");
            contextOpacity.Items.Add("50%");
            contextOpacity.Items.Add("25%");
            contextOpacity.ItemClicked += Context_ItemClicked;   
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TOOLWINDOW = 0x80;
                const int WS_EX_TOPMOST = 0x8;
                const int CS_NOCLOSE = 0x200;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                cp.ExStyle |= WS_EX_TOPMOST | WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        private void Context_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.BeginInvoke(() =>
            {
                switch (e.ClickedItem.Text)
                {
                    case "100%": this.Opacity = 1.00; break;
                    case "75%": this.Opacity = 0.75; break;
                    case "50%": this.Opacity = 0.50; break;
                    case "25%": this.Opacity = 0.25; break;
                    default: this.Opacity = 1.00; break;
                }
            });
        }

        //private void Opacity_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        //{
        //    if (e.ClickedItem == opacity.DropDownItems[0])
        //    {

        //    }
        //    MessageBox.Show(e.ClickedItem.Text);
        //}

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            c.ecode.Add(ECODE.TIMER_DISPLAY_TERMINATED);
            this.Dispose();
            base.OnFormClosing(e);
        }

        private void DefaultLocation()
        {
            this.BeginInvoke(() => this.Location = new Point(SystemInformation.WorkingArea.Width - (this.Width + 10), SystemInformation.WorkingArea.Height - (this.Height + 10)));
        }

        private void GUI_timer_Load(object sender, EventArgs e)
        {
            DefaultLocation();
        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                lastLocation = e.Location;
            }
                
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && e.Button == MouseButtons.Left)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void Control_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
            DefaultLocation();
        }

        private void Control_DoubleClick(object sender, EventArgs e)
        {
            c.app.UpdateTimerVisible();
        }

        private void GUI_timer_Deactivate(object sender, EventArgs e)
        {
            mouseDown = false;
            DefaultLocation();
        }

        private void Timer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextOpacity.Items[0].Enabled = Opacity == 1.0 ? false : true;
                contextOpacity.Items[1].Enabled = Opacity == 0.75 ? false : true;
                contextOpacity.Items[2].Enabled = Opacity == 0.50 ? false : true;
                contextOpacity.Items[3].Enabled = Opacity == 0.25 ? false : true;                
                contextOpacity.Show(Cursor.Position);

            }
        }
    }
}
