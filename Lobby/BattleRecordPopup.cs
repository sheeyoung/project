using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Net;
using Net.Api;
using Libs.Utils;

public class BattleRecordPopup : UIBasePanel
{
    [SerializeField] GameObject emptyGrp;
    [SerializeField] BattleRecordList recordList;
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
        emptyGrp.SetActive(true);
        recordList.gameObject.SetActive(false);
        UIManager.Inst.ShowConnectingPanel(true);
        ImplBase.Actor.Parser<GameHistoryGet.Ack>(new GameHistoryGet.Req(), (ack) =>
        {
            Debug.Log(JsonLib.Encode(ack));
            UIManager.Inst.ShowConnectingPanel(false);
            if (ack.Result != RESULT.OK)
                return;
            if (ack.List.Count > 0)
            {
                emptyGrp.SetActive(false);
                recordList.gameObject.SetActive(true);
                recordList.SetList(ack.List);
            }
        });
    }


}
