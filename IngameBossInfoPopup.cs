using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Libs.Unity;

public class IngameBossInfoPopup : UIBasePanel
{
    [Header("Boss")]
    [SerializeField] private Image BossIcon;
    [SerializeField] private Text BossName;        
    [Header("Ability")]
    [SerializeField] private GameObject[] BossAbilities;

    private int bossIndex = 0;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (param.Length <= 0)
        {
            ClosePanel();
            return;
        }

        bossIndex = (int)param[0];
        SetBoss();
        SetAbilityDesc();
    }

    private void SetBoss()
    {
        var bossInfo = Sheet.TBL.Monster[bossIndex];
        var bossLangInfo = Sheet.Langs.Monster[bossIndex];
        if (BossIcon) UIManager.Inst.SetSprite(BossIcon, UIManager.AtlasName.Enemy, bossInfo.Icon);
        if (BossName) BossName.text = bossLangInfo.Name;
    }

    private void SetAbilityDesc()
    {
        List<int> abilities = new List<int>();
        var bossInfo = Sheet.TBL.Monster[bossIndex];
        for (int i = 0; i < bossInfo.BaseAbilityId.Count; ++i)
        {
            if (bossInfo.BaseAbilityId[i] != 0)
                abilities.Add(bossInfo.BaseAbilityId[i]);
        }
        for (int i = 0; i < bossInfo.SequenceAbilityId.Count; ++i)
        {
            if (bossInfo.SequenceAbilityId[i] != 0 && !abilities.Contains(bossInfo.SequenceAbilityId[i]))
                abilities.Add(bossInfo.SequenceAbilityId[i]);
        }
        for (int i = 0; i < abilities.Count; ++i)
        {
            BossAbilities[i].SetActive(true);
            BossAbilityInfoItem item = new BossAbilityInfoItem(BossAbilities[i]);            
            item.SetBossAbilityInfo(Sheet.Langs.MonsterAbility[abilities[i]].Name, Sheet.Langs.MonsterAbility[abilities[i]].Desc);
        }
        if (BossAbilities.Length > abilities.Count)
        {
            for (int i = abilities.Count; i < BossAbilities.Length; ++i)
                BossAbilities[i].SetActive(false);
        }
        else if (BossAbilities.Length < abilities.Count)
        {
            Debug.LogErrorFormat("Need BossAbilities Object.......Ability count : {0}", abilities.Count);
        }
    }
}

public class BossAbilityInfoItem
{
    public GameObject item = null;
    public Text statName = null;
    public Text statDesc = null;

    public BossAbilityInfoItem(GameObject go)
    {
        item = go;
        statName = Utils.GetChildScript<Text>("LBStat", item.transform);
        statDesc = Utils.GetChildScript<Text>("LBDesc", item.transform);
    }

    public void SetBossAbilityInfo(string statname, string statdesc)
    {
        statName.text = statname;
        statDesc.text = statdesc;
    }
}

