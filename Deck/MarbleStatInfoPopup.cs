using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Libs.Unity;

public class MarbleStatInfoPopup : UIBasePanel
{
    enum StatType
    {
        Grade,
        Upgrade,
        Merge,
    }
    StatType currentStatType;
    [Header("Tap")]
    [SerializeField] private Button GradeButton;
    [SerializeField] private Button UpgradeButton;
    [SerializeField] private Button MergeButton;
    [Header("ButtonEffect")]
    [SerializeField] private GameObject[] GradeBtnEff;
    [SerializeField] private GameObject[] UpgradeBtnEff;
    [SerializeField] private GameObject[] MergeBtnEff;
    [Header("Panel")]
    [SerializeField] private GameObject GradePanel;
    [SerializeField] private GameObject UpgradePanel;
    [SerializeField] private GameObject MergePanel;
    [Header("DPS")]
    [SerializeField] private Text GradeDPS;
    [SerializeField] private Text UpgradeDPS;
    [SerializeField] private Text MergeDPS;
    [Header("AblilityInfo")]
    [SerializeField] private GameObject[] gradeItems;
    [SerializeField] private GameObject[] upgradeItems;
    [SerializeField] private GameObject[] mergeItems;
    [Header("Grade - Button")]
    [SerializeField] private Button gradeLeftButton;
    [SerializeField] private Button gradeRightButton;
    [Header("Upgrade - Button")]
    [SerializeField] private Button upgradeLeftButton;
    [SerializeField] private Button upgradeRightButton;
    [Header("Merge - Button")]
    [SerializeField] private Button mergeLeftButton;
    [SerializeField] private Button mergeRightButton;
    [Header("Add CriDam")]
    [SerializeField] private GameObject SpecialStat;
    [SerializeField] private Text SpecialStatText;

    private int marbleIndex;
    private TBL.Sheet.CMarble marbleInfo;
    private int marbleMinGrade = 0;
    private int marbleGrade = 0;
    private int marbleUpGrade = 0;
    private int marbleMerge = 0;

    private bool[] initPanels = new bool[3] { false, false, false };

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        gradeLeftButton.onClick.AddListener(() => { OnClickGradeUPDownButton(true); });
        gradeRightButton.onClick.AddListener(() => { OnClickGradeUPDownButton(false); });

        upgradeLeftButton.onClick.AddListener(() => { OnClickUpGradeUPDownButton(true); });
        upgradeRightButton.onClick.AddListener(() => { OnClickUpGradeUPDownButton(false); });

        mergeLeftButton.onClick.AddListener(() => { OnClickMergeUPDownButton(true); });
        mergeRightButton.onClick.AddListener(() => { OnClickMergeUPDownButton(false); });

    }

    private bool IsMax(StatType type)
    {
        bool isMax = false;
        switch (type)
        {
            case StatType.Grade:
                {
                    int nextcri = 0;
                    if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleInfo.Rarity))
                    {
                        if (Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity].ContainsKey(marbleGrade + 1))
                            nextcri = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][marbleGrade + 1].CriticalDamage;
                        else
                            isMax = true;
                    }
                }
                break;
            case StatType.Upgrade:
                {
                    if (marbleUpGrade >= Sheet.TBL.Const["CONST_MARBLE_MAX_LEVEL"].Value)
                        isMax = true;
                }
                break;
            case StatType.Merge:
                {
                    if (marbleMerge >= Sheet.TBL.Const["CONST_MARBLE_MAX_MERGE"].Value)
                        isMax = true;
                }
                break;
        }

        return isMax;
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);

        if (param.Length <= 0)
        {
            ClosePanel();
            return;
        }

        marbleIndex = (int)param[0];
        if (!Sheet.TBL.Marble.ContainsKey(marbleIndex))
        {
            ClosePanel();
            return;
        }
        marbleInfo = Sheet.TBL.Marble[marbleIndex];
        var marble = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex) ? User.Inst.Doc.MarbleInven[marbleIndex] : null;
        marbleMinGrade = marble == null ? marbleInfo.InitialGrade : marble.Grade;
        marbleGrade = marbleMinGrade;
        marbleUpGrade = 1;
        marbleMerge = 1;

        for (int i = 0; i < initPanels.Length; ++i)
            initPanels[i] = false;

        bool isMax = IsMax(StatType.Grade);
        gradeLeftButton.gameObject.SetActive(marbleGrade > marbleMinGrade);
        gradeRightButton.gameObject.SetActive(!isMax);
        InitGradePanel(isMax);
        AllButtonEffectOff();
        GradeBtnEff[1].SetActive(true);

        if (UpgradePanel != null) UpgradePanel.SetActive(false);
        if (MergePanel != null) MergePanel.SetActive(false);
        if (GradePanel != null) GradePanel.SetActive(true);
        currentStatType = StatType.Grade;        
    }

    public void OnClickGradePanelButton()
    {
        if (UpgradePanel != null && UpgradePanel.activeSelf)
            UpgradePanel.SetActive(false);
        if (MergePanel != null && MergePanel.activeSelf)
            MergePanel.SetActive(false);
        if (GradePanel == null || (GradePanel != null && GradePanel.activeSelf))
            return;

        bool isMax = IsMax(StatType.Grade);
        if (!initPanels[0])
            InitGradePanel(isMax);
        currentStatType = StatType.Grade;
        AllButtonEffectOff();
        GradeBtnEff[1].SetActive(true);
        GradePanel.SetActive(true);
        gradeLeftButton.gameObject.SetActive(marbleGrade > marbleMinGrade);
        gradeRightButton.gameObject.SetActive(!isMax);
    }

    public void OnClickUpgradePanelButton()
    {
        if (GradePanel != null && GradePanel.activeSelf)
            GradePanel.SetActive(false);
        if (MergePanel != null && MergePanel.activeSelf)
            MergePanel.SetActive(false);
        if (UpgradePanel == null || (UpgradePanel != null && UpgradePanel.activeSelf))
            return;
        bool isMax = IsMax(StatType.Upgrade);
        if (!initPanels[1])
            InitUpgradePanel(isMax);
        AllButtonEffectOff();
        UpgradeBtnEff[1].SetActive(true);
        UpgradePanel.SetActive(true);
        upgradeLeftButton.gameObject.SetActive(marbleUpGrade > 1);
        upgradeRightButton.gameObject.SetActive(!isMax);
    }

    public void OnClickMergePanelButton()
    {
        if (GradePanel != null && GradePanel.activeSelf)
            GradePanel.SetActive(false);
        if (UpgradePanel != null && UpgradePanel.activeSelf)
            UpgradePanel.SetActive(false);
        if (MergePanel == null || (MergePanel != null && MergePanel.activeSelf))
            return;
        bool isMax = IsMax(StatType.Merge);
        if (!initPanels[2])
            InitMergePanel(isMax);

        AllButtonEffectOff();
        MergeBtnEff[1].SetActive(true);
        MergePanel.SetActive(true);
        mergeLeftButton.gameObject.SetActive(marbleMerge > 1);
        mergeRightButton.gameObject.SetActive(!isMax);
    }

    public void OnClickGradeUPDownButton(bool isDown)
    {
        bool isMax = IsMax(StatType.Grade);

        if (isDown)
            marbleGrade = Mathf.Max(marbleGrade - 1, marbleMinGrade);
        else
        {
            if (!isMax)
                marbleGrade++;
        }
        isMax = IsMax(StatType.Grade);
        gradeLeftButton.gameObject.SetActive(marbleGrade > marbleMinGrade);
        gradeRightButton.gameObject.SetActive(!isMax);

        InitGradePanel(isMax);
        currentStatType = StatType.Grade;        
    }

    public void OnClickUpGradeUPDownButton(bool isDown)
    {
        bool isMax = IsMax(StatType.Upgrade);

        if (isDown)
            marbleUpGrade = Mathf.Max(marbleUpGrade - 1, 1);
        else
        {
            if (!isMax)
                marbleUpGrade++;
        }
        isMax = IsMax(StatType.Upgrade);
        upgradeLeftButton.gameObject.SetActive(marbleUpGrade > 1);
        upgradeRightButton.gameObject.SetActive(!isMax);

        InitUpgradePanel(isMax);        
    }

    public void OnClickMergeUPDownButton(bool isDown)
    {
        bool isMax = IsMax(StatType.Merge);
        
        if (isDown)
            marbleMerge = Mathf.Max(marbleMerge - 1, 1);
        else
        {
            if (!isMax)
                marbleMerge++;
        }
        isMax = IsMax(StatType.Merge);
        mergeLeftButton.gameObject.SetActive(marbleMerge > 1);
        mergeRightButton.gameObject.SetActive(!isMax);

        InitMergePanel(isMax);
    }

    #region GradePanel
    private void InitGradePanel(bool ismax)
    {        
        int nextcri = 0;
        if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleInfo.Rarity))
        {
            if (Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity].ContainsKey(marbleGrade + 1))
                nextcri = Sheet.TBL.Gens.GradeUp[marbleInfo.Rarity][marbleGrade + 1].CriticalDamage;
        }

        SetMarbleGradePanel(marbleGrade, ismax);
        SetIncreaseAbilityByGrade(ismax);
        SetAddedCriDam(nextcri, ismax);
        initPanels[0] = true;        
    }

    private void SetMarbleGradePanel(int grade, bool ismax)
    {
        GameObject maxGrp = Utils.FindObject("MaxGrp", GradePanel.transform);
        GameObject normalGrp = Utils.FindObject("NormalGrp", GradePanel.transform);
        maxGrp.SetActive(ismax);
        normalGrp.SetActive(!ismax);
        Transform parentPanel = ismax ? maxGrp.transform : normalGrp.transform;
        for (int i = 1; i <= 2; ++i)
        {
            string str = string.Format("Marble0{0}", i);
            Transform marble = Utils.FindTransform(str, parentPanel);
            Image marbleIcon = Utils.GetChildScript<Image>("SPMarble", marble);
            UIManager.Inst.SetSprite(marbleIcon, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
            Text level = Utils.GetChildScript<Text>("LBLevel", marble);
            string strlevel = "";
            if(i == 1)
                strlevel = string.Format(UIUtil.GetText("UI_Common_Lv"), ismax ? UIUtil.GetText("UI_Common_Max") : grade.ToString());
            else
                strlevel = string.Format(UIUtil.GetText("UI_Common_Lv"), ismax ? UIUtil.GetText("UI_Common_Max") : (grade + 1).ToString());
            level.text = strlevel;
            if (ismax)
                break;
        }
    }
    //로비 등급
    private void SetIncreaseAbilityByGrade(bool ismax)
    {
        var baseInfo = Sheet.TBL.Gens.LobbyGrade[marbleIndex][marbleGrade];        
        TBL.Sheet.CLobbyGrade nextGradeInfo = ismax ? default : Sheet.TBL.Gens.LobbyGrade[marbleIndex][marbleGrade + 1];
        int count = 0;
        string strName = "";
        float val1 = 0f, val2 = 0f;
        float atk = 0f, atkspeed = 0f;
        // Attack
        strName = UIUtil.GetText("UI_Common_Power");
        val1 = baseInfo.Attack;
        val2 = ismax ? 0 : nextGradeInfo.Attack;
        SetMarbleStatInfo(gradeItems[count++], ismax, strName, val1, val2, 1);
        atk = val1;
        // Attack Delay
        strName = UIUtil.GetText("UI_Common_AttackSpeed");
        val1 = baseInfo.AtkDelay;
        val2 = ismax ? 0 : nextGradeInfo.AtkDelay;
        SetMarbleStatInfo(gradeItems[count++], ismax, strName, val1, val2, 6);
        atkspeed = val1;
        // Cri Rate
        strName = UIUtil.GetText("UI_Common_CriticalRate");
        val1 = baseInfo.CriticalRate;
        val2 = ismax ? 0 : nextGradeInfo.CriticalRate;
        SetMarbleStatInfo(gradeItems[count++], ismax, strName, val1, val2, 3);
        // Ability
        var detailDesc = Sheet.TBL.DetailDescription[marbleIndex];
        var abilityNames = Sheet.Langs.DetailDescription[marbleIndex].AbilityName;
        for (int i = 0; i < detailDesc.AbilityId.Count; ++i)
        {
            if (detailDesc.AbilityId[i] == 0)
                continue;
            strName = abilityNames[i];
            val1 = UIUtil.GetAbilityValueByValueName(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i], marbleGrade);
            val2 = ismax ? 0 : UIUtil.GetAbilityValueByValueName(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i], marbleGrade + 1);
            SetMarbleStatInfo(gradeItems[count++], ismax, strName, val1, val2, detailDesc.ValueType[i]);
        }

        for (int i = count; i < gradeItems.Length; ++i)
            gradeItems[i].SetActive(false);

        // dps
        float dps = (atk == 0 || atkspeed == 0) ? 0 : atk / atkspeed;
        GradeDPS.text = string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), dps.ToString("0.00"));
    }

    private void SetAddedCriDam(int cridam, bool ismax)
    {
        if (ismax)
        {
            SpecialStat.SetActive(false);
        }
        else
        {
            if (!SpecialStat.activeSelf)
                SpecialStat.SetActive(true);
            SpecialStatText.text = string.Format("+{0}%", Mathf.CeilToInt(cridam * 0.01f));
        }
    }
    #endregion

    #region UpgradePanel
    private void InitUpgradePanel(bool ismax)
    {
        SetMarbleUpgradePanel();
        SetIncreaseAbilityByUpgrade(ismax);
        initPanels[1] = true;
    }
    private void SetMarbleUpgradePanel()
    {
        Transform marble = Utils.FindTransform("Marble02", UpgradePanel.transform);
        Image marbleIcon = Utils.GetChildScript<Image>("SPMarble", marble);
        UIManager.Inst.SetSprite(marbleIcon, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
        Image levelIcon = Utils.GetChildScript<Image>("SPLevelNum", marble);
        if (marbleUpGrade > 1)
        {
            levelIcon.gameObject.SetActive(true);
            UIManager.Inst.SetSprite(levelIcon, UIManager.AtlasName.UILobby, string.Format("Text_Lv0{0}", marbleUpGrade));
        }
        else
            levelIcon.gameObject.SetActive(false);

    }

    //인게임 등급
    private void SetIncreaseAbilityByUpgrade(bool ismax)
    {
        var baseInfo = Sheet.TBL.Gens.LobbyGrade[marbleIndex][marbleGrade];
        var upgradeInfo = Sheet.TBL.Gens.IngameUpgrade[marbleIndex];
        int count = 0;
        string strName = "";
        float val1 = 0f, val2 = 0f;
        float atk = 0f, atkspeed = 0f;
        // Attack
        strName = UIUtil.GetText("UI_Common_Power");
        val1 = baseInfo.Attack + (upgradeInfo.Attack * (marbleUpGrade - 1));
        val2 = ismax ? 0 : val1 + upgradeInfo.Attack;
        SetMarbleStatInfo(upgradeItems[count++], ismax, strName, val1, val2, 1);
        atk = val1;
        // Attack Delay
        strName = UIUtil.GetText("UI_Common_AttackSpeed");
        val1 = baseInfo.AtkDelay + (upgradeInfo.AtkDelay * (marbleUpGrade - 1));
        val2 = ismax ? 0 : val1 + upgradeInfo.AtkDelay;
        SetMarbleStatInfo(upgradeItems[count++], ismax, strName, val1, val2, 6);
        atkspeed = val1;
        // Cri Rate
        strName = UIUtil.GetText("UI_Common_CriticalRate");
        val1 = baseInfo.CriticalRate + (upgradeInfo.CriticalRate * (marbleUpGrade - 1));
        val2 = ismax ? 0 : val1 + upgradeInfo.CriticalRate;
        SetMarbleStatInfo(upgradeItems[count++], ismax, strName, val1, val2, 3);
        // Ability
        var detailDesc = Sheet.TBL.DetailDescription[marbleIndex];
        var abilityNames = Sheet.Langs.DetailDescription[marbleIndex].AbilityName;
        int grade = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex) ? User.Inst.Doc.MarbleInven[marbleIndex].Grade : Sheet.TBL.Marble[marbleIndex].InitialGrade;
        for (int i = 0; i < detailDesc.AbilityId.Count; ++i)
        {
            if (detailDesc.AbilityId[i] == 0)
                continue;
            strName = abilityNames[i];
            float baseVal = UIUtil.GetAbilityValueByValueName(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i], grade);
            float addVal = 0f;
            if (IsEqualMarble(marbleIndex, detailDesc.AbilityId[i]))
            {
                addVal += GetMarbleUpgradeAbilityValue(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i]);
            }
            else
            {
                if (IsEqualMarbleMergeGroup(marbleIndex, detailDesc.AbilityId[i]))
                    addVal += GetMarbleUpgradeAbilityValue(detailDesc.AbilityId[i], detailDesc.AbilityId[i], detailDesc.AbilityValue[i]);
            }
            
            val1 = baseVal + addVal * (marbleUpGrade - 1);
            val2 = ismax ? 0 : val1 + addVal; 
            SetMarbleStatInfo(upgradeItems[count++], ismax, strName, val1, val2, detailDesc.ValueType[i]);
        }

        for (int i = count; i < upgradeItems.Length; ++i)
            upgradeItems[i].SetActive(false);

        // dps
        float dps = (atk == 0 || atkspeed == 0) ? 0 : atk / atkspeed;
        UpgradeDPS.text = string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), dps.ToString("0.00"));
    }
    #endregion

    #region MergePanel
    private void InitMergePanel(bool ismax)
    {
        SetMarbleMergePanel();
        SetIncreaseAbilityByMerge(ismax);
        initPanels[2] = true;
    }
    private void SetMarbleMergePanel()
    {
        Transform marble = Utils.FindTransform("Marble02", MergePanel.transform);
        Image marbleIcon = Utils.GetChildScript<Image>("SPMarble", marble);
        UIManager.Inst.SetSprite(marbleIcon, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
        GameObject levelGrp = Utils.FindObject("LevelGrp", marble);
        if (marbleMerge > 1)
        {
            levelGrp.SetActive(true);
            var prefab = Sheet.TBL.Prefab[marbleInfo.Index];
            Image marbleLevel = Utils.GetChildScript<Image>("SPMergeNum", levelGrp.transform);
            string level = string.Format("{0}_{1}", prefab.MergeLv, marbleMerge);
            UIManager.Inst.SetSprite(marbleLevel, UIManager.AtlasName.MainMarbleNumber, level);
            Image marbleLevelShadow = Utils.GetChildScript<Image>("Shadow", levelGrp.transform);
            Color c = Color.white;
            if (ColorUtility.TryParseHtmlString(string.Format("#{0}", prefab.MergeLvShadowColor), out c))
                marbleLevelShadow.color = c;
        }
        else
        {
            levelGrp.SetActive(false);
        }
    }

    //인게임 합성
    private void SetIncreaseAbilityByMerge(bool ismax)
    {
        var baseInfo = Sheet.TBL.Gens.LobbyGrade[marbleIndex][marbleGrade];
        var mergeInfo = Sheet.TBL.Gens.IngameMerge[marbleIndex];
        int count = 0;
        string strName = "";
        float val1 = 0f, val2 = 0f;
        float atk = 0f, atkspeed = 0f;
        // Attack
        strName = UIUtil.GetText("UI_Common_Power");
        val1 = baseInfo.Attack + (mergeInfo.Attack * (marbleMerge - 1));
        val2 = ismax ? 0 : val1 + mergeInfo.Attack;
        SetMarbleStatInfo(mergeItems[count++], ismax, strName, val1, val2, 1);
        atk = val1;
        // Attack Delay
        strName = UIUtil.GetText("UI_Common_AttackSpeed");
        val1 = baseInfo.Attack == 0 ? 0 : Mathf.Max((baseInfo.AtkDelay + (mergeInfo.AtkDelay * (marbleMerge - 1))) * Sheet.TBL.MergeAD[marbleMerge].Merge_AtkDelay, 0.08f);
        val2 = ismax ? 0 : baseInfo.Attack == 0 ? 0 : Mathf.Max((baseInfo.AtkDelay + (mergeInfo.AtkDelay * marbleMerge)) * Sheet.TBL.MergeAD[marbleMerge + 1].Merge_AtkDelay, 0.08f);
        SetMarbleStatInfo(mergeItems[count++], ismax, strName, val1, val2, 6);
        atkspeed = val1;
        // Cri Rate
        strName = UIUtil.GetText("UI_Common_CriticalRate");
        val1 = baseInfo.CriticalRate + (mergeInfo.CriticalRate * (marbleMerge - 1));
        val2 = ismax ? 0 : val1 + mergeInfo.CriticalRate;
        SetMarbleStatInfo(mergeItems[count++], ismax, strName, val1, val2, 3);
        // Ability
        var detailDesc = Sheet.TBL.DetailDescription[marbleIndex];
        var abilityNames = Sheet.Langs.DetailDescription[marbleIndex].AbilityName;
        int grade = User.Inst.Doc.MarbleInven.ContainsKey(marbleIndex) ? User.Inst.Doc.MarbleInven[marbleIndex].Grade : Sheet.TBL.Marble[marbleIndex].InitialGrade;
        for (int i = 0; i < detailDesc.AbilityId.Count; ++i)
        {
            if (detailDesc.AbilityId[i] == 0)
                continue;
            strName = abilityNames[i];
            float baseVal = UIUtil.GetAbilityValueByValueName(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i], grade);
            float addVal = 0f;
            if (IsEqualMarble(marbleIndex, detailDesc.AbilityId[i]))
            {
                addVal += GetMarbleMergeAbilityValue(marbleIndex, detailDesc.AbilityId[i], detailDesc.AbilityValue[i]);
            }
            else
            {
                if(IsEqualMarbleMergeGroup(marbleIndex, detailDesc.AbilityId[i]))
                    addVal += GetMarbleMergeAbilityValue(detailDesc.AbilityId[i], detailDesc.AbilityId[i], detailDesc.AbilityValue[i]);
            }
                
            val1 = baseVal + addVal * (marbleMerge - 1);
            val2 = ismax ? 0 : val1 + addVal;
            SetMarbleStatInfo(mergeItems[count++], ismax, strName, val1, val2, detailDesc.ValueType[i]);
        }

        for (int i = count; i < mergeItems.Length; ++i)
            mergeItems[i].SetActive(false);

        // dps
        float dps = (atk == 0 || atkspeed == 0) ? 0 : atk / atkspeed;
        MergeDPS.text = string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), dps.ToString("0.00"));
    }

    private bool IsEqualMarble(int marbleId, int abilityId)
    {
        bool isequal = true;
        if (marbleId != abilityId)
        {
            if (!Sheet.TBL.Marble[marbleId].Ability.Exists(a => a == abilityId))
                isequal = false;
        }

        return isequal;
    }
    private bool IsEqualMarbleMergeGroup(int marbleId, int abilityId)
    {
        bool isequal = false;
        if (Sheet.TBL.Marble.ContainsKey(abilityId))
        {
            if (Sheet.TBL.Marble[marbleIndex].MergeGroup == Sheet.TBL.Marble[abilityId].MergeGroup)
                isequal = true;
        }

        return isequal;
    }

    private float GetMarbleUpgradeAbilityValue(int marbleId, int abilityId, string valueName)
    {
        var upgradeTable = Sheet.TBL.Gens.IngameUpgrade[marbleId];
        float val = 0f;
        for (int i = 0; i < upgradeTable.AbilityID.Count; ++i)
        {
            if (upgradeTable.AbilityID[i] == abilityId)
            {
                if (((Net.Impl.ABILITY_PROP_KEY)upgradeTable.Ability_Value[i]).ToString() == valueName)
                    val += upgradeTable.Ability_UPValue[i];
            }
        }

        return val;
    }

    private float GetMarbleMergeAbilityValue(int marbleId, int abilityId, string valueName)
    {
        var mergeTable = Sheet.TBL.Gens.IngameMerge[marbleId];
        float val = 0f;
        for (int i = 0; i < mergeTable.AbilityID.Count; ++i)
        {
            if (mergeTable.AbilityID[i] == abilityId)
            {
                if (((Net.Impl.ABILITY_PROP_KEY)mergeTable.Ability_Value[i]).ToString() == valueName)
                    val += mergeTable.Ability_UPValue[i];
            }
        }

        return val;
    }
    #endregion

    private void AllButtonEffectOff()
    {
        for (int i = 0; i < GradeBtnEff.Length; ++i)
            GradeBtnEff[i].SetActive(i == 0 ? true : false);
        for (int i = 0; i < UpgradeBtnEff.Length; ++i)
            UpgradeBtnEff[i].SetActive(i == 0 ? true : false);
        for (int i = 0; i < MergeBtnEff.Length; ++i)
            MergeBtnEff[i].SetActive(i == 0 ? true : false);
    }

    private void SetMarbleStatInfo(GameObject go, bool ismax, string strname, float val1, float val2, int valtype)
    {
        if (val1 == 0)
        {
            go.SetActive(false);
            return;
        }
        go.SetActive(true);
        MarbleStatInfoItem2 item = new MarbleStatInfoItem2(go, ismax);
        item.SetAbility(strname, val1, val2, valtype);
    }
}

public class MarbleStatInfoItem2
{
    public GameObject maxItem = null;
    public GameObject normalItem = null;
    public Text abilityNameText = null;
    public Text abilityNumText1 = null;
    public Text abilityNumText2 = null;

    public MarbleStatInfoItem2(GameObject go, bool isMax = false)
    {
        maxItem = go.transform.Find("OffGrp").gameObject;
        normalItem = go.transform.Find("OnGrp").gameObject;
        maxItem.SetActive(isMax);
        normalItem.SetActive(!isMax);
        if (isMax)
        {
            for (int i = 0; i < maxItem.transform.childCount; i++)
            {
                var child = maxItem.transform.GetChild(i);
                if (child.name.Equals("LBStat"))
                    abilityNameText = child.GetComponent<Text>();
                else if (child.name.Equals("LBStatNum"))
                    abilityNumText1 = child.GetComponent<Text>();
            }
        }
        else
        {
            for (int i = 0; i < normalItem.transform.childCount; i++)
            {
                var child = normalItem.transform.GetChild(i);
                if (child.name.Equals("LBStat"))
                    abilityNameText = child.GetComponent<Text>();
                else if (child.name.Equals("LBStatNum01"))
                    abilityNumText1 = child.GetComponent<Text>();
                else if (child.name.Equals("LBStatNum02"))
                    abilityNumText2 = child.GetComponent<Text>();
            }
        }
    }

    public void SetAbility(string abilityName, float abilityNum1, float abilityNum2, int valueType = 1)
    {
        if (abilityNameText) abilityNameText.text = abilityName;
        if (abilityNumText1) abilityNumText1.text = ConvertValue(abilityNum1, valueType);
        if (abilityNumText2)
        {
            Color c;
            if (abilityNum1 == abilityNum2)
            {
                if (ColorUtility.TryParseHtmlString("#6A6A6A", out c))
                    abilityNumText2.color = c;
            }
            else
            {
                if (ColorUtility.TryParseHtmlString("#43216E", out c))
                    abilityNumText2.color = c;
            }
            abilityNumText2.text = ConvertValue(abilityNum2, valueType);
        }
    }

    private string ConvertValue(float abilityNum, int valueType)
    {
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
                str = string.Format(UIUtil.GetText("UI_Common_Second"), val);
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
        return str;
    }
}