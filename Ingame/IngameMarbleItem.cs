using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class IngameMarbleItem : MonoBehaviour
{
    [SerializeField]
    public Image marbleImage;
    [SerializeField]
    public Image levelImage;
    [SerializeField]
    public Text costText;
    private int marbleIdx;
    public int slotIdx;
    public Action<int> MarbleItemClickEvent;
    public Action<bool, int> MarbleItemPressEvent;
    private GameObject idleEff;
    public void SetItem(int marbleIdx)
    {
        if(Sheet.TBL.Marble.ContainsKey(marbleIdx) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        this.marbleIdx = marbleIdx;
        var marbleData = Sheet.TBL.Marble[marbleIdx];
        string marbleIconName = marbleData.Icon;
        UIManager.Inst.SetSprite(marbleImage, UIManager.AtlasName.MainMarble, marbleIconName);
        SetIdleEffect(marbleData);
    }
    public void SetLevel(int level)
    {
        if (levelImage)
        {
            string imageName = string.Format("Text_Lv0{0}", level);
            UIManager.Inst.SetSprite(levelImage, UIManager.AtlasName.UILobby, imageName);
        }
    }
    public void SetCost(int needCost)
    {
        if (costText) costText.text = needCost > 0 ? needCost.ToString() : "-";
    }

    private void SetIdleEffect(TBL.Sheet.CMarble marbleTable)
    {
        //if (idleEff != null)
        //    Destroy(idleEff);

        //if (string.IsNullOrEmpty(marbleTable.IdleEffect))
        //    return;
        //var go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect + "_list");
        //if (go != null)
        //{
        //    idleEff = GameObject.Instantiate(go) as GameObject;
        //    idleEff.transform.SetParent(marbleImage.transform);
        //    Transform tfParticle = idleEff.transform;
        //    tfParticle.localPosition = new Vector3(0, 0, 0);
        //    tfParticle.localScale = new Vector3(90f, 90f, 90f);
        //}
    }

    public void OnClickItem()
    {
        MarbleItemClickEvent?.Invoke(slotIdx);
    }

    public void OnPointerDown()
    {
        MarbleItemPressEvent?.Invoke(true, slotIdx);
    }

    public void OnPointerUp()
    {
        MarbleItemPressEvent?.Invoke(false, -1);
    }
}
