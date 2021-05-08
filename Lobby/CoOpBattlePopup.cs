using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
using Net.Impl;
using System;
using Libs.Utils;

public class CoOpBattlePopup : UIBasePanel
{
    [SerializeField] Button teamBattleButton;
    [SerializeField] Button friendTeamBattleButton;
    [SerializeField] GameObject countGrp;
    [SerializeField] Text teamCountText;
    [SerializeField] GameObject timeGrp;
    [SerializeField] Text timeText;
    [SerializeField] GameObject adGrp;
    [SerializeField] Button adButton;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        adButton.onClick.AddListener(OnClickADButton);
        teamBattleButton.onClick.AddListener(OnClickCoOpButton);
        friendTeamBattleButton.onClick.AddListener(OnClickFriendButton);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        UIUtil.isCoOpFriend = false;
        if (adGrp) adGrp.SetActive(false);
        if (BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_COOP_TICKET) > 0)
        {
            if (countGrp) countGrp.SetActive(true);
            if (timeGrp) timeGrp.SetActive(false);
            if (teamCountText)
            {
                long coopMax = User.Inst.TBL.Assets[(int)ASSETS.ASSET_COOP_TICKET].MaxValue;
                long currentCoopCount = BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_COOP_TICKET);
                if (currentCoopCount < 0)
                    currentCoopCount = 0;
                teamCountText.text = string.Format("{0}/{1}", currentCoopCount, coopMax);
            }
        }
        else
        {
            if (countGrp) countGrp.SetActive(false);
            if (timeGrp) timeGrp.SetActive(true);

            long now = TimeLib.Seconds;
            DateTime nowDate = TimeLib.Now;
            DateTime yesterDay = nowDate.AddDays(1);
            yesterDay = new DateTime(yesterDay.Year, yesterDay.Month, yesterDay.Day, 0, 0, 0);
            var yesterSec = TimeLib.ConvertTo(yesterDay);
            long genTime = yesterSec - now;
            if (genTime < 0)
                genTime = 0;
            if (timeText) timeText.text = UIUtil.ConvertSecToTimeString(genTime, 1);

            if (adGrp)
            {
                if (BaseImpl.Inst.GetAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET) > 0)
                {
                    adGrp.SetActive(true);
                }
                else
                {
                    adGrp.SetActive(false);
                }
            }
        }
    }
    public void OnClickCoOpButton()
    {
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_COOP_TICKET, -1))
        {
            //협동전 횟수 잇음
            scene.Lobby.Instance.ReqQuickBattleJoin(Game.MODE.COOP);
        }
        else
        {
            //협동전 횟수 없음
            if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1))
            {
                //광고해서 협동전 가능
                UIManager.Inst.ShowAD("AddCoOpCount", Success, null, NotLoad);
                void Success()
                {
                    UIManager.Inst.ShowConnectingPanel(true);
                    ImplBase.Actor.Parser<AddCoOpTicket.Ack>(new AddCoOpTicket.Req(), (ack) =>
                    {
                        UIManager.Inst.ShowConnectingPanel(false);
                        if (ack.Result != RESULT.OK)
                        {
                            return;
                        }
                    });
                }
                void NotLoad()
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
                }
            }
            else
            {
                //광고티켓도없음
                if (Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1) == false)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
                    return;
                }

            }
        }
    }
    private void OnClickFriendButton()
    {
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_COOP_TICKET, -1))
        {
            //협동전 횟수 잇음
            //scene.Lobby.Instance.ReqQuickBattleJoin(Game.MODE.COOP);
            UIManager.Inst.OpenPanel(UIPanel.FriendsBattlePopup);
        }
        else
        {
            //협동전 횟수 없음
            if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1))
            {
                //광고해서 협동전 가능
                UIManager.Inst.ShowAD("AddCoOpCount", Success, null, NotLoad);
                void Success()
                {
                    UIManager.Inst.ShowConnectingPanel(true);
                    ImplBase.Actor.Parser<AddCoOpTicket.Ack>(new AddCoOpTicket.Req(), (ack) =>
                    {
                        UIManager.Inst.ShowConnectingPanel(false);
                        if (ack.Result != RESULT.OK)
                        {
                            return;
                        }
                    });
                }
                void NotLoad()
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
                }
            }
            else
            {
                //광고티켓도없음
                if (Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1) == false)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
                    return;
                }

            }
        }
    }
    private void OnClickADButton()
    {
        if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_COOP_TICKET, -1))
        {
            //협동전 횟수 잇음
            adGrp.SetActive(false);
            return;
        }
        else
        {
            //협동전 횟수 없음
            if (BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1))
            {
                //광고해서 협동전 가능
                UIManager.Inst.ShowAD("AddCoOpCount", Success, null, NotLoad);
                void Success()
                {
                    UIManager.Inst.ShowConnectingPanel(true);
                    ImplBase.Actor.Parser<AddCoOpTicket.Ack>(new AddCoOpTicket.Req(), (ack) =>
                    {
                        UIManager.Inst.ShowConnectingPanel(false);
                        if (ack.Result != RESULT.OK)
                        {
                            return;
                        }
                    });
                }
                void NotLoad()
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_Load"));
                }
            }
            else
            {
                //광고티켓도없음
                if (Net.Impl.BaseImpl.Inst.CheckAsset(User.Inst, ASSETS.ASSET_AD_COOP_TICKET, -1) == false)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Ok"), UIUtil.GetText("UI_Error_AD_None"));
                    return;
                }

            }
        }
    }

}
