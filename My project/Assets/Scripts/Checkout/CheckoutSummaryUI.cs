using System.Collections;
using LogiPack.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LogiPack.Checkout
{
    /// <summary>
    /// Modal de auditoría final de Check-out: calificación por estrellas (0 a 3),
    /// desglose por rondas, registro en el Top 5 y botones de navegación.
    /// </summary>
    public class CheckoutSummaryUI : MonoBehaviour
    {
        [Header("Contenedor Principal")]
        public GameObject ModalRoot;
        public CanvasGroup ModalCanvasGroup;

        [Header("Textos de Resultados")]
        public Text TitleText;
        public Text StarsText;
        public Text RankTitleText;
        public Text HitsSummaryText;
        public Text RoundBreakdownText;

        [Header("Botones de Acción")]
        public Button RetryButton;
        public Button MainMenuButton;

        private void Awake()
        {
            if (RetryButton != null) RetryButton.onClick.AddListener(OnRetryClicked);
            if (MainMenuButton != null) MainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void ShowSummary(int totalHits, int totalOrders, int[] roundHits)
        {
            if (ModalRoot != null) ModalRoot.SetActive(true);

            // Calcular estrellas según especificación aprobada:
            // 8-9 aciertos: 3 estrellas
            // 6-7 aciertos: 2 estrellas
            // 4-5 aciertos: 1 estrella
            // < 4 aciertos: 0 estrellas
            int stars = 0;
            string rankTitle = "NECESITA REPASO TEÓRICO";

            if (totalHits >= 8)
            {
                stars = 3;
                rankTitle = "AUDITOR LÓGICO EXPERTO";
            }
            else if (totalHits >= 6)
            {
                stars = 2;
                rankTitle = "AUDITOR APROBADO";
            }
            else if (totalHits >= 4)
            {
                stars = 1;
                rankTitle = "AUDITOR EN ENTRENAMIENTO";
            }

            // Registrar récord en PlayerPrefs
            RankingManager.RecordCheckoutScore(totalHits, totalOrders, stars);

            if (TitleText != null) TitleText.text = "REPORTE DE AUDITORÍA";

            if (StarsText != null)
            {
                string starGlyphs = stars switch
                {
                    3 => "★ ★ ★",
                    2 => "★ ★ ☆",
                    1 => "★ ☆ ☆",
                    _ => "☆ ☆ ☆"
                };
                StarsText.text = $"<color=#E5A823>{starGlyphs}</color>";
            }

            if (RankTitleText != null)
            {
                RankTitleText.text = rankTitle;
            }

            if (HitsSummaryText != null)
            {
                float pct = (totalOrders > 0) ? ((float)totalHits / totalOrders * 100f) : 0f;
                HitsSummaryText.text = $"<b>{totalHits} / {totalOrders}</b> Expedientes Correctos ({pct:F0}% de Precisión)";
            }

            if (RoundBreakdownText != null && roundHits != null && roundHits.Length >= 3)
            {
                RoundBreakdownText.text =
                    $"• Ronda 1:  <b>{roundHits[0]} / 3</b> aciertos\n" +
                    $"• Ronda 2:  <b>{roundHits[1]} / 3</b> aciertos\n" +
                    $"• Ronda 3:  <b>{roundHits[2]} / 3</b> aciertos";
            }

            // Animación suave de entrada
            if (ModalCanvasGroup != null)
            {
                StartCoroutine(FadeInRoutine());
            }
        }

        private IEnumerator FadeInRoutine()
        {
            ModalCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                ModalCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            ModalCanvasGroup.alpha = 1f;
        }

        private void OnRetryClicked()
        {
            if (LogiPack.Managers.SceneTransitionManager.Instance != null)
            {
                LogiPack.Managers.SceneTransitionManager.Instance.LoadSceneWithWipe("CheckoutScene", 1.0f);
            }
            else
            {
                SceneManager.LoadScene("CheckoutScene");
            }
        }

        private void OnMainMenuClicked()
        {
            if (LogiPack.Managers.SceneTransitionManager.Instance != null)
            {
                LogiPack.Managers.SceneTransitionManager.Instance.LoadSceneWithWipe("MainMenuScene", 1.0f);
            }
            else
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }
    }
}
