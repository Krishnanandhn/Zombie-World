using System;

namespace Project.Data
{
    [Serializable]
    public sealed class ProfileData
    {
        public const string SaveKey = "zombie_survival_profile";
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public int BestScore;
        public int BestWave;
        public int TotalKills;
        public int Victories;
        public float MasterVolume = 0.8f;
        public float MusicVolume = 0.5f;
        public float SfxVolume = 0.9f;
    }
}
