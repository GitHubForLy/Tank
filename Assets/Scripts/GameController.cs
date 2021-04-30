using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TankGame.UI;
using TankGame.UI.Panel;

namespace TankGame
{

    public class GameController : MonoBehaviour
    {
        public static string CurrentSceneName => SceneManager.GetActiveScene().name;

        private static object[] panelParameters=new object[0];
        private static System.Type StartOpenPanel=typeof(LoginPanel);


        // Start is called before the first frame update
        void Awake() 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PanelManager.Instance.DefaultCursroMode = CursorLockMode.None;
            PanelManager.Instance.DefaultCursorVisble = true;
        }

        void Start()
        {
            if(StartOpenPanel!=null)
                PanelManager.Instance.OpenPanel(StartOpenPanel, panelParameters);
        }

        /// <summary>
        /// 加载场景并打开指定的 panel
        /// </summary>
        public static void LoadScene(string SceneName,System.Type PanelType,params object[] parameters)
        {
            panelParameters = parameters;
            StartOpenPanel = PanelType;
            SceneManager.LoadScene(SceneName);
        }
        /// <summary>
        /// 加载场景并打开指定的 panel
        /// </summary>
        public static void LoadScene<T>(string SceneName, params object[] parameters)
        {
            panelParameters = parameters;
            StartOpenPanel = typeof(T);
            SceneManager.LoadScene(SceneName);
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        public static void LoadScene(string SceneName)
        {
            LoadScene(SceneName, null);
        }
    }

}
