using System;
using System.Drawing;
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

        //レジストリ保存
        private void RegSave()
        {
            Program.regkey.SetValue("APIKey", textBox1.Text);
            Program.regkey.SetValue("APISecret", textBox2.Text);
            Program.regkey.SetValue("AccessToken", textBox3.Text);
            Program.regkey.SetValue("AccessSecret", textBox4.Text);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //レジストリの読み込み
            this.Location = new Point((int)Program.regkey.GetValue("posX") - this.Size.Width, (int)Program.regkey.GetValue("posY") - this.Size.Height);
            textBox1.Text = (string)Program.regkey.GetValue("APIKey");
            textBox2.Text = (string)Program.regkey.GetValue("APISecret");
            textBox3.Text = (string)Program.regkey.GetValue("AccessToken");
            textBox4.Text = (string)Program.regkey.GetValue("AccessSecret");
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
