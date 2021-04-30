using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.UI.Panel
{
    public delegate void InputResultHandle(string msg);
    public class InputPanel : PanelBase
    {
        public event InputResultHandle OnInputResult;
        public InputField Input;
        public override void OnInit(params object[] paramaters)
        {
            Input.ActivateInputField();
        }
        public override void OnEnter()
        {
            OnInputResult?.Invoke(Input.text);
            Close();
        }

        /// <summary>
        /// 显示输入框 并获取输入的文字
        /// </summary>
        /// <param name="ResAction"></param>
        public static void ShowInput(InputResultHandle ResAction)
        {
            var panel =PanelManager.Instance.OpenPanel<InputPanel>();
            panel.OnInputResult += ResAction;
        }
    }

}
