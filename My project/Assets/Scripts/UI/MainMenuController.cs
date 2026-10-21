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
        [Header("Hemisferio Derecho (Logo y Botones)")]
        public CanvasGroup RightHemisphereGroup;
        public Button PlayButton;
        public Button OptionsButton;
        public Button QuitButton;

        [Header("Cuaderno de Modos")]
        public CuadernoMenuController Cuaderno;

        [Header("Área de Clic Exterior (Mesa)")]
        public Button OutsideDeskClickButton;

        [Header("Modal de Opciones Provisional")]
        public GameObject OptionsModal;
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

            if (OptionsModal != null) OptionsModal.SetActive(false);
            if (RightHemisphereGroup != null)
            {
                RightHemisphereGroup.alpha = 1f;
                RightHemisphereGroup.interactable = true;
                RightHemisphereGroup.blocksRaycasts = true;
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

        public void SetRightHemisphereVisible(bool visible)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeGroup(visible));
        }

        private IEnumerator FadeGroup(bool visible)
        {
            if (RightHemisphereGroup == null) yield break;

            float duration = 0.35f;
            float elapsed = 0f;
            float startAlpha = RightHemisphereGroup.alpha;
            float targetAlpha = visible ? 1f : 0f;

            RightHemisphereGroup.interactable = visible;
            RightHemisphereGroup.blocksRaycasts = visible;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                RightHemisphereGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            RightHemisphereGroup.alpha = targetAlpha;
        }

        private void OnOptionsClicked()
        {
            if (OptionsModal != null)
            {
                OptionsModal.SetActive(true);
            }
        }

        private void CloseOptions()
        {
            if (OptionsModal != null)
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
