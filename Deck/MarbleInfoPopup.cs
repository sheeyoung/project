using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Impl;
using Net.Api;
using Coffee.UIExtensions;

public class MarbleInfoPopup : UIBasePanel
{
    [Header("Asset")]
    [SerializeField] private Text goldText;
    [Header("BaseInfo")]
    [SerializeField] private Text rarityText;
    [SerializeField] private Text marbleNameText;
    [SerializeField] private Image marbleImage;
    [SerializeField] private Image marbleImage_off;
    [Header("LevelInfo")]
    [SerializeField] private Text levelText;
    [SerializeField] private Text marbleCount;    
    [SerializeField] private Image levelProgressImg;
    [SerializeField] private GameObject levelGaugeGrp;
    [SerializeField] private GameObject levelUpGrp;
    [SerializeField] private GameObject levelUpAlarmGrp;
    [Header("AttckInfo")]
    [SerializeField] private Text targetText;
    [SerializeField] private Text powerText;
    [SerializeField] private Text atkSpeedText;
    [SerializeField] private Text dpsText;
    [Header("AblilityInfo")]
    [SerializeField] private List<GameObject> abilityItems;
    [SerializeField] private Text descText;
    [SerializeField] private GameObject abilityTitle;
    [Header("Button")]
    [SerializeField] private Button ShowStatButton;
    [SerializeField] private Button GradeUpButton;
    [SerializeField] private Button EditButton;
    [SerializeField] private Button GoToShopButton;
    [Header("Bg")]
    [SerializeField] private GameObject Bg1;
    [SerializeField] private GameObject Bg2;
    [SerializeField] private GameObject Bg3;
    [SerializeField] private GameObject Bg4;
    [SerializeField] private GameObject Bg5;
    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);

    private int marbleIndex;
    private TBL.Sheet.CMarble marbleInfo;

    private GameObject idleEff;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        if (GoToShopButton) GoToShopButton.onClick.AddListener(OnClickGoToShop);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (gradeUpEff != null)
            gradeUpEff.Go.SetActive(false);
        if (param.Length <= 0)
        {
            ClosePanel();
            return;
        }

        marbleIndex = (int)param[0];
        if (Sheet.TBL.Marble.ContainsKey(marbleIndex) == false)
        {
            ClosePanel();
            return;
        }
        marbleInfo = Sheet.TBL.Marble[marbleIndex];

        SetMarbleInfo();        
    }

    private void SetMarbleInfo()
    {
        MarbleBaseInfo();
        MarbleLevelInfo();
        MarbleAttackInfo();
        MarbleAbilityInfo();
        SetDeckButton();
    }

    public override void UpdateAsset()
    {
        if (goldText)
        {
            long goldCount = BaseImpl.Inst.GetAsset(User.Inst, Net.ASSETS.ASSET_GOLD);
            goldText.text = goldCount.ToString("n0");
        }
    }

    private void MarbleBaseInfo()
    {
        if (rarityText) rarityText.text = UIUtil.GetRarityString(marbleInfo.Rarity);
        if (marbleNameText) marbleNameText.text = Sheet.Langs.Marble[marbleIndex].Name;

        bool isHave = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex);
        if (marbleImage) marbleImage.gameObject.SetActive(isHave);
        if (marbleImage_off) marbleImage_off.gameObject.SetActive(!isHave);

        if (isHave)
        {
            if (marbleImage) UIManager.Inst.SetSprite(marbleImage, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
            SetIdleEffect();
        }
        else
        {
            if (idleEff) idleEff.SetActive(false);
            if (marbleImage_off) UIManager.Inst.SetSprite(marbleImage_off, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
        }



        if (Bg1) Bg1.SetActive((int)MarbleRarity.Normal == marbleInfo.Rarity);
        if (Bg2) Bg2.SetActive((int)MarbleRarity.Rare == marbleInfo.Rarity);
        if (Bg3) Bg3.SetActive((int)MarbleRarity.Epic == marbleInfo.Rarity);
        if (Bg4) Bg4.SetActive((int)MarbleRarity.Legendary == marbleInfo.Rarity);
        if (Bg5) Bg5.SetActive((int)MarbleRarity.Chronicle == marbleInfo.Rarity);


        if(descText)
        {
            var stringTableInfo = Sheet.Langs.Marble[marbleIndex];
            descText.text = stringTableInfo.Marble_Desc;
        }
    }

    private void MarbleLevelInfo()
    {
        int grade = 1;
        int count = 0;
        int nextcount = 0;
        int needgold = 0;
        bool isMaxGrade = false;
        if(User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex))
        {
            grade = User.Inst.Doc.MarbleInven[marbleIndex].Grade;
            count = User.Inst.Doc.MarbleInven[marbleIndex].Count;            
            if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleInfo.Rarity))
            {
                if (Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity].ContainsKey(grade + 1))
                {
                    needgold = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][grade + 1].NeedGold;
                    nextcount = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][grade + 1].NeedMarble;
                }
                else
                    isMaxGrade = true;
            }

            if (levelText)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), isMaxGrade ? UIUtil.GetText("UI_Common_Max") : grade.ToString());
            }
            if (marbleCount)
            {
                marbleCount.gameObject.SetActive(true);
                marbleCount.text = string.Format("{0} / {1}", count, nextcount);
            }
            if (levelGaugeGrp) levelGaugeGrp.SetActive(true);
            if (levelProgressImg && count > 0 && nextcount > 0)
                levelProgressImg.fillAmount = Mathf.Min((float)count / nextcount, 1);
            else
                levelProgressImg.fillAmount = 0;
        }
        else
        {
            if (levelGaugeGrp) levelGaugeGrp.SetActive(false);
            if(marbleCount) marbleCount.gameObject.SetActive(false);
            if (levelText)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = UIUtil.GetText("UI_Common_UnObtain");
            }
        }
        
        // grade up button set
        GameObject gradeUpOff = GradeUpButton.transform.Find("OffGrp").gameObject;
        GameObject gradeUpOn = GradeUpButton.transform.Find("OnGrp").gameObject;
        GameObject gradeMax = GradeUpButton.transform.Find("MaxGrp").gameObject;
        if (isMaxGrade)
        {
            gradeMax.SetActive(true);
            gradeUpOff.SetActive(false);
            gradeUpOn.SetActive(false);
            GradeUpButton.interactable = false;
            levelUpGrp.SetActive(false);
            levelUpAlarmGrp.SetActive(false);
        }
        else
        {
            if (needgold > 0 && BaseImpl.Inst.CheckAsset(User.Inst, Net.ASSETS.ASSET_GOLD, -needgold) &&
                nextcount > 0 && count >= nextcount)
            {
                gradeMax.SetActive(false);
                gradeUpOff.SetActive(false);
                gradeUpOn.SetActive(true);
                gradeUpOn.transform.Find("PriceGrp/LBGoldCount").GetComponent<Text>().text = needgold.ToString();
                Color textColor;
                ColorUtility.TryParseHtmlString("#504F4A", out textColor);
                gradeUpOff.transform.Find("PriceGrp/LBGoldCount").GetComponent<Text>().color = textColor;
                GradeUpButton.interactable = true;
                levelUpGrp.SetActive(false);
                levelUpAlarmGrp.SetActive(true);
            }
            else
            {
                gradeMax.SetActive(false);
                gradeUpOn.SetActive(false);
                gradeUpOff.SetActive(true);
                levelUpAlarmGrp.SetActive(false);
                if (needgold > 0 && BaseImpl.Inst.CheckAsset(User.Inst, Net.ASSETS.ASSET_GOLD, -needgold))
                {
                    //돈은있음
                    Color textColor;
                    ColorUtility.TryParseHtmlString("#504F4A", out textColor);
                    gradeUpOff.transform.Find("PriceGrp/LBGoldCount").GetComponent<Text>().color = textColor;
                }
                else
                {
                    Color textColor;
                    ColorUtility.TryParseHtmlString("#DD1C2F", out textColor);
                    gradeUpOff.transform.Find("PriceGrp/LBGoldCount").GetComponent<Text>().color = textColor;
                }
                if (nextcount > 0 && count >= nextcount)
                    levelUpGrp.SetActive(true);
                else
                    levelUpGrp.SetActive(false);
                gradeUpOff.transform.Find("PriceGrp/LBGoldCount").GetComponent<Text>().text = needgold.ToString();
                GradeUpButton.interactable = true;
            }
        }
    }

    public void OnClickShowStatButton()
    {
        UIManager.Inst.OpenPanel(UIPanel.MarbleStatInfoPopup2, marbleIndex);
    }

    UIParticlePlay gradeUpEff;
    public void OnClickGradeUpButton()
    {
        int grade = 1;
        int count = 0;
        int nextcount = 0;
        int needgold = 0;
        bool isMaxGrade = false;
        if (User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex))
        {
            grade = User.Inst.Doc.MarbleInven[marbleIndex].Grade;
            count = User.Inst.Doc.MarbleInven[marbleIndex].Count;
            if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleInfo.Rarity))
            {
                if (Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity].ContainsKey(grade + 1))
                {
                    needgold = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][grade + 1].NeedGold;
                    nextcount = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][grade + 1].NeedMarble;
                }
                else
                    isMaxGrade = true;
            }
        }
        else
        {
            return;
        }

        if (isMaxGrade)
        {
            return;
        }
        else if(nextcount > 0 && ((count >= nextcount) == false))
        {
            return;
        }
        else
        {
            if (needgold > 0 && (BaseImpl.Inst.CheckAsset(User.Inst, Net.ASSETS.ASSET_GOLD, -needgold) == false))
            {
                UIManager.Inst.ShowNotEnoughAssetPopup();
                return;
            }
        }
        ImplBase.Actor.Parser<MarbleLobbyGradeUp.Ack>(new MarbleLobbyGradeUp.Req() { Idx = marbleIndex }, (ack)=>
        {
            if (ack.Result == RESULT.NOT_ENOUGH_ASSET)
                UIManager.Inst.ShowNotEnoughAssetPopup();
            if(ack.Result != RESULT.OK)
            {
                return;
            }
            UpdateAsset();
            SetMarbleInfo();
            scene.Lobby.Instance.lobbyPanel.RefreshDeck();
            scene.Lobby.Instance.lobbyPanel.SetMarbleTabAlarm();
            SoundManager.Inst.PlayEffect(7);
            if(gradeUpEff == null)
            {
                gradeUpEff = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_MarbleInfoPopup_01");
                var effTranform = gradeUpEff.Go.GetComponent<Transform>();
                effTranform.localScale = effSize;
                //gradeUpEff.SetPosition(transform.position);
            }
            gradeUpEff.Go.SetActive(true);
            gradeUpEff.Play();
        });
        
    }

    private void MarbleAttackInfo()
    {
        int grade = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex) ? User.Inst.Doc.MarbleInven[marbleIndex].Grade : Sheet.TBL.Marble[marbleIndex].InitialGrade;
        var lobbyGradeTable = Sheet.TBL.Gens.LobbyGrade[marbleIndex][grade];

        if (targetText) targetText.text = UIUtil.GetSortingString(marbleInfo.Sorting, marbleInfo.HP);
        if (powerText) powerText.text = lobbyGradeTable.Attack.ToString("n0");
        if (atkSpeedText) atkSpeedText.text = lobbyGradeTable.Attack == 0 ? "0" : lobbyGradeTable.AtkDelay.ToString();
        if (dpsText) dpsText.text = (lobbyGradeTable.Attack == 0 || lobbyGradeTable.AtkDelay == 0) ? "0" : (lobbyGradeTable.Attack / lobbyGradeTable.AtkDelay).ToString("0.00");
    }

    private void MarbleAbilityInfo()
    {
        var detailDesc = Sheet.TBL.DetailDescription[marbleIndex];
        var abilityNames = Sheet.Langs.DetailDescription[marbleIndex].AbilityName;
        int grade = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex) ? User.Inst.Doc.MarbleInven[marbleIndex].Grade : Sheet.TBL.Marble[marbleIndex].InitialGrade;
        int rarity = marbleInfo.Rarity;
        for (int i = 0; i < detailDesc.AbilityId.Count; ++i)
        {
            if (i >= abilityItems.Count)
                continue;
            if (detailDesc.AbilityId[i] == 0)
            {
                abilityItems[i].SetActive(false);
                continue;
            }
            abilityItems[i].SetActive(true);
            float value = UIUtil.GetAbilityValueByValueName(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i], grade);
            MarbleAbilityItem item = new MarbleAbilityItem(abilityItems[i]);
            item.SetAbility(detailDesc.AbilityIcon[i], abilityNames[i], value, detailDesc.ValueType[i],(MarbleRarity)rarity);
        }
        int count = 0;
        for(int i = 0; i < abilityItems.Count; i++)
        {
            if (abilityItems[i].activeSelf)
                count++;
        }
        if (abilityTitle) abilityTitle.SetActive(count > 0);
    }

    private void SetDeckButton()
    {
        if(User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex))
        {
            if (EditButton) EditButton.gameObject.SetActive(true);
        }
        else
        {
            if (EditButton) EditButton.gameObject.SetActive(false);
        }
    }
    public void OnClickDeckEditBtn()
    {
        ClosePanel();
        UIManager.Inst.OpenPanel(UIPanel.MarbleDeckEditPopup, MarbleDeckEditPopup.EditType.Select, marbleIndex);
    }

    private void OnClickGoToShop()
    {
        UIManager.Inst.GoToMenu(LobbyBottomMenu.Shop);
    }

    private void SetIdleEffect()
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
            tfParticle.localScale = new Vector3(190f, 190f, 190f);
        }
    }
    public override void RefreshEffect(bool isActive)
    {
        base.RefreshEffect(isActive);
        if(idleEff) idleEff.SetActive(isActive);
    }
}

public class MarbleAbilityItem
{
    public GameObject item = null;
    public Image abilityIcon = null;
    public Text abilityNameText = null;
    public Text abilityNumText = null;

    public MarbleAbilityItem(GameObject go)
    {
        item = go;
        for(int i = 0; i < item.transform.childCount; i++)
        {
            var child = item.transform.GetChild(i);
            if (child.name.Equals("SPIcon"))
                abilityIcon = child.GetComponent<Image>();
            else if (child.name.Equals("LBStat"))
                abilityNameText = child.GetComponent<Text>();
            else if (child.name.Equals("LBStatNum"))
                abilityNumText = child.GetComponent<Text>();
        }
    }

    public void SetAbility(string iconName, string abilityName, float abilityNum, int valueType, MarbleRarity marbleRarity)
    {
        if (!string.IsNullOrEmpty(abilityName))
            abilityNameText.text = abilityName;
        string str = "";
        float val = Mathf.Abs(abilityNum);
        switch (valueType)
        {
            case 1:
                str = val.ToString("0.0");
                break;
            case 2:
                str = string.Format("{0:0.00}%", val * 100f);
                break;
            case 3:
                str = string.Format("{0:0.00}%", val / 100f);
                break;
            case 4:
                str = string.Format(UIUtil.GetText("UI_Common_Second"), val );
                break;
            case 5:
                str = val.ToString("0");
                break;
            case 6:
                str = val.ToString("0.00");
                break;
            default:
                str = val.ToString("0.0");
                break;
        }
        abilityNumText.text = str;

        UIManager.Inst.SetSprite(abilityIcon, UIManager.AtlasName.UILobby, iconName);
        var color = UIManager.Inst.UIData.GetMarbleStatRarityColor(marbleRarity);
        var gradient = abilityIcon.gameObject.GetComponent<UIGradient>();
        gradient.color1 = color.topColor;
        gradient.color2 = color.BottomColor;
    }
}


