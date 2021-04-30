using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.Net;
using DataModel;

namespace TankGame.UI.Panel
{
    public class RegisterPanel : PanelBase
    {
        [SerializeField]
        private InputField m_NameInput;
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
            if (string.IsNullOrEmpty(m_NameInput.text))
            {
                PanelManager.Instance.ShowMessageBox("请输入昵称");
                return;
            }
            if (string.IsNullOrEmpty(m_AccountInput.text))
            {
                PanelManager.Instance.ShowMessageBox("请输入账号");
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(m_AccountInput.text, @"^[0-9a-zA-Z_]{1,}$"))
            {
                PanelManager.Instance.ShowMessageBox("账号 只能有数字,字母,下划线组成");
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
                Password = m_PasswordInput.text,
                UserAccount = m_NameInput.text
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

