using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TankGame.TankBehaviour;

namespace TankGame.AI 
{
    public enum AIStates
    {
        /// <summary>
        /// 死亡
        /// </summary>
        Died,
        /// <summary>
        /// 巡逻
        /// </summary>
        Patrol,
        /// <summary>
        /// 追击
        /// </summary>
        Chase
    }


    [RequireComponent(typeof(TankFire),typeof(TankTeam))]
    public class AIControl : MonoBehaviour
    {
        [Tooltip("视野距离")]
        public float SightDistance=50;
        public float StopDistance=0.2f;
        [Tooltip("最大转弯减速角度(转弯时小于此角度将减速转弯以减小误差)")]
        public float MaxTurnDecelerationAngle = 30f;
        public Transform[] WayPoints;
        public float DestroyTime = 4.0f;

        public AIStates State { get; private set; }

        private TankMovement tankMovement;
        private TankFire tankFire;
        private int currentWayIndex=0;          //当前目标路点
        private GameObject lastEnmeyTank;
        private float forwardDelta;
        private float turnDelta;
        private BrakeTypes brakeDelta;
        private int currentNavMeshIndex = -1;   //导航网格索引
        private NavMeshPath currentNavPath;
        private TankTeam tankTeam;
        private TankHealth tankHealth;
        private Vector3 currentChasePos;

        void Start()
        {
            tankFire = GetComponent<TankFire>();
            tankMovement = GetComponent<TankMovement>();
            currentNavPath = new NavMeshPath();
            tankHealth = GetComponent<TankHealth>();
            tankTeam = GetComponent<TankTeam>();

            if(WayPoints==null|| WayPoints.Length==0)
            {
                var objs = GameObject.FindGameObjectsWithTag(Tags.Waypoint);
                WayPoints = (from o in objs select o.transform).ToArray();
            }
            currentWayIndex = GetNextNavIndex();

            TankHealth.OnDie += TankHealth_OnDie;
        }

        private void TankHealth_OnDie(GameObject deadTank, Behaviour killer)
        {
            StartCoroutine(DeadDestroy());
        }

        private IEnumerator DeadDestroy()
        {
            yield return new WaitForSeconds(DestroyTime);
            Destroy(gameObject);
        }


        // Update is called once per frame
        void Update()
        {
            State = DetermineState();

            if (State == AIStates.Patrol)
                Patrol();
            else if (State == AIStates.Chase)
                Chase(lastEnmeyTank);
            else if (State == AIStates.Died)
                Die();
        }



        private AIStates DetermineState()
        {
            AIStates res;
            if(tankHealth.IsDie)
            {
                res = AIStates.Died;
            }
            else if(CheckSightEnemy(out lastEnmeyTank))
            {
                res = AIStates.Chase;
            }
            else
            {
                res = AIStates.Patrol;
            }
            return res;
        }

        private bool CheckSightEnemy(out GameObject EnemyTank)
        {
            float minDis = float.MaxValue;
            float dis;
            EnemyTank = null;

            foreach (var tank in BattlegroundManager.Instance.Tanks)
            {
                if (tank.Value.Instance == gameObject)
                    continue;


                //判断是否死亡 如果死亡则Instance则可能为空
                if (tank.Value.IsDie)
                    continue;

                //判断队友还是敌人
                var team = tank.Value.Instance.GetComponent<TankTeam>();
                if (team && tankTeam.TeamNumber == team.TeamNumber)
                    continue;

                dis=Vector3.Distance(tank.Value.Instance.transform.position, transform.position);
                if(dis<SightDistance)
                {
                    if(EnemyTank==null)
                    {
                        EnemyTank = tank.Value.Instance;
                        minDis = dis;
                    }
                    else if(dis<minDis)
                    {
                        EnemyTank = tank.Value.Instance;
                        minDis = dis;
                    }
                }
            }

            if (EnemyTank == null)
                return false;
            else
                return true;
        }

        private void FixedUpdate()
        {
            tankMovement.Move(forwardDelta, turnDelta,brakeDelta);            
        }

        private void Die()
        {
            forwardDelta = 0;
            brakeDelta = BrakeTypes.NoBrake;
            turnDelta = 0;
        }

        //巡逻
        private void Patrol()
        {
            if (WayPoints.Length < 1)
                return; 


            var dis = Vector3.Distance(transform.position, WayPoints[currentWayIndex].position);
            if (dis <= StopDistance || currentNavMeshIndex >= currentNavPath.corners.Length)
            {
                currentWayIndex = GetNextNavIndex();
                currentNavMeshIndex = -1;
            }

            if (currentNavMeshIndex==-1)
            {
                currentNavPath.ClearCorners();
                if(!NavMesh.CalculatePath(transform.position, WayPoints[currentWayIndex].position, NavMesh.AllAreas, currentNavPath))
                {
                    Debug.LogError("路径未找到  currentWayIndex:"+ currentWayIndex);
                    currentWayIndex = GetNextNavIndex();
                    currentNavMeshIndex = -1;
                    return;
                }

                currentNavMeshIndex = 0;
            }
            dis = Vector3.Distance(transform.position, currentNavPath.corners[currentNavMeshIndex]);
            if(dis<=StopDistance)
            {
                currentNavMeshIndex++;
                if (currentNavMeshIndex >= currentNavPath.corners.Length)
                    return;
            }


            Vector3 targetPos = currentNavPath.corners[currentNavMeshIndex];

            //获取向前和旋转值
            GetTurnAndForwardDelat(transform, targetPos, out turnDelta, out forwardDelta);    
            tankMovement.TargetDirection = transform.forward;
            brakeDelta = BrakeTypes.NoBrake;

            #region 调试代码
            //for(int i=0;i<currentNavPath.corners.Length;i++)
            //{
            //    if(i<currentNavPath.corners.Length-1)
            //        Debug.DrawLine(currentNavPath.corners[i], currentNavPath.corners[i+1], Color.red);              
            //}
            //print("angle:" + angle + "   turnDelta:" + turnDelta+"  dis:"+dis+"  index:"+currentWayIndex);
            //Debug.DrawLine(transform.position, targetPos, Color.red);
            #endregion
        }

        private void OnDrawGizmos()
        {
            #region 调试代码
            //Gizmos.color = Color.red;
            //for (int i = 0; i < currentNavPath.corners.Length; i++)
            //{
            //    Gizmos.DrawSphere(currentNavPath.corners[i],2);                  
            //}          
            #endregion
        }

        private int GetNextNavIndex()
        {
            //return (currentWayIndex + 1) % WayPoints.Length;         
            return Random.Range(0, WayPoints.Length);
        }


        /// <summary>
        /// 根据toPos指定的位置得到from本地坐标系下的欧拉角
        /// </summary>
        /// <param name="from"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public Vector3 GetLocalAngleForTarget(Transform from, Vector3 toPos)
        {
            var loc = from.InverseTransformPoint(toPos);
            return Quaternion.LookRotation(loc).eulerAngles;
        }

        //根据toPos指定的位置得到from本地坐标系下坦克的行驶参数
        private void GetTurnAndForwardDelat(Transform from, Vector3 toPos,out float turnDelta,out float forwardDelta)
        {
            var angle = GetLocalAngleForTarget(from, toPos);//获得本地坐标系下的目标角度

            if (angle.y > 180)
                angle.y -= 360;//[-180,180]

            if (Mathf.Abs(angle.y) > MaxTurnDecelerationAngle)       //大于30度则全速转弯
                turnDelta= Mathf.Sign(angle.y);
            else
                turnDelta= angle.y / MaxTurnDecelerationAngle;    //小于30度则根据比例进行转弯     

            //转弯越大 速度就越慢
            forwardDelta = 1 - Mathf.Abs(angle.y) / 180;
        }

        //追逐/追击
        private void Chase(GameObject enemyTank)
        {
            var enemyPos = enemyTank.transform.position + Vector3.up;
            tankMovement.TargetDirection = enemyPos - tankMovement.Gun.position;
            var loc= tankMovement.Gun.InverseTransformPoint(enemyPos);
            var gunAngle = Vector3.Angle(Vector3.forward, loc.normalized);;
            if (Mathf.Abs(gunAngle) <2)
            {
                tankFire.Fire();
            }

            //在自己和敌人中间距离为直径的圆中 随机取一个位置作为目标进行移动  暂没有判断该位置是否可达
            var dis = Vector3.Distance(transform.position, currentChasePos);
            if(dis<StopDistance)
            {
                var enemydis= Vector3.Distance(transform.position, enemyTank.transform.position);
                var enemyDir =(enemyTank.transform.position - transform.position).normalized;
                var chaseCenter = transform.position + enemyDir * enemydis / 2;
                currentChasePos = GetRandomPositionFromCircleRange(chaseCenter, enemydis / 2);                
            }

            //获取向前和旋转值
            GetTurnAndForwardDelat(transform, currentChasePos, out turnDelta, out forwardDelta);

            forwardDelta = 0;
            brakeDelta = BrakeTypes.MaxBrake;
        }



        private Vector3 GetRandomPositionFromCircleRange(Vector3 centerPos,float radius)
        {
            float rad = Random.Range(0, 360)*Mathf.Deg2Rad;
            float tmpRadius = Random.Range(0, radius);

            return new Vector3(Mathf.Cos(rad) * tmpRadius, centerPos.y, Mathf.Sin(rad) * tmpRadius);
        }


        //返回区域 [-180,180]
        private float GetAngle(Vector3 from,Vector3 to,Vector3 up)
        {
            var angle = Vector3.Angle(from, to);
            var normal = Vector3.Cross(from, to);//法向量
            angle *= Mathf.Sign(Vector3.Dot(normal, up));
            return angle;
        }

    }
}

