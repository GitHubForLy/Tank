using System;
using System.Linq;
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
        private Dictionary<GameObject, PanelBase> m_OpenPanels=new Dictionary<GameObject,PanelBase>();
        //private List<GameObject> m_OpenPanels = new List<GameObject>();

        /// <summary>
        /// 当前打开的最上层Panel
        /// </summary>
        public PanelBase TopOpenPanel => m_OpenPanels.Last().Value;

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
            var enter = Input.GetKeyDown(KeyCode.Return);

#if UNITY_EDITOR
#else
            if (ritbtn)
            {
                if (m_OpenPanels.Count > 0)
                    m_OpenPanels.Last().Value.OnEscape();
                //else
                //{
                //    OpenPanel<MainMenuPanel>();
                //}
            }
#endif
            if (enter)
            {
                if (m_OpenPanels.Count > 0)
                    m_OpenPanels.Last().Value.OnEnter();
            }

        }


        /// <summary>
        /// 打开Panel并管理此panel
        /// </summary>
        public PanelBase OpenPanel(Type panelType,params object[] paramaters) 
        {
            if (!typeof(PanelBase).IsAssignableFrom(panelType)) 
                return null;

            return OpenPanel(PanelResourcePath + panelType.Name, paramaters);        
        }

        /// <summary>
        /// 打开Panel并管理此panel
        /// </summary>
        public T OpenPanel<T>(params object[] paramaters)where T:PanelBase
        {
            return (T)OpenPanel(typeof(T), paramaters);
        }

        /// <summary>
        /// 打开Panel 如果已打开此类型panel 则不打开新的 并管理此panel
        /// </summary>
        public T OpenPanelUnique<T>(params object[] paramaters) where T : PanelBase
        {
            if(m_OpenPanels.Any(m=>m.Value is T))
            {           
                var panel =m_OpenPanels.First(m => m.Value is T);
                panel.Key.transform.parent.SetAsLastSibling();
                return (T)panel.Value;
            }
            return (T)OpenPanel(typeof(T), paramaters);
        }

        /// <summary>
        /// 根据panel名称 打开Panel并管理此panel
        /// </summary>
        public PanelBase OpenPanel(string PanelPath,params object[] paramaters)
        {
            var PanelObj = Resources.Load<GameObject>(PanelPath);
            if (PanelObj == null)
            {
                Debug.LogError("无法找到panel资源:" + PanelPath);
                return null;
            }

            var backpanel = Instantiate(BackMaskPanel);
            var gamePanel = Instantiate(PanelObj);

            var panelbase = gamePanel.GetComponent<PanelBase>();
            if (panelbase == null)
            {
                panelbase = gamePanel.AddComponent<PanelBase>();
                Debug.LogWarning("panel资源没有附加Panelbase组件，已自动附加:" + PanelPath);
            }

            panelbase.OnInit(paramaters);

            backpanel.transform.SetParent(m_Canvas, false);
            gamePanel.transform.SetParent(backpanel.transform, false);
            backpanel.transform.SetAsLastSibling();

            m_OpenPanels.Add(gamePanel, panelbase);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return panelbase;
        }



        /// <summary>
        /// 打开Panel并附加到指定的父物体上 不管理此panel
        /// </summary>
        public T SetupPanel<T>(Transform Parent,params object[] paramaters) where T:PanelBase
        {
            var PanelObj = Resources.Load<GameObject>(PanelResourcePath + typeof(T).Name);
            if (PanelObj == null)
            {
                Debug.LogError("无法找到panel资源:" + typeof(T).Name + "  路径:" + PanelResourcePath + typeof(T).Name);
                return null;
            }

            var gamePanel = Instantiate(PanelObj);

            var panelbase = gamePanel.GetComponent<T>();
            if (panelbase == null)
            {
                Debug.LogError("panel资源无效:" + typeof(T).Name + "  路径:" + PanelResourcePath + typeof(T).Name);
                Destroy(gamePanel);
                return null;
            }

            panelbase.OnInit(paramaters);

            gamePanel.transform.SetParent(Parent, false);
            return panelbase;
        }

        /// <summary>
        /// 关掉之前打开的panel（受管理的） 
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public bool ClosePanel(PanelBase panel)
        {
            Type panelType = panel.GetType();
            if (!m_OpenPanels.ContainsKey(panel.gameObject))
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

        /// <summary>
        /// 关闭所有窗体
        /// </summary>
        public void CloseAll()
        {
            PanelBase panel;
            for(int i=m_OpenPanels.Count-1;i>=0;i--)
            {
                panel = m_OpenPanels.ElementAt(i).Value;
                panel.OnClosed();

                //删除父物体 遮罩窗体
                Destroy(m_OpenPanels.ElementAt(i).Key.transform.parent.gameObject);
                m_OpenPanels.Remove(panel.gameObject);
            }
        }

        public void ShowMessageBox(string TipText)
        {
            ShowMessageBox(TipText,MessageBoxButtons.Ok,null);
        }
        public void ShowMessageBox(string TipText,MessageBoxButtons buttons=MessageBoxButtons.Ok, MessageBoxResultEvent callback = null)
        {
            var box = OpenPanel<MessageBox>();
            if (box != null)
            {
                box.Buttons = buttons;
                box.Text = TipText;
                if (callback != null)
                    box.OnResult += callback;
            }
        }
    }
}

