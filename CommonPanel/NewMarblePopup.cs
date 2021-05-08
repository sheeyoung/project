using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;
using System;

public class NewMarblePopup : UIBasePanel
{
    [SerializeField] private Image marbleImage;
    [Header("Title")]
    [SerializeField] private Text RarityText;
    [SerializeField] private Text NameText;
    [Header("Stat")]
    [SerializeField] private Text ATKText;
    [SerializeField] private Text ATKDelayText;
    [SerializeField] private Text DPSText;
    [SerializeField] private Text TargetText;
    [Header("Desc")]
    [SerializeField] private Text MarbleDescText;
    [Header("BGGrp")]
    [SerializeField] private GameObject BG_1;
    [SerializeField] private GameObject BG_2;
    [SerializeField] private GameObject BG_3;
    [SerializeField] private GameObject BG_4;
    [SerializeField] private GameObject BG_5;
    [SerializeField] private GameObject Eff_4;
    [SerializeField] private GameObject Eff_5;
    [Header("Group")]
    [SerializeField] private GameObject grandOpenGrp;
    [SerializeField] private GameObject newMarbleGrp;
    UIManager.PanelEndAction endAction;
    int marbleIndex;
    UIParticlePlay levelUpEff_1;
    UIParticlePlay levelUpEff_2;
    [SerializeField] Vector3 effSize = new Vector3(100f, 100f, 100f);
    [SerializeField] GameObject newMarkObject;

    private GameObject idleEff;
    AudioSource audioSource;

    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
    }
    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        endAction = null;
        if (param != null && param.Length >= 1)
            marbleIndex = (int)param[0];
        if (param != null && param.Length >= 2)
            endAction = (UIManager.PanelEndAction)param[1];
        if(param != null && param.Length >= 3)
        {
            bool isNew = (bool)param[2];
            newMarkObject.SetActive(isNew);
        }
        bool isGrandOpen = false;
        if (param != null && param.Length >= 4)
        {
            isGrandOpen = (bool)param[3];
        }

        if (Sheet.TBL.Marble.ContainsKey(marbleIndex) == false)
        {
            ClosePanel();
            return;
        }

        
        if (levelUpEff_1 == null)
        {
            levelUpEff_1 = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_NewMarblePopup_01");
            var effTranform = levelUpEff_1.Go.GetComponent<Transform>();
            effTranform.localScale = effSize;
            //levelUpEff_1.SetPosition(transform.position);
        }
        levelUpEff_1.Go.SetActive(false);

        if (levelUpEff_2 == null)
        {
            levelUpEff_2 = new UIParticlePlay(transform, "Prefab/Effect/UI/eff_UI_NewMarblePopup_02");
            var effTranform = levelUpEff_2.Go.GetComponent<Transform>();
            effTranform.localScale = effSize;
            //levelUpEff_2.SetPosition(transform.position);
        }
        levelUpEff_2.Go.SetActive(false);


        var marbleInfo = Sheet.TBL.Marble[marbleIndex];
        if (marbleInfo.Rarity >= (int)MarbleRarity.Legendary && isGrandOpen == false)
        {
            grandOpenGrp.SetActive(true);
            newMarbleGrp.SetActive(false);
            SetGrandOpen();
        }
        else
        {
            grandOpenGrp.SetActive(false);
            newMarbleGrp.SetActive(true);
            SetMarbleInfo();
        }
    }
    private void SetGrandOpen()
    {
        audioSource = SoundManager.Inst.PlayUIEffect(29);
        PlayAni("GradeOpen");
    }
    public void EndGrandOpen()
    {
        grandOpenGrp.SetActive(false);
        newMarbleGrp.SetActive(true);
        if (audioSource) audioSource.Stop();
        SetMarbleInfo();
    }
    private void SetMarbleInfo()
    {
        SoundManager.Inst.PlayUIEffect(24);
        PlayAni("NewMarble");
        var marbleInfo = Sheet.TBL.Marble[marbleIndex];
        var marbleLangInfo = Sheet.Langs.Marble[marbleIndex];
        var lobbyGradeTable = Sheet.TBL.Gens.LobbyGrade[marbleIndex][marbleInfo.InitialGrade];
        
        SetIdleEffect(marbleInfo);

        if (marbleImage) UIManager.Inst.SetSprite(marbleImage, UIManager.AtlasName.MainMarble, marbleInfo.Icon);

        if (RarityText) RarityText.text = UIUtil.GetRarityString(marbleInfo.Rarity);
        if (NameText) NameText.text = marbleLangInfo.Name;

        if (ATKText) ATKText.text = lobbyGradeTable.Attack.ToString("n0");
        if (ATKDelayText) ATKDelayText.text = lobbyGradeTable.Attack == 0 ? "0" : lobbyGradeTable.AtkDelay.ToString("f2");
        if (DPSText) DPSText.text = string.Format("{0} {1}", UIUtil.GetText("UI_Common_DPS"), (lobbyGradeTable.Attack == 0 || lobbyGradeTable.AtkDelay == 0) ? "0" : (lobbyGradeTable.Attack / lobbyGradeTable.AtkDelay).ToString("0.00"));
        if (TargetText) TargetText.text = UIUtil.GetSortingString(marbleInfo.Sorting, marbleInfo.HP);

        if (MarbleDescText)
        {
            MarbleDescText.text = marbleLangInfo.Marble_Desc;
        }

        if (BG_1) BG_1.SetActive(false);
        if (BG_2) BG_2.SetActive(false);
        if (BG_3) BG_3.SetActive(false);
        if (BG_4) BG_4.SetActive(false);
        if (BG_5) BG_5.SetActive(false);

        if (Eff_4) Eff_4.SetActive(false);
        if (Eff_5) Eff_5.SetActive(false);

        MarbleRarity rarity = (MarbleRarity)marbleInfo.Rarity;
        switch(rarity)
        {
            case MarbleRarity.Normal:
                if (BG_1) BG_1.SetActive(true);
                if (levelUpEff_1 != null)
                {
                    levelUpEff_1.Go.SetActive(true);
                    levelUpEff_1.Play();
                }
                break;
            case MarbleRarity.Rare:
                if (BG_2) BG_2.SetActive(true);
                if (levelUpEff_1 != null)
                {
                    levelUpEff_1.Go.SetActive(true);
                    levelUpEff_1.Play();
                }
                break;
            case MarbleRarity.Epic:
                if (BG_3) BG_3.SetActive(true);
                if (levelUpEff_1 != null)
                {
                    levelUpEff_1.Go.SetActive(true);
                    levelUpEff_1.Play();
                }
                break;
            case MarbleRarity.Legendary:
                if (BG_4) BG_4.SetActive(true);
                if (Eff_4) Eff_4.SetActive(true);
                if (levelUpEff_2 != null)
                {
                    levelUpEff_2.Go.SetActive(true);
                    levelUpEff_2.Play();
                }
                break;
            case MarbleRarity.Chronicle:
                if (BG_5) BG_5.SetActive(true);
                if (Eff_5) Eff_5.SetActive(true);
                if (levelUpEff_2 != null)
                {
                    levelUpEff_2.Go.SetActive(true);
                    levelUpEff_2.Play();
                }
                break;
        }

    }

    private void SetIdleEffect(TBL.Sheet.CMarble marbleInfo)
    {
        if (idleEff != null)
            Destroy(idleEff);

        if (string.IsNullOrEmpty(marbleInfo.IdleEffect))
            return;

        var go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleInfo.IdleEffect);
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleImage.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = new Vector3(310f, 310f, 310f);
        }
    }
    public override void ClosePanel()
    {
        base.ClosePanel();
        endAction?.Invoke();
    }

}
