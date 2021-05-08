using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;
using UnityEngine.Events;
using System;
using DG.Tweening;
using Net;
using Net.Api;
using Net.Impl;
using Libs.Utils;
using Platform;
using UnityEngine.EventSystems;

public static class UIUtil
{
    public static bool showBanner = false;
    public static string GetRarityString(int rarity)
    {        
        string rarityString = string.Empty;
        switch((MarbleRarity)rarity)
        {
            case MarbleRarity.Normal:
                rarityString = GetText("UI_Common_Grade_001");
                break;
            case MarbleRarity.Rare:
                rarityString = GetText("UI_Common_Grade_002");
                break;
            case MarbleRarity.Epic:
                rarityString = GetText("UI_Common_Grade_003");
                break;
            case MarbleRarity.Legendary:
                rarityString = GetText("UI_Common_Grade_004");
                break;
            case MarbleRarity.Chronicle:
                rarityString = GetText("UI_Common_Grade_005");
                break;
        }
        return rarityString;
    }

    public static string GetAttackTypeString(int attackType)
    {
        string attackTypeString = string.Empty;
        switch((AttackType)attackType)
        {
            case AttackType.Physics:
                attackTypeString = GetText("UI_Common_AttackType_001");
                break;
            case AttackType.Magic:
                attackTypeString = GetText("UI_Common_AttackType_002");
                break;
            case AttackType.Buff:
                attackTypeString = GetText("UI_Common_AttackType_003");
                break;
            case AttackType.Equip:
                attackTypeString = GetText("UI_Common_AttackType_004");
                break;
        }
        return attackTypeString;
    }
    public static string GetSortingString(int sorting, int hp)
    {
        string sortingString = string.Empty;
        if (sorting == 0)
            sortingString = GetText("UI_Common_AttackTarget_000");
        else
        {
            if (hp == 0)
            {
                if (sorting == 1 || sorting == 3)
                    sortingString = GetText("UI_Common_AttackTarget_001");
                else
                    sortingString = GetText("UI_Common_AttackTarget_002");
            }
            else if (hp == 1)
                sortingString = GetText("UI_Common_AttackTarget_003");
            else
                sortingString = GetText("UI_Common_AttackTarget_004");
        }
        
        return sortingString;
    }
    public static float GetAbilityValueByValueName(int marbleIndex, int abilityId, string valueName, int grade)
    {
        var abilityInfo = Sheet.TBL.Ability[abilityId];        
        float retValue = 0f;
        switch (valueName)
        {
            case "Value1":
                retValue = abilityInfo.Value1;
                break;
            case "Value2":
                retValue = abilityInfo.Value2;
                break;
            case "Value3":
                retValue = abilityInfo.Value3;
                break;
            case "Value4":
                retValue = abilityInfo.Value4;
                break;
            case "Value5":
                retValue = abilityInfo.Value5;
                break;
            case "Value6":
                retValue = abilityInfo.Value6;
                break;
            case "Value7":
                retValue = abilityInfo.Value7;
                break;
            case "Value8":
                retValue = abilityInfo.Value8;
                break;
            case "Time":
                retValue = abilityInfo.Time;
                break;
            case "TickTime":
                retValue = abilityInfo.TickTime;
                break;
            default:
                break;
        }


        bool diffmarble = false;
        if (marbleIndex != abilityId)
        {
            if (!Sheet.TBL.Marble[marbleIndex].Ability.Exists(a => a == abilityId))
                diffmarble = true;
        }

        if (diffmarble)
        {
            if (Sheet.TBL.Marble.ContainsKey(abilityId))
            {
                if (Sheet.TBL.Marble[marbleIndex].MergeGroup == Sheet.TBL.Marble[abilityId].MergeGroup)
                    retValue += GetMarbleGradeAbilityValue(abilityId, abilityId, valueName, grade);
            }
        }
        else
            retValue += GetMarbleGradeAbilityValue(marbleIndex, abilityId, valueName, grade);

        return retValue;
    }

    public static float GetMarbleGradeAbilityValue(int marbleId, int abilityId, string valueName, int grade)
    {
        var gradeTable = Sheet.TBL.Gens.LobbyGrade[marbleId][grade];
        float val = 0f;
        for (int i = 0; i < gradeTable.AbilityID.Count; ++i)
        {
            if (gradeTable.AbilityID[i] != abilityId || gradeTable.Ability_Value[i] == 0)
                continue;
            if (((Net.Impl.ABILITY_PROP_KEY)gradeTable.Ability_Value[i]).ToString() == valueName)
                val += gradeTable.Ability_UPValue[i];
        }
        return val;
    }    

    public static void AddButtonClickEvent(GameObject obj, UnityAction callback)
    {
        Button btn = obj.GetComponent<Button>();
        if (btn == null)
            btn = obj.AddComponent<Button>();

        AddButtonClickEvent(btn, callback);
    }
    public static void AddButtonClickEvent(Button btn, UnityAction callback)
    {
        EnableRaycast(btn.transform);
        btn.onClick.AddListener(callback);
    }
    public static void EnableRaycast(Transform parent)
    {
        for (int i = 0, iMax = parent.childCount; i < iMax; i++)
        {
            Transform node = parent.GetChild(i);
            try
            {
                Graphic[] graphics = node.gameObject.GetComponents<Graphic>();
                for (int gi = 0; gi < graphics.Length; ++gi)
                {
                    Text text = graphics[gi].gameObject.GetComponent<Text>();
                    if (text != null)
                        graphics[gi].raycastTarget = false;
                    Image image = graphics[gi].gameObject.GetComponent<Image>();
                    if (image != null)
                        graphics[gi].raycastTarget = false;
                }

                EnableRaycast(node);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.Log("GetChild Error : " + ex);
#endif
            }
        }
    }

    public static List<DOTweenAnimation> GetTweenAllChild(Transform parent)
    {
        List<DOTweenAnimation> datas = new List<DOTweenAnimation>();
        if (parent.GetComponent<DOTweenAnimation>() != null)
            datas.Add(parent.GetComponent<DOTweenAnimation>());
        GetTweenChild(parent, datas);

        return datas;
    }

    private static void GetTweenChild(Transform parent, List<DOTweenAnimation> tweenAnis)
    {
        for (int i = 0, iMax = parent.childCount; i < iMax; i++)
        {
            Transform node = parent.GetChild(i);
            try
            {
                DOTweenAnimation[] tweens = node.gameObject.GetComponents<DOTweenAnimation>();
                for (int j = 0; j < tweens.Length; j++)
                {
                    tweenAnis.Add(tweens[j]);
                }

                GetTweenChild(node, tweenAnis);
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.Log("GetChild Error : " + ex);
#endif
            }
        }
    }

    public static void SetAssetIcon(Net.ASSETS asset, Image iconimage)
    {
        if(User.Inst.TBL.Assets.ContainsKey((int)asset) == false)
        {
            iconimage.gameObject.SetActive(false);
            return;
        }

        iconimage.gameObject.SetActive(true);
        string iconName = User.Inst.TBL.Assets[(int)asset].Icon;
        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.UILobby, iconName);
    }
    public static void SetRewardIcon(Net.REWARD_TYPE rewardType, Image iconimage, int rewardValue = 0)
    {
        if (User.Inst.TBL.Assets.ContainsKey((int)rewardType))
        {
            SetAssetIcon((Net.ASSETS)rewardType, iconimage);
            return;
        }
        if(User.Inst.TBL.RewardType.ContainsKey((int)rewardType) == false)
        {
            iconimage.gameObject.SetActive(false);
            return;
        }

        iconimage.gameObject.SetActive(true);

        switch(rewardType)
        {
            case REWARD_TYPE.REWARD_BOX:
            case REWARD_TYPE.REWARD_BOX_EXPAND:
                {
                    if (User.Inst.TBL.Box.ContainsKey(rewardValue))
                    {
                        string iconName = User.Inst.TBL.Box[rewardValue].BoxImage;
                        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.ShopIcon, iconName);
                    }
                }
                break;
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
                {
                    if(User.Inst.TBL.Marble.ContainsKey(rewardValue))
                    {
                        string marbleName = User.Inst.TBL.Marble[rewardValue].Icon;
                        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.MainMarble, marbleName);
                    }
                    else
                    {
                        string iconName = User.Inst.TBL.RewardType[(int)rewardType].Icon;
                        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.UILobby, iconName);
                    }
                }
                break;
            case REWARD_TYPE.REWARD_GOLD_GROW_UP:
                {
                    SetAssetIcon(Net.ASSETS.ASSET_FREE_GOLD, iconimage);
                }
                break;
            case REWARD_TYPE.REWARD_COOPBOX:
                {
                    bool isset = false;
                    if (User.Inst.TBL.BOX_Slot.ContainsKey(rewardValue))
                    {
                        int boxid = User.Inst.TBL.BOX_Slot[rewardValue].BoxID;
                        if (User.Inst.TBL.Box.ContainsKey(boxid))
                        {
                            string iconName = User.Inst.TBL.Box[boxid].BoxImage;
                            UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.ShopIcon, iconName);
                            isset = true;
                        }
                    }
                    else if(User.Inst.TBL.Box.ContainsKey(rewardValue))
                    {
                        string iconName = User.Inst.TBL.Box[rewardValue].BoxImage;
                        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.ShopIcon, iconName);
                        isset = true;
                    }
                    if (!isset)
                    {
                        string iconName = User.Inst.TBL.RewardType[(int)rewardType].Icon;
                        UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.UILobby, iconName);
                    }
                }
                break;
            default:
                {
                    string iconName = User.Inst.TBL.RewardType[(int)rewardType].Icon;
                    UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.UILobby, iconName);
                }
                break;
        }
    }

    public static Dictionary<string, string> LocalString = null;

    public static string GetText(string key)
    {
        if (User.Inst.Langs == null)
        {
            //if (pLocalMap != null && pLocalMap.ContainsKey(key))
            //    return pLocalMap[key];
            //else    
            if (LocalString == null)
            {
                var obj = Resources.Load<TextAsset>(string.Format("Data/text.{0}", AppClient.Inst.Language.ToLower()));
                if (obj == null)
                    return key;

                LocalString = JsonLib.Decode<Dictionary<string, string>>(obj.text);
            }

            if (LocalString != null)
            {
                if (LocalString.ContainsKey(key))
                    return LocalString[key];                
            }
            return key;
        }
        else
        {
            if (User.Inst.Langs.UI_Text.ContainsKey(key) == false)
            {
                return key;
            }
            else
            {
                return User.Inst.Langs.UI_Text[key].Value;
            }
        }
    }
    public static string ConvertSecToTimeString(long sec, int type = 1)
    {
        int hour = (int)sec / 3600;
        int minute = (int)sec % 3600 / 60;
        int second = (int)sec % 3600 % 60;
        string sHour = hour.ToString().Length < 2 ? 0 + hour.ToString() : hour.ToString();
        string sMinute = minute.ToString().Length < 2 ? 0 + minute.ToString() : minute.ToString();
        string sSecond = second.ToString().Length < 2 ? 0 + second.ToString() : second.ToString();

        switch (type)
        {
            case 1:
                return sHour + ":" + sMinute + ":" + sSecond;
            case 2:
                return sMinute + ":" + sSecond;
            case 3:
                if (second > 0)
                {
                    minute += 1;
                    sMinute = minute.ToString().Length < 2 ? 0 + minute.ToString() : minute.ToString();
                }
                return sHour + ":" + sMinute;
            case 4:
                if (hour > 0)
                    return string.Format(GetText("UI_Common_Hour"), sHour);
                else if (minute > 0)
                    return string.Format(GetText("UI_Common_Minute"), sMinute);
                else
                    return string.Format(GetText("UI_Common_Second"), sSecond);
        }

        return "";
    }
    public static string ConvertSecToDateString(long sec)
    {
        long day = (sec / 3600) / 24;

        if (day > 0)
        {
            return string.Format(GetText("UI_Common_Day"), day);
        }
        else
        {
            int hour = (int)sec / 3600;
            int minute = (int)sec % 3600 / 60;
            int second = (int)sec % 3600 % 60;
            string sHour = hour.ToString().Length < 2 ? 0 + hour.ToString() : hour.ToString();
            string sMinute = minute.ToString().Length < 2 ? 0 + minute.ToString() : minute.ToString();
            string sSecond = second.ToString().Length < 2 ? 0 + second.ToString() : second.ToString();
            if (hour > 0)
                return string.Format(GetText("UI_Common_Hour"), sHour);
            else if (minute > 0)
                return string.Format(GetText("UI_Common_Minute"), sMinute);
            else
                return string.Format(GetText("UI_Common_Second"), sSecond);
        }
    }
    public static string GetRewardName(Net.REWARD_TYPE rewardType, int rewardValue = 0)
    {
        if (User.Inst.Langs.Assets.ContainsKey((int)rewardType))
        {
            return User.Inst.Langs.Assets[(int)rewardType].UIName;
        }
        if (User.Inst.Langs.RewardType.ContainsKey((int)rewardType) == false)
        {
            return string.Empty;
        }

        switch (rewardType)
        {
            case REWARD_TYPE.REWARD_BOX:
                {
                    if (User.Inst.Langs.Box.ContainsKey(rewardValue))
                    {
                        return User.Inst.Langs.Box[rewardValue].Name;
                    }
                }
                break;
            case REWARD_TYPE.REWARD_MARBLE:
            case REWARD_TYPE.REWARD_MARBLE_NORMAL:
            case REWARD_TYPE.REWARD_MARBLE_UNCOMMON:
            case REWARD_TYPE.REWARD_MARBLE_RARE:
            case REWARD_TYPE.REWARD_MARBLE_LEGEND:
            case REWARD_TYPE.REWARD_MARBLE_MYTH:
                {
                    if (User.Inst.Langs.Marble.ContainsKey(rewardValue))
                    {
                        return User.Inst.Langs.Marble[rewardValue].Name;
                    }
                }
                break;
            case REWARD_TYPE.REWARD_GOLD_GROW_UP:
                {
                    if(User.Inst.Langs.Assets.ContainsKey((int)REWARD_TYPE.ASSET_FREE_GOLD))
                        return User.Inst.Langs.Assets[(int)REWARD_TYPE.ASSET_FREE_GOLD].UIName;
                }
                break;
            case REWARD_TYPE.REWARD_COOPBOX:
                {
                    if(User.Inst.TBL.BOX_Slot.ContainsKey(rewardValue))
                    {
                        int boxId = User.Inst.TBL.BOX_Slot[rewardValue].BoxID;
                        if (User.Inst.Langs.Box.ContainsKey(boxId))
                        {
                            return User.Inst.Langs.Box[boxId].Name;
                        }
                    }
                    else if(User.Inst.Langs.Box.ContainsKey(rewardValue))
                    {
                        return User.Inst.Langs.Box[rewardValue].Name;
                    }
                }
                break;
        }
        return User.Inst.Langs.RewardType[(int)rewardType].UIName;
    }
    public static GameObject SetParent(GameObject parent, GameObject go, int layer = -1)
    {
        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            var rectTranform = go.GetComponent<RectTransform>();
            rectTranform.localScale = Vector3.one;
            rectTranform.localPosition = Vector3.zero;
            rectTranform.localRotation = Quaternion.identity;

            rectTranform.anchoredPosition = Vector2.zero;


            if (layer == -1) go.layer = parent.layer;
            else if (layer > -1 && layer < 32) go.layer = layer;

            go.SetActive(false);
            go.SetActive(true);
        }
        return go;
    }
    public static REWARD_TYPE GetRewardType(string rewardType)
    {
        return (REWARD_TYPE)System.Enum.Parse(typeof(REWARD_TYPE), rewardType);
    }
    public static ASSETS GetAssetsType(string assets)
    {
        return (ASSETS)System.Enum.Parse(typeof(ASSETS), assets);
    }

    public static string getProductCode(TBL.Sheet.CInAppShop desc, INAPP_TYPE market)
    {
        switch (market)
        {
            case INAPP_TYPE.TEST_STORE:
            case INAPP_TYPE.GOOGLE_PLAYSTORE:
                return desc.GooglePlay;
            case INAPP_TYPE.APPLE_APPSTORE:
                return desc.AppStore;
            default:
                return string.Empty;
        }
    }

    public static bool IsToday(long date)
    {
        DateTime questDate = TimeLib.ConvertTo(date);
        DateTime nowDate = TimeLib.Now;
        if (nowDate.Year == questDate.Year && nowDate.Month == questDate.Month && nowDate.Day == questDate.Day)
        {
            return true;
        }
        return false;
    }

    public static bool IsShowNotice = false;
    public static void ShowNotice()
    {
        IsShowNotice = true;
    }
    public static void LoadNotice()
    {
        IsShowNotice = false;
    }

    public static int GetRewardCount(REWARD_TYPE rewardType, int rewardCount)
    {
        switch(rewardType)
        {
            case REWARD_TYPE.REWARD_GOLD_GROW_UP:
                {
                    var reward = BaseImpl.Inst.GetReward(User.Inst, rewardType, rewardCount, 0);
                    return reward.Count;
                }
/*
            case REWARD_TYPE.REWARD_GOLD_GROW_UP_C:
                {
                    var reward = BaseImpl.Inst.GetReward(User.Inst, rewardType, rewardCount, 0);
                    return reward.Count;
                }
*/
        }
        return rewardCount;
    }
    public static bool IsGameToLobby = false;

    public static float GetConstValue(string key)
    {
        if (User.Inst.TBL.Const.ContainsKey(key) == false)
            return 0f;
        return User.Inst.TBL.Const[key].Value;
    }
    public static string GetUserName(string name)
    {
        if (User.Inst.Langs.UI_Text.ContainsKey(name) == false)
            return name;
        return User.Inst.Langs.UI_Text[name].Value;
    }
    #region league
    public static string GetLeagueName(long score)
    {
        int idx = GetLeagueIndex(score);

        return Sheet.Langs.PvPLeagueName[idx].LeagueName;
    }
    public static int GetLeagueIndex(long score)
    {
        int idx = 1;
        foreach (var l in Sheet.TBL.PvPLeagueName)
        {
            if (l.Value.NeedPoint > score)
                break;
            idx = l.Value.Index;
        }

        return idx;
    }
    #endregion
    public static void SetLevelIcon(int level, Image iconimage)
    {
        if (iconimage == null)
            return;
        if (User.Inst.TBL.Account_EXP.ContainsKey(level) == false)
        {
            iconimage.gameObject.SetActive(false);
            return;
        }

        iconimage.gameObject.SetActive(false);
        //string iconName = User.Inst.TBL.Account_EXP[level].LevelIcon;
        //UIManager.Inst.SetSprite(iconimage, UIManager.AtlasName.UILobby, iconName);
    }
    public static void SetLeagueIcon(long score, Image iconImage)
    {
        int index = UIUtil.GetLeagueIndex(score);
        var tableInfo = User.Inst.TBL.PvPLeagueName[index];
        if (iconImage) UIManager.Inst.SetSprite(iconImage, UIManager.AtlasName.LeagueIcon, tableInfo.LeagueIcon);
    }

    #region league연출
    public static bool IsShowLeague = false;
    public static int PreLeagueScore;
    #endregion

    #region special text 
    public static bool CheckSpecialText(string text)
    {
        Debug.Log("nickname : "+text);
        string str = @"[[~/\s!@\#$%^&*\()\-_=+|\\/:;?""<>']|[\u2700-\u27BF]|[\uE000-\uF8FF]|uD83C[\uDC00-\uDFFF]|[\u2011-\u26FF]|\uD83E[\uDD10-\uDDFF]|\uD83D[\uDE00-\uDEFF]|[\uD83C-\uDBFF\uDC00-\uDFFF]+";           
        System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);
        return rex.IsMatch(text);
    }
    #endregion
    #region 인게임에서 얻은재화 획득연출
    public static List<REWARD_TYPE> inGameGetRewards = new List<REWARD_TYPE>();
    public static void AddShowInGameReward(REWARD_TYPE addRewardType)
    {
        if (inGameGetRewards.Contains(addRewardType))
            return;
        inGameGetRewards.Add(addRewardType);
    }
    public static void ClearShopInGameReward()
    {
        inGameGetRewards.Clear();
    }
    #endregion

    #region 계정 생성 후 최초 협동전 플레이 여부
    public static bool IsFirstBattle = false;
    #endregion
    #region 게스트 체크
    public static bool IsGuestUser()
    {
        if (Auth.Inst == null || Auth.Inst.PlayerInfo == null)
            return true;
        int loginCount = 0;

        foreach (var pair in Auth.Inst.PlayerInfo.providerInfoData)
        {
            loginCount++;
        }

        return Auth.Inst.PlayerInfo.providerInfoData.ContainsKey(hive.AuthV4.ProviderType.GUEST) || loginCount <= 0;
    }
    #endregion
    #region UI 오브젝트 위에 마우스 커서가 있는지 체크
    public static bool IsPointerOverUIObject(Vector3 pos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(pos.x, pos.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    #endregion
    #region 레이어 변경
    public static void SetLayer(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;
        for (int i = 0; i < parent.childCount; ++i)
            SetLayer(parent.GetChild(i), layer);
    }
    #endregion
    #region 마블패스
    public static int GetMarblePassSeasonIndex()
    {
        int index = User.Inst.Doc.MarblePass.Idx;
        foreach(var item in User.Inst.TBL.MarblePassSeason)
        {
            var openTime = TimeLib.ConvertTo(item.Value.StartTime);
            var endTime = TimeLib.ConvertTo(item.Value.EndTime);
            var now = TimeLib.Now;
            if (DateTime.Compare(now, openTime) > 0 && DateTime.Compare(endTime, now) > 0)
            {
                index = item.Value.Index;
                break;
            }
            if(item.Value.Index > User.Inst.Doc.MarblePass.Idx && DateTime.Compare(endTime, now) <= 0)
            {
                index = item.Value.Index;
            }
        }
        return index;
    }
    public static bool CheckMarblePassSeasons()
    {
        bool isSeason = false;
        int index = User.Inst.Doc.MarblePass.Idx;
        foreach (var item in User.Inst.TBL.MarblePassSeason)
        {
            var openTime = TimeLib.ConvertTo(item.Value.StartTime);
            var endTime = TimeLib.ConvertTo(item.Value.EndTime);
            var now = TimeLib.Now;
            if (DateTime.Compare(now, openTime) > 0 && DateTime.Compare(endTime, now) > 0)
            {
                isSeason = true;
                break;
            }
        }
        return isSeason;
    }
    #endregion
    #region 협동전 친구대전
    public static bool isCoOpFriend = false;
    #endregion
    #region 구독체크
    public static bool CheckJoinSubscribe()
    {
        //bool isGain = BaseImpl.Inst.CheckJoinSubscribe(User.Inst, TimeLib.Seconds);
        //int index = (int)UIUtil.GetConstValue("CONST_SUBSCRIBE_SHOPID");
        //var desc = User.Inst.TBL.InAppShop[index];
        //var code = UIUtil.getProductCode(desc, IAP.Inst.Market);

        //foreach (var item in User.Inst.Doc.InApp)
        //{
        //    if (item.Value.Code.Equals(code))
        //    {
        //        var subscribeItem = item.Value;
        //        var subscribeExpire = TimeLib.ConvertTo(subscribeItem.ExpireDate);
        //        var now = TimeLib.Now;

        //        if (subscribeItem.State == 3 && subscribeExpire.CompareTo(now) > 0)
        //            isGain = true;
        //        break;
        //    }
        //}
        //return isGain;
        return BaseImpl.Inst.CheckJoinSubscribe(User.Inst, TimeLib.Seconds);
    }
    #endregion
}
