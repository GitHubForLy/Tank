using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.UI.Panel
{
    public class ResultPanel : PanelBase
    {
        public Sprite WinImg;
        public Sprite FailImg;
        public Sprite DogFallImg;

        private DataModel.RoomInfo room;
        
        public override void OnInit(params object[] paramaters)
        {
            int isWin = (int)paramaters[0];
            room = (DataModel.RoomInfo)paramaters[1];

            if (isWin == 1)
                GetComponent<Image>().sprite = WinImg;
            else if(isWin==0)
                GetComponent<Image>().sprite= FailImg;
            else if (isWin == -1)
                GetComponent<Image>().sprite = DogFallImg;     //平局
        }

        public void Back()
        {
            Close();
            PanelManager.Instance.OpenPanel<RoomPanel>(room);
        }
    }

}
