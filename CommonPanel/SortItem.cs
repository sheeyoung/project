using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class SortItem : MonoBehaviour
{
    [SerializeField] GameObject onObject;
    [SerializeField] GameObject offObejct;
    [SerializeField] Text sortItemOnText;
    [SerializeField] Text sortItemOffText;
    [SerializeField] Button itemButton;
    Action<int> clickEvent;


    Func<int, bool> checkSelect;
    int currentIdx;

    public void Init(Action<int> clickEvent, Func<int, bool> checkSelect)
    {
        this.clickEvent = clickEvent;
        this.checkSelect = checkSelect;
        itemButton.onClick.AddListener(OnClickItem);
    }
    public void SetData(int idx, string itemString)
    {
        gameObject.SetActive(true);
        this.currentIdx = idx;
        sortItemOnText.text = itemString;
        sortItemOffText.text = itemString;
        CheckSelectItem();
    }
    public void CheckSelectItem()
    {
        if (checkSelect == null)
            return;
        bool isSelect = checkSelect.Invoke(currentIdx);
        onObject.SetActive(isSelect);
        offObejct.SetActive(isSelect == false);
    }
    private void OnClickItem()
    {
        clickEvent?.Invoke(currentIdx);
    }
}
