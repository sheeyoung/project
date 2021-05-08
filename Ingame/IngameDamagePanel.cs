using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Libs.Unity;
using UnityEngine.Events;

public class IngameDamagePanel : BaseMono
{
    private RectTransform RectTrans = null;
    private RectTransform ParentRectTrans = null;
    public Text textDamage = null;

    private readonly Color criDamColor = new Color(1, 1, 0);
    private readonly Color normalDamColor = new Color(1, 1, 1);

    public void Init()
    {
        RectTrans = transform as RectTransform;
        ParentRectTrans = RectTrans.parent as RectTransform;
        if(textDamage == null)
            textDamage = Utils.GetChildScript<Text>("LBDamageCount", transform);
    }

    public void SetDamage(Vector3 pos, int damage, bool isCritical = false)
    {
        if (damage <= 0)
            return;
        int fontSize = isCritical ? 70 : 50;
        textDamage.color = isCritical ? criDamColor : normalDamColor;
        textDamage.fontSize = fontSize;
        textDamage.text = damage.ToString();
        gameObject.SetActive(true);
        pos.x += Random.Range(-0.3f, 0.3f);
        pos.y += Random.Range(-0.3f, 0.3f);
        //pos.y += 0.3f;
        StartCoroutine(UpdatePos(0.5f, pos, delegate { UIManager.Inst.HideDamagePanel(this); }));
    }

    public void SetCost(Vector3 pos, int cost)
    {
        if (cost <= 0)
            return;
        textDamage.text = string.Format("+{0}", cost);
        gameObject.SetActive(true);
        SetPos(pos);
        StartCoroutine(UpdateHide(0.8f, delegate { UIManager.Inst.HideStarPanel(this); }));
    }

    private IEnumerator UpdatePos(float time, Vector3 startPos, UnityAction endAction)
    {
        transform.SetAsLastSibling();
        float rndx = Random.Range(-0.01f, 0.01f);
        Vector3 pos = startPos;
        while (true)
        {
            if (Time.timeScale > 0f)
            {
                if (time <= 0f)
                    break;
                pos.x += rndx;
                pos.y += 0.02f;
                SetPos(pos);
                time -= Time.deltaTime;
            }
            yield return 0;
        }

        endAction?.Invoke();        
    }

    private IEnumerator UpdateHide(float time, UnityAction endAction)
    {
        transform.SetAsLastSibling();
        while (true)
        {
            if (Time.timeScale > 0f)
            {
                if (time <= 0f)
                    break;
                time -= Time.deltaTime;
            }
            yield return 0;
        }

        endAction?.Invoke();        
    }

    public void SetPos(Vector3 pos, bool islocal = true)
    {
        Vector3 screenPos = scene.GamePvP.Instance.MainCam.WorldToScreenPoint(pos);
        Vector2 localPos = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRectTrans, screenPos, UIManager.Inst.UICanvas.worldCamera, out localPos);
        if (islocal)
            RectTrans.localPosition = localPos;
        else
            RectTrans.position = localPos;
    }

}
