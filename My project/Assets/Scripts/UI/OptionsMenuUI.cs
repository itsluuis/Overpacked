using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using LogiPack.Audio;
using LogiPack.Core;
using LogiPack.Managers;

namespace LogiPack.UI
{
    /// <summary>
    /// Controlador del panel de Opciones y menú de Pausa.
    /// Funciona tanto en el Menú Principal como en la escena de Gameplay.
    /// Controla sliders de volumen para Música y SFX (0% a 100%) y la pausa de juego.
    /// </summary>
    public class OptionsMenuUI : MonoBehaviour
    {
        [Header("Contenedor Principal y Overlay")]
        public GameObject ContentPanel;
        public Image OverlayBackground;

        [Header("Controles de Música")]
        public Slider MusicSlider;
        public Text MusicValueText;

        [Header("Controles de SFX")]
        public Slider SFXSlider;
        public Text SFXValueText;

        [Header("Botones de Acción")]
        public Button ResumeButton;          // "SEGUIR CON LA PARTIDA" (Gameplay)
        public Button ReturnToMenuButton;    // "SALIR AL MENÚ" (Gameplay)
        public Button BackButton;            // "◀ VOLVER" (Menú Principal)

        [Header("Configuración")]
        [SerializeField] private bool _isGameplayScene = false;

        public bool IsOpen => ContentPanel != null && ContentPanel.activeSelf;

        private void Awake()
        {
            if (ResumeButton != null)
            {
                ResumeButton.onClick.AddListener(OnClickResume);
            }

            if (ReturnToMenuButton != null)
            {
                ReturnToMenuButton.onClick.AddListener(OnClickReturnToMenu);
            }

            if (BackButton != null)
            {
                BackButton.onClick.AddListener(OnClickBack);
            }

            if (MusicSlider != null)
            {
                MusicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            }

            if (SFXSlider != null)
            {
                SFXSlider.onValueChanged.AddListener(OnSFXSliderChanged);
            }
        }

        private void Start()
        {
            SyncSlidersFromAudio();
        }

        private void Update()
        {
            // Capturar la tecla ESC vía New Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Si la escena está transicionando, ignorar ESC
                if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
                {
                    return;
                }

                // Si estamos en Gameplay y el modal de fin de jornada está activo, no abrir pausa
                if (_isGameplayScene && IsDayEndModalActive())
                {
                    return;
                }

                ToggleOptions();
            }
        }

        private bool IsDayEndModalActive()
        {
            var dayEnd = FindAnyObjectByType<DayEndSummaryUI>();
            return dayEnd != null && dayEnd.ContentPanel != null && dayEnd.ContentPanel.activeSelf;
        }

        public void ToggleOptions()
        {
            if (IsOpen)
            {
                CloseOptions();
            }
            else
            {
                OpenOptions();
            }
        }

        public void OpenOptions()
        {
            if (ContentPanel == null) return;

            SyncSlidersFromAudio();
            ContentPanel.SetActive(true);

            if (_isGameplayScene)
            {
                GameModeManager.SetGameplayPaused(true);
            }
        }

        public void CloseOptions()
        {
            if (ContentPanel == null) return;

            ContentPanel.SetActive(false);

            if (_isGameplayScene)
            {
                // Solo despausar si el modal de fin de día no está activo
                if (!IsDayEndModalActive())
                {
                    GameModeManager.SetGameplayPaused(false);
                }
            }
        }

        private void SyncSlidersFromAudio()
        {
            if (AudioManager.Instance != null)
            {
                float musicVol = AudioManager.Instance.MusicVolume;
                float sfxVol = AudioManager.Instance.SFXVolume;

                if (MusicSlider != null) MusicSlider.value = musicVol;
                if (SFXSlider != null) SFXSlider.value = sfxVol;

                UpdateMusicLabel(musicVol);
                UpdateSFXLabel(sfxVol);
            }
            else
            {
                float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
                float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

                if (MusicSlider != null) MusicSlider.value = musicVol;
                if (SFXSlider != null) SFXSlider.value = sfxVol;

                UpdateMusicLabel(musicVol);
                UpdateSFXLabel(sfxVol);
            }
        }

        private void OnMusicSliderChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(val);
            }
            UpdateMusicLabel(val);
        }

        private void OnSFXSliderChanged(float val)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(val);
            }
            UpdateSFXLabel(val);
        }

        private void UpdateMusicLabel(float val)
        {
            if (MusicValueText != null)
            {
                MusicValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
            }
        }

        private void UpdateSFXLabel(float val)
        {
            if (SFXValueText != null)
            {
                SFXValueText.text = $"{Mathf.RoundToInt(val * 100f)}%";
            }
        }

        public void OnClickResume()
        {
            CloseOptions();
        }

        public void OnClickReturnToMenu()
        {
            CloseOptions();
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithWipe("MainMenuScene", 1.0f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
            }
        }

        public void OnClickBack()
        {
            CloseOptions();
        }

        public void SetIsGameplayScene(bool isGameplay)
        {
            _isGameplayScene = isGameplay;
        }
    }
}
