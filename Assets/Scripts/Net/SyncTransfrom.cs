using ServerCommon;
using System;
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
        private new  Rigidbody rigidbody;
        private Vector3 oldPos, oldEur;
        private Vector3 oldRcvPos=Vector3.zero, oldRcvEur=Vector3.zero;
        private Vector3 fPos, fEur;
        private float lastRcvTime;
        private float deltaTime;
        private float lastSendtime;

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            netIdentity = GetComponent<NetIdentity>();
        }

        private void Start()
        {
            if (!IsLocalPlayer)
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            oldPos = transform.position;
            oldEur = transform.eulerAngles;
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
                (string account,double sendtime, DataModel.Transform transform,DataModel.Vector3 velocity) data = dt.data.GetValue<(string , double, DataModel.Transform, DataModel.Vector3)>();
                if (data.account != netIdentity.Account)
                    return;

                var cuPos = data.transform.Position.ToUnityVector();
                var cuEur = data.transform.Rotation.ToUnityVector();
                var ve = data.velocity.ToUnityVector();
                ve.y = 0;

                deltaTime = (float)(NetManager.Instance.ServerTime - data.sendtime);
                Debug.Log("time:" + deltaTime+" doutime:"+(Time.time-lastRcvTime));

                fPos = cuPos + (ve *deltaTime);         //预测当前位置
                fEur = cuEur;
                lastRcvTime = Time.time;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label(deltaTime.ToString());
        }

        void Update()
        {
            if (IsLocalPlayer)
                return;
            transform.position = Vector3.Lerp(transform.position, fPos, deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.Euler(fEur),deltaTime);
        }

        /// <summary>
        /// 向服务器同步自己的Transfrom
        /// </summary>
        public void SyncTransform()
        {
            if (Time.time - lastSendtime < 0.2f)
                return;
            var pos = transform.position;
            var eur = transform.eulerAngles;
            if (pos.x == oldPos.x && pos.y == oldPos.y && pos.z == oldPos.z &&
                eur.x == oldEur.x && eur.y == oldEur.y && eur.z == oldEur.z)
                return;

            DataModel.Transform trans = new DataModel.Transform
            {
                Position = new DataModel.Vector3
                {
                    X = pos.x,
                    Y = pos.y,
                    Z = pos.z
                },
                Rotation = new DataModel.Vector3
                {
                    X = eur.z,
                    Y = eur.y,
                    Z = eur.z
                }
            };

            DataModel.Vector3 velocity = new DataModel.Vector3
            {
                X = rigidbody.velocity.x,
                Y = rigidbody.velocity.y,
                Z = rigidbody.velocity.z
            };

            var sendtime = NetManager.Instance.ServerTime;
            Debug.Log(" sendtime:" + sendtime );
            BoradcastMessage(DataModel.BroadcastActions.UpdateTransform, (netIdentity.Account, sendtime, trans, velocity));
            oldPos = pos;
            oldEur = eur;
            lastSendtime = Time.time;
        }




    }

}
