using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Net;

namespace TankGame.TankBehaviour
{

    public enum BrakeTypes
    {
        /// <summary>
        /// 最大制动
        /// </summary>
        MaxBrake,
        /// <summary>
        /// 不制动
        /// </summary>
        NoBrake,
        /// <summary>
        /// 自动制动  
        /// </summary>
        AutoBrake
    }


    [RequireComponent(typeof(AudioSource), typeof(Rigidbody), typeof(TankFire))]
    public class TankMovement : NetBehaviour
    {
        /// <summary>
        /// 车轮碰撞体
        /// </summary>
        [System.Serializable]
        public class Wheel
        {
            public WheelCollider LeftCollider;
            public WheelCollider RightCollider;
            public Transform LeftTransform;
            public Transform RightTransform;
        }
        [System.Serializable]
        public class NoColliderWheel
        {
            public Transform LeftWheel;
            public Transform RightWheel;
        }

        [System.Serializable]
        public enum TurnTypes
        {
            //DiffrentSpeed,
            Rigidbody
        }

        [Tooltip("是否由AI控制")]
        public bool AIControl = false;

        [Tooltip("移动速度")]
        public float MaxMotor = 10;
        [Tooltip("转弯速度")]
        public float AngleSpeed = 25;
        [Tooltip("炮塔旋转速度")]
        public float TurretRotateSpeed = 1;
        [Tooltip("炮管旋转速度")]
        public float GunRotateSpeed = 1;
        [Tooltip("炮管本地下半部角度 [0,GunMaxDownAngle]")]
        public float GunMaxDownAngle = 30;
        [Tooltip("炮管本地上半部角度 [GunMaxUpAngle,360]")]
        public float GunMinUpAngle = 330;
        public float CameraHeightDelta = 7f;
        [Tooltip("制动扭矩")]
        public float BrakeTorque = 800;
        [Tooltip("转弯类型\nDiffrentSpeed:差速转弯\nRigidbody:利用刚体进行旋转")]
        public TurnTypes TurnType = TurnTypes.Rigidbody;



        //车轮
        [Space(10), Tooltip("车轮碰撞体")]
        public Wheel[] Wheels;
        public Transform Gun;//炮管
        [SerializeField, Tooltip("没有实际作用的车轮(只显示)")]
        private NoColliderWheel[] m_NoColliderWheels;
        [SerializeField, Tooltip("履带")]
        private GameObject m_Track;
        [SerializeField]
        private Transform m_CenterOfMass;


        [HideInInspector]
        public bool EnableInput { get; set; } = true;
        [HideInInspector]
        public Transform turret;//炮塔
        [HideInInspector/*,SyncField*/]//炮塔要指向的方向
        public Vector3 TargetDirection;

        private Rigidbody m_rigidbody;
        private AudioSource audioSource;

        private float currnetMotorTorque;   
        private bool isReady = false;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();   
            m_rigidbody = GetComponent<Rigidbody>();
            turret = transform.Find("turret");
            if (!Gun)
                Gun = turret.transform.Find("gun"); 

            if (m_CenterOfMass)
                m_rigidbody.centerOfMass = m_CenterOfMass.localPosition;
            
            isReady = true;
        }

        /// <summary>
        /// 控制坦克移动和炮塔旋转
        /// </summary>
        /// <param name="motorDelta">动力值</param>
        /// <param name="steerDelta">旋转值</param>
        /// <param name="targetDirection">炮塔旋转的目标方向</param>
        //[SyncMethod]
        public void Move(float motorDelta, float steerDelta, BrakeTypes tbrake = BrakeTypes.AutoBrake)
        {
            if (!isReady)
                return;

            motorDelta = Mathf.Clamp(motorDelta, -1, 1);
            steerDelta = Mathf.Clamp(steerDelta, -1, 1);
            var motorAcc = motorDelta * MaxMotor;
            var motorSteer = steerDelta * MaxMotor;

            foreach (var wheel in Wheels)
            {
                wheel.LeftCollider.motorTorque = motorAcc;
                wheel.RightCollider.motorTorque = motorAcc;

                //if(TurnType==TurnTypes.DiffrentSpeed)
                //{
                //    wheel.LeftCollider.motorTorque = Mathf.Clamp(wheel.LeftCollider.motorTorque + motorSteer, -MaxMotor, MaxMotor);
                //    wheel.RightCollider.motorTorque = Mathf.Clamp(wheel.RightCollider.motorTorque - motorSteer, -MaxMotor, MaxMotor);
                //}        

                if (tbrake == BrakeTypes.AutoBrake)
                {
                    wheel.LeftCollider.brakeTorque = wheel.LeftCollider.rpm * motorDelta < 0 ? BrakeTorque : 0;
                    wheel.RightCollider.brakeTorque = wheel.LeftCollider.rpm * motorDelta < 0 ? BrakeTorque : 0;
                }
                else if (tbrake == BrakeTypes.MaxBrake)
                {
                    wheel.LeftCollider.brakeTorque = BrakeTorque;
                    wheel.RightCollider.brakeTorque = BrakeTorque;
                }
                else if (tbrake == BrakeTypes.NoBrake)
                {
                    wheel.LeftCollider.brakeTorque = 0;
                    wheel.RightCollider.brakeTorque = 0;
                }

            }

            if (TurnType == TurnTypes.Rigidbody)
            {
                m_rigidbody.MoveRotation(transform.rotation * Quaternion.AngleAxis(steerDelta * AngleSpeed * Time.fixedDeltaTime, Vector3.up));
            }
        }

        public void Update()
        {
            PlayAudio();
        }

        public void FixedUpdate()
        {
            ApplyWheelRotation();
            ApplyTrackRotation();

            if (TargetDirection != Vector3.zero)
            {
                rotateTurret(TargetDirection);
                rotateGun(TargetDirection);
            }
        }


        //炮塔水平旋转
        private void rotateTurret(Vector3 targetDirection)
        {
            if (Camera.main == null || turret == null)
                return;

            //先转换为本地欧拉角
            var locdir = turret.InverseTransformDirection(targetDirection);
            var locAngle = Quaternion.LookRotation(locdir).eulerAngles;

            var delta = locAngle.y % 360;//相对于自己的y分量 [0,360]
            if (delta > 180)
                delta -= 360;       //[-180,180];

            var sDelta = Mathf.Abs(delta);

            if (sDelta > 1f)
            {
                var roateAngle = Mathf.Sign(delta) * TurretRotateSpeed * Time.deltaTime;
                if (sDelta > TurretRotateSpeed * Time.deltaTime)
                {
                    turret.Rotate(Vector3.up, roateAngle);
                }
                else
                {
                    //小于1度时直接旋转完剩余角度
                    turret.Rotate(Vector3.up, delta);
                }
            }
        }

        //炮管垂直旋转
        private void rotateGun(Vector3 targetDirection)
        {
            if (Camera.main == null || Gun == null)
                return;

            var targetRotation = Quaternion.LookRotation(targetDirection);

            var delta = Mathf.DeltaAngle(Gun.eulerAngles.x, targetRotation.eulerAngles.x);
            delta -= CameraHeightDelta;

            float angle = Mathf.Sign(delta) * GunRotateSpeed * Time.fixedDeltaTime;

            if (Mathf.Abs(delta) > 2f)
                Gun.localEulerAngles = new Vector3(Gun.localEulerAngles.x + angle, Gun.localEulerAngles.y, Gun.localEulerAngles.z);
            else
                Gun.localEulerAngles = new Vector3(Gun.localEulerAngles.x + delta, Gun.localEulerAngles.y, Gun.localEulerAngles.z);


            ////根据自身欧拉角进行角度限制
            var angles = Gun.localEulerAngles;
            if (angles.x > GunMaxDownAngle && angles.x < GunMinUpAngle)
            {
                angles.x = Mathf.Abs(GunMaxDownAngle - angles.x) > Mathf.Abs(GunMinUpAngle - angles.x) ? GunMinUpAngle : GunMaxDownAngle;
                Gun.localEulerAngles = angles;
            }

        }

        //车轮旋转
        private void ApplyWheelRotation()
        {
            Quaternion rotation = new Quaternion();
            Vector3 pos;

            foreach (var wheel in Wheels)
            {
                wheel.LeftCollider.GetWorldPose(out pos, out rotation);
                wheel.LeftTransform.position = pos;
                wheel.LeftTransform.rotation = rotation;

                wheel.RightCollider.GetWorldPose(out pos, out rotation);
                wheel.RightTransform.position = pos;
                wheel.RightTransform.rotation = rotation;
            }

            //对于没有实际作用的车轮 暂时只给它复制一个旋转属性
            if (Wheels.Length > 0)
            {
                foreach (var wheel in m_NoColliderWheels)
                {
                    wheel.LeftWheel.rotation = Wheels[0].LeftTransform.rotation;
                    wheel.RightWheel.rotation = Wheels[0].RightTransform.rotation;
                }
            }

        }

        private void ApplyTrackRotation()
        {
            if (m_Track == null)
                return;
            if (Wheels.Length <= 0)
                return;

            var Mat = m_Track.GetComponent<MeshRenderer>().material;

            //暂时取第一个车轮的旋转作为履带的旋转根据
            Mat.mainTextureOffset = new Vector2(0, Wheels[0].LeftTransform.localEulerAngles.x / 4);
        }

        private void PlayAudio()
        {
            if (currnetMotorTorque != 0)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }
        }


    }
}