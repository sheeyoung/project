using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using UnityEngine.UI;
using System;
using Libs.Unity;
using Platform;
using UnityEngine.SceneManagement;
using Net;
using Net.Api;
public class OptionPopup : UIBasePanel
{
    [Header("로그인정보")]
    [SerializeField] Text userIDText;
    [SerializeField] LoginButton hiveGrp;
    [SerializeField] LoginButton profileGrp;
    [SerializeField] LoginButton logoutGrp;
    [SerializeField] LoginButton googleGrp;
    [SerializeField] LoginButton facebookGrp;
    [Header("사운드")]
    [SerializeField] Slider bgmSlider;
    [SerializeField] GameObject bgmOn;
    [SerializeField] GameObject bgmOff;
    [Space(20)]
    [SerializeField] Slider effectSlider;
    [SerializeField] GameObject effectOn;
    [SerializeField] GameObject effectOff;
    [Header("기타")]
    [SerializeField] LoginButton allPushGrp;
    [Space(20)]
    [SerializeField] LoginButton gamePushGrp;
    [Space(20)]
    [SerializeField] LoginButton noticePushGrp;
    [Space(20)]
    [SerializeField] LoginButton nightPushGrp;
    [Space(20)]
    [SerializeField] LoginButton vibrationGrp;
    [Space(20)]
    [SerializeField] Button languageButton;
    [SerializeField] Text languageText;
    [Space(20)]
    [SerializeField] Button termsButton;
    [SerializeField] Button inquiryButton;
    [Space(20)]
    [SerializeField] Text versionText;

    // 언어  
    private string curLang = "";
    private SortingData languageSortData;

    [Serializable]
    class LoginButton
    {
        public Button loginButton;
        public GameObject onGrp;
        public Text onButtonText;
        public GameObject offGrp;
        public Text offButtonText;
    }
    protected override void Init(UIPanel panelId)
    {
        base.Init(panelId);
    }
    protected override void SetEvent()
    {
        base.SetEvent();
        if (hiveGrp != null && hiveGrp.loginButton) hiveGrp.loginButton.onClick.AddListener(OnClickHiveButton);
        if (profileGrp != null && profileGrp.loginButton) profileGrp.loginButton.onClick.AddListener(OnClickProfileButton);
        if (logoutGrp != null && logoutGrp.loginButton) logoutGrp.loginButton.onClick.AddListener(OnClickLogoutButton);

        if (googleGrp != null && googleGrp.loginButton) googleGrp.loginButton.onClick.AddListener(OnClickGoogleButton);
        if (facebookGrp != null && facebookGrp.loginButton) facebookGrp.loginButton.onClick.AddListener(OnClickFaceBookButton);

        if (bgmSlider) bgmSlider.onValueChanged.AddListener(OnChangeBgmVolume);
        if (effectSlider) effectSlider.onValueChanged.AddListener(OnChangeEffectVolume);

        if(gamePushGrp != null && gamePushGrp.loginButton) gamePushGrp.loginButton.onClick.AddListener(OnClickRemotePushButton);
        if (nightPushGrp != null && nightPushGrp.loginButton) nightPushGrp.loginButton.onClick.AddListener(OnClickNightPushButton);
        if (allPushGrp != null && allPushGrp.loginButton) allPushGrp.loginButton.onClick.AddListener(OnClickEntirePush);
        if (noticePushGrp != null && noticePushGrp.loginButton) noticePushGrp.loginButton.onClick.AddListener(OnClickLocalPush);

        if (vibrationGrp != null && vibrationGrp.loginButton) vibrationGrp.loginButton.onClick.AddListener(OnClickVibrationButton);

        if (languageButton) languageButton.onClick.AddListener(OnClickLanguageButton);
        if (termsButton) termsButton.onClick.AddListener(OnClickTermsButton);
        if (inquiryButton) inquiryButton.onClick.AddListener(OnClickInquaryButton);
    }

    public override void OpenPanel(UIPanel panelId, params object[] param)
    {
        base.OpenPanel(panelId, param);
        SetOptionData();
        if (versionText) versionText.text = string.Format("Version {0}", Application.version);
    }
    private void SetOptionData()
    {
        SetUID();
        SetHive();
        SetLoginButton();
        SetProfile();
        SetLogout();
        SetBGMVolumeSlider();
        SetEffectVolumeSlider();
        SetEntirePush();
        SetRemotePush();
        SetLocalPush();
        SetNightPush();
        SetVibration();
        SetLanguage();
    }

    private void SetUID()
    {
        if(userIDText) userIDText.text = string.Format(UIUtil.GetText("UI_Option_001"), User.Inst.UserID);
    }
    private void SetLoginButton()
    {
        try
        {
#if UNITY_ANDROID
            SetGoogleButton();
#elif UNITY_IOS
        //SetAppleButton();
#endif
            SetFaceBookButton();
        }
        catch
        {

        }
    }
    private void SetHive()
    {
        try
        {
            Debug.Log("SetHiveButton");
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.HIVE))
            {
                Debug.Log("SetHiveButton - On");
                //on
                hiveGrp.onGrp.SetActive(true);
                hiveGrp.offGrp.SetActive(false);
            }
            else
            {
                Debug.Log("SetHiveButton - Off");
                //off
                hiveGrp.onGrp.SetActive(false);
                hiveGrp.offGrp.SetActive(true);
            }
        }
        catch
        {

        }
        
    }
    private void SetProfile()
    {
        try
        {
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.HIVE))
            {
                Debug.Log("SetHiveProfileButton - On");
                //on
                profileGrp.onGrp.SetActive(true);
                profileGrp.offGrp.SetActive(false);
            }
            else
            {
                Debug.Log("SetHiveProfileButton - Off");
                //off
                profileGrp.onGrp.SetActive(true);
                profileGrp.offGrp.SetActive(false);
            }
        }
        catch
        {

        }
        
    }
    private void SetLogout()
    {
        try
        {
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GUEST)
                || GetAutoConnectCount() <= 0)
            {
                Debug.Log("##################### Guest");
                logoutGrp.onGrp.SetActive(false);
                logoutGrp.offGrp.SetActive(true);
                logoutGrp.loginButton.interactable = false;
            }
            else
            {
                Debug.Log("##################### Not Guest");
                logoutGrp.onGrp.SetActive(true);
                logoutGrp.offGrp.SetActive(false);
                logoutGrp.loginButton.interactable = true;
            }
        }
        catch
        { }
    }
    private void SetGoogleButton()
    {
        Debug.Log("SetGoogleButton");
        if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GOOGLE))
        {
            Debug.Log("SetGoogleButton-on");
            //on
            googleGrp.onGrp.SetActive(true);
            googleGrp.offGrp.SetActive(false);
        }
        else
        {
            //off
            googleGrp.onGrp.SetActive(false);
            googleGrp.offGrp.SetActive(true);
        }
    }
    private void SetFaceBookButton()
    {
        Debug.Log("SetFaceBookButton");
        if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.FACEBOOK))
        {
            Debug.Log("SetFaceBookButton - on");
            //on
            facebookGrp.onGrp.SetActive(true);
            facebookGrp.offGrp.SetActive(false);
        }
        else
        {
            Debug.Log("SetFaceBookButton - off");
            //off
            facebookGrp.onGrp.SetActive(false);
            facebookGrp.offGrp.SetActive(true);
        }
    }
    private void SetBGMVolumeSlider()
    {
        float volume = Utils.GetBgmVolume();
        if (bgmSlider)
        {
            bgmSlider.value = volume;
        }
        if (bgmOff) bgmOff.SetActive(volume <= 0);
        if (bgmOn) bgmOn.SetActive(volume > 0);
    }
    private void SetEffectVolumeSlider()
    {
        float volume = Utils.GetEffectVolume();
        if (effectSlider)
        {
            effectSlider.value = volume;
        }
        if (effectOff) effectOff.SetActive(volume <= 0);
        if (effectOn) effectOn.SetActive(volume > 0);
    }
    private void SetRemotePush()
    {
        try
        {
            bool isAgree = Push.Inst.RemotePush.isAgreeNotice;
            gamePushGrp.onGrp.SetActive(isAgree);
            gamePushGrp.offGrp.SetActive(isAgree == false);
            string pushButton = UIUtil.GetText("UI_Option_007");
            string pushOnOff = isAgree ? UIUtil.GetText("UI_Common_ON") : UIUtil.GetText("UI_Common_OFF");
            pushButton = string.Format(pushButton, pushOnOff);
            gamePushGrp.onButtonText.text = pushButton;
            gamePushGrp.offButtonText.text = pushButton;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetNightPush()
    {
        try
        {
            string country = "kr";

#if !UNITY_EDITOR
            country = hive.Configuration.getHiveCountry();
#endif
            country = country.ToLower();
            Debug.Log("Country Code : " + country);
            if (country.Equals("ko") == false && country.Equals("kr") == false)
            {
                nightPushGrp.loginButton.gameObject.SetActive(false);
                return;
            }
            nightPushGrp.loginButton.gameObject.SetActive(true);
            bool isAgree = Push.Inst.RemotePush.isAgreeNight;
            nightPushGrp.onGrp.SetActive(isAgree);
            nightPushGrp.offGrp.SetActive(isAgree == false);
            string pushButton = UIUtil.GetText("UI_Option_010");
            string pushOnOff = isAgree ? UIUtil.GetText("UI_Common_ON") : UIUtil.GetText("UI_Common_OFF");
            pushButton = string.Format(pushButton, pushOnOff);
            nightPushGrp.onButtonText.text = pushButton;
            nightPushGrp.offButtonText.text = pushButton;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetLocalPush()
    {
        try
        {
            bool isAgree = Push.Inst.LocalPush;
            noticePushGrp.onGrp.SetActive(isAgree);
            noticePushGrp.offGrp.SetActive(isAgree == false);
            string pushButton = UIUtil.GetText("UI_Option_028");
            string pushOnOff = isAgree ? UIUtil.GetText("UI_Common_ON") : UIUtil.GetText("UI_Common_OFF");
            pushButton = string.Format(pushButton, pushOnOff);
            noticePushGrp.onButtonText.text = pushButton;
            noticePushGrp.offButtonText.text = pushButton;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetEntirePush()
    {
        try
        {
            bool isAgree = Push.Inst.EntirePush;
            allPushGrp.onGrp.SetActive(isAgree);
            allPushGrp.offGrp.SetActive(isAgree == false);
            string pushButton = UIUtil.GetText("UI_Option_027");
            string pushOnOff = isAgree ? UIUtil.GetText("UI_Common_ON") : UIUtil.GetText("UI_Common_OFF");
            pushButton = string.Format(pushButton, pushOnOff);
            allPushGrp.onButtonText.text = pushButton;
            allPushGrp.offButtonText.text = pushButton;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    private void SetVibration()
    {
        try
        {
            bool isOn = VibrationManager.Inst.IsOn;
            vibrationGrp.onGrp.SetActive(isOn);
            vibrationGrp.offGrp.SetActive(isOn == false);

            string vibrationButton = UIUtil.GetText("UI_Option_008");
            string vibrationOnOff = isOn ? UIUtil.GetText("UI_Common_ON") : UIUtil.GetText("UI_Common_OFF");
            vibrationButton = string.Format(vibrationButton, vibrationOnOff);
            vibrationGrp.onButtonText.text = vibrationButton;
            vibrationGrp.offButtonText.text = vibrationButton;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
    private void SetLanguage()
    {
        var lang = AppClient.Inst.Language;
        string lText = UIUtil.GetText("UI_Option_011");
        if (!User.Inst.TBL.Global.ContainsKey(lang))
        {
            lang = AppClient.Inst.Language;
            PlayerPrefs.SetString("Language", lang);
            languageText.text = string.Format(lText, lang);
        }
        else
        {
            languageText.text = string.Format(lText, User.Inst.Langs.Global[lang].Name);
        }
        curLang = lang;
    }
    private void OpenLanguagePopup()
    {
        if (languageSortData == null)
        {
            languageSortData = new SortingData();
            languageSortData.sortNames = new List<string>();
            var it = User.Inst.Langs.Global.GetEnumerator();
            while (it.MoveNext())
                languageSortData.sortNames.Add(it.Current.Value.Name);
        }
        languageSortData.itemClickEvent = null;
        languageSortData.itemClickEvent += ItemClick;
        int currentIndex = 0;
        var lang = AppClient.Inst.Language;
        var itGlobal = User.Inst.Langs.Global.GetEnumerator();
        while (itGlobal.MoveNext())
        {
            if (lang == itGlobal.Current.Value.Index)
            {
                for (int i = 0; i < languageSortData.sortNames.Count; i++)
                {
                    if (languageSortData.sortNames[i].Equals(itGlobal.Current.Value.Name))
                        currentIndex = i;
                }
            }
        }
        itGlobal.Dispose();

        languageSortData.currentSortIndex = currentIndex;
        UIManager.Inst.OpenPanel(UIPanel.SortPopup, UIUtil.GetText("UI_Option_020"), string.Empty, null, languageSortData);
        void ItemClick(int index)
        {
            bool isChange = LanguagePopupEnd(index);
            if (isChange)
            {
                var sortPopup = UIManager.Inst.GetActivePanel<SortPopup>(UIPanel.SortPopup);
                sortPopup.OnClickClose();
            }
        }
    }
    private bool LanguagePopupEnd(int index)
    {
        string key = "";
        string langText = "";
        foreach (var item in User.Inst.Langs.Global)
        {
            if (item.Value.Name.Equals(languageSortData.sortNames[index]))
            {
                key = item.Key;
                langText = item.Value.Name;
                break;
            }
        }
        if (key.Equals(curLang))
            return false;
        string desc = string.Format(UIUtil.GetText("UI_Option_014"), langText);
        UIManager.Inst.ShowConfirm2BtnPopup(UIUtil.GetText("UI_Option_024"), desc, rightButtonEvent: ConfirmButtonClick);
        void ConfirmButtonClick()
        {
            Debug.Log("종료 다시시작");
            ChangeLanguage(key);
        }
        return true;
    }

    private async void ChangeLanguage(string lang)
    {
        AppClient.Inst.Language = lang;

        TBL.Manager.Inst.Reset();

        await TBL.Manager.Inst.Load(AppClient.Inst.Language, AppClient.Inst.TBL);

        SceneManager.LoadScene(1);

    }
    private int GetAutoConnectCount()
    {
        int loginCount = 0;

        foreach (var pair in Auth.Inst.PlayerInfo.providerInfoData)
        {
            loginCount++;
        }
        return loginCount;
    }

    private void SetLoading(bool isActive)
    {
        UIManager.Inst.ShowConnectingPanel(isActive);
    }

    private void OnChangeBgmVolume(float value)
    {
        SoundManager.Inst.SetBGMVolume(value);
        SetBGMVolumeSlider();
    }
    private void OnChangeEffectVolume(float value)
    {
        SoundManager.Inst.SetEffectVolume(value);
        SetEffectVolumeSlider();
    }

    public async void OnClickHiveButton()
    {
        try
        {
            Debug.LogFormat(">>>>>>> OnClickHiveButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.HIVE));
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.HIVE))
            {
                if (GetAutoConnectCount() <= 1)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Option_019"));
                    return;
                }
                var result = await Auth.Inst.DisconnectAsync(AUTH_TYPE.HIVE);
                if (result)
                {
                    SetOptionData();
                    if (GetAutoConnectCount() <= 0)
                    {                        
                        scene.SceneLoader.Inst.LoadScene("TitleScene");
                    }
                }
            }
            else
            {
                var result = await Auth.Inst.ConnectAsync(AUTH_TYPE.HIVE);
                if (result)
                {
                    SetOptionData();
                }
            }
        }
        finally
        {
            SetLoading(false);
            Debug.LogFormat("<<<<<<< OnClickHiveButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.HIVE));
        }
    }
    private void OnClickProfileButton()
    {
        SetLoading(true);
        long hiveId = Auth.Inst.PlayerInfo.playerId;// Convert.ToInt64(User.Inst.UserID);
        hive.AuthV4.showProfile(hiveId, (result) => {
            Debug.Log("AuthV4TestView.onAuthV4ShowProfile() Callback\nresult = " + result.toString() + "\n");
            SetLoading(false);
        });
    }

    public async void OnClickLogoutButton()
    {
        try
        {
            SetLoading(true);
            Debug.LogFormat(">>>>> OnClickLogoutButton()");
            var result = await Auth.Inst.SignOutAsync();
            Debug.LogFormat("Auth.Inst.SignOutAsync. result:{0}", result);
            if (result)
            {
                scene.SceneLoader.Inst.LoadScene("TitleScene");
            }
        }
        finally
        {
            SetLoading(false);
            Debug.LogFormat("<<<<<< OnClickLogoutButton().");

        }
    }

    public async void OnClickGoogleButton()
    {
        try
        {
            Debug.LogFormat(">>>>> OnClickGoogleButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GOOGLE));
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GOOGLE))
            {
                if (GetAutoConnectCount() <= 1)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Option_019"));
                    return;
                }
                var result = await Auth.Inst.DisconnectAsync(AUTH_TYPE.GOOGLE);
                if (result)
                {
                    SetOptionData();
                    if (GetAutoConnectCount() <= 0)
                    {
                        scene.SceneLoader.Inst.LoadScene("TitleScene");
                    }
                }
            }
            else
            {
                var result = await Auth.Inst.ConnectAsync(AUTH_TYPE.GOOGLE);
                if (result)
                {
                    SetOptionData();
                }
            }
        }
        finally
        {
            SetLoading(false);
            Debug.LogFormat("<<<<<< OnClickGoogleButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GOOGLE));

        }
    }
    public async void OnClickFaceBookButton()
    {
        try
        {
            Debug.LogFormat(">>>>>> OnClickFaceBookButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.FACEBOOK));
            if (Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.FACEBOOK))
            {
                if (GetAutoConnectCount() <= 1)
                {
                    UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText("UI_Common_Notice"), UIUtil.GetText("UI_Option_019"));
                    return;
                }
                var result = await Auth.Inst.DisconnectAsync(AUTH_TYPE.FACEBOOK);
                if (result)
                {
                    SetOptionData();
                    if (GetAutoConnectCount() <= 0)
                    {
                        scene.SceneLoader.Inst.LoadScene("TitleScene");
                    }
                }
            }
            else
            {
                var result = await Auth.Inst.ConnectAsync(AUTH_TYPE.FACEBOOK);
                if (result)
                {
                    SetOptionData();
                }
            }
        }
        finally
        {
            SetLoading(false);
            Debug.LogFormat("<<<<<<< OnClickFaceBookButton(). State:{0}", Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.FACEBOOK));
        }
    }
    private void OnClickEntirePush()
    {
        try
        {
            SetLoading(true);
            Push.Inst.EntirePush = Push.Inst.EntirePush? false : true;
            Push.Inst.ApplyRemoteSetting((result) =>
            {
                SetEntirePush();
                SetLocalPush();
                SetRemotePush();
                SetNightPush();
                SetLoading(false);

                string desc = "";
                string nowDate = Libs.Utils.TimeLib.Now.ToString("yyyy.MM.dd");
                if (Push.Inst.EntirePush)
                {
                    desc = UIUtil.GetText("UI_Option_023");
                }
                else
                {
                    desc = UIUtil.GetText("UI_Option_022");
                }
                desc = string.Format(desc, nowDate, "", UIUtil.GetText("UI_Option_029"));
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText(""), desc, buttonText: UIUtil.GetText("UI_Common_Ok"));
            });
        }
        catch (Exception)
        {
            SetLoading(false);
        }
    }
    private void OnClickLocalPush()
    {
        try
        {
            //if (Push.Inst.EntirePush == false)
            //    return;
            SetLoading(true);
            Push.Inst.LocalPush = Push.Inst.LocalPush ? false : true;
            Push.Inst.ApplyRemoteSetting((result) =>
            {
                SetEntirePush();
                SetLocalPush();
                SetRemotePush();
                SetNightPush();
                SetLoading(false);

                string desc = "";
                string nowDate = Libs.Utils.TimeLib.Now.ToString("yyyy.MM.dd");
                if (Push.Inst.LocalPush)
                {
                    desc = UIUtil.GetText("UI_Option_023");
                }
                else
                {
                    desc = UIUtil.GetText("UI_Option_022");
                }
                desc = string.Format(desc, nowDate, "", UIUtil.GetText("UI_Option_030"));
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText(""), desc, buttonText: UIUtil.GetText("UI_Common_Ok"));
            });
        }
        catch (Exception)
        {
            SetLoading(false);
        }
    }
    private void OnClickRemotePushButton()
    {
        try
        {
            //if (Push.Inst.EntirePush == false)
            //    return;
            SetLoading(true);
            Push.Inst.RemotePush.isAgreeNotice = Push.Inst.RemotePush.isAgreeNotice ? false : true;
            if(Push.Inst.RemotePush.isAgreeNotice == false)
                Push.Inst.RemotePush.isAgreeNight = false;
            Push.Inst.ApplyRemoteSetting((result) =>
            {
                SetEntirePush();
                SetLocalPush();
                SetRemotePush();
                SetNightPush();
                SetLoading(false);

                string desc = "";
                string nowDate = Libs.Utils.TimeLib.Now.ToString("yyyy.MM.dd");
                if (Push.Inst.RemotePush.isAgreeNotice)
                {
                    desc = UIUtil.GetText("UI_Option_023");
                }
                else
                {
                    desc = UIUtil.GetText("UI_Option_022");
                }
                desc = string.Format(desc, nowDate, "", UIUtil.GetText("UI_Option_025"));
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText(""), desc, buttonText: UIUtil.GetText("UI_Common_Ok"));
            });
        }
        catch (Exception)
        {
            SetLoading(false);
        }
    }
    
    private void OnClickNightPushButton()
    {
        try
        {
            //if (Push.Inst.EntirePush == false)
            //    return;
            if (Push.Inst.RemotePush.isAgreeNotice == false)
                return;
            SetLoading(true);
            Push.Inst.RemotePush.isAgreeNight = Push.Inst.RemotePush.isAgreeNight ? false : true;
            Push.Inst.ApplyRemoteSetting((result) =>
            {
                SetEntirePush();
                SetLocalPush();
                SetRemotePush();
                SetNightPush();
                SetLoading(false);

                string desc = "";
                string nowDate = Libs.Utils.TimeLib.Now.ToString("yyyy.MM.dd");
                if (Push.Inst.RemotePush.isAgreeNight)
                {
                    desc = UIUtil.GetText("UI_Option_023");
                }
                else
                {
                    desc = UIUtil.GetText("UI_Option_022");
                }
                desc = string.Format(desc, nowDate, "", UIUtil.GetText("UI_Option_026"));
                UIManager.Inst.ShowConfirm1BtnPopup(UIUtil.GetText(""), desc, buttonText:UIUtil.GetText("UI_Common_Ok"));
            });
        }
        catch (Exception)
        {
            SetLoading(false);
        }
    }
    private void OnClickVibrationButton()
    {
        VibrationManager.Inst.Toggle();
        SetVibration();
    }
    private void OnClickLanguageButton()
    {
        OpenLanguagePopup();
    }
    private void OnClickTermsButton()
    {
        hive.AuthV4.showTerms((result) => {
            Debug.Log("OptionPopup.onAuthV4ShowTerms() Callback\nresult = " + result.toString());
        });
    }
    private void OnClickInquaryButton()
    {
        // HIVE SDK 1:1 문의하기 요청
        hive.AuthV4.showInquiry((result) => {
            Debug.Log("hive.AuthV4.showInquiry. result : " + result.ToString());
        });
    }
    public void OnClickLicence()
    {
        Application.OpenURL("https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL");
    }
}
