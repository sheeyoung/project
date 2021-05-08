using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System;

public class SortPopup : UIBasePanel
{
    [SerializeField] Text titleText;
    [SerializeField] Text descText;

    [SerializeField] Button closeBtn;
    [SerializeField] GameObject listContent;
    [SerializeField] GameObject sortItemObject;

    List<SortItem> sortItems;
    private SortingData sortData;

    public Action closeAction;


    //0 : title
    //1 : desc
    //2 : closeAction
    //3 : SortingData
    public override void OpenPanel(UIPanel panel, params object[] data)
    {
        base.OpenPanel(panel, data);
        if (data == null || data.Length <= 0)
        {
            ClosePanel();
            return;
        }
        if(titleText) titleText.text = (string)data[0];
        if(descText) descText.text = (string)data[1];
        closeAction = (Action)data[2];
        sortData = data[3] as SortingData;

        SetList();
    }

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        sortItems = new List<SortItem>();
    }

    protected override void SetEvent()
    {
        base.SetEvent();
        closeBtn.onClick.AddListener(OnClickClose);
    }

    private void SetList()
    {
        for (int i = 0; i < sortItems.Count; i++)
            sortItems[i].gameObject.SetActive(false);
        for (int i = 0; i < sortData.sortNames.Count; i++)
        {
            if (sortItems.Count <= i)
            {
                var go = Instantiate(sortItemObject);
                UIUtil.SetParent(listContent, go);
                var sortItemComp = go.GetComponent<SortItem>();
                sortItemComp.Init(OnClickItem, CheckSelectItem);
                sortItems.Add(sortItemComp);
            }
            sortItems[i].SetData(i, sortData.sortNames[i]);
        }
    }

    private void RefreshItems()
    {
        for (int i = 0; i < sortItems.Count; i++)
        {
            if (sortItems[i].gameObject.activeSelf)
                sortItems[i].CheckSelectItem();
        }
    }

    public void OnClickClose()
    {
        ClosePanel();
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        closeAction = null;
    }

    public void OnClickItem(int index)
    {
        sortData.itemClickEvent?.Invoke(index);
        sortData.currentSortIndex = index;
        RefreshItems();
    }
    public bool CheckSelectItem(int index)
    {
        return sortData.currentSortIndex.Equals(index);
    }


}

public class SortingData
{
    public int currentSortIndex;
    public List<string> sortNames = new List<string>();
    public Action<int> itemClickEvent;
}
