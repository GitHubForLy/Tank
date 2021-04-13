using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AopCore;

namespace TankGame.Net
{
    /// <summary>
    /// 广播目标方法的调用
    /// </summary>
    public class SyncMethodAttribute : MethodHookAttribute
    {
        public override void OnMethodEnter(MethodExecuteArgs args)
        {
            var behaviour = args.Instance as NetBehaviour;
            if(behaviour==null)
            {
                Debug.LogWarning("必须继承自Netbehaviour才能使该特性生效,目标类:"+args.Instance.ToString());
                return;
            }

            if(behaviour.IsLocalPlayer)
                CommonRequest.Instance.BroadcastMethod(args);
        }
    }

}

