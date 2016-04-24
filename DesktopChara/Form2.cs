using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopChara
{
    public partial class Form2 : Form
    {
        private File file;

        public Form2()
        {
            InitializeComponent();
        }

        //OK
        private void button1_Click(object sender, EventArgs e)
        {
            RegSave();
            this.Dispose();
        }

        //キャンセル
        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void RegSave()
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            file = new File();
            this.pictureBox1.Image = Image.FromFile(file.GetPath("icon",0));
            Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\test\DesktopChara", false);
            this.Location = new Point((int)regkey.GetValue("posX") - this.Size.Width, (int)regkey.GetValue("posY") - this.Size.Height);
        }
    }
}
