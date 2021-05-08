using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Events;

namespace UI
{
    public class UIBasePanel : BaseMono
    {
        protected RectTransform myRectTransform = null;
        public RectTransform rectTransform
        {
            get
            {
                if (myRectTransform == null)
                    myRectTransform = transform as RectTransform;
                return myRectTransform;
            }
        }

        private UIPanel panelId;

        private List<DOTweenAnimation> openAni;

        public GameObject CloseBg = null;
        public Button closeButton;

        protected virtual void Init(UIPanel panelId)
        {
            this.panelId = panelId;
            transform.gameObject.SetActive(true);
            
            if (openAni == null)
                openAni = GetTweenAllChild(transform);
            if (CloseBg != null)
                UIUtil.AddButtonClickEvent(CloseBg, OnClickClosePanel);
            if (closeButton != null)
                closeButton.onClick.AddListener(OnClickClosePanel);
        }

        private bool isInit = true;

        public virtual void OpenPanel(UIPanel panelId, params object[] param)
        {
            if (isInit)
            {
                Init(panelId);
                SetEvent();
                isInit = false;
            }
            ApplySafeArea();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (openAni != null) PlayOpenAni();
            UpdateAsset();
        }

        protected virtual void SetEvent()
        {

        }
        public List<DOTweenAnimation> GetTweenAllChild(Transform parent)
        {
            List<DOTweenAnimation> datas = new List<DOTweenAnimation>();
            if (parent.GetComponent<DOTweenAnimation>() != null)
                datas.Add(parent.GetComponent<DOTweenAnimation>());
            GetTweenChild(parent, datas);

            return datas;
        }

        private void GetTweenChild(Transform parent, List<DOTweenAnimation> tweenAnis)
        {
            for (int i = 0, iMax = parent.childCount; i < iMax; i++)
            {
                Transform node = parent.GetChild(i);
                try
                {
                    DOTweenAnimation[] tweens = node.gameObject.GetComponents<DOTweenAnimation>();
                    for (int j = 0; j < tweens.Length; j++)
                    {
                        tweenAnis.Add(tweens[j]);
                    }

                    GetTweenChild(node, tweenAnis);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.Log("GetChild Error : " + ex);
#endif
                }
            }
        }
        public virtual void PlayOpenAni()
        {
            for (int i = 0; i < openAni.Count; i++)
            {
                DOTween.Restart(openAni[i].gameObject, "Start");
            }
        }
        public void PlayAni(string key)
        {
            for (int i = 0; i < openAni.Count; i++)
            {
                DOTween.Restart(openAni[i].gameObject, key);
            }
        }
        

        public virtual void ClosePanel()
        {
            UIManager.Inst.ClosePanel(panelId);

            gameObject.SetActive(false);
        }

        public virtual void OnClickClosePanel()
        {
            ClosePanel();
        }

        public virtual void UpdateAsset() { }

        public void ApplySafeArea()
        {
            var area = Screen.safeArea;            
#if UNITY_IOS && !UNITY_EDITOR
            area = new Rect(0, area.y, area.width, Screen.height);
#endif
            var anchorMin = area.position;
            var anchorMax = area.position + area.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        public virtual void Refresh() { }
        public virtual void RefreshEffect(bool isActive) { }
    }
}