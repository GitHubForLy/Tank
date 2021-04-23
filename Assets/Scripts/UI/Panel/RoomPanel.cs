using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;
using DataModel;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;

namespace TankGame.UI.Panel
{
    public class RoomPanel : PanelBase
    {
        public GameObject UserCardPrefab;
        public GameObject MsgCardPrefab;
        [Space(10)]
        public GameObject ReadyButton;
        public GameObject[] UserCardSockets;
        public GameObject MsgContent;
        public Scrollbar MsgScrollbar;
        [Space(10)]
        public string UserNameText = "UserName";
        public string UserStateText = "UserState";
        public string JoinTipText = "joinTip";



        private RoomUser Self;
        private bool IsOwner;
        private int roomID;
        private Dictionary<RoomUser, GameObject> users = new Dictionary<RoomUser, GameObject>();

        public override void OnInit(params object[] paramaters)
        {
            roomID = (int)paramaters[0];
            IsOwner= (bool)paramaters[1];

            if (IsOwner)
                ReadyButton.transform.GetChild(0).GetComponent<Text>().text = "开始游戏";
            else
                ReadyButton.transform.GetChild(0).GetComponent<Text>().text = "准备";

            InitUsers();

            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;

            for (int id=0;id<UserCardSockets.Length; id++)
            {
                int index = id;
                UserCardSockets[id].GetComponent<Button>().onClick.AddListener(()=>OnUserCardSocketClick(index));
            }

        }

        private void InitUsers()
        {
            CommonRequest.Instance.DoRequest<RoomUser[]>(roomID, EventActions.GetRoomUsers, res =>
            {
                foreach (var ru in res)
                {
                    if (ru.Account == NetManager.Instance.LoginAccount)
                        Self = ru;
                    var obj = InstantiateUserCard(ru);
                    SetCardInfo(obj, ru);
                    users.Add(ru, obj);
                }
            });
        }

        private void Instance_OnReceiveBroadcast((string action, ServerCommon.IDynamicType subdata) msg)
        {
            if (msg.action == BroadcastActions.RoomChange)
            {
                var user = msg.subdata.GetValue<RoomUser>();
                switch (user.LastOpeartion)
                {
                    case RoomUser.RoomOpeartion.Leave:
                        UserLeave(user);
                        break;
                    case RoomUser.RoomOpeartion.Join:
                        UserJoin(user);
                        break;
                    case RoomUser.RoomOpeartion.Ready:
                        UserReady(user);
                        break;
                    case RoomUser.RoomOpeartion.CancelReady:
                        UserCancelReady(user);
                        break;
                    case RoomUser.RoomOpeartion.ChangeIndex:
                        UserChangeIndex(user);
                        break;
                }
            }
            else if (msg.action == BroadcastActions.BroadcastRoomMsg)
            {
                (string account, string msg) data = msg.subdata.GetValue<(string, string)>();
                if (data.account != NetManager.Instance.LoginAccount)
                    AppendMessage(data.account, data.msg);
            }
            else if (msg.action == BroadcastActions.DoStartFight)
                StartFight();
        }

        private void UserLeave(RoomUser user)
        {
            if(users.ContainsKey(user))
            {
                Destroy(users[user]);
                users.Remove(user);
            }
        }

        private void UserJoin(RoomUser user)
        {
            if (user.Account != NetManager.Instance.LoginAccount)
            {
                var obj = InstantiateUserCard(user);
                SetCardInfo(obj, user);
                users.Add(user, obj);
            }
        }
        private void UserReady(RoomUser user)
        {
            if(users.ContainsKey(user))
            {
                if (user != Self)
                {
                    SetCardInfo(users[user], user);
                    users.Keys.First(m => m == user).State = user.State;
                }
            }
        }

        private void UserCancelReady(RoomUser user)
        {
            if (users.ContainsKey(user))
            {
                if (user != Self)
                {
                    SetCardInfo(users[user], user);
                    users.Keys.First(m => m == user).State = user.State;
                }
            }
        }

        private void UserChangeIndex(RoomUser user)
        {
            if (users.ContainsKey(user))
            {
                if (user != Self)
                {
                    if (users.ContainsKey(user))
                    {
                        int oldindex = users.Keys.First(m => m.Account == user.Account).Index;
                        ChangeIndex(user, oldindex, user.Index);
                    }
                }
            }
        }

        private GameObject InstantiateUserCard(RoomUser user)
        {
            if(UserCardSockets.Length<=user.Index)
            {
                Debug.LogError("没有找到对应的用户槽");
                return null;
            }
            var cardp = UserCardSockets[user.Index];
            cardp.transform.Find(JoinTipText).gameObject.SetActive(false);
            var card = Instantiate(UserCardPrefab, cardp.transform);
            SetCardInfo(card, user);

            return card;
        }
       
        private void SetCardInfo(GameObject card,RoomUser user)
        {
            card.transform.Find(UserNameText).GetComponent<Text>().text = user.Account;
            if (user.IsRoomOwner)
                card.transform.Find(UserStateText).GetComponent<Text>().text = "准备";
            else
                card.transform.Find(UserStateText).GetComponent<Text>().text = user.State == RoomUserStates.Ready ? "准备" : "";
        }


        public void LeaveRoom()
        {
            CommonRequest.Instance.DoRequest<StandRespone>(null,EventActions.LeaveRoom,res=> 
            {
                if (res.IsSuccess)
                {
                    Close();
                    PanelManager.Instance.OpenPanel<RoomListPanel>();
                }
                else
                    PanelManager.Instance.ShowMessageBox("退出失败:" + res.Message);
            });
        }


        public void OnReadyButtonClick()
        {
            if(IsOwner)
            {
                //开始游戏
                DoStartFight();
            }
            else
            {
                if (Self.State == RoomUserStates.Waiting)
                    DoReady();
                else if (Self.State == RoomUserStates.Ready)
                    DoCancelReady();
            }
        }

        private void DoReady()
        {
            CommonRequest.Instance.DoRequest<StandRespone>(null, EventActions.RoomReady, res =>
            {
                if (res.IsSuccess)
                {
                    ReadyButton.transform.GetChild(0).GetComponent<Text>().text = "取消准备";
                    Self.State = RoomUserStates.Ready;
                    SetCardInfo(users[Self], Self);
                }
                else
                {
                    PanelManager.Instance.ShowMessageBox("准备失败:" + res.Message);
                }
            });
        }
        private void DoCancelReady()
        {
            CommonRequest.Instance.DoRequest<StandRespone>(null, EventActions.RoomCancelReady, res =>
            {
                if (res.IsSuccess)
                {
                    ReadyButton.transform.GetChild(0).GetComponent<Text>().text = "准备";
                    Self.State = RoomUserStates.Waiting;
                    SetCardInfo(users[Self], Self);
                }
                else
                {
                    PanelManager.Instance.ShowMessageBox("取消准备失败:" + res.Message);
                }
            });
        }

        /// <summary>
        /// 用户槽点击事件
        /// </summary>
        /// <param name="Index">目标索引</param>
        public void OnUserCardSocketClick(int Index)
        {
            if (users.Any(m => m.Key.Index == Index))
                return;
            CommonRequest.Instance.DoRequest<StandRespone>(Index, EventActions.RoomChangeIndex, res =>
            {
                if (res.IsSuccess)
                {
                    ChangeIndex(Self,Self.Index,Index);
                }
            });
        }

        /// <summary>
        /// 更改用户的位置
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="index">新位置</param>
        private void ChangeIndex(RoomUser user,int oldIndex, int index)
        {
            users.Keys.First(m => m.Index == oldIndex).Index = index;

            UserCardSockets[oldIndex].transform.Find(JoinTipText).gameObject.SetActive(true);
            var cardp = UserCardSockets[index];
            cardp.transform.Find(JoinTipText).gameObject.SetActive(false);
            users[user].transform.SetParent(cardp.transform);
        }

        public override void OnEscape()
        {
            if(Self.State== RoomUserStates.Ready && !IsOwner)
            {
                OnReadyButtonClick();
                return;
            }

            PanelManager.Instance.ShowMessageBox("确定要退出房间吗？", MessageBoxButtons.OkCancel, res =>
            {
                if(res==MessageBoxResult.Ok)
                {
                    LeaveRoom();
                }
            });
        }

        /// <summary>
        /// 消息框输入消息
        /// </summary>
        public override void OnEnter()
        {
            InputPanel.ShowInput(message =>
            {
                AppendMessage(NetManager.Instance.LoginAccount,message);
                CommonRequest.Instance.Broadcast(message, BroadcastActions.BroadcastRoomMsg);
            });
        }

        public void AppendMessage(string account,string message)
        {
            var card = Instantiate(MsgCardPrefab, MsgContent.transform);
            card.transform.GetChild(0).GetComponent<Text>().text = account + ":"+message;
            StartCoroutine(InsSrollBar());
        }

        /// <summary>
        /// 滚动滑动条到底部
        /// </summary>
        IEnumerator InsSrollBar()
        {
            yield return new WaitForEndOfFrame();
            MsgScrollbar.value = 0;
        }

        private void DoStartFight()
        {
            if(users.Any(m=>m.Key.State!= RoomUserStates.Ready))
            {
                PanelManager.Instance.ShowMessageBox("尚有人未准备 无法开始");
                return;
            }
            CommonRequest.Instance.DoRequest<StandRespone>(Time.time, EventActions.DoStartFight, res =>
            {
                if (!res.IsSuccess)
                    PanelManager.Instance.ShowMessageBox("无法开始:" + res.Message);
            });
        }

        private void StartFight()
        {
            //这里直接采用房间的用户列表信息 也可以由StartFight返回用户列表
            BattlegroundManager.RoomUsers = users.Keys.ToArray();
            SceneManager.LoadScene("TankScene");
        }

    }
}
