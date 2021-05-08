using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Libs.Utils;

public partial class UIManager : MonoBehaviour
{
    private void InitDynamicUI()
    {
        if (scene.SceneLoader.Inst.GetActiveSceneName().Equals("Lobby") || scene.SceneLoader.Inst.GetActiveSceneName().Equals("TitleScene"))
            return;
        InitDamageUIPool();
    }

    #region 데미지 UI, 코스트 획득 UI
    // damage ui 의 부모 오브젝트(hierarchy 정리를 위해)
    private GameObject damageParent = null;
    // ui object pool
    private ObjectPool<IngameDamagePanel> damagePanelPool = null;
    private ObjectPool<IngameDamagePanel> StarPanelPool = null;

    private void InitDamageUIPool()
    {
        if (damageParent == null)
        {
            damageParent = new GameObject("DamageUIParent");
            damageParent.transform.SetParent(UICanvas.transform, false);
            damageParent.AddComponent<RectTransform>();
        }

        GameObject damageObj = UIResourceManager.Inst.Load<GameObject>("UI/Prefab/Ingame/DamageFont");
        damagePanelPool = new ObjectPool<IngameDamagePanel>(10, () =>
        {
            GameObject obj = Instantiate(damageObj);
            obj.SetActive(false);
            obj.transform.SetParent(damageParent.transform, false);
            IngameDamagePanel damageui = obj.GetComponent<IngameDamagePanel>();
            damageui.Init();
            return damageui;
        });

        GameObject starObj = UIResourceManager.Inst.Load<GameObject>("UI/Prefab/Ingame/StarFont");
        StarPanelPool = new ObjectPool<IngameDamagePanel>(5, () =>
        {
            GameObject obj = Instantiate(starObj);
            obj.SetActive(false);
            obj.transform.SetParent(damageParent.transform, false);
            IngameDamagePanel damageui = obj.GetComponent<IngameDamagePanel>();
            damageui.Init();
            return damageui;
        });
    }

    public IngameDamagePanel GetDamagePanel()
    {
        IngameDamagePanel panel = damagePanelPool.Get();
        return panel;
    }

    public IngameDamagePanel GetStarPanel()
    {
        IngameDamagePanel panel = StarPanelPool.Get();
        return panel;
    }

    public void HideDamagePanel(IngameDamagePanel panel)
    {
        panel.gameObject.SetActive(false);
        damagePanelPool.Free(panel);
    }

    public void HideStarPanel(IngameDamagePanel panel)
    {
        panel.gameObject.SetActive(false);
        StarPanelPool.Free(panel);
    }

    public void SetDamage(int dam, CompEnemy target, bool isCrit = false)
    {
        IngameDamagePanel panel = GetDamagePanel();
        Vector3 pos = target.hpText.transform.position;
        panel.SetDamage(pos, dam, isCrit);
    }
    public void SetCost(int cost, CompDice target)
    {
        IngameDamagePanel panel = GetStarPanel();
        Vector3 pos = target.transform.position;
        panel.SetCost(pos, cost);
    }
    #endregion
}
