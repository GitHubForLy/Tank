﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.Player;
using UnityEngine.SceneManagement;
using System;

namespace TankGame.UI.Panel
{
    public class MainMenuPanel : PanelBase
    {
        private float lastTimescale;
        protected override void OnInit()
        {
            base.OnInit();
            lastTimescale = Time.timeScale;
            Time.timeScale = 0;

        }

        public override void OnClosed()
        {
            base.OnClosed();
            Time.timeScale = lastTimescale;
        }

        public void Resume()
        {
            Close();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

    }
}
