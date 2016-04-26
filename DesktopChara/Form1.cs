using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SpeechLib;
using CoreTweet;
using System.Data;

namespace DesktopChara
{
    public partial class Form1 : Form
    {
        private Random rnd;
        private Filelist filelist;
        private Programlist list;
        private Point lastMousePosition;
        private bool mouseCapture;
        private String lasttype;
        private int lastno  = 0;
        private string mode = "clock";
        private Timer timer;
        private Tokens twitter;
        //モード切替用(Dictation実装)
        //音声認識オブジェクト
        private SpeechLib.SpInProcRecoContext AlwaysRule = null;
        //音声認識のための言語モデル
        private SpeechLib.ISpeechRecoGrammar AlwaysGrammarRule = null;
        //音声認識のための言語モデルのルールのトップレベルオブジェクト.
        private SpeechLib.ISpeechGrammarRule AlwaysGrammarRuleGrammarRule = null;
        //操作用
        //音声認識オブジェクト
        private SpeechLib.SpInProcRecoContext ControlRule = null;
        //音声認識のための言語モデル
        private SpeechLib.ISpeechRecoGrammar ControlGrammarRule = null;
        //音声認識のための言語モデルのルールのトップレベルオブジェクト.
        private SpeechLib.ISpeechGrammarRule ControlGrammarRuleGrammarRule = null;
        //プログラム起動用
        //音声認識オブジェクト
        private SpeechLib.SpInProcRecoContext ProgramRule = null;
        //音声認識のための言語モデル
        private SpeechLib.ISpeechRecoGrammar ProgramGrammarRule = null;
        //音声認識のための言語モデルのルールのトップレベルオブジェクト.
        private SpeechLib.ISpeechGrammarRule ProgramGrammarRuleGrammarRule = null;

        public Form1()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            timer = new Timer();
            timer.Tick += new EventHandler(UpdateTime);
            timer.Interval = 1000;
            timer.Enabled = true;
            rnd = new Random();
            AlwaysSpeechInit();
            ControlSpeechInit();
            ProgramSpeechInit();
        }

        //ツイッター初期化
        private void InitTwitter()
        {
            if((string)Program.regkey.GetValue("APIKey") != "")
            {
                try
                {
                    twitter = CoreTweet.Tokens.Create((string)Program.regkey.GetValue("APIKey")
                    , (string)Program.regkey.GetValue("APISecret")
                    , (string)Program.regkey.GetValue("AccessToken")
                    , (string)Program.regkey.GetValue("AccessSecret"));
                    //起動情報ツイート
                    DateTime dtNow = DateTime.Now;
                    //twitter.Statuses.Update(new { status = "有栖ちゃんを起動させました\n" + dtNow.ToString("HH:mm:ss") });
                }
                catch
                {
                    MessageBox.Show("Twitterへの接続に失敗しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    twitter = null;
                }
            }
        }

        //音声認識初期化
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
                Application.Exit();
            }

            //マイクから拾ってね。
            this.AlwaysRule.Recognizer.AudioInput = this.CreateMicrofon();

            //音声認識イベントで、デリゲートによるコールバックを受ける.
            //認識完了
            this.AlwaysRule.Recognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.SpeechRecognitionType srt, SpeechLib.ISpeechRecoResult isrr)
                {
                    //音声認識終了
                    this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
                    //アクティブにする
                    this.Focus();
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
                Application.Exit();
            }

            //マイクから拾ってね。
            this.ControlRule.Recognizer.AudioInput = this.CreateMicrofon();

            //音声認識イベントで、デリゲートによるコールバックを受ける.
            //認識完了
            this.ControlRule.Recognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.SpeechRecognitionType srt, SpeechLib.ISpeechRecoResult isrr)
                {
                    string strText = isrr.PhraseInfo.GetText(0, -1, true);
                    SpeechTextBranch(strText);
                };
            //認識失敗
            this.ControlRule.FalseRecognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.ISpeechRecoResult isrr)
                {
                    label1.Text = "？？";
                    show(filelist.GetPath("what", 0));
                };

            //言語モデルの作成
            this.ControlGrammarRule = this.ControlRule.CreateGrammar(0);

            this.ControlGrammarRule.Reset(0);
            //言語モデルのルールのトップレベルを作成する.
            this.ControlGrammarRuleGrammarRule = this.ControlGrammarRule.Rules.Add("ControlRule",
                SpeechRuleAttributes.SRATopLevel | SpeechRuleAttributes.SRADynamic);
            //認証用文字列の追加.
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "プログラムを実行したい");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "ツイートしたい");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "バッテリー残量は");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "プログラムリスト更新");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "設定を開いて");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "時計に戻して");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "君の名前は");
            this.ControlGrammarRuleGrammarRule.InitialState.AddWordTransition(null, "終了");

            //ルールを反映させる。
            this.ControlGrammarRule.Rules.Commit();
        }
        private void ProgramSpeechInit()
        {
            //ルール認識 音声認識オブジェクトの生成
            this.ProgramRule = new SpeechLib.SpInProcRecoContext();
            bool hit = false;
            foreach (SpObjectToken recoperson in this.ProgramRule.Recognizer.GetRecognizers()) //'Go through the SR enumeration
            {
                string language = recoperson.GetAttribute("Language");
                if (language == "411")
                {//日本語を聴き取れる人だ
                    this.ProgramRule.Recognizer.Recognizer = recoperson; //君に聞いていて欲しい
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                MessageBox.Show("日本語認識が利用できません。\r\n日本語音声認識 MSSpeech_SR_ja-JP_TELE をインストールしてください。\r\n");
                Application.Exit();
            }

            //マイクから拾ってね。
            this.ProgramRule.Recognizer.AudioInput = this.CreateMicrofon();

            //音声認識イベントで、デリゲートによるコールバックを受ける.
            //認識完了
            this.ProgramRule.Recognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.SpeechRecognitionType srt, SpeechLib.ISpeechRecoResult isrr)
                {
                    string strText = isrr.PhraseInfo.GetText(0, -1, true);
                    ProgramRun(strText);
                };
            //認識失敗
            this.ProgramRule.FalseRecognition +=
                delegate (int streamNumber, object streamPosition, SpeechLib.ISpeechRecoResult isrr)
                {
                    label1.Text = "？？";
                    show(filelist.GetPath("what", 0));
                };

            //言語モデルの作成
            this.ProgramGrammarRule = this.ProgramRule.CreateGrammar(0);

            this.ProgramGrammarRule.Reset(0);
            //言語モデルのルールのトップレベルを作成する.
            this.ProgramGrammarRuleGrammarRule = this.ProgramGrammarRule.Rules.Add("ProgramRule",
                SpeechRuleAttributes.SRATopLevel | SpeechRuleAttributes.SRADynamic);
            //認証用文字列の追加.
            list = new Programlist();
            foreach(string voice in list.voicelist)
            {
                this.ProgramGrammarRuleGrammarRule.InitialState.AddWordTransition(null, voice);
            }

            //ルールを反映させる。
            this.ProgramGrammarRule.Rules.Commit();
        }

        //表示前処理
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
            if(Program.regkey != null) InitTwitter();
            this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
        }

        //マウス動作取得
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
                    else if (Program.type == "start")
                    {
                        show(filelist.GetPath("change", 0));
                        lastno = 0;
                    }
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
                    if (mode == "clock")
                    {
                        //音声認識ストップ
                        this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
                        mode = "setting";
                        //現在の位置を一旦保存
                        RegSave();
                        lasttype = Program.type;
                        show(filelist.GetPath("kusonemi", 0));
                        Form2 setting = new Form2();
                        setting.ShowDialog(this);
                        //変更された情報を読み込む
                        RegLoad();
                        InitTwitter();
                        show(filelist.GetPath(lasttype, lastno));
                        //音声認識再開
                        this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
                    }
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

        //長押しキー取得
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

        //実行ボタン
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != @"")
            {
#if DEBUG
                if(mode == "voice")
                {
                    SpeechTextBranch(textBox1.Text);
                    textBox1.Text = @"";
                    return;
                }
#endif
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
        private SpeechLib.SpObjectToken CreateMicrofon()
        {
            SpeechLib.SpObjectTokenCategory objAudioTokenCategory = new SpeechLib.SpObjectTokenCategory();
            objAudioTokenCategory.SetId(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech Server\v11.0\AudioInput", false);
            SpeechLib.SpObjectToken objAudioToken = new SpeechLib.SpObjectToken();
            objAudioToken.SetId(objAudioTokenCategory.Default, @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech Server\v11.0\AudioInput", false);
            //return null;
            return objAudioToken;
        }
        
        //音声認識分岐
        private void SpeechTextBranch(string speechtext)
        {
            //音声認識終了
            this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSInactive);
            switch (speechtext)
            {
                case "プログラムを実行したい":
                    show(filelist.GetPath("search", 0));
                    label1.Text = "何を実行する？";
                    textBox1.Location = new Point(12, 43);
                    button1.Location = new Point(136, 40);
                    textBox1.Visible = true;
                    button1.Visible = true;
                    mode = "file";
                    this.ProgramGrammarRule.CmdSetRuleState("ProgramRule", SpeechRuleState.SGDSActive);
                    break;
                case "ツイートしたい":
                    show(filelist.GetPath("chair", 0));
                    label1.Text = "なんてつぶやく？";
                    mode = "twitter";
                    RegSave();
                    Form3 tweet = new Form3();
                    tweet.ShowDialog(this);
                    if (Program.tweetdata != "" && twitter != null)
                    {
                        label1.Text = "つぶやき中";
                        try
                        {
                            twitter.Statuses.Update(new { status = Program.tweetdata });
                        }
                        catch
                        {
                            MessageBox.Show("ツイートに失敗しました", "エラー", MessageBoxButtons.OK,MessageBoxIcon.Error);
                        }
                    }
                    label1_MouseUp(null, null);
                    break;
                case "バッテリー残量は":
                    show(filelist.GetPath("find", 0));
#if DEBUG
                    textBox1.Visible = false;
                    button1.Visible = false;
                    textBox1.Text = "";
#endif
                    PowerLineStatus pls = SystemInformation.PowerStatus.PowerLineStatus;
                    if (pls == PowerLineStatus.Online)
                    {
                        label1.Text = "AC電源駆動だよ";
                    }
                    else
                    {
                        float blp = SystemInformation.PowerStatus.BatteryLifePercent;
                        label1.Text = "バッテリー残量は\n" + blp * 100 + "% だよ";
                    }
                    mode = "battery";
                    this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSActive);
                    break;
                case "プログラムリスト更新":
                    label1.Text = "読み直してるよ";
                    show(filelist.GetPath("sleep", 3));
                    mode = "refresh";
                    ProgramSpeechInit();
                    MessageBox.Show("更新完了");
                    GC.Collect();
                    label1_MouseUp(null, null);
                    break;
                case "設定を開いて":
                    label1_MouseUp(null, null);
                    //音声認識ストップ
                    this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
                    mode = "setting";
                    //現在の位置を一旦保存
                    RegSave();
                    lasttype = Program.type;
                    show(filelist.GetPath("kusonemi", 0));
                    Form2 setting = new Form2();
                    setting.ShowDialog(this);
                    //変更された情報を読み込む
                    RegLoad();
                    InitTwitter();
                    show(filelist.GetPath(lasttype, lastno));
                    //音声認識再開
                    this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
                    break;
                case "時計に戻して":
                    label1_MouseUp(null, null);
                    break;
                case "君の名前は":
                    show(filelist.GetPath("tere", 0));
                    label1.Text = "鳥海 有栖だよ";
                    mode = "name";
                    this.ControlGrammarRule.CmdSetRuleState("ControlRule", SpeechRuleState.SGDSActive);
                    break;
                case "終了":
                    label1.Text = "終了しちゃうの？";
                    show(filelist.GetPath("naki", 0));
                    DialogResult result = MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        RegSave();
                        Application.Exit();
                    }
                    mode = "exit";
                    label1_MouseUp(null, null);
                    break;
            }
        }

        //プログラム実行音声認識
        private void ProgramRun(string command)
        {
            this.ProgramGrammarRule.CmdSetRuleState("ProgramRule", SpeechRuleState.SGDSInactive);
            DataRow[] rows = list.dt.Select("voice = '" + command + "'");
            try
            {
                Process.Start((string)rows[0][1]);
            }
            catch
            {
                MessageBox.Show("実行出来なかったよ\n" + (string)rows[0][1], "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            label1_MouseUp(null, null);
        }

        //テキストクリック
        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e != null && e.Button == MouseButtons.Right) return;
            if (mode == "clock")
            {
                label1.Text = "どうしたの？";
                timer.Enabled = false;
                if(Program.type != "surprise") lasttype = Program.type;
                show(filelist.GetPath("general", 1));
                mode = "voice";
#if DEBUG
                //デバッグ用入力ボックス
                textBox1.Location = new Point(12, 43);
                button1.Location = new Point(136, 40);
                textBox1.Visible = true;
                button1.Visible = true;
#endif
                //音声認識開始
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
                show(filelist.GetPath(lasttype, lastno));
                this.Focus();
                mode = "clock";
                //音声認識開始
                this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
            }
        }

        //通知領域アイコンクリック
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.Visible && mode == "clock")
            {
                this.Visible = false;
                this.WindowState = FormWindowState.Minimized;
                this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSInactive);
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.AlwaysGrammarRule.CmdSetRuleState("AlwaysRule", SpeechRuleState.SGDSActive);
            }
        }
    }
}
