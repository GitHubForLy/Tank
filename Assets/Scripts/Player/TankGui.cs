using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.TankBehaviour;
using TankGame.Net;

namespace TankGame.Player
{
    public class TankGui : NetBehaviour
    {
        public Texture AimTarget;
        public Texture FireTarget;
        public Texture KillTexture;
        public Image HealthObj;
        public Text HealthText;

        public float KillIconDelayDuration = 1.5f;

        [HideInInspector]
        public Vector3 TargetPos;

        private TankMovement tankMovement;
        private TankFire tankFire;
        private TankHealth tankHealth;
        private float lastKillEnemyTime = -10;

        // Start is called before the first frame update
        void Start()
        {
            tankMovement = GetComponent<TankMovement>();
            tankFire = GetComponent<TankFire>();
            tankHealth = GetComponent<TankHealth>();
            tankFire.OnKillEnemy += TankFire_OnKillEnemy;

            if (!HealthObj)
                HealthObj = GameObject.FindGameObjectWithTag(Tags.Hp).GetComponent<Image>();
            if (!HealthText)
                HealthText = GameObject.FindGameObjectWithTag(Tags.HpText).GetComponent<Text>();

            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;
        }

        private void Instance_OnReceiveBroadcast((string action, ServerCommon.IDynamicType subdata) msg)
        {
            if (!IsLocalPlayer)
                return;
           if(msg.action==DataModel.BroadcastActions.Die)
            {
                var killer = msg.subdata.GetValue<string>();
                if (killer == NetManager.Instance.LoginAccount) //是自己杀了对方
                    lastKillEnemyTime = Time.time;
            }
        }

        private void TankFire_OnKillEnemy(DamageHit damageHit)
        {
            lastKillEnemyTime = Time.time;
        }




        private void OnGUI()
        {
            if(!BattlegroundManager.Instance.IsLocalPlayer(this))
                return;
            if (Time.timeScale == 0)//暂停游戏
                return;
            
            DrawAimIco();

            DrawHealth();

            DrawKillIcon();

        }

        private void DrawAimIco()
        {
            if (tankHealth.IsDie)
                return;

            var tar = Camera.main.WorldToScreenPoint(TargetPos);
            Rect rt = new Rect(tar.x - AimTarget.width / 2, tar.y - AimTarget.height / 2, AimTarget.width, AimTarget.height);
            GUI.DrawTexture(rt, AimTarget);


            var ishit= tankFire.ShootRaycast(out  RaycastHit hit, out Vector3 firePos);
            var pos = Camera.main.WorldToScreenPoint(firePos);
            Rect frt = new Rect(pos.x - FireTarget.width / 2, Screen.height - pos.y - FireTarget.height / 2, FireTarget.width, FireTarget.height);
            GUI.DrawTexture(frt, FireTarget);


            if(ishit && hit.collider.gameObject.CompareTag(Tags.Tank))
            {
                var account= hit.collider.gameObject.GetComponent<NetIdentity>().Account;
                var name= BattlegroundManager.Instance.Tanks[account].UserName;
                var cont = new GUIContent(name);
                var style = new GUIStyle();
                style.normal.textColor = Color.white;
                style.alignment = TextAnchor.MiddleCenter;
                var size= style.CalcSize(cont);
                Rect trt = new Rect(new Vector2(pos.x-size.x/2, pos.y-size.y/2), size);

                GUI.Label(trt,name,style);
            }
        }

        /// <summary>
        /// 绘制生命条
        /// </summary>
        private void DrawHealth()
        {
            HealthObj.fillAmount = tankHealth.CurrentHealth / tankHealth.MaxHealth;
            HealthText.text = $"{Mathf.Ceil(tankHealth.CurrentHealth)} / {tankHealth.MaxHealth}";
        }

        private void DrawKillIcon()
        {

            if (Time.time - lastKillEnemyTime < KillIconDelayDuration)
            {
                Rect rt = new Rect(Screen.width / 2 - KillTexture.width / 2, Screen.height / 2 - KillTexture.height / 2,
                    KillTexture.width, KillTexture.height);
                GUI.DrawTexture(rt, KillTexture);
            }
        }

        private void OnDestroy()
        {
            NetManager.Instance.OnReceiveBroadcast -= Instance_OnReceiveBroadcast;
        }
    }
}