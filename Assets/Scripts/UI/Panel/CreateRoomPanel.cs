using DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using TankGame.Net;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.UI.Panel
{
    public class CreateRoomPanel : PanelBase
    {
        [SerializeField]
        private InputField RoomName;
        [SerializeField]
        private Dropdown FightMode;
        [SerializeField]
        private Dropdown TargetKillCount;
        [SerializeField]
        private Toggle IsPassword;
        [SerializeField]
        private InputField Password;


        public void Ok()
        {
            if(!CheckInput(out string msg))
            {
                PanelManager.Instance.ShowMessageBox(msg);
                return;
            }

            RoomSetting setting = new RoomSetting
            {
                Mode = FightMode.value == 0 ? DataModel.FightMode.KillCount : DataModel.FightMode.Time,
                MaxTime = 20 * 60,
                RoomName = RoomName.text,
                TargetKillCount =int.Parse(TargetKillCount.options[TargetKillCount.value].text),
                HasPassword=IsPassword.isOn,
                Password=Password.text
            };

            var lod = PanelManager.Instance.OpenPanel<LoadingPanel>();
            CommonRequest.Instance.DoRequest<StandRespone<RoomInfo>>(setting, EventActions.CreateRoom, res =>
            {
                lod.Close();
                if (res.IsSuccess)
                {
                    Close();
                    PanelManager.Instance.OpenPanel<RoomPanel>(res.Data);
                }
                else
                {
                    PanelManager.Instance.ShowMessageBox("创建房间失败:" + res.Message);
                }
            });
        }

        private bool CheckInput(out string errmsg)
        {
            if(RoomName.text=="")
            {
                RoomName.ActivateInputField();
                errmsg = "房间名称不能为空";
                return false;
            }    
            else if(IsPassword.isOn && Password.text=="")
            {
                Password.ActivateInputField();
                errmsg = "密码不能为空";
                return false;
            }
            errmsg = "";
            return true;
        }

        public void OnIsPasswordChanged()
        {
            Password.interactable = IsPassword.isOn;
            if (!IsPassword.isOn)
                Password.text = "";
        }


        public void Cancel()
        {
            Close();
        }
    }

}
