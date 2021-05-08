using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Net.Api;
using UnityEngine.UI;
using Libs.Utils;
using Net;
using System;
using UI;
public class MailItem : UIBaseGrp
{
    [SerializeField] Text titleText;
    [SerializeField] Text bodyText;
    [SerializeField] Text sendDateText;
    [SerializeField] Text limitDateText;
    [Header("보상")]
    [SerializeField] GameObject rewardGrp;
    [SerializeField] GameObject rewardObject;
    [SerializeField] Transform rewardContent;
    List<RewardItem> rewardItems;
    [SerializeField] GameObject moreGrp;
    [Header("보상 받기 전")]
    [SerializeField] Button showRewardButton;
    [SerializeField] Button getRewardButton;
    [Header("보상 받은 후")]
    [SerializeField] GameObject getGrp;

    Inbox currentInbox;
    Action<String> rewardGetButtonEvent;


    private const int maxRewardCount = 5;

    public void InitItem(Action<string> rewardGetEvent)
    {
        if (getRewardButton) getRewardButton.onClick.AddListener(OnClickRewardGetButton);
        if (showRewardButton) showRewardButton.onClick.AddListener(OnClickRewardPopupButton);
        rewardGetButtonEvent = rewardGetEvent;
        rewardItems = new List<RewardItem>();
    }
    public void Init(Inbox inbox)
    {
        this.currentInbox = inbox;
        gameObject.SetActive(true);
        switch(currentInbox.Type)
        {
            case INBOX_TYPE.INAPP:
                SetInAppData();
                break;
            case INBOX_TYPE.MARBLE_PASS_REWARD:
            case INBOX_TYPE.MARBLE_PASS_END:
            case INBOX_TYPE.MARBLE_PASS_BEGIN:
                SetPassData();
                break;
            default:
                SetMailData();
                break;
                
        }
    }

    private string getTitleText()
    {
        if (User.Inst.Langs.UI_Text.ContainsKey(currentInbox.Title))
            return UIUtil.GetText(currentInbox.Title);
        else
        {
            switch (currentInbox.Type)
            {
                case INBOX_TYPE.EVENT_REWARD:
                    return Events.Inst.GetTitle(Convert.ToInt32(currentInbox.Title));
            }
            return currentInbox.Title;

        }
    }

    private string getBodyText()
    {
        switch (currentInbox.Type)
        {
            case INBOX_TYPE.EVENT_REWARD:
                return Events.Inst.GetBody(Convert.ToInt32(currentInbox.Body));
        }
        return currentInbox.Title;
    }
    private void SetPassData()
    {
        int passIndex = Convert.ToInt32(currentInbox.Title);
        string title = "";
        string desc = "";
        switch (currentInbox.Type)
        {
            case INBOX_TYPE.MARBLE_PASS_REWARD:
                title = UIUtil.GetText("UI_MarblePass_Mail_Season_EndReward");
                desc = UIUtil.GetText("UI_MarblePass_Mail_Season_EndReward_Desc");
                break;
            case INBOX_TYPE.MARBLE_PASS_END:
                title = UIUtil.GetText("UI_MarblePass_Mail_Season_End");
                desc = UIUtil.GetText("UI_MarblePass_Mail_Season_End_Desc");
                break;
            case INBOX_TYPE.MARBLE_PASS_BEGIN:
                title = UIUtil.GetText("UI_MarblePass_Mail_Season_Start");
                desc = UIUtil.GetText("UI_MarblePass_Mail_Season_Start_Desc");
                if (User.Inst.TBL.Gens.MarblePassSeason.ContainsKey(passIndex))
                {
                    var pass = User.Inst.TBL.Gens.MarblePassSeason[passIndex];
                    var remainTime = UIUtil.ConvertSecToDateString(pass.EndTime - pass.StartTime);
                    desc = string.Format(desc, remainTime);
                }
                break;

        }

        if (titleText)
        {
            titleText.text = title;
        }
        if (bodyText) bodyText.text = desc;
        if (sendDateText)
        {
            string sendDateString = currentInbox.Sender;
            sendDateText.text = sendDateString;
        }
        if (limitDateText)
        {
            var deleteDate = TimeLib.ConvertTo(currentInbox.SendDate).AddDays(7);
            var remainDate = deleteDate - TimeLib.Now;

            limitDateText.text = UIUtil.ConvertSecToDateString((long)remainDate.TotalSeconds);
        }

        if (rewardGrp) rewardGrp.SetActive(false);
        if (getRewardButton) getRewardButton.gameObject.SetActive(false);

        if (currentInbox.Rewards != null && currentInbox.Rewards.Count > 0)
        {
            if (getRewardButton) getRewardButton.gameObject.SetActive(true);
            if (rewardGrp) rewardGrp.SetActive(true);
            for (int i = 0; i < rewardItems.Count; i++)
            {
                rewardItems[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < currentInbox.Rewards.Count; i++)
            {
                if (i >= maxRewardCount)
                    break;
                if (i >= rewardItems.Count)
                {
                    var go = UIManager.Inst.CreateObject(rewardObject, rewardContent);
                    var rewardItem = go.GetComponent<RewardItem>();
                    rewardItems.Add(rewardItem);
                }
                int getRewardCount = UIUtil.GetRewardCount((REWARD_TYPE)currentInbox.Rewards[i].Type, currentInbox.Rewards[i].Count);
                rewardItems[i].SetItem((REWARD_TYPE)currentInbox.Rewards[i].Type, string.Format("x{0}", getRewardCount), currentInbox.Rewards[i].Value);
            }
            if (currentInbox.Rewards.Count > maxRewardCount)
            {
                moreGrp.SetActive(true);
                moreGrp.transform.SetAsLastSibling();
            }
            else
            {
                moreGrp.SetActive(false);
            }

            if (currentInbox.State == INBOX_STATE.WAIT)
            {
                if (getGrp) getGrp.SetActive(false);
            }
            else
            {
                if (getGrp) getGrp.SetActive(true);
            }
        }
    }
    private void SetMailData()
    {
        if (titleText)
        {
            titleText.text = getTitleText();
        }
        if (bodyText) bodyText.text = getBodyText();
        if (sendDateText)
        {
            string sendDateString = currentInbox.Sender;
            sendDateText.text = sendDateString;
        }
        if (limitDateText)
        {
            var deleteDate = TimeLib.ConvertTo(currentInbox.SendDate).AddDays(7);
            var remainDate = deleteDate - TimeLib.Now;

            limitDateText.text = UIUtil.ConvertSecToDateString((long)remainDate.TotalSeconds);
        }

        if (rewardGrp) rewardGrp.SetActive(false);
        if (getRewardButton) getRewardButton.gameObject.SetActive(false);

        if (currentInbox.Rewards != null && currentInbox.Rewards.Count > 0)
        {
            if (getRewardButton) getRewardButton.gameObject.SetActive(true);
            if (rewardGrp) rewardGrp.SetActive(true);
            for(int i = 0; i < rewardItems.Count; i++)
            {
                rewardItems[i].gameObject.SetActive(false);
            }
            for(int i = 0; i < currentInbox.Rewards.Count; i++)
            {
                if (i >= maxRewardCount)
                    break;
                if(i >= rewardItems.Count)
                {
                    var go = UIManager.Inst.CreateObject(rewardObject, rewardContent);
                    var rewardItem = go.GetComponent<RewardItem>();
                    rewardItems.Add(rewardItem);
                }
                int getRewardCount = UIUtil.GetRewardCount((REWARD_TYPE)currentInbox.Rewards[i].Type, currentInbox.Rewards[i].Count);
                rewardItems[i].SetItem((REWARD_TYPE)currentInbox.Rewards[i].Type, string.Format("x{0}", getRewardCount), currentInbox.Rewards[i].Value);
            }
            if(currentInbox.Rewards.Count > maxRewardCount)
            {
                moreGrp.SetActive(true);
                moreGrp.transform.SetAsLastSibling();
            }
            else
            {
                moreGrp.SetActive(false);
            }

            if (currentInbox.State == INBOX_STATE.WAIT)
            {
                if (getGrp) getGrp.SetActive(false);
            }
            else
            {
                if (getGrp) getGrp.SetActive(true);
            }
        }
        
    }

    private void SetInAppData()
    {
        int inAppId = int.Parse(currentInbox.Title);
        if (User.Inst.TBL.InAppShop.ContainsKey(inAppId) == false)
        {
            return;
        }
        if (User.Inst.TBL.InAppShop.ContainsKey(inAppId) == false)
            return;

        var appTableInfo = User.Inst.TBL.InAppShop[inAppId];
        var appLagsTableInfo = User.Inst.Langs.InAppShop[inAppId];
        if (titleText) titleText.text = appLagsTableInfo.ShopName;

        string rewardString = string.Empty;
        for (int i = 0; i < currentInbox.Rewards.Count; i++)
        {
            REWARD_TYPE rewardType = (REWARD_TYPE)currentInbox.Rewards[i].Type;
            string rewardName = UIUtil.GetRewardName(rewardType, currentInbox.Rewards[i].Value);
            if (i == currentInbox.Rewards.Count - 1)
                rewardName = string.Format("{0} x{1}", rewardName, currentInbox.Rewards[i].Count);
            else
                rewardName = string.Format("{0} x{1}\n", rewardName, currentInbox.Rewards[i].Count);
            rewardString += rewardName;
        }

        if (bodyText) bodyText.text = rewardString;
        if (sendDateText)
        {
            string sendDateString = currentInbox.Sender;
            sendDateText.text = sendDateString;
        }
        if (limitDateText)
        {
            limitDateText.text = "-";
        }

        if (rewardGrp) rewardGrp.SetActive(false);
        if (currentInbox.Rewards.Count > 0)
        {
            if (rewardGrp) rewardGrp.SetActive(true);

            for (int i = 0; i < rewardItems.Count; i++)
            {
                rewardItems[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < currentInbox.Rewards.Count; i++)
            {
                if (i >= rewardItems.Count)
                {
                    var go = UIManager.Inst.CreateObject(rewardObject, rewardContent);
                    var rewardItem = go.GetComponent<RewardItem>();
                    rewardItems.Add(rewardItem);
                }
                int getRewardCount = UIUtil.GetRewardCount((REWARD_TYPE)currentInbox.Rewards[i].Type, currentInbox.Rewards[i].Count);
                rewardItems[i].SetItem((REWARD_TYPE)currentInbox.Rewards[i].Type, string.Format("x{0}", getRewardCount), currentInbox.Rewards[i].Value);
            }
            if (currentInbox.Rewards.Count > maxRewardCount)
            {
                moreGrp.SetActive(true);
                moreGrp.transform.SetAsLastSibling();
            }
            else
            {
                moreGrp.SetActive(false);
            }
            if (currentInbox.State == INBOX_STATE.WAIT)
            {
                if (getGrp) getGrp.SetActive(false);
            }
            else
            {
                if (getGrp) getGrp.SetActive(true);
            }
        }

    }
    private void OnClickRewardGetButton()
    {
        if (currentInbox.State == INBOX_STATE.READ)
            return;
        rewardGetButtonEvent?.Invoke(currentInbox.GID);
    }
    private void OnClickRewardPopupButton()
    {
        if (currentInbox.State != INBOX_STATE.WAIT)
            return;
        if (currentInbox.Rewards.Count > 0)
        {
            string title = titleText.text;
            var reward = currentInbox.Rewards[0];
            REWARD_TYPE rewardType = (REWARD_TYPE)reward.Type;
            if(rewardType == REWARD_TYPE.REWARD_BOX)
            {
                UIManager.Inst.ShowBoxRewardPopup(reward.Value, title,buttonText:UIUtil.GetText("UI_Common_All_Get"), buttonAction: OnClickRewardGetButton);
            }
            else
            {
                UIManager.Inst.ShowRewardPopup(currentInbox.Rewards, title, buttonText: UIUtil.GetText("UI_Common_All_Get"), buttonAction: OnClickRewardGetButton);
            }
        }
    }
}
