using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MarbleDeckItem : MonoBehaviour
{
    //기본 정보
    [SerializeField] private Image marbleIcon;
    [SerializeField] private Text marbleGrade;
    [SerializeField] private Text marbleNameText;

    //마블정보
    [SerializeField] private Image progress;
    [SerializeField] private Text getCount;
    [SerializeField] private GameObject deckMark;
    [SerializeField] private GameObject gradeUpMark;
    [SerializeField] private GameObject SelectMark;

    [SerializeField] Button itemButton;

    [Header("Rarity")]
    [SerializeField] private Image decoImage;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image effImage;
    [SerializeField] private Image lineImage;
    [SerializeField] private Text rarityText;
    [Header("MarbleEff Size")]
    [SerializeField] Vector3 marbleEffSize = new Vector3(120f, 120f, 120f);

    private GameObject idleEff;
    private string idleEffName = "";

    public int DeckCount { get; private set; }
    
    public int MarbleIdx { get; private set; }
    private TBL.Sheet.CMarble marbleTable;

    public Action<int> ClickAction;
    public Func<int, bool> CheckSelectDeck;
    [SerializeField] bool isList;

    private void Start()
    {
        if (itemButton)
        {
            itemButton.onClick.AddListener(OnClickItem);
        }

    }
    public void SetData(int Marbleidx, int deckCount = -1)
    {
        if (Sheet.TBL.Marble.ContainsKey(Marbleidx) == false)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        MarbleIdx = Marbleidx;
        marbleTable = Sheet.TBL.Marble[Marbleidx];

        if(deckCount >= 0)
            this.DeckCount = deckCount;

        Refresh();
    }

    public void Refresh()
    {
        SetBaseMarbleInfo();
        SetStateMarbleInfo();
        SetSelectItem();
        SetMarbleRarityInfo();
        SetIdleEffect();
    }

    private void SetBaseMarbleInfo()
    {
        if (marbleIcon)
        {
            string marbleIconName = marbleTable.Icon;
            UIManager.Inst.SetSprite(marbleIcon, UIManager.AtlasName.MainMarble, marbleIconName);
        }
        if (marbleGrade)
        {
            int grade = 1;
            bool isMaxGrade = false;
            if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
            {
                grade = User.Inst.Doc.MarbleInven[MarbleIdx].Grade;
                if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleTable.Rarity))
                {
                    if (!Sheet.TBL.Gens.GradeUp[marbleTable.Rarity].ContainsKey(grade + 1))
                        isMaxGrade = true;
                }
            }
            marbleGrade.text = string.Format(UIUtil.GetText("UI_Common_Lv"), isMaxGrade ? UIUtil.GetText("UI_Common_Max") : grade.ToString());
        }
        var marbleLangInfo = User.Inst.Langs.Marble[MarbleIdx];
        if (marbleNameText)
            marbleNameText.text = marbleLangInfo.Name;
    }

    private void SetStateMarbleInfo()
    {

    }
    private void SetMarbleRarityInfo()
    {
        var rarityInfo = UIManager.Inst.UIData.GetMarbleRarityColorInfo((UI.MarbleRarity)marbleTable.Rarity);

        if (decoImage) decoImage.color = rarityInfo.DecoColor;
        if (bgImage) bgImage.color = rarityInfo.BGColor;
        if (effImage) effImage.color = rarityInfo.EffColor;
        if (lineImage) lineImage.color = rarityInfo.LineColor;
        if (rarityText)
        {
            rarityText.color = rarityInfo.RarityTextColor;
            rarityText.text = UIUtil.GetRarityString(marbleTable.Rarity);
        }
    }
    private void SetSelectItem()
    {
        if (SelectMark == null)
            return;
        if (CheckSelectDeck == null)
        {
            SelectMark.SetActive(false);
            return;
        }

        bool isSelect = CheckSelectDeck(DeckCount);
        SelectMark.SetActive(isSelect);
    }
    private void SetIdleEffect()
    {
        if (idleEff)
        {
            if (marbleTable.IdleEffect.Equals(idleEffName))
            {
                return;
            }
        }

        if (idleEff != null && marbleTable.IdleEffect.Equals(idleEffName) == false)
            Destroy(idleEff);

        if (string.IsNullOrEmpty(marbleTable.IdleEffect))
            return;
        idleEffName = marbleTable.IdleEffect;
        GameObject go = null;
        if(isList)
        {
            go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect+"_list");
        }
        else
        {
            go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect);
        }
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleIcon.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = marbleEffSize;
        }

    }

    public void OnClickItem()
    {
        ClickAction.Invoke(DeckCount);
    }

    public void RefreshEffect(bool isActive)
    {
        if(idleEff) idleEff.SetActive(isActive);
    }

}
