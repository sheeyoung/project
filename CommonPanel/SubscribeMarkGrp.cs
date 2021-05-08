using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net.Impl;
using App.UI;
using DG.Tweening;
using Libs.Utils;
using Platform;
public class SubscribeMarkGrp : MonoBehaviour
{
    [Header("구독중 일때")]
    [SerializeField] GameObject markGrp;
    [SerializeField] GameObject bubbleGrp;
    [SerializeField] UIEventButton bubbleButton;
    [SerializeField] Text bubbleText;
    [Header("구독중 아닐때")]
    [SerializeField] GameObject subscribeNotOpenGrp;
    [SerializeField] Text subscribeNotOpenText;
    public float bubbleWaitTime = 5f;
    public SubscribeCompenSation subscribeCompenSation;
    bool isInit = true;
    bool isShow;
    bool isPlaySubscribe;

    public enum SubscribeCompenSation
    {
        CoOpBox,
        PVP,
        CoOP,
        ShopMarble,
    }
    private void OnEnable()  
    {
        if(isInit)
        {
            isInit = false;
            SetEvent();
        }
        int index = (int)UIUtil.GetConstValue("CONST_SUBSCRIBE_SHOPID");
        if (User.Inst.TBL.InAppShop.ContainsKey(index) == false)
        {
            if(markGrp) markGrp.SetActive(false);
            if(subscribeNotOpenGrp) subscribeNotOpenGrp.SetActive(false);
            if (bubbleGrp) bubbleGrp.SetActive(false);
            isPlaySubscribe = false;
            return;
        }
        isPlaySubscribe = true;
        if (bubbleGrp) bubbleGrp.SetActive(false);
        CheckSubscribe();
        isShow = false;
    }
    private void CheckSubscribe()
    {
        bool isGain = UIUtil.CheckJoinSubscribe();
        //if (subscribeNotOpenGrp && isGain)
        //{
        //    isPlaySubscribe = true;
        //}
        if (subscribeNotOpenGrp) subscribeNotOpenGrp.SetActive(isGain == false);

        if (markGrp) markGrp.SetActive(isGain);
        
        if (isGain)
            SetSubscribeDesc();
        else
            SetNotSubscribeDesc();
    }
    private void SetNotSubscribeDesc()
    {
        string desc = "";
        switch(subscribeCompenSation)
        {
            case SubscribeCompenSation.CoOpBox:
                desc = UIUtil.GetText("UI_Subscribe_010");
                break;
            case SubscribeCompenSation.ShopMarble:
                desc = UIUtil.GetText("UI_Subscribe_008");
                int rate = (int)UIUtil.GetConstValue("CONST_SUBSCRIBE_DAILYSHOP_SALE") / 100;
                desc = string.Format(desc, rate);
                break;
        }
        if (subscribeNotOpenText) subscribeNotOpenText.text = desc;
    }
    private void SetSubscribeDesc()
    {
        string desc = "";
        switch (subscribeCompenSation)
        {
            case SubscribeCompenSation.CoOpBox:
                desc = UIUtil.GetText("UI_Subscribe_011");
                break;
            case SubscribeCompenSation.PVP:
                string rewardString = string.Format("{0}, {1}", UIUtil.GetText("UI_Subscribe_005"), UIUtil.GetText("UI_Subscribe_007"));
                desc = string.Format(UIUtil.GetText("UI_Subscribe_009"), rewardString);
                break;
            case SubscribeCompenSation.CoOP:
                desc = string.Format(UIUtil.GetText("UI_Subscribe_009"), UIUtil.GetText("UI_Subscribe_007"));
                break;
            case SubscribeCompenSation.ShopMarble:
                desc = string.Format( UIUtil.GetText("UI_Subscribe_009"), UIUtil.GetText("UI_Subscribe_006"));
                break;
        }
        if (bubbleText) bubbleText.text = desc;
    }
    private void SetEvent()
    {
        if (bubbleButton) bubbleButton.AddClickUpEvnet(() => { OnBubbleEvent(true); });
        if (bubbleButton) bubbleButton.AddClickDownEvnet(() => { OnBubbleEvent(false); });
        if (bubbleButton) bubbleButton.AddClickEvnet(OnClickBubbleButton);
    }
    public void OnClickBuySubscribeButton()
    {
        UIManager.Inst.CloseAllWindow();
        UIManager.Inst.OpenPanel(UI.UIPanel.SubscribePopup);
    }
    private void OnBubbleEvent(bool up)
    {
        if (bubbleGrp) bubbleGrp.SetActive(up == false);
        if (up == false)
        {
            isShow = true;
            if (bubbleGrp) DOTween.Restart(bubbleGrp, "ButtonSelect");
        }
    }

    float deltaTime;
    private void Update()
    {
        if (isPlaySubscribe)
        {
            if (deltaTime > 1f)
            {

                deltaTime = 0f;
                CheckSubscribe();
            }
            else
                deltaTime += Time.deltaTime;
        }

    }
    private void OnClickBubbleButton()
    {
        //if (bubbleGrp)
        //{
        //    bubbleGrp.SetActive(true);
        //    DOTween.Restart(bubbleGrp, "ButtonSelect");
        //    StartCoroutine(WaitBubble());
        //}
    }
    IEnumerator WaitBubble()
    {
        yield return new WaitForSeconds(bubbleWaitTime);
        if (bubbleGrp) bubbleGrp.SetActive(false);
    }
}
