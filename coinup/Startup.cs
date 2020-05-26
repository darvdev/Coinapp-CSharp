using System.Diagnostics;
using System.Windows.Forms;

namespace coinup
{
    public partial class Startup : Form
    {
        private ContextMenuStrip option;
        private ToolStripMenuItem device;
        private ToolStripMenuItem software;
        public Startup()
        {
            InitializeComponent();
            Text = "Coinapp Updater v" + Application.ProductVersion;
            option = new ContextMenuStrip(); 
            device = new ToolStripMenuItem("Start with show verbose output");
            software = new ToolStripMenuItem("Update software wihtout asking");
            option.ItemClicked += Option_ItemClicked;
        }

        private void picturebox_software_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Hide();
                Software software = new Software();
                software.Show();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (option.Items.Contains(device)) option.Items.Remove(device);
                if (!option.Items.Contains(software)) option.Items.Add(software);
                option.Show(Cursor.Position);
            }
        }

        private void picturebox_device_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Hide();
                Device device = new Device();
                device.Show();
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (option.Items.Contains(software)) option.Items.Remove(software);
                if (!option.Items.Contains(device)) option.Items.Add(device);
                option.Show(Cursor.Position);
            }
        }

        private void label_copyright_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Process.Start("https://www.facebook.com/CoinappTechnology");
            }

        }

        private void Option_ItemClicked(object sender, ToolStripItemClickedEventArgs e) 
        {
            switch (e.ClickedItem.Text)
            {
                case "Start with show verbose output":
                    {
                        this.Hide();
                        Program.VERBOSE = true;
                        Device device = new Device();
                        device.Show();
                        break;
                    }
                case "Update software wihtout asking":
                    {
                        this.Hide();
                        Program.SILENT = true;
                        Software software = new Software();
                        software.Show();
                        break;
                    }
                default: break;
            }
        }
    }
}
