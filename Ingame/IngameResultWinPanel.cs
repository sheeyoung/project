using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using scene;
using DG.Tweening;
using Net.Api;
using Net.Impl;

[System.Serializable]
public class RewardConditionGrp
{
    public GameObject group;
    public Text onRewardConditionText;
    public Text offRewardConditionText;
    public GameObject on;
    public GameObject off;
    public List<DOTweenAnimation> tweenAnis;
}
public class IngameResultWinPanel : UIBasePanel
{
    [SerializeField] private IngameResultUserInfoGrp userInfoGrp;
    [SerializeField] private List<RewardConditionGrp> rewardConditionGrps;
    [SerializeField] private GameObject assetItemObject;
    [SerializeField] private GameObject assetItemParent;
    [SerializeField] private GameObject startAni_Lastobject;   

    Dictionary<Net.REWARD_TYPE, IngameResultAssetItem> assetItemDic;

    class TotalReward
    {
        public Net.REWARD_TYPE rewardType;
        public int totalAddRewardCount;
    }
    Dictionary<Net.REWARD_TYPE, int> totalReward = new Dictionary<Net.REWARD_TYPE, int>(new RewardTypeComparer());
    Dictionary<REWARD_VALUE, RewardGains> rewardConditions = new Dictionary<REWARD_VALUE, RewardGains>(new RewardValueComparer());
    private int rewardPvPScore = 0;
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        SoundManager.Inst.PlayEffect(19);
        for (int i = 0; i < rewardConditionGrps.Count; i++)
        {
            rewardConditionGrps[i].tweenAnis = UIUtil.GetTweenAllChild(rewardConditionGrps[i].on.transform);
        }

        startAni_Lastobject.GetComponent<DOTweenAnimation>().onComplete.AddListener(StartRewardAni);
        assetItemDic = new Dictionary<Net.REWARD_TYPE, IngameResultAssetItem>(new RewardTypeComparer());
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);

        assetItemDic.Clear();
        totalReward.Clear();

        rewardConditions = (Dictionary<REWARD_VALUE, RewardGains>)param[0];
        rewardPvPScore = (int)param[1];

        foreach (var rewardCon in rewardConditions)
        {
            var reward = rewardCon.Value;
            for (int rewardgainCount = 0; rewardgainCount < reward.Count; rewardgainCount++)
            {
                if (reward[rewardgainCount].Count == 0)
                    continue;
                Net.REWARD_TYPE rewardType = (Net.REWARD_TYPE)reward[rewardgainCount].Type;
                if (totalReward.ContainsKey(rewardType) == false)
                    totalReward.Add(rewardType, 0);
                totalReward[rewardType] += reward[rewardgainCount].Count;
            }
        }

        SetData();
        PlayOpenAni();
    }

    private void SetData()
    {
        if(userInfoGrp) userInfoGrp.Init(GamePvP.Instance.BattleMode.Home, true, rewardPvPScore);

        for(int i = 0; i < rewardConditionGrps.Count; i++)
        {
            rewardConditionGrps[i].group.SetActive(false);
        }

        int count = 0;
        foreach(var item in rewardConditions)
        {
            rewardConditionGrps[count].group.SetActive(true);
            rewardConditionGrps[count].onRewardConditionText.text = GetRewardName(item.Key);
            rewardConditionGrps[count].offRewardConditionText.text = GetRewardName(item.Key);
            rewardConditionGrps[count].on.SetActive(false);
            rewardConditionGrps[count].off.SetActive(true);
            count++;
        }

        foreach(var assetItem in assetItemDic.Values)
        {
            assetItem.gameObject.SetActive(false);
        }
        foreach (var item in totalReward)
        {
            Net.REWARD_TYPE rewardType = item.Key;
            UIUtil.AddShowInGameReward(rewardType);
            if (assetItemDic.ContainsKey(rewardType) == false)
            {
                var go = UIManager.Inst.CreateObject(assetItemObject, assetItemParent.transform);
                var assetItem = go.GetComponent<IngameResultAssetItem>();
                assetItemDic.Add(rewardType, assetItem);
            }
            assetItemDic[rewardType].Init(rewardType, 0);

        }
    }

    private string GetRewardName(REWARD_VALUE value)
    {
        string rewardName = "";
        switch (value)
        {
            case REWARD_VALUE.BASE:
                rewardName = UIUtil.GetText("UI_Ingame_Result_Win_Panel_004");
                break;
            case REWARD_VALUE.ROUND_BONUS:
                rewardName = string.Format(UIUtil.GetText("UI_Ingame_Result_Win_Panel_001"), GamePvP.Instance.BattleMode.Round - 1);
                break;
            case REWARD_VALUE.AD_BONUS:
                rewardName = UIUtil.GetText("UI_Ingame_Result_Win_Panel_003");
                break;
            case REWARD_VALUE.STRAIGHT_WIN_BONUS:
                rewardName = string.Format(UIUtil.GetText("UI_Ingame_Result_Win_Panel_002"), User.Inst.Doc.PvP.Wins);
                break;
            case REWARD_VALUE.SUBSCRIPTION_BONUS:
                rewardName = UIUtil.GetText("UI_Ingame_Result_Win_Panel_005");
                break;
        }

        return rewardName;
    }

    private void StartRewardAni()
    {
        StartCoroutine(ShowReward());
        SoundManager.Inst.PlayEffect(20);
    }

    IEnumerator ShowReward()
    {
        int order = 0;
        foreach(var value in rewardConditions)
        {
            rewardConditionGrps[order].on.SetActive(true);
            rewardConditionGrps[order].off.SetActive(false);
            for(int i = 0; i < rewardConditionGrps[order].tweenAnis.Count; i++)
                DOTween.Restart(rewardConditionGrps[order].tweenAnis[i].gameObject, "OnEff");
            for(int i = 0; i < value.Value.Count; i++)
            {
                if(assetItemDic.ContainsKey((Net.REWARD_TYPE)value.Value[i].Type))
                {
                    assetItemDic[(Net.REWARD_TYPE)value.Value[i].Type].AddAsset(value.Value[i].Count);
                }
            }
            order++;
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void OnClickGoToLobby()
    {
        ClosePanel();        
    }

    public override void ClosePanel()
    {
        GamePvP.Instance.BattleMode.LeaveRoom();
    }
}
