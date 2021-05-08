using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net;
using Net.Api;

public class MarbleDeckGrp : MonoBehaviour
{
    [SerializeField]
    private List<MarbleDeckItem> DeckItems = new List<MarbleDeckItem>();

    [Header("DeckButton")]
    [SerializeField] Button Deck1Btn;
    [SerializeField] Button Deck2Btn;
    [SerializeField] Button Deck3Btn;

    [Header("DeckButton On")]
    [SerializeField] GameObject Deck1BtnOn;
    [SerializeField] GameObject Deck2BtnOn;
    [SerializeField] GameObject Deck3BtnOn;

    [Header("DeckButton Off")]
    [SerializeField] GameObject Deck1BtnOff;
    [SerializeField] GameObject Deck2BtnOff;
    [SerializeField] GameObject Deck3BtnOff;

    public int currentShowDeckCount { private set; get; }

    public Action<int> marbleDeckButtonClickEvnet;
    public Func<int, bool> CheckSelectEvent;
    public Action<int> deckButtonClickEvent;

    void Start()
    {
        for (int i = 0; i < DeckItems.Count; i++)
        {
            DeckItems[i].ClickAction += OnClickMarbleDeckItem;
            DeckItems[i].CheckSelectDeck += IsSelectMarbleDeck;
        }
        if (Deck1Btn) Deck1Btn.onClick.AddListener(() => { OnClickChangeDeck(1); });
        if (Deck2Btn) Deck2Btn.onClick.AddListener(() => { OnClickChangeDeck(2); });
        if (Deck3Btn) Deck3Btn.onClick.AddListener(() => { OnClickChangeDeck(3); });
    }
    public void Refresh(List<int> refreshDeck)
    {
        for (int i = 0; i < DeckItems.Count; i++)
        {
            if (refreshDeck.Count <= i)
                break;
            DeckItems[i].SetData(refreshDeck[i]);
        }
    }
    public void Refresh()
    {
        for (int i = 0; i < DeckItems.Count; i++)
        {
            DeckItems[i].Refresh();
        }
    }
    public void InitDeck(int currentDeck)
    {
        currentShowDeckCount = currentDeck;// User.Inst.Doc.SelectedDeck;

        var decks = User.Inst.Doc.Decks[User.Inst.Doc.SelectedDeck];

        for (int i = 0; i < DeckItems.Count; i++)
        {
            if (decks.Count <= i)
                break;
            DeckItems[i].SetData(decks[i], i);
        }

        SetDeckButton();
    }

    private void SetDeckButton()
    {
        if (Deck1BtnOn) Deck1BtnOn.SetActive(currentShowDeckCount == 1);
        if (Deck1BtnOff) Deck1BtnOff.SetActive(currentShowDeckCount != 1);

        if (Deck2BtnOn) Deck2BtnOn.SetActive(currentShowDeckCount == 2);
        if (Deck2BtnOff) Deck2BtnOff.SetActive(currentShowDeckCount != 2);

        if (Deck3BtnOn) Deck3BtnOn.SetActive(currentShowDeckCount == 3);
        if (Deck3BtnOff) Deck3BtnOff.SetActive(currentShowDeckCount != 3);
    }

    private void OnClickChangeDeck(int selectDeck)
    {
        if (currentShowDeckCount == selectDeck)
            return;
        ImplBase.Actor.Parser<ChangeSelectedDeck.Ack>(new ChangeSelectedDeck.Req() { DeckCount = selectDeck }, (ack) =>
         {
             InitDeck(User.Inst.Doc.SelectedDeck);
             deckButtonClickEvent?.Invoke(selectDeck);
         });
    }

    public void OnClickMarbleDeckItem(int deckCount)
    {
        Debug.Log("shy - MarbleDeckGrp - ClickDeckItem");
        marbleDeckButtonClickEvnet?.Invoke(deckCount);
    }

    public bool IsSelectMarbleDeck(int deckCount)
    {
        if (CheckSelectEvent == null)
            return false;
        return CheckSelectEvent.Invoke(deckCount);
    }

    public void RefreshEffect(bool isActive)
    {
        for(int i = 0; i < DeckItems.Count; i++)
        {
            DeckItems[i].RefreshEffect(isActive);
        }
    }


}
