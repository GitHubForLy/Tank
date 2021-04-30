using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.UI.Panel
{
    public class InputPasswordPanel : PanelBase
    {
        [SerializeField]
        private InputField Password;
        private bool Isok = false;

        public event Action<bool, string> OnResult;
        public void Ok()
        {
            Isok = true;
            Close();
        }

        public void Cancel()
        {
            Isok = false;
            Close();
        }

        public override void OnClosed()
        {
            OnResult?.Invoke(Isok, Password.text);
        }


        /// <summary>
        /// 打开密码输入框 并获取结果
        /// </summary>
        /// <param name="ResultCallback"></param>
        public static void ShowAndGetResult(Action<bool,string> ResultCallback)
        {
            var panel= PanelManager.Instance.OpenPanel<InputPasswordPanel>();
            panel.OnResult += ResultCallback;
        }
    }

}
