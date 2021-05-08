using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using DG.Tweening;

public class IngameBossSelectPopup : UIBasePanel
{
    [SerializeField] private Transform BossListParent;
    [SerializeField] private GameObject SelectBossItem;

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

        SetBoss((int)param[0]);

        if (OpenEffect == null)
        {
            if (OpenEffect == null)
                OpenEffect = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_IngameBossSelectPopup_01");
            var rectTranform = OpenEffect.Go.GetComponent<Transform>();
            rectTranform.localScale = Vector3.one * 100;
        }
        OpenEffect.Play();
    }

    private void EndBossSelect()
    {
        ClosePanel();
        UIManager.Inst.OpenPanel(UIPanel.IngameBossInfoSimplePopup, bossIndex);
    }

    private void SetBoss(int idx)
    {
        SoundManager.Inst.PlayEffect(9);
        var genInfo = Sheet.TBL.MobGen[(int)Net.Impl.ENEMY_GRADE.BOSS];
        var list = genInfo.MobID.FindAll(m => m != 0);
        if (list.Count <= 0)
        {
            ClosePanel();
            return;
        }
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, 10000) % (n + 1);
            int val = list[k];
            list[k] = list[n];
            list[n] = val;
        }

        bossIndex = genInfo.MobID[idx % list.Count];
        scene.GamePvP.Instance.BattleMode.InGamePanel.SetBossIcon(bossIndex);
        for (int i = 0; i < BossListParent.childCount; ++i)
        {
            GameObject go = BossListParent.GetChild(i).gameObject;
            BossSelectItem item = new BossSelectItem(go);
            if (go == SelectBossItem)
            {
                item.SetBoss(Sheet.Langs.Monster[bossIndex].Name, Sheet.TBL.Monster[bossIndex].Icon);
                StartCoroutine(item.SelectBoss(2.6f));
            }
            else
                item.SetBoss(Sheet.Langs.Monster[list[i % list.Count]].Name, Sheet.TBL.Monster[list[i % list.Count]].Icon);
        }

        Invoke("EndBossSelect", 3f);
    }
}

public class BossSelectItem
{
    public GameObject item = null;
    public Image bossIcon = null;
    public Text bossNameText = null;
    public DOTweenAnimation[] openAni = null;

    public BossSelectItem(GameObject go)
    {
        item = go;
        for (int i = 0; i < item.transform.childCount; i++)
        {
            var child = item.transform.GetChild(i);
            if (child.name.Equals("SPEnemyIcon"))
                bossIcon = child.GetComponent<Image>();
            else if (child.name.Equals("LBName"))
                bossNameText = child.GetComponent<Text>();
        }

        openAni = go.GetComponentsInChildren<DOTweenAnimation>();
    }

    public void SetBoss(string bossname, string bossiconname)
    {
        UIManager.Inst.SetSprite(bossIcon, UIManager.AtlasName.Enemy, bossiconname);
        bossNameText.text = bossname;
    }

    public IEnumerator SelectBoss(float time)
    {
        float tick = Game.GameTick.Inst.Next + time * Game.GameTick.Inst.TicksPerSecond;
        while (true)
        {
            if (Game.GameTick.Inst.Next >= tick)
                break;
            yield return 0;
        }
        if (openAni != null)
        {
            for (int i = 0; i < openAni.Length; i++)
                DOTween.Restart(openAni[i].gameObject, "SelectBoss");
        }
    }
}
