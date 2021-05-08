using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxInfoMarbleItem : MonoBehaviour
{
    [SerializeField] public Image rewardIconImage;
    [SerializeField] public Text rewardRateText;
    public void SetData(string iconImageName, string rateText)
    {
        gameObject.SetActive(true);
        rewardRateText.text = rateText;
        UIManager.Inst.SetSprite(rewardIconImage, UIManager.AtlasName.MainMarble, iconImageName);
    }
}
