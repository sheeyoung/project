using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net;
using DG.Tweening;
public class RewardItem : MonoBehaviour
{
    [SerializeField] GameObject rewardGrp;
    [SerializeField] Image rewardIcon;
    [SerializeField] GameObject marbleGrp;
    [SerializeField] Image marbleIcon;
    [SerializeField] Text marbleCountText;
    [SerializeField] Text rewardCountText;
    [SerializeField] Text rewardNameText;
    private List<DOTweenAnimation> openAni;
    UIParticlePlay eff;
    public void SetItem(REWARD_TYPE rewardType, string rewardCount, int rewardValue = 0)
    {
        gameObject.SetActive(true);
        if(eff != null) eff.Go.SetActive(false);
        if (idleEff != null) idleEff.SetActive(false);
        string rewardIconName = "";
        switch(rewardType)
        {
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                {
                    if (marbleIcon && User.Inst.TBL.Marble.ContainsKey(rewardValue))
                    {
                        if (rewardGrp) rewardGrp.SetActive(false);
                        if (marbleGrp) marbleGrp.SetActive(true);
                        UIUtil.SetRewardIcon(rewardType, marbleIcon, rewardValue);
                    }
                    else
                    {
                        if (rewardGrp) rewardGrp.SetActive(true);
                        if (marbleGrp) marbleGrp.SetActive(false);
                        UIUtil.SetRewardIcon(rewardType, rewardIcon, rewardValue);
                    }
                }
                break;
            default:
                {
                    if (rewardGrp) rewardGrp.SetActive(true);
                    if (marbleGrp) marbleGrp.SetActive(false);

                    UIUtil.SetRewardIcon(rewardType, rewardIcon, rewardValue);
                }
                break;
        }

        if(rewardCountText) rewardCountText.text = rewardCount;
        if (marbleCountText) marbleCountText.text = rewardCount;
        if (rewardNameText) rewardNameText.text = UIUtil.GetRewardName(rewardType, rewardValue);
    }
    public void PlayAni(string aniKey)
    {
        if(openAni == null)
            openAni = UIUtil.GetTweenAllChild(transform);
        for (int i = 0; i < openAni.Count; i++)
        {
            DOTween.Restart(openAni[i].gameObject, aniKey);
        }
    }
    string effPath = string.Empty;
    public void SetEff(string effPath)
    {
        if (eff == null || this.effPath.Equals(effPath) == false)
        {
            if (eff != null)
                Destroy(eff.Go);
            eff = new UIParticlePlay(transform, effPath);
            var effTranform = eff.Go.GetComponent<Transform>();
            effTranform.localScale = new Vector3(100f,100f,100f);
        }
        eff.Go.SetActive(true);
        eff.Go.transform.localPosition = Vector3.zero;
        eff.Play();
    }

    private GameObject idleEff;
    private string idleEffName = "";
    [Header("MarbleEff Size")]
    [SerializeField] Vector3 marbleEffSize = new Vector3(120f, 120f, 120f);
    public void SetIdleEffect(int index, bool isList)
    {
        if (User.Inst.TBL.Marble.ContainsKey(index) == false)
            return;
        var marbleTable = User.Inst.TBL.Marble[index];
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
        GameObject go = null;
        if (isList)
        {
            go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect + "_list");
        }
        else
        {
            go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect);
        }
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleIcon.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = marbleEffSize;
        }

    }
}
