using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace DesktopChara
{
    static class Program
    {
        public static string basepath = Application.ExecutablePath;
        public static Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\test\DesktopChara",true);
        public static string skinfolder = "default";
        public static string type = @"start";
        public static string tweetdata = "";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //前回設定したスキンを読み込む
            if (regkey != null) skinfolder = (string)regkey.GetValue("skin");
#if DEBUG
            basepath = basepath.Replace("DesktopChara.EXE", @"skins\");
#else
            basepath = basepath.Replace("DesktopChara.exe", @"skins\");
#endif
            basepath = basepath + skinfolder + @"\";
            Application.Run(new Form1());
        }
    }

    //ファイルリスト参照用クラス
    //csv用にもう少し抽象化したほうがよさげ
    public class Filelist
    {
        private DataTable dt = new DataTable();

        public Filelist()
        {
            //CSVファイルの名前
            string csvFileName = "filelist.csv";

            //接続文字列
            string conString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                + Program.basepath + ";Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
            System.Data.OleDb.OleDbConnection con =
                new System.Data.OleDb.OleDbConnection(conString);

            string commText = "SELECT * FROM [" + csvFileName + "]";
            System.Data.OleDb.OleDbDataAdapter da =
                new System.Data.OleDb.OleDbDataAdapter(commText, con);

            //DataTableに格納する
            da.Fill(dt);
        }

        public string GetPath(string type,int no)
        {
            string path = "";
            try
            {
                DataRow[] rows = dt.Select("type = '" + type + "'");
                path = Program.basepath + (string)rows[no][0];
            }
            catch (Exception)
            {
                MessageBox.Show("ファイルリスト参照中にエラーが発生しました\nプログラムを終了します\ntype=" + type + ",no=" + no, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            if (type != "ballon") Program.type = type;
            return path;
        }
    }

    /// <summary>
    /// INIファイルを読み書きするクラス
    /// </summary>
    public class IniFile
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedstring,
            int nSize,
            string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern int WritePrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpstring,
            string lpFileName);

        string filePath;

        /// <summary>
        /// ファイル名を指定して初期化します。
        /// ファイルが存在しない場合は初回書き込み時に作成されます。
        /// </summary>
        public IniFile(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// sectionとkeyからiniファイルの設定値を取得、設定します。 
        /// </summary>
        /// <returns>指定したsectionとkeyの組合せが無い場合は""が返ります。</returns>
        public string this[string section, string key]
        {
            set
            {
                WritePrivateProfileString(section, key, value, filePath);
            }
            get
            {
                StringBuilder sb = new StringBuilder(256);
                GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, filePath);
                return sb.ToString();
            }
        }

        /// <summary>
        /// sectionとkeyからiniファイルの設定値を取得します。
        /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
        /// </summary>
        /// <returns>
        /// 指定したsectionとkeyの組合せが無い場合はdefaultvalueで指定した値が返ります。
        /// </returns>
        public string GetValue(string section, string key, string defaultvalue)
        {
            StringBuilder sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, defaultvalue, sb, sb.Capacity, filePath);
            return sb.ToString();
        }
    }
}
