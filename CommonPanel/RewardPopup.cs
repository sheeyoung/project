using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
using System;
public class RewardPopup : UIBasePanel
{
    [SerializeField] Text titleText;
    [SerializeField] GameObject rewardItemObject;
    [SerializeField] Transform rewardContent;
    [SerializeField] Text tipText;
    [SerializeField] Text buttonText;
    [SerializeField] Button confirmButton;

    RewardGains rewardGains;
    Rewards rewards;
    Action buttonAction;
    List<RewardItem> rewardItems;
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        rewardItems = new List<RewardItem>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        confirmButton.onClick.AddListener(OnClickButton);
    }

    // 0 : title
    // 1 : rewards
    // 2 : tip
    // 3 : buttonText
    // 4 : buttonAction
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);

        string title = string.Empty;
        string tip = string.Empty;
        string button = string.Empty;
        rewardGains = null;
        rewards = null;
        if (param.Length >= 1)
            title = (string)param[0];
        if (param.Length >= 2)
        {
            if (param[1].GetType() == typeof(RewardGains))
                rewardGains = (RewardGains)param[1];
            else
                rewards = (Rewards)param[1];
        }
        if (param.Length >= 3)
            tip = (string)param[2];
        if (param.Length >= 4)
            button = (string)param[3];
        if (param.Length >= 5)
            buttonAction = (Action)param[4];

        if (string.IsNullOrEmpty(title))
            title = UIUtil.GetText("UI_Common_Ok");
        if(string.IsNullOrEmpty(button))
            button = UIUtil.GetText("UI_Common_Ok");
        titleText.text = title;
        buttonText.text = button;
        tipText.text = tip;

        SetReward();
    }

    private void SetReward()
    {
        if(rewardGains != null)
        {
            for (int i = 0; i < rewardGains.Count; i++)
            {
                if (rewardItems.Count <= i)
                {
                    var go = GameObject.Instantiate(rewardItemObject, rewardContent);
                    var item = go.GetComponent<RewardItem>();
                    rewardItems.Add(item);
                }
                REWARD_TYPE rewardType = (REWARD_TYPE)rewardGains[i].Type;
                int getRewardCount = UIUtil.GetRewardCount(rewardType, rewardGains[i].Count);
                rewardItems[i].SetItem(rewardType, getRewardCount.ToString(), rewardGains[i].Value);
            }
        }
        else
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewardItems.Count <= i)
                {
                    var go = GameObject.Instantiate(rewardItemObject, rewardContent);
                    var item = go.GetComponent<RewardItem>();
                    rewardItems.Add(item);
                }
                REWARD_TYPE rewardType = (REWARD_TYPE)rewards[i].Type;
                int getRewardCount = UIUtil.GetRewardCount(rewardType, rewards[i].Count);
                rewardItems[i].SetItem(rewardType, getRewardCount.ToString(), rewards[i].Value);
            }
        }
        
    }

    private void OnClickButton()
    {
        buttonAction?.Invoke();
        ClosePanel();
    }
}
