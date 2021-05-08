using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UI;
using DG.Tweening;
using scene;
using System.Threading.Tasks;

[Serializable]
public struct PlayerInfo
{
    [SerializeField] public Text Level;
    [SerializeField] public Text UserName;
    [SerializeField] public Image UserGrade;
    [SerializeField] public List<IngameMarbleItem> DiceInfo;
    [SerializeField] public GameObject Emote;
    [SerializeField] public Image EmoteIcon;
}

public class IngameMain : UIBasePanel
{
    [Header("User Game Info")]
    [SerializeField] protected Text CurrentCostText;
    [SerializeField] protected Text AddMarbleCostText;
    [SerializeField] public Transform AddMarblePos;

    [Space(15)]
    [Header("Player Info")]
    [SerializeField] protected PlayerInfo HomeInfo;
    [SerializeField] protected PlayerInfo AwayInfo;

    [Header("Emote")]
    [SerializeField] protected GameObject EmoteDisable;

    [Space(15)]
    [Header("GameTime Info")]
    [SerializeField] protected Image BossIcon;
    [SerializeField] protected Text BossSpawnTime;
    [SerializeField] protected Text BossSpawnReady;

    // away marble icon press
    private bool IsPressAwayIcon = false;

    // 이모티콘 사용 여부
    protected bool IsUseEmote = true;

    // boss index
    public int BossIndex { protected set; get; } = 0;

    // battle mode
    public BattleMode BattleMode { get { return GamePvP.Instance.BattleMode; } }

    protected int NextMarbleNeedCost = 0;

    // 마블 강화 이펙트
    protected Dictionary<int, UIParticlePlay[]> UpgradeEffects = new Dictionary<int, UIParticlePlay[]>();

    protected IEnumerator TickCurCost()
    {
        while (true)
        {
            if (BattleMode.IsGameOver)
            {
                if (BattleMode.IsTutorial)
                {
                    if (BattleMode.Tutorial.IsCheckCreateMarble)
                        BattleMode.CheckCreateMarble?.Invoke(false);
                }
                break;
            }
            CurrentCostText.text = BattleMode.Home.Cost.ToString();
            float tick = Game.GameTick.Inst.Next + 0.5f * Game.GameTick.Inst.TicksPerSecond;
            if (BattleMode.IsTutorial)
            {
                bool isShow = IsCheckCreateMarble();
                if(isShow != BattleMode.Tutorial.IsCheckCreateMarble)
                    BattleMode.CheckCreateMarble?.Invoke(isShow);
            }
            while (true)
            {
                if (Game.GameTick.Inst.Next >= tick)
                    break;
                yield return 0;
            }
        }
    }

    //상대편정보
    public void SetAwayInfo()
    {
        //nick
        var away = BattleMode.Away;
        if (away == null)
            return;
        AwayInfo.Level.text = string.Format(UIUtil.GetText("UI_Common_Lv"), away.Level);
        AwayInfo.UserName.text = UIUtil.GetUserName(away.Nick);
        UIUtil.SetLevelIcon(away.Level, AwayInfo.UserGrade);
        for (int i = 0; i < AwayInfo.DiceInfo.Count; i++)
        {
            if (away.Slots.ContainsKey(i) == false)
            {
                AwayInfo.DiceInfo[i].gameObject.SetActive(false);
                continue;
            }
            AwayInfo.DiceInfo[i].slotIdx = i;
            AwayInfo.DiceInfo[i].MarbleItemPressEvent += OnPressMarbleItem;
            AwayInfo.DiceInfo[i].gameObject.SetActive(true);
            AwayInfo.DiceInfo[i].SetItem(away.Slots[i].Idx);
            AwayInfo.DiceInfo[i].SetLevel(away.Slots[i].Upgrade);
        }

        AwayInfo.Emote.SetActive(false);
    }

    //내정보
    public void SetHomeInfo()
    {
        //nick
        var home = BattleMode.Home;
        if (home == null)
            return;
        HomeInfo.Level.text = string.Format(UIUtil.GetText("UI_Common_Lv"), home.Level);
        HomeInfo.UserName.text = UIUtil.GetUserName(home.Nick);
        UIUtil.SetLevelIcon(home.Level, HomeInfo.UserGrade);
        for (int i = 0; i < HomeInfo.DiceInfo.Count; i++)
        {
            if (home.Slots.ContainsKey(i) == false)
            {
                HomeInfo.DiceInfo[i].gameObject.SetActive(false);
                continue;
            }
            HomeInfo.DiceInfo[i].slotIdx = i;
            HomeInfo.DiceInfo[i].MarbleItemClickEvent += OnClickMarbleItem;

            HomeInfo.DiceInfo[i].gameObject.SetActive(true);

            HomeInfo.DiceInfo[i].SetItem(home.Slots[i].Idx);
            HomeInfo.DiceInfo[i].SetLevel(home.Slots[i].Upgrade);
            int costData = 0;
            if (home.Slots[i].Upgrade <= Sheet.TBL.IngameLevelUp.Count)
                costData = Sheet.TBL.IngameLevelUp[home.Slots[i].Upgrade].NeedGold;
            HomeInfo.DiceInfo[i].SetCost(costData);
        }

        HomeInfo.Emote.SetActive(false);
        SetMarbleNeedCost();
    }

    public Vector3 GetUpgradeSlotPos(int idx)
    {
        Transform slot = HomeInfo.DiceInfo[idx].transform;
        Transform child = slot.Find("MarbleGrp");
        return child != null ? child.transform.position : slot.position;
    }

    // 이모티콘
    protected void SetUseEmote()
    {
        EmoteDisable.SetActive(!IsUseEmote);
    }

    public void OnClickMarbleItem(int slotIdx)
    {
        var emptySlots = BattleMode.HomePlayer.DiceBoard.GetEmptySlots();
        if (emptySlots != null && emptySlots.Count == BattleMode.HomePlayer.DiceBoard.slotList.Count)
        {
            UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Ingame_Panel_002"));
            return;
        }
        BattleMode.HomePlayer.MarbleIngameUpgrade(slotIdx);        
        var home = BattleMode.Home;
        HomeInfo.DiceInfo[slotIdx].SetLevel(home.Slots[slotIdx].Upgrade);
        int costData = 0;
        if (home.Slots[slotIdx].Upgrade <= Sheet.TBL.IngameLevelUp.Count)
            costData = Sheet.TBL.IngameLevelUp[home.Slots[slotIdx].Upgrade].NeedGold;
        HomeInfo.DiceInfo[slotIdx].SetCost(costData);
    }

    public void OnPressMarbleItem(bool ispress, int slotIdx)
    {
        IsPressAwayIcon = ispress;
        if (IsPressAwayIcon)
            StartCoroutine(UpdateAwayMarbleInfo(slotIdx));
    }

    private IEnumerator UpdateAwayMarbleInfo(int slotIdx)
    {
        float time = Sheet.TBL.Const["CONST_INGAME_MARBLE_INFO_DELAY_OTHER"].Value;
        while (IsPressAwayIcon)
        {
            if (time >= 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                {
                    var marbleidx = BattleMode.Away.Slots[slotIdx].Idx;
                    var mergegroup = Sheet.TBL.Marble[marbleidx].MergeGroup;
                    UIManager.Inst.OpenPanelBaseCanvas(UI.UIPanel.IngameMarbleInfoPopup, null, false, mergegroup);
                }
                yield return 0;
            }
            else
                yield return new WaitForSeconds(0.1f);
        }

        var panel = UIManager.Inst.GetActivePanel<IngameMarbleInfoPopup>(UIPanel.IngameMarbleInfoPopup);
        if (panel != null)
            panel.ClosePanel();
    }

    public void OnClickEmote()
    {
        if (!Game.GameTick.Inst.IsRun || BattleMode.IsGameOver || !IsUseEmote)
            return;

        UIManager.Inst.OpenPanelBaseCanvas(UIPanel.IngameEmotPopup, null);
    }

    public void OnClickOtherEmote()
    {
        IsUseEmote = !IsUseEmote;
        SetUseEmote();
    }

    public void OnClickAddMarbleButton()
    {
        if (BattleMode.Round <= 0)
            return;

        var home = BattleMode.Home;
        if (home == null)
            return;

        int needcost = (int)User.Inst.TBL.Const["CONST_MARBLE_SUMMON_COST_INIT"].Value + home.MarbleCount * (int)User.Inst.TBL.Const["CONST_MARBLE_SUMMON_COST_UP"].Value;
        if (home.Cost < needcost)
        {
            //Debug.LogFormat("Not enough cost.....{0} need cost. but current cost {1}", needcost, home.Cost);
            return;
        }

        var size = home.BoardSize.SlotCount;
        var emptyList = new List<int>();
        for (int i = 0; i < size; i++)
            if (!home.Boards.ContainsKey(i))
                emptyList.Add(i);
        if (emptyList.Count <= 0)
            return;

        User.Inst.PlayGame.GameParse<Net.Api.MarbleAdd.Ack>(new Net.Api.MarbleAdd.Req() { Idx = -1 });
        SetMarbleNeedCost();
        BattleMode.CheckCreateMarble?.Invoke(false);
        //Debug.LogFormat("CurCost : {0}", home.Cost);
    }

    public void SetMarbleNeedCost()
    {
        var home = BattleMode.Home;
        if (home == null)
            return;

        AddMarbleCostText.text = "-";
        if (BattleMode.IsGameOver)
            return;
        NextMarbleNeedCost = (int)User.Inst.TBL.Const["CONST_MARBLE_SUMMON_COST_INIT"].Value + home.MarbleCount * (int)User.Inst.TBL.Const["CONST_MARBLE_SUMMON_COST_UP"].Value;
        AddMarbleCostText.text = NextMarbleNeedCost.ToString();
    }

    #region Emote
    protected float HomeEmoteTime = 0f, AwayEmoteTime = 0f;
    public async void SetEmoteIcon(bool me, int index)
    {
        if (!IsUseEmote)
            return;

        string iconname = string.Format("EmotIcon_01_0{0}", index + 1);
        if (me)
        {
            HomeInfo.Emote.SetActive(true);
            UIManager.Inst.SetSprite(HomeInfo.EmoteIcon, UIManager.AtlasName.EmotIcon, iconname);
            DOTween.Restart(HomeInfo.Emote, "OtherEmot");
            DOTween.Restart(HomeInfo.EmoteIcon, "OtherEmot");
            await Task.Delay(1000);
            HomeInfo.Emote.SetActive(false);
        }
        else
        {
            AwayInfo.Emote.SetActive(true);
            UIManager.Inst.SetSprite(AwayInfo.EmoteIcon, UIManager.AtlasName.EmotIcon, iconname);
            DOTween.Restart(AwayInfo.Emote, "OtherEmot");
            DOTween.Restart(AwayInfo.EmoteIcon, "OtherEmot");
            await Task.Delay(1000);
            AwayInfo.Emote.SetActive(false);
        }
    }
    #endregion

    #region 보스 정보 파업
    public void OnClickBossInfo()
    {
        UIManager.Inst.OpenPanel(UIPanel.IngameBossInfoPopup, BossIndex);
    }
    
    public void SetBossIcon(int bossIndex)
    {
        BossIndex = bossIndex;
        if (BossIcon)
            UIManager.Inst.SetSprite(BossIcon, UIManager.AtlasName.Enemy, Sheet.TBL.Monster[BossIndex].Icon);
    }
    #endregion

    #region 게임 포기
    public void OnClickGiveupButton()
    {
        if (GamePvP.Instance.BattleMode.IsGameOver)
            return;

        UIManager.Inst.OpenPanel(UIPanel.IngameSurrenderPopup);
    }
    #endregion

    #region 튜토리얼
    protected bool IsCheckCreateMarble()
    {
        bool res = NextMarbleNeedCost <= BattleMode.Home.Cost;
        if (res)
        {
            var home = BattleMode.Home;
            if (home != null)
            {
                var size = home.BoardSize.SlotCount;
                var emptyList = new List<int>();
                for (int i = 0; i < size; i++)
                    if (!home.Boards.ContainsKey(i))
                        emptyList.Add(i);
                if (emptyList.Count > 0)
                    res = true;
                else
                    res = false;
            }
        }

        return res;
    }
    #endregion
    #region upgrade effect
    protected void InitEffect()
    {
        string upgradeeffpath = "Prefab/Effect/UI/eff_UI_IngameMyMarbleItem_01";
        for (int i = 0; i < 2; ++i)
        {
            UIParticlePlay[] p = new UIParticlePlay[5];
            PlayerInfo pInfo = i == 0 ? HomeInfo : AwayInfo;
            for (int j = 0; j < p.Length; ++j)
            {
                if (p[j] == null)
                    p[j] = new UIParticlePlay(pInfo.DiceInfo[j].transform, upgradeeffpath);
                var rectTranform = p[j].Go.GetComponent<Transform>();
                rectTranform.localScale = Vector3.one * 100;
                p[j].Stop();
            }
            UpgradeEffects.Add(i, p);
        }
    }
    public void ShowMarbleUpgradeEffect(bool isHome, int slot)
    {
        UpgradeEffects[isHome ? 0 : 1][slot].Play();
    }
    #endregion
}
