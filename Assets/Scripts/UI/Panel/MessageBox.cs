using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TankGame.UI;

public enum MessageBoxResult 
{
    //Ok,
    //Cancel,
    //Yes,
    No
}
public enum MessageBoxButtons
{
    None,
    //OkCancel,
    //Ok,
    //YesNo
}

public delegate void MessageBoxResultEvent(MessageBoxResult result);

public class MessageBox : PanelBase
{
    [Tooltip("可以到达的原始宽带的最大比例"), Range(1, 2)]
    public float MaxWidthRate = 1.5f;

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
                UpdateButton();
            }
            boxButtons = value;
        }
    }

    public event MessageBoxResultEvent OnResult;

    protected override void OnInit()
    {
        tras = TipText.gameObject.transform as RectTransform;
        contrans = ContextPanel.transform as RectTransform;
        thistrans = (transform as RectTransform);
    }


    public void Update()
    {
        offset = Input.mousePosition - transform.position;

        if (tras.rect.height > contrans.rect.height)
        {
            thistrans.sizeDelta = new Vector2(thistrans.sizeDelta.x, thistrans.sizeDelta.y + tras.rect.height - contrans.rect.height+5);
        }

        if (!IsMax && tras.rect.height > contrans.rect.height)
        {
            thistrans.sizeDelta = new Vector2(thistrans.sizeDelta.x * MaxWidthRate, thistrans.sizeDelta.y);
            IsMax = true;
        }
    }

    public void Close()
    {
        base.Close();
        OnResult?.Invoke(MessageBoxResult.No);
    }

    public void OnDrag()
    {
        Debug.Log("drag");
        transform.position = Input.mousePosition - offset;
    }

    private void UpdateButton()
    {

    }
}
