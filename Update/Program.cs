using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Update
{
    static class Program
    {
        public static IPEndPoint ServerIpPort;
        public static float Version;
        public static string TargetPath;


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if(args.Length>=3)
            {

                //版本
                Version = float.Parse(args[0]);
                //目标目录
                TargetPath = args[1];
                //ip端口
                string ip = args[2].Split(':')[0];
                string port = args[2].Split(':')[1];
                ServerIpPort = new IPEndPoint(IPAddress.Parse(ip), short.Parse(port));


                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var from1 = new Form1();
                Application.Run(from1);
            }
        }
    }
}
