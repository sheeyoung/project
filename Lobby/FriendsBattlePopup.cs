using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
public class FriendsBattlePopup : UIBasePanel
{
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

    }
    public void OnClickCreateRoom()
    {
        scene.Lobby.Instance.CreateFriendBattle();
        //UIManager.Inst.OpenPanel(UIPanel.FriendsCodeCreatePopup);
    }
    public void OnClickJoinRoom()
    {
        UIManager.Inst.OpenPanel(UIPanel.FriendsCodeJoinPopup);
    }
}
