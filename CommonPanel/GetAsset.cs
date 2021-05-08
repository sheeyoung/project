using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
public class GetAsset : MonoBehaviour
{
    [SerializeField] Text assetCount;
    [SerializeField] GameObject tweenAniObject;
    Action<GameObject> onCompleteAni;
    public void Init(Action<GameObject> onCompleteAni)
    {
        this.onCompleteAni = onCompleteAni;
        var anis = tweenAniObject.GetComponents<DOTweenAnimation>();
        DOTweenAnimation lastEndAni = null;
        float longAniTime = 0;
        for(int i = 0; i < anis.Length; i++)
        {
            float aniTime = anis[i].duration + anis[i].delay;
            if(aniTime > longAniTime)
            {
                longAniTime = aniTime;
                lastEndAni = anis[i];
            }
        }
        lastEndAni.onComplete.AddListener(OnAniComplete);
    }

    public void SetCount(string count)
    {
        gameObject.SetActive(true);
        assetCount.text = count;
        DOTween.Restart(tweenAniObject, "GetReward");
    }

    private void OnAniComplete()
    {
        onCompleteAni?.Invoke(gameObject);
    }
}
