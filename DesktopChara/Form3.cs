using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopChara
{
    public partial class Form3 : Form
    {
        [DllImport("SpeechDialog.dll")]
        public extern static bool SpeechDlg(IntPtr Handle, [MarshalAs(UnmanagedType.LPArray)] byte[] res);

        public Form3()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            label1.Text = "" + (140 - textBox1.TextLength) + "";
        }

        //OK
        private void button1_Click(object sender, EventArgs e)
        {
            Program.tweetdata = textBox1.Text;
            textBox1.Text = "";
            this.Dispose();
        }

        //キャンセル
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

        private void button3_Click(object sender, EventArgs e)
        {
            bool res;
            byte[] res_byte = new byte[4096];
            res = SpeechDlg(IntPtr.Zero, res_byte);
            if (res)
            {
                textBox1.Text = System.Text.Encoding.GetEncoding("shift_jis").GetString(res_byte);           
            }
        }
    }
}
