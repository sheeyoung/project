using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class MarblePanel : UIBaseGrp
{
    [Header("Deck")]
    [SerializeField] MarbleDeckGrp marbleDeckGrp;
    [Header("Info")]
    [SerializeField] Text criDamText;
    [Header("Marble")]
    [SerializeField] MarbleRarityList normalList;
    [SerializeField] MarbleRarityList rareList;
    [SerializeField] MarbleRarityList epicList;
    [SerializeField] MarbleRarityList legendList;
    [SerializeField] MarbleRarityList chronicleList;
    [SerializeField] ScrollRect scrollRectGrp;
    protected override void SetEvent()
    {
        base.SetEvent();
        normalList.ClickItemEvent += OnClickMarbleItem;
        rareList.ClickItemEvent += OnClickMarbleItem;
        epicList.ClickItemEvent += OnClickMarbleItem;
        legendList.ClickItemEvent += OnClickMarbleItem;
        chronicleList.ClickItemEvent += OnClickMarbleItem;
    }

    public override void Init()
    {
        base.Init();
        SetDeck();
        SetCriDam();
        SetMarbleList();
    }

    private void SetDeck()
    {
        if(marbleDeckGrp)
        {
            marbleDeckGrp.InitDeck(User.Inst.Doc.SelectedDeck);
            marbleDeckGrp.marbleDeckButtonClickEvnet += OnclickDeckItem;
        }
    }
    private void SetCriDam()
    {
        if (criDamText) criDamText.text = string.Format(UIUtil.GetText("UI_Marble_Panel_002"), (Sheet.TBL.Const["CONST_MARBLE_CRIDMG_INIT"].Value + User.Inst.Doc.AddedCriDamage) / 100f);
    }

    private void SetMarbleList()
    {
        normalList.SetList(MarbleRarity.Normal);
        rareList.SetList(MarbleRarity.Rare);
        epicList.SetList(MarbleRarity.Epic);
        legendList.SetList(MarbleRarity.Legendary);
        chronicleList.SetList(MarbleRarity.Chronicle);
    }

    public override void Refresh()
    {
        if (marbleDeckGrp)
            marbleDeckGrp.InitDeck(User.Inst.Doc.SelectedDeck);
        SetCriDam();

        if (normalList) normalList.Refresh();
        if (rareList) rareList.Refresh();
        if (epicList) epicList.Refresh();
        if (legendList) legendList.Refresh();
        if (chronicleList) chronicleList.Refresh();

    }


    public void OnClickMarbleItem(int marbleIdx)
    {
        UIManager.Inst.OpenPanel(UIPanel.MarbleInfoPopup, marbleIdx);
    }
    public void OnclickDeckItem(int deckCount)
    {
        try
        {
            int marbleIdx = User.Inst.Doc.Decks[User.Inst.Doc.SelectedDeck][deckCount];
            UIManager.Inst.OpenPanel(UIPanel.MarbleInfoPopup, marbleIdx);

        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void OnClickDeckEditBtn()
    {
        UIManager.Inst.OpenPanel(UIPanel.MarbleDeckEditPopup, MarbleDeckEditPopup.EditType.All);
    }

    public override void ScrollCompSwitch(bool isOn)
    {
        base.ScrollCompSwitch(isOn);
        scrollRectGrp.enabled = isOn;
    }

    public override void RefreshEffect(bool isActive)
    {
        marbleDeckGrp.RefreshEffect(isActive);
        normalList.RefreshEffect(isActive);
        rareList.RefreshEffect(isActive);
        epicList.RefreshEffect(isActive);
        legendList.RefreshEffect(isActive);
        chronicleList.RefreshEffect(isActive);
    }

}
