using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Net;
using Net.Api;

public class UserInfoItem : MonoBehaviour
{
    [Serializable]
    public class Item
    {
        [Header("Info")]
        [SerializeField] public GameObject infoGrp;
        [SerializeField] public Text ExpText;
        [SerializeField] public RectTransform progressBar;
        [SerializeField] public Image GradeIconImage;
        [SerializeField] public Text LevelText;
        [Header("OnReward")]
        [SerializeField] public GameObject RewardOnGrp;
        [SerializeField] public Image RewardIconOnImage;
        [SerializeField] public Text RewardCountOnText;
        [SerializeField] public GameObject RewardMarbleOnGrp;
        [SerializeField] public Image rewardMarbleIconOnImage;
        [Header("OffReward")]
        [SerializeField] public GameObject RewardOffGrp;
        [SerializeField] public Image RewardIconOffImage;
        [SerializeField] public Text RewardCountOffText;
        [SerializeField] public GameObject RewardMarbleOffGrp;
        [SerializeField] public Image rewardMarbleIconOffImage;
        [Header("NextReward")]
        [SerializeField] public GameObject RewardNextGrp;
        [SerializeField] public Image RewardIconNextImage;
        [SerializeField] public Text RewardCountNextText;
        [SerializeField] public GameObject RewardMarbleNextGrp;
        [SerializeField] public Image rewardMarbleIconNextImage;
        [Header("GetAni")]
        [SerializeField] public GameObject GetAniGrp;
        [Space(10)]
        [SerializeField] public Button GetRewardButton;
    }
    [SerializeField]
    List<Item> items;
    enum ItemType
    {
        NORMAL,
        END
    }

    Action<int> getRewardAction;
    int currentLevel;
    public void Init()
    {
        items[0].GetRewardButton.onClick.AddListener(OnClickGetRewardButton);
        items[1].GetRewardButton.onClick.AddListener(OnClickGetRewardButton);
    }
    public void SetLevel(int level, Action<int> getRewardEvent)
    {
        this.getRewardAction = null;
        if (User.Inst.TBL.Account_EXP.ContainsKey(level) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        this.getRewardAction = getRewardEvent;
        currentLevel = level;
        SetData();

        ItemType itemType = User.Inst.TBL.Account_EXP.ContainsKey(currentLevel + 1) ? ItemType.NORMAL : ItemType.END;
        int itemCount = (int)itemType;
        items[itemCount].GetAniGrp.SetActive(false);
    }

    private void SetData()
    {
        var tableInfo = User.Inst.TBL.Account_EXP[currentLevel];

        ItemType itemType = User.Inst.TBL.Account_EXP.ContainsKey(currentLevel + 1) ? ItemType.NORMAL : ItemType.END;
        items[0].infoGrp.SetActive(itemType == ItemType.NORMAL);
        items[1].infoGrp.SetActive(itemType == ItemType.END);

        int itemCount = (int)itemType;

        
        int preNeedExp = 0;
        int preExp = 0;
        float percent = 1;
        if (User.Inst.TBL.Account_EXP.ContainsKey(currentLevel - 1))
        {
            preNeedExp = User.Inst.TBL.Account_EXP[currentLevel - 1].NeedEXP;
        }
        if (User.Inst.TBL.Account_EXP.ContainsKey(currentLevel - 2))
        {
            preExp = User.Inst.TBL.Account_EXP[currentLevel - 2].AccumEXP;
        }
        if(preNeedExp != 0)
            percent = (float)(User.Inst.Doc.Exp - preExp) / (float)preNeedExp;

        //경험치
        items[itemCount].ExpText.text = (preExp + preNeedExp).ToString("n0");

        //프로그래스바
        if (percent < 0)
            percent = 0f;
        else if (percent > 1)
            percent = 1f;
        items[itemCount].progressBar.localScale = new Vector3(percent, 1f, 1f);

        //레벨아이콘
        UIUtil.SetLevelIcon(currentLevel, items[itemCount].GradeIconImage);
        //UIManager.Inst.SetSprite(items[itemCount].GradeIconImage, UIManager.AtlasName.UILobby, tableInfo.LevelIcon);

        //레벨
        items[itemCount].LevelText.text = string.Format(UIUtil.GetText("UI_Common_Lv"), currentLevel);

        //보상
        items[itemCount].RewardOffGrp.SetActive(false);
        items[itemCount].RewardOnGrp.SetActive(false);
        items[itemCount].RewardNextGrp.SetActive(false);
        if (string.IsNullOrEmpty(tableInfo.RewardType) == false)
        {
            var rewardType = UIUtil.GetRewardType(tableInfo.RewardType);

            if (currentLevel <= User.Inst.Doc.RewardLevel)
            {
                //이미 보상 받음
                items[itemCount].RewardOffGrp.SetActive(true);
                switch (rewardType)
                {
                    case REWARD_TYPE.REWARD_MARBLE:
                    case REWARD_TYPE.REWARD_MARBLE_NORMAL:
                    case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                    case REWARD_TYPE.REWARD_MARBLE_RARE:
                    case REWARD_TYPE.REWARD_MARBLE_LEGEND:
                    case REWARD_TYPE.REWARD_MARBLE_MYTH:
                        if (User.Inst.TBL.Marble.ContainsKey(tableInfo.RewardValue))
                        {
                            items[itemCount].RewardMarbleOffGrp.SetActive(true);
                            items[itemCount].RewardIconOffImage.gameObject.SetActive(false);

                            UIUtil.SetRewardIcon(rewardType, items[itemCount].rewardMarbleIconOffImage, tableInfo.RewardValue);
                        }
                        else
                        {
                            items[itemCount].RewardMarbleOffGrp.SetActive(false);
                            items[itemCount].RewardIconOffImage.gameObject.SetActive(true);
                            UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconOffImage, tableInfo.RewardValue);
                        }
                        break;
                    default:
                        items[itemCount].RewardMarbleOffGrp.SetActive(false);
                        items[itemCount].RewardIconOffImage.gameObject.SetActive(true);
                        UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconOffImage, tableInfo.RewardValue);
                        break;

                }

                items[itemCount].RewardCountOffText.text = tableInfo.RewardCount.ToString();
                items[itemCount].GetRewardButton.enabled = false;
            }
            else
            {
                if (currentLevel > User.Inst.Doc.Level)
                {
                    //달성못함
                    items[itemCount].RewardNextGrp.SetActive(true);
                    switch (rewardType)
                    {
                        case REWARD_TYPE.REWARD_MARBLE:
                        case REWARD_TYPE.REWARD_MARBLE_NORMAL:
                        case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                        case REWARD_TYPE.REWARD_MARBLE_RARE:
                        case REWARD_TYPE.REWARD_MARBLE_LEGEND:
                        case REWARD_TYPE.REWARD_MARBLE_MYTH:
                            if (User.Inst.TBL.Marble.ContainsKey(tableInfo.RewardValue))
                            {
                                items[itemCount].RewardMarbleNextGrp.SetActive(true);
                                items[itemCount].RewardIconNextImage.gameObject.SetActive(false);

                                UIUtil.SetRewardIcon(rewardType, items[itemCount].rewardMarbleIconNextImage, tableInfo.RewardValue);
                            }
                            else
                            {
                                items[itemCount].RewardMarbleNextGrp.SetActive(false);
                                items[itemCount].RewardIconNextImage.gameObject.SetActive(true);
                                UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconNextImage, tableInfo.RewardValue);
                            }

                            break;
                        default:
                            items[itemCount].RewardMarbleNextGrp.SetActive(false);
                            items[itemCount].RewardIconNextImage.gameObject.SetActive(true);
                            UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconNextImage, tableInfo.RewardValue);
                            break;

                    }

                    items[itemCount].RewardCountNextText.text = tableInfo.RewardCount.ToString();
                    items[itemCount].GetRewardButton.enabled = true;

                }
                else if (currentLevel == User.Inst.Doc.RewardLevel + 1)
                {
                    //받을차례
                    items[itemCount].RewardOnGrp.SetActive(true);
                    switch (rewardType)
                    {
                        case REWARD_TYPE.REWARD_MARBLE:
                        case REWARD_TYPE.REWARD_MARBLE_NORMAL:
                        case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                        case REWARD_TYPE.REWARD_MARBLE_RARE:
                        case REWARD_TYPE.REWARD_MARBLE_LEGEND:
                        case REWARD_TYPE.REWARD_MARBLE_MYTH:
                            if (User.Inst.TBL.Marble.ContainsKey(tableInfo.RewardValue))
                            {
                                items[itemCount].RewardMarbleOnGrp.SetActive(true);
                                items[itemCount].RewardIconOnImage.gameObject.SetActive(false);

                                UIUtil.SetRewardIcon(rewardType, items[itemCount].rewardMarbleIconOnImage, tableInfo.RewardValue);
                            }
                            else
                            {
                                items[itemCount].RewardMarbleOnGrp.SetActive(false);
                                items[itemCount].RewardIconOnImage.gameObject.SetActive(true);
                                UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconOnImage, tableInfo.RewardValue);
                            }
                            break;
                        default:
                            items[itemCount].RewardMarbleOnGrp.SetActive(false);
                            items[itemCount].RewardIconOnImage.gameObject.SetActive(true);
                            UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconOnImage, tableInfo.RewardValue);
                            break;

                    }

                    items[itemCount].RewardCountOnText.text = tableInfo.RewardCount.ToString();
                    items[itemCount].GetRewardButton.enabled = true;
                }
                else
                {
                    //아직 받을 차례 아님
                    items[itemCount].RewardNextGrp.SetActive(true);
                    switch (rewardType)
                    {
                        case REWARD_TYPE.REWARD_MARBLE:
                        case REWARD_TYPE.REWARD_MARBLE_NORMAL:
                        case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
                        case REWARD_TYPE.REWARD_MARBLE_RARE:
                        case REWARD_TYPE.REWARD_MARBLE_LEGEND:
                        case REWARD_TYPE.REWARD_MARBLE_MYTH:
                            if (User.Inst.TBL.Marble.ContainsKey(tableInfo.RewardValue))
                            {
                                items[itemCount].RewardMarbleNextGrp.SetActive(true);
                                items[itemCount].RewardIconNextImage.gameObject.SetActive(false);

                                UIUtil.SetRewardIcon(rewardType, items[itemCount].rewardMarbleIconNextImage, tableInfo.RewardValue);
                            }
                            else
                            {
                                items[itemCount].RewardMarbleNextGrp.SetActive(false);
                                items[itemCount].RewardIconNextImage.gameObject.SetActive(true);
                                UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconNextImage, tableInfo.RewardValue);
                            }
                            break;
                        default:
                            items[itemCount].RewardMarbleNextGrp.SetActive(false);
                            items[itemCount].RewardIconNextImage.gameObject.SetActive(true);
                            UIUtil.SetRewardIcon(rewardType, items[itemCount].RewardIconNextImage, tableInfo.RewardValue);
                            break;

                    }

                    items[itemCount].RewardCountNextText.text = tableInfo.RewardCount.ToString();
                    items[itemCount].GetRewardButton.enabled = true;
                }

            }
        }
    }
    public void Refresh()
    {
        SetData();
    }
    public void OnClickGetRewardButton()
    {
        var tableInfo = User.Inst.TBL.Account_EXP[currentLevel];

        if (User.Inst.Doc.Level < currentLevel
             || User.Inst.Doc.RewardLevel != currentLevel - 1)
        {
            var rewardType = UIUtil.GetRewardType(tableInfo.RewardType);
            if(rewardType == REWARD_TYPE.REWARD_BOX || rewardType == REWARD_TYPE.REWARD_BOX_EXPAND)
            {
                UIManager.Inst.ShowBoxRewardPopup(tableInfo.RewardValue);
            }
            
            return;
        }
        if (string.IsNullOrEmpty(tableInfo.RewardType))
            return;
        getRewardAction?.Invoke(currentLevel);
    }
}
