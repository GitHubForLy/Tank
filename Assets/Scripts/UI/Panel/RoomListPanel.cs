using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;
using DataModel;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

namespace TankGame.UI.Panel
{
    public class RoomListPanel :PanelBase
    {
        public GameObject RoomTitlePrefab;
        public GameObject RoomListContent;
        public string RoomId_Name = "RoomID";
        public string RoomState_Name = "Status";
        public string RoomUserCount_Name = "UserCount";
        public string RoomName = "RoomName";

        private Dictionary<RoomInfo,GameObject> Rooms = new Dictionary<RoomInfo, GameObject>();
        private RoomInfo selected;

        public override void OnInit(params object[] paramaters)
        {
            base.OnInit();
            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;
            CommonRequest.Instance.DoRequest<RoomInfo[]>(null, EventActions.GetRoomList,res=> 
            {
                foreach (var rom in res)
                {
                    InstantiateRoomTitle(rom);
                }
                if (Rooms.Count > 0)
                {
                    RoomTitle_OnPointerDown(Rooms.First().Value);
                }
            });
        }


        private void Instance_OnReceiveBroadcast((string action, ServerCommon.IDynamicType subdata) msg)
        {
            if(msg.action==BroadcastActions.CreateRoom)
            {
                RoomInfo data = msg.subdata.GetValue<RoomInfo>();
                InstantiateRoomTitle(data);
            }
            else if(msg.action==BroadcastActions.LeaveRoom)
            {
                RoomInfo data = msg.subdata.GetValue<RoomInfo>();
                if(Rooms.ContainsKey(data)) //不包含说明时自己退出房间 到这个面板时才收到消息
                {
                    if (data.UserCount <= 0)
                    {
                        if (selected != null && data.Equals(selected))
                            selected = null;
                        Rooms.Remove(data);
                        Destroy(GetRoomTitleForId(data.RoomId));
                    }
                    else
                        SetRoomInfo(GetRoomTitleForId(data.RoomId), data);
                }
            }
            else if(msg.action==BroadcastActions.JoinRoom)
            {
                RoomInfo data = msg.subdata.GetValue<RoomInfo>();
                SetRoomInfo(GetRoomTitleForId(data.RoomId), data);
            }
        }


        private GameObject GetRoomTitleForId(int roomid)
        {
            return RoomListContent.transform.Find("Room_" + roomid.ToString()).gameObject;
        }

        private GameObject InstantiateRoomTitle(RoomInfo rom)
        {
            var titlepanel = Instantiate(RoomTitlePrefab, RoomListContent.transform);
            titlepanel.name = "Room_" + rom.RoomId.ToString();
            titlepanel.GetComponent<RoomTitle>().OnPointerDown += RoomTitle_OnPointerDown;
            titlepanel.GetComponent<RoomTitle>().Room = rom;

            SetRoomInfo(titlepanel,rom);
            Rooms.Add(rom,titlepanel);
            return titlepanel;
        }

        private void RoomTitle_OnPointerDown(GameObject obj)
        {
            if (selected != null)
            {
                if (Rooms[selected] == obj)
                    return;

                Rooms[selected].GetComponent<Image>().color = new Color(1, 1, 1, 100f / 255);
            }

            obj.GetComponent<Image>().color = new Color(54f / 255, 188f / 255, 243f / 255);
            selected = obj.GetComponent<RoomTitle>().Room;
            
        }

        /// <summary>
        /// 设置房间信息
        /// </summary>
        public void SetRoomInfo(GameObject romobj,RoomInfo rom)
        {
            romobj.transform.Find(RoomId_Name).GetComponent<Text>().text = rom.RoomId.ToString();
            romobj.transform.Find(RoomState_Name).GetComponent<Text>().text = rom.State == 0 ? "准备中" : "已开始";
            romobj.transform.Find(RoomUserCount_Name).GetComponent<Text>().text = rom.UserCount + "/" + rom.MaxCount;
            romobj.transform.Find(RoomName).GetComponent<Text>().text = rom.Name;
        }


        public void CreateRoom()
        {
            CommonRequest.Instance.DoRequest<StandRespone<int>>("坦克大战房间", EventActions.CreateRoom,res=> 
            { 
                if(res.IsSuccess)
                {
                    Close();
                    PanelManager.Instance.OpenPanel<RoomPanel>(res.Data,true);                  
                }
                else
                {
                    PanelManager.Instance.ShowMessageBox("创建房间失败:" + res.Message);
                }
            });
        }

        public void JoinRoom()
        {
            if (selected == null)
                return;
            if (selected.State != RoomState.Waiting)
                return;
            if (selected.UserCount >= selected.MaxCount)
                return;

            CommonRequest.Instance.DoRequest<StandRespone<int>>(selected.RoomId, EventActions.JoinRoom, res =>
            {
                if (res.IsSuccess)
                {
                    Close();
                    PanelManager.Instance.OpenPanel<RoomPanel>(res.Data,false);
                }
                else
                    PanelManager.Instance.ShowMessageBox("加入房间失败:" + res.Message);
            });
        }
        public override void OnClosed()
        {
            NetManager.Instance.OnReceiveBroadcast -= Instance_OnReceiveBroadcast;
        }

        public override void OnEscape()
        {
            PanelManager.Instance.OpenPanel<MainMenuPanel>();
        }
    }

}

