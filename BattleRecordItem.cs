using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.UI;
using Net.Api;
public class BattleRecordItem : BaseListItem
{
    [Header("BG Group")]
    [SerializeField] GameObject teamBGGrp;
    [SerializeField] GameObject pvpBGGrp;
    [Header("NameInfo")]
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image levelIconImage;
    [Header("MarbleDeck")]
    [SerializeField] List<MarbleDeckItem> marbleDecks;
    [Header("Scroe")]
    [SerializeField] Text scoreInfoText;
    [Header("Team Score")]
    [SerializeField] GameObject teamScoreGrp;
    [SerializeField] Text teamScoreText;
    [Header("PvP Score")]
    [SerializeField] GameObject pvpWinScoroeGrp;
    [SerializeField] GameObject pvpLoseScoreGrp;
    public override void ScrollCellContent(object data)
    {
        if (data is GameOpponent == false)
            return;
        GameOpponent record = (GameOpponent)data;
        
        teamBGGrp.SetActive(false);
        pvpBGGrp.SetActive(false);

        teamScoreGrp.SetActive(false);
        pvpWinScoroeGrp.SetActive(false);
        pvpLoseScoreGrp.SetActive(false);

        if (record.Mode == 1)
        {
            // pvp
            pvpBGGrp.SetActive(true);
            if(record.Result == GAME_RESULT.WIN)
                pvpWinScoroeGrp.SetActive(true);
            else if (record.Result == GAME_RESULT.LOSE)
                pvpLoseScoreGrp.SetActive(true);
            scoreInfoText.text = UIUtil.GetText("UI_Common_Mode_PvP");
        }
        else if(record.Mode == 2)
        {
            //Team
            teamBGGrp.SetActive(true);
            teamScoreGrp.SetActive(true);
            teamScoreText.text = record.LastRound.ToString("n0");
            scoreInfoText.text = UIUtil.GetText("UI_Common_Mode_Coop");
        }

        //userinfo
        UIUtil.SetLevelIcon(record.Level, levelIconImage);
        levelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), record.Level.ToString());
        nameText.text = UIUtil.GetUserName(record.Nick);
        for (int i = 0; i < marbleDecks.Count; i++)
            marbleDecks[i].gameObject.SetActive(false);
        for (int i = 0; i < record.Deck.Count; i++)
        {
            if (marbleDecks.Count <= i)
                continue;
            marbleDecks[i].SetData(record.Deck[i].Idx, i);
        }
    }
}
