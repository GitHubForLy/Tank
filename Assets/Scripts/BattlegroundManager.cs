using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.TankBehaviour;
using TankGame.Net;
using ServerCommon;
using System.Runtime.InteropServices;
using TankGame.UI;
using System.Linq;
using UnityEngine.SceneManagement;

namespace TankGame
{
    /// <summary>
    /// 这个类只应负责处理开始游戏后的单次对局 
    /// </summary>
    public class BattlegroundManager : MonoBehaviour
    {

        public string Account{get;set;}
        public GameObject TankPrefab;
        public Transform[] Waypoints;

        public GameObject LocalTank{get;set;}


        public class Tank
        {
            public int TeamNumber{get;set;}

            // public string Account{get;set;}
            public GameObject Instance { get; set; }
            public bool IsDie
            { 
                get { return Instance==null||Instance.GetComponent<TankHealth>().IsDie; } 
            }
        }

        /// <summary>
        /// 战场实例
        /// </summary>
        public static BattlegroundManager Instance { get;private set; }


        /// <summary>
        /// 全局坦克列表
        /// </summary>
        public Dictionary<string,Tank> Tanks{get;}=new Dictionary<string, Tank>();

        private CameraFollow cameraFollow;

        private void Awake()
        {
            //不需要调用  DontDestroyOnLoad 因为要让这个脚本只在TankScene中保持单例  而不是全局单例
            Instance = this;
            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;
        }

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PanelManager.Instance.DefaultCursroMode = CursorLockMode.Locked;
            PanelManager.Instance.DefaultCursorVisble = false;

            Waypoints = GameObject.FindGameObjectsWithTag(Tags.Waypoint).Select(g => g.transform).ToArray();
            cameraFollow = Camera.main.GetComponent<CameraFollow>();

            if (NetManager.Instance.IsLogin)
            {
                StartGame(NetManager.Instance.LoginAccount);
            }
        }

        /// <summary>
        /// 接收到广播消息
        /// </summary>
        private void Instance_OnReceiveBroadcast((string action, IDynamicType subdata) msg)
        {
            foreach (var tank in Tanks.Values)
            {
                var nets = tank.Instance.GetComponents<NetBehaviour>();
                foreach (var net in nets)
                {
                    net.OnBroadcast(msg);
                }
            }

            //全局处理
            DoHandle(msg);
        }


        private void DoHandle((string action, IDynamicType subData) msg)
        {
            switch(msg.action)
            {
                case DataModel.BroadcastActions.Login:
                    OnPlayerLogin(msg.subData);
                    break;
                case DataModel.BroadcastActions.Loginout:
                    OnUserLoginOut(msg.subData);
                    break;
            }
        }

        private void OnUserLoginOut(IDynamicType subData)
        {
            var data = subData.GetValue<(string account,string timestamp)>();
            if(NetManager.Instance.IsLogin)
            {
                Debug.Log("logout:  account:" + data.account+" time:"+data.timestamp);
                if (data.account == this.Account)
                {
                    //被挤下来
                    if (data.timestamp == NetManager.Instance.LoginTimestamp)
                    {
                        PanelManager.Instance.ShowMessageBox("你的账号已于别处登录",res=> 
                        {
                            NetManager.Instance.IsLogin = false;
                            SceneManager.LoadScene("StartScene");
                        });
                    }
                }
                else
                    LoginoutTank(data.account); 
            }
        }


        private void OnPlayerLogin(IDynamicType data)
        {
            var info = data.GetValue<DataModel.LoginInfo>();
            Debug.Log("login:" + info.Account);
            if (info.Account == this.Account)
                return;
            var wayindex = Random.Range(0, Waypoints.Length);
            SpawnTank(info.Account, Waypoints[wayindex].position,Waypoints[wayindex].eulerAngles);
        }


        public void LoginoutTank(string account)
        {
            if(Tanks.ContainsKey(account))
            {
                Destroy(Tanks[account].Instance);
                Tanks.Remove(account);
            }
        }

        /// <summary>
        /// 是否本地玩家
        /// </summary>
        public bool IsLocalPlayer(NetBehaviour behaviour)
        {
            var identity=behaviour.GetComponent<NetIdentity>();
            if (identity == null)
                return true;
            if (identity.Account == this.Account)
                return true;
            return false;
        }


        private void StartGame(string account)
        {
            if(Tanks.ContainsKey(account))
            {
                Debug.LogError("已经存在账号:"+account);
                return;
            }

            Account = account;
            if(Waypoints.Length>0)
            {
                var wayindex = Random.Range(0, Waypoints.Length);
                var tank = SpawnTank(account, Waypoints[wayindex].position, Waypoints[wayindex].eulerAngles);
                cameraFollow.target = tank.transform.Find("camera_follow").gameObject;
                LocalTank = tank;
                DataModel.LoginInfo info = new DataModel.LoginInfo
                {
                    Account = account,
                    WaypointIndex = wayindex
                };

                CommonRequest.Instance.Broadcast(info, DataModel.BroadcastActions.Login);
            }
            else
            {
                Debug.LogError("没有路点");
            }

            GetServerTanks();
        }

        void GetServerTanks()
        {
            CommonRequest.Instance.DoRequest<List<(string account,DataModel.Transform tran)>>(null, DataModel.EventActions.GetPlayerTransforms,(data)=> 
            { 
                foreach(var ptran in data)
                {
                    if (ptran.account != this.Account)
                    {
                        SpawnTank(ptran.account, ptran.tran.Position.ToUnityVector(), ptran.tran.Rotation.ToUnityVector());
                    }
                }         
            });
        }

        /// <summary>
        /// 生成坦克
        /// </summary>
        private GameObject SpawnTank(string account,Vector3 position,Vector3 rotation)
        {
            if (Tanks.ContainsKey(account))
                return Tanks[account].Instance;

            var instance = Instantiate(TankPrefab, position, Quaternion.Euler(rotation));

            Tank tank = new Tank
            {
                TeamNumber = Random.Range(0, 100),
                Instance = instance
            };
            Tanks.Add(account, tank);

            instance.GetComponent<NetIdentity>().Account = account;
            return instance;
        }

    }
}

