using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.UI.Panel;

namespace TankGame.UI
{
    public class PanelManager : MonoBehaviour
    {
        public string PanelResourcePath = "Panels/";

        [Tooltip("背景的遮罩panel")]
        public GameObject BackMaskPanel;

        public static PanelManager Instance { get; set; }


        [SerializeField]
        private Transform m_Canvas;
        //private Dictionary<Type,GameObject> m_OpenPanels=new Dictionary<Type,GameObject>();
        private List<GameObject> m_OpenPanels = new List<GameObject>();

        public bool DefaultCursorVisble;
        public CursorLockMode DefaultCursroMode;

        private void Awake()
        {
            if(Instance==null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            m_Canvas = GameObject.FindGameObjectWithTag(Tags.Canvas).transform;
            m_OpenPanels.Clear();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        private void Update()
        {
            var ritbtn= Input.GetKeyDown(KeyCode.Escape);
            if(ritbtn)
            {
                if (m_OpenPanels.Count > 0)
                    ClosePanel(m_OpenPanels[m_OpenPanels.Count - 1].GetComponent<PanelBase>());
                else
                {
                    OpenPanel<MainMenuPanel>();
                }
            }
        }


        public PanelBase OpenPanel<T>()where T:PanelBase
        {

            var PanelObj=Resources.Load<GameObject>(PanelResourcePath + typeof(T).Name);
            if(PanelObj == null)
            {
                Debug.LogError("无法找到panel资源:" + typeof(T).Name + "  路径:" + PanelResourcePath + typeof(T).Name);
                return null;
            }


            var backpanel = Instantiate(BackMaskPanel);
            var gamePanel = Instantiate(PanelObj);

            var panelbase = gamePanel.GetComponent<T>();
            if (panelbase == null)
            {
                Debug.LogError("panel资源无效:" + typeof(T).Name + "  路径:" + PanelResourcePath + typeof(T).Name);
                Destroy(gamePanel);
                Destroy(backpanel);
                return null;
            }

            backpanel.transform.SetParent(m_Canvas, false);
            gamePanel.transform.SetParent(backpanel.transform, false);
            backpanel.transform.SetAsLastSibling();

            m_OpenPanels.Add(gamePanel);
            Cursor.visible=true;
            Cursor.lockState=CursorLockMode.None;
            return panelbase;
        }


        public bool ClosePanel(PanelBase panel)
        {
            Type panelType = panel.GetType();
            if (!m_OpenPanels.Contains(panel.gameObject))
                return false;

            panel.OnCloseing(out bool isClose);
            if (!isClose)
                return false;

            panel.OnClosed();

            //删除父物体 遮罩窗体
            Destroy(panel.gameObject.transform.parent.gameObject);
            m_OpenPanels.Remove(panel.gameObject);


            if(m_OpenPanels.Count<=0)
            {
                Cursor.lockState=DefaultCursroMode;
                Cursor.visible=DefaultCursorVisble;
            }

            return true;
        }


        public void ShowMessageBox(string TipText,MessageBoxResultEvent callback=null)
        {
            var box= OpenPanel<MessageBox>();
            if (box != null)
            {
                (box as MessageBox).Text = TipText;
                if(callback!=null)
                (box as MessageBox).OnResult += callback;
            }

        }

    }
}

