using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.Events;

namespace UI
{
    public class UIBaseGrp : BaseMono
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

        protected List<DOTweenAnimation> openAni;

        public virtual void Init()
        {
            if (isInit)
            {
                InitData();
                SetEvent();
                isInit = false;
            }

            transform.gameObject.SetActive(true);

            //if (openAni == null)
            //    openAni = GetTweenAllChild(transform);
            //PlayOpenAni();
        }

        private bool isInit = true;
        protected virtual void InitData()
        {

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
        //public virtual void PlayOpenAni()
        //{
        //    for (int i = 0; i < openAni.Count; i++)
        //    {
        //        DOTween.Restart(openAni[i].gameObject, "Start");
        //    }
        //}

        public void PlayAni(string key)
        {
            if (openAni == null)
                openAni = GetTweenAllChild(transform);
            for (int i = 0; i < openAni.Count; i++)
            {
                DOTween.Restart(openAni[i].gameObject, key);
            }
        }

        public virtual void UpdateAsset() { }

        public virtual void Refresh() { }
        public virtual void ScrollCompSwitch(bool isOn) { }
        public virtual void RefreshEffect(bool isActive) { }
    }
}