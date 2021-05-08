using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
using Net.Impl;
using App.UI;
using DG.Tweening;
public class BoxPopupItem : UIBaseGrp
{
    [SerializeField] Button infoButton;
    [SerializeField] Image boxImage;
    [SerializeField] Text boxCountText;
    [SerializeField] Text boxNameText;
    [SerializeField] GameObject boxOpenRemainTimeObject;
    [SerializeField] Text boxOpenRemainTimeText;
    [Header("Button")]
    [SerializeField] GameObject buttonGrp;
    [SerializeField] Button boxOpenButton;
    [SerializeField] UIEventButton adButton;
    [SerializeField] Button immediatelyOpenButton;
    [SerializeField] GameObject buttonOffGrp;
    [SerializeField] Image priceTypeIcon;
    [SerializeField] Text priceText;
    [SerializeField] Text adButtonText;
    [SerializeField] Text adCountText;
    [SerializeField] GameObject enableImage;
    [Header("광고 다시보기 ToastPopup")]
    [SerializeField] GameObject adWaitGrp;
    [SerializeField] Text adWaitText;

    private int currentBoxID;
    private int currentBoxSlotID;
    private bool playTime;
    public void Init(int boxSlotID)
    {
        base.Init();
        currentBoxSlotID = boxSlotID;
        if(User.Inst.TBL.BOX_Slot.ContainsKey(boxSlotID) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        currentBoxID = User.Inst.TBL.BOX_Slot[boxSlotID].BoxID;
        enableImage.SetActive(false);
        SetBoxButton();
        SetBoxInfo();
        refreshTime = 0;
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        if (infoButton) infoButton.onClick.AddListener(OnClickInfoButton);
        if (boxOpenButton) boxOpenButton.onClick.AddListener(OnClickOpenBoxButton);
        if (adButton) adButton.AddClickEvnet(OnClickADButton);
        if (adButton) adButton.AddClickUpEvnet(() => { OnADButtonEvent(true); });
        if (adButton) adButton.AddClickDownEvnet(() => { OnADButtonEvent(false); });
        if (immediatelyOpenButton) immediatelyOpenButton.onClick.AddListener(OnClickImmediatelyOpenButton);

    }
    
    private void SetBoxInfo()
    {
        playTime = false;
        if (boxOpenRemainTimeObject) boxOpenRemainTimeObject.SetActive(false);
        int boxCount = 0;
        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID)
            && User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxCount > 0)
        {
            var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
            var now = Libs.Utils.TimeLib.Seconds;
            if (openTime > now)
            {
                if (boxOpenRemainTimeObject) boxOpenRemainTimeObject.SetActive(true);
                if (boxOpenRemainTimeText) boxOpenRemainTimeText.text = UIUtil.ConvertSecToTimeString(openTime - now, 1);
                playTime = true;
            }

            boxCount = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxCount;
        }
        int maxCount = 0;
        if(User.Inst.TBL.BOX_Slot.ContainsKey(currentBoxSlotID))
        {
            maxCount = User.Inst.TBL.BOX_Slot[currentBoxSlotID].MaxCount;
            if(BaseImpl.Inst.CheckJoinSubscribe(User.Inst))
                maxCount = Mathf.CeilToInt(maxCount * Sheet.TBL.Const["CONST_SUBSCRIBE_COOP_BOXMAX_INC"].Value);
        }
        if (boxCountText) boxCountText.text = string.Format("{0}/{1}", boxCount, maxCount);



        //BoxImage
        var boxSlotInfo = User.Inst.TBL.BOX_Slot[currentBoxSlotID];
        var boxinfo = User.Inst.TBL.Box[boxSlotInfo.BoxID];

        UIManager.Inst.SetSprite(boxImage, UIManager.AtlasName.ShopIcon, boxinfo.BoxImage);

        //BoxName
        boxNameText.text = User.Inst.Langs.Box[boxSlotInfo.BoxID].Name;
    }

    long currentAdCount;
    long adTicketGenTime;
    private void SetBoxButton()
    {
        adWaitGrp.SetActive(false);

        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID) == false
            || User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxCount <= 0)
        {
            if (buttonGrp) buttonGrp.SetActive(false);
            if (buttonOffGrp) buttonOffGrp.SetActive(true);
            return;
        }
        if (buttonGrp) buttonGrp.SetActive(true);
        if (buttonOffGrp) buttonOffGrp.SetActive(false);

        var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
        var now = Libs.Utils.TimeLib.Seconds;
        if (boxOpenButton) boxOpenButton.gameObject.SetActive(openTime <= now);
        if (immediatelyOpenButton) immediatelyOpenButton.gameObject.SetActive(openTime > now);
        if (adButton) adButton.gameObject.SetActive(openTime > now);
        // TODO: 시간에 따른 다이아양 수정
        if (openTime > now)
        {
            string min = string.Format("-{0}", UIUtil.GetText("UI_Common_Minute"));
            adButtonText.text = string.Format(min, UIUtil.GetConstValue("CONST_AD_COOPBOX_TIME_DEC"));
            SetAdTicketInfo();

            if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID))
            {
                long remainTime = openTime - now;
                int remainTime_min = (int)remainTime / 60;
                int index = 0;
                foreach (var pair in User.Inst.TBL.Quick_Open)
                {
                    if (pair.Value.Time <= remainTime_min)
                        index = pair.Key;
                }
                ASSETS priceType = ASSETS.ASSET_FREE_DIAMOND;
                int priceValue = 0;
                bool ableBuy = false;
                if (User.Inst.TBL.Quick_Open.ContainsKey(index))
                {
                    priceType = (ASSETS)System.Enum.Parse(typeof(ASSETS), User.Inst.TBL.Quick_Open[index].PriceType);
                    priceValue = User.Inst.TBL.Quick_Open[index].Price;
                    ableBuy = BaseImpl.Inst.CheckAsset(User.Inst, priceType, (-1) * priceValue);
                }
                UIUtil.SetAssetIcon(priceType, priceTypeIcon);
                if(priceValue <= 0)
                    priceText.text = UIUtil.GetText("UI_Common_Free");
                else
                    priceText.text = priceValue.ToString();
                immediatelyOpenButton.interactable = ableBuy;
            }
        }
    }

    private void OnClickInfoButton()
    {
        UIManager.Inst.ShowBoxRewardPopup(currentBoxID);
    }
    private void OnClickOpenBoxButton()
    {
        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID) == false)
            return;
        var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
        var now = Libs.Utils.TimeLib.Seconds;
        if (openTime > now)
            return;
        ImplBase.Actor.Parser<BoxOpen.Ack>(new BoxOpen.Req() { BoxID = currentBoxSlotID, OpenType = 1}, (ack) =>
        {
            var boxSlotInfo = User.Inst.TBL.BOX_Slot[currentBoxSlotID];
            var boxInfo = User.Inst.TBL.Box[boxSlotInfo.BoxID];
            if (string.IsNullOrEmpty(boxInfo.Open_Effect) == false)
            {
                //병오픈 연출보여주기
                REWARD_TYPE marbleRewardType = REWARD_TYPE.REWARD_MARBLE;
                foreach (var item in ack.Reward)
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
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Reward);
                }
            }
            else
            {
                UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Reward);
            }
            SetBoxButton();
            SetBoxInfo();
            UIManager.Inst.RefreshLobbyAsset();

            if(scene.Lobby.Instance) scene.Lobby.Instance.SetTeamRewardBoxPush();
        });
    }
    private void OnADButtonEvent(bool up)
    {
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_ITEMBOX_TIME, -1))
        {
            return;
        }
        adWaitGrp.SetActive(up == false);
        if(up == false)
        {
            DOTween.Restart(adWaitGrp, "ADBtn");
        }
    }
    private void OnClickADButton()
    {
        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID) == false)
            return;
        var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
        var now = Libs.Utils.TimeLib.Seconds;
        if (openTime <= now)
        {
            return;
        }

        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_ITEMBOX_TIME, -1) == false)
        {
            //UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
            return;
        }

        UIManager.Inst.ShowAD("ItemBoxTimeReduce", Success, ()=> { StartCoroutine(EnableAdButton()); }, NotLoad);
        void Success()
        {
            UIManager.Inst.ShowConnectingPanel(true);
            ImplBase.Actor.Parser<ADBoxOpendReduce.Ack>(new ADBoxOpendReduce.Req() { BoxID = currentBoxSlotID }, (ack) =>
            {
                UIManager.Inst.ShowConnectingPanel(false);
                if (ack.Result != RESULT.OK)
                {
                    return;
                }
                SetBoxButton();
                StartCoroutine(EnableAdButton());
                if (scene.Lobby.Instance) scene.Lobby.Instance.SetTeamRewardBoxPush();
            });
        }
        void NotLoad()
        {
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
        }
    }
    private void OnClickImmediatelyOpenButton()
    {
        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID) == false)
            return;
        var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
        var now = Libs.Utils.TimeLib.Seconds;
        if(openTime > now)
        {
            long remainTime = openTime - now;
            int remainTime_min = (int)remainTime / 60;
            int index = 0;
            foreach (var pair in User.Inst.TBL.Quick_Open)
            {
                if (pair.Value.Time <= remainTime_min)
                    index = pair.Key;
            }
            if (User.Inst.TBL.Quick_Open.ContainsKey(index))
            {
                var priceType = (ASSETS)System.Enum.Parse(typeof(ASSETS), User.Inst.TBL.Quick_Open[index].PriceType);
                int priceValue = User.Inst.TBL.Quick_Open[index].Price;
                bool ableBuy = BaseImpl.Inst.CheckAsset(User.Inst, priceType, (-1) * priceValue);
                if (ableBuy == false)
                {
                    UIManager.Inst.ShowNotEnoughAssetPopup();
                    return;
                }
            }
        }
        ImplBase.Actor.Parser<BoxOpen.Ack>(new BoxOpen.Req() { BoxID = currentBoxSlotID, OpenType = 2 }, (ack) =>
        {
            if (ack.Result == RESULT.NOT_ENOUGH_ASSET)
                UIManager.Inst.ShowNotEnoughAssetPopup();
            if(ack.Result != RESULT.OK)
            {
                return;
            }


            var boxSlotInfo = User.Inst.TBL.BOX_Slot[currentBoxSlotID];
            var boxInfo = User.Inst.TBL.Box[boxSlotInfo.BoxID];
            if (string.IsNullOrEmpty(boxInfo.Open_Effect) == false)
            {
                //병오픈 연출보여주기
                REWARD_TYPE marbleRewardType = REWARD_TYPE.REWARD_MARBLE;
                foreach (var item in ack.Reward)
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
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Reward);
                }
            }
            else
            {
                UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Reward);
            }

            SetBoxButton();
            SetBoxInfo();
            UIManager.Inst.RefreshLobbyAsset();
            if (scene.Lobby.Instance) scene.Lobby.Instance.SetTeamRewardBoxPush();
        });
    }
    float refreshTime = 0;
    private void Update()
    {
        if(playTime)
        {
            if (refreshTime > 1f)
            {
                var openTime = User.Inst.Doc.BoxSlot[currentBoxSlotID].BoxOpenDate;
                var now = Libs.Utils.TimeLib.Seconds;
                if (openTime > now)
                {
                    if (boxOpenRemainTimeText) boxOpenRemainTimeText.text = UIUtil.ConvertSecToTimeString(openTime - now, 1);
                    // TODO: 시간에 따른 다이아양 수정
                    if (openTime > now)
                    {
                        if (User.Inst.Doc.BoxSlot.ContainsKey(currentBoxSlotID))
                        {
                            long remainTime = openTime - now;
                            int remainTime_min = (int)remainTime / 60;
                            int index = 0;
                            foreach (var pair in User.Inst.TBL.Quick_Open)
                            {
                                if (pair.Value.Time <= remainTime_min)
                                    index = pair.Key;
                            }
                            ASSETS priceType = ASSETS.ASSET_FREE_DIAMOND;
                            int priceValue = 0;
                            bool ableBuy = false;
                            if (User.Inst.TBL.Quick_Open.ContainsKey(index))
                            {
                                priceType = (ASSETS)System.Enum.Parse(typeof(ASSETS), User.Inst.TBL.Quick_Open[index].PriceType);
                                priceValue = User.Inst.TBL.Quick_Open[index].Price;
                                ableBuy = BaseImpl.Inst.CheckAsset(User.Inst, priceType, (-1) * priceValue);
                            }
                            UIUtil.SetAssetIcon(priceType, priceTypeIcon);
                            if (priceValue <= 0)
                                priceText.text = UIUtil.GetText("UI_Common_Free");
                            else
                                priceText.text = priceValue.ToString();
                            immediatelyOpenButton.interactable = ableBuy;
                        }
                    }
                    if(currentAdCount != BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_ITEMBOX_TIME))
                    {
                        SetAdTicketInfo();
                    }
                }
                else
                {
                    if (boxOpenRemainTimeObject) boxOpenRemainTimeObject.SetActive(false);
                    SetBoxInfo();
                    SetBoxButton();
                    playTime = false;
                }
                refreshTime = 0f;
            }
            else
                refreshTime += Time.deltaTime;
        }



        if(adWaitGrp.activeSelf)
        {
            var now = Libs.Utils.TimeLib.Seconds;
            adWaitText.text = UIUtil.ConvertSecToTimeString(adTicketGenTime - now, 1);
        }
    }

    private void SetAdTicketInfo()
    {
        Debug.Log("SetAdTicketInfo");
        var tableInfo = User.Inst.TBL.Assets[(int)ASSETS.ASSET_AD_ITEMBOX_TIME];
        currentAdCount = BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_ITEMBOX_TIME);
        long maxAdCount = tableInfo.MaxValue;
        adCountText.text = string.Format(UIUtil.GetText("UI_CoopBox_ADCount"), currentAdCount, maxAdCount);
        adTicketGenTime = User.Inst.Doc.Assets[(int)ASSETS.ASSET_AD_ITEMBOX_TIME].GenTime + (tableInfo.ChargeDelay * 60);
    }

    IEnumerator EnableAdButton()
    {
        enableImage.SetActive(true);
        adButton.enabled = false;
        yield return new WaitForSeconds(2f);
        adButton.enabled = true;
        enableImage.SetActive(false);
    }
}
