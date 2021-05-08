using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
using Net.Impl;
using System.Text;
using Platform;

public class NickNamePopup : UIBasePanel
{
    [Header("Asset")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text GemText;

    [SerializeField] InputField nickNameInput;
    [SerializeField] Image priceIconImage;
    [SerializeField] Text priceText;
    [SerializeField] Button changeButton;
    [SerializeField] Button freeChangeButton;
    [SerializeField] GameObject guestNoticeGrp;

    private string prevNickNameInput = string.Empty;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        changeButton.onClick.AddListener(OnClickNickChangeButton);
        freeChangeButton.onClick.AddListener(OnClickNickChangeButton);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        nickNameInput.text = string.Empty;
        prevNickNameInput = string.Empty;
        SetButton();

        if (UIUtil.IsGuestUser())
        {
            Debug.Log("##################### Guest");
            guestNoticeGrp.SetActive(true);
        }
        else
        {
            Debug.Log("##################### Not Guest");
            guestNoticeGrp.SetActive(false);
        }
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
    private void SetButton()
    {
        freeChangeButton.gameObject.SetActive(false);
        changeButton.gameObject.SetActive(false);
        if (User.Inst.Langs.UI_Text.ContainsKey(User.Inst.Doc.PlayerNick))
        {
            freeChangeButton.gameObject.SetActive(true);
        }
        else
        {
            changeButton.gameObject.SetActive(true);
            int shopId = (int)UIUtil.GetConstValue("CONST_NICK_CHANGE_SHOPID");
            var shopItem = User.Inst.TBL.Shop[shopId];
            var priceType = UIUtil.GetAssetsType(shopItem.PriceType);
            UIUtil.SetAssetIcon(priceType, priceIconImage);
            priceText.text = shopItem.Price.ToString();
        }
        
    }

    public void CheckNameInputChanged()
    {
        nickNameInput.text = nickNameInput.text.Replace(" ", "");        
        if (UIUtil.CheckSpecialText(nickNameInput.text))
        {
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_NickName_000"), UIUtil.GetText("UI_NickName_004"), UIUtil.GetText("UI_Common_Ok"));
            nickNameInput.text = prevNickNameInput;
            return;
        }

        prevNickNameInput = nickNameInput.text;
    }
    private string prevNick;
    public void OnClickNickChangeButton()
    {
        prevNick = User.Inst.Doc.PlayerNick;
        string changeName = nickNameInput.text.Replace(" ", "");
        if (User.Inst.Doc.PlayerNick.Equals(changeName))
        {
            // 지금 닉네임과 같을 때
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_NickName_000"), UIUtil.GetText("UI_NickName_006"), UIUtil.GetText("UI_Common_Ok"));
            return;
        }

        if (User.Inst.Langs.UI_Text.ContainsKey(User.Inst.Doc.PlayerNick) == false)
        {
            int shopId = (int)UIUtil.GetConstValue("CONST_NICK_CHANGE_SHOPID");
            var shopItem = User.Inst.TBL.Shop[shopId];
            var priceType = UIUtil.GetAssetsType(shopItem.PriceType);
            if (BaseImpl.Inst.CheckAsset(User.Inst, priceType, shopItem.Price * (-1)) == false)
            {
                UIManager.Inst.ShowNotEnoughAssetPopup();
                return;
            }
        }
        
        if (changeName.Length < 2 || changeName.Length > 10)
        {
            //짧거나 길때
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_NickName_000"), UIUtil.GetText("UI_NickName_005"), UIUtil.GetText("UI_Common_Ok"));
            return;
        }
        
        if (UIUtil.CheckSpecialText(changeName))
        {
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_NickName_000"), UIUtil.GetText("UI_NickName_004"), UIUtil.GetText("UI_Common_Ok"));
            return;
        }

        string langKey = AppClient.Inst.Language;
        langKey = langKey.ToLower();
        string contry = "kr";

#if !UNITY_EDITOR
        contry = hive.Configuration.getHiveCountry();
#endif

        changeName = BannedWordManager.Instance.ApplyBannedWordsFilter(changeName, "****", langKey, contry, "name");
        Debug.Log(log.ToString());
        if(string.Equals(changeName, nickNameInput.text) == false)
        {
            //금칙어사용시
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_NickName_000"), UIUtil.GetText("UI_NickName_004"), UIUtil.GetText("UI_Common_Ok"));
            return;
        }

        UIManager.Inst.ShowConnectingPanel(true);
        ImplBase.Actor.Parser<UserNameModify.Ack>(new UserNameModify.Req() { Name = changeName }, (ack) =>
        {
            UIManager.Inst.ShowConnectingPanel(false);

            if (ack.Result == RESULT.NOT_ENOUGH_ASSET)
                UIManager.Inst.ShowNotEnoughAssetPopup();
            if (ack.Result != RESULT.OK)
            {
                //UIManager.Inst.ShowConfirm1BtnPopup()
                return;
            }

            
            var userInfoPopup = UIManager.Inst.GetActivePanel<UserInfoPopup>(UIPanel.UserInfoPopup);
            if (userInfoPopup != null)
                userInfoPopup.SetUserBaseInfo();
            
            var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
            if (lobbyPanel != null)
                lobbyPanel.SetUserInfo();

            UIManager.Inst.RefreshLobbyAsset();

            if (User.Inst.Langs.UI_Text.ContainsKey(prevNick) && User.Inst.Doc.GetWelcomeGift == false)
            {
                int welcomeBoxID = (int)UIUtil.GetConstValue("CONST_TUTORIAL_WELCOME_GIFT");
                UIManager.Inst.ShowBoxRewardPopup(welcomeBoxID,buttonAction:GetWelcomeGift,closeAction: GetWelcomeGift);
            }
            else
                ClosePanel();
        });
    }
    private void GetWelcomeGift()
    {
        UIManager.Inst.ShowConnectingPanel(true);
        ImplBase.Actor.Parser<GetWelcomeGift.Ack>(new GetWelcomeGift.Req(), (ack) =>
        {
            UIManager.Inst.ShowConnectingPanel(false);
            int welcomeBoxID = (int)UIUtil.GetConstValue("CONST_TUTORIAL_WELCOME_GIFT");
            var boxInfo = User.Inst.TBL.Box[welcomeBoxID];
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
                    UIManager.PanelEndAction endResultPopup = ShowBanner;
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards, endResultPopup);
                }
            }
            else
            {
                UIManager.PanelEndAction endResultPopup = ShowBanner;
                UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards, endResultPopup);
            }
            //UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
            UIManager.Inst.RefreshLobbyAsset();
            ClosePanel();
        });
    }

    public override void ClosePanel()
    {
        if (UIUtil.IsFirstBattle && User.Inst.Doc.PlayerNick.Equals("__NEW_NICK"))
        {
            ShowBanner();
        }

        base.ClosePanel();
    }
    private void ShowBanner()
    {
        if (UIUtil.IsFirstBattle)
        {
            UIUtil.IsFirstBattle = false;
            UIUtil.showBanner = false;
            var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
            if (lobbyPanel != null)
                lobbyPanel.ShowBanner();
        }
    }

    private SERVERMODE serverMode = SERVERMODE.DEV;
    private string appId = "com.olarksil.rmd.normal.freefull.google.global.android.common";
    private string game = "RandomMarbleDefense";
    private float timeout = 3.0f;

    private StringBuilder log = new StringBuilder();


    void Start()
    {
        Application.logMessageReceived += (string condition, string stackTrace, LogType type) => { log.Insert(0, string.Format("[{0}]{1}\n{2}\n", type, condition, stackTrace)); };

        string savePath = Application.persistentDataPath + "/Test"; //bannedWordFile savePath
        string filename = "/output.txt"; //bannedWordFile name (path = savePath + filename)

        BannedWordManager.Instance.Initialize((int)serverMode, appId, game, timeout, savePath, filename, Debug.unityLogger);
        BannedWordManager.Instance.AttachEventHandler(UpdateProgress);
        BannedWordManager.Instance.StartBannedWordProcess();
    }

    static public void UpdateProgress(PROCESS_STATE state)
    {
        switch (state)
        {
            case PROCESS_STATE.INITIALIZED:
                Debug.Log("###### UdateProgress : INITIALIZED");
                break;
            case PROCESS_STATE.REQUESTING:
                Debug.Log("###### UdateProgress : REQUESTING");
                break;
        }
    }

    

}
