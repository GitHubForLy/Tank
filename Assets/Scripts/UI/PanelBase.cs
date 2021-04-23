using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TankGame.UI
{
    public class PanelBase : MonoBehaviour
    {
        private void Awake()
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="paramaters">可选参数</param>
        public virtual void OnInit(params object[] paramaters)
        {


        }

        /// <summary>
        /// Esc键点击时
        /// </summary>
        public virtual void OnEscape()
        {
            Close();
        }

        /// <summary>
        /// 回车键点击时
        /// </summary>
        public virtual void OnEnter()
        {

        }

        public virtual void OnCloseing(out bool isClose)
        {
            isClose = true;
        }

        public virtual void OnClosed()
        {
            
        }

        public virtual void Close()
        {
            PanelManager.Instance.ClosePanel(this);
        }
    }
}
