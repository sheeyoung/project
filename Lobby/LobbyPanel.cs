using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net.Impl;
using DG.Tweening;
using System;
using Libs.Utils;
using Net.Api;
using Net;
using Platform;
using SwipeMenu;

[System.Serializable]
public class BottomMenu
{
    public LobbyBottomMenu type;
    public UIBaseGrp menuGrp;
    public GameObject menuObject;
    public GameObject on;
    public GameObject off;
    public GameObject alarm;
}

public class LobbyPanel : UIBasePanel
{
    //[SerializeField] private GameObject center;
    [Header("Asset")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text GemText;
    //[SerializeField] private Button goldButton;
    //[SerializeField] private Button gemButton;
    [Header("UserInfo")]
    [SerializeField] private Image UserLevelIconImage;
    [SerializeField] private Text LevelText;
    [SerializeField] private Text NameText;
    [SerializeField] private RectTransform ExpRT;
    [SerializeField] private Button userInfoButton;
    [SerializeField] private GameObject userAlarm;
    [Header("SubMenu")]
    [SerializeField] private GameObject SubMenuPopup;
    [SerializeField] private GameObject SubMenuPopup_Alarm;
    [SerializeField] private GameObject SubMenu_InboxAlarm;
    [SerializeField] private GameObject SubMenuPopupColider;
    [SerializeField] private GameObject SubMenuPopupColider2;
    [Header("BottomMenu")]
    [SerializeField] public List<BottomMenu> BottomMenuPanel = new List<BottomMenu>();
    [Header("MoveMenu")]
    [SerializeField] GameObject center;
    [Header("AssetGrp")]
    [SerializeField] List<GameObject> onOffGrps;
    [SerializeField] GameObject tweenAssetGrp;
    [SerializeField] Text freeGoldText;
    [SerializeField] Text paidGoldText;
    [SerializeField] Button goldButton;
    [SerializeField] Text freeGemText;
    [SerializeField] Text paidGemText;
    [SerializeField] Button gemButton;


    private LobbyBottomMenu currentMenu;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);

        Menu.instance.menuItems = new MenuItem[3];// BottomMenuPanel.Count];
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuObject == null)
                continue;
            var go = UIManager.Inst.CreateObject(BottomMenuPanel[i].menuObject, center.transform);
            BottomMenuPanel[i].menuGrp = go.GetComponent<UIBaseGrp>();
            Menu.instance.menuItems[i] = go.GetComponent<MenuItem>();
            Menu.instance.menuItems[i].lobbyBottomMenu = BottomMenuPanel[i].type;
        }
        currentMenu = LobbyBottomMenu.Battle;
        //Menu.instance.startingMenuItem = (int)currentMenu;
        Menu.instance.endUpdateMenu = UpdateMenu;
        Menu.instance.Init();
        AppClient.Inst.OnInbox += RefreshInboxAlarm;

        ShowBanner();

        ShowInGameRewardEffect();
        StartCoroutine(SetPush());
    }
    public void ShowBanner()
    {
        if (UIUtil.showBanner == false)
        {
#if UNITY_EDITOR || DEV
            Debug.Log("################# ShowBanner");
#endif
            UIUtil.showBanner = true;
#if !UNITY_EDITOR
            hive.Promotion.showPromotion(hive.PromotionType.BANNER, true, onBannerCB);
#endif
        }
    }

    protected override void SetEvent()
    {
        base.SetEvent();
        userInfoButton.onClick.AddListener(OnClickUserInfoButton);
        if(goldButton) goldButton.onClick.AddListener(() => { OnClickAssetButton(ASSETS.ASSET_FREE_GOLD); });
        if(gemButton) gemButton.onClick.AddListener(() => { OnClickAssetButton(ASSETS.ASSET_FREE_DIAMOND); });
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        SetUserInfo();
        SetTab(currentMenu);
        SubMenuPopup.SetActive(false);
        SubMenuPopupColider.SetActive(false);
        SubMenuPopupColider2.SetActive(false);
        InitQuest();

        SubMenu_InboxAlarm.SetActive(false);
        SubMenuPopup_Alarm.SetActive(false);

        for (int i = 0; i < onOffGrps.Count; i++)
            onOffGrps[i].SetActive(false);

        AppImpl.Parse<InboxGet.Ack>(new InboxGet.Req(), (ack) =>
        {
            Debug.Log(JsonLib.Encode(ack));
            if (ack.IsSuccess == false)
                return;
            int inBoxCount = 0;
            for (int i = 0; i < ack.List.Count; i++)
            {
                if (ack.List[i].State == INBOX_STATE.WAIT)
                {
                    inBoxCount++;
                }
            }
            RefreshInboxAlarm(inBoxCount);
        });


        CheckNotGainInAppItem();

        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuGrp != null)
            {
                BottomMenuPanel[i].menuGrp.Init();
                BottomMenuPanel[i].menuGrp.gameObject.SetActive(currentMenu == BottomMenuPanel[i].type);
            }
        }

        SetMarbleTabAlarm();
    }
    private void CheckNotGainInAppItem()
    {
        if (IAP.Inst.ReceiptList.Count > 0)
        {
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Shop_002"), UIUtil.GetText("UI_Common_Ok"));
        }
    }
    public void SetUserInfo()
    {
        if (UserLevelIconImage) UIUtil.SetLevelIcon(User.Inst.Doc.Level, UserLevelIconImage);
        if (LevelText) LevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), User.Inst.Doc.Level);
        if (NameText) NameText.text = UIUtil.GetUserName(User.Inst.Doc.PlayerNick);
        if (ExpRT)
        {
            Vector3 sca = Vector3.one;
            if (User.Inst.Doc.Level > 1)
            {
                sca.x = (float)(User.Inst.Doc.Exp - Sheet.TBL.Account_EXP[User.Inst.Doc.Level - 1].AccumEXP) / Sheet.TBL.Account_EXP[User.Inst.Doc.Level].NeedEXP;
            }
            else
            {
                sca.x = (float)(User.Inst.Doc.Exp) / Sheet.TBL.Account_EXP[User.Inst.Doc.Level].NeedEXP;
            }
            ExpRT.localScale = sca;
        }
        if(userAlarm)
        {
            bool isActive = User.Inst.Doc.Level > User.Inst.Doc.RewardLevel;
            userAlarm.SetActive(isActive);
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


        //gem
        long paidDiaCount = BaseImpl.Inst.GetOriginalAsset(User.Inst, Net.ASSETS.ASSET_DIAMOND);
        long freeDiaCount = BaseImpl.Inst.GetOriginalAsset(User.Inst, Net.ASSETS.ASSET_FREE_DIAMOND);
        if (freeGemText)
        {
            freeGemText.text = freeDiaCount.ToString("n0");
        }
        if(paidGemText)
        {
            paidGemText.text = paidDiaCount.ToString("n0");
        }

        //gold
        long paidGoldCount = BaseImpl.Inst.GetOriginalAsset(User.Inst, Net.ASSETS.ASSET_GOLD);
        long freeGoldCount = BaseImpl.Inst.GetOriginalAsset(User.Inst, Net.ASSETS.ASSET_FREE_GOLD);
        if (freeGoldText)
        {
            freeGoldText.text = freeGoldCount.ToString("n0");
        }
        if (paidGoldText)
        {
            paidGoldText.text = paidGoldCount.ToString("n0");
        }
    }
    private void SetTab(LobbyBottomMenu menu)
    {
        currentMenu = menu;
        if (currentMenu == LobbyBottomMenu.Shop)
        {
            ShopReset();
            return;
        }
        
        OpendCurrentMenu();
    }
    private void OpendCurrentMenu()
    {
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuGrp != null)
            {
                BottomMenuPanel[i].menuGrp.gameObject.SetActive(currentMenu == BottomMenuPanel[i].type);
                if (currentMenu == BottomMenuPanel[i].type)
                    BottomMenuPanel[i].menuGrp.Init();
            }
            BottomMenuPanel[i].on.SetActive(currentMenu == BottomMenuPanel[i].type);
            BottomMenuPanel[i].off.SetActive(currentMenu != BottomMenuPanel[i].type);
        }
    }

    public void onPromotionViewCB(hive.ResultAPI result, hive.PromotionEventType promotionEventType)
    {

        if (result.isSuccess())
        {
#if UNITY_EDITOR || DEV
            // API 호출 성공
            Debug.Log("################## onPromotionViewCB Success");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEV
            Debug.Log("################## onPromotionViewCB fail : " + result.errorCode.ToString());
            Debug.Log(result.errorMessage);
#endif
        }
    }

    public void onBannerCB(hive.ResultAPI result, hive.PromotionEventType promotionEventType)
    {

        if (result.isSuccess())
        {
#if UNITY_EDITOR || DEV
            // API 호출 onBannerCB
            Debug.Log("################## onBannerCB Success");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEV
            Debug.Log("################## onBannerCB fail : " + result.errorCode.ToString());
            Debug.Log(result.errorMessage);
#endif
        }
    }

    public void OnClickSubMenu()
    {
        bool isshow = !SubMenuPopup.activeSelf;
        SubMenuPopup.SetActive(isshow);
        SubMenuPopupColider.SetActive(isshow);
        SubMenuPopupColider2.SetActive(isshow);
        if (isshow)
        {
            DOTween.PlayForward(SubMenuPopup, "OpenSubMenu");
        }
        else
        {
            DOTween.PlayBackwards(SubMenuPopup, "OpenSubMenu");
        }
    }

    public void OnClickNoticeButton()
    {
#if UNITY_EDITOR || DEV
        Debug.Log("OnClickNoticeButton");
#endif
        hive.Promotion.showPromotion(hive.PromotionType.NEWS, true, onPromotionViewCB);
        OnClickSubMenu();
    }

    public void OnClickBattleRecordButton()
    {
#if UNITY_EDITOR || DEV
        Debug.Log("OnClickBattleRecordButton");
#endif
        UIManager.Inst.OpenPanel(UIPanel.BattleRecordPopup);
        OnClickSubMenu();
    }
    public void OnClickRankingButton()
    {
#if UNITY_EDITOR || DEV
        Debug.Log("OnClickRankingButton");
#endif
        UIManager.Inst.OpenPanel(UIPanel.RankingPopup);
        OnClickSubMenu();
    }
    public void OnClickMailButton()
    {
#if UNITY_EDITOR || DEV
        Debug.Log("OnClickMailButton");
#endif
        UIManager.Inst.OpenPanel(UIPanel.MailPopup);
        OnClickSubMenu();
    }

    public void OnClickSettingButton()
    {
#if UNITY_EDITOR || DEV
        Debug.Log("OnClickSettingButton");
#endif
        UIManager.Inst.OpenPanel(UIPanel.OptionPopup);
        OnClickSubMenu();
    }
    public void OnClickAssetButton(ASSETS asset)
    {
        try
        {
            Debug.Log("OnClickAssetButton");
            var shop = GetGrpObject(LobbyBottomMenu.Shop).GetComponent<ShopPanel>();
            if (asset == ASSETS.ASSET_FREE_DIAMOND)
                shop.SetStartScroll(ShopPanel.ShopType.DIA_SHOP);
            else if (asset == ASSETS.ASSET_FREE_GOLD)
                shop.SetStartScroll(ShopPanel.ShopType.GOLD_SHOP);
            if (currentMenu == LobbyBottomMenu.Shop)
            {
                ShopReset();
            }
            else
            {
                GoToMenu(LobbyBottomMenu.Shop);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    public void OnClickBottomMenu(int index)
    {
        Debug.Log("OnclickBottomMenu : " + ((LobbyBottomMenu)index).ToString());
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            BottomMenuPanel[i].on.SetActive(index == (int)BottomMenuPanel[i].type);
            BottomMenuPanel[i].off.SetActive(index != (int)BottomMenuPanel[i].type);
        }
        GoToMenu((LobbyBottomMenu)index);
    }

    private GameObject GetGrpObject(LobbyBottomMenu tab)
    {
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].type == tab)
                return BottomMenuPanel[i].menuGrp.gameObject;
        }
        return null;
    }

    private void OnClickUserInfoButton()
    {
        UIManager.Inst.OpenPanel(UIPanel.UserInfoPopup);
    }

    public void OnClickAssetButton()
    {
        for (int i = 0; i < onOffGrps.Count; i++)
            onOffGrps[i].SetActive(true);
        DOTween.PlayForward(tweenAssetGrp, "OpenSubMenu");
    }

    public void OnClickAssetColliderButton()
    {
        for (int i = 0; i < onOffGrps.Count; i++)
            onOffGrps[i].SetActive(false);
        DOTween.PlayBackwards(tweenAssetGrp, "OpenSubMenu");
    }

    public void RefreshDeck()
    {
        UpdateAsset();
        for (int i = 0; i < BottomMenuPanel.Count; ++i)
        {
            if(BottomMenuPanel[i].menuGrp != null)
                BottomMenuPanel[i].menuGrp.Refresh();
        }
    }

    

    #region ShopReset
    private void ShopReset()
    {
        bool isReset = false;
        if (User.Inst.Doc.DailyShop == null)
            isReset = true;
        else if (User.Inst.Doc.DailyShop.Count <= 0)
            isReset = true;
        else if (User.Inst.Doc.DailyShop.ContainsKey((int)SHOP_TYPE.DAILY_SPECIAL) == false)
            isReset = true;
        else
        {
            int date = User.Inst.Doc.DailyShop[(int)SHOP_TYPE.DAILY_SPECIAL].OpenDate;
            int now = Convert.ToInt32(TimeLib.Now.ToString("yyMMdd"));

            if (date.Equals(now) == false)
                isReset = true;
        }
        if(isReset == false)
        {
            OpendCurrentMenu();
            return;
        }
        else
        {
            UIManager.Inst.ShowConnectingPanel(true);
            ImplBase.Actor.Parser<ShopReset.Ack>(new ShopReset.Req() { Type = SHOP_TYPE.ALL }, (ack) =>
            {
                Debug.Log(JsonLib.Encode(ack));
                UIManager.Inst.ShowConnectingPanel(false);
                OpendCurrentMenu();
            });
        }
    }
    #endregion

    #region 퀘스트 초기화
    long tomorrowDay = 0;
    private void InitQuest()
    {
        DateTime questDate = TimeLib.ConvertTo(User.Inst.Doc.Quest.QuestDate);
        DateTime nowDate = TimeLib.Now;
        if(nowDate.Year == questDate.Year && nowDate.Month == questDate.Month && nowDate.Day == questDate.Day)
        {
            var tomorrow = nowDate.AddDays(1);
            tomorrow = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
            tomorrowDay = TimeLib.ConvertTo(tomorrow);
            return;
        }
#if UNITY_EDITOR || DEV
        Debug.Log("##################QuestInit");
#endif
        Net.ImplBase.Actor.Parser<Net.Api.QuestInit.Ack>(new Net.Api.QuestInit.Req(), (ack) =>
        {
            Debug.Log(ack);
            var yester = nowDate.AddDays(1);
            yester = new DateTime(yester.Year, yester.Month, yester.Day, 0, 0, 0);
            tomorrowDay = TimeLib.ConvertTo(yester);
        });
    }
    #endregion
    #region 메일함 알람
    public void RefreshInboxAlarm(int Count)
    {
        SubMenu_InboxAlarm.SetActive(Count > 0);
        SubMenuPopup_Alarm.SetActive(Count > 0);
    }
    #endregion

    float playTime = 0f;
    private void Update()
    {
        if (playTime < 1f)
        {
            playTime += Time.deltaTime;
            return;
        }
        playTime = 0f;

        long now = TimeLib.Seconds;
        if (now >= tomorrowDay)
            InitQuest();
    }


    public void GoToMenu(LobbyBottomMenu menu)
    {
        Debug.Log("GoToMenu : " + menu.ToString());
        MenuItem item = null;
        LobbyBottomMenu type = LobbyBottomMenu.Clan;
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuGrp != null)
            {
                if (menu == BottomMenuPanel[i].type)
                {
                    item = BottomMenuPanel[i].menuGrp.gameObject.GetComponent<MenuItem>();
                    type = BottomMenuPanel[i].type;
                    break;
                }
            }
        }
        if (item == null)
            return;
        Debug.Log("GoToMenu : " + type.ToString());
        if (Menu.instance.MenuCentred(item))
        {
            SetTab(currentMenu);
            return;
        }

        Menu.instance.AnimateToTargetItem(item);
    }
    public override void Refresh()
    {
        UpdateAsset();
        SetUserInfo();
        SetTab(currentMenu);
    }


    public UIBaseGrp GetBottomMenu(LobbyBottomMenu menu)
    {
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuGrp != null)
            {
                if (menu == BottomMenuPanel[i].type)
                    return BottomMenuPanel[i].menuGrp;
            }
        }
        return null;
    }

    #region 재화 획득 이펙트
    public void ShowInGameRewardEffect()
    {
        if(UIUtil.inGameGetRewards.Count > 0)
        {
            for(int i = 0; i < UIUtil.inGameGetRewards.Count; i++)
            {
                PlayAssetGetEffect(UIUtil.inGameGetRewards[i]);
            }
        }
        UIUtil.ClearShopInGameReward();
        SoundManager.Inst.PlayUIEffect(20);
    }
    public void PlayAssetGetEffect(REWARD_TYPE rewardType)
    {
        if(rewardEffects.ContainsKey(rewardType) == false)
        {
            string rewardEffectString = "";
            switch (rewardType)
            {
                case REWARD_TYPE.REWARD_BOX:
                case REWARD_TYPE.REWARD_BOX_EXPAND:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_Box_p1";
                    break;
                case REWARD_TYPE.ASSET_DIAMOND:
                case REWARD_TYPE.ASSET_FREE_DIAMOND:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_Dia_p1";
                    break;
                case REWARD_TYPE.ASSET_FREE_GOLD:
                case REWARD_TYPE.ASSET_GOLD:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_Gold_p1";
                    break;
                case REWARD_TYPE.REWARD_MATCH_EXPL:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_Medal_p1";
                    break;
                case REWARD_TYPE.REWARD_PVP_SCORE:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_Trophy_p1";
                    break;
                case REWARD_TYPE.REWARD_ACCOUNT_EXP:
                    rewardEffectString = "Prefab/Effect/UI/eff_UI_GetAssets_XP_p1";
                    break;
            }
            if (string.IsNullOrEmpty(rewardEffectString))
                return;
            var go= UIResourceManager.Inst.Load<GameObject>(rewardEffectString);
            GameObject ins = GameObject.Instantiate(go) as GameObject;
            var assetEffect = ins.GetComponent<AssetEffect>();

            ins.transform.SetParent(UIManager.Inst.uiTopCanvas.transform);
            var rectTranform = ins.GetComponent<Transform>();
            rectTranform.localScale = new Vector3(100f, 100f, 100f);

            assetEffect.Init(rewardType, Vector3.zero);
            rewardEffects.Add(rewardType, assetEffect);
        }
        Vector3 desPosition = Vector3.zero;
        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_BOX:
            case REWARD_TYPE.REWARD_BOX_EXPAND:
                desPosition = userInfoButton.transform.position;
                break;
            case REWARD_TYPE.ASSET_DIAMOND:
            case REWARD_TYPE.ASSET_FREE_DIAMOND:
                desPosition = GemText.transform.position;
                break;
            case REWARD_TYPE.ASSET_FREE_GOLD:
            case REWARD_TYPE.ASSET_GOLD:
                desPosition = GoldText.transform.position;
                break;
            case REWARD_TYPE.REWARD_MATCH_EXPL:
                desPosition = userInfoButton.transform.position;
                break;
            case REWARD_TYPE.REWARD_PVP_SCORE:
                desPosition = userInfoButton.transform.position;
                break;
            case REWARD_TYPE.REWARD_ACCOUNT_EXP:
                desPosition = userInfoButton.transform.position;
                break;
            default:
                return;
        }
        SetStartAssetGetEffect(rewardType, Vector3.zero);
        
        var eff = rewardEffects[rewardType];
        eff.MoveTarget(desPosition, UIManager.Inst.UIData.startSpeed, UIManager.Inst.UIData.accSpeed);
    }
    private Dictionary<REWARD_TYPE, AssetEffect> rewardEffects = new Dictionary<REWARD_TYPE, AssetEffect>(new RewardTypeComparer()); 
    private UIParticlePlay assetGetParticle_UserInfoPos;
    private UIParticlePlay assetGetParticle_GoldPos;
    private UIParticlePlay assetGetParticle_DiaPos;
    private Dictionary<REWARD_TYPE,UIParticlePlay> assetGetParticles_Start;
    public void SetStartAssetGetEffect(REWARD_TYPE rewardType, Vector3 pos)
    {
        if (assetGetParticles_Start == null)
            assetGetParticles_Start = new Dictionary<REWARD_TYPE, UIParticlePlay>(new RewardTypeComparer());
        float size = UIManager.Inst.UIData.effSize;

        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_BOX:
            case REWARD_TYPE.REWARD_BOX_EXPAND:
                if(assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_BOX) == false
                    || assetGetParticles_Start[REWARD_TYPE.REWARD_BOX] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_Box_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);
                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_BOX) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.REWARD_BOX, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.REWARD_BOX] = assetGetParticle;
                }
                
                assetGetParticles_Start[REWARD_TYPE.REWARD_BOX].Play();
                assetGetParticles_Start[REWARD_TYPE.REWARD_BOX].SetPosition(pos);
                break;
            case REWARD_TYPE.ASSET_DIAMOND:
            case REWARD_TYPE.ASSET_FREE_DIAMOND:
                if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.ASSET_DIAMOND) == false 
                    || assetGetParticles_Start[REWARD_TYPE.ASSET_DIAMOND] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_Dia_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);

                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.ASSET_DIAMOND) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.ASSET_DIAMOND, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.ASSET_DIAMOND] = assetGetParticle;
                }
                assetGetParticles_Start[REWARD_TYPE.ASSET_DIAMOND].Play();
                assetGetParticles_Start[REWARD_TYPE.ASSET_DIAMOND].SetPosition(pos);
                break;
            case REWARD_TYPE.ASSET_FREE_GOLD:
            case REWARD_TYPE.ASSET_GOLD:
                if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.ASSET_GOLD) == false
                    || assetGetParticles_Start[REWARD_TYPE.ASSET_GOLD] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_Gold_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);

                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.ASSET_GOLD) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.ASSET_GOLD, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.ASSET_GOLD] = assetGetParticle;
                }
                
                assetGetParticles_Start[REWARD_TYPE.ASSET_GOLD].Play();
                assetGetParticles_Start[REWARD_TYPE.ASSET_GOLD].SetPosition(pos);
                break;
            case REWARD_TYPE.REWARD_MATCH_EXPL:
                if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_MATCH_EXPL) == false
                    || assetGetParticles_Start[REWARD_TYPE.REWARD_MATCH_EXPL] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_Medal_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);

                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_MATCH_EXPL) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.REWARD_MATCH_EXPL, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.REWARD_MATCH_EXPL] = assetGetParticle;
                }
                
                assetGetParticles_Start[REWARD_TYPE.REWARD_MATCH_EXPL].Play();
                assetGetParticles_Start[REWARD_TYPE.REWARD_MATCH_EXPL].SetPosition(pos);
                break;
            case REWARD_TYPE.REWARD_PVP_SCORE:
                if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_PVP_SCORE) == false
                    || assetGetParticles_Start[REWARD_TYPE.REWARD_PVP_SCORE] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_Trophy_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);

                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_PVP_SCORE) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.REWARD_PVP_SCORE, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.REWARD_PVP_SCORE] = assetGetParticle;
                }
                
                assetGetParticles_Start[REWARD_TYPE.REWARD_PVP_SCORE].Play();
                assetGetParticles_Start[REWARD_TYPE.REWARD_PVP_SCORE].SetPosition(pos);
                break;
            case REWARD_TYPE.REWARD_ACCOUNT_EXP:
                if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_ACCOUNT_EXP) == false 
                    || assetGetParticles_Start[REWARD_TYPE.REWARD_ACCOUNT_EXP] == null)
                {
                    var assetGetParticle = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_XP_c1");
                    var rectTranform = assetGetParticle.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);

                    if (assetGetParticles_Start.ContainsKey(REWARD_TYPE.REWARD_ACCOUNT_EXP) == false)
                        assetGetParticles_Start.Add(REWARD_TYPE.REWARD_ACCOUNT_EXP, assetGetParticle);
                    else
                        assetGetParticles_Start[REWARD_TYPE.REWARD_ACCOUNT_EXP] = assetGetParticle;
                }
                
                assetGetParticles_Start[REWARD_TYPE.REWARD_ACCOUNT_EXP].Play();
                assetGetParticles_Start[REWARD_TYPE.REWARD_ACCOUNT_EXP].SetPosition(pos);
                break;
            default:
                return;
        }
    }
    public void SetDestinationAssetEffect(REWARD_TYPE rewardType)
    {
        float size = UIManager.Inst.UIData.effSize;
        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_BOX:
            case REWARD_TYPE.REWARD_BOX_EXPAND:
            case REWARD_TYPE.REWARD_MATCH_EXPL:
            case REWARD_TYPE.REWARD_PVP_SCORE:
            case REWARD_TYPE.REWARD_ACCOUNT_EXP:
                if (assetGetParticle_UserInfoPos == null)
                {
                    assetGetParticle_UserInfoPos = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_t1");
                    var rectTranform = assetGetParticle_UserInfoPos.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);
                }
                assetGetParticle_UserInfoPos.SetPosition(userInfoButton.transform.position);
                assetGetParticle_UserInfoPos.Play();
                break;
            case REWARD_TYPE.ASSET_DIAMOND:
            case REWARD_TYPE.ASSET_FREE_DIAMOND:
                if (assetGetParticle_DiaPos == null)
                {
                    assetGetParticle_DiaPos = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_t1");
                    var rectTranform = assetGetParticle_DiaPos.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);
                }
                assetGetParticle_DiaPos.SetPosition(GemText.transform.position);
                assetGetParticle_DiaPos.Play();
                break;
            case REWARD_TYPE.ASSET_FREE_GOLD:
            case REWARD_TYPE.ASSET_GOLD:
                if (assetGetParticle_GoldPos == null)
                {
                    assetGetParticle_GoldPos = new UIParticlePlay(UIManager.Inst.uiTopCanvas.transform, "Prefab/Effect/UI/eff_UI_GetAssets_t1");
                    var rectTranform = assetGetParticle_GoldPos.Go.GetComponent<Transform>();
                    rectTranform.localScale = new Vector3(size, size, size);
                }
                assetGetParticle_GoldPos.SetPosition(GoldText.transform.position);
                assetGetParticle_GoldPos.Play();
                break;
        }
    }

    #endregion

    #region 메뉴 좌우 이동
    public void UpdateMenu(int index)
    {
        Debug.Log("UpdateMenu : " + ((LobbyBottomMenu)index).ToString());
        if (currentMenu == (LobbyBottomMenu)index)
            return;
        SetTab((LobbyBottomMenu)index);
    }

    #endregion

    #region 협동전상자 푸시
    IEnumerator SetPush()
    {
        yield return new WaitForSeconds(30f);
#if UNITY_EDITOR || DEV
        Debug.Log("SetPush");
#endif
        if (scene.Lobby.Instance) scene.Lobby.Instance.SetTeamRewardBoxPush();
    }
    #endregion

    #region 스와이프시 스크롤 크기
    public void ScrollCompSwitch(bool isOn)
    {
        for(int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if(BottomMenuPanel[i].menuGrp != null)
            {
                BottomMenuPanel[i].menuGrp.ScrollCompSwitch(isOn);
            }
        }
    }
    #endregion

    public override void RefreshEffect(bool isActive)
    {
        base.RefreshEffect(isActive);
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].menuGrp != null)
            {
                BottomMenuPanel[i].menuGrp.RefreshEffect(isActive);
            }
        }
    }

    #region 상점 알림
    public void SetShopTabAlarm()
    {
        bool isComplete = false;
        if (User.Inst.Doc.DailyShop.ContainsKey((int)SHOP_TYPE.DAILY_SPECIAL))
        {
            var dailySlots = User.Inst.Doc.DailyShop[(int)SHOP_TYPE.DAILY_SPECIAL].Slots;
            for (int i = 0; i < dailySlots.Count; i++)
            {
                var slotNo = Convert.ToByte(i + 1);

                if (dailySlots.ContainsKey(slotNo) == false)
                {
                    continue;
                }

                var dailyShopItem = User.Inst.Doc.DailyShop[(int)SHOP_TYPE.DAILY_SPECIAL].Slots[slotNo];
                if (User.Inst.TBL.DailySpecial.ContainsKey(dailyShopItem.Idx) == false)
                {
                    continue;
                }
                var shopTableInfo = User.Inst.TBL.DailySpecial[dailyShopItem.Idx];
                if (string.IsNullOrEmpty(shopTableInfo.PriceType) || shopTableInfo.PriceEach == 0)
                {
                    //무료
                    if (dailyShopItem.State == 2)
                        isComplete = true;
                }
            }
        }
        
        for (int i = 0; i < BottomMenuPanel.Count; i++)
        {
            if (BottomMenuPanel[i].type == LobbyBottomMenu.Shop)
            {
                BottomMenuPanel[i].alarm.SetActive(isComplete == false);
                break;
            }
        }
    }
    #endregion
    #region 마블 알림
    public void SetMarbleTabAlarm()
    {
        bool isMarbleUpgrade = false;
        foreach (var pair in User.Inst.Doc.MarbleInven)
        {
            int marbleIndex = pair.Value.Idx;
            int grade = 1;
            int curCount = 0, nextCount = 0;
            bool isMaxGrade = false;

            grade = pair.Value.Grade;
            curCount = pair.Value.Count;
            var marbleTable = User.Inst.TBL.Marble[marbleIndex];
            if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleTable.Rarity))
            {
                if (Sheet.TBL.Gens.GradeUp[marbleTable.Rarity].ContainsKey(grade + 1))
                    nextCount = Sheet.TBL.Gens.GradeUp[marbleTable.Rarity][grade + 1].NeedMarble;
                else
                    isMaxGrade = true;
            }
            if (isMaxGrade == false)
            {
                isMarbleUpgrade = (nextCount <= curCount);
                if (isMarbleUpgrade) break;
            }
        }
        for(int i =0; i< BottomMenuPanel.Count; i++)
        {
            if(BottomMenuPanel[i].type == LobbyBottomMenu.Marble)
            {
                BottomMenuPanel[i].alarm.SetActive(isMarbleUpgrade);
                break;
            }
        }
    }
    #endregion
}
