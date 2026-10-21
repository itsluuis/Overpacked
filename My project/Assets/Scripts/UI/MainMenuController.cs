using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI.Menu
{
    /// <summary>
    /// Controlador principal de la escena MainMenuScene.
    /// Coordina los botones de la derecha, la interacción con el cuaderno
    /// y el modal de opciones.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Hemisferio Izquierdo (Logo y Botones)")]
        public CanvasGroup LeftHemisphereGroup;
        // Compatibilidad hacia atrás
        public CanvasGroup RightHemisphereGroup { get => LeftHemisphereGroup; set => LeftHemisphereGroup = value; }
        public Button PlayButton;
        public Button OptionsButton;
        public Button QuitButton;

        [Header("Cuaderno de Modos")]
        public CuadernoMenuController Cuaderno;

        [Header("Área de Clic Exterior (Mesa)")]
        public Button OutsideDeskClickButton;

        [Header("Modal de Opciones")]
        public GameObject OptionsModal;
        public OptionsMenuUI OptionsUI;
        public Button CloseOptionsButton;

        private Coroutine _fadeCoroutine;

        private void Start()
        {
            Application.runInBackground = true;

            if (PlayButton != null) PlayButton.onClick.AddListener(OnPlayClicked);
            if (OptionsButton != null) OptionsButton.onClick.AddListener(OnOptionsClicked);
            if (QuitButton != null) QuitButton.onClick.AddListener(OnQuitClicked);

            if (OutsideDeskClickButton != null)
            {
                OutsideDeskClickButton.onClick.AddListener(OnOutsideDeskClicked);
            }

            if (CloseOptionsButton != null)
            {
                CloseOptionsButton.onClick.AddListener(CloseOptions);
            }

            if (OptionsModal != null && OptionsUI == null) OptionsModal.SetActive(false);
            if (LeftHemisphereGroup != null)
            {
                LeftHemisphereGroup.alpha = 1f;
                LeftHemisphereGroup.interactable = true;
                LeftHemisphereGroup.blocksRaycasts = true;
            }
        }

        private void OnPlayClicked()
        {
            if (Cuaderno != null)
            {
                Cuaderno.OpenNotebook();
            }
        }

        private void OnOutsideDeskClicked()
        {
            // Si el cuaderno está abierto y no está animando, cerrarlo
            if (Cuaderno != null && Cuaderno.IsOpen && !Cuaderno.IsAnimating)
            {
                Cuaderno.CloseNotebook();
            }
        }

        public void SetLeftHemisphereVisible(bool visible)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeGroup(visible));
        }

        public void SetRightHemisphereVisible(bool visible) => SetLeftHemisphereVisible(visible);

        private IEnumerator FadeGroup(bool visible)
        {
            if (LeftHemisphereGroup == null) yield break;

            float duration = 0.35f;
            float elapsed = 0f;
            float startAlpha = LeftHemisphereGroup.alpha;
            float targetAlpha = visible ? 1f : 0f;

            LeftHemisphereGroup.interactable = visible;
            LeftHemisphereGroup.blocksRaycasts = visible;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                LeftHemisphereGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            LeftHemisphereGroup.alpha = targetAlpha;
        }

        private void OnOptionsClicked()
        {
            if (OptionsUI != null)
            {
                OptionsUI.OpenOptions();
            }
            else if (OptionsModal != null)
            {
                OptionsModal.SetActive(true);
            }
        }

        private void CloseOptions()
        {
            if (OptionsUI != null)
            {
                OptionsUI.CloseOptions();
            }
            else if (OptionsModal != null)
            {
                OptionsModal.SetActive(false);
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenuController] Cerrando aplicación...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
