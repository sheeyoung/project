using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace App.UI
{
    public class UIEventButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private UnityAction OnClickDown;
        private UnityAction OnClickUp;
        private UnityAction OnClick;

        public void AddClickDownEvnet(UnityAction action)
        {
            Graphic[] graphics = gameObject.GetComponents<Graphic>();
            for (int i = 0; i < graphics.Length; ++i)
            {
                Text text = graphics[i].gameObject.GetComponent<Text>();
                if (text != null)
                    graphics[i].raycastTarget = false;
                else if (!graphics[i].raycastTarget)
                    graphics[i].raycastTarget = true;
            }
            OnClickDown += action;
        }
        public void RemoveClickDownEvnet(UnityAction action)
        {
            OnClickDown -= action;
        }


        public void AddClickUpEvnet(UnityAction action)
        {
            Graphic[] graphics = gameObject.GetComponents<Graphic>();
            for (int i = 0; i < graphics.Length; ++i)
            {
                Text text = graphics[i].gameObject.GetComponent<Text>();
                if (text != null)
                    graphics[i].raycastTarget = false;
                else if (!graphics[i].raycastTarget)
                    graphics[i].raycastTarget = true;
            }
            OnClickUp += action;
        }

        public void RemoveClickUpEvnet(UnityAction action)
        {
            OnClickUp -= action;
        }



        public void AddClickEvnet(UnityAction action)
        {
            Graphic[] graphics = gameObject.GetComponents<Graphic>();
            for (int i = 0; i < graphics.Length; ++i)
            {
                Text text = graphics[i].gameObject.GetComponent<Text>();
                if (text != null)
                    graphics[i].raycastTarget = false;
                else if (!graphics[i].raycastTarget)
                    graphics[i].raycastTarget = true;
            }
            OnClick += action;
        }

        public void RemoveClickEvnet(UnityAction action)
        {
            OnClick -= action;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (OnClickDown != null)
                OnClickDown();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (OnClickUp != null)
                OnClickUp();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick != null)
                OnClick();
        }
    }
}
