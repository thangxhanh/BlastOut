namespace Dacodelaac.DataStorage
{
    public static partial class GameData
    {
        const string MUSIC_KEY = "MUSIC";
        const string SOUND_KEY = "SOUND";
        const string VIBRATION_KEY = "VIBRATION";

        public static bool Music
        {
            get => Storage.Get(MUSIC_KEY, true);
            set => Storage.Set(MUSIC_KEY, value);
        }
        
        public static bool Sound
        {
            get => Storage.Get(SOUND_KEY, true);
            set => Storage.Set(SOUND_KEY, value);
        }
        
        public static bool Vibration
        {
            get => Storage.Get(VIBRATION_KEY, true);
            set => Storage.Set(VIBRATION_KEY, value);
        }
    }
}