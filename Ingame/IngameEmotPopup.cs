using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Photon.Pun;
using Net;

public class IngameEmotPopup : UIBasePanel
{
    [Header("Button")]
    [SerializeField] private Button Emote1;
    [SerializeField] private Button Emote2;
    [SerializeField] private Button Emote3;
    [SerializeField] private Button Emote4;
    [SerializeField] private Button Emote5;
    [SerializeField] private Button Emote6;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
    }

    public void OnClickEmote(int index)
    {
        ClosePanel();
        scene.GamePvP.Instance.BattleMode.InGamePanel.SetEmoteIcon(true, index);
        if (!Game.GameBot.Inst.IsBot && !scene.GamePvP.Instance.BattleMode.IsSinglePlay)
            PeerNet.Inst.RaiseEvent((byte)Net.Api.EVENT_CODE.EVT_INGAME_EMOTE, index, PhotonOpt.EventOptions, PhotonOpt.SendOptions);
    }
}
