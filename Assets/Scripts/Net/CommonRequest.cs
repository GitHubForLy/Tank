using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;
using UnityEngine;
using System.Net.Sockets;
using DataModel;
using JsonFormatter;
using ServerCommon;
using AopCore;

namespace TankGame.Net
{
    /// <summary>
    /// 控制器类型
    /// </summary>
    public class ControllerNames
    {
        public const string Broadcast= "Broadcast";
        public const string Event = "Event";
    }


    /// <summary>
    /// 事件回调函数
    /// </summary>
    public delegate void EventCallback<T>(T Data);

    public class EventList
    {
        private Dictionary<int,Delegate> queue=new Dictionary<int, Delegate>();
        public void Add<T>(int id, EventCallback<T> callback)
        {
            queue.Add(id, callback);
        }

        public bool ContainsId(int id) => queue.ContainsKey(id);

        public EventCallback<T> Get<T>(int id)
        {
            return (EventCallback<T>)queue[id];
        }
        public Delegate Get(int id)
        {
            return queue[id];
        }

        public bool Remove(int id)
        {
            return queue.Remove(id);
        }
    }


    /// <summary>
    /// 请求服务器
    /// </summary>
    public class CommonRequest
    {
        private AsyncTcpClient NetClient;
        private int eventReqId=0;
        private EventList eventList = new EventList();

        private static CommonRequest commonRequest;
        private EndPoint endPoint;
        private JsonDataFormatter formatter;

        /// <summary>
        /// 广播消息队列 
        /// </summary>
        public BroadcastQueue BroadQueue {get;} =new BroadcastQueue();

        /// <summary>
        /// 与服务器连接出错
        /// </summary>
        public event Action<SocketException> OnConnectionError;

        /// <summary>
        /// 待执行的委托列表
        /// </summary>
        public static List<(Delegate callback, IDynamicType res)> Delegates { get; } = new List<(Delegate, IDynamicType)>();


        /// <summary>
        /// 唯一实例
        /// </summary>
        public static CommonRequest Instance
        {
            get
            {
                if (commonRequest == null)
                    commonRequest = new CommonRequest();
                return commonRequest;
            }
        }


        private CommonRequest()
        {
            formatter=new JsonDataFormatter();
            Init();
        }

        private void Init()
        {
            NetClient = new AsyncTcpClient();
            NetClient.OnReceiveData += NetClient_OnReceiveData;
            NetClient.OnConnectException += NetClient_OnConnectException;
        }

        private void NetClient_OnConnectException(object sender, SocketException e)
        {
            OnConnectionError?.Invoke(e);
        }


        /// <summary>
        /// 连接到服务器并开始接收数据
        /// </summary>
        public void Start(EndPoint endPoint)
        {
            this.endPoint = endPoint;
            NetClient.Connect(endPoint);
            NetClient.StartRecive();
        }


        public void Close()
        {
            NetClient.Close();
        }

        /// <summary>
        /// 重新连接服务器并接受数据
        /// </summary>
        public void ReStart()
        {
            if (NetClient.IsConnected)
                NetClient.Close();

            Init();
            Start(endPoint);
        }

        private void NetClient_OnReceiveData(object sender, byte[] data)
        {
            //Respone respone = formatter.Deserialize<Respone>(data);
            var dydata= formatter.DeserializeDynamic(data);
            var contro= dydata.GetChid(nameof(Respone.Controller))?.GetValue<string>();
            var action = dydata.GetChid(nameof(Respone.Action))?.GetValue<string>();
            if (contro == null || action == null)
            {
                Debug.LogError("接收到数据错误");
                return;
            }

            switch (contro)
            {
                case ControllerNames.Broadcast:
                    var sda= dydata.GetChid(nameof(Respone.Data));
                    BroadQueue.Enqueue((action, sda));
                    break;
                case ControllerNames.Event:
                    CallEvent(dydata); 
                    break;
            }
        }


        private void CallEvent(IDynamicType dyanmicData)
        {
            var respone=dyanmicData.GetValue<Respone>();
            var sub = dyanmicData.GetChid(nameof(Respone.Data));/*.GetValue<StandRespone>();*/

            if (eventList.ContainsId(respone.RequestId))
            {
                lock(Delegates)
                {
                    Delegates.Add((eventList.Get(respone.RequestId), sub));
                }
                eventList.Remove(respone.RequestId);
            }
        }


        /// <summary>
        /// 请求消息
        /// </summary>
        /// <param name="ActionName"></param>
        public void DoRequest(string action)
        {
            DoRequest(null, action);
        }

        /// <summary>
        /// 请求消息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ActionName"></param>
        public void DoRequest(object request,string action)
        {
            DoRequest<Type>( request, action, null,null);
        }

        /// <summary>
        /// 使用指定回调函数请求
        /// </summary>
        public void DoRequest<ResType>(object request, string action, EventCallback<ResType> callback)
        {
            DoRequest<ResType>(request, action, callback, null);
        }

        /// <summary>
        /// 使用指定回调函数请求
        /// </summary>
        public void DoRequest<ResType>(object request, string action, Action ComplatedCallback)
        {
            DoRequest<ResType>(request, action, null, ComplatedCallback);
        }

        /// <summary>
        /// 使用指定回调函数请求
        /// </summary>
        public void DoRequest<ResType>(object request, string action, EventCallback<ResType> callback,Action ComplatedCallback)
        {
            Request req = new Request
            { 
                Controller = ControllerNames.Event,
                Action = action,
                Data = request,
                RequestId = ++eventReqId
            };
            if (callback != null)
            {
                eventList.Add(eventReqId, callback);
            }

            NetClient.SendDataAsync(formatter.Serialize(req),ComplatedCallback);
        }


        /// <summary>
        /// 广播推送消息
        /// </summary>
        public void Broadcast(object request, string ActionName)
        {
            Request req = new Request
            {
                Action = ActionName,
                Controller = ControllerNames.Broadcast,
                Data =request
            };

            NetClient.SendDataAsync(formatter.Serialize(req)); 
        }

        /// <summary>
        /// 广播方法的调用
        /// </summary>
        public void BroadcastMethod(MethodExecuteArgs args)
        {
            (string ClassFullName,string MethodName,object[] Parameters) md = (args.Method.DeclaringType.FullName,args.Method.Name,args.ParameterValues);

            Request req = new Request
            {
                Action = nameof(BroadcastMethod),
                Controller = ControllerNames.Broadcast,
                Data = md
            };

            NetClient.SendDataAsync(formatter.Serialize(req));
        }

        /// <summary>
        /// 广播字段复制
        /// </summary>
        public void BroadcastField(FieldUpdateArgs args)
        {

        }
    }
}
