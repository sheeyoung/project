using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
public class BoxInfoPopup : UIBasePanel
{
    [SerializeField] Image boxIconImage;
    [SerializeField] Transform infoItemParent;
    [SerializeField] GameObject infoItmeObject;
    [SerializeField] Button okButton;
    [SerializeField] Button buyButton;

    List<BoxInfoItem> boxInfoItems;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
        boxInfoItems = new List<BoxInfoItem>();
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        okButton.onClick.AddListener(OnClickClosePanel);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        if(param.Length <= 0)
        {
            ClosePanel();
            return;
        }

        int boxId = (int)param[0];
        if(User.Inst.TBL.BOX_Slot.ContainsKey(boxId) == false)
        {
            ClosePanel();
            return;
        }
        okButton.gameObject.SetActive(true);
        buyButton.gameObject.SetActive(false);

        var boxSlotTableInfo = User.Inst.TBL.BOX_Slot[boxId];
        var boxInfo = User.Inst.TBL.Box[boxSlotTableInfo.BoxID];
        UIManager.Inst.SetSprite(boxIconImage, UIManager.AtlasName.ShopIcon, boxInfo.BoxImage);

        var rewards = User.Inst.TBL.Gens.Reward[boxInfo.RewardID];
        int boxInfoItemCount = 0;
        foreach (var rewardItem in rewards)
        {
            if(rewardItem.Key == 0)//CompType 1
            {
                for(int i = 0; i < rewardItem.Value.Count; i++)
                {
                    if(boxInfoItems.Count <= boxInfoItemCount)
                    {
                        var go = GameObject.Instantiate(infoItmeObject, infoItemParent);
                        var boxItem = go.GetComponent<BoxInfoItem>();
                        boxItem.Init();
                        boxInfoItems.Add(boxItem);
                    }
                    boxInfoItems[boxInfoItemCount].SetDataCompType1(rewardItem.Value[0].Index, 1);
                    boxInfoItemCount++;
                }
            }
            else
            {
                if (boxInfoItems.Count <= boxInfoItemCount)
                {
                    var go = GameObject.Instantiate(infoItmeObject, infoItemParent);
                    var boxItem = go.GetComponent<BoxInfoItem>();
                    boxItem.Init();
                    boxInfoItems.Add(boxItem);
                }
                boxInfoItems[boxInfoItemCount].SetDataCompType2(boxInfo.RewardID, rewardItem.Key, 1);
                boxInfoItemCount++;
            }
        }


    }


}
