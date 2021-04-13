using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.TankBehaviour;
using TankGame.Net;

namespace TankGame.Player
{
    /// <summary>
    /// 用户输入
    /// </summary>
    [RequireComponent(typeof(TankMovement),typeof(TankFire),typeof(TankGui))]
    public class PlayerControl : NetBehaviour
    {
        [HideInInspector]
        public bool EnableInput=true;

        private TankMovement tankMovement;
        private TankFire tankFire;
        private TankGui tankGui;
        private TankHealth tankHealth;
        private float forwardInput;
        private float turnInput;
        private bool fireInput;
        private Vector3 targetPos;


        // Start is called before the first frame update
        void Start()
        {
            tankMovement=GetComponent<TankMovement>();
            tankFire=GetComponent<TankFire>();
            tankGui = GetComponent<TankGui>();
            tankHealth = GetComponent<TankHealth>();
            tankHealth.OnDie += TankHealth_OnDie;
        }

        private void TankHealth_OnDie(GameObject deadTank,Behaviour Killer)
        {
            EnableInput = false;
        }

        private void Update()
        {
            if (!EnableInput)
                return;
            
            if(!IsLocalPlayer)
                return;

            forwardInput = Input.GetAxis("Vertical");
            turnInput = Input.GetAxis("Horizontal");
            fireInput = Input.GetMouseButton(0);

            //开火
            if (fireInput)
                tankFire.Fire();

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, tankFire.MaxShootDistance))
                targetPos = hit.point;
            else
                targetPos = ray.GetPoint(tankFire.MaxShootDistance);

            tankGui.TargetPos = targetPos;

            if(tankMovement.turret!=null)
                tankMovement.TargetDirection = targetPos - tankMovement.turret.position;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            if (!EnableInput)
                return;

            tankMovement.Move(forwardInput, turnInput);
        }

        
    }
}

