using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TankGame.UI
{
    public class PanelBase : MonoBehaviour
    {
        private void Awake()
        {
            OnInit();
        }


        protected virtual void OnInit()
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
