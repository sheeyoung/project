using System.Collections.Generic;

namespace UI
{
    public enum UIPanel
    {
        //common
        CommonPopup,
        TopNoticePopup,

        //Ingame
        IngamePanel,
        IngameResultPanel,
        IngameEmotPopup,
        IngameSurrenderPopup,
        IngameBossSelectPopup,
        IngameBossInfoPopup,
        IngameBossInfoSimplePopup,
        IngameReconnectPopup,
        LoadingPopup,
        LoadingPanel,
        IngameResultWinPanel,
        IngameCoOpResultPanel,
        IngameCoOpPanel,
        IngameMarbleInfoPopup,

        //Lobby
        LobbyPanel,
        MarbleDeckEditPopup,
        MarbleInfoPopup,
        MarbleStatInfoPopup2,
        BoxPopup,
        OptionPopup,
        SortPopup,
        DailyQuestPopup,
        MailPopup,
        CommonRewardPopup,
        UserInfoPopup,
        NickNamePopup,
        CommonRewardResultPopup,
        RankingPopup,
        LevelupPopup,
        BattleRecordPopup,
        LeagueUpDownPopup,
        CoOpBattlePopup,
        FriendsCodeCreatePopup,
        FriendsCodeJoinPopup,
        FriendsBattlePopup,
        NewMarblePopup,
        BoxOpenPopup,
        MarbleInfoTipPopup,
        RankingLeaguePopup,
        PassPopup,
        PassPurchasePopup,
        PassPointPopup,
        SubscribePopup,
        SubscribePurchasePopup,
        HelpPopup,
    }
    public struct UIPanelComparer : IEqualityComparer<UIPanel>
    {
        public bool Equals(UIPanel a, UIPanel b) { return a == b; }
        public int GetHashCode(UIPanel a) { return (int)a; }
    }

    public enum PopUpID
    {

    }
    public enum PanelRoot
    {
        Bottom,
        Top
    }
    public enum LobbyBottomMenu
    {
        Shop = 0,
        Marble = 1,
        Battle = 2,
        Event,
        Clan
    }

    public enum MarbleRarity
    {
        Normal = 1,
        Rare = 2,
        Epic,
        Legendary,
        Chronicle
    }
    public enum AttackType
    {
        Physics = 1,
        Magic = 2,
        Buff = 3,
        Equip = 4
    }
    public enum Sorting
    {
        TheHead = 1,
        Random = 2,
        Buff = 3,

    }
    public enum NoticeLocalPushID
    {
        NOT_LOGIN_1 = 1,
        NOT_LOGIN_3 = 2,
        NOT_LOGIN_7 = 3,
        COOP_BOX = 4,
        MARBLE_PASS = 5,
    }
}