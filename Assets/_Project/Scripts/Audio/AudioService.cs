using System.Collections.Generic;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Audio
{
    // Pooled one-shot sources plus a single cross-fading music source. Volumes persist through ProfileData.
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        const int SourceCount = 20;
        const float MusicFadeSpeed = 1.6f;
        const float MinRepeatInterval = 0.025f;

        readonly Dictionary<SfxId, AudioClip> _sfx = new Dictionary<SfxId, AudioClip>();
        readonly Dictionary<MusicId, AudioClip> _music = new Dictionary<MusicId, AudioClip>();
        readonly Dictionary<SfxId, float> _lastPlayed = new Dictionary<SfxId, float>();

        AudioSource[] _sources;
        AudioSource _musicSource;
        ProfileData _profile;
        MusicId _currentMusic;
        MusicId _targetMusic;
        float _musicFade;
        int _nextSource;

        public float MasterVolume
        {
            get => _profile.MasterVolume;
            set => _profile.MasterVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => _profile.MusicVolume;
            set => _profile.MusicVolume = Mathf.Clamp01(value);
        }

        public float SfxVolume
        {
            get => _profile.SfxVolume;
            set => _profile.SfxVolume = Mathf.Clamp01(value);
        }

        public void Init(ProfileData profile)
        {
            _profile = profile;

            foreach (KeyValuePair<SfxId, AudioClip> pair in SfxLibrary.BuildAll()) _sfx[pair.Key] = pair.Value;
            _music[MusicId.Menu] = MusicLibrary.Build(MusicId.Menu);
            _music[MusicId.Combat] = MusicLibrary.Build(MusicId.Combat);
            _music[MusicId.Boss] = MusicLibrary.Build(MusicId.Boss);

            _sources = new AudioSource[SourceCount];
            for (int i = 0; i < SourceCount; i++)
            {
                var src = new GameObject("Sfx" + i).AddComponent<AudioSource>();
                src.transform.SetParent(transform, false);
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _sources[i] = src;
            }

            _musicSource = new GameObject("Music").AddComponent<AudioSource>();
            _musicSource.transform.SetParent(transform, false);
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.spatialBlend = 0f;
        }

        public void PlaySfx(SfxId id, float volume = 1f, float pitchVariance = 0.06f)
        {
            if (_sources == null || !_sfx.TryGetValue(id, out AudioClip clip)) return;

            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(id, out float last) && now - last < MinRepeatInterval) return;
            _lastPlayed[id] = now;

            AudioSource src = _sources[_nextSource];
            _nextSource = (_nextSource + 1) % _sources.Length;
            src.Stop();
            src.clip = clip;
            src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            src.volume = Mathf.Clamp01(volume * _profile.SfxVolume * _profile.MasterVolume);
            src.Play();
        }

        public void PlayMusic(MusicId id) => _targetMusic = id;

        void Update()
        {
            if (_musicSource == null) return;

            float dt = Time.unscaledDeltaTime;
            if (_currentMusic != _targetMusic)
            {
                _musicFade = Mathf.MoveTowards(_musicFade, 0f, MusicFadeSpeed * dt);
                if (_musicFade <= 0f)
                {
                    _currentMusic = _targetMusic;
                    _musicSource.Stop();
                    if (_music.TryGetValue(_currentMusic, out AudioClip clip) && clip != null)
                    {
                        _musicSource.clip = clip;
                        _musicSource.Play();
                    }
                }
            }
            else if (_currentMusic != MusicId.None)
            {
                _musicFade = Mathf.MoveTowards(_musicFade, 1f, MusicFadeSpeed * dt);
            }

            _musicSource.volume = _musicFade * _profile.MusicVolume * _profile.MasterVolume;
        }
    }
}
