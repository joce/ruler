using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Ruler
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            richTextBox.Text = richTextBox.Text.Replace("{VERSION}", Application.ProductVersion);
			Icon appIcon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
			pictureBox.Image = appIcon.ToBitmap();
        }

        private void RichTextBox_Enter(object sender, EventArgs e)
        {
            //HACK: Prevent the I-Beam cursor from appearing.
            ActiveControl = null;
        }

        private void RichTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
