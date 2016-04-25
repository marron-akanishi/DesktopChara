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
        private IniFile skindata;
        private string skinname;

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
            //レジストリの読み込み
            this.Location = new Point((int)Program.regkey.GetValue("posX") - this.Size.Width, (int)Program.regkey.GetValue("posY") - this.Size.Height);
            //スキンデータの読み込み
            string path = Program.basepath + "skin.ini";
            skindata = new IniFile(path);
            skinname = skindata.GetValue("skininfo","name","スキン名");
            string skinicon = skindata.GetValue("skininfo", "icon", "icon.png");
            path = path.Replace("skin.ini",skinicon);
            label2.Text = skinname;
            pictureBox1.Image = Image.FromFile(path);
        }
    }
}
