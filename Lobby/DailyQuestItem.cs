using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;

public class DailyQuestItem : UIBaseGrp
{
    [SerializeField] Text questTitleText;
    [SerializeField] Image qusetIconBGImage;
    [SerializeField] Image qusetIconProgressImage;
    [SerializeField] Image qusetIconProgress2Image;
    [SerializeField] Text questCountText;
    [Header("Reward")]
    [SerializeField] GameObject rewardItemObject;
    [SerializeField] Transform rewardContent;
    [Header("Button")]
    [SerializeField] Button refreshButton;
    [SerializeField] Button rewardGetButton;
    [SerializeField] GameObject rewardGetOn;
    [SerializeField] GameObject rewardGetOff;
    [Header("RewardGet")]
    [SerializeField] GameObject rewardGetObjet;
    [Header("Lock")]
    [SerializeField] GameObject lockObject;
    [SerializeField] Text lockText;

    int currentQuestGroup;
    List<RewardItem> rewards;
    protected override void InitData()
    {
        base.InitData();
        rewards = new List<RewardItem>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();

        refreshButton.onClick.AddListener(OnClickRefreshQuest);
        rewardGetButton.onClick.AddListener(OnClickGetRewardButton);
    }

    public void Init(int questGroupID)
    {
        try
        { 
            base.Init();
        
            currentQuestGroup = questGroupID;

            lockObject.SetActive(true);
            if (User.Inst.Doc.Quest.Items.ContainsKey(questGroupID) == false)
            {
                SetQuestLockInfo(questGroupID);
                return;
            }
        
            int questID = User.Inst.Doc.Quest.Items[questGroupID].Index;
            if(User.Inst.TBL.Quest.ContainsKey(questID) == false)
            {
                return;
            }

            lockObject.SetActive(false);
            SetQuestInfo();
            SetQuestReward();
            QuestComplete();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }

    }
    private void SetQuestLockInfo(int questGroupID)
    {
        try
        {
            var questGroupTableInfo = User.Inst.TBL.QuestGroup[questGroupID];

            lockText.text = string.Format(UIUtil.GetText("UI_Common_NeedAccountLv"), questGroupTableInfo.NeedAccountLv);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SetQuestInfo()
    {
        try
        { 
            var questItem = User.Inst.Doc.Quest.Items[currentQuestGroup];
            int questID = questItem.Index;
            var questInfo = User.Inst.TBL.Quest[questID];
            var questLangInfo = User.Inst.Langs.Quest[questID];

            questTitleText.text = string.Format(questLangInfo.Title, questInfo.QuestValue);

            UIManager.Inst.SetSprite(qusetIconBGImage, UIManager.AtlasName.UILobby, questInfo.Icon);
            UIManager.Inst.SetSprite(qusetIconProgressImage, UIManager.AtlasName.UILobby, questInfo.Icon);
            UIManager.Inst.SetSprite(qusetIconProgress2Image, UIManager.AtlasName.UILobby, questInfo.Icon+"2");

            int userQuestCount = questItem.Count;
            int questMaxCount = questInfo.QuestValue;

            questCountText.text = string.Format("{0}/{1}", userQuestCount, questMaxCount);

            float percent = (float)userQuestCount / (float)questMaxCount;
            qusetIconProgressImage.fillAmount = percent;
            qusetIconProgress2Image.fillAmount = percent;

            if(userQuestCount >= questMaxCount)
            {
                rewardGetOn.SetActive(true);
                rewardGetOff.SetActive(false);
                rewardGetButton.interactable = true;
            }
            else
            {
                rewardGetOn.SetActive(false);
                rewardGetOff.SetActive(true);
                rewardGetButton.interactable = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SetQuestReward()
    {
        try
        { 
            int questId = User.Inst.Doc.Quest.Items[currentQuestGroup].Index;
            var questTableInfo = User.Inst.TBL.Quest[questId];

            for(int i = 0; i < rewards.Count; i++)
            {
                rewards[i].gameObject.SetActive(false);
            }

            for(int i = 0; i < questTableInfo.RewardType.Count; i++)
            {
                if(rewards.Count <= i)
                {
                    var go = GameObject.Instantiate(rewardItemObject, rewardContent);
                    var rewardItem = go.GetComponent<RewardItem>();
                    rewards.Add(rewardItem);
                }
                var rewardType = UIUtil.GetRewardType(questTableInfo.RewardType[i]);
                int getRewardCount = UIUtil.GetRewardCount(rewardType, questTableInfo.RewardCount[i]);
                string rewardCount = string.Format("{0}", getRewardCount);
                rewards[i].SetItem(rewardType, rewardCount, User.Inst.Doc.Quest.Items[currentQuestGroup].CompleteRewardValue[i]);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void QuestComplete()
    {
        try
        {
            bool isComplete = User.Inst.Doc.Quest.Items[currentQuestGroup].GetReward;
            rewardGetObjet.SetActive(isComplete);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void OnClickRefreshQuest()
    {
        try
        { 
            if (User.Inst.Doc.Quest.Items[currentQuestGroup].GetReward)
                return;
            int fressRefreshCount = (int)User.Inst.TBL.Const["CONST_QUEST_RENEW_FREE_COUNT"].Value;
            var questGroup = User.Inst.Doc.Quest.Items[currentQuestGroup];
            if (questGroup.RefreshCount >= fressRefreshCount)
            {
                //첫갱신이 아니라면 구매해야한다
                //int refreshPriceCount = (int)User.Inst.TBL.Const["CONST_QUEST_RENEW_DIAMOND_PRICE"].Value * (-1);
                //bool ableRefresh = Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_FREE_DIAMOND, refreshPriceCount);
                //if (ableRefresh == false)
                //{
                //    UIManager.Inst.ShowNotEnoughAssetPopup();
                //    return;
                //}
                UIManager.Inst.ShowBuyDescPopup(UIUtil.GetText("UI_Quest_001"), UIUtil.GetText("UI_Quest_002"), ASSETS.ASSET_FREE_DIAMOND, (int)User.Inst.TBL.Const["CONST_QUEST_RENEW_DIAMOND_PRICE"].Value, RefreshQuest);
            }
            else
                RefreshQuest();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
        void RefreshQuest()
        {
            try
            { 
                ImplBase.Actor.Parser<QuestRefresh.Ack>(new QuestRefresh.Req() { QuestGroupID = currentQuestGroup }, (ack) =>
                 {
                     if (ack.Result == RESULT.NOT_ENOUGH_ASSET)
                         UIManager.Inst.ShowNotEnoughAssetPopup();
                     if (ack.Result != RESULT.OK)
                     {
                         return;
                     }
                     int questID = User.Inst.Doc.Quest.Items[currentQuestGroup].Index;
                     if (User.Inst.TBL.Quest.ContainsKey(questID) == false)
                     {
                         return;
                     }

                     lockObject.SetActive(false);
                     SetQuestInfo();
                     SetQuestReward();
                     QuestComplete();
                     UIManager.Inst.RefreshLobbyAsset();
                 });
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
    private void OnClickGetRewardButton()
    {
        try
        {
            if (User.Inst.Doc.Quest.Items[currentQuestGroup].GetReward)
                return;
            var questItem = User.Inst.Doc.Quest.Items[currentQuestGroup];
            int questID = questItem.Index;
            var questInfo = User.Inst.TBL.Quest[questID];
            if (questItem.Count < questInfo.QuestValue)
                return;
            UIManager.Inst.ShowConnectingPanel(true);
            ImplBase.Actor.Parser<GetQuestReward.Ack>(new GetQuestReward.Req() { QuestGroupID = currentQuestGroup }, (ack) =>
            {
                try
                {
                    UIManager.Inst.ShowConnectingPanel(false);
                    if (ack.Result != RESULT.OK)
                    {
                        return;
                    }
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
                    QuestComplete();
                    UIManager.Inst.RefreshLobbyAsset();
                    var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
                    var battlePanel = (BattlePanel)lobbyPanel.GetBottomMenu(LobbyBottomMenu.Battle);
                    battlePanel.SetQuestNoti();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
