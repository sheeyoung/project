using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarbleDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
{
    public MarbleDeckListItem deckListItem;
    private GraphicRaycaster gr;
    private Camera uiCamera;

    public void SetItem()
    {
        if(gr == null)
        {
            gr = UIManager.Inst.UICanvas.GetComponent<GraphicRaycaster>();
        }
        uiCamera = UIManager.Inst.UICamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        deckListItem.ItemDragBegin();
        //transform.position = new Vector3(eventData.position.x, eventData.position.y, 0);
        //GetComponent<RectTransform>().position = new Vector3(eventData.position.x, eventData.position.y, 0);

        if (uiCamera == null)
            return;
        Vector3 worldPosition = uiCamera.ScreenToWorldPoint(eventData.position);
        GetComponent<RectTransform>().position = new Vector3(worldPosition.x, worldPosition.y, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = new Vector3(eventData.position.x, eventData.position.y, 0);
        //GetComponent<RectTransform>().position = new Vector3(eventData.position.x, eventData.position.y, 0);
        if (uiCamera == null)
            return;
        Vector3 worldPosition = uiCamera.ScreenToWorldPoint(eventData.position);
        GetComponent<RectTransform>().position = new Vector3(worldPosition.x, worldPosition.y, 0);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MarbleDeckItem deckItem = null;

        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(eventData, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go.GetComponent<MarbleDeckItem>() != null)
                {
                    deckItem = go.GetComponent<MarbleDeckItem>();
                    break;
                }

            }
        }
        deckListItem.ItemPointerUp(deckItem);
        transform.localPosition = new Vector3(0f, 0f, 0f);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MarbleDeckItem deckItem = null;

        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(eventData, results);

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go.GetComponent<MarbleDeckItem>() != null)
                {
                    deckItem = go.GetComponent<MarbleDeckItem>();
                    break;
                }

            }
        }
        deckListItem.ItemPointerUp(deckItem);
        transform.localPosition = new Vector3(0f, 0f, 0f);
    }

}
