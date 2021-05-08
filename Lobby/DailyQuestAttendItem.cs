using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;
using Net.Api;
public class DailyQuestAttendItem : UIBaseGrp
{
    [SerializeField] GameObject rewardGrp;
    [SerializeField] Image rewardIconImage;
    [SerializeField] GameObject marbleGrp;
    [SerializeField] Image marbleIconImage;
    [SerializeField] Text rewardCountText;
    [SerializeField] GameObject offGrp;
    [SerializeField] GameObject timeGrp;
    [SerializeField] Text remainTimeText;
    [SerializeField] GameObject onGrp;
    [SerializeField] Button getRewardButton;
    [SerializeField] GameObject getGrp;

    int currentGroupID;
    int currentID;
    TBL.Sheet.CDailyReward dailyRewardInfo;

    private GameObject idleEff;
    private string idleEffName = "";
    public void Init(int attendGroupID)
    {
        try
        { 
            base.Init();

            currentGroupID = attendGroupID;
            if(User.Inst.Doc.Quest.AttendItems.ContainsKey(currentGroupID)==false)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            if(idleEff) idleEff.SetActive(false);
            SetItemInfo();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetItemInfo()
    {
        currentID = User.Inst.Doc.Quest.AttendItems[currentGroupID].Index;
        dailyRewardInfo = User.Inst.TBL.DailyReward[currentID];

        SetReward();
        AttendComplete();
        SetButton();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        getRewardButton.onClick.AddListener(OnClickGetRewardButton);
    }
    private void SetReward()
    {
        try
        { 
            var rewardType = UIUtil.GetRewardType(dailyRewardInfo.RewardType);
            switch(rewardType)
            {
                case REWARD_TYPE.REWARD_MARBLE:
                case REWARD_TYPE.REWARD_MARBLE_LEGEND:
                case REWARD_TYPE.REWARD_MARBLE_MYTH:
                case REWARD_TYPE.REWARD_MARBLE_NORMAL:
                case REWARD_TYPE.REWARD_MARBLE_RARE:
                case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                    if (marbleIconImage && User.Inst.TBL.Marble.ContainsKey(User.Inst.Doc.Quest.AttendItems[currentGroupID].CompleteRewardValue))
                    {
                        if (rewardGrp) rewardGrp.SetActive(false);
                        if (marbleGrp) marbleGrp.SetActive(true);
                        UIUtil.SetRewardIcon(rewardType, marbleIconImage, User.Inst.Doc.Quest.AttendItems[currentGroupID].CompleteRewardValue);
                        SetIdleEffect();
                    }
                    else
                    {
                        if (rewardGrp) rewardGrp.SetActive(true);
                        if (marbleGrp) marbleGrp.SetActive(false);
                        UIUtil.SetRewardIcon(rewardType, rewardIconImage, User.Inst.Doc.Quest.AttendItems[currentGroupID].CompleteRewardValue);
                    }
                    break;
                default:
                    if (rewardGrp) rewardGrp.SetActive(true);
                    if (marbleGrp) marbleGrp.SetActive(false);
                    UIUtil.SetRewardIcon(rewardType, rewardIconImage, User.Inst.Doc.Quest.AttendItems[currentGroupID].CompleteRewardValue);
                    break;
            }
        
            rewardCountText.text = UIUtil.GetRewardCount(rewardType, dailyRewardInfo.RewardCount).ToString();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    bool playRemainTime;
    private void SetButton()
    {
        try
        {
            var attendInfo = User.Inst.Doc.Quest.AttendItems[currentGroupID];
            if(attendInfo.GetReward)
            {
                onGrp.SetActive(true);
                offGrp.SetActive(false);
                getRewardButton.enabled = false;
                timeGrp.SetActive(false);
                return;
            }

            timeGrp.SetActive(true);

            int attendTime = 0;
            if (dailyRewardInfo.Type == 2)
            {
                int connTime = AppClient.Inst.GetConnTime();
                int rewardTime = AppClient.Inst.GetRewardTime();
                attendTime = AppClient.Inst.GetConnTime() - AppClient.Inst.GetRewardTime();
            }
            else
            {
                attendTime = AppClient.Inst.GetConnTime();
            }
            int needPlayTimeSec = 60 * dailyRewardInfo.NeedPlayTime;
            if(attendTime >= needPlayTimeSec)
            {
                //출석시간 달성
                onGrp.SetActive(true);
                offGrp.SetActive(false);
                playRemainTime = false;
                getRewardButton.enabled = true;
            }
            else
            {
                //출석시간 미달성
                onGrp.SetActive(false);
                offGrp.SetActive(true);

                playRemainTime = true;
                deltaTime = 0;
                SetRemainTime();
                getRewardButton.enabled = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetRemainTime()
    {
        try
        {
            int attendTime = 0;
            if (dailyRewardInfo.Type == 2)
            {
                int connTime = AppClient.Inst.GetConnTime();
                int rewardTime = AppClient.Inst.GetRewardTime();
                attendTime = AppClient.Inst.GetConnTime() - AppClient.Inst.GetRewardTime();
            }
            else
            {
                attendTime = AppClient.Inst.GetConnTime();
            }
            int needPlayTimeSec = 60 * dailyRewardInfo.NeedPlayTime;

            int remainTime = needPlayTimeSec - attendTime;
            remainTimeText.text = UIUtil.ConvertSecToTimeString(remainTime, 4);
            if (attendTime >= needPlayTimeSec)
            {
                //출석시간 달성
                onGrp.SetActive(true);
                offGrp.SetActive(false);
                playRemainTime = false;
                getRewardButton.enabled = true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetIdleEffect()
    {
        var marbleTable = User.Inst.TBL.Marble[User.Inst.Doc.Quest.AttendItems[currentGroupID].CompleteRewardValue];
        if (idleEff)
        {
            if (marbleTable.IdleEffect.Equals(idleEffName))
            {
                return;
            }
        }

        if (idleEff != null && marbleTable.IdleEffect.Equals(idleEffName) == false)
            Destroy(idleEff);

        if (string.IsNullOrEmpty(marbleTable.IdleEffect))
            return;
        idleEffName = marbleTable.IdleEffect;

        var go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect);
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleIconImage.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = new Vector3(120f, 120f, 120f);
        }
    }

    private void AttendComplete()
    {
        getGrp.SetActive(User.Inst.Doc.Quest.AttendItems[currentGroupID].GetReward);

    }

    float deltaTime;
    private void Update()
    {
        try
        { 
            if(playRemainTime)
            {
                if (deltaTime > 1f)
                {
                    deltaTime = 0f;
                    SetRemainTime();
                }
                else
                    deltaTime += Time.deltaTime;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }


    private void OnClickGetRewardButton()
    {
        try
        {
            int attendTime = 0;
            if (dailyRewardInfo.Type == 1)
                attendTime = AppClient.Inst.GetConnTime();
            else
                attendTime = AppClient.Inst.GetConnTime() - AppClient.Inst.GetRewardTime();

            int needPlayTimeSec = 60 * dailyRewardInfo.NeedPlayTime;
            if (attendTime < needPlayTimeSec)
            {
                return;
            }
            if (User.Inst.Doc.Quest.AttendItems[currentGroupID].GetReward)
                return;
            UIManager.Inst.ShowConnectingPanel(true);

            ImplBase.Actor.Parser<GetAttendReward.Ack>(new GetAttendReward.Req() { AttendID = currentGroupID, ConnectTime = AppClient.Inst.GetConnTime() }, (ack) =>
            {
                try
                {
                    UIManager.Inst.ShowConnectingPanel(false);
                    if (ack.Result != RESULT.OK)
                    {
                        return;
                    }
                    UIManager.Inst.ShowConnectingPanel(true);
                    AppClient.Inst.UpdateConnTime(Refresh);        // 접속 시간 동기화 처리..

                    //string rewardDesc = string.Empty;
                    //for (int i = 0; i < ack.Rewards.Count; i++)
                    //{
                    //    string rewardName = UIUtil.GetRewardName((Net.REWARD_TYPE)ack.Rewards[i].Type, ack.Rewards[i].Value);
                    //    int count = UIUtil.GetRewardCount((Net.REWARD_TYPE)ack.Rewards[i].Type, ack.Rewards[i].Count);
                    //    rewardDesc += string.Format("{0} x{1}\n", rewardName, count);
                    //}
                    //UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_GetReward"), rewardDesc);
                    void Refresh()
                    {
                        UIManager.Inst.ShowConnectingPanel(false);
                        UIManager.Inst.OpenPanel(UIPanel.CommonRewardResultPopup, ack.Rewards);
                        PlayAni("Get");
                        SetItemInfo();
                        UIManager.Inst.RefreshLobbyAsset();

                        var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UIPanel.LobbyPanel);
                        var battlePanel = (BattlePanel)lobbyPanel.GetBottomMenu(LobbyBottomMenu.Battle);
                        battlePanel.SetQuestNoti();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                }
            });
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
