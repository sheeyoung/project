using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Libs.Unity;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;
using UI;
using Net;
using Platform;
using Net.Api;
using Libs.Utils;

public partial class UIManager : MonoBehaviour
{
    private static UIManager _inst = null;
    public static UIManager Inst
    {
        get
        {
            if (_inst == null)
            {
                GameObject go = Resources.Load<GameObject>("UI/Prefab/UIManager");
                GameObject obj = Instantiate<GameObject>(go);
                //MonoBehaviour.DontDestroyOnLoad(obj);
                _inst = obj.GetComponent<UIManager>();
            }
            return _inst;
        }
    }
    #region panelData
    public struct PanelData
    {
        public PanelRoot root;
        public bool isAutoClose;
        public bool isBack;
        public bool blockBack;
        public bool isBase;

        public PanelData(
            PanelRoot root,
            bool isAutoClose = false,
            bool isBack = true,
            bool blockBack = false,
            bool isBase = false)
        {
            this.isAutoClose = isAutoClose;
            this.isBack = isBack;
            this.blockBack = blockBack;
            this.root = root;
            this.isBase = isBase;
        }
    }
    #endregion
    #region Screen Size관련
    /// <summary>
    /// 원래 스크린 사이즈 저장이 완료되었는지.
    /// </summary>
    public bool OriginalScreenGetDone { get; private set; }
    public float OriginalScreenWidth { get; private set; }
    public float OriginalScreenHeight { get; private set; }
    public int ChangedScreenWidth { get; private set; }
    public int ChangedScreenHeight { get; private set; }

    /// <summary>
    /// Screen.SafeArea값. 시뮬레이션이 켜있다면, 해당값이 적용된값
    /// </summary>
    public Rect ScreenSafeArea { get; private set; }
    /// <summary>
    /// SafeArea크기가 원래 화면크기와 같다.
    /// </summary>
    public bool IsOriginalScreenSameWithSafeArea { get; private set; }
    /// <summary>
    /// 스크린 사이즈를 해상도 재 세팅을 완료 세팅했는지.
    /// </summary>
    public bool ScreenSetResolutionDone { get; private set; }
    /// <summary>
    /// Screen.setResolution기능을 사용하여,
    /// 해상도 고정을 사용할 것인지
    /// </summary>
    private bool fixScreenResolution = true;
    //가로 해상도 고정(세로뷰일때 사용)
    public int fixScreenWidthValue = 720;
    public bool limit_H_W_AspectRatio = true;
    public float limit_H_W_AspectRatioMax = 2.3f;
    public float limit_H_W_AspectRatioMin = 1.3f;

    IEnumerator ScrennSetting_Start()
    {
        //---
        //+ Screen 세팅
        // screen.width값을 제대로 가져오기 위해서 한프레임 쉰다.
        yield return null;

        if (!OriginalScreenGetDone)
        {
            OriginalScreenWidth = Screen.width;
            OriginalScreenHeight = Screen.height;
            OriginalScreenGetDone = true;
            if (Application.isEditor)
            {
                ScreenSafeArea = new Rect(0, 0, OriginalScreenWidth, OriginalScreenHeight);
                Debug.Log(ScreenSafeArea);
                IsOriginalScreenSameWithSafeArea = true;
            }
            else
            {
                // 아이폰 해상도의 경우 safeArea가 제대로 반환되지 않아, 임시로 하드코딩 해두었다.
#if UNITY_IOS
                    {
                        var __device = UnityEngine.iOS.Device.generation;
                        switch (__device)
                        {
                            case UnityEngine.iOS.DeviceGeneration.iPhoneX:
                                ScreenSafeArea = Screen.safeArea;
                                IsOriginalScreenSameWithSafeArea = false;
                                break;
                            case UnityEngine.iOS.DeviceGeneration.iPhoneXS:
                                ScreenSafeArea = new Rect(0, OriginalScreenHeight * 0.04f, OriginalScreenWidth, OriginalScreenHeight * 0.91f );
                                IsOriginalScreenSameWithSafeArea = false;
                                break;
                            case UnityEngine.iOS.DeviceGeneration.iPhoneXSMax:
                                ScreenSafeArea = new Rect(0, OriginalScreenHeight * 0.04f, OriginalScreenWidth, OriginalScreenHeight * 0.91f);
                                IsOriginalScreenSameWithSafeArea = false;
                                break;
                            case UnityEngine.iOS.DeviceGeneration.iPhoneXR:
                                ScreenSafeArea = new Rect(0, OriginalScreenHeight * 0.04f, OriginalScreenWidth, OriginalScreenHeight * 0.91f);
                                IsOriginalScreenSameWithSafeArea = false;
                                break;
                            case UnityEngine.iOS.DeviceGeneration.iPhone7:
                            case UnityEngine.iOS.DeviceGeneration.iPhone7Plus:
                            case UnityEngine.iOS.DeviceGeneration.iPhone8:
                            case UnityEngine.iOS.DeviceGeneration.iPhone8Plus:
                            case UnityEngine.iOS.DeviceGeneration.iPhone6:
                            case UnityEngine.iOS.DeviceGeneration.iPhone6Plus:
                                ScreenSafeArea = new Rect(0, 0, OriginalScreenWidth, OriginalScreenHeight);
                                IsOriginalScreenSameWithSafeArea = true;
                                break;
                            default:
                                ScreenSafeArea = Screen.safeArea;
                                IsOriginalScreenSameWithSafeArea = false;
                                break;
                        }

                    }
#elif UNITY_ANDROID
                    {
                        if (Screen.safeArea.x == 0f && Screen.safeArea.y == 0f && Screen.safeArea.width == OriginalScreenWidth && Screen.safeArea.height == OriginalScreenHeight)
                        {
                            ScreenSafeArea = new Rect(0, 0, OriginalScreenWidth, OriginalScreenHeight);
                            IsOriginalScreenSameWithSafeArea = true;
                        }
                        else
                        {
                            ScreenSafeArea = Screen.safeArea;
                            IsOriginalScreenSameWithSafeArea = false;
                        }
                    }
#endif
            }
        }

        if (!ScreenSetResolutionDone)
        {
            // 해상도 보정 사용
            if (fixScreenResolution)
            {
                int __targetWidth = fixScreenWidthValue;
                float __aspect = (float)Screen.height / Screen.width;
                if (limit_H_W_AspectRatio)
                {
                    if (__aspect > limit_H_W_AspectRatioMax) __aspect = limit_H_W_AspectRatioMax;
                    if (__aspect < limit_H_W_AspectRatioMin) __aspect = limit_H_W_AspectRatioMin;
                }
                ChangedScreenWidth = __targetWidth;
                ChangedScreenHeight = (int)(__targetWidth * __aspect);
                Screen.SetResolution(ChangedScreenWidth, ChangedScreenHeight, true);
                // Screen.SetResolution은 한프레임 후에 제대로 반영된다.
            }
            ScreenSetResolutionDone = true;
        }

    }
    #endregion
    public delegate void PanelEndAction();

    [SerializeField]
    UIDatabase uidatabase;
    public UIDatabase UIData { get { return uidatabase; } }

    public Canvas UICanvas { get {
            if (uiCanvas == null || uiCanvas.Count == 0)
                return null;
            if (activeDepth > 0)
                return uiCanvas[1];
            return uiCanvas[0]; } }

    public Camera UICamera { get
        {
            if (cameras == null || cameras.Count == 0)
                return null;
            if(activeDepth > 0)
                return cameras[1];
            return cameras[0];
        } }
    public List<Canvas> uiCanvas;
    public Canvas uiTopCanvas;
    public List<Camera> cameras;
    public List<Blur> blurs;

    // active panel
    private List<UIPanel> activePanels = new List<UIPanel>();

    private int activeDepth = 0;
    private const int CanvasDepth = 2;

    private Dictionary<UIPanel, UIBasePanel> loadpanel = new Dictionary<UIPanel, UIBasePanel>(new UIPanelComparer());
    private Dictionary<UIPanel, PanelData> panelTable = new Dictionary<UIPanel, PanelData>(new UIPanelComparer());


    [SerializeField] GameObject reporterPopupObject;
    private ReporterPopup reporterPopup;

    public enum AtlasName
    {
        EmotIcon,
        Enemy,
        LeagueIcon,
        MainMarble,
        MainMarbleNumber,
        UILobby,
        ShopIcon,
    }

    public void Init(bool tempScene = false)
    {
        if (uiCanvas == null)
        {
            uiCanvas = new List<Canvas>();
            blurs = new List<Blur>();
            cameras = new List<Camera>();
        }
        SetPanelTable();

        uiCanvas.Clear();
        blurs.Clear();
        cameras.Clear();

        for (int i = 0; i < CanvasDepth; i++)
        {
            if(tempScene)
            {
                string temp = "UIMainCanvas";
                string tempCamera = "Main Camera";
                GameObject canvasObject = GameObject.Find(temp);
                if (canvasObject) uiCanvas.Add(canvasObject.GetComponent<Canvas>());
                GameObject cameraObject = GameObject.Find(tempCamera);
                if (cameraObject)
                {
                    cameras.Add(cameraObject.GetComponent<Camera>());
                }
            }
            else
            {
                string canvasName = "PopupCanvas";
                if (i == 0)
                    canvasName = "UIMainCanvas";
                else
                    canvasName = canvasName + i.ToString();

                string cameraName = "Popup Camera";
                if (i == 0)
                    cameraName = "UIMainCamera";
                else
                    cameraName = cameraName + i.ToString();

                GameObject canvasObject = GameObject.Find(canvasName);
                if (canvasObject) uiCanvas.Add(canvasObject.GetComponent<Canvas>());
                GameObject cameraObject = GameObject.Find(cameraName);
                if (cameraObject)
                {
                    blurs.Add(cameraObject.GetComponent<Blur>());
                    cameras.Add(cameraObject.GetComponent<Camera>());
                }
            }
        }
        var topCanvasGo= GameObject.Find("PopupCanvas_Top");
        if (topCanvasGo) uiTopCanvas = topCanvasGo.GetComponent<Canvas>();

        loadpanel.Clear();
        activePanels.Clear();

        activeDepth = 0;

        InitDynamicUI();
        //NoticeManager.Inst.OnReload += ShowNotice;

#if UNITY_EDITOR || DEV
        //Debug 모드에서만 나오게 설정하기
        GameObject go = Instantiate(reporterPopupObject, uiCanvas[uiCanvas.Count -1].transform);
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        reporterPopup = go.GetComponent<ReporterPopup>();
#endif
    }

    private void SetPanelTable()
    {
        if (panelTable.Count > 0)
            return;
        //common
        panelTable.Add(UIPanel.CommonPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.TopNoticePopup, new PanelData(PanelRoot.Top, isBack: false, blockBack: false));

        //InGame
        panelTable.Add(UIPanel.IngamePanel, new PanelData(PanelRoot.Bottom, isBack: false, isBase:true));
        panelTable.Add(UIPanel.IngameResultPanel, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameEmotPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameSurrenderPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameBossSelectPopup, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.IngameBossInfoPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameBossInfoSimplePopup, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.IngameReconnectPopup, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.LoadingPopup, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.LoadingPanel, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.IngameCoOpResultPanel, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameResultWinPanel, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.IngameCoOpPanel, new PanelData(PanelRoot.Bottom, isBack: false, isBase: true));

        //Lobby
        panelTable.Add(UIPanel.LobbyPanel, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true, isBase: true));
        panelTable.Add(UIPanel.MarbleDeckEditPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.MarbleInfoPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.MarbleStatInfoPopup2, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.BoxPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.OptionPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.SortPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.DailyQuestPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.MailPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.CommonRewardPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.UserInfoPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.NickNamePopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.CommonRewardResultPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.RankingPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.LevelupPopup, new PanelData(PanelRoot.Bottom, isBack:false, blockBack:true));
        panelTable.Add(UIPanel.LeagueUpDownPopup, new PanelData(PanelRoot.Bottom, isBack: false, blockBack: true));
        panelTable.Add(UIPanel.BattleRecordPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.MarbleInfoTipPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.CoOpBattlePopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.PassPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.PassPurchasePopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.PassPointPopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.SubscribePopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.SubscribePurchasePopup, new PanelData(PanelRoot.Bottom));
        panelTable.Add(UIPanel.HelpPopup, new PanelData(PanelRoot.Bottom));
    }
    public GameObject CreateObject(GameObject create, Transform parent)
    {
        var go = Instantiate(create, new Vector3(0, 0, 0), Quaternion.identity, parent.transform);
        go.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        go.GetComponent<RectTransform>().localScale = Vector3.one;
        return go;
    }

    private GameObject FindPanelObject(UIPanel window)
    {
        for(int i = 0; i < uidatabase.PanelObject.Count; i++)
        {
            if (uidatabase.PanelObject[i].name.Equals(window.ToString()))
                return uidatabase.PanelObject[i];
        }
#if UNITY_EDITOR
        Debug.LogError("Not Found UIDatabase Panel");
#endif
        return null;
    }
    private GameObject FindPopupObject(UIPanel window)
    {
        for (int i = 0; i < uidatabase.PanelObject.Count; i++)
        {
            if (uidatabase.PanelObject[i].name.Equals(window.ToString()))
                return uidatabase.PanelObject[i];
        }
#if UNITY_EDITOR
        Debug.LogError("Not Found UIDatabase Panel");
#endif
        return null;
    }
    public void OpenPanel(UIPanel panel, params object[] param)
    {
        OpenPanelWithOpenEndEvent(panel, null, param);
    }
    public void OpenPanelWithOpenEndEvent(UIPanel panel, Action OpenEndEvent, params object[] param)
    {
        if (IsActive(panel))
            return;

        Transform parent = null;

        if(panelTable.ContainsKey(panel))
        {
            var panelDate = panelTable[panel];
            if (panelDate.root == PanelRoot.Top)
            {
                parent = uiTopCanvas.transform;
            }
            else
            {
                if(activeDepth > 0)
                {
                    parent = uiCanvas[1].transform;
                    var backPanel = activePanels[activePanels.Count - 1];
                    var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                    activePanel.RefreshEffect(false);
                    SetParent(uiCanvas[0].transform, activePanel.transform);
                }
                else
                {
                    blurs[0].enabled = true;
                    parent = uiCanvas[1].transform;
                    for(int i = 0; i < activePanels.Count; i++)
                    {
                        var backPanel = activePanels[i];
                        var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                        activePanel.RefreshEffect(false);
                    }
                }
                activeDepth++;
            }
        }
        else
        {
            if (activeDepth > 0)
            {
                parent = uiCanvas[1].transform;
                var backPanel = activePanels[activePanels.Count - 1];
                var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                activePanel.RefreshEffect(false);
                SetParent(uiCanvas[0].transform, activePanel.transform);
            }
            else
            {
                blurs[0].enabled = true;
                parent = uiCanvas[1].transform;
                for (int i = 0; i < activePanels.Count; i++)
                {
                    var backPanel = activePanels[i];
                    var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                    activePanel.RefreshEffect(false);
                }
            }
            activeDepth++;
        }

        if (loadpanel.ContainsKey(panel) == false)
        {
            var assetReference = FindPanelObject(panel);
            GameObject windowObj = Instantiate<GameObject>(assetReference, new Vector3(0, 0, 0), Quaternion.identity);
            SetParent(parent, windowObj.transform);

            UIBasePanel  basewindow = windowObj.GetComponent<UIBasePanel>();
            loadpanel.Add(panel, basewindow);
        }
        else
        {
            var openWindow = loadpanel[panel];
            SetParent(parent, openWindow.transform);
        }

        var showWindow = loadpanel[panel];
        activePanels.Add(panel);
        showWindow.OpenPanel(panel, param);
        

        OpenEndEvent?.Invoke();
    }
    public void OpenPanelBaseCanvas(UIPanel panel, Action OpenEndEvent, params object[] param)
    {
        if (IsActive(panel))
            return;

        if (loadpanel.ContainsKey(panel) == false)
        {
            var assetReference = FindPanelObject(panel);
            GameObject windowObj = Instantiate<GameObject>(assetReference, new Vector3(0, 0, 0), Quaternion.identity);
            SetParent(uiCanvas[0].transform, windowObj.transform);
#if UNITY_EDITOR
            Debug.LogFormat(">>>>> OpenPanelBaseCanvas >>>>>> Panel : {0}, Parent : {1}", panel.ToString(), uiCanvas[0].transform.name);
#endif

            UIBasePanel basePanel = windowObj.GetComponent<UIBasePanel>();
            loadpanel.Add(panel, basePanel);
        }

        var showPanel = loadpanel[panel];
        activePanels.Add(panel);
        showPanel.OpenPanel(panel, param);

        OpenEndEvent?.Invoke();
    }
    private void SetParent(Transform parent, Transform child)
    {
        child.SetParent(parent);
        child.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        child.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        child.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
        child.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        child.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    public void ClosePanel(UIPanel panel)
    {
        if (IsActive(panel))
        {
            activePanels.Remove(panel);
            
            activeDepth--;
            if (activeDepth < 0)
                activeDepth = 0;

            if(activeDepth > 0)
            {
                var backPanel = activePanels[activePanels.Count - 1];
                var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                activePanel.RefreshEffect(true);
                SetParent(uiCanvas[1].transform, activePanel.transform);
                if (blurs.Count > 0 && blurs[0] != null) blurs[0].enabled = true;
            }
            else
            {
                if(blurs.Count > 0 && blurs[0] != null) blurs[0].enabled = false;
                for (int i = 0; i < activePanels.Count; i++)
                {
                    var backPanel = activePanels[i];
                    var activePanel = GetActivePanel<UIBasePanel>(backPanel);
                    activePanel.RefreshEffect(true);
                }
            }
            
        }
    }

    public bool IsActive(UIPanel window)
    {
        return activePanels.Contains(window);
    }
    public T GetActivePanel<T>(UIPanel panel) where T : UIBasePanel
    {
        if (IsActive(panel) == false)
            return null;
        return (T)loadpanel[panel];
    }

    public bool SetSprite(Image imageItem, AtlasName atlasName, string imageName)
    {
        SpriteAtlas atlas = null;
        string strAtlasName = atlasName.ToString();
        for (int i = 0; i < uidatabase.spriteAtlas.Count; i++)
        {
            if (uidatabase.spriteAtlas[i].name.Equals(strAtlasName) == false)
                continue;
            atlas = uidatabase.spriteAtlas[i];
            break;
        }
        if(atlas != null && atlas.GetSprite(imageName) != null)
        {
            imageItem.gameObject.SetActive(true);
            imageItem.sprite = atlas.GetSprite(imageName);
            return true;
        }
        else
            imageItem.gameObject.SetActive(false);

        return false;
    }

    public bool SetSprite(SpriteRenderer imageItem, AtlasName atlasName, string imageName)
    {
        SpriteAtlas atlas = null;
        string strAtlasName = atlasName.ToString();
        for (int i = 0; i < uidatabase.spriteAtlas.Count; i++)
        {
            if (uidatabase.spriteAtlas[i].name.Equals(strAtlasName) == false)
                continue;
            atlas = uidatabase.spriteAtlas[i];
            break;
        }
        if (atlas != null && atlas.GetSprite(imageName) != null)
        {
            imageItem.gameObject.SetActive(true);
            imageItem.sprite = atlas.GetSprite(imageName);
            return true;
        }
        else
            imageItem.gameObject.SetActive(false);

        return false;
    }

    public void ClearAllWindow()
    {
        activePanels.Clear();
        foreach(var it in loadpanel.Values)
        {
            if (it.gameObject != null)
                Destroy(it.gameObject);
        }
        loadpanel.Clear();
    }
    public void CloseAllWindow()
    {
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            var panel = activePanels[i];
            if (panelTable.ContainsKey(panel) == false)
                continue;

            if (panelTable[panel].blockBack)
                break;
            if (panelTable[panel].isBack == false)
                continue;
            var activePanel = GetActivePanel<UIBasePanel>(panel);
            activePanel.ClosePanel();
        }
    }
    public void UpdateAsset()
    {
        foreach (var w in activePanels)
        {
            UIBasePanel panel = GetActivePanel<UIBasePanel>(w);
            if (panel != null)
                panel.UpdateAsset();
        }
    }

    #region ShowConfirmPopup
    public void ShowConfirm2BtnPopup(string title, string desc, string leftButtonText = "", Action leftButtonEvent = null, string rightButtonText = "", Action rightButtonEvent = null, bool isCloseBg = true, bool isAutoClose = false)
    {
        if (string.IsNullOrEmpty(leftButtonText))
            leftButtonText = UIUtil.GetText("UI_Common_Cancel");
        if (string.IsNullOrEmpty(rightButtonText))
            rightButtonText = UIUtil.GetText("UI_Common_Ok");
        OpenPanel(UIPanel.CommonPopup, title, desc, rightButtonText, rightButtonEvent, true, leftButtonText, leftButtonEvent, isCloseBg, isAutoClose);
    }

    public void ShowConfirm1BtnPopup(string title, string desc, string buttonText = "", Action buttonEvent = null, bool isCloseBg = true, bool isAutoClose = false)
    {
        if (string.IsNullOrEmpty(buttonText))
            buttonText = UIUtil.GetText("UI_Common_Ok");
        OpenPanel(UIPanel.CommonPopup, title, desc, buttonText, buttonEvent, false, "", null, isCloseBg, isAutoClose);
    }

    //시스템 팝업은 무조건 1버튼
    public void ShowSystemPopup(string title, string desc, string buttonText = "", Action endPanelEvent = null, bool closebg = true, bool autoClose = false)
    {
        Action endEvent = endPanelEvent;
        ShowConfirm1BtnPopup(title, desc, buttonText: buttonText, buttonEvent: endEvent, isCloseBg : closebg, isAutoClose: autoClose);
    }
    #endregion

    public void ShowNotice()
    {
        if (NoticeManager.Inst.Data != null)
        {
            OpenPanel(UIPanel.TopNoticePopup);
        }
    }

    private void CheckLobbyEscapeKey()
    {
        bool isBack = false;
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            var panel = activePanels[i];
            if (panelTable.ContainsKey(panel) == false)
                continue;

            if (panel == UIPanel.IngameReconnectPopup)
            {
                var activePanel = GetActivePanel<IngameReconnectPopup>(panel);
                if (activePanel != null)
                    activePanel.CancelWaitBattleStart();
            }
            else
            {
                if (panelTable[panel].blockBack)
                    break;
                if (panelTable[panel].isBack == false)
                    continue;
                var activePanel = GetActivePanel<UIBasePanel>(panel);
                activePanel.ClosePanel();
            }
            isBack = true;
            break;
        }
        if (isBack == false)
        {
            ShowGameQuitPopup();
        }
    }
    public void ShowGameQuitPopup()
    {
        string title = UIUtil.GetText("UI_Common_Exit_Title");
        string desc = UIUtil.GetText("UI_Common_Exit_Desc");
        ShowConfirm2BtnPopup(title, desc, rightButtonEvent: GameQuit);
        void GameQuit()
        {
            Application.Quit();
        }
    }

    private void CheckBattleEscapeKey()
    {
        bool isBack = false;
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            var panel = activePanels[i];
            if (panelTable.ContainsKey(panel) == false)
                continue;

            if (panelTable[panel].blockBack)
                break;
            if (panelTable[panel].isBack == false)
                continue;
            var activePanel = GetActivePanel<UIBasePanel>(panel);
            activePanel.ClosePanel();
            isBack = true;
            break;
        }
        if (isBack == false)
        {
            OpenPanel(UIPanel.IngameSurrenderPopup);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (scene.SceneLoader.Inst.GetActiveSceneName() == "GamePvP")
                CheckBattleEscapeKey();
            else
                CheckLobbyEscapeKey();
        }

        if (UIUtil.IsShowNotice)
        {
            if (uiTopCanvas != null)
            {
                ShowNotice();
            }
        }
    }

    public void ShowConnectingPanel(bool isActive)
    {
        if (isActive)
        {
            OpenPanel(UIPanel.LoadingPopup);
        }
        else
        {
            UIBasePanel go = GetActivePanel<UIBasePanel>(UIPanel.LoadingPopup);
            if (go == null)
                return;
            UIBasePanel basePanel = go.GetComponent<UIBasePanel>();
            basePanel.ClosePanel();
        }
    }
    public void ShowDailyShopPopup(Reward reward, int dailyIndex, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetDailyShop(reward, dailyIndex, title, desc, buttonText, buttonAction);
    }
    public void ShowBuyDescPopup(string title, string desc, ASSETS priceType, int priceValue, Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetBuyDesc(title, desc, priceType, priceValue, buttonAction);
    }
    public void ShowRewardPopup(Net.Api.RewardGains rewardGains, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetRewardGains(rewardGains, title, desc, buttonText, buttonAction);
    }
    public void ShowRewardPopup(Net.Api.Rewards rewards, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetRewards(rewards, title, desc, buttonText, buttonAction);
    }
    public void ShowBoxRewardPopup(int boxId, string title = "", string desc = "", string buttonText = "", Action buttonAction = null, Action closeAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetBoxID(boxId, title, desc, buttonText, buttonAction, closeAction);
    }
    public void ShowShopRewardPopup(int shopId, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetShopID(shopId, title, desc, buttonAction);
    }
    public void ShowInAppRewardPopup(int inAppID, bool isPackage, string title = "", string desc = "", string buttonText = "", Action buttonAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetInAppID(inAppID, isPackage, title, desc, buttonText, buttonAction);
    }
    public void ShowADRewardPopup(int boxID, string title = "", string desc = "", string buttonText = "", Action buttonAction = null, Action endAction = null)
    {
        OpenPanel(UIPanel.CommonRewardPopup);
        var rewardPopup = GetActivePanel<CommonRewardPopup>(UIPanel.CommonRewardPopup);
        rewardPopup.SetADID(boxID, title, desc, buttonText, buttonAction, endAction);
    }

    #region ADMob

    private Queue<AdMob.RESULT> ADQueue;
    public void ShowAD(string key, Action success, Action fail, Action notLoad)
    {
        if (ADQueue == null)
            ADQueue = new Queue<AdMob.RESULT>();
        ShowConnectingPanel(true);
        ImplBase.Actor.Parser<ADLoad.Ack>(new ADLoad.Req(), (ack) =>
        {
            Debug.Log(JsonLib.Encode(ack));
            try
            {
                if (User.Inst.Doc.ADLoadCount == 100)
                    Auth.Inst.SendSingularEvent("AD100");
                else if (User.Inst.Doc.ADLoadCount == 50)
                    Auth.Inst.SendSingularEvent("AD50");
                else if (User.Inst.Doc.ADLoadCount == 20)
                    Auth.Inst.SendSingularEvent("AD20");
                else if (User.Inst.Doc.ADLoadCount == 10)
                    Auth.Inst.SendSingularEvent("AD10");
                else if (User.Inst.Doc.ADLoadCount == 5)
                    Auth.Inst.SendSingularEvent("AD5");
            }catch(System.Exception ex)
            {
                Debug.Log(ex);
            }
        });
        ADQueue.Clear();
        StartCoroutine(CheckQueue(success, fail, notLoad));
        AdMob.Inst.Show(key, (result) =>
        {
            Debug.Log("AdMob.Inst.Show. Result:" + result.ToString());

            ADQueue.Enqueue(result);
        });
    }
    private IEnumerator CheckQueue(Action success, Action fail, Action notLoad)
    {
        WaitForSeconds waitSec = new WaitForSeconds(1);
        while(true)
        {
            if(ADQueue.Count > 0)
            {
                Debug.Log("CheckQueue");
                var adResult = ADQueue.Dequeue();

                ShowConnectingPanel(false);

                if (adResult == AdMob.RESULT.NOT_LOADED)
                {
                    notLoad?.Invoke();
                }    
                else if(adResult == AdMob.RESULT.OK)
                {
                    success?.Invoke();
                }
                else
                {
                    fail?.Invoke();
                }

                try
                {
                    if (User.Inst.Doc.ADCompleteCount == 100)
                        Auth.Inst.SendSingularEvent("AD100_end");
                    else if (User.Inst.Doc.ADCompleteCount == 50)
                        Auth.Inst.SendSingularEvent("AD50_end");
                    else if (User.Inst.Doc.ADCompleteCount == 20)
                        Auth.Inst.SendSingularEvent("AD20_end");
                    else if (User.Inst.Doc.ADCompleteCount == 10)
                        Auth.Inst.SendSingularEvent("AD10_end");
                    else if (User.Inst.Doc.ADCompleteCount == 5)
                        Auth.Inst.SendSingularEvent("AD5_end");
                }
                catch (System.Exception ex)
                {
                    Debug.Log(ex);
                }
                yield break;
            }

            yield return waitSec;
        }
    }
    #endregion



    public void RefreshLobbyAsset()
    {
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            var panel = activePanels[i];
            
            var activePanel = GetActivePanel<UIBasePanel>(panel);
            activePanel.UpdateAsset();
        }
    }

    public void ShowNotEnoughAssetPopup()
    {
        UIManager.Inst.ShowConfirm2BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Common_NeedAsset"), UIUtil.GetText("UI_Common_Cancel"), null, UIUtil.GetText("UI_Common_Ok"), GoToShop);
        void GoToShop()
        {
            //GoToMenu(LobbyBottomMenu.Shop);
            CloseAllWindow();
            var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UI.UIPanel.LobbyPanel);
            lobbyPanel.OnClickAssetButton(ASSETS.ASSET_FREE_DIAMOND);
        }
    }
    public void GoToMenu(LobbyBottomMenu menu)
    {
        Debug.Log("UImanager - GoToMenu");
        CloseAllWindow();
        var lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UI.UIPanel.LobbyPanel);
        lobbyPanel.GoToMenu(menu);
    }

    public void RefreshAllPanel()
    {
        for (int i = activePanels.Count - 1; i >= 0; i--)
        {
            var panel = activePanels[i];

            var activePanel = GetActivePanel<UIBasePanel>(panel);
            activePanel.Refresh();
        }
    }
    public bool IsBasePanel()
    {
        int topCount = activePanels.Count - 1;
        if (topCount < 0)
            return false;
        var item = activePanels[topCount];
        if (panelTable.ContainsKey(item) == false)
        {
            return false;
        }

        var panelDate = panelTable[item];
        return panelDate.isBase;

    }

    private MarbleInfoTipPopup marbleInfoTipPopup;
    public void OpenMarbleTipInfo(int marbleIndex, bool isOpen)
    {
        if(marbleInfoTipPopup == null)
        {
            var assetReference = FindPanelObject(UIPanel.MarbleInfoTipPopup);
            GameObject windowObj = Instantiate<GameObject>(assetReference, new Vector3(0, 0, 0), Quaternion.identity);
            SetParent(uiCanvas[1].transform, windowObj.transform);

            marbleInfoTipPopup = windowObj.GetComponent<MarbleInfoTipPopup>();
        }
        if(isOpen)
            marbleInfoTipPopup.OpenPanel(UIPanel.MarbleInfoTipPopup, marbleIndex);
        else
            marbleInfoTipPopup.ClosePanel();
    }
}
