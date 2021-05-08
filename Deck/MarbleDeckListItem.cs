using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MarbleDeckListItem : MarbleListItem
{ 
    [Header("DragItem")]
    [SerializeField] private MarbleDragItem dragItem;
    [SerializeField] private GameObject dragItemParent;

    private MarbleDeckEditPopup deckEditPopup;

    public override void SetData(int idx)
    {
        base.SetData(idx);
        if (dragItem)
        {
            dragItem.deckListItem = this;
            dragItem.SetItem();
        }
        
    }

    public void ItemDragBegin()
    {
        if (deckEditPopup == null)
        {
            deckEditPopup = UIManager.Inst.GetActivePanel<MarbleDeckEditPopup>(UI.UIPanel.MarbleDeckEditPopup);
        }
        dragItem.transform.SetParent(deckEditPopup.transform);
        OnClickItem();
    }
    public void ItemPointerUp(MarbleDeckItem hoverItem)
    {
        dragItem.transform.SetParent(dragItemParent.transform);
        if(hoverItem) hoverItem.OnClickItem();
    }
}
