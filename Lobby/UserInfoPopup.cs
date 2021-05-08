using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net.Impl;
using Net.Api;
using Net;
public class UserInfoPopup : UIBasePanel
{
    [Header("Asset")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text GemText;
    [Header("UserBaseInfo")]
    [SerializeField] Text userNameText;
    [SerializeField] Button userNameChangeButton;

    [Header("UserLevelInfo")]
    [SerializeField] Image userGradeImage;
    [SerializeField] Text userLevelText;
    [SerializeField] Text userExpText;

    [Header("LevelReward")]
    [SerializeField] GameObject infoItemObject;
    [SerializeField] Transform infoItemContent;
    [SerializeField] CompScrollSlider scrollSlider;

    [Header("BattleRecord")]
    [SerializeField] Text battleCountText;
    [SerializeField] Text battleWinCountText;
    [SerializeField] Text battelWinRateText;
    [SerializeField] Button battleWinRateResetButton;

    [Header("OtherRecord")]
    [SerializeField] Text maxWaveText;
    [SerializeField] Text maxTrouphyText;
    [SerializeField] Text marbleCountText;

    List<UserInfoItem> userLevelRewardItems;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        userLevelRewardItems = new List<UserInfoItem>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        userNameChangeButton.onClick.AddListener(OnClickChangeNickName);
        battleWinRateResetButton.onClick.AddListener(OnClickResetWinRate);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        SetUserBaseInfo();
        SetUserLevelInfo();
        SetLevelReward();
        SetBattleRecord();
        SetOtherRecord();
    }
    public override void UpdateAsset()
    {
        if (GoldText)
        {
            long goldCount = BaseImpl.Inst.GetAsset(User.Inst, Net.ASSETS.ASSET_GOLD);
            GoldText.text = goldCount.ToString("n0");
        }
        if (GemText)
        {
            long diamondCount = BaseImpl.Inst.GetAsset(User.Inst, Net.ASSETS.ASSET_DIAMOND);
            GemText.text = diamondCount.ToString("n0");
        }
    }
    public void SetUserBaseInfo()
    {
        if (userNameText) userNameText.text = UIUtil.GetUserName(User.Inst.Doc.PlayerNick);
    }
    private void SetUserLevelInfo()
    {
        if (userGradeImage) UIUtil.SetLevelIcon(User.Inst.Doc.Level, userGradeImage);
        if (userLevelText) userLevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), User.Inst.Doc.Level);
        if (User.Inst.TBL.Account_EXP.ContainsKey(User.Inst.Doc.Level))
        {
            var levelTabelInfo = User.Inst.TBL.Account_EXP[User.Inst.Doc.Level];
            int preExp = 0;
            if(User.Inst.TBL.Account_EXP.ContainsKey(User.Inst.Doc.Level - 1))
            {
                var preLevelTableInfo = User.Inst.TBL.Account_EXP[User.Inst.Doc.Level - 1];
                preExp = preLevelTableInfo.AccumEXP;
            }
            string exp = string.Format("{0}/{1}", User.Inst.Doc.Exp - preExp, levelTabelInfo.NeedEXP);
            if (userExpText) userExpText.text = exp;
        }
        else
        {
            if (userExpText) userExpText.text = "";
        }
    }
    private void SetLevelReward()
    {
        int itemCount = 0;
        RectTransform target = null;
        foreach(var item in User.Inst.TBL.Account_EXP.Values)
        {
            if (userLevelRewardItems.Count <= itemCount)
            {
                var go = UIManager.Inst.CreateObject(infoItemObject, infoItemContent);
                var infoItem = go.GetComponent<UserInfoItem>();
                infoItem.Init();
                userLevelRewardItems.Add(infoItem);
            }
            userLevelRewardItems[itemCount].SetLevel(item.Index, OnClickItemGetReward);
            
            if (itemCount == 0)
                target = userLevelRewardItems[itemCount].gameObject.GetComponent<RectTransform>();
            if (User.Inst.Doc.RewardLevel == item.Index)
                target = userLevelRewardItems[itemCount].gameObject.GetComponent<RectTransform>();
            itemCount++;
        }
        scrollSlider.TargetRt = target;
        scrollSlider.MoveToTarget();
    }

    private void SetBattleRecord()
    {
        battleCountText.text = User.Inst.Doc.PvP.Record.Join.ToString("n0");
        battleWinCountText.text = User.Inst.Doc.PvP.Record.Win.ToString("n0");
        double join = User.Inst.Doc.PvP.Record.Join;
        double win = User.Inst.Doc.PvP.Record.Win;
        double rate = 0;
        if(join != 0)
            rate = win / join;
        rate *= 100d;

        battelWinRateText.text = string.Format("{0:0}%",rate);

        battleWinRateResetButton.gameObject.SetActive(join > 0);
    }

    private void SetOtherRecord()
    {
        maxWaveText.text = User.Inst.Doc.Team.Record.MaxRound.ToString("n0");
        maxTrouphyText.text = User.Inst.Doc.PvP.Record.MaxTrophy.ToString("n0");

        //마블정보
        int userMarbleCount = User.Inst.Doc.MarbleInven.Count;
        int marbleMaxCount = 0;
        foreach(var item in User.Inst.TBL.Marble.Values)
        {
            if (item.Visiable == 0)
                continue;
            marbleMaxCount++;
        }

        marbleCountText.text = string.Format("{0}/{1}", userMarbleCount, marbleMaxCount);
    }
    private void OnClickItemGetReward(int level)
    {
        UIManager.Inst.ShowConnectingPanel(true);
        ImplBase.Actor.Parser<GetLevelReward.Ack>(new GetLevelReward.Req() { Level = level }, (ack) =>
         {
             UIManager.Inst.ShowConnectingPanel(false);
             if (ack.Result != RESULT.OK)
             {
                 return;
             }

             var levelTabelInfo = User.Inst.TBL.Account_EXP[level];
             var rewardType = UIUtil.GetRewardType(levelTabelInfo.RewardType);
             if (rewardType == REWARD_TYPE.REWARD_BOX
             || rewardType == REWARD_TYPE.REWARD_BOX_EXPAND)
             {
                 int boxID = levelTabelInfo.RewardValue;
                 var boxInfo = User.Inst.TBL.Box[boxID];
                 if (string.IsNullOrEmpty(boxInfo.Open_Effect) == false)
                 {
                     //병오픈 연출보여주기
                     REWARD_TYPE marbleRewardType = REWARD_TYPE.REWARD_MARBLE;
                     foreach (var item in ack.Rewards)
                     {
                         if (item.Type == (int)REWARD_TYPE.REWARD_MARBLE
                         || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_LEGEND
                         || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_MYTH
                            || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_NORMAL
                            || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_RARE
                            || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
                         {
                             if (User.Inst.TBL.Marble.ContainsKey(item.Value))
                             {
                                 var rewardTableInfo = User.Inst.TBL.Marble[item.Value];
                                 if (marbleRewardType != REWARD_TYPE.REWARD_MARBLE_MYTH && rewardTableInfo.Rarity == (int)MarbleRarity.Legendary)
                                     marbleRewardType = REWARD_TYPE.REWARD_MARBLE_LEGEND;
                                 else if (rewardTableInfo.Rarity == (int)MarbleRarity.Chronicle)
                                     marbleRewardType = REWARD_TYPE.REWARD_MARBLE_MYTH;
                             }
                         }
                     }

                     UIManager.PanelEndAction end = EndAction;
                     UIManager.Inst.OpenPanel(UIPanel.BoxOpenPopup, boxInfo.Open_Effect, end, marbleRewardType);
                     void EndAction()
                     {
                         UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
                     }
                 }
                 else
                 {
                     UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
                 }
             }
             else
             {
                 UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
             }

             //UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);

             for (int i = 0; i < userLevelRewardItems.Count; i++)
             {
                 userLevelRewardItems[i].Refresh();
             }

             var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
             lobbyPanel.SetUserInfo();
             UIManager.Inst.RefreshLobbyAsset();
         });
    }
    private void OnClickChangeNickName()
    {
        UIManager.Inst.OpenPanel(UIPanel.NickNamePopup);
    }
    private void OnClickResetWinRate()
    {
        int shopId = (int)UIUtil.GetConstValue("CONST_POV_RESET_SHOPID");
        if (User.Inst.TBL.Shop.ContainsKey(shopId) == false)
            return;
        var tableInfo = User.Inst.TBL.Shop[shopId];
        ASSETS priceType = UIUtil.GetAssetsType(tableInfo.PriceType);
        UIManager.Inst.ShowBuyDescPopup(UIUtil.GetText("UI_Account_WinRate_Reset"), UIUtil.GetText("UI_Account_WinRate_Reset_Desc"), priceType, tableInfo.Price, buttonAction: ResetWinRate);
    }
    private void ResetWinRate()
    {
        UIManager.Inst.ShowConnectingPanel(true);
        ImplBase.Actor.Parser<ResetWinRate.Ack>(new ResetWinRate.Req(), (ack) =>
        {
            UIManager.Inst.ShowConnectingPanel(false);

            if (ack.Result == RESULT.NOT_ENOUGH_ASSET)
                UIManager.Inst.ShowNotEnoughAssetPopup();
            if (ack.Result != RESULT.OK)
            {
                return;
            }
            SetBattleRecord();
            UIManager.Inst.RefreshLobbyAsset();
        });
    }
}
