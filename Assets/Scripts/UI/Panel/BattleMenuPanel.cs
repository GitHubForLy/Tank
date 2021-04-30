using DataModel;
using System.Collections;
using System.Collections.Generic;
using TankGame.Net;
using UnityEngine;

namespace TankGame.UI.Panel
{
    public class BattleMenuPanel : PanelBase
    {
        private float lastTimescale;
        public override void OnInit(params object[] paramaters)
        {
            base.OnInit();
            BattlegroundManager.Instance?.PauseGame();
        }
        public override void OnClosed()
        {
            base.OnClosed();
            BattlegroundManager.Instance?.ResumeGame();
        }

        public void LeaveRoom()
        {
            PanelManager.Instance.ShowMessageBox("确定要退出战场吗?", MessageBoxButtons.OkCancel, btn =>
            {
                if(btn == MessageBoxResult.Ok)
                {
                    CommonRequest.Instance.DoRequest<StandRespone>(null, EventActions.LeaveRoom, res =>
                    {
                        if (res.IsSuccess)
                        {
                            GameController.LoadScene<RoomListPanel>("StartScene");
                        }
                        else
                            PanelManager.Instance.ShowMessageBox("退出失败:" + res.Message);
                    });
                }
            });
        }
        public void Resume()
        {
            Close();
        }
    }
}

