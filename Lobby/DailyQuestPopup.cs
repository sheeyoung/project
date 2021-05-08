using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
public class DailyQuestPopup : UIBasePanel
{
    [SerializeField] DailyQuestAttendItem attentItem1;
    [SerializeField] DailyQuestAttendItem attentItem2;
    [SerializeField] DailyQuestAttendItem attentItem3;

    [SerializeField] DailyQuestItem slot1;
    [SerializeField] DailyQuestItem slot2;
    [SerializeField] DailyQuestItem slot3;
    [SerializeField] DailyQuestItem slot4;
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
        SetDailyQuest();
    }
    private void SetDailyQuest()
    {
        slot1.Init(1);
        slot2.Init(2);
        slot3.Init(3);
        slot4.Init(4);

        attentItem1.Init(1);
        attentItem2.Init(2);
        attentItem3.Init(3);
    }

    public override void Refresh()
    {
        base.Refresh();
        SetDailyQuest();
    }
}
