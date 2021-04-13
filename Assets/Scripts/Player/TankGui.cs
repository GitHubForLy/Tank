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


            tankFire.ShootRaycast(out _, out Vector3 firePos);
            var pos = Camera.main.WorldToScreenPoint(firePos);
            Rect frt = new Rect(pos.x - FireTarget.width / 2, Screen.height - pos.y - FireTarget.height / 2, FireTarget.width, FireTarget.height);
            GUI.DrawTexture(frt, FireTarget);
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


    }
}