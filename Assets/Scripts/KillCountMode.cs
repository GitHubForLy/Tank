using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.UI;
using TankGame.UI.Panel;
using TankGame.TankBehaviour;
using TankGame.Net;
using DataModel;
using ServerCommon;
using System;
using UnityEngine.UI;

namespace TankGame
{
    public class KillCountMode : MonoBehaviour
    {
        [Tooltip("复活时间")]
        public float ReviveTime = 3.0f;

        public GameObject ResultTip;


        private GameObject canvas;
        private KillCountPanel countPanel;
        private int redKillCount=0;
        private int blueKillCount = 0;
        // Start is called before the first frame update

        void Start()
        {
            canvas = GameObject.FindGameObjectWithTag(Tags.Canvas);
            countPanel= PanelManager.Instance.SetupPanel<KillCountPanel>(canvas.transform);
            //countPanel.StartTime(MaxSeconds);
            //countPanel.OnTimeFinished += CountPanel_OnTimeFinished;
            countPanel.SetTargetKillCount(BattlegroundManager.Room.Setting.TargetKillCount);

            ResultTip.SetActive(false);
            NetManager.Instance.OnReceiveBroadcast += Instance_OnReceiveBroadcast;
        }

        
        public IEnumerator Revive(GameObject deadTank)
        {
            yield return new WaitForSecondsRealtime(ReviveTime);
            BattlegroundManager.Instance.Revive(deadTank);
        }

        private void Instance_OnReceiveBroadcast((string action, ServerCommon.IDynamicType subdata) msg)
        {
            if (msg.action == DataModel.BroadcastActions.Die)
            {
                OnPlayerDie(msg.subdata);
            }
            else if (msg.action == DataModel.BroadcastActions.RemainingTime)
            {
                OnUpdateRemainingTime(msg.subdata);
            }
            //else if (msg.action == BroadcastActions.GameFinished)
            //    OnGameFinished(msg.subdata);
        }

        //private void OnGameFinished(IDynamicType subdata)
        //{
        //    var team = subdata.GetValue<int>();
        //    StartCoroutine(GameEnd(team));
        //}

        private void OnPlayerDie(IDynamicType subdata)
        {
            (string dieAccount, string killerAccount) data = subdata.GetValue<(string, string)>();
            if (BattlegroundManager.Instance.Tanks[data.killerAccount].Instance.GetComponent<TankTeam>().TeamNumber == 0)
            {
                redKillCount++;
                countPanel.RedCount = redKillCount;
            }
            else
            {
                blueKillCount++;
                countPanel.BlueCount = blueKillCount;
            }

            StartCoroutine(Revive(BattlegroundManager.Instance.Tanks[data.dieAccount].Instance));

            if (redKillCount >= BattlegroundManager.Room.Setting.TargetKillCount)
                StartCoroutine(GameEnd(0));
            else if (blueKillCount >= BattlegroundManager.Room.Setting.TargetKillCount)
                StartCoroutine(GameEnd(1));
        }

        private IEnumerator GameEnd(int WinTeam)
        {
            int win = WinTeam == -1 ? -1 : (WinTeam == BattlegroundManager.Instance.LocalTank.TeamNumber ? 1 : 0);

            if (win == -1)
                ResultTip.transform.Find("Text").GetComponent<Text>().text = "平局";
            else if(win == 1)
                ResultTip.transform.Find("Text").GetComponent<Text>().text = "胜利";
            else
                ResultTip.transform.Find("Text").GetComponent<Text>().text = "失败";
            ResultTip.SetActive(true);


            yield return new WaitForSecondsRealtime(3f);

            if (win == 1)
                DoWin();
            else if (WinTeam == -1)  //平局
                DogFall();
            else
                DoFail();
        }

        /// <summary>
        /// 平局
        /// </summary>
        private void DogFall()
        {
            GameController.LoadScene<ResultPanel>("StartScene", -1,BattlegroundManager.Room);
        }

        public void DoWin()
        {
            GameController.LoadScene<ResultPanel>("StartScene",1, BattlegroundManager.Room);
        }
        public void DoFail()
        {
            GameController.LoadScene<ResultPanel>("StartScene",0, BattlegroundManager.Room);
        }

        private void OnUpdateRemainingTime(IDynamicType subData)
        {
            int remain = subData.GetValue<int>();
            countPanel.SetRemainTime(remain);
            if (remain <= 0)
                CountPanel_OnTimeFinished();
        }

        private void CountPanel_OnTimeFinished()
        {
            Debug.Log("finished");
            if (redKillCount > blueKillCount)
                StartCoroutine(GameEnd(0));
            else if(blueKillCount > redKillCount)
                StartCoroutine(GameEnd(1));
            else
                StartCoroutine(GameEnd(-1));
        }

        void OnDestroy()
        {
            //countPanel.OnTimeFinished -= CountPanel_OnTimeFinished;
            NetManager.Instance.OnReceiveBroadcast -= Instance_OnReceiveBroadcast;
        }
    }
}

