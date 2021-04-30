using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;

namespace TankGame.TankBehaviour
{
    public delegate void KillEnemyEventHandle(DamageHit damageHit);
    public class TankFire : NetBehaviour
    {
        public GameObject Bullut;
        public Transform FirePoint;
        public GameObject Explosion;
        public AudioClip FireAudio;
        public AudioClip ExplodeAudio;

        public float ExplosionForce=2000f;
        public float Damage = 35;
        [Tooltip("爆炸范围半径")]
        public float ExplosionRadius = 2;
        public float MaxShootDistance = 300f;
        public float Speed = 100f;
        public float FireInteval = 0.7f;

        /// <summary>
        /// 击杀敌人时
        /// </summary>
        public event KillEnemyEventHandle OnKillEnemy;


        private TankTeam tankTeam;
        private float lastFireTime = 0;


        public void Start()
        {
            tankTeam = GetComponent<TankTeam>();
        }


        /// <summary>
        /// 开火
        /// </summary>
        public void Fire()
        {
            if (Time.time - lastFireTime < FireInteval)
                return;
            if(IsLocalPlayer)
                BroadFire();

            ShootRaycast(out RaycastHit hit, out Vector3 FireTargetPos);

            if (!IsLocalPlayer)
            {
                DoTakeDamage(FireTargetPos);
            }           

            Instantiate(Explosion, FireTargetPos, FirePoint.rotation);
            AudioSource.PlayClipAtPoint(FireAudio, transform.position);
            AudioSource.PlayClipAtPoint(ExplodeAudio, FireTargetPos);

            lastFireTime = Time.time;
        }

        static int id = 0;
        private void BroadFire()
        {
            //id++;
            //Debug.Log(id);
            CommonRequest.Instance.Broadcast(null, DataModel.BroadcastActions.Fire);
        }

        private void DoTakeDamage(Vector3 FireTargetPos)
        {

            var colliders = Physics.OverlapSphere(FireTargetPos, ExplosionRadius);

            List<GameObject> hitObjs = new List<GameObject>();

            foreach (var collider in colliders)
            {
                var obj = collider.gameObject;
                if (obj.CompareTag(Tags.Tank))
                {
                    //同一个坦克只被伤害一次
                    if (hitObjs.Contains(obj))
                        continue;
                    else
                        hitObjs.Add(obj);

                    var enemyHealth = obj.GetComponentInParent<TankHealth>();
                    var team = obj.GetComponentInParent<TankTeam>();
                    if (tankTeam.TeamNumber != team.TeamNumber && !enemyHealth.IsDie)
                    {

                        var hitDamage = CalculateDamage(FireTargetPos, collider);
                        print("damage:" + hitDamage);

                        if (collider.attachedRigidbody)
                            collider.attachedRigidbody.AddExplosionForce(ExplosionForce, FireTargetPos, ExplosionRadius);

                        var damhit = enemyHealth.TakeDamage(hitDamage, this);

                        //击杀对方
                        if (damhit.IsMakeDeath)
                            OnKillEnemy?.Invoke(damhit);
                    }
                }
            }
        }

        //计算伤害
        public float CalculateDamage(Vector3 firePos,Collider hitCollider)
        {
            var dis = Vector3.Distance(firePos, hitCollider.ClosestPointOnBounds(firePos));
            var rate = 1 - Mathf.Clamp01(dis / ExplosionRadius);
            return Damage * rate;
        }

        public bool ShootRaycast(out RaycastHit raycastHit, out Vector3 FireTargetPos)
        {
            Vector3 point;
            bool res;
            res = Physics.Raycast(FirePoint.position, FirePoint.forward, out raycastHit, MaxShootDistance);

            if (res)
                point = raycastHit.point;
            else
                point = FirePoint.position + FirePoint.forward * MaxShootDistance;

            FireTargetPos = point;
            return res;
        }

    }

}
