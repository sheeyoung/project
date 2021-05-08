using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Data;

public class IngameResultUserInfoGrp : MonoBehaviour
{
    [Header("UserInfo")]
    [SerializeField] Image UserGradeIcon;
    [SerializeField] Text UserLevelText;
    [SerializeField] Text UserNameText;
    [SerializeField] Image LeagueIcon;
    [SerializeField] Text LeagueName;
    [SerializeField] Text LeaguePercentageOfVictories;
    [SerializeField] Text TrophyCountText;

    [SerializeField] List<MarbleDeckItem> decks = new List<MarbleDeckItem>();

    public void Init(PvP pvpInfo, bool isHome, int rewardscore = 0)
    {
        if (UserGradeIcon) UIUtil.SetLevelIcon(pvpInfo.Level, UserGradeIcon);
        if (UserNameText) UserNameText.text = UIUtil.GetUserName(pvpInfo.Nick);
        if (UserLevelText) UserLevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), pvpInfo.Level);
        //if (LeagueIcon) UIUtil.SetLeagueIcon(pvpInfo.PvPScore, LeagueIcon);

        if (isHome)
            SetLeagueData((float)((User.Inst.Doc.PvP.Record != null && User.Inst.Doc.PvP.Record.Join > 0) ? (double)User.Inst.Doc.PvP.Record.Win / User.Inst.Doc.PvP.Record.Join : 0), User.Inst.Doc.PvP.Score, rewardscore);
        else
            SetLeagueData(pvpInfo.PoV, pvpInfo.PvPScore);
        
        SetDeck(pvpInfo.Slots);
        
    }

    public void SetLeagueData(float pov, long score, int rewardscore = 0)
    {
        if (LeagueIcon) UIUtil.SetLeagueIcon(score, LeagueIcon);
        if (LeagueName) LeagueName.text = UIUtil.GetLeagueName(score);
        if (LeaguePercentageOfVictories) LeaguePercentageOfVictories.text = string.Format(UIUtil.GetText("UI_Common_WinRate"), pov * 100);
        if (TrophyCountText)
        {
            Color c;
            if (rewardscore == 0)
            {
                if (ColorUtility.TryParseHtmlString("#FFFF69", out c))
                    TrophyCountText.color = c;
                TrophyCountText.text = score.ToString();
            }
            else
            {
                if (ColorUtility.TryParseHtmlString(rewardscore > 0 ? "#FFFF69" : "#F8AFD4", out c))
                    TrophyCountText.color = c;                
                TrophyCountText.text = string.Format("{0}({1}{2})", score, rewardscore > 0 ? "+" : "", rewardscore);
            }
        }
    }

    public void SetDeck(PvP.CSlots slots)
    {
        if (slots == null)
            return;

        for (int i = 0; i < decks.Count; i++)
        {
            if (slots.Count <= i)
            {
                decks[i].SetData(0, i);
                continue;
            }
            decks[i].SetData(slots[i].Idx, i);
        }

    }

}
