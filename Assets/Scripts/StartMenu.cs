using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TankGame.UI;
using TankGame.UI.Panel;

namespace TankGame
{
    public class StartMenu : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake() 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PanelManager.Instance.DefaultCursroMode = CursorLockMode.None;
            PanelManager.Instance.DefaultCursorVisble = true;
        }


    }

}
