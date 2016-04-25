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
    public partial class Form1 : Form
    {
        private Random rnd;
        private Filelist filelist;
        private Point lastMousePosition;
        private bool mouseCapture;
        private String lasttype;
        private int lastno  = 0;
        private string mode = "clock";
        private Timer timer;

        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Tick += new EventHandler(UpdateTime);
            timer.Interval = 1000;
            timer.Enabled = true;
            rnd = new Random();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegLoad();
            this.textBox1.Visible = false;
            this.button1.Visible = false;
            this.label1.Text = "";
            filelist = new Filelist();
            string path = filelist.GetPath("start",0);
            show(path);
            this.TopMost = true;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    //マウスの位置を所得
                    this.lastMousePosition = Control.MousePosition;
                    this.mouseCapture = true;

                    //この間だけ画像変更
                    lasttype = Program.type;
                    show(filelist.GetPath("surprise",0));
                    break;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.mouseCapture == false)
            {
                return;
            }

            // 最新のマウスの位置を取得
            Point mp = Control.MousePosition;

            // 差分を取得
            int offsetX = mp.X - this.lastMousePosition.X;
            int offsetY = mp.Y - this.lastMousePosition.Y;

            // コントロールを移動
            this.Location = new Point(this.Left + offsetX, this.Top + offsetY);

            this.lastMousePosition = mp;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    this.mouseCapture = false;
                    show(filelist.GetPath(lasttype,lastno));
                    break;
                case MouseButtons.Left:
                    if (this.mouseCapture == true) break;
                    if (Program.type == "general")
                    {
                        int no = rnd.Next(7);
                        show(filelist.GetPath("random", no));
                        lastno = no;
                    }
                    else if (Program.type == "random" || Program.type == "change")
                    {
                        show(filelist.GetPath("general", 0));
                        lastno = 0;
                    }
                    else if (Program.type == "start") show(filelist.GetPath("change", 0));
                    break;
            }
        }

        private void Form1_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (this.mouseCapture == true && this.Capture == false)
            {
                this.mouseCapture = false;
            }
        }

        //キー取得
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.F2:
                    //現在の位置を一旦保存
                    RegSave();
                    Form2 setting = new Form2();
                    setting.ShowDialog(this);
                    //変更された情報を読み込む
                    RegLoad();
                    break;
                case Keys.Escape:
                    DialogResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        RegSave();
                        Application.Exit();
                    }
                    break;
                case Keys.ControlKey:
                    this.mouseCapture = false;
                    show(filelist.GetPath(lasttype, lastno));
                    break;
            }
        }

        //画像を表示する
        public void show(string path)
        {
            //キャラ設置
            if (this.pictureBox2.Image != null) this.pictureBox2.Image.Dispose();
            this.pictureBox2.Image = Image.FromFile(@path);
            //吹き出し設置
            if (this.pictureBox1.Image != null) this.pictureBox1.Image.Dispose();
            this.pictureBox1.Image = Image.FromFile(filelist.GetPath("ballon", 0));
            //ウィンドウ透過
            this.TransparencyKey = BackColor;
        }

        //ウィンドウの大きさを画像の大きさに変更
        private void size_change(string path)
        {
            //元画像の縦横サイズを調べる
            System.Drawing.Bitmap bmpSrc = new System.Drawing.Bitmap(@path);
            int width = bmpSrc.Width;
            int height = bmpSrc.Height;
            bmpSrc.Dispose();
            //ウィンドウのサイズを変更
            this.Size = new Size(width, height + 54);
        }

        //時計の描画
        public void UpdateTime(object sender, EventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            this.label1.Text = dtNow.ToString("yyyy/MM/dd") + "\n" + dtNow.ToString("HH:mm:ss");
        }

        //レジストリへの書き込み
        private void RegSave()
        {
            Program.regkey.SetValue("posX", this.Location.X);
            Program.regkey.SetValue("posY", this.Location.Y);
            Program.regkey.SetValue("skin", Program.skinfolder);
        }

        //レジストリの読み込み
        private void RegLoad()
        {
            if(Program.regkey == null)
            {
                this.Location = new Point(100, 100);
                return;
            }
            this.Location = new Point((int)Program.regkey.GetValue("posX"), (int)Program.regkey.GetValue("posY"));
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    this.lastMousePosition = Control.MousePosition;
                    this.mouseCapture = true;
                    //この間だけ画像変更
                    if(Program.type != "surprise") lasttype = Program.type;
                    show(filelist.GetPath("surprise", 0));
                    break;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if(mode == "clock")
            {
                label1.Text = @"ファイル名を入力してね";
                textBox1.Visible = true;
                button1.Visible = true;
                timer.Enabled = false;
                mode = "file";
            }
            else
            {
                textBox1.Visible = false;
                button1.Visible = false;
                timer.Enabled = true;
                mode = "clock";
            }
        }
    }
}
