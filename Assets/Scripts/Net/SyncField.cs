using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AopCore;
using System.Reflection;

namespace TankGame.Net
{
    public class SyncFieldAttribute : FiledHookAttribute
    {
        public override void OnSetValue(FieldUpdateArgs args)
        {
            var behaviour = args.Instance as NetBehaviour;
            if (behaviour == null)
            {
                Debug.LogWarning("必须继承自Netbehaviour才能使该特性生效,目标类:" + args.Instance.ToString());
                return;
            }

            if (behaviour.IsLocalPlayer)
                CommonRequest.Instance.BroadcastField(args);
        }
    }

}
