using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarbleStatInfoItem : MonoBehaviour
{
    [SerializeField] private GameObject offGrp;
    [SerializeField] private Text offNameText;
    [SerializeField] private Text offStatText;
    [SerializeField] private GameObject onGrp;
    [SerializeField] private Text onNameText;
    [SerializeField] private Text onStatText;
    public void InitItem(string statName, string statValue, bool isChange)
    {
        gameObject.SetActive(true);
        if(offGrp) offGrp.SetActive(isChange == false);
        if (onGrp) onGrp.SetActive(isChange);
        if (isChange)
        {
            if (onNameText) onNameText.text = statName;
            if (onStatText) onStatText.text = statValue;
        }
        else
        {
            if (offNameText) offNameText.text = statName;
            if (offStatText) offStatText.text = statValue;
        }
    }
}
