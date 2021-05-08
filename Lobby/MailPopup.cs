using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net.Api;
using Net;
using Libs.Utils;
using System;
using System.Threading.Tasks;
using System.Threading;
using Platform;
using UI;

public class MailPopup : UIBasePanel
{
    //[SerializeField] Button closeButton;
    [SerializeField] GameObject content;
    [SerializeField] GameObject listItemObject;
    [SerializeField] Button allRecvButton;
    [SerializeField] GameObject allRecvOnGrp;
    [SerializeField] GameObject allRecvOffGrp;
    [SerializeField] Button termsButton;
    [SerializeField] GameObject emptyGrp;

    List<MailItem> mailItems;
    List<Inbox> inboxInfoItems;
    public override void OpenPanel(UIPanel panel, params object[] data)
    {
        base.OpenPanel(panel, data);

        RefreshList(RefreshEndEvent);
        var lang = AppClient.Inst.Language;
#if !UNITY_EDITOR
            lang = hive.Configuration.getHiveCountry();
#endif
        lang = lang.ToLower();
        Debug.Log("lang Code : " + lang);
        if (lang.Equals("ko") == false && lang.Equals("kr") == false)
        {
            if (termsButton) termsButton.gameObject.SetActive(false);
        }
        else
        {
            if (termsButton) termsButton.gameObject.SetActive(true);
        }
        UIManager.Inst.ShowConnectingPanel(true);
        void RefreshEndEvent()
        {
            UIManager.Inst.ShowConnectingPanel(false);
            OnClickAllReadMailButton();
        }

    }
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        if (mailItems == null)
            mailItems = new List<MailItem>();
        if (inboxInfoItems == null)
            inboxInfoItems = new List<Inbox>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();

        if (closeButton) closeButton.onClick.AddListener(OnClickClosePanel);
        if (allRecvButton) allRecvButton.onClick.AddListener(OnClickAllRecvMailButton);
        if (termsButton) termsButton.onClick.AddListener(OnClickTermsButton);
    }
    public void RefreshList(Action endEvent)
    {
        AppImpl.Parse<InboxGet.Ack>(new InboxGet.Req(), (ack) =>
        {
            Debug.Log(JsonLib.Encode(ack));
            if (ack.IsSuccess == false)
                return;
            SetMailList(ack.List);
            endEvent?.Invoke();
        });
    }
    private void SetMailList(List<Inbox> inBoxList)
    {
        //if (emptyGrp)
        //    emptyGrp.SetActive(inBoxList.Count == 0);
        inBoxList.Sort(sortMailList);
        int sortMailList(Inbox a, Inbox b)
        {
            if (a.State == INBOX_STATE.READ && b.State == INBOX_STATE.WAIT)
                return 1;
            else if (a.State == INBOX_STATE.WAIT && b.State == INBOX_STATE.READ)
                return -1;
            if (a.SendDate < b.SendDate)
                return 1;
            else if (a.SendDate > b.SendDate)
                return -1;
            return 0;
        }

        for (int i = 0; i < mailItems.Count; i++)
        {
            mailItems[i].gameObject.SetActive(false);
        }
        inboxInfoItems.Clear();

        int allRecvCount = 0;
        int mailCount = 0;
        for (int i = 0; i < inBoxList.Count; i++)
        {
            if(inBoxList[i].State == INBOX_STATE.READ)
            {
                if(inBoxList[i].Rewards != null)
                    continue;
                var deleteDate = TimeLib.ConvertTo(inBoxList[i].SendDate).AddDays(7);
                var remainDate = deleteDate - TimeLib.Now;
                int remainDay = remainDate.Days;
                if (remainDay <= 1)
                    continue;
            }
            if (mailItems.Count <= i)
            {
                GameObject go = Instantiate(listItemObject, Vector3.zero, Quaternion.identity, content.transform);
                go.GetComponent<RectTransform>().localScale = Vector3.one;
                go.GetComponent<RectTransform>().localPosition = Vector3.zero;
                var listItem = go.GetComponent<MailItem>();
                mailItems.Add(listItem);
                listItem.InitItem(GetReward);
            }
            inboxInfoItems.Add(inBoxList[i]);
            mailItems[i].Init(inBoxList[i]);
            if (inBoxList[i].State == INBOX_STATE.WAIT)
            {
                bool isRewardBox = false;
                bool isCoopBox = false;
                if (inBoxList[i].Rewards == null)
                {
                    mailCount++;
                    continue;
                }
                for (int rewardCount = 0; rewardCount < inBoxList[i].Rewards.Count; rewardCount++)
                {
                    var item = inboxInfoItems[i].Rewards[rewardCount];
                    if ((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX
                        || (REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX_EXPAND)
                    {
                        if (User.Inst.TBL.Box.ContainsKey(item.Value))
                        {
                            var boxTableInfo = User.Inst.TBL.Box[item.Value];
                            if(string.IsNullOrEmpty(boxTableInfo.Open_Effect) == false)
                                isRewardBox = true;
                        }
                    }
                    else if ((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_COOPBOX)
                    {
                        int coopBoxCount = 0;
                        if (User.Inst.Doc.BoxSlot.ContainsKey(item.Value))
                        {
                            coopBoxCount = User.Inst.Doc.BoxSlot[item.Value].BoxCount;
                        }
                        if (User.Inst.TBL.COOP.ContainsKey(item.Value))
                        {
                            var coopTableInfo = User.Inst.TBL.BOX_Slot[item.Value];
                            isCoopBox = coopBoxCount >= coopTableInfo.MaxCount;
                        }
                    }
                }
                if (inBoxList[i].Rewards.Count!=0 && isRewardBox == false && isCoopBox == false)
                    allRecvCount++;
            }
            mailCount++;
        }
        if (emptyGrp)
            emptyGrp.SetActive(mailCount == 0);
        if (allRecvCount <= 0)
        {
            if (allRecvOnGrp) allRecvOnGrp.SetActive(false);
            if (allRecvOffGrp) allRecvOffGrp.SetActive(true);
            if (allRecvButton) allRecvButton.interactable = false;
        }
        else
        {
            if (allRecvOnGrp) allRecvOnGrp.SetActive(true);
            if (allRecvOffGrp) allRecvOffGrp.SetActive(false);
            if (allRecvButton) allRecvButton.interactable = true;
        }
    }

    private void GetReward(string GID)
    {
        var gid = GID;

        var inboxItem = inboxInfoItems.Find((x) => { return x.GID.Equals(GID); });
        bool isRewardBox = false;
        bool isCoopBox = false;
        int rewardBoxIndex = 0;
        for (int rewardCount = 0; rewardCount < inboxItem.Rewards.Count; rewardCount++)
        {
            var item = inboxItem.Rewards[rewardCount];
            if ((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX
                || (REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX_EXPAND)
            {
                rewardBoxIndex = item.Value;
                isRewardBox = true;
            }
            else if((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_COOPBOX)
            {
                isCoopBox = true;
                int coopBoxCount = 0;
                if (User.Inst.Doc.BoxSlot.ContainsKey(item.Value))
                {
                    coopBoxCount = User.Inst.Doc.BoxSlot[item.Value].BoxCount;
                }
                if (User.Inst.TBL.COOP.ContainsKey(item.Value))
                {
                    var coopTableInfo = User.Inst.TBL.BOX_Slot[item.Value];
                    isCoopBox = coopBoxCount >= coopTableInfo.MaxCount;
                }


            }
        }

        if(isCoopBox)
        {
            Debug.Log("CoOp Box is Full");
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_CoopBox_Limit"), UIUtil.GetText("UI_Common_Ok"));
            return;
        }

        var req = new InboxAccept.Req() { GID = gid };
        UIManager.Inst.ShowConnectingPanel(true);
        AppImpl.Parse<InboxAccept.Ack>(req, (ack) =>
        {
            Debug.Log(JsonLib.Encode(ack));
            UIManager.Inst.ShowConnectingPanel(false);

            if (ack.IsSuccess == false)
                return;
            SetMailList(ack.List);

            RewardGains rewardGains = new RewardGains();
            foreach (var item in ack.Gains.Values)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    rewardGains.Add(item[i]);
                }
            }
            if (isRewardBox)
            {
                var boxInfo = User.Inst.TBL.Box[rewardBoxIndex];
                if (string.IsNullOrEmpty(boxInfo.Open_Effect) == false)
                {
                    //병오픈 연출보여주기
                    REWARD_TYPE marbleRewardType = REWARD_TYPE.REWARD_MARBLE;
                    foreach (var item in rewardGains)
                    {
                        if (item.Type == (int)REWARD_TYPE.REWARD_MARBLE
                        || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_LEGEND
                        || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_MYTH
                           || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_NORMAL
                           || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_RARE
                           || item.Type == (int)REWARD_TYPE.REWARD_MARBLE_UNCOMMON)
                        {
                            if (User.Inst.TBL.Marble.ContainsKey(item.Value))
                            {
                                var rewardTableInfo = User.Inst.TBL.Marble[item.Value];
                                if (marbleRewardType != REWARD_TYPE.REWARD_MARBLE_MYTH && rewardTableInfo.Rarity == (int)MarbleRarity.Legendary)
                                    marbleRewardType = REWARD_TYPE.REWARD_MARBLE_LEGEND;
                                else if (rewardTableInfo.Rarity == (int)MarbleRarity.Chronicle)
                                    marbleRewardType = REWARD_TYPE.REWARD_MARBLE_MYTH;
                            }
                        }
                    }

                    UIManager.PanelEndAction end = EndAction;
                    UIManager.Inst.OpenPanel(UIPanel.BoxOpenPopup, boxInfo.Open_Effect, end, marbleRewardType);
                    void EndAction()
                    {
                        UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, rewardGains);
                    }
                }
                else
                {
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, rewardGains);
                }
            }
            else
            {
                if(rewardGains.Count > 0)
                    UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, rewardGains);
            }

            int count = 0;
            for (int i = 0; i < ack.List.Count; i++)
            {
                if (ack.List[i].State == INBOX_STATE.WAIT)
                    count++;
            }
            // TODO : 로비에서 알림 체크해서 끄기
            var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
            if(lobbyPanel) lobbyPanel.RefreshInboxAlarm(count);
            UIManager.Inst.RefreshLobbyAsset();
        });
    }


    private void OnClickAllRecvMailButton()
    {
        UIManager.Inst.ShowConnectingPanel(true);

        AllRecvMail();
    }

    List<Inbox> refreshList;
    private async Task AllRecvMail()
    {
        RewardGains rewards = new RewardGains();
        bool isCoopBox = false;
        for (int i = 0; i < inboxInfoItems.Count; i++)
        {
            if (inboxInfoItems[i].State == INBOX_STATE.READ)
                continue;
            if (inboxInfoItems[i].Rewards == null)
                continue;
            bool isRewardBox = false;
            
            for (int rewardCount = 0; rewardCount < inboxInfoItems[i].Rewards.Count; rewardCount++)
            {
                var item = inboxInfoItems[i].Rewards[rewardCount];
                if ((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX
                    || (REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_BOX_EXPAND)
                {
                    if (User.Inst.TBL.Box.ContainsKey(item.Value))
                    {
                        var boxTableInfo = User.Inst.TBL.Box[item.Value];
                        if (string.IsNullOrEmpty(boxTableInfo.Open_Effect) == false)
                            isRewardBox = true;
                    }
                }
                else if ((REWARD_TYPE)item.Type == REWARD_TYPE.REWARD_COOPBOX)
                {
                    //isCoopBox = true;
                    int coopBoxCount = 0;
                    if (User.Inst.Doc.BoxSlot.ContainsKey(item.Value))
                    {
                        coopBoxCount = User.Inst.Doc.BoxSlot[item.Value].BoxCount;
                    }
                    if (User.Inst.TBL.COOP.ContainsKey(item.Value))
                    {
                        var coopTableInfo = User.Inst.TBL.BOX_Slot[item.Value];
                        isCoopBox = coopBoxCount >= coopTableInfo.MaxCount;
                    }
                }
            }
            if (isRewardBox)
                continue;
            if (isCoopBox)
                continue;
            var gains = await RecvMail(inboxInfoItems[i].GID);
            rewards.AddRange(gains);
        }
        
        UIManager.Inst.ShowConnectingPanel(false);
        if (rewards.Count <= 0)
            return;
        if (isCoopBox)
        {
            UIManager.PanelEndAction endResult = CoopBoxMaxPopup;
            UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, rewards, endResult);
        }
        else
            UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, rewards);

        SetMailList(refreshList);

        int count = 0;
        for (int i = 0; i < refreshList.Count; i++)
        {
            if (refreshList[i].State == INBOX_STATE.WAIT)
                count++;
        }
        // TODO : 로비에서 알림 체크해서 끄기
        var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
        if(lobbyPanel) lobbyPanel.RefreshInboxAlarm(count);
        UIManager.Inst.RefreshLobbyAsset();

        void CoopBoxMaxPopup()
        {
            Debug.Log("CoOp Box is Full");
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_CoopBox_Limit"), UIUtil.GetText("UI_Common_Ok"));
        }
    }

    private async Task<RewardGains> RecvMail(string gid)
    {
        RewardGains recvAck = new RewardGains();
        var cts = new CancellationTokenSource();

        var req = new InboxAccept.Req() { GID = gid };
        AppImpl.Parse<InboxAccept.Ack>(req, (ack) =>
        {
            try
            {
                Debug.Log(JsonLib.Encode(ack));
                if (ack.IsSuccess == false)
                    return;
                refreshList = ack.List;
                if (ack.Gains != null)
                {
                    foreach (var reward in ack.Gains)
                    {
                        recvAck.AddRange(reward.Value);
                    }
                }
            }
            finally
            {
                cts.Cancel();
            }

        });

        await Wait(cts);

        return recvAck;
    }
    public async Task Wait(CancellationTokenSource cts)
    {
        try
        {
            while (cts.IsCancellationRequested == false)
                await Task.Delay(1000, cts.Token);
        }
        catch
        {
        }
        finally
        {
            cts.Dispose();
            cts = null;
        }
    }

    #region 보상없는 메일처리
    private void OnClickAllReadMailButton()
    {
        UIManager.Inst.ShowConnectingPanel(true);

        AllReadMail();
    }
    private async Task AllReadMail()
    {
        for (int i = 0; i < inboxInfoItems.Count; i++)
        {
            if (inboxInfoItems[i].State == INBOX_STATE.READ)
                continue;
            if (inboxInfoItems[i].Rewards != null)
                continue;
            await ReadMail(inboxInfoItems[i].GID);
            inboxInfoItems[i].State = INBOX_STATE.READ;
        }

        UIManager.Inst.ShowConnectingPanel(false);
        int count = 0;
        for (int i = 0; i < inboxInfoItems.Count; i++)
        {
            if (inboxInfoItems[i].State == INBOX_STATE.WAIT)
                count++;
        }
        // TODO : 로비에서 알림 체크해서 끄기
        var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
        Debug.Log("MailCount : " + count);
        if (lobbyPanel) lobbyPanel.RefreshInboxAlarm(count);
    }
    private async Task ReadMail(string gid)
    {
        var cts = new CancellationTokenSource();

        var req = new InboxRead.Req() { GID = gid };
        AppImpl.Parse<InboxRead.Ack>(req, (ack) =>
        {
            try
            {
                Debug.Log(JsonLib.Encode(ack));
                if (ack.IsSuccess == false)
                    return;

            }
            finally
            {
                cts.Cancel();
            }

        });

        await Wait(cts);
        return;
    }
    #endregion

    private void OnClickTermsButton()
    {
        Application.OpenURL("https://terms.withhive.com/terms/policy/view/M30");
    }
}
