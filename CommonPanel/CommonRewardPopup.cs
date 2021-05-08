using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
using System;
using Net.Impl;

public class CommonRewardPopup : UIBasePanel
{
    [Header("Asset")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text GemText;
    [Header("Top Icon Image")]
    [SerializeField] GameObject boxGrp;

    [SerializeField] GameObject shopGrp;
    [SerializeField] Image shopIconImage;
    [SerializeField] Image shopPackageIconImage;
    [Header("Top Icon Image - Marble")]
    [SerializeField] GameObject marbleGrp;
    [SerializeField] Image marbleImage;
    [SerializeField] Text marbleLevelText;
    [SerializeField] Text marbleCountText;
    [SerializeField] GameObject marbleUpgradeGrp;
    [SerializeField] Image marbleGauge;
    [SerializeField] GameObject marbleNotGainGrp;
    [SerializeField] GameObject marbleGainGrp;

    [Header("PopupTitle")]
    [SerializeField] Text titleText;
    [Header("Desc")]
    [SerializeField] GameObject descGrp;
    [SerializeField] Text descText;
    [SerializeField] Button descButton;
    [Header("Reward")]
    [SerializeField] GameObject titleGrp1;
    [SerializeField] Text titleText1;
    [SerializeField] Transform infoItemParent1;
    [SerializeField] GameObject titleGrp2;
    [SerializeField] Text titleText2;
    [SerializeField] Transform infoItemParent2;
    [SerializeField] GameObject infoItmeObject;
    [Header("Button")]
    [Header("ConfirmButton")]
    [SerializeField] Button okButton;
    [SerializeField] Text okButtonText;
    [Header("PurchaseButton")]
    [SerializeField] Button buyButton;
    [Header("Asset")]
    [SerializeField] GameObject assetGrp;
    [SerializeField] Image assetPriceIconImage;
    [SerializeField] Text assetPriceText;
    [Header("InApp")]
    [SerializeField] GameObject inappGrp;
    [SerializeField] Text inappPriceText;
    [Header("AD")]
    [SerializeField] GameObject adGrp;
    [SerializeField] Text adText;
    [Header("TermsButton")]
    [SerializeField] Button termsButton;

    List<BoxInfoItem> boxInfoItems;
    List<BoxInfoItem> expandBoxInfoItems;

    Action buttonAction;
    Action closeAction;
    private GameObject idleEff;


    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        boxInfoItems = new List<BoxInfoItem>();
        expandBoxInfoItems = new List<BoxInfoItem>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        okButton.onClick.AddListener(OnClickButton);
        buyButton.onClick.AddListener(OnClickButton);
        termsButton.onClick.AddListener(OnClickTermsButton);
        descButton.onClick.AddListener(OnClickGachaRateButton);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (idleEff) idleEff.gameObject.SetActive(false);
        for (int i = 0; i < boxInfoItems.Count; i++)
        {
            boxInfoItems[i].ResetItem();
        }
        for (int i = 0; i < expandBoxInfoItems.Count; i++)
        {
            expandBoxInfoItems[i].ResetItem();
        }
        closeAction = null;
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
    private void OnClickButton()
    {
        buttonAction?.Invoke();
        ClosePanel();
    }

    //reward테이블 아이템
    private void SetReward(int rewardID, int rewardCount, bool isExpand)
    {
        if (User.Inst.TBL.Gens.Reward.ContainsKey(rewardID) == false)
            return;
        var rewards = User.Inst.TBL.Gens.Reward[rewardID];
        int boxInfoItemCount = 0;
        List<BoxInfoItem> addItemList;
        Transform parent;
        if (isExpand)
        {
            addItemList = expandBoxInfoItems;
            parent = infoItemParent2;
        }
        else
        {
            addItemList = boxInfoItems;
            parent = infoItemParent1;
        }

        foreach (var rewardItem in rewards)
        {
            if (rewardItem.Key == 0)//CompType 1
            {
                for (int i = 0; i < rewardItem.Value.Count; i++)
                {
                    if (rewardItem.Value[i].CompoRate == 10000)
                    {
                        bool duplication = false;
                        var rewardType = UIUtil.GetRewardType(rewardItem.Value[i].RewardType);
                        int rewardValue = rewardItem.Value[i].RewardValue;
                        for (int boxCount = 0; boxCount < addItemList.Count; boxCount++)
                        {
                            if (addItemList[boxCount].IsEqual(rewardType, rewardValue) && addItemList[boxCount].CompRate == 10000)
                            {
                                duplication = true;
                                addItemList[boxCount].SetDataCompType1_duplication(rewardItem.Value[i].Index, rewardCount);
                                break;
                            }
                        }
                        if (duplication)
                            continue;
                    }
                    if (addItemList.Count <= boxInfoItemCount)
                    {
                        var go = GameObject.Instantiate(infoItmeObject, parent);
                        var boxItem = go.GetComponent<BoxInfoItem>();
                        boxItem.Init();
                        addItemList.Add(boxItem);
                    }
                    addItemList[boxInfoItemCount].transform.SetParent(parent);
                    addItemList[boxInfoItemCount].SetDataCompType1(rewardItem.Value[i].Index, rewardCount);
                    boxInfoItemCount++;
                }
            }
            else
            {
                if (addItemList.Count <= boxInfoItemCount)
                {
                    var go = GameObject.Instantiate(infoItmeObject, parent);
                    var boxItem = go.GetComponent<BoxInfoItem>();
                    boxItem.Init();
                    addItemList.Add(boxItem);
                }
                addItemList[boxInfoItemCount].transform.SetParent(parent);
                addItemList[boxInfoItemCount].SetDataCompType2(rewardID, rewardItem.Key, rewardCount);
                boxInfoItemCount++;
            }
        }
    }

    public void SetBoxID(int boxID, string title = "", string desc = "", string buttonText = "", Action buttonAction = null, Action closeAction = null)
    {
        ClearRewardItem();
        this.buttonAction = null;
        this.closeAction = null;
        if (User.Inst.TBL.Box.ContainsKey(boxID) == false)
        {
            ClosePanel();
            return;
        }
        var boxInfo = User.Inst.TBL.Box[boxID];
        var boxLnagInfo = User.Inst.Langs.Box[boxID];

        boxGrp.SetActive(true);
        shopGrp.SetActive(true);
        marbleGrp.SetActive(false);
        
        shopPackageIconImage.gameObject.SetActive(false);
        UIManager.Inst.SetSprite(shopIconImage, UIManager.AtlasName.ShopIcon, boxInfo.BoxImage);

        //타이틀
        if (string.IsNullOrEmpty(title))
        {
            titleText.text = boxLnagInfo.Name;
        }
        else
        {
            titleText.text = title;
        }

        //설명
        descGrp.SetActive(false);
        descButton.interactable = false;
        if (IsMarble(boxInfo.RewardID) > 0)
        {
            //마블 뽑기 팁
            descGrp.SetActive(true);
            descText.text = UIUtil.GetText("UI_Common_BoxTip");
            descButton.interactable = true;
        }
        else
        {
            if (string.IsNullOrEmpty(desc))
            {
                descGrp.SetActive(false);
            }
            else
            {
                descGrp.SetActive(true);
                descText.text = desc;
            }
        }

        //버튼
        okButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(buttonText))
        {
            okButtonText.text = UIUtil.GetText("UI_Common_Ok");
        }
        else
        {
            okButtonText.text = buttonText;
        }
        this.buttonAction = buttonAction;
        this.closeAction = closeAction;

        //보상 아이템
        SetReward(boxInfo.RewardID, 1, false);
        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);
        descGrp.SetActive(false);
        if (User.Inst.TBL.Box.ContainsKey(boxInfo.ExpandBoxID))
        {
            titleGrp1.SetActive(true);
            titleGrp2.SetActive(true);
            infoItemParent2.gameObject.SetActive(true);
            var expandBoxInfo = User.Inst.TBL.Box[boxInfo.ExpandBoxID];
            var expandBoxLangInfo = User.Inst.Langs.Box[boxInfo.ExpandBoxID];
            titleText2.text = expandBoxLangInfo.Name;
            SetReward(expandBoxInfo.RewardID, 1, true);

            if (IsMarble(expandBoxInfo.RewardID) > 0)
            {
                //마블 뽑기 팁
                descGrp.SetActive(true);
                descText.text = UIUtil.GetText("UI_Common_BoxTip");
                descButton.interactable = true;
            }
        }

        if (termsButton) termsButton.gameObject.SetActive(false);

    }
    public void SetShopID(int shopID, string title = "", string desc = "", Action buttonAction = null)
    {
        ClearRewardItem();
        this.buttonAction = null;

        if (User.Inst.TBL.Shop.ContainsKey(shopID) == false)
        {
            ClosePanel();
            return;
        }

        //맨위 그룹
        var shopInfo = User.Inst.TBL.Shop[shopID];
        var shopLangInfo = User.Inst.Langs.Shop[shopID];

        boxGrp.SetActive(true);
        shopGrp.SetActive(true);
        marbleGrp.SetActive(false);


        //맨위 아이콘
        descGrp.SetActive(false);
        descButton.interactable = false;
        REWARD_TYPE rewardType = UIUtil.GetRewardType(shopInfo.RewardType);
        if (rewardType == REWARD_TYPE.REWARD_MARBLE
            || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
            || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
            || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
            || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
            || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
        {
            boxGrp.SetActive(true);
            shopGrp.SetActive(false);
            marbleGrp.SetActive(true);
            SetMarble(shopInfo.RewardValue);

            //마블 뽑기 팁
            descGrp.SetActive(true);
            descText.text = UIUtil.GetText("UI_Common_BoxTip");
            descButton.interactable = true;
        }
        else
        {
            shopPackageIconImage.gameObject.SetActive(false);
            UIUtil.SetRewardIcon(rewardType, shopIconImage, shopInfo.RewardValue);

            if (rewardType == REWARD_TYPE.REWARD_BOX)
            {
                if (User.Inst.TBL.Box.ContainsKey(shopInfo.RewardValue) == false)
                {
                    ClosePanel();
                    return;
                }
                var boxInfo = User.Inst.TBL.Box[shopInfo.RewardValue];
                if (IsMarble(boxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(desc))
                    {
                        descGrp.SetActive(false);
                    }
                    else
                    {
                        descGrp.SetActive(true);
                        descText.text = desc;
                    }
                }
            }
            else
            {

                if (string.IsNullOrEmpty(desc))
                {
                    descGrp.SetActive(false);
                }
                else
                {
                    descGrp.SetActive(true);
                    descText.text = desc;
                }
            }
        }
        

        //타이틀
        if (string.IsNullOrEmpty(title))
        {
            titleText.text = shopLangInfo.Name;
        }
        else
        {
            titleText.text = title;
        }
        
        //버튼
        okButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(true);
        assetGrp.SetActive(true);
        inappGrp.SetActive(false);
        adGrp.SetActive(false);

        UIUtil.SetAssetIcon(UIUtil.GetAssetsType(shopInfo.PriceType), assetPriceIconImage);
        assetPriceText.text = shopInfo.Price.ToString();

        this.buttonAction = buttonAction;

        //보상그룹
        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);

        if (rewardType == REWARD_TYPE.REWARD_BOX
            || rewardType == REWARD_TYPE.REWARD_BOX_EXPAND)
        {
            var boxInfo = User.Inst.TBL.Box[shopInfo.RewardValue];
            SetReward(boxInfo.RewardID, shopInfo.RewardCount, false);
            if (User.Inst.TBL.Box.ContainsKey(boxInfo.ExpandBoxID))
            {
                titleGrp1.SetActive(true);
                titleGrp2.SetActive(true);
                var expandBoxInfo = User.Inst.TBL.Box[boxInfo.ExpandBoxID];
                var expandBoxLangInfo = User.Inst.Langs.Box[boxInfo.ExpandBoxID];
                titleText2.text = expandBoxLangInfo.Name;
                infoItemParent2.gameObject.SetActive(true);
                SetReward(expandBoxInfo.RewardID, shopInfo.RewardCount, true);

                if (IsMarble(expandBoxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
            }
        }
        else
        {
            if (boxInfoItems.Count <= 0)
            {
                var go = GameObject.Instantiate(infoItmeObject, infoItemParent1);
                var boxItem = go.GetComponent<BoxInfoItem>();
                boxItem.Init();
                boxInfoItems.Add(boxItem);
            }
            boxInfoItems[0].SetData((REWARD_TYPE)shopInfo.RewardValue, shopInfo.RewardCount, shopInfo.RewardValue);
        }

        if (termsButton) termsButton.gameObject.SetActive(false);
    }
    public void SetInAppID(int inAppID , bool isPackage, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        ClearRewardItem();
        this.buttonAction = null;

        if (User.Inst.TBL.InAppShop.ContainsKey(inAppID) == false)
        {
            ClosePanel();
            return;
        }

        var shopInfo = User.Inst.TBL.InAppShop[inAppID];
        var inappLangInfo = User.Inst.Langs.InAppShop[inAppID];

        //맨위 그룹
        boxGrp.SetActive(true);
        shopGrp.SetActive(true);
        marbleGrp.SetActive(false);

        //맨위 아이콘
        descGrp.SetActive(false);
        descButton.interactable = false;
        REWARD_TYPE rewardType = UIUtil.GetRewardType(shopInfo.RewardType);
        if (rewardType == REWARD_TYPE.REWARD_MARBLE 
            || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
            || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
            || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
            || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
            || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
        {
            shopGrp.SetActive(false);
            marbleGrp.SetActive(true);
            SetMarble(shopInfo.RewardValue);

            //마블 뽑기 팁
            descGrp.SetActive(true);
            descText.text = UIUtil.GetText("UI_Common_BoxTip");
            descButton.interactable = true;
        }
        else
        {
            if(isPackage)
            {
                shopIconImage.gameObject.SetActive(false);
                if (string.IsNullOrEmpty(shopInfo.ShopImage) == false)
                {
                    UIManager.Inst.SetSprite(shopPackageIconImage, UIManager.AtlasName.ShopIcon, shopInfo.ShopImage);
                }
                else
                {
                    UIUtil.SetRewardIcon(rewardType, shopIconImage, shopInfo.RewardValue);
                }
            }
            else
            {
                shopPackageIconImage.gameObject.SetActive(false);
                if (string.IsNullOrEmpty(shopInfo.ShopImage) == false)
                {
                    UIManager.Inst.SetSprite(shopIconImage, UIManager.AtlasName.ShopIcon, shopInfo.ShopImage);
                }
                else
                {
                    UIUtil.SetRewardIcon(rewardType, shopIconImage, shopInfo.RewardValue);
                }

            }

            if (rewardType == REWARD_TYPE.REWARD_BOX)
            {
                if (User.Inst.TBL.Box.ContainsKey(shopInfo.RewardValue) == false)
                {
                    ClosePanel();
                    return;
                }
                var boxInfo = User.Inst.TBL.Box[shopInfo.RewardValue];
                if (IsMarble(boxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(desc))
                    {
                        descGrp.SetActive(false);
                    }
                    else
                    {
                        descGrp.SetActive(true);
                        descText.text = desc;
                    }
                }
            }
        }

        //타이틀
        if (string.IsNullOrEmpty(title))
        {
            titleText.text = inappLangInfo.ShopName;
        }
        else
        {
            titleText.text = title;
        }
        
        //버튼
        okButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(true);
        assetGrp.SetActive(false);
        inappGrp.SetActive(true);
        adGrp.SetActive(false);

        inappPriceText.text = buttonText;

        this.buttonAction = buttonAction;


        //보상 아이템
        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);

        if (rewardType == REWARD_TYPE.REWARD_BOX
            || rewardType == REWARD_TYPE.REWARD_BOX_EXPAND)
        {
            var boxInfo = User.Inst.TBL.Box[shopInfo.RewardValue];
            SetReward(boxInfo.RewardID, shopInfo.RewardCount, false);
            if(User.Inst.TBL.Box.ContainsKey(boxInfo.ExpandBoxID))
            {
                titleGrp1.SetActive(true);
                titleGrp2.SetActive(true);
                var expandBoxInfo = User.Inst.TBL.Box[boxInfo.ExpandBoxID];
                var expandBoxLangInfo = User.Inst.Langs.Box[boxInfo.ExpandBoxID];
                titleText2.text = expandBoxLangInfo.Name;
                infoItemParent2.gameObject.SetActive(true);
                SetReward(expandBoxInfo.RewardID, shopInfo.RewardCount, true);

                if (IsMarble(expandBoxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
            }
        }
        else
        {
            if (boxInfoItems.Count <= 0)
            {
                var go = GameObject.Instantiate(infoItmeObject, infoItemParent1);
                var boxItem = go.GetComponent<BoxInfoItem>();
                boxItem.Init();
                boxInfoItems.Add(boxItem);
            }
            boxInfoItems[0].SetData((REWARD_TYPE)shopInfo.RewardValue, shopInfo.RewardCount, shopInfo.RewardValue);
        }

        var lang = AppClient.Inst.Language;
#if !UNITY_EDITOR
            lang = hive.Configuration.getHiveCountry();
#endif
        lang = lang.ToLower();
        Debug.Log("lang Code : " + lang);
        if (lang.Equals("ko") == false && lang.Equals("kr") == false)
        {
            if (termsButton) termsButton.gameObject.SetActive(false);
        }
        else
        {
            if (termsButton) termsButton.gameObject.SetActive(true);
        }
    }
    public void SetADID(int boxID, string title = "", string desc = "", string buttonText = "", Action buttonAction = null, Action endAction = null)
    {
        ClearRewardItem();
        this.buttonAction = null;

        if (User.Inst.TBL.Box.ContainsKey(boxID) == false)
        {
            ClosePanel();
            return;
        }

        var boxInfo = User.Inst.TBL.Box[boxID];
        var boxLnagInfo = User.Inst.Langs.Box[boxID];

        //위쪽 아이콘
        boxGrp.SetActive(true);
        shopGrp.SetActive(true);
        marbleGrp.SetActive(false);

        shopPackageIconImage.gameObject.SetActive(false);
        UIManager.Inst.SetSprite(shopIconImage, UIManager.AtlasName.ShopIcon, boxInfo.BoxImage);

        descButton.interactable = false;
        descGrp.SetActive(false);
        if (IsMarble(boxInfo.RewardID) > 0)
        {
            //마블 뽑기 팁
            descGrp.SetActive(true);
            descText.text = UIUtil.GetText("UI_Common_BoxTip");
            descButton.interactable = true;
        }
        else
        {
            if (string.IsNullOrEmpty(desc))
            {
                descGrp.SetActive(false);
            }
            else
            {
                descGrp.SetActive(true);
                descText.text = desc;
            }
        }

        //타이틀
        //titleText.text = title;
        titleText.text = boxLnagInfo.Name;
        //버튼
        okButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(true);
        assetGrp.SetActive(false);
        inappGrp.SetActive(false);
        adGrp.SetActive(true);

        inappPriceText.text = buttonText;

        this.buttonAction = buttonAction;
        this.closeAction = endAction;
        //보상아이템
        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);

        SetReward(boxInfo.RewardID, 1, false);

        if (termsButton) termsButton.gameObject.SetActive(false);
    }
    public void SetRewards(Rewards rewards, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        ClearRewardItem();
        boxGrp.SetActive(false);
        this.buttonAction = null;

        titleText.text = title;
        descButton.interactable = false;
        descGrp.SetActive(false);
        if (string.IsNullOrEmpty(desc))
        {
            descGrp.SetActive(false);
        }
        else
        {
            descGrp.SetActive(true);
            descText.text = desc;
        }
        okButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(buttonText))
        {
            okButtonText.text = UIUtil.GetText("UI_Common_Ok");
        }
        else
        {
            okButtonText.text = buttonText;
        }
        this.buttonAction = buttonAction;

        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);
        for (int i = 0; i < rewards.Count; i++)
        {
            if (boxInfoItems.Count <= i)
            {
                var go = GameObject.Instantiate(infoItmeObject, infoItemParent1);
                var boxItem = go.GetComponent<BoxInfoItem>();
                boxItem.Init();
                boxInfoItems.Add(boxItem);
            }
            boxInfoItems[i].SetData((REWARD_TYPE)rewards[i].Type, rewards[i].Count, rewards[i].Value);

        }

        if (termsButton) termsButton.gameObject.SetActive(false);
    }
    public void SetRewardGains(RewardGains rewardGains, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        ClearRewardItem();
        boxGrp.SetActive(false);
        this.buttonAction = null;

        titleText.text = title;
        descButton.interactable = false;
        descGrp.SetActive(false);
        if (string.IsNullOrEmpty(desc))
        {
            descGrp.SetActive(false);
        }
        else
        {
            descGrp.SetActive(true);
            descText.text = desc;
        }
        okButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);
        if(string.IsNullOrEmpty(buttonText))
        {
            okButtonText.text = UIUtil.GetText("UI_Common_Ok");
        }
        else
        {
            okButtonText.text = buttonText;
        }
        this.buttonAction = buttonAction;

        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);
        for (int i = 0; i < rewardGains.Count; i++)
        {
            if (boxInfoItems.Count <= i)
            {
                var go = GameObject.Instantiate(infoItmeObject, infoItemParent1);
                var boxItem = go.GetComponent<BoxInfoItem>();
                boxItem.Init();
                boxInfoItems.Add(boxItem);
            }
            boxInfoItems[i].SetData((REWARD_TYPE)rewardGains[i].Type, rewardGains[i].Count, rewardGains[i].Value);

        }

        if (termsButton) termsButton.gameObject.SetActive(false);
    }

    public void SetDailyShop(Reward reward, int dailyIndex, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        if (User.Inst.TBL.DailySpecial.ContainsKey(dailyIndex) == false)
        {
            ClosePanel();
            return;
        }

        ClearRewardItem();
        this.buttonAction = null;

        boxGrp.SetActive(true);
        marbleGrp.SetActive(false);
        shopGrp.SetActive(true);

        //위쪽 아이콘
        descButton.interactable = false;
        descGrp.SetActive(false);
        if (((REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE
            || (REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE_LEGEND
            || (REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE_MYTH
            || (REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE_NORMAL
            || (REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE_RARE
            || (REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
            && reward.Value != 0)
        {
            boxGrp.SetActive(true);
            shopGrp.SetActive(false);
            marbleGrp.SetActive(true);
            SetMarble(reward.Value);

            //마블 뽑기 팁
            descGrp.SetActive(true);
            descText.text = UIUtil.GetText("UI_Common_BoxTip");
            descButton.interactable = true;
        }
        else
        {
            boxGrp.SetActive(true);
            shopGrp.SetActive(true);
            marbleGrp.SetActive(false);
            shopPackageIconImage.gameObject.SetActive(false);
            UIUtil.SetRewardIcon((REWARD_TYPE)reward.Type, shopIconImage, reward.Value);

            if ((REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_BOX)
            {
                if (User.Inst.TBL.Box.ContainsKey(reward.Value) == false)
                {
                    ClosePanel();
                    return;
                }
                var boxInfo = User.Inst.TBL.Box[reward.Value];
                if (IsMarble(boxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(desc))
                    {
                        descGrp.SetActive(false);
                    }
                    else
                    {
                        descGrp.SetActive(true);
                        descText.text = desc;
                    }
                }
            }
        }

        titleText.text = title;
        

        //버튼
        okButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(true);
        assetGrp.SetActive(true);
        inappGrp.SetActive(false);
        adGrp.SetActive(false);
        var shopInfo = User.Inst.TBL.DailySpecial[dailyIndex];
        UIUtil.SetAssetIcon(UIUtil.GetAssetsType(shopInfo.PriceType), assetPriceIconImage);
        int price = shopInfo.PriceEach * reward.Count;
        if (UIUtil.CheckJoinSubscribe())
        {
            var rate = price * (UIUtil.GetConstValue("CONST_SUBSCRIBE_DAILYSHOP_SALE") / 10000);
            price -= (int)Math.Floor(rate);
        }

        if (price == 0)
        {
            okButtonText.text = UIUtil.GetText("free");
            okButton.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(false);
        }
        else
            assetPriceText.text = price.ToString("n0");

        this.buttonAction = buttonAction;

        //보상
        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);
        if ((REWARD_TYPE)reward.Type == REWARD_TYPE.REWARD_BOX
            || (REWARD_TYPE) reward.Type == REWARD_TYPE.REWARD_BOX_EXPAND)
        {
            var boxInfo = User.Inst.TBL.Box[shopInfo.Value];
            SetReward(boxInfo.RewardID, reward.Count, false);
            if(User.Inst.TBL.Box.ContainsKey(boxInfo.ExpandBoxID))
            {
                titleGrp1.SetActive(true);
                titleGrp2.SetActive(true);
                var expandBoxLangInfo = User.Inst.Langs.Box[boxInfo.ExpandBoxID];
                var expandBoxInfo = User.Inst.TBL.Box[boxInfo.ExpandBoxID];
                titleText2.text = expandBoxLangInfo.Name;
                infoItemParent2.gameObject.SetActive(true);
                
                SetReward(expandBoxInfo.RewardID, reward.Count, true);
                if (IsMarble(expandBoxInfo.RewardID) > 0)
                {
                    //마블 뽑기 팁
                    descGrp.SetActive(true);
                    descText.text = UIUtil.GetText("UI_Common_BoxTip");
                    descButton.interactable = true;
                }
            }
        }
        else
        {
            if (boxInfoItems.Count <= 0)
            {
                var go = GameObject.Instantiate(infoItmeObject, infoItemParent1);
                var boxItem = go.GetComponent<BoxInfoItem>();
                boxItem.Init();
                boxInfoItems.Add(boxItem);
            }
            boxInfoItems[0].SetData((REWARD_TYPE)reward.Type, reward.Count, reward.Value);
        }
        if (termsButton) termsButton.gameObject.SetActive(false);
    }


    public void SetBuyDesc(string title, string desc , ASSETS priceType, int priceValue, Action buttonAction = null)
    {
        ClearRewardItem();
        this.buttonAction = null;

        boxGrp.SetActive(false);

        titleText.text = title;

        descButton.interactable = false;
        descGrp.SetActive(true);
        descText.text = desc;

        //버튼
        okButton.gameObject.SetActive(false);
        buyButton.gameObject.SetActive(true);
        assetGrp.SetActive(true);
        inappGrp.SetActive(false);
        adGrp.SetActive(false);

        UIUtil.SetAssetIcon(priceType, assetPriceIconImage);
        if (priceValue == 0)
        {
            okButtonText.text = UIUtil.GetText("free");
            okButton.gameObject.SetActive(true);
            buyButton.gameObject.SetActive(false);
        }
        else
            assetPriceText.text = priceValue.ToString("n0");

        this.buttonAction = buttonAction;

        if (termsButton) termsButton.gameObject.SetActive(false);

        titleGrp1.SetActive(false);
        titleGrp2.SetActive(false);
        infoItemParent2.gameObject.SetActive(false);
    }
    public void SetMarble(int marbleId)
    {
        if(User.Inst.TBL.Marble.ContainsKey(marbleId) == false)
        {
            boxGrp.SetActive(false);
            return;
        }
        var marbleTable = User.Inst.TBL.Marble[marbleId];
        if (marbleImage)
        {
            string marbleIconName = marbleTable.Icon;
            UIManager.Inst.SetSprite(marbleImage, UIManager.AtlasName.MainMarble, marbleIconName);
        }
        SetIdleEffect(marbleTable);

        int grade = 1;
        int curCount = 0, nextCount = 0;
        bool isMaxGrade = false;

        if (User.Inst.Doc.MarbleInven.ContainsKey(marbleId))
        {
            grade = User.Inst.Doc.MarbleInven[marbleId].Grade;
            curCount = User.Inst.Doc.MarbleInven[marbleId].Count;
            marbleGainGrp.SetActive(true);
            marbleNotGainGrp.SetActive(false);
        }
        else
        {
            marbleGainGrp.SetActive(false);
            marbleNotGainGrp.SetActive(true);
        }
       

        if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleTable.Rarity))
        {
            if (Sheet.TBL.Gens.GradeUp[marbleTable.Rarity].ContainsKey(grade + 1))
                nextCount = Sheet.TBL.Gens.GradeUp[marbleTable.Rarity][grade + 1].NeedMarble;
            else
                isMaxGrade = true;
        }
        if (marbleGauge && marbleCountText)
        {
            if (nextCount > 0)
                marbleGauge.fillAmount = Mathf.Min((float)curCount / nextCount, 1);
            else
                marbleGauge.fillAmount = 0;
            marbleCountText.text = string.Format("{0} / {1}", curCount, nextCount);
        }
        if (marbleUpgradeGrp) marbleUpgradeGrp.SetActive(curCount >= nextCount);
        if (marbleLevelText) marbleLevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), isMaxGrade ? UIUtil.GetText("UI_Common_Max") : grade.ToString());
        
        //if (Bg)
        //{
        //    var colorInfo = UIManager.Inst.UIData.GetRarityColor((UI.MarbleRarity)marbleTable.Rarity);
        //    Bg.color1 = colorInfo.topColor;
        //    Bg.color2 = colorInfo.BottomColor;
        //}
    }

    private void SetIdleEffect(TBL.Sheet.CMarble marbleInfo)
    {
        if (idleEff != null)
            Destroy(idleEff);

        if (string.IsNullOrEmpty(marbleInfo.IdleEffect))
            return;

        var go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleInfo.IdleEffect);
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleImage.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = new Vector3(310f, 310f, 310f);
        }
    }

    private void ClearRewardItem()
    {
        for(int i = 0; i <  boxInfoItems.Count; i++)
        {
            boxInfoItems[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < expandBoxInfoItems.Count; i++)
        {
            expandBoxInfoItems[i].gameObject.SetActive(false);
        }
    }

    private int IsMarble(int rewardId)
    {
        if (User.Inst.TBL.Gens.Reward.ContainsKey(rewardId) == false)
            return 0;
        var rewardTable = User.Inst.TBL.Gens.Reward[rewardId];
        foreach(var item in rewardTable.Values)
        {
            for(int i = 0; i < item.Count; i++)
            {
                var rewardType = UIUtil.GetRewardType(item[i].RewardType);
                if(rewardType == REWARD_TYPE.REWARD_MARBLE
                    || rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND
                    || rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH
                    || rewardType == REWARD_TYPE.REWARD_MARBLE_NORMAL
                    || rewardType == REWARD_TYPE.REWARD_MARBLE_RARE
                    || rewardType == REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
                {
                    return 1;
                }
            }
        }
        return 0;
    }

    private void OnClickTermsButton()
    {
        Application.OpenURL("https://terms.withhive.com/terms/policy/view/M30");
    }

    public override void OnClickClosePanel()
    {
        closeAction?.Invoke();
        ClosePanel();
    }
    private void OnClickGachaRateButton()
    {
        var lang = AppClient.Inst.Language;
        Debug.Log("OnClickGachaRateButton : " + lang);
        lang = lang.ToLower();
        Debug.Log("OnClickGachaRateButton : " + lang);
        if (lang.Equals("kr") || lang.Equals("ko"))
        {
            Application.OpenURL("https://image-glb.qpyou.cn/markup/img/tf/rmd/rmd_ko.html");
        }
        else if(lang.Equals("en"))
        {
            Application.OpenURL("https://image-glb.qpyou.cn/markup/img/tf/rmd/rmd_en.html");
        }
        else if (lang.Equals("es"))
        {
            Application.OpenURL("https://image-glb.qpyou.cn/markup/img/tf/rmd/rmd_es.html");
        }
        else if (lang.Equals("pt"))
        {
            Application.OpenURL("https://image-glb.qpyou.cn/markup/img/tf/rmd/rmd_pt.html");
        }
    }
}
