using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System;
using Photon.Pun;

public enum RECONNECT_POPUP_TYPE
{
    WAIT_BATTLE_START = 0,
    WAIT_RECONNECT_USER,
    DISCONNECTED_SERVER,
}

public class IngameReconnectPopup : UIBasePanel
{
    [SerializeField] private Text Title;
    [SerializeField] private Text WaitTime;
    [SerializeField] private Text Desc;
    [SerializeField] private Button Close;

    public RECONNECT_POPUP_TYPE Type { private set; get; } = RECONNECT_POPUP_TYPE.WAIT_BATTLE_START;
    public float waitingTime { private set; get; } = 0f;
    private float forceWaitingTime = 0;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (param.Length <= 0)
            return;

        Type = (RECONNECT_POPUP_TYPE)param[0];
        if (param.Length > 1)
            forceWaitingTime = (float)param[1];
        switch (Type)
        {
            case RECONNECT_POPUP_TYPE.WAIT_BATTLE_START:
                {
                    Title.text = UIUtil.GetText("UI_Battle_Popup_001");
                    waitingTime = 1;
                    forceWaitingTime = forceWaitingTime > 0 ? forceWaitingTime : (long)Sheet.TBL.Const["CONST_MATCH_LIMIT_TIME"].Value;
                    Desc.gameObject.SetActive(false);
                    Close.gameObject.SetActive(true);
                }
                break;
            case RECONNECT_POPUP_TYPE.WAIT_RECONNECT_USER:
                {
                    Title.text = UIUtil.GetText("UI_Ingame_Reconnect_Popup_000");
                    waitingTime = Sheet.TBL.Const["CONST_BREAKAWAY_WAIT_TIME"].Value;
                    Desc.gameObject.SetActive(false);
                    Close.gameObject.SetActive(false);
                }
                break;
            case RECONNECT_POPUP_TYPE.DISCONNECTED_SERVER:
                {
                    Title.text = UIUtil.GetText("UI_Common_Notice");
                    waitingTime = Sheet.TBL.Const["CONST_BREAKAWAY_WAIT_TIME"].Value;
                    Desc.gameObject.SetActive(true);
                    Desc.text = UIUtil.GetText("UI_Error_Network_Reconnect"); 
                    Close.gameObject.SetActive(true);
                }
                break;
        }
        
        SetData();
    }

    private void SetData()
    {
        if (WaitTime)
            WaitTime.text = ((int)waitingTime).ToString();
        switch (Type)
        {
            case RECONNECT_POPUP_TYPE.WAIT_BATTLE_START:
                StartCoroutine(UpdateBattleWaitingTime());
                break;
            case RECONNECT_POPUP_TYPE.WAIT_RECONNECT_USER:
                StartCoroutine(UpdateUserWaitingTime());
                break;
            case RECONNECT_POPUP_TYPE.DISCONNECTED_SERVER:
                StartCoroutine(UpdateReconnectWaitingTime());
                break;
        }        
    }

    private IEnumerator UpdateBattleWaitingTime()
    {
        while (true)
        {
            waitingTime += Time.deltaTime;
            if (WaitTime)
                WaitTime.text = ((int)waitingTime).ToString();

            if (waitingTime >= forceWaitingTime)
            {
                CancelWaitBattleStart();
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Error_MatchFail"));
            }

            yield return 0;
        }

    }

    private IEnumerator UpdateUserWaitingTime()
    {
        while (true)
        {
            waitingTime -= Time.deltaTime;
            if (waitingTime < 0)
                break;

            if (WaitTime)
                WaitTime.text = ((int)waitingTime).ToString();

            yield return 0;
        }

        if (waitingTime < 0)
        {
            scene.GamePvP.Instance.BattleMode.SinglePlay(true);
            ClosePanel();
        }
    }

    private IEnumerator UpdateReconnectWaitingTime()
    {
        while (true)
        {
            waitingTime -= Time.deltaTime;
            if (waitingTime < 0)
                break;

            if (WaitTime)
                WaitTime.text = ((int)waitingTime).ToString();

            yield return 0;
        }

        if (waitingTime < 0)
        {
            ClosePanel();
            OpenReConnectPopup();
        }
    }

    public void ResetUserWaitingTime()
    {
        waitingTime = Sheet.TBL.Const["CONST_BREAKAWAY_WAIT_TIME"].Value;
    }

    // 입장 대기 중 팝업 닫힘 버튼
    public void CancelWaitBattleStart()
    {
        ClosePanel();
        if (Type == RECONNECT_POPUP_TYPE.WAIT_BATTLE_START)
            scene.Lobby.Instance.CancelJoin();
        else if (Type == RECONNECT_POPUP_TYPE.DISCONNECTED_SERVER)
            OpenReConnectPopup();
    }

    private void OpenReConnectPopup()
    {
        var pTitle = UIUtil.GetText("UI_Common_Notice");
        var pMsg = UIUtil.GetText("UI_Error_Session_Expired");
        var pBtn = UIUtil.GetText("UI_Common_Ok");
        UIManager.Inst.ShowSystemPopup(pTitle, pMsg, endPanelEvent: new Action(delegate ()
        {
            Net.AppClient.Inst.Reset();
            PhotonNetwork.Disconnect();
            scene.SceneLoader.Inst.LoadScene("TitleScene");
        }), closebg : false);
    }

    public override void ClosePanel()
    {
        StopAllCoroutines();
        base.ClosePanel();
    }
}
