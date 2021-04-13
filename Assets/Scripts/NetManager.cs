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

namespace TankGame
{
    public class NetManager : MonoBehaviour
    {
        public string ServerHost = "localhost";   
        public short ServerPort = 4789;  
        public int ServerReconnectSceonds = 10;

        public static NetManager Instance { get; private set; }
        public string LoginAccount { get; set; }
        public string Password { get; set; }
        public string LoginTimestamp { get; private set; }
        public bool IsLogin { get; set; } = false;


        private bool IsNeedReconnect = false;
        private bool isReConnecting = false;


        [DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag,int dwReserved);


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            var end = new IPEndPoint(Dns.GetHostAddresses(ServerHost)[0], ServerPort);
            CommonRequest.Instance.OnConnectionError += Instance_OnConnectionError;
            CommonRequest.Instance.Start(end);
            int flag=0; 
            if (!InternetGetConnectedState(ref flag, 0))
                PanelManager.Instance.ShowMessageBox("未连接到网络，请连接到网络后重试");

            Application.logMessageReceived += Application_logMessageReceived;
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
        }


        //调用网络请求回调方法
        private void InvokeEventDelegate()
        {
            lock (CommonRequest.Delegates)
            {
                foreach (var dega in CommonRequest.Delegates)
                {
                    var s= dega.callback.Method.GetParameters()[0];
                    //dega.callback(dega.res);
                    dega.callback.DynamicInvoke(dega.res.GetValue(s.ParameterType));
                }
                CommonRequest.Delegates.Clear();
            }
        }


        public void Login(string account,string password,Action<StandRespone> callback)
        {
            LoginTimestamp = GetTimestamp();
            LoginRequest loginRequest = new LoginRequest
            {
                UserName = account,
                Password = password
            };
            CommonRequest.Instance.DoRequest<StandRespone>(loginRequest, EventActions.Login, (res) =>
            {
                if (res.IsSuccess)
                {
                    LoginAccount = account;
                    Password = password;
                    IsLogin = true;
                    LoginTimestamp = res.Message.Split('|')[1];
                }
                callback?.Invoke(res);
            });
        }

        private string GetTimestamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
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
}


