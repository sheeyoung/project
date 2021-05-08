using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Net.Api;
using Net;
using UnityEngine.UI;
using DG.Tweening;

public class CommonRewardResultPopup : UIBasePanel
{
    [SerializeField] GameObject rewardItemObject;
    [SerializeField] Transform rewardItemContent;
    List<CommonRewardResultItem> rewardItems;
    Rewards rewards;
    RewardGains rewardGains;
    UIManager.PanelEndAction endAction;
    UIParticlePlay rewardResultEff;
    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);
    [SerializeField] float openDeltaTime = 0.5f;
    [SerializeField] Button playAniColliderGrp;

    Coroutine rewardCoroutine;
    Coroutine rewardGainsCoroutine;

    [SerializeField] Text buttonText;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        rewardItems = new List<CommonRewardResultItem>();

        rewards = new Rewards();
        rewardGains = new RewardGains();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        playAniColliderGrp.onClick.AddListener(Skip);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);

        rewards.Clear();
        rewardGains.Clear();

        rewardCoroutine = null;
        rewardGainsCoroutine = null;

        for (int i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].gameObject.SetActive(false);
        }

        //if (buttonText) buttonText.text = UIUtil.GetText("UI_Common_Skip");
        //var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
        showCount = 0;
        isWait = false;
        isSkip = false;

        if (param[0] is Rewards)
        {
            var temprewards = (Rewards)param[0];
            for(int i = 0; i < temprewards.Count; i++)
            {
                var item = rewards.Find((x) => { return x.Type == temprewards[i].Type && x.Value == temprewards[i].Value; });
                if (item == null)
                    rewards.Add(new Reward() { Type = temprewards[i].Type, Value = temprewards[i].Value, Count = temprewards[i].Count });
                else
                    item.Count += temprewards[i].Count;

                //if(lobbyPanel) lobbyPanel.PlayAssetGetEffect((REWARD_TYPE)temprewards[i].Type);
            }
            if (buttonText)
            {
                if(temprewards.Count > 1)
                    buttonText.text = UIUtil.GetText("UI_Common_Skip");
                else
                    buttonText.text = UIUtil.GetText("UI_Common_Ok");
            }
            rewardCoroutine = StartCoroutine(SetRewards());
        }
        else if(param[0] is RewardGains)
        {
            var tempRewardGains = (RewardGains)param[0];

            for (int i = 0; i < tempRewardGains.Count; i++)
            {
                var item = rewardGains.Find((x) => { return x.Type == tempRewardGains[i].Type && x.Value == tempRewardGains[i].Value; });
                if (item == null)
                    rewardGains.Add(new RewardGain() { Type = tempRewardGains[i].Type, Value = tempRewardGains[i].Value, Count = tempRewardGains[i].Count });
                else
                    item.Count += tempRewardGains[i].Count;
                //if(lobbyPanel) lobbyPanel.PlayAssetGetEffect((REWARD_TYPE)tempRewardGains[i].Type);
            }
            if (buttonText)
            {
                if (tempRewardGains.Count > 1)
                    buttonText.text = UIUtil.GetText("UI_Common_Skip");
                else
                    buttonText.text = UIUtil.GetText("UI_Common_Ok");
            }
            rewardGainsCoroutine = StartCoroutine(SetRewardGains());
        }
        else
        {
            ClosePanel();
            return;
        }
        endAction = null;
        if (param != null && param.Length > 1)
            endAction = (UIManager.PanelEndAction)param[1];

        if (rewardResultEff == null)
        {
            rewardResultEff = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_CommonRewardResultPopup_01");
            var effTranform = rewardResultEff.Go.GetComponent<Transform>();
            effTranform.localScale = effSize;
            //rewardResultEff.SetPosition(transform.position);
        }
        rewardResultEff.Play();
        playAniColliderGrp.gameObject.SetActive(true);

    }
    public override void ClosePanel()
    {
        SoundManager.Inst.SetBgmToggle(false);
        endAction?.Invoke();
        var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
        if (lobbyPanel != null)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                if (lobbyPanel) lobbyPanel.PlayAssetGetEffect((REWARD_TYPE)rewards[i].Type);
            }
            for (int i = 0; i < rewardGains.Count; i++)
            {
                if (lobbyPanel) lobbyPanel.PlayAssetGetEffect((REWARD_TYPE)rewardGains[i].Type);
            }

            lobbyPanel.SetMarbleTabAlarm();
        }
        SoundManager.Inst.PlayUIEffect(20);
        base.ClosePanel();
    }
    AudioSource audioSource;
    float waitTime = 0f;
    IEnumerator SetRewards()
    {
        if (rewards == null)
            yield break;

        for(int i = 0; i < rewards.Count; i++)
        {
            if(rewardItems.Count <= i)
            {
                var go = UIManager.Inst.CreateObject(rewardItemObject, rewardItemContent);
                var rewardItem = go.GetComponent<CommonRewardResultItem>();
                rewardItems.Add(rewardItem);
                if(waitTime == 0)
                {
                    waitTime = rewardItem.GetAniTime();
                }
            }
            
            rewardItems[i].SetItem((REWARD_TYPE)rewards[i].Type, rewards[i].Count.ToString("n0"), rewards[i].Value);
            string effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_01";
            if (rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE
                    || rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_LEGEND
                    || rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_MYTH
                    || rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_NORMAL
                    || rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_RARE
                    || rewards[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                if (User.Inst.TBL.Marble.ContainsKey(rewards[i].Value))
                {
                    var rewardTableInfo = User.Inst.TBL.Marble[rewards[i].Value];
                    if (rewardTableInfo.Rarity == (int)MarbleRarity.Legendary)
                        effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_02";
                    else if (rewardTableInfo.Rarity == (int)MarbleRarity.Chronicle)
                        effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_03";
                }
            }
            rewardItems[i].SetEff(effPath);
            SoundManager.Inst.PlayUIEffect(23);

            var rewardType = (REWARD_TYPE)rewards[i].Type;
            if (rewardType == REWARD_TYPE.REWARD_MARBLE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewards[i].Value];
                bool isNew = User.Inst.Doc.MarbleInven[rewards[i].Value].Count < rewards[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewards[i].Value].Grade;

                if (marbleInfo.Rarity == (int)MarbleRarity.Chronicle
                    || marbleInfo.Rarity == (int)MarbleRarity.Legendary)
                {
                    isWait = true;

                    UIManager.PanelEndAction end = RestartPlayAni;
                    yield return new WaitForSeconds(waitTime);
                    rewardItems[i].EndSpecialMarbleAni();
                    
                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewards[i].Value, end, isNew,false);
                }
                else if (User.Inst.Doc.MarbleInven[rewards[i].Value].Count < rewards[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewards[i].Value].Grade)
                {
                    isWait = true;
                    UIManager.PanelEndAction end = RestartPlayAni;
                    yield return new WaitForSeconds(openDeltaTime);
                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewards[i].Value, end, isNew, false);
                }
            }
            if (isWait)
            {
                showCount = i;
                while (isWait)
                    yield return null;
            }
            else
            {
                showCount = i;
                yield return new WaitForSeconds(openDeltaTime);
            }
        }
        playAniColliderGrp.gameObject.SetActive(false);
        SoundManager.Inst.SetBgmToggle(false);
        if (buttonText) buttonText.text = UIUtil.GetText("UI_Common_Ok");
    }

    IEnumerator ShowRewards()
    {
        if (rewards == null)
            yield break;

        for (int i = showCount+1; i < rewards.Count; i++)
        {
            if (rewards.Count <= i)
                continue;
            var rewardType = (REWARD_TYPE)rewards[i].Type;
            if (rewardType == REWARD_TYPE.REWARD_MARBLE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewards[i].Value];
                bool isNew = User.Inst.Doc.MarbleInven[rewards[i].Value].Count < rewards[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewards[i].Value].Grade;

               if (User.Inst.Doc.MarbleInven[rewards[i].Value].Count < rewards[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewards[i].Value].Grade)
                {
                    isWait = true;
                    UIManager.PanelEndAction end = RestartPlayAni;
                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewards[i].Value, end, isNew, false);
                    //SoundManager.Inst.PlayUIEffect(24);
                }
            }
            if (isWait)
            {
                while (isWait)
                    yield return null;
            }
        }
    }

    IEnumerator SetRewardGains()
    {
        if (rewardGains == null)
            yield break;
        for (int i = 0; i < rewardGains.Count; i++)
        {
            if (rewardItems.Count <= i)
            {
                var go = UIManager.Inst.CreateObject(rewardItemObject, rewardItemContent);
                var rewardItem = go.GetComponent<CommonRewardResultItem>();
                rewardItems.Add(rewardItem);
                if (waitTime == 0)
                {
                    waitTime = rewardItem.GetAniTime();
                }
            }
            rewardItems[i].SetItem((REWARD_TYPE)rewardGains[i].Type, rewardGains[i].Count.ToString("n0"), rewardGains[i].Value);
            string effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_01";
            if (rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE
                    || rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_LEGEND
                    || rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_MYTH
                    || rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_NORMAL
                    || rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_RARE
                    || rewardGains[i].Type == (int)REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                if (User.Inst.TBL.Marble.ContainsKey(rewardGains[i].Value))
                {
                    var rewardTableInfo = User.Inst.TBL.Marble[rewardGains[i].Value];
                    if (rewardTableInfo.Rarity == (int)MarbleRarity.Legendary)
                        effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_02";
                    else if (rewardTableInfo.Rarity == (int)MarbleRarity.Chronicle)
                        effPath = "Prefab/Effect/UI/eff_UI_CommonRewardResultItem_03";
                }
            }
            rewardItems[i].SetEff(effPath);
            SoundManager.Inst.PlayUIEffect(23);

            var rewardType = (REWARD_TYPE)rewardGains[i].Type;
            if (rewardType == REWARD_TYPE.REWARD_MARBLE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewardGains[i].Value];
                bool isNew = User.Inst.Doc.MarbleInven[rewardGains[i].Value].Count < rewardGains[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewardGains[i].Value].Grade;

                if (marbleInfo.Rarity == (int)MarbleRarity.Chronicle
                    || marbleInfo.Rarity == (int)MarbleRarity.Legendary)
                {
                    isWait = true;

                    UIManager.PanelEndAction end = RestartPlayAni;
                    yield return new WaitForSeconds(waitTime);

                    rewardItems[i].EndSpecialMarbleAni();

                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewardGains[i].Value, end, isNew, false);
                    //SoundManager.Inst.PlayUIEffect(24);
                }
                else if (User.Inst.Doc.MarbleInven[rewardGains[i].Value].Count < rewardGains[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewardGains[i].Value].Grade)
                {
                    isWait = true;
                    UIManager.PanelEndAction end = RestartPlayAni;
                    yield return new WaitForSeconds(openDeltaTime);
                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewardGains[i].Value, end, isNew, false);
                    //SoundManager.Inst.PlayUIEffect(24);
                }
            }

            if (isWait)
            {
                showCount = i;
                while (isWait)
                    yield return null;
            }
            else
            {
                showCount = i;
                yield return new WaitForSeconds(openDeltaTime);
            }
        }
        playAniColliderGrp.gameObject.SetActive(false);
        SoundManager.Inst.SetBgmToggle(false);
        if (buttonText) buttonText.text = UIUtil.GetText("UI_Common_Ok");
    }
    IEnumerator ShowRewardGains()
    {
        if (rewardGains == null)
            yield break;
        for (int i = showCount+1; i < rewardGains.Count; i++)
        {
            if (rewardGains.Count <= i)
                continue;
            var rewardType = (REWARD_TYPE)rewardGains[i].Type;
            if (rewardType == REWARD_TYPE.REWARD_MARBLE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewardGains[i].Value];
                bool isNew = User.Inst.Doc.MarbleInven[rewardGains[i].Value].Count < rewardGains[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewardGains[i].Value].Grade;

                if (User.Inst.Doc.MarbleInven[rewardGains[i].Value].Count < rewardGains[i].Count
                    && marbleInfo.InitialGrade == User.Inst.Doc.MarbleInven[rewardGains[i].Value].Grade)
                {
                    isWait = true;
                    UIManager.PanelEndAction end = RestartPlayAni;
                    UIManager.Inst.OpenPanel(UIPanel.NewMarblePopup, rewardGains[i].Value, end, isNew, false);
                    //SoundManager.Inst.PlayUIEffect(24);
                }
            }

            if (isWait)
            {
                while (isWait)
                    yield return null;
            }
        }
    }

    private bool isWait = false;
    private void RestartPlayAni()
    {
        isWait = false;
    }

    int showCount;
    bool isSkip;
    private void Skip()
    {
        if (isSkip)
            return;
        isSkip = true;
        isWait = false;
        SoundManager.Inst.SetBgmToggle(false);
        playAniColliderGrp.gameObject.SetActive(false);
        if (buttonText) buttonText.text = UIUtil.GetText("UI_Common_Ok");
        if (rewardCoroutine != null) StopCoroutine(rewardCoroutine);
        if(rewardGainsCoroutine != null) StopCoroutine(rewardGainsCoroutine);
        if(audioSource != null) audioSource.Stop();

        if (rewards.Count > 0)
            StartCoroutine(ShowRewards());
        for (int i = showCount; i < rewards.Count; i++)
        {
            if (rewardItems.Count <= i)
            {
                var go = UIManager.Inst.CreateObject(rewardItemObject, rewardItemContent);
                var rewardItem = go.GetComponent<CommonRewardResultItem>();
                rewardItems.Add(rewardItem);
            }
            rewardItems[i].SetItem((REWARD_TYPE)rewards[i].Type, rewards[i].Count.ToString("n0"), rewards[i].Value);
            
            if ((REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE
                || (REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || (REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE_MYTH
                || (REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || (REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE_RARE
                || (REWARD_TYPE)rewards[i].Type == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewards[i].Value];
                if (marbleInfo.Rarity == (int)MarbleRarity.Chronicle
                    || marbleInfo.Rarity == (int)MarbleRarity.Legendary)
                {
                    rewardItems[i].EndSpecialMarbleAni();
                }
            }
        }


        if (rewardGains.Count > 0)
            StartCoroutine(ShowRewardGains());
        for (int i = showCount; i < rewardGains.Count; i++)
        {
            if (rewardItems.Count <= i)
            {
                var go = UIManager.Inst.CreateObject(rewardItemObject, rewardItemContent);
                var rewardItem = go.GetComponent<CommonRewardResultItem>();
                rewardItems.Add(rewardItem);
            }
            rewardItems[i].SetItem((REWARD_TYPE)rewardGains[i].Type, rewardGains[i].Count.ToString("n0"), rewardGains[i].Value);

            if((REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE
                || (REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || (REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE_MYTH
                || (REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || (REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE_RARE
                || (REWARD_TYPE)rewardGains[i].Type == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                var marbleInfo = User.Inst.TBL.Marble[rewardGains[i].Value];
                if (marbleInfo.Rarity == (int)MarbleRarity.Chronicle
                    || marbleInfo.Rarity == (int)MarbleRarity.Legendary)
                {
                    rewardItems[i].EndSpecialMarbleAni();
                }
            }
        }
    }

}
