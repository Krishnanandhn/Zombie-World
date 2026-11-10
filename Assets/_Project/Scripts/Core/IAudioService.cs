namespace Project.Core
{
    public interface IAudioService
    {
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        void PlaySfx(SfxId id, float volume = 1f, float pitchVariance = 0.06f);
        void PlayMusic(MusicId id);
    }
}
