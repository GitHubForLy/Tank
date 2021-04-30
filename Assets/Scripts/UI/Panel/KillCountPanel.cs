using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TankGame.UI.Panel
{
    public delegate void TimeFinishedEventHandle();
    public class KillCountPanel :PanelBase
    {
        [SerializeField]
        private Text m_TargetKillCount;
        [SerializeField]
        private Text m_Time;
        [SerializeField]
        private Text m_RedCount;
        [SerializeField]
        private Text m_BlueCount;

        private TimeSpan timeSpan;

        public event TimeFinishedEventHandle OnTimeFinished;

        public int RedCount
        {
            get
            {
                return int.Parse(m_RedCount.text);
            }
            set
            {
                m_RedCount.text = value.ToString();
            }
        }

        public int BlueCount
        {
            get
            {
                return int.Parse(m_BlueCount.text);
            }
            set
            {
                m_BlueCount.text = value.ToString();
            }
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="Seconds">总秒数</param>
        public void StartTime(int  Seconds)
        {
            StartTime(TimeSpan.FromSeconds(Seconds));
        }

        /// <summary>
        /// 设置当前剩余时间
        /// </summary>
        public void SetRemainTime(int Seconds)
        {
            var span = TimeSpan.FromSeconds(Seconds);
            m_Time.text = span.Minutes + ":" + span.Seconds;
        }

        /// <summary>
        /// 设置目标击杀数
        /// </summary>
        public void SetTargetKillCount(int KillCount)
        {
            m_TargetKillCount.text = "目标:" + KillCount.ToString();
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="time">时间</param>
        public void StartTime(TimeSpan time)
        {
            timeSpan = time;
            StartCoroutine(ProcessTime());
        }

        private IEnumerator ProcessTime()
        {
            m_Time.text = timeSpan.Minutes + ":" + timeSpan.Seconds;
            yield return new WaitForSecondsRealtime(1);
            timeSpan= timeSpan.Add(new TimeSpan(0, 0, -1));

            if(timeSpan.TotalSeconds<=0)
                OnTimeFinished?.Invoke();
            else
                StartCoroutine(ProcessTime());
        }
    }

}
