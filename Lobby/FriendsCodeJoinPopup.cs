using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
public class FriendsCodeJoinPopup : UIBasePanel
{
    [SerializeField] InputField codeInput;
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
        codeInput.text = "";
    }

    public void OnValueChanged()
    {
        codeInput.text = codeInput.text.ToUpper();
    }

    public void OnClickJoinButton()
    {
        string code = codeInput.text.ToUpper();
        if(string.IsNullOrEmpty(code))
        {
            return;
        }
        UIUtil.isCoOpFriend = true;
        ClosePanel();
        scene.Lobby.Instance.JoinFriendBattle(code);
    }

    public void OnClickCopyCodeButton()
    {
        codeInput.text = UniClipboard.GetText();
    }


}
