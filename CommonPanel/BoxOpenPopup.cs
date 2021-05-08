using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using Net;

public class BoxOpenPopup : UIBasePanel
{
    UIParticlePlayTexture idleEff;
    UIParticlePlayTexture openEff;

    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);
    [SerializeField] float OpenWaitTime = 2.0f;

    UIManager.PanelEndAction endAction;
    string spriteName;
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
        isClick = false;
        spriteName = string.Empty;
        endAction = null;
        if (param != null && param.Length >= 1)
            spriteName = (string)param[0];
        Debug.Log("##### BoxSpriteNAme : " + spriteName);
        if (param != null && param.Length > 1)
            endAction = (UIManager.PanelEndAction)param[1];

        REWARD_TYPE rewardType = REWARD_TYPE.REWARD_MARBLE;
        if (param != null && param.Length > 2)
            rewardType = (REWARD_TYPE)param[2];

        if (idleEff != null) idleEff.gameObject.SetActive(false);
        if (openEff != null) openEff.gameObject.SetActive(false);

        if (idleEff != null)
        {
            Destroy(idleEff);
        }
        GameObject particle = null;
        if(rewardType == REWARD_TYPE.REWARD_MARBLE_LEGEND)
            particle = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/UI/eff_UI_BoxOpenPopup_idle_02");
        else if(rewardType == REWARD_TYPE.REWARD_MARBLE_MYTH)
            particle = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/UI/eff_UI_BoxOpenPopup_idle_03");
        else
            particle = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/UI/eff_UI_BoxOpenPopup_idle_01");

        GameObject ins = GameObject.Instantiate(particle) as GameObject;
        idleEff = ins.GetComponent<UIParticlePlayTexture>();
        idleEff.SetParticle(transform);
        var effTranform = idleEff.GetComponent<Transform>();
        effTranform.localScale = effSize;
        idleEff.transform.localPosition = (Vector3.zero);
        
        idleEff.gameObject.SetActive(true);
        var effSprite = UIManager.Inst.UIData.EffectSprite.GetSprite(spriteName);
        idleEff.Play(effSprite);
        SoundManager.Inst.SetBgmToggle(true);
        SoundManager.Inst.PlayUIEffect(21);
    }
    private bool isClick;
    public void OnClickBg()
    {
        if (isClick)
            return;
        isClick = true;
        SoundManager.Inst.AllOffEffectSound();
        SoundManager.Inst.PlayUIEffect(3);
        idleEff.gameObject.SetActive(false);
        StartCoroutine(StartSound());
        StartCoroutine(StartOpen());
    }
    IEnumerator StartOpen()
    {
        SoundManager.Inst.PlayUIEffect(28);
        if (openEff == null)
        {
            GameObject particle = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/UI/eff_UI_BoxOpenPopup_open_01");
            GameObject ins = GameObject.Instantiate(particle) as GameObject;
            openEff = ins.GetComponent<UIParticlePlayTexture>();
            openEff.SetParticle(transform);
            var effTranform = openEff.gameObject.GetComponent<Transform>();
            effTranform.localScale = effSize;
            openEff.transform.localPosition = (Vector3.zero);
        }
        openEff.gameObject.SetActive(true);
        var effSprite = UIManager.Inst.UIData.EffectSprite.GetSprite(spriteName);
        openEff.Play(effSprite);
        yield return new WaitForSeconds(OpenWaitTime);
        ClosePanel();
    }
    IEnumerator StartSound()
    {
        yield return new WaitForSeconds(1.5f);
        SoundManager.Inst.PlayUIEffect(22);
    }
    public override void ClosePanel()
    {
        base.ClosePanel();
        endAction?.Invoke();
    }
}
