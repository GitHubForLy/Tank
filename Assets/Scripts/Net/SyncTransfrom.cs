using ServerCommon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TankGame.Net
{
    public sealed class SyncTransfrom : NetBehaviour
    {
        /// <summary>
        /// 是否需要同步服务器上的位置
        /// </summary>
        public bool IsNeedSyncTransform=true;
        private NetIdentity netIdentity;

        public void Awake()
        {
            netIdentity = GetComponent<NetIdentity>();
        }


        private void FixedUpdate()
        {
            if (!IsLocalPlayer)
                return;
            SyncTransform();
        }

        public override void OnBroadcast((string Action, IDynamicType data) dt)
        {
            base.OnBroadcast(dt);

            if(dt.Action== DataModel.BroadcastActions.UpdateTransform && IsNeedSyncTransform && !IsLocalPlayer)
            {
                
                (string account, DataModel.Transform transform) data = dt.data.GetValue<(string account, DataModel.Transform transform)>();
                try
                {
                    if (data.account == netIdentity.Account)
                    {
                        transform.position = data.transform.Position.ToUnityVector();
                        transform.eulerAngles = data.transform.Rotation.ToUnityVector();
                    }
                }
                catch(System.Exception e)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }


        /// <summary>
        /// 向服务器同步自己的Transfrom
        /// </summary>
        public void SyncTransform()
        {
            DataModel.Transform trans = new DataModel.Transform
            {
                Position = new DataModel.Vector3
                {
                    X = transform.position.x,
                    Y = transform.position.y,
                    Z = transform.position.z
                },
                Rotation = new DataModel.Vector3
                {
                    X = transform.eulerAngles.z,
                    Y = transform.eulerAngles.y,
                    Z = transform.eulerAngles.z
                }
            };
            BoradcastMessage(DataModel.BroadcastActions.UpdateTransform, (netIdentity.Account, trans));
        }
    }

}
