using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TankGame.Net
{
    public delegate void ReceiveDataEventHandle(object sender, byte[] data);

    public class ConnectionException : SocketException
    {
        public ConnectionException()
        {
        }
        public ConnectionException(SocketError error):base((int)error)
        {
        }
    }



    internal class AsyncTcpClient
    {
        private Socket socket;
        private SocketAsyncEventArgs sendarg;
        private SocketAsyncEventArgs receiveArg;
        private byte[] recvBuff;
        private DataPackage dataPackage;
        private bool isReceiveing = false;


        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => socket.Connected;

        /// <summary>
        /// 收到数据响应
        /// </summary>
        public event ReceiveDataEventHandle OnReceiveData;

        /// <summary>
        /// 是否正在异步发送中
        /// </summary>
        public bool IsAsyncSending { get; private set; }

        /// <summary>
        /// 连接异常
        /// </summary>
        public event EventHandler<SocketException> OnConnectException;

        /// <summary>
        /// 心跳间隔 毫秒 默认4000毫秒
        /// </summary>
        public int HeartInteval { get; set; } = 4000;

        public AsyncTcpClient(int recvBuffSize=1024)
        {
            dataPackage = new DataPackage();
            recvBuff = new byte[recvBuffSize];
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sendarg = new SocketAsyncEventArgs();
            receiveArg = new SocketAsyncEventArgs();
            sendarg.Completed += SendAsync_Completed;
            receiveArg.SetBuffer(recvBuff, 0, recvBuffSize);
            receiveArg.Completed += ReceiveArg_Completed;
            IsAsyncSending = false;
        }

        private void ReceiveArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success&& e.BytesTransferred>0)
            {
                try
                {
                    dataPackage.IncommingData(e.Buffer, e.Offset, e.BytesTransferred);
                    while (dataPackage.OutgoingPackage(out byte[] data))
                    {
                        OnReceiveData?.Invoke(this, data);
                    }  
                }
                catch(Exception er)
                {
                    UnityEngine.Debug.LogError("receive handle error:" + er.ToString());
                }

                if (!socket.ReceiveAsync(e))
                    ReceiveArg_Completed(this, e);
            }
            else
            { 
                UnityEngine.Debug.LogWarning("receive SocketError:" + e.SocketError.ToString()+" BytesTransferred:"+e.BytesTransferred);
                dataPackage.Clear(); 
                OnConnectException?.Invoke(this, new ConnectionException(e.SocketError));
            }
        }

        private void SendAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError!=SocketError.Success && e.SocketError!=SocketError.WouldBlock)
            {
                //出错
                dataPackage.Clear();
                OnConnectException?.Invoke(this, new ConnectionException(e.SocketError));
            }
            IsAsyncSending = false;
            (e.UserToken as Action)?.Invoke();
        }

        /// <summary>
        /// 连接到指定服务器
        /// </summary>
        public void Connect(EndPoint endPoint)
        {
            if (IsConnected)
                return;

            try
            {
                socket.Connect(endPoint);
            }
            catch(SocketException e)
            {
                OnConnectException?.Invoke(this, e);
            }
            //Task.Factory.StartNew(HeartWork);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if(IsConnected)
            socket.Close();
        }

        ///// <summary>
        ///// 异步连接到指定服务器
        ///// </summary>
        //public Task ConnectAsync(EndPoint endPoint)
        //{
        //    if (IsConnected)
        //        return Task.CompletedTask;

        //    return socket.ConnectAsync(endPoint);
        //}

        /// <summary>
        /// 开始接收数据 接收到完整数据包将触发<see cref="NetCore.AsyncTcpClient.OnReceiveData"/>事件
        /// </summary>
        public void StartRecive()
        {
            if (isReceiveing)
                return;
            if (!socket.ReceiveAsync(receiveArg))
                ReceiveArg_Completed(this, receiveArg);
            isReceiveing = true;
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        public void SendDataAsync(byte[] data)
        {
            SendDataAsync(data, null);
        }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        public void SendDataAsync(byte[] data,Action sendcomplated)
        {
            //if (IsAsyncSending)
            //{
            //    UnityEngine.Debug.Log("error: ending");
            //    return;
            //}
            //IsAsyncSending = true;
            //var pkdata = DataPackage.PackData(data);
            //sendarg.SetBuffer(pkdata, 0, pkdata.Length);
            //sendarg.UserToken = sendcomplated;
            //if (!socket.SendAsync(sendarg))
            //{
            //    SendAsync_Completed(this, sendarg);
            //    UnityEngine.Debug.Log("同步完成");
            //}

            var pkdata = DataPackage.PackData(data);
            socket.BeginSend(pkdata, 0, pkdata.Length, SocketFlags.None, SendComplated, sendcomplated);

        }

        private void SendComplated(IAsyncResult result)
        {
            (result.AsyncState as Action)?.Invoke();

            int res= socket.EndSend(result,out SocketError error);
            if (error != SocketError.Success && error != SocketError.WouldBlock)
            {
                //出错
                dataPackage.Clear();
                OnConnectException?.Invoke(this, new ConnectionException(error));
            }
        }


        /// <summary>
        /// 异步发送数据
        /// </summary>
        public void SendData(byte[] data, Action sendcomplated)
        {

            var pkdata = DataPackage.PackData(data);
            sendarg.SetBuffer(pkdata, 0, pkdata.Length);
            sendarg.UserToken = sendcomplated;
            int count = socket.Send(pkdata);
            if (count == pkdata.Length)
                SendAsync_Completed(this, sendarg);
            else
                OnConnectException?.Invoke(this, new SocketException(count));
        }



        private async void HeartWork()
        {
            var data= Encoding.UTF8.GetBytes("heart");
            var pdata= DataPackage.PackData(data);
            while (IsConnected)
            {
                try
                {
                    socket.Send(pdata);
                }
                catch(SocketException e)
                {
                    OnConnectException?.Invoke(this, e);
                }

                DateTime now = DateTime.Now;
                while((DateTime.Now-now).TotalMilliseconds<HeartInteval)
                {
                    if (!IsConnected)
                        return;
                    await Task.Delay(200);
                }
            }
        }

    }
}
