using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class MarbleInfoTipPopup : UIBasePanel
{
    [Header("Title")]
    [SerializeField] private Text LevelText;
    [SerializeField] private Text NameText;
    [Header("Stat")]
    [SerializeField] private Text ATKText;
    [SerializeField] private Text ATKDelayText;
    [SerializeField] private Text DPSText;
    [SerializeField] private Text TargetText;
    [Header("Icon")]
    [SerializeField] private Image IconImage;
    [Header("Desc")]
    [SerializeField] private Text DescText;
    [Header("Grade")]
    [Header("Rarity")]
    [SerializeField] private Image decoImage;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image effImage;
    [SerializeField] private Image lineImage;
    [SerializeField] private Text rarityText;
    [SerializeField] private UIGradient bgGradient;

    private TBL.Sheet.CMarble marbleInfo = default;
    private GameObject idleEff;
    int marbleIndex;
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

        marbleIndex = (int)param[0];
        if (!Sheet.TBL.Marble.ContainsKey(marbleIndex))
        {
            ClosePanel();
            return;
        }

        marbleInfo = Sheet.TBL.Marble[marbleIndex];

        SetData();
        SetMarbleRarityInfo();
        SetIdleEffect();
    }

    private void SetData()
    {
        int grade = Sheet.TBL.Marble[marbleIndex].InitialGrade;

        if (LevelText) LevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), grade);
        if (NameText) NameText.text = Sheet.Langs.Marble[marbleInfo.Index].Name;

        var mergeInfo = Sheet.TBL.Gens.LobbyGrade[marbleIndex][grade];
        float atk = mergeInfo.Attack;
        if (ATKText) ATKText.text = atk.ToString("n0");
        if (ATKDelayText) ATKDelayText.text = (atk == 0) ? "0" : mergeInfo.AtkDelay.ToString("f2");
        if (DPSText) DPSText.text = string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), ((atk == 0 || mergeInfo.AtkDelay == 0) ? "0" : (atk / mergeInfo.AtkDelay).ToString("0.00")));
        if (TargetText) TargetText.text = UIUtil.GetSortingString(marbleInfo.Sorting, marbleInfo.HP);

        if (IconImage) UIManager.Inst.SetSprite(IconImage, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
        if (DescText) DescText.text = Sheet.Langs.Marble[marbleInfo.Index].Ingame_Desc;
    }
    private void SetMarbleRarityInfo()
    {
        var marbleTable = Sheet.TBL.Marble[marbleInfo.Index];
        var rarityInfo = UIManager.Inst.UIData.GetMarbleRarityColorInfo((UI.MarbleRarity)marbleTable.Rarity);

        if (decoImage) decoImage.color = rarityInfo.DecoColor;
        if (bgImage) bgImage.color = rarityInfo.BGColor;
        if (effImage) effImage.color = rarityInfo.EffColor;
        if (lineImage) lineImage.color = rarityInfo.LineColor;
        if (rarityText)
        {
            rarityText.color = rarityInfo.RarityTextColor;
            rarityText.text = UIUtil.GetRarityString(marbleTable.Rarity);
        }


        var color = UIManager.Inst.UIData.GetMarbleStatRarityColor((UI.MarbleRarity)marbleTable.Rarity);
        bgGradient.color1 = color.topColor;
        bgGradient.color2 = color.BottomColor;
    }
    public override void ClosePanel()
    {
        //base.ClosePanel();
        gameObject.SetActive(false);
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
            idleEff.transform.SetParent(IconImage.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = new Vector3(160f, 160f, 160f);
        }
    }
}
