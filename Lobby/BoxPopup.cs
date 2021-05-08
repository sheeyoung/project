using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net.Impl;

public class BoxPopup : UIBasePanel
{
    [Header("Asset")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text GemText;

    [SerializeField] BoxPopupItem box1;
    [SerializeField] BoxPopupItem box2;
    [SerializeField] BoxPopupItem box3;
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

        box1.Init(1);
        box2.Init(2);
        box3.Init(3);
    }
    public override void UpdateAsset()
    {
        if (GoldText)
        {
            long goldCount = BaseImpl.Inst.GetAsset(User.Inst, Net.ASSETS.ASSET_GOLD);
            GoldText.text = goldCount.ToString("n0");
        }
        if (GemText)
        {
            long diamondCount = BaseImpl.Inst.GetAsset(User.Inst, Net.ASSETS.ASSET_DIAMOND);
            GemText.text = diamondCount.ToString("n0");
        }
    }
    public override void Refresh()
    {
        base.Refresh();
        box1.Init(1);
        box2.Init(2);
        box3.Init(3);
    }
}
