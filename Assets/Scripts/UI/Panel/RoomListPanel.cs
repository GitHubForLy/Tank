using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;
using DataModel;

namespace TankGame.UI.Panel
{
    public class RoomListPanel :PanelBase
    {
        public GameObject RoomTitlePrefab;
        public GameObject RoomListContent;

        public void CreateRoom()
        {
            CommonRequest.Instance.DoRequest<StandRespone>(null, EventActions.Login);



            if(RoomListContent!=null && RoomTitlePrefab!=null)
                Instantiate(RoomTitlePrefab, RoomListContent.transform);
        }


        public void JoinRoom()
        {

        }
    }

}

