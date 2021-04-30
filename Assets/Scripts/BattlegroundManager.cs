using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.TankBehaviour;
using TankGame.Net;
using ServerCommon;
using System.Runtime.InteropServices;
using TankGame.UI;
using System.Linq;
using TankGame.UI.Panel;
using UnityEngine.UI;
using TankGame.Player;

namespace TankGame
{
    /// <summary>
    /// 这个类只应负责处理开始游戏后的单次对局 
    /// </summary>
    public class BattlegroundManager : MonoBehaviour
    {
        /// <summary>
        /// 所在房间的用户
        /// </summary>
        public static DataModel.RoomUser[] RoomUsers { get;  set; }
        /// <summary>
        /// 所在的房间
        /// </summary>
        public static DataModel.RoomInfo Room { get; set; }

        public GameObject TankPrefab;
        public Transform[] Waypoints;
        public Tank LocalTank{get;set;}


        private bool inputopen = false;
        private bool escapeOpen = false;

        public class Tank
        {
            public int TeamNumber{get;set;}
            public DataModel.RoomUser RoomUser { get; set; }

            // public string Account{get;set;}
            public string UserName { get; set; }

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
        public static float StartTime;

        /// <summary>
        /// 全局坦克列表
        /// </summary>
        public Dictionary<string,Tank> Tanks{get;}=new Dictionary<string, Tank>();

        private CameraFollow cameraFollow;
        private float timescale;

        private void Awake()
        {
            //不需要调用  DontDestroyOnLoad 因为要让这个脚本只在TankScene中保持单例  而不是全局单例
            Instance = this;
            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (!NetManager.Instance.IsLogin)
                return;

            Cursor.visible = false;
            PanelManager.Instance.DefaultCursroMode = CursorLockMode.Locked;
            PanelManager.Instance.DefaultCursorVisble = false;

            Waypoints = GameObject.FindGameObjectsWithTag(Tags.Waypoint).Select(g => g.transform).ToArray();
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            timescale = Time.timeScale;
            TankHealth.OnDie += BattlegroundManager_OnDie;

            StartGame();
        }


        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (!escapeOpen)
                {
                    escapeOpen = true;
                    PanelManager.Instance.OpenPanel<TankGame.UI.Panel.BattleMenuPanel>();
                }
                else
                    escapeOpen = false;

            }
        }

       
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            //Time.timeScale = 0;

            LocalTank.Instance.GetComponent<TankGame.Player.TankGui>().enabled = false;
            LocalTank.Instance.GetComponent<TankGame.Player.PlayerControl>().enabled = false;
            cameraFollow.enabled = false;
        }
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            //Time.timeScale = timescale;
            LocalTank.Instance.GetComponent<TankGame.Player.TankGui>().enabled = true;
            LocalTank.Instance.GetComponent<TankGame.Player.PlayerControl>().enabled = true;
            cameraFollow.enabled = true;
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
                case DataModel.BroadcastActions.Loginout:
                    OnUserLoginOut(msg.subData);
                    break;
            }
        }



        private void OnUserLoginOut(IDynamicType subData)
        {
            var data = subData.GetValue<(string account,string timestamp)>();
            if (data.account != NetManager.Instance.LoginAccount)
            {
                LoginoutTank(data.account);
            }
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
            if (identity.Account == NetManager.Instance.LoginAccount)
                return true;
            return false;
        }


        private void StartGame()
        {
            if(Tanks.ContainsKey(NetManager.Instance.LoginAccount))
            {
                Debug.LogError("已经存在账号:"+ NetManager.Instance.LoginAccount);
                return;
            }

            if(Waypoints.Length>=RoomUsers.Length)
            {
                foreach(var user in RoomUsers)
                {
                   SpawnTank(user, Waypoints[user.Index].position, Waypoints[user.Index].eulerAngles);
                }
            }
            else
            {
                Debug.LogError("没有路点");
            }
        }

        private void BattlegroundManager_OnDie(GameObject deadTank, Behaviour killer)
        {
            deadTank.GetComponent<PlayerControl>().EnableInput = false;
        }


        /// <summary>
        /// 生成坦克
        /// </summary>
        public Tank SpawnTank(DataModel.RoomUser user,Vector3 position,Vector3 rotation)
        {
           
            var account = user.Account;
            var instance = Instantiate(TankPrefab, position, Quaternion.Euler(rotation));
       
            instance.GetComponent<TankTeam>().TeamNumber = user.Team;
            instance.GetComponent<NetIdentity>().Account = account;

            Tank tank = new Tank
            {
                TeamNumber = user.Team,
                Instance = instance,
                RoomUser = user,
                UserName = user.Info.UserName
            };
            if (user.Account == NetManager.Instance.LoginAccount)
            {
                LocalTank = tank;
                cameraFollow.target = instance.transform.Find("camera_follow").gameObject;
            }
            if (Tanks.ContainsKey(account))
            {
                if (Tanks[account].Instance != null)
                    Destroy(Tanks[account].Instance);
                Tanks[account] = tank;
            }
            else
                Tanks.Add(account, tank);

            return tank;
        }


        public void Revive(GameObject deadTank)
        {
            var account = deadTank.GetComponent<NetIdentity>().Account;
            var user = Tanks[account].RoomUser;
            SpawnTank(user, Waypoints[user.Index].position, Waypoints[user.Index].eulerAngles);
        }


        private void OnDestroy()
        {
            NetManager.Instance.OnReceiveBroadcast -= Instance_OnReceiveBroadcast;
        }

    }
}

