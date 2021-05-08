using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;
public class MarbleTipButton : MonoBehaviour
{
    [SerializeField] UIEventButton toastEventButton;
    [SerializeField] MarbleDeckItem marbleDeck;
    void Start()
    {
        toastEventButton.AddClickDownEvnet(() => { OnClickButton(true); });
        toastEventButton.AddClickUpEvnet(() => { OnClickButton(false); });
    }
    private void OnClickButton(bool isDown)
    {
        if(marbleDeck)
        {
            int marbleIndex = marbleDeck.MarbleIdx;
            UIManager.Inst.OpenMarbleTipInfo(marbleIndex, isDown);
        }
    }
}
