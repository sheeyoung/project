using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class IngameSurrenderPopup : UIBasePanel
{
    protected override void Init(UIPanel windowId)
    {
        base.Init(windowId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
    }
    public void OnClickGiveup()
    {
        ClosePanel();
        Net.Api.GAME_RESULT res = Net.Api.GAME_RESULT.LOSE;
        if (Net.PeerNet.Inst.Mode == Game.MODE.PVP)
        {
            if (scene.GamePvP.Instance.BattleMode.IsSinglePlay || !Game.GameTick.Inst.IsRun)
                res = Net.Api.GAME_RESULT.DROP;
            else
                res = Net.Api.GAME_RESULT.LOSE;
        }
        else if (Net.PeerNet.Inst.Mode == Game.MODE.COOP)
            res = Net.Api.GAME_RESULT.WIN;
        scene.GamePvP.Instance.BattleMode.RequestGameOver(res, true, true);
    }
}
