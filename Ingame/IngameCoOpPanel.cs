using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using scene;

public class IngameCoOpPanel : IngameMain
{
    [Header("Wave")]
    [SerializeField] protected Text WaveText;
    [Header("Disconnect")]
    [SerializeField] protected GameObject DisconnectIcon;

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        StartCoroutine(TickCurCost());
        SetAwayInfo();
        SetHomeInfo();
        SetUseEmote();
        InitEffect();
        WaveText.text = "1";
    }

    #region Wave
    public void SetWave(int wave)
    {
        WaveText.text = wave.ToString();

        // 보스 등장 라운드 계산
        int index = 0;
        foreach (var cb in Sheet.TBL.COOP_Batch)
        {
            if (cb.Value.RoundGroupMin > wave)
                break;
            index = cb.Key;
        }
        var coopBatchInfo = Sheet.TBL.COOP_Batch[index];
        // 이번에 진행 라운드
        int gengroupIndex = (wave - 1) % coopBatchInfo.GenGroup.Count;

        // 보스 라운드
        string[] arrBossRounds = coopBatchInfo.BossRound.Split(',');
        int bossRound = 0;
        for (int i = 0; i < arrBossRounds.Length; ++i)
        {
            if (!int.TryParse(arrBossRounds[i], out bossRound))
                continue;
            if (gengroupIndex + 1 <= bossRound)
                break;
        }

        if (BossSpawnTime)
        {
            BattleMode.NextBossRound = false;
            int remain = bossRound - (gengroupIndex + 1);
            if (remain == 0)
            {
                BossSpawnReady.gameObject.SetActive(false);
                BossSpawnTime.text = UIUtil.GetText("UI_Ingame_Panel_001");
            }
            else
            {
                BossSpawnReady.gameObject.SetActive(true);
                BossSpawnTime.text = string.Format("{0}{1}", remain, UIUtil.GetText("UI_Common_Round"));
                if (remain == 1)
                    BattleMode.NextBossRound = true;
            }
        }

        if (BossIcon)
        {
            int gengroup = Sheet.TBL.COOP_Batch[index].GenGroup[bossRound - 1];
            var genInfo = Sheet.TBL.Gens.MobGen[gengroup];
            if (genInfo.GenType == 3)
            {
                for (int i = 0; i < genInfo.MobID.Count; ++i)
                {
                    if (genInfo.MobID[i] == 0 || !Sheet.TBL.Monster.ContainsKey(genInfo.MobID[i]))
                        continue;
                    var monInfo = Sheet.TBL.Monster[genInfo.MobID[i]];
                    if (monInfo.Grade == (int)Net.Impl.ENEMY_GRADE.BOSS)
                    {                        
                        BossIndex = genInfo.MobID[i];
                        break;
                    }
                }
            }
            SetBossIcon(BossIndex);
        }

    }
    #endregion    

    #region 다른 유저가 게임을 나간 경우.(게임포기)
    public void OtherUserLeftGame()
    {
        DisconnectIcon.SetActive(true);
    }
    #endregion
}
