using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using scene;
using UI;
using DG.Tweening;

public class IngamePanel : IngameMain
{        
    [SerializeField] private GameObject BossWarning;
    [SerializeField] private DOTweenAnimation BossWarningAnim;
    
    // boss warning 연출
    private bool IsBossWarning = false;
    
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        StartCoroutine(TickCurCost());
        SetAwayInfo();
        SetHomeInfo();
        SetUseEmote();
        InitEffect();
        if (BossIcon)
            BossIcon.gameObject.SetActive(false);
        if (BossSpawnTime)
            BossSpawnTime.text = "";
    }        

    #region 보스 출현 시간 관련
    private float remainBossSpawn;
    private bool PlayTime = false;    
    public void SetBossSpawnTime(float remainSec)
    {        
        remainBossSpawn = remainSec;
        if (BossSpawnTime)
            BossSpawnTime.text = GetTime(remainSec);
    }

    public void StartBossSpawnTime()
    {
        PlayTime = true;
        BossSpawnReady.gameObject.SetActive(true);
        StartCoroutine(UpdateBossTime());
    }

    private IEnumerator UpdateBossTime()
    {
        // 보스 등장 시간
        while (PlayTime)
        {
            if (Game.GameTick.Inst.IsRun)
            {
                int spawnRemainSec = (((PvPEnemySpawnManager)BattleMode.HomeSpawnMgr).CurRoundTime + Game.GameTick.Inst.TicksPerSecond - Game.GameTick.Inst.Next) / Game.GameTick.Inst.TicksPerSecond;
                SetBossSpawnTime(spawnRemainSec);
            }

            yield return 0;
        }
    }

    private string GetTime(float remainSec)
    {
        int min = (int)(remainSec / 60f);
        if (min < 0)
            min = 0;
        int sec = (int)((remainSec - (min * 60f)) % 60f);
        if (sec < 0)
            sec = 0;
        if (!IsBossWarning && min <= 0 && sec <= 10)
        {
            IsBossWarning = true;
            BossWarning.SetActive(true);
            DOTween.PlayForward(BossWarning.gameObject, "BossPopup");
            DOTween.PlayForward(BossWarningAnim.gameObject, "BossPopup");
        }
        return string.Format("{0:00}:{1:00}", min, sec);
    }
    public void BossSpwn()
    {
        PlayTime = false;
        if (BossSpawnTime)
        {
            BossSpawnTime.text = UIUtil.GetText("UI_Ingame_Panel_001");
            BossSpawnReady.gameObject.SetActive(false);
            if (IsBossWarning)
            {
                IsBossWarning = false;
                BossWarningAnim.DORewind();
                BossWarning.SetActive(false);
            }
        }
    }
    #endregion
}

