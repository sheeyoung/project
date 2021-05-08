using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using Net.Api;
using Net.Impl;
using scene;
using Platform;
public class IngameResultPanel : UIBasePanel
{

    [SerializeField] private IngameResultUserInfoGrp HomeGrp;
    [SerializeField] private IngameResultUserInfoGrp AwayGrp;


    [SerializeField] private GameObject HomeWinGrp;
    [SerializeField] private GameObject AwayWinGrp;
    [SerializeField] private GameObject HomeLoseGrp;
    [SerializeField] private GameObject AwayLoseGrp;
    private GAME_RESULT gameResult;
    private int RewardPvPScore = 0;
    Dictionary<REWARD_VALUE, RewardGains> rewardConditions = new Dictionary<REWARD_VALUE, RewardGains>(new RewardValueComparer());
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        SoundManager.Inst.StopBGM();
        SoundManager.Inst.PlayEffect(18);

        if (param.Length > 0)
        {
            gameResult = (GAME_RESULT)param[0];
            rewardConditions = (Dictionary<REWARD_VALUE, RewardGains>)param[1];
            RewardPvPScore = (int)param[2];
        }

        if (gameResult == GAME_RESULT.WIN)
            HomeWin();
        else if (gameResult == GAME_RESULT.LOSE)
            HomeLose();
        else if (gameResult == GAME_RESULT.DROP)
            HomeDrop();
        HomeGrp.Init(GamePvP.Instance.BattleMode.Home, true, RewardPvPScore);
        AwayGrp.Init(GamePvP.Instance.BattleMode.Away, false);
        SetSingular();
    }
    private void SetSingular()
    {
        try
        {
            long total = User.Inst.Doc.PvP.Record.Join + User.Inst.Doc.Team.Record.Join;
            if (total == 1)
                Auth.Inst.SendSingularEvent("Tutorial complete");
            else if (total == 2)
                Auth.Inst.SendSingularEvent("Play2");
            else if (total == 3)
                Auth.Inst.SendSingularEvent("Play3");
            else if (total == 4)
                Auth.Inst.SendSingularEvent("Play4");
            else if(total == 5)
                Auth.Inst.SendSingularEvent("Play5");
            else if(total == 6)
                Auth.Inst.SendSingularEvent("Play6");
            else if (total == 7)
                Auth.Inst.SendSingularEvent("Play7");
            else if (total == 8)
                Auth.Inst.SendSingularEvent("Play8");
            else if (total == 9)
                Auth.Inst.SendSingularEvent("Play9");
            else if (total == 10)
                Auth.Inst.SendSingularEvent("Play10");
            else if (total == 20)
                Auth.Inst.SendSingularEvent("Play20");
            else if (total == 50)
                Auth.Inst.SendSingularEvent("Play50");

            if(User.Inst.Doc.PvP.Record.Join == 1)
                Auth.Inst.SendSingularEvent("Fast1");
            else if (User.Inst.Doc.PvP.Record.Join == 2)
                Auth.Inst.SendSingularEvent("Fast2");
            else if (User.Inst.Doc.PvP.Record.Join == 3)
                Auth.Inst.SendSingularEvent("Fast3");
            else if (User.Inst.Doc.PvP.Record.Join == 4)
                Auth.Inst.SendSingularEvent("Fast4");
            else if (User.Inst.Doc.PvP.Record.Join == 5)
                Auth.Inst.SendSingularEvent("Fast5");
            else if (User.Inst.Doc.PvP.Record.Join == 6)
                Auth.Inst.SendSingularEvent("Fast6");
            else if (User.Inst.Doc.PvP.Record.Join == 7)
                Auth.Inst.SendSingularEvent("Fast7");
            else if (User.Inst.Doc.PvP.Record.Join == 8)
                Auth.Inst.SendSingularEvent("Fast8");
            else if (User.Inst.Doc.PvP.Record.Join == 9)
                Auth.Inst.SendSingularEvent("Fast9");
            else if (User.Inst.Doc.PvP.Record.Join == 10)
                Auth.Inst.SendSingularEvent("Fast10");
            else if (User.Inst.Doc.PvP.Record.Join == 20)
                Auth.Inst.SendSingularEvent("Fast20");
            else if (User.Inst.Doc.PvP.Record.Join == 50)
                Auth.Inst.SendSingularEvent("Fast50");
        }
        catch(System.Exception ex)
        {
            Debug.Log(ex);
        }
    }

    private void HomeWin()
    {
        HomeWinGrp.gameObject.SetActive(true);
        HomeLoseGrp.gameObject.SetActive(false);
        AwayWinGrp.gameObject.SetActive(false);
        AwayLoseGrp.gameObject.SetActive(true);
    }

    private void HomeLose()
    {
        HomeWinGrp.gameObject.SetActive(false);
        HomeLoseGrp.gameObject.SetActive(true);
        AwayWinGrp.gameObject.SetActive(true);
        AwayLoseGrp.gameObject.SetActive(false);
    }

    private void HomeDrop()
    {
        HomeWinGrp.gameObject.SetActive(false);
        HomeLoseGrp.gameObject.SetActive(true);
        AwayWinGrp.gameObject.SetActive(false);
        AwayLoseGrp.gameObject.SetActive(true);
    }


    public void OnClickGoToLobby()
    {
        ClosePanel();        
    }

    public override void ClosePanel()
    {
        if (gameResult == GAME_RESULT.WIN)
        {
            UIManager.Inst.OpenPanel(UIPanel.IngameResultWinPanel, rewardConditions, RewardPvPScore);            
            base.ClosePanel();
        }
        else
            GamePvP.Instance.BattleMode.LeaveRoom();        
    }
}
