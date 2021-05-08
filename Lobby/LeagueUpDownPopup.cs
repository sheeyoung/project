using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System;
using Net.Impl;
public class LeagueUpDownPopup : UIBasePanel
{
    [Header("LeagueUp")]
    [SerializeField] GameObject leagueUpGrp;
    [SerializeField] Image leagueUpNewIconImage;
    [SerializeField] Text leagueUpNewNameText;
    [SerializeField] Image leagueUpPrevIconImage;
    [SerializeField] Text leagueUpPrevNameText;
    [SerializeField] Text upPvpScoreText;

    [Header("LeagueDown")]
    [SerializeField] GameObject leagueDownGrp;
    [SerializeField] Image leagueDownNewIconImage;
    [SerializeField] Text leagueDownNewNameText;
    [SerializeField] Image leagueDownPrevIconImage;
    [SerializeField] Text leagueDownPrevNameText;
    [SerializeField] Text downPvpScoreText;

    [Header("Time")]
    [SerializeField] float autoCloseTime = 5f;

    UIManager.PanelEndAction endAction;

    UIParticlePlay leagueUpEff;
    UIParticlePlay leagueDownEff;

    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);

        int prevLeagueId = UIUtil.GetLeagueIndex(UIUtil.PreLeagueScore);
        int currentLeaugeId = BaseImpl.Inst.GetPvPLeague(User.Inst);

        leagueUpGrp.SetActive(false);
        leagueDownGrp.SetActive(false);

        if (leagueUpEff != null) leagueUpEff.Go.SetActive(false);
        if (leagueDownEff != null) leagueDownEff.Go.SetActive(false);

        if (UIUtil.PreLeagueScore < User.Inst.Doc.PvP.Score)
        {
            //승급
            leagueUpGrp.SetActive(true);

            UIUtil.SetLeagueIcon(UIUtil.PreLeagueScore, leagueUpPrevIconImage);
            leagueUpPrevNameText.text = UIUtil.GetLeagueName(UIUtil.PreLeagueScore);

            UIUtil.SetLeagueIcon(User.Inst.Doc.PvP.Score, leagueUpNewIconImage);
            leagueUpNewNameText.text = UIUtil.GetLeagueName(User.Inst.Doc.PvP.Score);
            
            upPvpScoreText.text = User.Inst.Doc.PvP.Score.ToString("n0");

            if (leagueUpEff == null)
            {
                leagueUpEff = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_LeagueUpDownPopup_01");
                var effTranform = leagueUpEff.Go.GetComponent<Transform>();
                effTranform.localScale = effSize;
                //leagueUpEff.SetPosition(Vector3.zero);
            }
            leagueUpEff.Go.SetActive(true);
            leagueUpEff.Play();

            SoundManager.Inst.PlayUIEffect(25);
        }
        else
        {
            //강등
            leagueDownGrp.SetActive(true);

            UIUtil.SetLeagueIcon(UIUtil.PreLeagueScore, leagueDownPrevIconImage);
            leagueDownPrevNameText.text = UIUtil.GetLeagueName(UIUtil.PreLeagueScore);

            UIUtil.SetLeagueIcon(User.Inst.Doc.PvP.Score, leagueDownNewIconImage);
            leagueDownNewNameText.text = UIUtil.GetLeagueName(User.Inst.Doc.PvP.Score);
            
            downPvpScoreText.text = User.Inst.Doc.PvP.Score.ToString("n0");

            if (leagueDownEff == null)
            {
                leagueDownEff = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_LeagueUpDownPopup_02");
                var effTranform = leagueDownEff.Go.GetComponent<Transform>();
                effTranform.localScale = effSize;
                //leagueDownEff.SetPosition(Vector3.zero);
            }
            leagueDownEff.Go.SetActive(true);
            leagueDownEff.Play();

            SoundManager.Inst.PlayUIEffect(26);
        }
        endAction = null;
        if (param != null && param.Length > 0)
            endAction = (UIManager.PanelEndAction)param[0];
        StartCoroutine(AutoClose());
    }
    IEnumerator AutoClose()
    {
        Debug.Log("AutoClose");
        yield return new WaitForSecondsRealtime(autoCloseTime);
        Debug.Log("AutoClose222222");
        OnClickClosePanel();
    }
    public override void ClosePanel()
    {
        endAction?.Invoke();
        base.ClosePanel();
    }
}

