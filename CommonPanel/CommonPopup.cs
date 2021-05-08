using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using System;
using UnityEngine.UI;

public class CommonPopup : UIBasePanel
{
    [SerializeField] Text title;
    [SerializeField] Text desc;
    [SerializeField] GameObject leftButtonObject;
    [SerializeField] Text leftButtonText;
    [SerializeField] GameObject rightButtonObject;
    [SerializeField] Text rightButtonText;

    Action leftButtonClickEvent;
    Action rightButtonClickEvent;
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }

    //0 : title
    //1 : desc
    //2 : rightButtonText
    //3 : rightButton Action
    //4 : bool ShowLeftButton
    //5 : leftButtonText
    //6 : leftButton Action
    //7 : bg close 
    //8 : auto close
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        if (param.Length > 7)
        {
            bool isclosebg = (bool)param[7];
            if (!isclosebg)
                CloseBg = null;
        }

        base.OpenPanel(panelId, param);

        title.text = (string)param[0];
        desc.text = (string)param[1];

        rightButtonClickEvent = null;
        leftButtonClickEvent = null;

        rightButtonText.text = (string)param[2];
        rightButtonClickEvent = (Action)param[3];

        bool showLeftButton = (bool)param[4];
        leftButtonObject.SetActive(showLeftButton);
        if (showLeftButton)
        {
            leftButtonText.text = (string)param[5];
            leftButtonClickEvent = (Action)param[6];
        }

        if (param.Length > 8)
        {
            bool isautoclose = (bool)param[8];
            if (isautoclose)
                StartCoroutine(AutoClose(2));
        }
    }

    protected override void SetEvent()
    {
        base.SetEvent();

        leftButtonObject.GetComponent<Button>().onClick.AddListener(OnClickLeftButton);
        rightButtonObject.GetComponent<Button>().onClick.AddListener(OnClickRightButton);
    }


    private void OnClickLeftButton()
    {
        leftButtonClickEvent?.Invoke();
        ClosePanel();
    }

    private void OnClickRightButton()
    {
        rightButtonClickEvent?.Invoke();
        ClosePanel();
    }

    private IEnumerator AutoClose(float t)
    {
        yield return new WaitForSeconds(t);
        if (gameObject.activeSelf)
            ClosePanel();
    }
}
