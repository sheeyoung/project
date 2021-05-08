using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;
using scene;
using Net.Api;
using DG.Tweening;
using System;
using Platform;
public class IngameCoOpResultPanel : UIBasePanel
{
    [Header("WaveCount")]
    [SerializeField] Text waveCountText;
    [Header("WaveGauge")]
    [SerializeField] Text waveGaugePercentText;
    [SerializeField] RectTransform waveGaugeTr;
    [SerializeField] float playTime;
    [SerializeField] float playSpeed;
    [Header("UserInfo")]
    [SerializeField] IngameResultUserInfoGrp otherUserInfoGrp;
    [SerializeField] IngameResultUserInfoGrp myUserInfoGrp;
    [Header("Reward")]
    [SerializeField] GameObject gameResultRewardObejct;
    [SerializeField] Transform rewardCenterParent;
    [SerializeField] Transform rewardLeftParent;
    bool isCenter;    
    [SerializeField] private GameObject startAni_Lastobject;

    class ResultRewardItemInfo
    {
        public IngameResultAssetItem item;
        public Net.REWARD_TYPE type;
        public int value;
        public int addCount;
    }
    List<ResultRewardItemInfo> resultAssetItems;
    RewardGains CoopBoxRewards = new RewardGains();

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        resultAssetItems = new List<ResultRewardItemInfo>();

        startAni_Lastobject.GetComponent<DOTweenAnimation>().onComplete.AddListener(StartRewardAni);
    }
    
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        SoundManager.Inst.StopBGM();
        SoundManager.Inst.PlayEffect(18);
        playWaveAni = false;
        int wave = 0, startwave = 0, boxcount = 0;
        if (param.Length > 0)
        {
            wave = Convert.ToInt32(param[0]);
            startwave = Convert.ToInt32(param[1]);
            boxcount = Convert.ToInt32(param[2]);
        }
        SetClearWave(wave);
        SetWaveGauge(startwave, (int)User.Inst.Doc.Assets[(int)Net.ASSETS.ASSET_ITEMBOX_PIECE].Value, (int)User.Inst.TBL.Const["CONST_COOP_BOXPIECE_EXCHANGE_COUNT"].Value, boxcount);
        SetUserInfo();


        SetTotalReward((RewardGains)param[3]);
        SetSingular();
    }
    private void SetSingular()
    {
        try
        {
            long total = User.Inst.Doc.PvP.Record.Join + User.Inst.Doc.Team.Record.Join;
            if (total == 1)
                Auth.Inst.SendSingularEvent("Tutorial complete");
            else if (total == 2)
                Auth.Inst.SendSingularEvent("Play2");
            else if (total == 3)
                Auth.Inst.SendSingularEvent("Play3");
            else if (total == 4)
                Auth.Inst.SendSingularEvent("Play4");
            else if (total == 5)
                Auth.Inst.SendSingularEvent("Play5");
            else if (total == 6)
                Auth.Inst.SendSingularEvent("Play6");
            else if (total == 7)
                Auth.Inst.SendSingularEvent("Play7");
            else if (total == 8)
                Auth.Inst.SendSingularEvent("Play8");
            else if (total == 9)
                Auth.Inst.SendSingularEvent("Play9");
            else if (total == 10)
                Auth.Inst.SendSingularEvent("Play10");
            else if (total == 20)
                Auth.Inst.SendSingularEvent("Play20");
            else if (total == 50)
                Auth.Inst.SendSingularEvent("Play50");

            if (UIUtil.isCoOpFriend == false)
            {
                if (User.Inst.Doc.Team.Record.Join == 1)
                    Auth.Inst.SendSingularEvent("Co1");
                else if (User.Inst.Doc.Team.Record.Join == 2)
                    Auth.Inst.SendSingularEvent("Co2");
                else if (User.Inst.Doc.Team.Record.Join == 3)
                    Auth.Inst.SendSingularEvent("Co3");
                else if (User.Inst.Doc.Team.Record.Join == 4)
                    Auth.Inst.SendSingularEvent("Co4");
                else if (User.Inst.Doc.Team.Record.Join == 5)
                    Auth.Inst.SendSingularEvent("Co5");
                else if (User.Inst.Doc.Team.Record.Join == 6)
                    Auth.Inst.SendSingularEvent("Co6");
                else if (User.Inst.Doc.Team.Record.Join == 7)
                    Auth.Inst.SendSingularEvent("Co7");
                else if (User.Inst.Doc.Team.Record.Join == 8)
                    Auth.Inst.SendSingularEvent("Co8");
                else if (User.Inst.Doc.Team.Record.Join == 9)
                    Auth.Inst.SendSingularEvent("Co9");
                else if (User.Inst.Doc.Team.Record.Join == 10)
                    Auth.Inst.SendSingularEvent("Co10");
                else if (User.Inst.Doc.Team.Record.Join == 20)
                    Auth.Inst.SendSingularEvent("Co20");
                else if (User.Inst.Doc.Team.Record.Join == 50)
                    Auth.Inst.SendSingularEvent("Co50");
            }
            else
            {
                if (User.Inst.Doc.Team.Record.FriendJoin == 1)
                    Auth.Inst.SendSingularEvent("Fr_co1");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 5)
                    Auth.Inst.SendSingularEvent("Fr_co5");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 10)
                    Auth.Inst.SendSingularEvent("Fr_co10");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 20)
                    Auth.Inst.SendSingularEvent("Fr_co20");

                if (User.Inst.Doc.Team.Record.FriendJoin == 1)
                    Auth.Inst.SendSingularEvent("Fr_co1");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 2)
                    Auth.Inst.SendSingularEvent("Fr_co2");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 3)
                    Auth.Inst.SendSingularEvent("Fr_co3");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 4)
                    Auth.Inst.SendSingularEvent("Fr_co4");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 5)
                    Auth.Inst.SendSingularEvent("Fr_co5");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 6)
                    Auth.Inst.SendSingularEvent("Fr_co6");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 7)
                    Auth.Inst.SendSingularEvent("Fr_co7");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 8)
                    Auth.Inst.SendSingularEvent("Fr_co8");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 9)
                    Auth.Inst.SendSingularEvent("Fr_co9");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 10)
                    Auth.Inst.SendSingularEvent("Fr_co10");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 20)
                    Auth.Inst.SendSingularEvent("Fr_co20");
                else if (User.Inst.Doc.Team.Record.FriendJoin == 50)
                    Auth.Inst.SendSingularEvent("Fr_co50");
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }
    }
    private void SetClearWave(int totalWave)
    {
        if (waveCountText) waveCountText.text = totalWave.ToString();
    }

    private bool playWaveAni;
    private float curWaveValue;
    private float perSecAddWaveValue;
    private int endWaveValue;
    private int maxWaveValue;
    private int boxCount;
    private void SetWaveGauge(int startWaveValue, int endWaveValue, int maxWaveValue, int boxCount)
    {        
        playWaveAni = true;
        curWaveValue = startWaveValue;
        this.endWaveValue = endWaveValue;
        this.maxWaveValue = maxWaveValue;
        this.boxCount = boxCount;
        guageAniCount = 0;

        if (boxCount > 0)
        {
            int perValue = endWaveValue;
            perSecAddWaveValue = (float)(boxCount * 100f + perValue) / playTime;
        }
        else
        {
            int perValue = endWaveValue - startWaveValue;
            perSecAddWaveValue = (float)perValue / playTime;
        }
        float percent = (float)startWaveValue / (float)maxWaveValue;
        if (percent > 1f)
            percent = 1f;

        if (waveGaugeTr) waveGaugeTr.localScale = new Vector3(percent, 1, 1);
        percent *= 100f;
        if (waveGaugePercentText)
        {
            if(endWaveValue >= maxWaveValue)
                waveGaugePercentText.text = UIUtil.GetText("UI_Common_Max");
            else
                waveGaugePercentText.text = string.Format("{0}/{1}", endWaveValue, maxWaveValue);
        }
    }

    private void SetUserInfo()
    {
        if (otherUserInfoGrp) otherUserInfoGrp.Init(GamePvP.Instance.BattleMode.Away, false);
        if (myUserInfoGrp) myUserInfoGrp.Init(GamePvP.Instance.BattleMode.Home, true);
    }

    private void SetTotalReward(RewardGains rewards)
    {
        for (int i = 0; i < rewards.Count; i++)
        {
            if (rewards[i].Count == 0)
                continue;
            if ((Net.REWARD_TYPE)rewards[i].Type == Net.REWARD_TYPE.REWARD_COOPBOX)
                CoopBoxRewards.Add(rewards[i]);
            else
            {
                UIUtil.AddShowInGameReward((Net.REWARD_TYPE)rewards[i].Type);
                if (resultAssetItems.Count <= i)
                {
                    Transform parentTr = rewardCenterParent;
                    isCenter = true;
                    if (rewards.Count + boxCount >= 5)
                    {
                        parentTr = rewardLeftParent;
                        isCenter = false;
                    }
                    GameObject go = UIManager.Inst.CreateObject(gameResultRewardObejct, parentTr);
                    var item = go.GetComponent<IngameResultAssetItem>();
                    ResultRewardItemInfo info = new ResultRewardItemInfo()
                    {
                        item = item,
                        addCount = rewards[i].Count,
                        type = (Net.REWARD_TYPE)rewards[i].Type,
                        value = 0
                    };
                    resultAssetItems.Add(info);
                    Debug.Log("SetTotalReward111");
                }
                resultAssetItems[i].item.Init((Net.REWARD_TYPE)rewards[i].Type, 0);
            }
        }
        SoundManager.Inst.PlayEffect(19);
    }

    private void StartRewardAni()
    {
        SoundManager.Inst.PlayEffect(20);
        for (int i = 0; i < resultAssetItems.Count; i++)
        {
            resultAssetItems[i].item.AddAsset(resultAssetItems[i].addCount);
        }
    }

    public void OnClickConfirmButton()
    {
        ClosePanel();
    }

    public override void ClosePanel()
    {
        GamePvP.Instance.BattleMode.LeaveRoom();
    }

    int guageAniCount;
    private void Update()
    {
        if (playWaveAni)
        {
            try
            {
                curWaveValue += (perSecAddWaveValue * (Time.deltaTime / playTime));
                float percent = curWaveValue / (float)maxWaveValue;
                if (percent > 1f)
                    percent = 1f;

                if (waveGaugeTr) waveGaugeTr.localScale = new Vector3(percent, 1, 1);
                percent *= 100f;
                if (waveGaugePercentText) waveGaugePercentText.text = string.Format("{0}/{1}", (int)curWaveValue, maxWaveValue);
                
                if (boxCount > guageAniCount)
                {
                    Debug.Log("boxCount : " + boxCount + " guageAniCount : " + guageAniCount);
                    Debug.Log("curWaveValue : " + curWaveValue + " maxWaveValue : " + maxWaveValue);
                    if (curWaveValue >= maxWaveValue)
                    {
                        curWaveValue = 0;
                        PlayAni("BoxMax");
                        OnBoxMaxComplete(guageAniCount);
                        guageAniCount++;
                    }
                }
                else
                {
                    if (curWaveValue >= endWaveValue)
                    {
                        float endPercent = endWaveValue / (float)maxWaveValue;

                        if (waveGaugeTr) waveGaugeTr.localScale = new Vector3(endPercent, 1, 1);
                        endPercent *= 100f;
                        if (waveGaugePercentText)
                        {
                            if(endWaveValue >= maxWaveValue)
                                waveGaugePercentText.text = UIUtil.GetText("UI_Common_Max");
                            else
                                waveGaugePercentText.text = string.Format("{0}/{1}", (int)endWaveValue, maxWaveValue);
                        }
                        playWaveAni = false;
                    }
                }

            }
            catch
            {
                playWaveAni = false;
            }
        }
    }

    public void OnBoxMaxComplete(int idx)
    {
        Debug.Log("########OnBoxMaxComplete");
        bool isset = false;
        playWaveAni = true;
        if (resultAssetItems.Count > 0)
        {
            var item = resultAssetItems.Find(i => i.type == Net.REWARD_TYPE.REWARD_COOPBOX && i.value == CoopBoxRewards[idx].Value);
            if (item != null)
            {
                item.addCount++;
                item.item.AppearAni();
                isset = true;
            }
        }

        if (!isset)
        {
            Transform parentTr = rewardCenterParent;
            if (isCenter == false)
            {
                parentTr = rewardLeftParent;
            }
            GameObject go = UIManager.Inst.CreateObject(gameResultRewardObejct, parentTr);
            var item = go.GetComponent<IngameResultAssetItem>();
            ResultRewardItemInfo info = new ResultRewardItemInfo()
            {
                item = item,
                type = Net.REWARD_TYPE.REWARD_COOPBOX,
                value = CoopBoxRewards[idx].Value,
                addCount = 1,
            };
            resultAssetItems.Add(info);

            item.Init(Net.REWARD_TYPE.REWARD_COOPBOX, 0, CoopBoxRewards[idx].Value);
            item.AppearAni();
        }
    }
}
