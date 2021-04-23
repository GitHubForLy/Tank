using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;
using System.Net;
using System;
using System.Net.Sockets;
using UnityEditor;
using TankGame.UI;
using System.Runtime.InteropServices;
using DataModel;
using ServerCommon;
using UnityEngine.SceneManagement;
using TankGame.UI.Panel;

namespace TankGame
{
    public delegate void BroadcastEventHandle((string action, IDynamicType subdata) msg);

    public class NetManager : MonoBehaviour
    {
        private Dictionary<string, Dictionary<string, Type>> ReceiveTyps;
        public string ServerHost = "localhost";   
        public short ServerPort = 4789;  
        public int ServerReconnectSceonds = 10;

        public static NetManager Instance { get; private set; }
        public string LoginAccount { get; set; }
        public string Password { get; set; }
        public string LoginTimestamp { get; private set; }
        public bool IsLogin { get; private set; } = false;
        /// <summary>
        /// 与服务器的单次延迟 单位秒
        /// </summary>
        public float TimeDelay { get; private set; }
        /// <summary>
        /// 与服务器的时差 比服务器时间大则正数 否则负数  单位秒
        /// </summary>
        public float ServerTimeDiff { get; private set; }
        /// <summary>
        /// 当前服务器的时间戳(低精度)
        /// </summary>
        public double ServerTime => GetServerTime();

        /// <summary>
        /// 接收到广播消息时
        /// </summary>
        public event BroadcastEventHandle OnReceiveBroadcast;


        private bool IsNeedReconnect = false;
        private bool isReConnecting = false;


        [DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag,int dwReserved);


        private void Awake()
        {
            if (Instance != null && Instance == this)
                return;
            Debug.Log("net awake, instance?" + (Instance == null));
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Init()
        {
            Debug.Log("init");
            var end = new IPEndPoint(Dns.GetHostAddresses(ServerHost)[0], ServerPort);
            CommonRequest.Instance.OnConnectionError += Instance_OnConnectionError;
            CommonRequest.Instance.Start(end);
            int flag = 0;
            if (!InternetGetConnectedState(ref flag, 0))
                PanelManager.Instance.ShowMessageBox("未连接到网络，请连接到网络后重试");

            Application.logMessageReceived += Application_logMessageReceived;

            StartCoroutine(CheckTime());
        }

        private IEnumerator CheckTime()
        {
            var time = GetTime();
            CommonRequest.Instance.DoRequest<double>(null, EventActions.CheckTime, res =>
            {
                var ctime = GetTime();
                var delay = (ctime - time) / 2;     //与服务器延迟粗略的计算为 请求来回行程时间的一半
                TimeDelay = (float)delay; 

                var servertime = res + delay;  //此时的服务器时间=返回的服务器时间+单向延迟
                ServerTimeDiff = (float)(ctime - servertime);    //与服务器的时差=当前时间-服务器时间
            });
            yield return new WaitForSeconds(2.5f);
            StartCoroutine(CheckTime());
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Assert || type == LogType.Exception || type == LogType.Error)
                TankGame.UI.PanelManager.Instance.ShowMessageBox(type.ToString() + "  " + condition + " strace:" + stackTrace);
        }

        private void Instance_OnConnectionError(SocketException exception)
        {
            IsNeedReconnect = true;
        }


        private IEnumerator Reconnect()
        {
            PanelManager.Instance.ShowMessageBox($"连接出错将在{ServerReconnectSceonds}秒后重连");
            //EditorUtility.DisplayDialog("提示", $"连接出错将在{ServerReconnectSceonds}秒后重连", "确认");
            isReConnecting = true;
            for (int i = ServerReconnectSceonds; i >= 0; i--)
            {
                if (CommonRequest.Instance.IsConnected)
                    break;
                Debug.LogWarning($"连接出错,将在{i}秒后重连");
                yield return new WaitForSeconds(1);
            }
            Debug.LogWarning($"对服务器进行重连");
            CommonRequest.Instance.ReStart();
            isReConnecting = false;

            if (IsLogin)
                ReLogin();
        
        }



        private void Update()
        {
            if (IsNeedReconnect && !isReConnecting)
                StartCoroutine(Reconnect());
            IsNeedReconnect = false;

            InvokeEventDelegate();
            InvokeBroadcastMethod();
        }

        /// <summary>
        /// 广播消息
        /// </summary>
        private void InvokeBroadcastMethod()
        {
            while (CommonRequest.Instance.BroadQueue.Count > 0)
            {
                var msg = CommonRequest.Instance.BroadQueue.Dequeue();

                DoHandle(msg);
                //调用事件

                OnReceiveBroadcast?.Invoke(msg);
            }
        }

        private void DoHandle((string action, IDynamicType subData) msg)
        {
            if(msg.action==BroadcastActions.Loginout)
            {
                OnUserLoginOut(msg.subData);
            }
        }

        /// <summary>
        /// 处理用户登出
        /// </summary>
        private void OnUserLoginOut(IDynamicType subData)
        {
            var data = subData.GetValue<(string account, string timestamp)>();
            if (IsLogin)
            {
                if (data.account == LoginAccount)
                {
                    //被挤下来
                    if (data.timestamp == LoginTimestamp)
                    {
                        Debug.Log("被挤下来了");
                        PanelManager.Instance.ShowMessageBox("你的账号已于别处登录", MessageBoxButtons.Ok, res =>
                        {
                            IsLogin = false;
                            if(SceneManager.GetActiveScene().name=="StartScene")
                            {
                                PanelManager.Instance.CloseAll();
                                PanelManager.Instance.OpenPanel<LoginPanel>();
                            }
                            else
                                SceneManager.LoadScene("StartScene");
                        });
                    }
                }
            }
        }




        //调用网络请求回调方法
        private void InvokeEventDelegate()
        {
            lock (CommonRequest.Delegates)
            {
                for(int i=CommonRequest.Delegates.Count-1;i>=0;i--)
                {
                    var dega = CommonRequest.Delegates[i];
                    CommonRequest.Delegates.RemoveAt(i);
                    try
                    {
                        var parameter = dega.callback.Method.GetParameters()[0];
                        //dega.callback(dega.res);
                        dega.callback.DynamicInvoke(dega.res.GetValue(parameter.ParameterType));                        
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("event 回调方法出错:" + e.Message);
                    }
                }            
            }
        }


        public void Login(string account,string password,Action<StandRespone> callback)
        {
            LoginRequest loginRequest = new LoginRequest
            {
                UserName = account,
                Password = password
            };
            CommonRequest.Instance.DoRequest<StandRespone<string>>(loginRequest, EventActions.Login, (res) =>
            {
                if (res.IsSuccess)
                {
                    LoginAccount = account;
                    Password = password;
                    IsLogin = true;
                    LoginTimestamp = res.Data;
                }
                callback?.Invoke(res);
            });
        }

        public void ReLogin()
        {
            if (!IsLogin)
                return;

            Login(LoginAccount, Password, res =>
            {
                if (!res.IsSuccess)
                {
                    PanelManager.Instance.ShowMessageBox("重新登录失败:" + res.Message);
                }
            });         
        }


        void OnDestroy()
        {
            if(Instance==this)
            {
                Debug.Log("NetManager Destroy");
                if (!IsLogin)
                {
                    CommonRequest.Instance.DoRequest<int>(null, DataModel.EventActions.Loginout, () =>
                    {
                        print("client closeing");
                        CommonRequest.Instance.Close();
                    });
                }
                else
                {
                    print("client closeing");
                    CommonRequest.Instance.Close();
                }
            }           
        }


        public static double GetTime()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ts.TotalSeconds;
        }

        public  double GetServerTime()
        {
             return GetTime() - ServerTimeDiff;
        }
    }
}


