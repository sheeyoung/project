using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
public class FriendsCodeCreatePopup : UIBasePanel
{
    [SerializeField] Text codeText;
    [SerializeField] Text remainTimeText;

    float remainTime;

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
        if(param.Length <= 0)
        {
            ClosePanel();
            return;
        }
        UIUtil.isCoOpFriend = true;
        string code = (string)param[0];
        codeText.text = code;

        remainTime = UIUtil.GetConstValue("CONST_COOP_FRIEND_TIME");
        remainTimeText.text = UIUtil.ConvertSecToTimeString((long)remainTime, 1);

        StartCoroutine(ShowRemainTime());
    }
    
    IEnumerator ShowRemainTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            remainTime -= 0.5f;
            if (remainTime < 0)
            {
                ClosePanel();
                yield break;
            }
            remainTimeText.text = UIUtil.ConvertSecToTimeString((long)remainTime, 1);
        }
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        // TODO: 기다리다 팝업창 끄고 나갈때 처리
        scene.Lobby.Instance.CancelJoin();
    }
    public void OnClickCopyCodeButton()
    {
        string code = codeText.text;
        UniClipboard.SetText(code);
    }
}
