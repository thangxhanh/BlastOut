using Dacodelaac.Utils;

namespace Dacodelaac.DataStorage
{
    public static partial class GameData
    {
        const string LAST_LOGIN_TIME_KEY = "LAST_LOGIN_TIME";
        const string LAST_ACTIVE_TIME_KEY = "LAST_ACTIVE_TIME";
        const string GAME_SESSION_COUNT = "game_session_count";
        const string COIN_KEY = "coin";
        public const string KEY_NEWVERSION = "KEY_NEWVERSION";
        
        public static double LastLoginTime
        {
            get => TimeUtils.TicksToSeconds(Storage.Get<long>(LAST_LOGIN_TIME_KEY));
            set => Storage.Set(LAST_LOGIN_TIME_KEY, TimeUtils.SecondsToTicks(value));
        }
        
        public static double LastActiveTime
        {
            get => TimeUtils.TicksToSeconds(Storage.Get<long>(LAST_ACTIVE_TIME_KEY));
            set => Storage.Set(LAST_ACTIVE_TIME_KEY, TimeUtils.SecondsToTicks(value));
        }

        public static int GameSessionCount
        {
            get => Storage.Get(GAME_SESSION_COUNT, 0);
            set => Storage.Set(GAME_SESSION_COUNT, value);
        }

        #region Ads Config

        public static bool NoAds
        {
            get => Storage.Get("NoAds", false);
            set => Storage.Set("NoAds", value);
        }


        /// <summary>1 = đã từng mua IAP. Dùng làm user property để tách tệp trên Firebase.</summary>
        public static int IsIapUser
        {
            get => Storage.Get("is_iap_user", 0);
            set => Storage.Set("is_iap_user", value);
        }

        /// <summary>Tổng số giao dịch IAP thành công.</summary>
        public static int IapCount
        {
            get => Storage.Get("iap_count", 0);
            set => Storage.Set("iap_count", value);
        }

        /// <summary>Tiền trong game. Lưu bằng int — kiểu nguyên thuỷ đi qua BinaryFormatter/IL2CPP
        /// an toàn hơn struct tự định nghĩa. Muốn hiển thị dạng 1.2K thì soi sang một
        /// ShortDoubleVariable (isSavable = 0), đừng lưu trực tiếp kiểu đó.</summary>
        public static int Coin
        {
            get => Storage.Get(COIN_KEY, 0);
            set => Storage.Set(COIN_KEY, value);
        }
        #endregion
    }
}