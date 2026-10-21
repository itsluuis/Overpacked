using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LogiPack.Core;

namespace LogiPack.Audio
{
    /// <summary>
    /// Gestor centralizado y persistente de audio (música dinámica y SFX) para Overpacked.
    /// Vive en DontDestroyOnLoad y adapta la música al modo y escena actual con crossfade suave.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PREF_MUSIC_VOLUME = "MusicVolume";
        private const string PREF_SFX_VOLUME = "SFXVolume";

        [Header("Pistas de Música Oficiales")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _tutorialMusic;
        [SerializeField] private AudioClip _infiniteMusic;

        [Header("Canales de Audio")]
        [SerializeField] private AudioSource _musicSourceA;
        [SerializeField] private AudioSource _musicSourceB;
        [SerializeField] private AudioSource _sfxSource;

        private AudioSource _activeMusicSource;
        private Coroutine _fadeCoroutine;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        public float MusicVolume => _musicVolume;
        public float SFXVolume => _sfxVolume;

        public AudioClip MenuMusic { get => _menuMusic; set => _menuMusic = value; }
        public AudioClip TutorialMusic { get => _tutorialMusic; set => _tutorialMusic = value; }
        public AudioClip InfiniteMusic { get => _infiniteMusic; set => _infiniteMusic = value; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureAudioSources();
            LoadVolumePreferences();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void Start()
        {
            // Reproducir la pista correspondiente a la escena inicial
            PlayMusicForCurrentScene(immediate: true);
        }

        private void EnsureAudioSources()
        {
            if (_musicSourceA == null)
            {
                _musicSourceA = gameObject.AddComponent<AudioSource>();
                _musicSourceA.loop = true;
                _musicSourceA.playOnAwake = false;
            }

            if (_musicSourceB == null)
            {
                _musicSourceB = gameObject.AddComponent<AudioSource>();
                _musicSourceB.loop = true;
                _musicSourceB.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            _activeMusicSource = _musicSourceA;
        }

        private void LoadVolumePreferences()
        {
            _musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, 1f);

            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (_activeMusicSource != null)
            {
                _activeMusicSource.volume = _musicVolume;
            }
            if (_sfxSource != null)
            {
                _sfxSource.volume = _sfxVolume;
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, _musicVolume);
            PlayerPrefs.Save();

            if (_fadeCoroutine == null && _activeMusicSource != null)
            {
                _activeMusicSource.volume = _musicVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PREF_SFX_VOLUME, _sfxVolume);
            PlayerPrefs.Save();

            if (_sfxSource != null)
            {
                _sfxSource.volume = _sfxVolume;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayMusicForCurrentScene(immediate: false);
        }

        public void PlayMusicForCurrentScene(bool immediate = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            AudioClip targetClip = null;

            if (sceneName == "MainMenuScene")
            {
                targetClip = _menuMusic;
            }
            else if (sceneName == "GameplayScene")
            {
                targetClip = GameModeManager.IsInfiniteMode ? _infiniteMusic : _tutorialMusic;
            }

            if (targetClip != null)
            {
                PlayMusicTrack(targetClip, immediate ? 0f : 0.8f);
            }
        }

        public void PlayMusicTrack(AudioClip newClip, float fadeDuration = 0.8f)
        {
            if (newClip == null) return;

            // Si ya está sonando esta pista exacta, no reiniciarla
            if (_activeMusicSource != null && _activeMusicSource.clip == newClip && _activeMusicSource.isPlaying)
            {
                return;
            }

            if (fadeDuration <= 0f)
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;

                if (_musicSourceA != null) _musicSourceA.Stop();
                if (_musicSourceB != null) _musicSourceB.Stop();

                _activeMusicSource = _musicSourceA;
                _activeMusicSource.clip = newClip;
                _activeMusicSource.volume = _musicVolume;
                _activeMusicSource.Play();
            }
            else
            {
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(CrossfadeMusicRoutine(newClip, fadeDuration));
            }
        }

        private IEnumerator CrossfadeMusicRoutine(AudioClip newClip, float duration)
        {
            AudioSource outgoingSource = _activeMusicSource;
            AudioSource incomingSource = (_activeMusicSource == _musicSourceA) ? _musicSourceB : _musicSourceA;

            incomingSource.clip = newClip;
            incomingSource.volume = 0f;
            incomingSource.Play();

            float timer = 0f;
            float startVol = outgoingSource != null ? outgoingSource.volume : 0f;

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(timer / duration);

                if (outgoingSource != null)
                {
                    outgoingSource.volume = Mathf.Lerp(startVol, 0f, t);
                }

                incomingSource.volume = Mathf.Lerp(0f, _musicVolume, t);
                yield return null;
            }

            if (outgoingSource != null)
            {
                outgoingSource.Stop();
                outgoingSource.volume = 0f;
            }

            incomingSource.volume = _musicVolume;
            _activeMusicSource = incomingSource;
            _fadeCoroutine = null;
        }

        /// <summary>
        /// Gancho preparado para reproducir efectos de sonido (SFX).
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume * volumeScale);
        }
    }
}
