using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace DesktopChara
{
    static class Program
    {
        public static Windowclass window;
        public static string type = @"start";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            loop();
        }

        static void loop()
        {
            window = new Windowclass();
        }
    }

    public class Windowclass
    {
        public Form1 mainform = new Form1();
        public Windowclass()
        {
            Application.Run(mainform);
        }
    }

    public class File
    {
        private string basepath = Application.ExecutablePath;
        private DataTable dt = new DataTable();

        public File()
        {
#if DEBUG
            basepath = basepath.Replace("DesktopChara.EXE", @"media\");
#else
            basepath = basepath.Replace("DesktopChara.exe", @"media\");
#endif
            //CSVファイルの名前
            string csvFileName = "filelist.csv";

            //接続文字列
            string conString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                + basepath + ";Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";
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
            DataRow[] rows = dt.Select("type = '" + type + "'");
            string path = basepath + (string)rows[no][0];
            if(type != "icon" && type != "ballon") Program.type = type;
            return path;
        }
    }
}
