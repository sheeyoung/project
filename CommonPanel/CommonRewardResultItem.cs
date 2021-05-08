using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net;
using DG.Tweening;
using UI;

public class CommonRewardResultItem : MonoBehaviour
{
    [SerializeField] GameObject descGrp;
    [SerializeField] GameObject gradeGrp;
    [SerializeField] Image rewardIcon;
    [Header("Marble")]
    [SerializeField] GameObject marbleGrp;
    [SerializeField] Image marbleIcon;

    [SerializeField] Text marbleCountText;
    [SerializeField] Text rewardCountText;
    [SerializeField] Text rewardNameText;
    private List<DOTweenAnimation> openAni;
    UIParticlePlay eff;
    public GameObject itemGradeOpenObject;
    public float waitTime = 0.2f;

    public void SetItem(REWARD_TYPE rewardType, string rewardCount, int rewardValue = 0)
    {
        gameObject.SetActive(true);
        if (eff != null) eff.Go.SetActive(false);
        string rewardIconName = "";
        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                {
                    
                    var marbleTableInfo = User.Inst.TBL.Marble[rewardValue];
                    if(marbleTableInfo.Rarity == (int)MarbleRarity.Chronicle
                        || marbleTableInfo.Rarity == (int)MarbleRarity.Legendary)
                    {
                        if (marbleGrp) marbleGrp.SetActive(true);
                        if (rewardIcon) rewardIcon.gameObject.SetActive(false);

                        if (marbleIcon) UIUtil.SetRewardIcon(rewardType, marbleIcon, rewardValue);
                        
                        if (gradeGrp) gradeGrp.SetActive(true);
                        if (descGrp) descGrp.SetActive(false);
                        PlayAni("GradeOpen");
                    }
                    else
                    {
                        if (marbleGrp) marbleGrp.SetActive(true);
                        if (rewardIcon) rewardIcon.gameObject.SetActive(false);
                        if (marbleIcon) UIUtil.SetRewardIcon(rewardType, marbleIcon, rewardValue);
                        
                        if (gradeGrp) gradeGrp.SetActive(false);
                        if (descGrp) descGrp.SetActive(true);

                        PlayAni("Open");
                    }
                }
                break;
            default:
                {
                    if (gradeGrp) gradeGrp.SetActive(false);
                    if (descGrp) descGrp.SetActive(true);
                    if (marbleIcon) marbleGrp.SetActive(false);
                    if (rewardIcon) rewardIcon.gameObject.SetActive(true);
                    PlayAni("Open");
                    UIUtil.SetRewardIcon(rewardType, rewardIcon, rewardValue);
                }
                break;
        }

        if (rewardCountText) rewardCountText.text = rewardCount;
        if (marbleCountText) marbleCountText.text = rewardCount;
        if (rewardNameText) rewardNameText.text = UIUtil.GetRewardName(rewardType, rewardValue);
    }
    public void PlayAni(string aniKey)
    {
        if (openAni == null)
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
            effTranform.localScale = new Vector3(100f, 100f, 100f);
        }
        eff.Go.SetActive(true);
        eff.Go.transform.localPosition = Vector3.zero;
        eff.Play();
    }

    public void EndSpecialMarbleAni()
    {
        if (descGrp) descGrp.SetActive(true);
        if (gradeGrp) gradeGrp.SetActive(false);
        
        PlayAni("Open");
    }
    public float GetAniTime()
    {
        float wait = 0.4f;
        var tween = itemGradeOpenObject.GetComponent<DOTweenAnimation>();
        if (tween) wait = tween.duration + tween.delay;
        return wait + waitTime;
    }
}
