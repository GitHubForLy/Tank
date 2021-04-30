using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Update
{
    public class UpdateClient
    {
        private IPEndPoint endPoint;
        private string targetDirectory;
        private int rcvSize;
        private DataPackage package;
        private bool isStop = false;    

        public event Action<int> ProgressEvent;
        public event Action<int> BytesEvent;
        public event Action<string> StateEvent;

        public UpdateClient(IPEndPoint pt,string targetDirectory,int recvBufferSize=1024*10)
        {
            package = new DataPackage();
            endPoint = pt;
            this.targetDirectory = targetDirectory;
            rcvSize = recvBufferSize;
        }

        public void StopUpdate()
        {
            isStop = true;
        }

        public  void Update(float version)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(endPoint);
            var stream = tcpClient.GetStream();
            long count;
            long rcvcount=0, sumsize = 0;

            StateEvent?.Invoke("开始更新...");
            //发送开始标志
            var data = Encoding.UTF8.GetBytes("S"+version.ToString());
            WriteBuffer(stream, data);

            StateEvent?.Invoke("读取文件和大小...");
            //读取文件数量 和总大小
            data = ReadBuffer(stream);
            var filecountAndSize = Encoding.UTF8.GetString(data, 0, data.Length);
            int filecount = int.Parse(filecountAndSize.Split('|')[0]);
            sumsize = long.Parse(filecountAndSize.Split('|')[1]);

            DateTime now = DateTime.Now;
            long oldbytes = 0;

            StateEvent?.Invoke("开始文件  ...");

            bool isContinue= getLastFile(out string lastfile, out long lastcount);


            //开始接收文件
            for (int i = 0; i < filecount; i++)
            {
                //读取文件名 和文件大小
                data = ReadBuffer(stream);
                var fileAndLen = Encoding.UTF8.GetString(data, 0, data.Length);
                var filename = fileAndLen.Split('|')[0];
                var len = int.Parse(fileAndLen.Split('|')[1]);
                filename = targetDirectory + filename.Substring(1);
                count = 0;
                FileStream fs;

                //发送接收的位置 
                if (isContinue && lastfile==filename)
                {
                    data = BitConverter.GetBytes(lastcount);
                    WriteBuffer(stream, data);
                    rcvcount += lastcount;
                    count = lastcount;
                    fs = new FileStream(filename, FileMode.Append);
                }
                else
                {
                    fs = new FileStream(filename, FileMode.Create);
                    data = BitConverter.GetBytes((long)0);
                    WriteBuffer(stream, data);
                }

                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                StateEvent?.Invoke("接收文件:"+filename+"  ...");

                //循环接收文件内容
                while (!isStop)
                {
                    data= ReadBuffer(stream);
                    fs.Write(data, 0, data.Length);
                    count += data.Length;


                    rcvcount += data.Length;
                    ProgressEvent?.Invoke((int)((double)rcvcount / sumsize * 100));

                    if((DateTime.Now-now).TotalSeconds  >1)
                    {       
                        BytesEvent?.Invoke((int)(rcvcount - oldbytes));
                        oldbytes = rcvcount;
                        now = DateTime.Now;
                    }


                    if (count >= len)
                        break;
                }
                fs.Flush();
                fs.Close();

                //如果停止 写入文件的位置
                if (isStop)
                {
                    writeLastFile(,filename, count);
                    return;
                }
            }

            Delfiles(stream);

            if (File.Exists("lastfile"))
                File.Delete("lastfile");

            tcpClient.Close();
            StateEvent?.Invoke("更新完成");
        }


        private void writeLastFile(float version,string filename,long count)
        {
            System.Diagnostics.Debug.WriteLine("count:" + count);
            FileStream logf = File.Create("lastfile");
            var bytes = Encoding.UTF8.GetBytes(version + "|"+filename + "|" + count.ToString());
            logf.Write(bytes,0,bytes.Length);
            logf.Flush();
            logf.Close();
        }

        private bool getLastFile(out float version,out string filename,out long count)
        {
            filename = "";
            count = 0;
            version = 0;

            if (!File.Exists("lastfile"))
                return false;
            string str = File.ReadAllText("lastfile");
            version = float.Parse(str.Split('|')[0]);
            filename = str.Split('|')[1];
            count = long.Parse(str.Split('|')[2]);

            return true;
        }


        private void Delfiles(NetworkStream stream)
        {
            byte[] data;
            StateEvent?.Invoke("开始删除文件  ...");
            //删除文件
            string name = "";
            data = ReadBuffer(stream);
            var dels = Encoding.UTF8.GetString(data, 0, data.Length);
            foreach (var de in dels.Split('|'))
            {
                name = targetDirectory + de.Substring(1);
                if (File.Exists(name))
                {
                    StateEvent?.Invoke("删除文件:" + name + "  ...");
                    File.Delete(name);
                }
            }
        }


        private byte[] ReadBuffer(NetworkStream stream)
        {
            byte[] buffer = new byte[rcvSize];

            while (!package.CanOutPackage())
            {
                var count = stream.Read(buffer, 0, buffer.Length);
                package.IncommingData(buffer, 0, count);
            }
            return package.OutgoingPackage();
        }

        private void WriteBuffer(NetworkStream stream, byte[] data)
        {
            WriteBuffer(stream, data, 0, data.Length);
        }
        private void WriteBuffer(NetworkStream stream, byte[] data, int index, int length)
        {
            data = DataPackage.PackData(data.Skip(index).Take(length).ToArray());
            stream.Write(data, 0, data.Length);
        }
    }
}
