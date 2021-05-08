using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Libs.Utils;
using System;
using Net.Api;
using Net.Impl;

public class BattlePanel : UIBaseGrp
{
    [Header("TopMenu")]
    [SerializeField] Button RankingButton;
    [SerializeField] Button ContributivenessButton;
    [Header("TopMenu-Quest")]
    [SerializeField] Button QuestButton;
    [SerializeField] GameObject questNotiGrp;
    [SerializeField] GameObject questRewardGrp;
    [SerializeField] Image questShopImage;
    [SerializeField] GameObject questMarbleGrp;
    [SerializeField] Image questMarbleImage;
    [SerializeField] GameObject questTimeGrp;
    [SerializeField] Text questTimeText;
    [Header("SubMenu-CoopBox")]
    [SerializeField] Button RewardBoxButton;
    [SerializeField] GameObject coopBoxTimeGrp;
    [SerializeField] Text coopBoxRemainTimeText;
    [SerializeField] GameObject coopBoxNotiGrp;
    
    [Header("Deck")]
    [SerializeField] Button marbleDeckLeftButton;
    [SerializeField] Button marbleDeckRightButton;
    [SerializeField] MarbleDeckGrp marbleDeckGrp;
    [SerializeField] Button marbleDeckEditButton;
    [Header("Battle")]
    [SerializeField] Button CoOpButton;
    [SerializeField] Button BattleButton;
    [SerializeField] Button FriendsButton;
    [Header("CoOpGrp")]
    [SerializeField] Text coopBattleCountText;
    [SerializeField] Text coopSeasonNameText;
    [SerializeField] Image coopBossImage;
    [SerializeField] Text coopBossName;
    [SerializeField] RectTransform coopGauge;
    [SerializeField] Text coopBoxPiecePercent;
    [SerializeField] Text coopTicketGenTimeText;
    [SerializeField] GameObject coopOnGrp;
    [SerializeField] GameObject coopOffGrp;
    [SerializeField] GameObject coopOffADGrp;

    [Header("PvPGrp")]
    [SerializeField] GameObject pvpADOnGrp;
    [SerializeField] GameObject pvpADOffGrp;
    [SerializeField] GameObject pvpADGrp;
    [SerializeField] GameObject pvpNoADGrp;
    [SerializeField] Image pvpLeagueIconImage;
    [SerializeField] Text pvpTropyCountText;
    //[Header("PVP - AD")]
    //[SerializeField] Text ADPvpLeagueNameText;
    //[SerializeField] Image ADPvpLeagueIconImage;
    //[SerializeField] Text ADPvpTropyCountText;
    //[Header("PVP - NoAD")]
    //[SerializeField] Text NoADPvpLeagueNameText;
    //[SerializeField] Image NoADPvpLeagueIconImage;
    //[SerializeField] Text NoADPvpTropyCountText;

    [Header("MarblePass")]
    [SerializeField] MarblePassGrp marblePassGrp;
    public override void Init()
    {
        base.Init();
        SetDeck();
        SetCoOpMode();
        SetPvPMode();
        SetCoOpBoxTime();
        SetQuestNoti();
        marblePassGrp.SetMarblePass();
    }

    private const int DeckMax = 3;

    protected override void SetEvent()
    {
        base.SetEvent();
        pvpADOffGrp.GetComponent<Button>().onClick.AddListener(OnClickPvPADBonusButton);
        if (marbleDeckGrp)
        {
            marbleDeckGrp.marbleDeckButtonClickEvnet += OnclickDeckItem;
        }

        if (marbleDeckLeftButton) marbleDeckLeftButton.onClick.AddListener(() => { OnClickMarbleArrowButton(true); });
        if (marbleDeckRightButton) marbleDeckRightButton.onClick.AddListener(() => { OnClickMarbleArrowButton(false); });

        if (marbleDeckEditButton) marbleDeckEditButton.onClick.AddListener(OnClickMarbleEditDeckButton);
    }
    private void SetDeck()
    {
        if (marbleDeckGrp)
        {
            marbleDeckGrp.InitDeck(User.Inst.Doc.SelectedDeck);
        }

        if (marbleDeckLeftButton) marbleDeckLeftButton.gameObject.SetActive(true);
        if (marbleDeckRightButton) marbleDeckRightButton.gameObject.SetActive(true);

        int selectCount = User.Inst.Doc.SelectedDeck;
        if (selectCount <= 1)
        {
            if(marbleDeckLeftButton) marbleDeckLeftButton.gameObject.SetActive(false);
        }
        if (selectCount >= DeckMax)
        {
            if(marbleDeckRightButton) marbleDeckRightButton.gameObject.SetActive(false);
        }
    }

    public override void Refresh()
    {
        SetDeck();
        marblePassGrp.SetMarblePass();
    }

    bool playCoOpTime;
    long yesterSec = 0;
    private void SetCoOpMode()
    {
        coopOffGrp.SetActive(false);
        coopOnGrp.SetActive(false);
        playCoOpTime = false;
        if (BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_COOP_TICKET) > 0)
        {
            coopOnGrp.SetActive(true);
            if (coopBattleCountText)
            {
                long coopMax = User.Inst.TBL.Assets[(int)ASSETS.ASSET_COOP_TICKET].MaxValue;
                if (BaseImpl.Inst.CheckJoinSubscribe(User.Inst))
                    coopMax += (int)Sheet.TBL.Const["CONST_SUBSCRIBE_COOP_COUNT_INC"].Value;
                long currentCoopCount = BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_COOP_TICKET);
                coopBattleCountText.text = string.Format("{0}/{1}", currentCoopCount, coopMax);
            }
            
        }
        else
        {
            if (coopOffGrp)
            {
                coopOffGrp.SetActive(true);
                if (BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET) > 0)
                {
                    if (coopOffADGrp) coopOffADGrp.SetActive(true);
                }
                else
                {
                    if(coopOffADGrp) coopOffADGrp.SetActive(false);
                }
            }
            long now = TimeLib.Seconds;
            DateTime nowDate = TimeLib.Now;
            DateTime yesterDay = nowDate.AddDays(1);
            yesterDay = new DateTime(yesterDay.Year, yesterDay.Month, yesterDay.Day, 0,0,0);
            yesterSec = TimeLib.ConvertTo(yesterDay);
            long genTime = yesterSec - now;
            if (genTime < 0)
                genTime = 0;
            if(coopTicketGenTimeText) coopTicketGenTimeText.text = UIUtil.ConvertSecToTimeString(genTime, 1);
            playCoOpTime = true;
        }

        int seasonIndex = 9999;
        foreach (var item in User.Inst.TBL.Season)
        {
            DateTime start;
            DateTime end;
            DateTime.TryParse(item.Value.StartTime, out start);
            long startTime = TimeLib.ConvertTo(start);
            DateTime.TryParse(item.Value.EndTime, out end);
            long endTime = TimeLib.ConvertTo(end);

            long now = TimeLib.Seconds;
            if (startTime <= now && now <= endTime)
            {
                if (seasonIndex > item.Key)
                    seasonIndex = item.Key;
                break;
            }
        }
        var seasonInfo = User.Inst.TBL.Season[seasonIndex];
        var seasonLangInfo = User.Inst.Langs.Season[seasonIndex];
        var bossInfo = User.Inst.TBL.Monster[seasonInfo.BossID];
        var bossLangInfo = User.Inst.Langs.Monster[seasonInfo.BossID];
        if (coopSeasonNameText) coopSeasonNameText.text = seasonLangInfo.SeasonName;
        if (coopBossImage) UIManager.Inst.SetSprite(coopBossImage, UIManager.AtlasName.Enemy, bossInfo.Icon);
        if (coopBossName) coopBossName.text = bossLangInfo.Name;

        int currentItemBoxPiece = (int)User.Inst.Doc.Assets[(int)Net.ASSETS.ASSET_ITEMBOX_PIECE].Value;
        int maxBoxPiece = (int)User.Inst.TBL.Const["CONST_COOP_BOXPIECE_EXCHANGE_COUNT"].Value;
        float percent = (float)currentItemBoxPiece / (float)maxBoxPiece;
        if(coopGauge) coopGauge.localScale = new Vector3(percent, 1f, 1f);

        if(coopBoxPiecePercent) coopBoxPiecePercent.text = string.Format("{0}/{1}", currentItemBoxPiece, maxBoxPiece);

    }

    private void SetPvPMode()
    {
        SetPvPADBonus();
        string legueName = UIUtil.GetLeagueName(User.Inst.Doc.PvP.Score);
        int index = UIUtil.GetLeagueIndex(User.Inst.Doc.PvP.Score);
        var tableInfo = User.Inst.TBL.PvPLeagueName[index];
        //if (ADPvpLeagueNameText) ADPvpLeagueNameText.text = legueName;
        //if (NoADPvpLeagueNameText) NoADPvpLeagueNameText.text = legueName;
        //if (ADPvpLeagueIconImage) UIManager.Inst.SetSprite(ADPvpLeagueIconImage, UIManager.AtlasName.LeagueIcon, tableInfo.LeagueIcon);
        //if (NoADPvpLeagueIconImage) UIManager.Inst.SetSprite(NoADPvpLeagueIconImage, UIManager.AtlasName.LeagueIcon, tableInfo.LeagueIcon);
        //if (ADPvpTropyCountText) ADPvpTropyCountText.text = User.Inst.Doc.PvP.Score.ToString("n0");
        //if (NoADPvpTropyCountText) NoADPvpTropyCountText.text = User.Inst.Doc.PvP.Score.ToString("n0");
        if(pvpLeagueIconImage) UIManager.Inst.SetSprite(pvpLeagueIconImage, UIManager.AtlasName.LeagueIcon, tableInfo.LeagueIcon);
        if (pvpTropyCountText) pvpTropyCountText.text = User.Inst.Doc.PvP.Score.ToString("n0");
    }

    private void SetPvPADBonus()
    {
        if(User.Inst.Doc.BonusAd > 0)
        {
            //광고를 보고 보너스를 얻은 상태
            pvpADOnGrp.SetActive(true);
            pvpADOffGrp.SetActive(false);
            //pvpADGrp.SetActive(true);
            //pvpNoADGrp.SetActive(false);
        }
        else
        {
            pvpADOnGrp.SetActive(false);
            pvpADOffGrp.SetActive(BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_PVP_ADD_REWARD) > 0);
            //if(BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_PVP_ADD_REWARD) > 0)
            //{
            //    pvpADGrp.SetActive(true);
            //    pvpNoADGrp.SetActive(false);
            //}
            //else
            //{
            //    pvpADGrp.SetActive(false);
            //    pvpNoADGrp.SetActive(true);
            //}
        }
    }

    public void OnclickDeckItem(int deckCount)
    {
        try
        {
            Debug.Log("OnclickDeckItem");
            scene.Lobby.Instance.lobbyPanel.GoToMenu(LobbyBottomMenu.Marble);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    
    //public void SetQuestNoti()
    //{
    //    bool isQuestComplete = false;
    //    foreach(var item in User.Inst.Doc.Quest.Items.Values)
    //    {
    //        int questID = item.Index;
    //        var questInfo = User.Inst.TBL.Quest[questID];

    //        int userQuestCount = item.Count;
    //        int questMaxCount = questInfo.QuestValue;
    //        if(userQuestCount >= questMaxCount)
    //        {
    //            isQuestComplete = true;
    //            break;
    //        }
    //    }
    //    if(isQuestComplete)
    //    {
    //        questNotiGrp.SetActive(true);
    //        return;
    //    }

    //    SetAttendQuestNoti();
    //}
    public void SetQuestNoti()
    {
        bool isQuestComplete = false;

        foreach (var item in User.Inst.Doc.Quest.Items.Values)
        {
            if (item.GetReward)
                continue;
            int questID = item.Index;
            var questInfo = User.Inst.TBL.Quest[questID];

            int userQuestCount = item.Count;
            int questMaxCount = questInfo.QuestValue;
            if (userQuestCount >= questMaxCount)
            {
                isQuestComplete = true;
                break;
            }
        }
        questNotiGrp.SetActive(isQuestComplete);

        bool isAttendQuestComplete = false;
        REWARD_TYPE rewardType = REWARD_TYPE.ASSET_FREE_GOLD;
        int rewardValue = 0;
        int minAttendTime = -1;
        foreach (var item in User.Inst.Doc.Quest.AttendItems.Values)
        {
            int currentID = item.Index;
            var dailyRewardInfo = User.Inst.TBL.DailyReward[currentID];
            if (item.GetReward)
                continue;
            if (dailyRewardInfo.Type == 2)
            {
                int attendTime = AppClient.Inst.GetConnTime() - AppClient.Inst.GetRewardTime();
                int needPlayTimeSec = 60 * dailyRewardInfo.NeedPlayTime;

                if (attendTime >= needPlayTimeSec)
                {
                    //출석시간 달성
                    isAttendQuestComplete = true;
                    minAttendTime = 0;
                    rewardType = UIUtil.GetRewardType(dailyRewardInfo.RewardType);
                    rewardValue = item.CompleteRewardValue;
                }
                else
                {
                    if (minAttendTime == -1 || minAttendTime > attendTime)
                    {
                        minAttendTime = needPlayTimeSec - attendTime;
                        rewardType = UIUtil.GetRewardType(dailyRewardInfo.RewardType);
                        rewardValue = item.CompleteRewardValue;
                    }
                }
            }
            else
            {
                int attendTime = AppClient.Inst.GetConnTime();
                int needPlayTimeSec = 60 * dailyRewardInfo.NeedPlayTime;

                if (attendTime >= needPlayTimeSec)
                {
                    //출석시간 달성
                    isAttendQuestComplete = true;
                }
                else
                {
                    if (minAttendTime == -1 || minAttendTime > attendTime)
                    {
                        minAttendTime = needPlayTimeSec - attendTime;
                        rewardType = UIUtil.GetRewardType(dailyRewardInfo.RewardType);
                        rewardValue = item.CompleteRewardValue;
                    }
                }
            }
        }
        if(isQuestComplete == false)
            questNotiGrp.SetActive(isAttendQuestComplete);
        if(minAttendTime != -1)
        {
            questRewardGrp.SetActive(true);
            questTimeGrp.SetActive(false);

            if(rewardType == REWARD_TYPE.REWARD_MARBLE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            {
                questMarbleGrp.SetActive(true);
                questShopImage.gameObject.SetActive(false);
                UIUtil.SetRewardIcon(rewardType, questMarbleImage, rewardValue);
            }
            else
            {
                questMarbleGrp.SetActive(false);
                questShopImage.gameObject.SetActive(true);
                UIUtil.SetRewardIcon(rewardType, questShopImage, rewardValue);
            }

            if(isAttendQuestComplete == false)
            {
                questTimeGrp.SetActive(true);
                questTimeText.text = UIUtil.ConvertSecToTimeString(minAttendTime, 1);
            }
        }
        else
        {
            questRewardGrp.SetActive(false);
            questTimeGrp.SetActive(false);
        }

    }

    private void SetCoOpBoxTime()
    {
        long minTime = 0;
        int openCount = 0;
        foreach(var item in User.Inst.Doc.BoxSlot)
        {
            if (item.Value.BoxCount <= 0)
                continue;
            var openTime = item.Value.BoxOpenDate;
            var now = Libs.Utils.TimeLib.Seconds;
            if (openTime > now)
            {
                if(minTime == 0)
                    minTime = openTime - now;
                if (minTime > openTime - now)
                    minTime = openTime - now;
            }
            else
                openCount++;
        }
        if (coopBoxNotiGrp) coopBoxNotiGrp.SetActive(openCount > 0);
        if (minTime == 0)
        {
            if (coopBoxTimeGrp) coopBoxTimeGrp.SetActive(false);
        }
        else
        {
            if (coopBoxTimeGrp) coopBoxTimeGrp.SetActive(true);
            if (coopBoxRemainTimeText) coopBoxRemainTimeText.text = UIUtil.ConvertSecToTimeString(minTime, 1);
        }
    }

    float playTime = 0f;
    private void Update()
    {
        if (playTime < 1f)
        {
            playTime += Time.deltaTime;
            return;
        }
        playTime = 0f;

        if (playCoOpTime)
        {
            long now = TimeLib.Seconds;
            long genTime = yesterSec - now;
            if (genTime <= 0)
            {
                SetCoOpMode();
                return;
            }
            if (coopTicketGenTimeText) coopTicketGenTimeText.text = UIUtil.ConvertSecToTimeString(genTime, 1);
        }
        SetCoOpBoxTime();
        SetQuestNoti();
    }
    #region Button click event
    public void OnClickQuestButton()
    {
        Debug.Log("OnClickQuestButton");
        if (User.Inst.Doc.Quest.Items.Count <= 0)
        {
            Net.ImplBase.Actor.Parser<Net.Api.QuestInit.Ack>(new Net.Api.QuestInit.Req(), (ack) =>
            {
                UIManager.Inst.OpenPanel(UIPanel.DailyQuestPopup);
            });
        }
        else
            UIManager.Inst.OpenPanel(UIPanel.DailyQuestPopup);
    }
    public void OnClickRankingButton()
    {
        Debug.Log("OnClickRankingButton");
        UIManager.Inst.OpenPanel(UIPanel.RankingPopup);
    }
    public void OnClickRewardBoxButton()
    {
        Debug.Log("OnClickRewardBoxButton");
        UIManager.Inst.OpenPanel(UIPanel.BoxPopup);
    }
    public void OnClickContributivenessButton()
    {
        Debug.Log("OnClickContributivenessButton");
    }
    public void OnClickCoOpButton()
    {
        Debug.Log("OnClickCoOpButton");
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_COOP_TICKET, -1))
        {
            //협동전 횟수 잇음
            UIManager.Inst.OpenPanel(UIPanel.CoOpBattlePopup);
        }
        else
        {
            //협동전 횟수 없음
            if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1))
            {
                //광고해서 협동전 가능
                UIManager.Inst.ShowAD("AddCoOpCount", Success, null, NotLoad);
                void Success()
                {
                    UIManager.Inst.ShowConnectingPanel(true);
                    ImplBase.Actor.Parser<AddCoOpTicket.Ack>(new AddCoOpTicket.Req(), (ack) =>
                    {
                        UIManager.Inst.ShowConnectingPanel(false);
                        if (ack.Result != RESULT.OK)
                        {
                            return;
                        }
                        SetCoOpMode();
                    });
                }
                void NotLoad()
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
                }
            }
            else
            {
                //광고티켓도없음
                if (Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1) == false)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
                    return;
                }

            }
        }

        //UIManager.Inst.OpenPanel(UIPanel.CoOpBattlePopup);
        //if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_COOP_TICKET, -1))
        //{
        //    //협동전 횟수 잇음
        //    scene.Lobby.Instance.ReqQuickBattleJoin(Game.MODE.COOP);
        //}
        //else
        //{
        //    //협동전 횟수 없음
        //    if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1))
        //    {
        //        //광고해서 협동전 가능
        //        UIManager.Inst.ShowAD("AddCoOpCount", Success, null, NotLoad);
        //        void Success()
        //        {
        //            UIManager.Inst.ShowConnectingPanel(true);
        //            ImplBase.Actor.Parser<AddCoOpTicket.Ack>(new AddCoOpTicket.Req(), (ack) =>
        //            {
        //                UIManager.Inst.ShowConnectingPanel(false);
        //                if (ack.Result != RESULT.OK)
        //                {
        //                    return;
        //                }
        //                SetCoOpMode();
        //            });
        //        }
        //        void NotLoad()
        //        {
        //            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
        //        }
        //    }
        //    else
        //    {
        //        //광고티켓도없음
        //        if (Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1) == false)
        //        {
        //            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
        //            return;
        //        }

        //    }
        //}
    }
    public void OnClickBattleButton()
    {
        scene.Lobby.Instance.ReqQuickBattleJoin(Game.MODE.PVP);
    }
    public void OnClickFriendsButton()
    {
        Debug.Log("OnClickFriendsButton");
    }
    private void OnClickPvPADBonusButton()
    {
        //횟수 없음
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_PVP_ADD_REWARD, -1))
        {
            //광고해서 협동전 가능
            UIManager.Inst.ShowAD("PvPAddReward", Success, null, NotLoad);
            void Success()
            {
                UIManager.Inst.ShowConnectingPanel(true);
                ImplBase.Actor.Parser<AddADPvPAddReward.Ack>(new AddADPvPAddReward.Req(), (ack) =>
                {
                    UIManager.Inst.ShowConnectingPanel(false);
                    if (ack.Result != RESULT.OK)
                    {
                        return;
                    }
                    SetPvPADBonus();
                });
            }
            void NotLoad()
            {
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
            }
        }
    }
    #endregion

    #region Notice

    #endregion

    private void OnClickMarbleArrowButton(bool isLeft)
    {
        int selectDeck = User.Inst.Doc.SelectedDeck;
        if (isLeft) selectDeck--;
        else selectDeck++;

        if (selectDeck < 1)
            selectDeck = 1;
        if (selectDeck > DeckMax)
            selectDeck = DeckMax;
        ImplBase.Actor.Parser<ChangeSelectedDeck.Ack>(new ChangeSelectedDeck.Req() { DeckCount = selectDeck }, (ack) =>
        {
            SetDeck();
        });
    }

    private void OnClickMarbleEditDeckButton()
    {
        UIManager.Inst.OpenPanel(UIPanel.MarbleDeckEditPopup, MarbleDeckEditPopup.EditType.All);
    }

    public override void RefreshEffect(bool isActive)
    {
        base.RefreshEffect(isActive);
        if(marbleDeckGrp) marbleDeckGrp.RefreshEffect(isActive);
        if (marblePassGrp) marblePassGrp.RefreshEffect(isActive);
    }

    public void OnClickMarblePassButton()
    {
        UIManager.Inst.OpenPanel(UIPanel.PassPopup);
    }
}
