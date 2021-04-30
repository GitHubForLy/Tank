using ServerCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace TankGame.Net
{
    [RequireComponent(typeof(NetIdentity))]
    public class NetBehaviour : MonoBehaviour
    {

        //private IEnumerable<MethodInfo> syncMethods;

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

        //private void Start()
        //{
        //    syncMethods= GetType().GetMethods().Where(m=>m.IsDefined(typeof(SyncMethodAttribute),true)).Select(m=>m.de);
        //}


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
