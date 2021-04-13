using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.Net;
using DataModel;

namespace TankGame.UI
{
    public class RegisterPanel : PanelBase
    {
        [SerializeField]
        private InputField m_AccountInput;
        [SerializeField]
        private InputField m_PasswordInput;
        [SerializeField]
        private InputField m_ConfirmPasswordInput;

        public void BackLogin()
        {
            PanelManager.Instance.OpenPanel<LoginPanel>();
            PanelManager.Instance.ClosePanel(this);
        }

        public void Register()
        {
            if (string.IsNullOrEmpty(m_AccountInput.text))
            {
                PanelManager.Instance.ShowMessageBox("请输入账号");
                return;
            }
            if (string.IsNullOrEmpty(m_PasswordInput.text))
            {
                PanelManager.Instance.ShowMessageBox("请输入密码");
                return;
            }
            if (string.IsNullOrEmpty(m_ConfirmPasswordInput.text))
            {
                PanelManager.Instance.ShowMessageBox("请输入确认密码");
                return;
            }
            if (m_PasswordInput.text!=m_ConfirmPasswordInput.text)
            {
                PanelManager.Instance.ShowMessageBox("两次输入密码不一致");
                return;
            }


            RegisterRequest request = new RegisterRequest
            {
                UserName = m_AccountInput.text,
                Password = m_PasswordInput.text
            };

            CommonRequest.Instance.DoRequest<StandRespone>(request, EventActions.Register, (res) =>
            {
                if (res.IsSuccess)
                    PanelManager.Instance.ShowMessageBox("注册成功");
                else
                    PanelManager.Instance.ShowMessageBox(res.Message);
            });
        }
    }

}

