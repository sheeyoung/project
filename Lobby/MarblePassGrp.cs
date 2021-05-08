using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Libs.Utils;
using Net;

public class MarblePassGrp : MonoBehaviour
{
    enum PassState
    {
        Pass,
        Wait
    }
    private PassState currentPassState;
    [Header("Top")]
    [SerializeField] GameObject passCompleteGrp;
    [SerializeField] GameObject passNotCompleteGrp;
    [Header("Complete")]
    [SerializeField] Text completeSeasonPeriodTitleText;
    [SerializeField] Text completeSeasonPeriodText;
    [Header("NotComplete")]
    [SerializeField] Text seasonNameText;
    [SerializeField] Text seasonPeriodText;
    [SerializeField] Text passPointLevelText;
    [SerializeField] Image passProgressImage;

    [SerializeField] RewardItem commonRewardItem;
    [SerializeField] RewardItem premiumItem;
    [SerializeField] GameObject lockGrp;
    private GameObject idleEff;
    private GameObject idleEff_premium;

    int currentLevel;
    TBL.Sheet.CMarblePass tableInfo;
    int currentMPIndex;
    bool isRewardComplete;
    bool isShow;
    public void SetMarblePass()
    {
        isShow = false;
        currentMPIndex = UIUtil.GetMarblePassSeasonIndex();
        if(User.Inst.TBL.MarblePassSeason.ContainsKey(currentMPIndex) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        isShow = true;
        gameObject.SetActive(true);
        currentLevel = -1;
        
        var seasonInfo = User.Inst.TBL.MarblePassSeason[currentMPIndex];
        if (User.Inst.TBL.Gens.MarblePass.ContainsKey(seasonInfo.PassGroup) == false)
            return;

        var tableInfos = User.Inst.TBL.Gens.MarblePass[seasonInfo.PassGroup];

        var passSeasonTableInfo = User.Inst.TBL.Gens.MarblePassSeason[currentMPIndex];
        var openTime = passSeasonTableInfo.StartTime;
        var endTime = passSeasonTableInfo.EndTime;
        var now = TimeLib.Seconds;
        currentPassState = PassState.Wait;
        if (openTime < now && now < endTime)
        {
            currentPassState = PassState.Pass;
        }


        int premiumMaxIndex = 1;
        int freeMaxIndex = 1;
        int totalRewardCount = 0;
        foreach (var item in tableInfos)
        {
            if (item.Value.Level != 0)
                totalRewardCount++;
            if (item.Value.Level == 0)
                continue;
            if (User.Inst.Doc.MarblePass.Level < item.Value.Level)
                continue;
            if (User.Inst.Doc.MarblePass.Rewards != null && User.Inst.Doc.MarblePass.Rewards.Premium.Contains(item.Value.Level) == false && premiumMaxIndex < item.Value.Level)
                premiumMaxIndex = item.Value.Level;
            if (User.Inst.Doc.MarblePass.Rewards != null && User.Inst.Doc.MarblePass.Rewards.Free.Contains(item.Value.Level) == false && freeMaxIndex < item.Value.Level && item.Value.Level != 0)
                freeMaxIndex = item.Value.Level;
        }


        if (currentPassState == PassState.Pass && tableInfos.ContainsKey(User.Inst.Doc.MarblePass.Level +1))
        {
            //패스 기간이고 최대 레벨이 아닐때
            currentLevel = User.Inst.Doc.MarblePass.Level + 1;
        }
        else
        {
            if (User.Inst.Doc.MarblePass.IsPaid > 0)
            {
                if (premiumMaxIndex > 0)
                {
                    //프리미엄 구매완료 미수령프리미엄 보상 최고 레벨표기
                    currentLevel = premiumMaxIndex;
                }
                else
                {
                    //프리미엄 구매완료 프리미엄 전체 받음 프리 보상 중 최고 레벨표기
                    currentLevel = freeMaxIndex;
                }

                //프리미엄 구매완료 모든 보상 수령완료 : currentLevel이 0이면 모든 보상 수령완료
            }
            else
            {
                if (freeMaxIndex > 0)
                {
                    //프리미엄 구매 안함 미수령 프리 보상 최고 레벨표기
                    currentLevel = freeMaxIndex;
                }
                else
                {
                    //프리미엄 구매 안함 프리 보상 전체 수령 프리미엄 최고레벨보상 표기
                    currentLevel = premiumMaxIndex;
                }
            }
        }
        if (tableInfos.ContainsKey(currentLevel) == false)
        {
            passCompleteGrp.SetActive(true);
            passNotCompleteGrp.SetActive(false);
            SetCompletePeriod();
            isRewardComplete = true;
        }
        else
        {
            isRewardComplete = false;
            passCompleteGrp.SetActive(false);
            passNotCompleteGrp.SetActive(true);

            var marblePassItem = tableInfos[currentLevel];
            tableInfo = tableInfos[currentLevel];

            int freeRewardCount = 0;
            if(User.Inst.Doc.MarblePass.Rewards != null)
                freeRewardCount = User.Inst.Doc.MarblePass.Rewards.Free.Count;
            int premiumRewardCount = 0;
            if(User.Inst.Doc.MarblePass.Rewards != null) premiumRewardCount = User.Inst.Doc.MarblePass.Rewards.Premium.Count;

            commonRewardItem.completeGrp.SetActive(totalRewardCount == freeRewardCount);
            commonRewardItem.rewardGrp.SetActive(totalRewardCount != freeRewardCount);

            premiumItem.completeGrp.SetActive(totalRewardCount == premiumRewardCount);
            premiumItem.rewardGrp.SetActive(totalRewardCount != premiumRewardCount);

            if (totalRewardCount != freeRewardCount)
            {
                if (string.IsNullOrEmpty(marblePassItem.RewardType) == false 
                    && User.Inst.Doc.MarblePass.Rewards.Free.Contains(marblePassItem.Level) == false)
                {
                    var rewardType = UIUtil.GetRewardType(marblePassItem.RewardType);
                    SetReward(commonRewardItem, rewardType, marblePassItem.RewardCount, marblePassItem.RewardValue, ref idleEff, false);
                }
                else
                {
                    commonRewardItem.completeGrp.SetActive(true);
                    commonRewardItem.rewardGrp.SetActive(false);
                }
            }
            if (totalRewardCount != premiumRewardCount)
            {
                if (User.Inst.Doc.MarblePass.Rewards.Premium.Contains(marblePassItem.Level) == false)
                {
                    var rewardType = UIUtil.GetRewardType(marblePassItem.PremiumRewardType);
                    SetReward(premiumItem, rewardType, marblePassItem.PremiumRewardCount, marblePassItem.PremiumRewardValue, ref idleEff_premium, true);
                }
                else
                {
                    premiumItem.completeGrp.SetActive(true);
                    premiumItem.rewardGrp.SetActive(false);
                }
            }
            SetPassInfo();
            if (lockGrp) lockGrp.SetActive(User.Inst.Doc.MarblePass.IsPaid == 0);
        }

        
    }
    private void SetCompletePeriod()
    {
        if (completeSeasonPeriodText)
        {
            var seasonTableInfo = User.Inst.TBL.MarblePassSeason[currentMPIndex];
            DateTime endTime = TimeLib.ConvertTo(seasonTableInfo.EndTime);
            var remainTime = endTime - TimeLib.Now;
            if (remainTime.TotalSeconds < 0)
            {
                long remainMintime = 0;
                int nextSeasonIndex = 0;
                foreach (var item in User.Inst.TBL.MarblePassSeason)
                {
                    var startTime = TimeLib.ConvertTo(item.Value.StartTime);
                    var nextRemainTime = (startTime - TimeLib.Now).TotalSeconds;
                    if (nextRemainTime < 0)
                        continue;
                    if (remainMintime == 0 || nextRemainTime < remainMintime)
                    {
                        remainMintime = (long)nextRemainTime;
                        nextSeasonIndex = item.Value.Index;
                    }
                }
                long nextStartTimeString = 0;
                if (User.Inst.TBL.MarblePassSeason.ContainsKey(nextSeasonIndex))
                {
                    nextStartTimeString = User.Inst.TBL.Gens.MarblePassSeason[nextSeasonIndex].StartTime;
                    completeSeasonPeriodText.text = UIUtil.ConvertSecToDateString(nextStartTimeString - TimeLib.Seconds);
                }
                else
                    completeSeasonPeriodText.text = "-";

                if (completeSeasonPeriodTitleText) completeSeasonPeriodTitleText.text = UIUtil.GetText("UI_MarblePass_003");
            }
            else
            {
                if (completeSeasonPeriodTitleText) completeSeasonPeriodTitleText.text = UIUtil.GetText("UI_MarblePass_004");
                completeSeasonPeriodText.text = UIUtil.ConvertSecToDateString((long)remainTime.TotalSeconds);
            }
        }
    }

    private void SetReward(RewardItem item, REWARD_TYPE rewardType, int rewardCount, int rewardValue, ref GameObject marbleEff, bool isPremium)
    {
        gameObject.SetActive(true);
        if (marbleEff != null) marbleEff.SetActive(false);

        bool getReward = false;
        if(User.Inst.Doc.MarblePass.Rewards.Free != null ) User.Inst.Doc.MarblePass.Rewards.Free.Contains(currentLevel);
        if (isPremium)
        {
            if(User.Inst.Doc.MarblePass.Rewards != null)
                getReward = User.Inst.Doc.MarblePass.Rewards.Premium.Contains(currentLevel);
        }

        string rewardIconName = "";
        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                {
                    if (User.Inst.TBL.Marble.ContainsKey(rewardValue))
                    {
                        if (item.rewardIcon) item.rewardIcon.gameObject.SetActive(false);
                        if (item.marbleGrp) item.marbleGrp.SetActive(true);
                        UIUtil.SetRewardIcon(rewardType, item.marbleIcon, rewardValue);
                        SetIdleEffect(rewardValue, ref marbleEff, item.marbleIcon.transform);
                    }
                    else
                    {
                        if (item.rewardIcon) item.rewardIcon.gameObject.SetActive(true);
                        if (item.marbleGrp) item.marbleGrp.SetActive(false);
                        UIUtil.SetRewardIcon(rewardType, item.rewardIcon, rewardValue);
                    }
                }
                break;
            default:
                {
                    if (item.rewardIcon) item.rewardIcon.gameObject.SetActive(true);
                    if (item.marbleGrp) item.marbleGrp.SetActive(false);
                    UIUtil.SetRewardIcon(rewardType, item.rewardIcon, rewardValue);
                }
                break;
        }

        if (item.countText) item.countText.text = rewardCount.ToString("n0");

        if (item.alarmGrp) item.alarmGrp.SetActive(false);
        if (item.effGrp) item.effGrp.SetActive(false);
        if (getReward == false)
        {
            if (User.Inst.Doc.MarblePass.Points >= tableInfo.NeedPoint)
            {
                if (item.alarmGrp) item.alarmGrp.SetActive(true);
                if (item.effGrp) item.effGrp.SetActive(true);
            }
        }
    }
    [Header("MarbleEff Size")]
    [SerializeField] Vector3 marbleEffSize = new Vector3(120f, 120f, 120f);
    private void SetIdleEffect(int index, ref GameObject effGo, Transform parent)
    {
        if (User.Inst.TBL.Marble.ContainsKey(index) == false)
            return;
        var marbleTable = User.Inst.TBL.Marble[index];

        if (effGo != null)
            Destroy(effGo);

        if (string.IsNullOrEmpty(marbleTable.IdleEffect))
            return;
        GameObject go = null;
        go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect);
        if (go != null)
        {
            effGo = GameObject.Instantiate(go) as GameObject;
            effGo.transform.SetParent(parent);
            Transform tfParticle = effGo.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = marbleEffSize;
        }

    }

    private void SetPassInfo()
    {
        if (User.Inst.TBL.MarblePassSeason.ContainsKey(currentMPIndex) == false)
            return;
        var tableInfo = User.Inst.TBL.MarblePassSeason[currentMPIndex];
        if (seasonNameText) seasonNameText.text = string.Format(UIUtil.GetText("UI_Common_Season"), currentMPIndex);
        if (seasonPeriodText)
        {
            DateTime endTime = TimeLib.ConvertTo(tableInfo.EndTime);
            var remainTime = endTime - TimeLib.Now;
            if (remainTime.TotalSeconds < 0)
            {
                long remainMintime = 0;
                int nextSeasonIndex = 0;
                foreach (var item in User.Inst.TBL.MarblePassSeason)
                {
                    var startTime = TimeLib.ConvertTo(item.Value.StartTime);
                    var nextRemainTime = (startTime - TimeLib.Now ).TotalSeconds;
                    if (nextRemainTime < 0)
                        continue;
                    if (remainMintime == 0 || nextRemainTime < remainMintime)
                    {
                        remainMintime = (long)nextRemainTime;
                        nextSeasonIndex = item.Value.Index;
                    }
                }
                long nextStartTimeString = 0;
                if (User.Inst.TBL.MarblePassSeason.ContainsKey(nextSeasonIndex))
                {
                    nextStartTimeString = User.Inst.TBL.Gens.MarblePassSeason[nextSeasonIndex].StartTime;
                    seasonPeriodText.text = UIUtil.ConvertSecToDateString(nextStartTimeString - TimeLib.Seconds);
                }
                else
                    seasonPeriodText.text = "-";
            }
            else
                seasonPeriodText.text = UIUtil.ConvertSecToDateString((long)remainTime.TotalSeconds);
        }
        if (passPointLevelText) passPointLevelText.text = User.Inst.Doc.MarblePass.Level.ToString();

        var marblePassInfos = User.Inst.TBL.Gens.MarblePass[tableInfo.PassGroup];
        int preLevelPoint = 0;
        int currentLevelPoint = 0;
        int level = User.Inst.Doc.MarblePass.Level;
        if (User.Inst.Doc.MarblePass.Level == 0)
            level = 1;
        if (marblePassInfos.ContainsKey(level ))
            preLevelPoint = marblePassInfos[level].NeedPoint;
        if (marblePassInfos.ContainsKey(level + 1))
            currentLevelPoint = marblePassInfos[level+1].NeedPoint;
        else
            currentLevelPoint = preLevelPoint;

        float percent = (float)(User.Inst.Doc.MarblePass.Points - preLevelPoint) / (float)(currentLevelPoint - preLevelPoint);
        if (percent > 1f)
            percent = 1f;
        if (passProgressImage) passProgressImage.fillAmount = percent;
    }
    public void RefreshEffect(bool isActive)
    {
        if(idleEff) idleEff.gameObject.SetActive(isActive);
        if(idleEff_premium) idleEff_premium.gameObject.SetActive(isActive);
    }
    float deltaTime;
    private void Update()
    {
        if (isShow == false)
            return;
        if (deltaTime > 1f)
        {
            CheckTime();
            deltaTime = 0f;
        }
        else
            deltaTime += Time.deltaTime;
    }

    #region 시간체크
    private void CheckTime()
    {
        var state = PassState.Wait;
        if (currentPassState == PassState.Wait)
        {
            if (User.Inst.TBL.Gens.MarblePassSeason.ContainsKey(currentMPIndex + 1) == false)
                return;

            var item = User.Inst.TBL.Gens.MarblePassSeason[currentMPIndex + 1];
            var openTime = TimeLib.ConvertTo(item.StartTime);
            var endTime = TimeLib.ConvertTo(item.EndTime);
            var now = TimeLib.Now;
            if (DateTime.Compare(now, openTime) > 0 && DateTime.Compare(endTime, now) > 0)
            {
                state = PassState.Pass;
            }
        }
        else
        {
            var item = User.Inst.TBL.Gens.MarblePassSeason[currentMPIndex];
            var openTime = TimeLib.ConvertTo(item.StartTime);
            var endTime = TimeLib.ConvertTo(item.EndTime);
            var now = TimeLib.Now;
            if (DateTime.Compare(now, openTime) > 0 && DateTime.Compare(endTime, now) > 0)
            {
                state = PassState.Pass;
            }
        }
        
        if (state != currentPassState)
        {
            SetMarblePass();
            if(state == PassState.Pass)
                if (scene.Lobby.Instance) scene.Lobby.Instance.SetMarblePassPush();
        }
        else
        {
            if (isRewardComplete)
            {
                SetCompletePeriod();
            }
            else
            {
                SetPassInfo();
            }
        }
    }
    #endregion

    [Serializable]
    public class RewardItem
    {
        [Header("On")]
        [SerializeField] public GameObject completeGrp;
        [SerializeField] public GameObject rewardGrp;
        [SerializeField] public Image rewardIcon;
        [SerializeField] public GameObject marbleGrp;
        [SerializeField] public Image marbleIcon;
        
        [Space(20)]
        [SerializeField] public Text countText;
        [SerializeField] public GameObject alarmGrp;
        [SerializeField] public GameObject effGrp;
    }
}
