using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.UI;
using DG.Tweening;
using Net;
public class BoxInfoItem : MonoBehaviour
{
    [SerializeField] GameObject rewardGrp;
    [SerializeField] Image rewardIconImage;
    [SerializeField] GameObject marbleGrp;
    [SerializeField] Image marbleIconImage;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardCountText;
    [SerializeField] GameObject rateToastButtonObject;
    [SerializeField] UIEventButton rateToastEventButton;
    [Header("Rate Toast")]
    [SerializeField] GameObject rateToastObject;
    [SerializeField] GameObject rateToastItemObject;
    [SerializeField] Transform rateToastItemParent;
    List<BoxInfoMarbleItem> rateToastItems;
    [Header("Rate Alarm")]
    [SerializeField] GameObject rateAlramGrp;
    [SerializeField] Text rateAlarmText;

    private REWARD_TYPE rewardType;
    private int rewardValue;
    private int compRate;
    private int rewardCount_Min;
    private int rewardCount_Max;
    public int CompRate { get { return compRate; } }
    public bool IsEqual(REWARD_TYPE rewardType, int rewardValue)
    {
        return rewardType == this.rewardType && rewardValue == this.rewardValue;
    }
    public void Init()
    {
        rateToastEventButton.AddClickDownEvnet(()=> { OnClickRateButton(true); });
        rateToastEventButton.AddClickUpEvnet(() => { OnClickRateButton(false); });
        rateToastItems = new List<BoxInfoMarbleItem>();
    }
    public void ResetItem()
    {
        rewardType = default(REWARD_TYPE);
        rewardValue = 0;
        rewardCount_Min = 0;
        rewardCount_Max = 0;
        gameObject.SetActive(false);
    }
    public void SetData(REWARD_TYPE rewardType, int rewardCount, int rewardValue)
    {
        int getRewardCount = UIUtil.GetRewardCount(rewardType, rewardCount);
        this.rewardType = rewardType;
        this.rewardValue = rewardValue;
        this.rewardCount_Min = getRewardCount;
        this.rewardCount_Max = getRewardCount;

        gameObject.SetActive(true);
        rateToastObject.SetActive(false);
        rateToastButtonObject.SetActive(false);
        rateAlramGrp.SetActive(false);

        string countText = string.Format("x{0}", getRewardCount);
        rewardCountText.text = countText;

        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                if (marbleGrp) marbleGrp.SetActive(true);
                if (rewardGrp) rewardGrp.SetActive(false);
                if(marbleIconImage && User.Inst.TBL.Marble.ContainsKey(rewardValue)) 
                    UIUtil.SetRewardIcon(rewardType, marbleIconImage, rewardValue);
                else 
                    UIUtil.SetRewardIcon(rewardType, rewardIconImage, rewardValue);
                break;
            default:
                if (marbleGrp) marbleGrp.SetActive(false);
                if (rewardGrp) rewardGrp.SetActive(true);
                UIUtil.SetRewardIcon(rewardType, rewardIconImage, rewardValue);
                break;
        }

        rewardNameText.text = UIUtil.GetRewardName(rewardType, rewardValue);
    }

    //compType 1
    public void SetDataCompType1(int rewardIndex, int rewardCount)
    {
        if (User.Inst.TBL.Reward.ContainsKey(rewardIndex) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        var reward = User.Inst.TBL.Reward[rewardIndex];

        Net.REWARD_TYPE rewardType = UIUtil.GetRewardType(reward.RewardType);

        rateToastObject.SetActive(false);
        rateToastButtonObject.SetActive(false);

        int getRewardCount = UIUtil.GetRewardCount(rewardType, rewardCount);

        string countText = string.Empty;
        if (reward.CompoRate == 10000)
        {
            if (reward.CountMin == reward.CountMax)
                countText = string.Format("x{0}", reward.CountMax * getRewardCount);
            else
                countText = string.Format("x{0} ~ {1}", reward.CountMin * getRewardCount, reward.CountMax * getRewardCount);
            rewardCount_Min = reward.CountMin * getRewardCount;
            rewardCount_Max = reward.CountMax * getRewardCount;
            rateAlramGrp.SetActive(false);
        }
        else
        {
            countText = string.Format("x{0} ~ {1}", 0, reward.CountMax * getRewardCount);
            rewardCount_Min = 0;
            rewardCount_Max = reward.CountMax * getRewardCount;
            rateAlramGrp.SetActive(true);
            int rate = (int)(((float)reward.CompoRate / 10000f) * 100f);
            rateAlarmText.text = string.Format(UIUtil.GetText("UI_Common_BoxRate"), rate);
        }
        
        rewardCountText.text = countText;

        this.rewardType = rewardType;
        this.rewardValue = 0;
        this.compRate = reward.CompoRate;

        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                if (marbleIconImage && User.Inst.TBL.Marble.ContainsKey(reward.RewardValue))
                {
                    if (marbleGrp) marbleGrp.SetActive(true);
                    if (rewardGrp) rewardGrp.SetActive(false);
                    UIUtil.SetRewardIcon(rewardType, marbleIconImage, reward.RewardValue);
                }
                else
                {
                    if (marbleGrp) marbleGrp.SetActive(false);
                    if (rewardGrp) rewardGrp.SetActive(true);
                    UIUtil.SetRewardIcon(rewardType, rewardIconImage, reward.RewardValue);
                }
                break;
            default:
                if (marbleGrp) marbleGrp.SetActive(false);
                if (rewardGrp) rewardGrp.SetActive(true);
                UIUtil.SetRewardIcon(rewardType, rewardIconImage, reward.RewardValue);
                break;
        }

        rewardNameText.text = UIUtil.GetRewardName(rewardType, reward.RewardValue);
    }
    public void SetDataCompType1_duplication(int rewardIndex, int rewardCount)
    {
        if (User.Inst.TBL.Reward.ContainsKey(rewardIndex) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        var reward = User.Inst.TBL.Reward[rewardIndex];

        Net.REWARD_TYPE rewardType = UIUtil.GetRewardType(reward.RewardType);
        int getRewardCount = UIUtil.GetRewardCount(rewardType, rewardCount);

        string countText = string.Empty;
        if (reward.CompoRate == 10000)
        {
            rewardCount_Min += (reward.CountMin * getRewardCount);
            rewardCount_Max += (reward.CountMax * getRewardCount);

            if (rewardCount_Min == rewardCount_Max)
                countText = string.Format("x{0}", rewardCount_Max);
            else
                countText = string.Format("x{0} ~ {1}", rewardCount_Min, rewardCount_Max);
        }
        else
        {
            rewardCount_Min += 0;
            rewardCount_Max += reward.CountMax * getRewardCount;

            countText = string.Format("x{0} ~ {1}", 0, reward.CountMax * getRewardCount);
        }

        rewardCountText.text = countText;
    }

    //compeType 2
    public void SetDataCompType2(int rewardID, int rewardGroup, int rewardCount)
    {
        if(User.Inst.TBL.Gens.Reward.ContainsKey(rewardID) == false || User.Inst.TBL.Gens.Reward[rewardID].ContainsKey(rewardGroup) == false)
        {
            gameObject.SetActive(false);
            return;
        }

        var rewards = User.Inst.TBL.Gens.Reward[rewardID][rewardGroup];
        if (rewards.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        rateToastObject.SetActive(false);
        SetRateToast(rewardID, rewardGroup);
        rateToastButtonObject.SetActive(true);
        rateAlramGrp.SetActive(false);

        Net.REWARD_TYPE rewardType = UIUtil.GetRewardType(rewards[0].RewardType);
        int getRewardCount = UIUtil.GetRewardCount(rewardType, rewardCount * rewards[0].CountMax);

        rewardCountText.text = string.Format("x{0}", getRewardCount);

        if (marbleGrp) marbleGrp.SetActive(false);
        if (rewardGrp) rewardGrp.SetActive(true);
        UIUtil.SetRewardIcon(rewardType, rewardIconImage);
        rewardNameText.text = UIUtil.GetRewardName(rewardType);
    }

    private void SetRateToast(int rewardID, int rewardGroup)
    {
        var rewards = User.Inst.TBL.Gens.Reward[rewardID][rewardGroup];
        for (int i = 0; i < rateToastItems.Count; i++)
            rateToastItems[i].gameObject.SetActive(false);
        for(int i=0; i < rewards.Count; i++)
        {
            if(rateToastItems.Count <= i)
            {
                var go = GameObject.Instantiate(rateToastItemObject, rateToastItemParent);
                var item = go.GetComponent<BoxInfoMarbleItem>();
                rateToastItems.Add(item);
            }
            string iconName = string.Empty;
            string rate = string.Format("{0:0}%", ((float)rewards[i].CompoRate / 10000f)*100);
            if (rewards[i].RewardValue == 0)
            {
                Net.REWARD_TYPE rewardType = UIUtil.GetRewardType(rewards[i].RewardType);
                if (User.Inst.TBL.Assets.ContainsKey((int)rewardType))
                {
                    iconName = User.Inst.TBL.Assets[(int)rewardType].RewardIcon;
                }
                if (User.Inst.TBL.RewardType.ContainsKey((int)rewardType))
                {
                    iconName = User.Inst.TBL.RewardType[(int)rewardType].Icon;
                }
            }
            else
            {
                Net.REWARD_TYPE rewardType = UIUtil.GetRewardType(rewards[i].RewardType);
                switch (rewardType)
                {
                    case Net.REWARD_TYPE.REWARD_MARBLE:
                    case Net.REWARD_TYPE.REWARD_MARBLE_LEGEND:
                    case Net.REWARD_TYPE.REWARD_MARBLE_MYTH:
                    case Net.REWARD_TYPE.REWARD_MARBLE_NORMAL:
                    case Net.REWARD_TYPE.REWARD_MARBLE_RARE:
                    case Net.REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                        if (User.Inst.TBL.Marble.ContainsKey(rewards[i].RewardValue))
                            iconName = User.Inst.TBL.Marble[rewards[i].RewardValue].Icon;
                        break;
                }
            }
            rateToastItems[i].SetData(iconName, rate);
        }
    }

    private void OnClickRateButton(bool isDown)
    {
#if UNITY_EDITOR
        Debug.Log("rateButton : " + isDown.ToString());
#endif
        if(isDown)
        {
            DOTween.Restart(rateToastItemParent);
        }
        rateToastObject.SetActive(isDown);
    }


}
