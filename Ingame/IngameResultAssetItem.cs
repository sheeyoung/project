using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Net;
using DG.Tweening;

public class IngameResultAssetItem : MonoBehaviour
{
    [SerializeField] Image assetIconImage;
    [SerializeField] Text assetNumText;
    [SerializeField] float numberUpTime = 0.1f;
    [SerializeField] GameObject getAssetObject;

    Stack<GetAsset> getAssets;

    protected List<DOTweenAnimation> ani;
    public REWARD_TYPE GetRewardType { get { return curRewardType; } }

    float currentNum;
    int totalNum;
    float addPerSecNum;
    bool isPlayNumberUpCount;

    REWARD_TYPE curRewardType;
    public void Init(REWARD_TYPE asset, int count, int rewardvalue = 0)
    {
        gameObject.SetActive(true);
        curRewardType = asset;

        assetNumText.text = count.ToString();
        currentNum = count;
        totalNum = count;
        UIUtil.SetRewardIcon(asset, assetIconImage, rewardvalue);

        if(getAssets == null)
            getAssets = new Stack<GetAsset>();
    }
    public void AppearAni()
    {
        transform.SetAsFirstSibling();
        if (ani == null)
            ani = GetTweenAllChild(transform);
        for (int i = 0; i < ani.Count; i++)
        {
            DOTween.Restart(ani[i].gameObject, "Appear");
        }
    }
    public List<DOTweenAnimation> GetTweenAllChild(Transform parent)
    {
        List<DOTweenAnimation> datas = new List<DOTweenAnimation>();
        if (parent.GetComponent<DOTweenAnimation>() != null)
            datas.Add(parent.GetComponent<DOTweenAnimation>());
        GetTweenChild(parent, datas);

        return datas;
    }

    private void GetTweenChild(Transform parent, List<DOTweenAnimation> tweenAnis)
    {
        for (int i = 0, iMax = parent.childCount; i < iMax; i++)
        {
            Transform node = parent.GetChild(i);
            try
            {
                DOTweenAnimation[] tweens = node.gameObject.GetComponents<DOTweenAnimation>();
                for (int j = 0; j < tweens.Length; j++)
                {
                    tweenAnis.Add(tweens[j]);
                }

                GetTweenChild(node, tweenAnis);
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR
                Debug.Log("GetChild Error : " + ex);
#endif
            }
        }
    }
    public void AddAsset(int addCount)
    {
        totalNum += addCount;
        int diff  = totalNum - (int)currentNum;
        addPerSecNum = (float)diff / numberUpTime;
        Debug.LogFormat("addPerSecNum : {0}", addPerSecNum);
        isPlayNumberUpCount = true;

        if(getAssets.Count <= 0)
        {
            var go = UIManager.Inst.CreateObject(getAssetObject, transform);
            var getAsset = go.GetComponent<GetAsset>();
            getAsset.Init(OnCompleteGetAssetAni);
            getAssets.Push(getAsset);
        }
        var getAssetPlay = getAssets.Pop();
        string addString = string.Format("+{0}", addCount);
        getAssetPlay.SetCount(addString);
    }
    private void OnCompleteGetAssetAni(GameObject go)
    {
        var getAsset = go.GetComponent<GetAsset>();
        go.SetActive(false);
        getAssets.Push(getAsset);
    }

    private void Update()
    {
        if (isPlayNumberUpCount)
        {
            currentNum += addPerSecNum * (Time.deltaTime / numberUpTime);
            assetNumText.text = ((int)currentNum).ToString();
            if (currentNum > totalNum)
            {
                currentNum = totalNum;
                assetNumText.text = totalNum.ToString();
                isPlayNumberUpCount = false;
            }
        }
    }


}
