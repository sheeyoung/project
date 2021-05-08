using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Net;
using Net.Api;
using scene;
public class MarbleDeckEditPopup : UIBasePanel
{
    [SerializeField] MarbleDeckGrp marbleDeckGrp;

    [Header("AllItem")]
    [SerializeField] GameObject allItemObject;
    [SerializeField] MarbleRarityList normalList;
    [SerializeField] MarbleRarityList rareList;
    [SerializeField] MarbleRarityList epicList;
    [SerializeField] MarbleRarityList legendList;
    [SerializeField] MarbleRarityList chronicleList;

    [Header("SelectItem")]
    [SerializeField] GameObject selectItemObject;
    [SerializeField] MarbleDeckListItem selectItem;

    private MarbleDragItem dragItem;


    public enum EditType
    {
        All,
        Select,
    }
    private EditType currentEditType;
    //private Dictionary<int, int> editDeck;
    private Dictionary<int, Dictionary<int, int>> editDeck;
    private const int DeckMarbleMaxCount = 5;
    private const int DeckMax = 3;

    public int SelectMarbleIdx { get; private set; }
    public int SelectDeckCount { get; private set; }

    private const int NotSelct = -1;

    protected override void Init(UIPanel windowId)
    {
        base.Init(windowId);

        editDeck = new Dictionary<int, Dictionary<int, int>>();

        for (int i = 1; i <= DeckMax; i++)
        {
            if (editDeck.ContainsKey(i) == false)
                editDeck[i] = new Dictionary<int, int>();
            for(int mCount = 0; mCount < DeckMarbleMaxCount; mCount++)
            {
                editDeck[i].Add(mCount, 0);
            }
        }

        marbleDeckGrp.marbleDeckButtonClickEvnet += OnClickMarbleDeckItem;
        marbleDeckGrp.CheckSelectEvent += IsSelectDeck;
        marbleDeckGrp.deckButtonClickEvent += OnClickDeckChangeButton;

        normalList.ClickItemEvent += OnClickListItem;
        normalList.ChcekSelect += isSelectMarbleItem;

        rareList.ClickItemEvent += OnClickListItem;
        rareList.ChcekSelect += isSelectMarbleItem;

        epicList.ClickItemEvent += OnClickListItem;
        epicList.ChcekSelect += isSelectMarbleItem;

        legendList.ClickItemEvent += OnClickListItem;
        legendList.ChcekSelect += isSelectMarbleItem;

        chronicleList.ClickItemEvent += OnClickListItem;
        chronicleList.ChcekSelect += isSelectMarbleItem;
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (param.Length >= 1)
            currentEditType = (EditType)param[0];
        else
            currentEditType = EditType.All;

        SetDeck();

        if(currentEditType == EditType.Select
            && param.Length < 2)
        {
            currentEditType = EditType.All;
        }

        selectItemObject.SetActive(false);
        allItemObject.SetActive(false);

        if (currentEditType == EditType.All)
        {
            allItemObject.SetActive(true);
            SelectMarbleIdx = NotSelct;
            SetMarbleList();
        }
        else
        {
            selectItemObject.SetActive(true);
            SelectMarbleIdx = (int)param[1];
            SetSelectMarble();
        }

        SelectDeckCount = NotSelct;

        for(int i = 1; i <= DeckMax; i++)
        {
            if (editDeck.ContainsKey(i) == false)
                continue;
            if (User.Inst.Doc.Decks.ContainsKey(i) == false)
                continue;

            foreach (var marbleItem in User.Inst.Doc.Decks[i])
            {
                if (editDeck[i].ContainsKey(marbleItem.Key) == false)
                    continue;
                editDeck[i][marbleItem.Key] = marbleItem.Value;
            }   

        }
    }

    private void SetDeck()
    {
        if (marbleDeckGrp)
        {
            marbleDeckGrp.InitDeck(User.Inst.Doc.SelectedDeck);
        }
    }

    private void SetMarbleList()
    {
        if(normalList) normalList.SetList(MarbleRarity.Normal, false);
        if(rareList) rareList.SetList(MarbleRarity.Rare, false);
        if(epicList) epicList.SetList(MarbleRarity.Epic, false);
        if(legendList) legendList.SetList(MarbleRarity.Legendary, false);
        if(chronicleList) chronicleList.SetList(MarbleRarity.Chronicle, false);
    }
    private void SetSelectMarble()
    {
        selectItem.SetData(SelectMarbleIdx);
    }

    public override void Refresh()
    {
        List<int> newDeck = new List<int>();
        for(int i = 0; i < DeckMarbleMaxCount; i++)
        {
            newDeck.Add(editDeck[User.Inst.Doc.SelectedDeck][i]);
        }
        marbleDeckGrp.Refresh(newDeck);

        if(currentEditType == EditType.All)
        {
            normalList.Refresh();
            rareList.Refresh();
            epicList.Refresh();
            legendList.Refresh();
            chronicleList.Refresh();
        }
        else
        {

        }
    }
    public override void ClosePanel()
    {
        bool isSame = true;
        foreach(var item in User.Inst.Doc.Decks)
        {
            for (int i = 0; i < DeckMarbleMaxCount; i++)
            {
                if (editDeck[item.Key][i] != item.Value[i])
                    isSame = false;
            }
        }
        if(isSame == false)
        {
            UIManager.Inst.ShowConfirm2BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Marble_Deck_Edit_002"), UIUtil.GetText("UI_Common_Leave"), Close, UIUtil.GetText("UI_Common_Save"), SaveAndClose);
        }
        else
        {
            base.ClosePanel();
        }
        void SaveAndClose()
        {
            OnClickSaveBtn();
        }
        void Close()
        {
            base.ClosePanel();
        }
    }

    #region 클릭 이벤트
    public void OnClickMarbleDeckItem(int deckCount)
    {
        if (currentEditType == EditType.Select)
        {
            if (editDeck[User.Inst.Doc.SelectedDeck].ContainsKey(deckCount))
            {
                if (editDeck[User.Inst.Doc.SelectedDeck].ContainsValue(SelectMarbleIdx))
                {
                    int curKey = -1;
                    foreach (var item in editDeck[User.Inst.Doc.SelectedDeck])
                    {
                        if (item.Value == SelectMarbleIdx)
                        {
                            curKey = item.Key;
                            break;
                        }
                    }
                    if (curKey >= 0)
                    {
                        editDeck[User.Inst.Doc.SelectedDeck][curKey] = editDeck[User.Inst.Doc.SelectedDeck][deckCount];
                    }
                }
                editDeck[User.Inst.Doc.SelectedDeck][deckCount] = SelectMarbleIdx;
            }
        }
        else
        {
            if (SelectMarbleIdx == NotSelct)
            {
                if(SelectDeckCount != NotSelct)
                {
                    int curVal = editDeck[User.Inst.Doc.SelectedDeck][SelectDeckCount];
                    editDeck[User.Inst.Doc.SelectedDeck][SelectDeckCount] = editDeck[User.Inst.Doc.SelectedDeck][deckCount];
                    editDeck[User.Inst.Doc.SelectedDeck][deckCount] = curVal;

                    SelectDeckCount = NotSelct;
                }
                else if(editDeck[User.Inst.Doc.SelectedDeck].ContainsKey(deckCount))
                    SelectDeckCount = deckCount;
            }
            else
            {
                if (editDeck[User.Inst.Doc.SelectedDeck].ContainsKey(deckCount))
                {
                    if(editDeck[User.Inst.Doc.SelectedDeck].ContainsValue(SelectMarbleIdx))
                    {
                        int curKey = -1;
                        foreach(var item in editDeck[User.Inst.Doc.SelectedDeck])
                        {
                            if(item.Value == SelectMarbleIdx)
                            {
                                curKey = item.Key;
                                break;
                            }
                        }
                        if(curKey >= 0)
                        {
                            editDeck[User.Inst.Doc.SelectedDeck][curKey] = editDeck[User.Inst.Doc.SelectedDeck][deckCount];
                        }
                    }
                    editDeck[User.Inst.Doc.SelectedDeck][deckCount] = SelectMarbleIdx;
                    SelectMarbleIdx = NotSelct;
                }
            }
        }

        Refresh();

    }

    public void OnClickListItem(int marbleIdx)
    {
        if (currentEditType == EditType.Select)
            return;

        if (SelectDeckCount == NotSelct)
        {
            SelectMarbleIdx = marbleIdx;
        }
        else
        {
            int curKey = -1;
            foreach (var item in editDeck[User.Inst.Doc.SelectedDeck])
            {
                if (item.Value == marbleIdx)
                {
                    curKey = item.Key;
                    break;
                }
            }
            if(curKey >= 0)
            {
                editDeck[User.Inst.Doc.SelectedDeck][curKey] = editDeck[User.Inst.Doc.SelectedDeck][SelectDeckCount];
            }

            editDeck[User.Inst.Doc.SelectedDeck][SelectDeckCount] = marbleIdx;
            SelectDeckCount = NotSelct;

        }
        Refresh();
    }

    int saveCount;
    public void OnClickSaveBtn()
    {
        //SaveDeck
        saveCount = 1;
        SaveDeck();
        //SaveDeck.Req req = new SaveDeck.Req();
        //req.DeckCount = User.Inst.Doc.SelectedDeck;
        //req.Deck = new List<int>();
        //for(int i =0; i < DeckMarbleMaxCount; i++)
        //{
        //    req.Deck.Add(editDeck[User.Inst.Doc.SelectedDeck][i]);
        //}
        //ImplBase.Actor.Parser<SaveDeck.Ack>(req, (ack) => {
        //    Lobby.Instance.lobbyPanel.RefreshDeck();
        //    ClosePanel();
        //});
    }

    private void SaveDeck()
    {
        bool isSame = true;
        for(int i =0; i < DeckMarbleMaxCount; i++)
        {
            if (editDeck[saveCount][i] != User.Inst.Doc.Decks[saveCount][i])
                isSame = false;
        }
        if(isSame)
        {

            Debug.Log("SaveDeck isSame : " + saveCount);
            saveCount++;
            if (saveCount > DeckMax)
            {
                Lobby.Instance.lobbyPanel.RefreshDeck();
                base.ClosePanel();
            }
            else
                SaveDeck();
            return;
        }
        SaveDeck.Req req = new SaveDeck.Req();
        req.DeckCount = saveCount;
        req.Deck = new List<int>();
        for (int i = 0; i < DeckMarbleMaxCount; i++)
        {
            req.Deck.Add(editDeck[saveCount][i]);
        }
        ImplBase.Actor.Parser<SaveDeck.Ack>(req, (ack) => {
            saveCount++;
            if (saveCount > DeckMax)
            {
                Lobby.Instance.lobbyPanel.RefreshDeck();
                base.ClosePanel();
            }
            else
                SaveDeck();
        });
    }

    public void OnClickDeckChangeButton(int selectDeck)
    {
        Refresh();
    }
    #endregion


    #region Action
    public bool IsSelectDeck(int deckCount)
    {
        return SelectDeckCount.Equals(deckCount);
    }

    public bool isSelectMarbleItem(int marbleIndex)
    {
        return SelectMarbleIdx.Equals(marbleIndex);
    }

    #endregion

}
