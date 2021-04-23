using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.UI;

public enum MessageBoxResult 
{
    None,
    Ok,
    Cancel,
    Yes,
    No
}
public enum MessageBoxButtons
{
    None,
    OkCancel,
    Ok,
    YesNo
}

public delegate void MessageBoxResultEvent(MessageBoxResult result);

public class MessageBox : PanelBase
{
    [Tooltip("可以到达的原始宽带的最大比例"), Range(1, 2)]
    public float MaxWidthRate = 1.5f;
    [SerializeField]
    private GameObject m_ButtonOK;
    [SerializeField]
    private GameObject m_ButtonCancel;
    [SerializeField]
    private GameObject m_ButtonYes;
    [SerializeField]
    private GameObject m_ButtonNo;

    [SerializeField]
    private Text TipText;
    [SerializeField]
    private GameObject ContextPanel;
    private Vector3 offset;

    private bool IsMax = false;
    private RectTransform tras;
    private RectTransform contrans;
    private RectTransform thistrans;
    private MessageBoxButtons boxButtons=MessageBoxButtons.None;
    private float InitHeight;

    /// <summary>
    /// 获取或设置显示的内容
    /// </summary>
    public string Text
    {
        get
        {
            return TipText.text;
        }
        set
        {
            TipText.text = value;
        }
    }

    public MessageBoxButtons Buttons
    {
        get
        {
            return boxButtons;
        }
        set
        {
            if(boxButtons!=value)
            {
                boxButtons = value;
                UpdateButton();
            }
        }
    }

    public event MessageBoxResultEvent OnResult;

    public override void OnInit(params object[] paramaters)
    {
        tras = TipText.gameObject.transform as RectTransform;
        contrans = ContextPanel.transform as RectTransform;
        thistrans = (transform as RectTransform);
        InitHeight = thistrans.rect.height;
    }


    public void Update()
    {
        offset = Input.mousePosition - transform.position;

        if (!IsMax && tras.rect.height > InitHeight*2.5)
        {
            thistrans.sizeDelta = new Vector2(thistrans.sizeDelta.x * MaxWidthRate, thistrans.sizeDelta.y);
            IsMax = true;
        }
    }

    public new void Close()
    {
        base.Close();
        if(Buttons== MessageBoxButtons.OkCancel)
            OnResult?.Invoke(MessageBoxResult.Cancel);
        else if(Buttons == MessageBoxButtons.YesNo)
            OnResult?.Invoke(MessageBoxResult.No);
        else
            OnResult?.Invoke(MessageBoxResult.None);
    }

    public void OnDrag()
    {
        Debug.Log("drag");
        transform.position = Input.mousePosition - offset;
    }


    public void OnButtonOkClick()
    {
        base.Close();
        OnResult?.Invoke(MessageBoxResult.Ok);
    }

    public void OnButtonCancelClick()
    {
        base.Close();
        OnResult?.Invoke(MessageBoxResult.Cancel);
    }
    public void OnButtonYesClick()
    {
        base.Close();
        OnResult?.Invoke(MessageBoxResult.Yes);
    }
    public void OnButtonNoClick()
    {
        base.Close();
        OnResult?.Invoke(MessageBoxResult.No);
    }


    private void UpdateButton()
    {
        switch(Buttons)
        {
            case MessageBoxButtons.None:
                m_ButtonOK.SetActive(false);
                m_ButtonYes.SetActive(false);
                m_ButtonNo.SetActive(false);
                m_ButtonCancel.SetActive(false);
                break;
            case MessageBoxButtons.YesNo:
                m_ButtonCancel.SetActive(false);
                m_ButtonOK.SetActive(false);
                m_ButtonYes.SetActive(true);
                m_ButtonNo.SetActive(true);
                break;
            case MessageBoxButtons.Ok:
                m_ButtonCancel.SetActive(false);
                m_ButtonOK.SetActive(true);
                m_ButtonYes.SetActive(false);
                m_ButtonNo.SetActive(false);
                break;
            case MessageBoxButtons.OkCancel:
                m_ButtonCancel.SetActive(true);
                m_ButtonOK.SetActive(true);
                m_ButtonYes.SetActive(false);
                m_ButtonNo.SetActive(false);
                break;
        }
    }
}
