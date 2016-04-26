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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label1.Text = "" + (140 - textBox1.TextLength) + "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Program.tweetdata = textBox1.Text;
            textBox1.Text = "";
            this.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Program.tweetdata = "";
            textBox1.Text = "";
            this.Dispose();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            //レジストリの読み込み
            this.Location = new Point((int)Program.regkey.GetValue("posX") - this.Size.Width, (int)Program.regkey.GetValue("posY") - this.Size.Height);
        }
    }
}
