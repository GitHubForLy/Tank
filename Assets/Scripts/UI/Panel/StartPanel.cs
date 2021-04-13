using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TankGame.UI.Panel
{
    public class StartPanel : PanelBase
    {
        protected override void OnInit()
        {
            base.OnInit();
        }

        public void OnStart()
        {
            //Battleground.Instance.SpawnTank();
            PanelManager.Instance.ClosePanel(this);
        }
    }

}
