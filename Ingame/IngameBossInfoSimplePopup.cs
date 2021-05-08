using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System.Threading.Tasks;

public class IngameBossInfoSimplePopup : UIBasePanel
{
    [Header("Boss")]
    [SerializeField] private Image BossIcon;
    [SerializeField] private Text BossName;
    [SerializeField] private Text BossDesc;

    private int bossIndex = 0;

    // UI open effect
    protected UIParticlePlay OpenEffect = null;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if (param.Length <= 0 || scene.GamePvP.Instance.BattleMode.IsGameOver)
        {
            ClosePanel();
            return;
        }

        bossIndex = (int)param[0];
        SetBoss();

        if (OpenEffect == null)
        {
            if (OpenEffect == null)
                OpenEffect = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_IngameBossInfoSimplePopup_01");
            var rectTranform = OpenEffect.Go.GetComponent<Transform>();
            rectTranform.localScale = Vector3.one * 100;
        }
        OpenEffect.Play();

        CloseBossInfo();
    }

    private void SetBoss()
    {
        var bossInfo = Sheet.TBL.Monster[bossIndex];
        var bossLangInfo = Sheet.Langs.Monster[bossIndex];
        if (BossIcon) UIManager.Inst.SetSprite(BossIcon, UIManager.AtlasName.Enemy, bossInfo.Icon);
        if (BossName) BossName.text = bossLangInfo.Name;
        if (BossDesc) BossDesc.text = bossLangInfo.Desc;
        SoundManager.Inst.PlayEffect(10);
        //SoundManager.Inst.PlayEffect(bossInfo.MonsterCreateSND);
    }

    public async void CloseBossInfo()
    {
        await Task.Delay((int)(Sheet.TBL.Const["CONST_COOP_BOSS_DELAY"].Value * 1000));
        ClosePanel();
        if (Net.PeerNet.Inst.Mode == Game.MODE.PVP)
            scene.GamePvP.Instance.BattleMode.SetRound();
    }
}