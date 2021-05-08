using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
public class HelpPopup : UIBasePanel
{
    [SerializeField] Text titleText;
    [SerializeField] Text descText;
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if(param.Length < 2)
        {
            ClosePanel();
            return;
        }

        string title = (string)param[0];
        string desc = (string)param[1];
        titleText.text = title;
        descText.text = desc;
    }
}
