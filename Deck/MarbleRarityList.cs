using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System;
using DG.Tweening;

public class MarbleRarityList : MonoBehaviour
{
    [SerializeField] GameObject Content;
    [SerializeField] GameObject MarbleItemObject;

    [SerializeField] GameObject onMark;
    [SerializeField] GameObject offMark;
    [SerializeField] Text rarityNameText;


    private MarbleRarity currentRarity;

    private List<MarbleListItem> marbleItems = new List<MarbleListItem>();

    public Action<int> ClickItemEvent;
    public Func<int, bool> ChcekSelect;


    public void SetList(MarbleRarity rarity, bool showAll = true)
    {
        currentRarity = rarity;

        if (rarityNameText)
        {
            string rarityName = string.Format(UIUtil.GetText("UI_Common_Grade_000"), UIUtil.GetRarityString((int)rarity));
            rarityNameText.text = rarityName;
        }

        List<int> marbles = new List<int>();
        foreach(var item in Sheet.TBL.Marble)
        {
            if (item.Value.Rarity != (int)currentRarity)
                continue;
            if (item.Value.Visiable == 0)
                continue;
            if(showAll == false
                && User.Inst.Doc.MarbleInven.ContainsKey(item.Key) == false)
            {
                continue;
            }
            marbles.Add(item.Key);
        }

        for (int i = 0; i< marbles.Count; i++)
        {
            if(marbleItems.Count > i)
            {
                marbleItems[i].SetData(marbles[i]);
                continue;
            }

            var go = UIManager.Inst.CreateObject(MarbleItemObject, Content.transform);
            if (go == null || go.GetComponent<MarbleListItem>() == null)
                continue;

            var item = go.GetComponent<MarbleListItem>();
            item.SetData(marbles[i]);
            item.ClickAction += OnClickItem;
            item.CheckSelect += IsSelectItem;

            marbleItems.Add(item);
        }
        int emptyItem = 4 - (marbleItems.Count % 4);
        if(emptyItem > 0 && emptyItem < 4)
        {
            for(int i = 0; i < emptyItem; i++)
            {
                var go = UIManager.Inst.CreateObject(MarbleItemObject, Content.transform);
                if (go == null || go.GetComponent<MarbleListItem>() == null)
                    continue;

                var item = go.GetComponent<MarbleListItem>();
                item.SetData(0);
                item.ClickAction += OnClickItem;
                item.CheckSelect += IsSelectItem;

                marbleItems.Add(item);
            }
        }

        if (onMark) onMark.SetActive(openList);
        if (offMark) offMark.SetActive(openList == false);
    }

    private void SetAni(bool isOpen)
    {
        //if(listOpenAni == null)
        //{
        //    listOpenAni = GetComponent<DOTweenAnimation>();
        //    listOpenAni.onComplete.AddListener(completeAction);
        //}

        //if(listOpenAni)
        //{
        //    if (isOpen)
        //        listOpenAni.DORewind();
        //    else
        //        listOpenAni.DORestart();

        //}

        //void completeAction()
        //{
        //    for (int i = 0; i < transform.childCount; i++)
        //    {
        //        var tr = transform.GetChild(i);
        //        tr.gameObject.SetActive(isOpen);
        //    }
        //}
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
        //LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }


    public void Refresh()
    {
        for(int i = 0; i < marbleItems.Count; i++)
        {
            marbleItems[i].Refresh();
        }
    }


    public void OnClickItem(int index)
    {
        Debug.Log("shy - MarbleRarityList - ClickItem");
        ClickItemEvent?.Invoke(index);
    }

    public bool IsSelectItem(int marbleIndex)
    {
        if (ChcekSelect == null)
            return false;
        return ChcekSelect.Invoke(marbleIndex);
    }

    private bool openList = true;
    public void OnClickOpenBtn()
    {
        openList = !openList;
        for (int i = 0; i < transform.childCount; i++)
        {
            var tr = transform.GetChild(i);
            tr.gameObject.SetActive(openList);
        }

        if (onMark) onMark.SetActive(openList);
        if (offMark) offMark.SetActive(openList == false);
    }

    public void RefreshEffect(bool isActive)
    {
        for (int i = 0; i < marbleItems.Count; i++)
            marbleItems[i].RefreshEffect(isActive);
    }
}
