using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class IngameMarbleInfoPopup : UIBasePanel
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
    private CompDice marble = null;

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

        bool isHome = (bool)param[0];
        if (isHome)
        {
            marble = (CompDice)param[1];
            if (!Sheet.TBL.Marble.ContainsKey(marble.Desc.MergeGroup) || !User.Inst.Doc.MarbleInven.ContainsKey(marble.Desc.MergeGroup))
            {
                ClosePanel();
                return;
            }

            marbleInfo = Sheet.TBL.Marble[marble.Desc.MergeGroup];

            SetData(true);
            SetMarbleRarityInfo();
        }
        else
        {
            int marbleIdx = (int)param[1];
            if (!Sheet.TBL.Marble.ContainsKey(marbleIdx))
            {
                ClosePanel();
                return;
            }
            marbleInfo = Sheet.TBL.Marble[marbleIdx];

            SetData(false);
            SetMarbleRarityInfo();
        }
    }

    private void SetData(bool home)
    {
        int grade = home ? User.Inst.Doc.MarbleInven[marbleInfo.Index].Grade : marbleInfo.InitialGrade;

        if (LevelText)
        {
            LevelText.gameObject.SetActive(home);
            if (home)
                LevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), grade);
        }

        if (NameText) NameText.text = Sheet.Langs.Marble[marbleInfo.Index].Name;

        float atk = home ? marble.ATK : 0;
        if (ATKText) ATKText.text = home ? atk.ToString("n0") : "-";
        if (ATKDelayText) ATKDelayText.text = home ? (atk == 0) ? "0" : marble.ATK_Delay.ToString("f2") : "-";
        if (DPSText) DPSText.text = home ? string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), ((atk == 0 || marble.ATK_Delay == 0) ? "0" : (atk / marble.ATK_Delay).ToString("0.00"))) : "-";
        if (TargetText) TargetText.text = UIUtil.GetSortingString(marbleInfo.Sorting, marbleInfo.HP);

        if (IconImage) UIManager.Inst.SetSprite(IconImage, UIManager.AtlasName.MainMarble, marbleInfo.Icon);
        if (DescText) DescText.text = Sheet.Langs.Marble[marbleInfo.Index].Ingame_Desc;
    }
    private void SetMarbleRarityInfo()
    {
        var rarityInfo = UIManager.Inst.UIData.GetMarbleRarityColorInfo((MarbleRarity)marbleInfo.Rarity);

        if (decoImage) decoImage.color = rarityInfo.DecoColor;
        if (bgImage) bgImage.color = rarityInfo.BGColor;
        if (effImage) effImage.color = rarityInfo.EffColor;
        if (lineImage) lineImage.color = rarityInfo.LineColor;
        if (rarityText)
        {
            rarityText.color = rarityInfo.RarityTextColor;
            rarityText.text = UIUtil.GetRarityString(marbleInfo.Rarity);
        }
        if (bgGradient)
        {
            var color = UIManager.Inst.UIData.GetMarbleStatRarityColor((UI.MarbleRarity)marbleInfo.Rarity);
            bgGradient.color1 = color.topColor;
            bgGradient.color2 = color.BottomColor;
        }
    }
}
