using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using System;
public class LevelupPopup : UIBasePanel
{
    [SerializeField] Image levelIconImage;
    [SerializeField] Text levelText;
    [SerializeField] float autoCloseTime = 3f;
    UIManager.PanelEndAction endAction;
    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);
    UIParticlePlay levelUpEff;
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
        Debug.Log("AutoClose##################");
        UIUtil.SetLevelIcon(User.Inst.Doc.Level, levelIconImage);
        levelText.text = User.Inst.Doc.Level.ToString();
        endAction = null;
        if (param != null && param.Length > 0)
            endAction = (UIManager.PanelEndAction)param[0];

        if(levelUpEff == null)
        {
            levelUpEff = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_LevelupPopup_01");
            var effTranform = levelUpEff.Go.GetComponent<Transform>();
            effTranform.localScale = effSize;
            //levelUpEff.SetPosition(Vector3.zero);
        }
        levelUpEff.Play();
        SoundManager.Inst.PlayUIEffect(27);
        StartCoroutine(AutoClose());
    }
    IEnumerator AutoClose()
    {
        Debug.Log("AutoClose");
        yield return new WaitForSecondsRealtime(autoCloseTime);
        Debug.Log("AutoClose222222");
        OnClickClosePanel();
    }
    public override void ClosePanel()
    {
        endAction?.Invoke();
        base.ClosePanel();
    }
}
