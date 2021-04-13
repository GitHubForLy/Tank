using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TankGame.TankBehaviour
{

    /// <summary>
    /// 伤害信息
    /// </summary>
    public struct DamageHit
    {
        public float RealDamage { get; set; }

        public bool IsMakeDeath { get; set; }
    }

    public delegate void OnDieEventHandle(GameObject deadTank,Behaviour killer);

    public class TankHealth : MonoBehaviour
    {
        public GameObject DieEffect;
        public float MaxHealth = 100;
        public float CurrentHealth { get; private set; }
        public bool IsDie { get; private set; }

        public event OnDieEventHandle OnDie;


        // Start is called before the first frame update
        void Start()
        {
            IsDie = false;
            CurrentHealth = MaxHealth;
        }


        public DamageHit TakeDamage(float Damage, Behaviour sender)
        {
            DamageHit hit = new DamageHit() { IsMakeDeath = false, RealDamage = 0 };
            if (IsDie)
                return hit;

            hit.RealDamage = Damage;
            CurrentHealth -= Damage;
            if (CurrentHealth <= 0)
            {
                hit.IsMakeDeath = true;
                CurrentHealth = 0;
                IsDie = true;
                DoDie(sender);
            }
            return hit;
        }

        private void DoDie(Behaviour killer)
        {
            OnDie?.Invoke(gameObject,killer);
            if (DieEffect)
            {
                Instantiate(DieEffect, transform.TransformPoint(Vector3.zero), transform.rotation, transform);
            }
        }

    }

}
