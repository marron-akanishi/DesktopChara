﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SpeechLib;

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
        //操作用
        //音声認識オブジェクト
        private SpeechLib.SpInProcRecoContext ControlRule = null;
        //音声認識のための言語モデル
        private SpeechLib.ISpeechRecoGrammar ControlGrammarRule = null;
        //音声認識のための言語モデルのルールのトップレベルオブジェクト.
        private SpeechLib.ISpeechGrammarRule ControlGrammarRuleGrammarRule = null;
        //モード切替用
        //音声認識オブジェクト
        private SpeechLib.SpInProcRecoContext AlwaysRule = null;
        //音声認識のための言語モデル
        private SpeechLib.ISpeechRecoGrammar AlwaysGrammarRule = null;
        //音声認識のための言語モデルのルールのトップレベルオブジェクト.
        private SpeechLib.ISpeechGrammarRule AlwaysGrammarRuleGrammarRule = null;

        public Form1()
        {
            InitializeComponent();
            timer = new Timer();
            timer.Tick += new EventHandler(UpdateTime);
            timer.Interval = 1000;
            timer.Enabled = true;
            rnd = new Random();
            AlwaysSpeechInit();
            ControlSpeechInit();
        }

        private void AlwaysSpeechInit()
        {
            //ルール認識 音声認識オブジェクトの生成
            this.AlwaysRule = new SpeechLib.SpInProcRecoContext();
            bool hit = false;
            foreach (SpObjectToken recoperson in this.AlwaysRule.Recognizer.GetRecognizers()) //'Go through the SR enumeration
            {
                string language = recoperson.GetAttribute("Language");
                if (language == "411")
                {//日本語を聴き取れる人だ
                    this.AlwaysRule.Recognizer.Recognizer = recoperson; //君に聞いていて欲しい
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                MessageBox.Show("日本語認識が利用できません。\r\n日本語音声認識 MSSpeech_SR_ja-JP_TELE をインストールしてください。\r\n");
            }

            //マイクから拾ってね。
            this.AlwaysRule.Recognizer.AudioInput = this.CreateMicrofon();

            //音声認識イベントで、デリゲートによるコールバックを受ける.
            //認識完了
            this.AlwaysRule.Recognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.SpeechRecognitionType srt, SpeechLib.ISpeechRecoResult isrr)
                {
                    string strText = isrr.PhraseInfo.GetText(0, -1, true);
                    //音声認識終了
                    this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
                    label1_MouseUp(null, null);
                };

            //言語モデルの作成
            this.AlwaysGrammarRule = this.AlwaysRule.CreateGrammar(0);

            this.AlwaysGrammarRule.Reset(0);
            //言語モデルのルールのトップレベルを作成する.
            this.AlwaysGrammarRuleGrammarRule = this.AlwaysGrammarRule.Rules.Add("AlwaysRule",
                SpeechRuleAttributes.SRATopLevel | SpeechRuleAttributes.SRADynamic);
            //認証用文字列の追加.
            this.AlwaysGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "ありすちゃんありすちゃん");
            
            //ルールを反映させる。
            this.AlwaysGrammarRule.Rules.Commit();
        }

        private void ControlSpeechInit()
        {
            //ルール認識 音声認識オブジェクトの生成
            this.ControlRule = new SpeechLib.SpInProcRecoContext();
            bool hit = false;
            foreach (SpObjectToken recoperson in this.ControlRule.Recognizer.GetRecognizers()) //'Go through the SR enumeration
            {
                string language = recoperson.GetAttribute("Language");
                if (language == "411")
                {//日本語を聴き取れる人だ
                    this.ControlRule.Recognizer.Recognizer = recoperson; //君に聞いていて欲しい
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                MessageBox.Show("日本語認識が利用できません。\r\n日本語音声認識 MSSpeech_SR_ja-JP_TELE をインストールしてください。\r\n");
            }

            //マイクから拾ってね。
            this.ControlRule.Recognizer.AudioInput = this.CreateMicrofon();

            //音声認識イベントで、デリゲートによるコールバックを受ける.
            //認識完了
            this.ControlRule.Recognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.SpeechRecognitionType srt, SpeechLib.ISpeechRecoResult isrr)
                {
                    string strText = isrr.PhraseInfo.GetText(0, -1, true);
                    //音声認識終了
                    this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSInactive);
                    SpeechTextBranch(strText);
                };
            //認識失敗
            this.ControlRule.FalseRecognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.ISpeechRecoResult isrr)
                {
                    label1.Text = "？？";
                };

            //言語モデルの作成
            this.ControlGrammarRule = this.ControlRule.CreateGrammar(0);

            this.ControlGrammarRule.Reset(0);
            //言語モデルのルールのトップレベルを作成する.
            this.ControlGrammarRuleGrammarRule = this.ControlGrammarRule.Rules.Add("ControlRule",
                SpeechRuleAttributes.SRATopLevel | SpeechRuleAttributes.SRADynamic);
            //認証用文字列の追加.
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "プログラムを実行したい");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "時計に戻して");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "君の名前は");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "終了");

            //ルールを反映させる。
            this.ControlGrammarRule.Rules.Commit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegLoad();
            textBox1.Visible = false;
            button1.Visible = false;
            label1.Text = "";
            filelist = new Filelist();
            string path = filelist.GetPath("start",0);
            show(path);
            this.TopMost = true;
            this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
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
                    //音声認識ストップ
                    this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
                    this.AlwaysGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSInactive);
                    //現在の位置を一旦保存
                    RegSave();
                    Form2 setting = new Form2();
                    setting.ShowDialog(this);
                    //変更された情報を読み込む
                    RegLoad();
                    //音声認識再開
                    if(mode == "clock") this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
                    else if(mode == "voice") this.AlwaysGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSActive);
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
            if (pictureBox2.Image != null) pictureBox2.Image.Dispose();
            pictureBox2.Image = Image.FromFile(@path);
            //吹き出し設置
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            pictureBox1.Image = Image.FromFile(filelist.GetPath("ballon", 0));
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
            label1.Text = dtNow.ToString("yyyy/MM/dd") + "\n" + dtNow.ToString("HH:mm:ss");
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != @"")
            {
                try
                {
                    Process.Start(textBox1.Text);
                }
                catch
                {
                    MessageBox.Show("実行出来なかったよ\n" + textBox1.Text, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                textBox1.Text = @"";
                label1_MouseUp(null, null);
            }
        }

        //マイクから読み取るため、マイク用のデバイスを指定する.
        // C++ だと SpCreateDefaultObjectFromCategoryId ヘルパーがあるんだけど、C#だとないんだなこれが。
        private SpeechLib.SpObjectToken CreateMicrofon()
        {
            SpeechLib.SpObjectTokenCategory objAudioTokenCategory = new SpeechLib.SpObjectTokenCategory();
            objAudioTokenCategory.SetId(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech Server\v11.0\AudioInput", false);
            SpeechLib.SpObjectToken objAudioToken = new SpeechLib.SpObjectToken();
            objAudioToken.SetId(objAudioTokenCategory.Default, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech Server\v11.0\AudioInput", false);
            return objAudioToken;
        }
        
        //音声認識分岐
        private void SpeechTextBranch(string speechtext)
        {
            if(speechtext == "プログラムを実行したい")
            {
                label1.Text = @"何を実行する？";
                textBox1.Location = new Point(12, 43);
                button1.Location = new Point(136, 40);
                textBox1.Visible = true;
                button1.Visible = true;
                timer.Enabled = false;
                mode = "file";
            }
            else if(speechtext == "時計に戻して")
            {
                label1_MouseUp(null, null);
            }
            else if(speechtext == "君の名前は")
            {
                label1.Text = "鳥海 有栖だよ";
                mode = "name"; 
            }
            else if(speechtext == "終了")
            {
                label1.Text = "終了処理中";
                DialogResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    RegSave();
                    Application.Exit();
                }
                mode = "exit";
                label1_MouseUp(null, null);
            }
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e != null && e.Button == MouseButtons.Right) return;
            if (mode == "clock")
            {
                /*label1.Text = @"何を実行する？";
                textBox1.Location = new Point(12, 43);
                button1.Location = new Point(136, 40);
                textBox1.Visible = true;
                button1.Visible = true;
                timer.Enabled = false;
                mode = "file";*/
                label1.Text = "どうしたの？";
                timer.Enabled = false;
                mode = "voice";
                //音声認識開始。(トップレベルのオブジェクトの名前で SpeechRuleState.SGDSActive を指定する.)
                this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSActive);
            }
            else
            {
                //音声認識終了
                this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSInactive);
                textBox1.Text = "";
                textBox1.Visible = false;
                button1.Visible = false;
                UpdateTime(null, null);
                timer.Enabled = true;
                this.Focus();
                mode = "clock";
                //音声認識開始
                this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
            }
        }
    }
}
