using ServerCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TankGame.Net
{
    [RequireComponent(typeof(NetIdentity))]
    public class NetBehaviour : MonoBehaviour
    {
        public bool IsLocalPlayer 
        { 
            get
            {
                if(BattlegroundManager.Instance==null)
                {
                    Debug.LogError("null instance");
                    return false;
                }
                return BattlegroundManager.Instance.IsLocalPlayer(this);
            } 
        }

        public virtual void OnBroadcast((string Action,IDynamicType data) dt)
        {

        }
 

        /// <summary>
        /// 广播数据
        /// </summary>
        /// <param name="action">数据action</param>
        /// <param name="data">数据</param>
        public void BoradcastMessage(string action,object data)
        {
            CommonRequest.Instance.Broadcast(data,action);
        }
    }
}
