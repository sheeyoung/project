using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.UI;

public class PlayButtonSound : MonoBehaviour
{
    public AudioClip audioClip;
    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
            return;
        }
        UIEventButton buttonEvent = GetComponent<UIEventButton>();
        if (buttonEvent != null)
        {
            buttonEvent.RemoveClickDownEvnet(OnClick);
            buttonEvent.AddClickDownEvnet(OnClick);
        }
	}

    public void OnClick()
    {
        SoundManager.Inst.PlayUISound(audioClip);
    }
	
}
