using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Coffee.UIExtensions;
using System.Collections.Generic;

public class MarbleListItem : MonoBehaviour
{

    [Header("OnItem")]
    [SerializeField] private GameObject onItem;
    //기본 정보
    [SerializeField] private Image marbleIcon;
    [SerializeField] private Text marbleGrade;

    //마블정보
    [SerializeField] private Image progress;
    [SerializeField] private Text getCount;
    [SerializeField] private GameObject deckMark;
    [SerializeField] private GameObject gradeUpMark;
    [SerializeField] private GameObject SelectMark;
    [SerializeField] private List<Text> marbleNameTexts;

    [SerializeField] Button itemButtonOn;

    [Header("OffItem")]
    [SerializeField] private GameObject offItem;
    [SerializeField] private Image marbleIconOff;

    [SerializeField] Button itemButtonOff;

    [Header("EmptyItem")]
    [SerializeField] private GameObject emptyItem;
    [Header("Bg-Rarity")]
    [SerializeField] private UIGradient Bg;
    [SerializeField] private GameObject LegendaryorChronicleBG;

    private GameObject idleEff;
    private string idleEffName = "";
    public int MarbleIdx { get; private set; }
    private TBL.Sheet.CMarble marbleTable;

    public Action<int> ClickAction;
    public Func<int, bool> CheckSelect;
    public Action<Vector3, int> DragStartEvent;

    private enum ItemState
    {
        Empty,
        Off,
        On,
    }
    private ItemState currentItemState;

    private void Start()
    {
        if (itemButtonOn)
        {
            itemButtonOn.onClick.AddListener(OnClickItem);
        }
        if (itemButtonOff)
        {
            itemButtonOff.onClick.AddListener(OnClickItem);
        }
    }

    public virtual void SetData(int idx)
    {
        if(emptyItem) emptyItem.SetActive(false);
        if(offItem) offItem.SetActive(false);
        if(onItem) onItem.SetActive(false);
        if (SelectMark) SelectMark.SetActive(false);

        currentItemState = ItemState.Empty;
        if (Sheet.TBL.Marble.ContainsKey(idx) == false)
        {
            SetEmpty();
            return;
        }

        MarbleIdx = idx;
        marbleTable = Sheet.TBL.Marble[idx];

        if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
        {
            currentItemState = ItemState.On;
        }
        else
        {
            currentItemState = ItemState.Off;
        }
        SetItem(true);
    }

    public void Refresh()
    {
        if (currentItemState == ItemState.Empty)
            return;

        if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
        {
            currentItemState = ItemState.On;
        }
        else
        {
            currentItemState = ItemState.Off;
        }
        SetItem(false);
    }
    public void SetItem(bool isInit)
    {
        if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
        {
            if (onItem) onItem.SetActive(true);

            SetBaseMarbleInfo();
            SetStateMarbleInfo();
            SetSelectItem();
            SetGradeUpMark();
            SetDeckMark();
            if(isInit) SetIdleEffect();
        }
        else
        {
            if (offItem) offItem.SetActive(true);
            SetNotGain();
        }
    }

    private void SetEmpty()
    {
        if(emptyItem) emptyItem.SetActive(true);
    }

    private void SetNotGain()
    {
        if (marbleIconOff)
        {
            string marbleIconName = marbleTable.Icon;
            UIManager.Inst.SetSprite(marbleIconOff, UIManager.AtlasName.MainMarble, marbleIconName);
        }
        var marbleLangTableInfo = User.Inst.Langs.Marble[MarbleIdx];
        if (marbleNameTexts != null)
        {
            for(int i = 0; i< marbleNameTexts.Count; i++)
                marbleNameTexts[i].text = marbleLangTableInfo.Name;
        }
    }

    private void SetBaseMarbleInfo()
    {
        if (marbleIcon)
        {
            string marbleIconName = marbleTable.Icon;
            UIManager.Inst.SetSprite(marbleIcon, UIManager.AtlasName.MainMarble, marbleIconName);
        }

        int grade = 1;
        int curCount = 0, nextCount = 0;
        bool isMaxGrade = false;

        if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
        {
            grade = User.Inst.Doc.MarbleInven[MarbleIdx].Grade;
            curCount = User.Inst.Doc.MarbleInven[MarbleIdx].Count;
            if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleTable.Rarity))
            {
                if (Sheet.TBL.Gens.GradeUp[marbleTable.Rarity].ContainsKey(grade + 1))
                    nextCount = Sheet.TBL.Gens.GradeUp[marbleTable.Rarity][grade + 1].NeedMarble;
                else
                    isMaxGrade = true;
            }
        }

        if (marbleGrade) marbleGrade.text = string.Format(UIUtil.GetText("UI_Common_Lv"), isMaxGrade ? UIUtil.GetText("UI_Common_Max") : grade.ToString());
        if (progress && getCount)
        {
            if (nextCount > 0)
                progress.fillAmount = Mathf.Min((float)curCount / nextCount, 1);
            else
                progress.fillAmount = 0;
            getCount.text = string.Format("{0} / {1}", curCount, nextCount);
        }
        if(Bg)
        {
            var colorInfo = UIManager.Inst.UIData.GetRarityColor((UI.MarbleRarity)marbleTable.Rarity);
            Bg.color1 = colorInfo.topColor;
            Bg.color2 = colorInfo.BottomColor;
        }
        var marbleLangTableInfo = User.Inst.Langs.Marble[MarbleIdx];
        if (marbleNameTexts != null)
        {
            for (int i = 0; i < marbleNameTexts.Count; i++)
                marbleNameTexts[i].text = marbleLangTableInfo.Name;
        }
        if (LegendaryorChronicleBG)
            LegendaryorChronicleBG.SetActive(marbleTable.Rarity >= (int)UI.MarbleRarity.Legendary);
    }

    private void SetGradeUpMark()
    {
        int grade = 1;
        int curCount = 0, nextCount = 0;
        bool isMaxGrade = false;

        if (User.Inst.Doc.MarbleInven.ContainsKey(MarbleIdx))
        {
            grade = User.Inst.Doc.MarbleInven[MarbleIdx].Grade;
            curCount = User.Inst.Doc.MarbleInven[MarbleIdx].Count;
            if (Sheet.TBL.Gens.GradeUp.ContainsKey(marbleTable.Rarity))
            {
                if (Sheet.TBL.Gens.GradeUp[marbleTable.Rarity].ContainsKey(grade + 1))
                    nextCount = Sheet.TBL.Gens.GradeUp[marbleTable.Rarity][grade + 1].NeedMarble;
                else
                    isMaxGrade = true;
            }
        }
        if (isMaxGrade)
        {
            if (gradeUpMark) gradeUpMark.SetActive(false);
        }
        else
        {
            if (gradeUpMark) gradeUpMark.SetActive(nextCount <= curCount);
        }
    }
    private void SetDeckMark()
    {
        if(deckMark)
        {
            bool isDeck = false;
            var decks = User.Inst.Doc.Decks[User.Inst.Doc.SelectedDeck];
            foreach (var marbleItem in decks)
            {
                if (marbleItem.Value.Equals(MarbleIdx))
                {
                    isDeck = true;
                    break;
                }
            }
            deckMark.SetActive(isDeck);
        }
    }


    private void SetSelectItem()
    {
        if (SelectMark == null)
            return;
        if(CheckSelect == null)
        {
            SelectMark.SetActive(false);
            return;
        }

        bool isSelect = CheckSelect(MarbleIdx);
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

        var go = UIResourceManager.Inst.Load<GameObject>("Prefab/Effect/Battle/" + marbleTable.IdleEffect+"_list");
        if (go != null)
        {
            idleEff = GameObject.Instantiate(go) as GameObject;
            idleEff.transform.SetParent(marbleIcon.transform);
            Transform tfParticle = idleEff.transform;
            tfParticle.localPosition = new Vector3(0, 0, 0);
            tfParticle.localScale = new Vector3(120f, 120f, 120f);
        }
    }

    private void SetStateMarbleInfo()
    {

    }

    public void OnClickItem()
    {
        if (currentItemState == ItemState.Empty)
            return;
        ClickAction?.Invoke(MarbleIdx);
    }

    public void RefreshEffect(bool isActive)
    {
        if(idleEff) idleEff.SetActive(isActive);
    }
}
