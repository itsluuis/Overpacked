using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Modal de fin de jornada (22:00 PM) con estilo Overcooked / Papers, Please.
    /// Presenta el balance contable, estrellas obtenidas (0 a 3),
    /// impacto en el prestigio de la empresa y botón para avanzar al siguiente turno.
    /// </summary>
    public class DayEndSummaryUI : MonoBehaviour
    {
        [Header("Referencias a Managers")]
        public EnvironmentManager Environment;
        public EconomyManager Economy;
        public OrderManager Orders;

        [Header("Contenedor Principal")]
        public GameObject ContentPanel;
        public Image OverlayBackground;

        [Header("Elementos de Texto")]
        public Text HeaderTitleText;
        public Text StarsDisplayText;
        public Text RatingTitleText;
        public Text PrestigeBadgeText;
        public Text FinancialBreakdownText;
        public Text QuotasStatusText;

        [Header("Botones de Acción")]
        public Button NextDayButton;
        public Text NextDayButtonText;
        public Button ReturnToMenuButton;
        public Button RetryInfiniteButton;

        private void Awake()
        {
            if (NextDayButton != null)
            {
                NextDayButton.onClick.AddListener(OnClickNextDay);
            }
            if (ReturnToMenuButton != null)
            {
                ReturnToMenuButton.onClick.AddListener(OnClickReturnToMenu);
            }
            if (RetryInfiniteButton != null)
            {
                RetryInfiniteButton.onClick.AddListener(OnClickRetryInfinite);
            }
        }

        private void Start()
        {
            if (Environment != null)
            {
                Environment.OnDayCompleted += HandleDayCompleted;
            }

            // Ocultar modal visualmente al inicio manteniendo el GameObject activo para recibir eventos
            SetModalVisible(false);
        }

        private void OnDestroy()
        {
            if (Environment != null)
            {
                Environment.OnDayCompleted -= HandleDayCompleted;
            }
        }

        public void HandleDayCompleted()
        {
            if (Economy == null) return;

            DailySummary summary = Economy.CalculateDailySummary();
            RenderSummary(summary);
            SetModalVisible(true);
        }

        public void SetModalVisible(bool visible)
        {
            LogiPack.Core.GameModeManager.SetGameplayPaused(visible);

            if (visible)
            {
                transform.SetAsLastSibling();
            }

            if (OverlayBackground != null)
            {
                OverlayBackground.enabled = visible;
            }

            if (ContentPanel != null)
            {
                ContentPanel.SetActive(visible);
            }
        }

        private void RenderSummary(DailySummary summary)
        {
            // 0. Registrar récord en el ranking Top 5 de Jornada Normal
            LogiPack.Core.RankingManager.RecordNormalDayScore(summary.NetMoney, summary.Stars);

            // 1. Título de la Jornada (+10px)
            if (HeaderTitleText != null)
            {
                int dayNum = (Environment != null) ? Environment.CurrentDayNumber : 1;
                HeaderTitleText.fontSize = 32;
                HeaderTitleText.text = $"📋 REPORTE DE JORNADA — DÍA {dayNum}\n<size=18>CIERRE DE TURNO: 22:00 PM</size>";
            }

            // 2. Estrellas (+10px)
            if (StarsDisplayText != null)
            {
                StarsDisplayText.fontSize = 58;
                StarsDisplayText.verticalOverflow = VerticalWrapMode.Overflow;
                string starsStr = "";
                for (int i = 0; i < 3; i++)
                {
                    if (i < summary.Stars)
                        starsStr += "<color=#FFD700>★</color> ";
                    else
                        starsStr += "<color=#555555>☆</color> ";
                }
                StarsDisplayText.text = starsStr.Trim();
            }

            // 3. Título de Calificación (+10px)
            if (RatingTitleText != null)
            {
                RatingTitleText.fontSize = 32;
                string colorHex = summary.Stars switch
                {
                    3 => "#00FFAA",
                    2 => "#55FF55",
                    1 => "#FFCC00",
                    _ => "#FF4444"
                };
                RatingTitleText.text = $"<color={colorHex}><b>{summary.RatingTitle.ToUpper()}</b></color>";
            }

            // 4. Badge de Prestigio (+10px)
            if (PrestigeBadgeText != null)
            {
                PrestigeBadgeText.gameObject.SetActive(true);
                PrestigeBadgeText.fontSize = 26;
                string sign = summary.PrestigeImpact > 0 ? "+" : "";
                string pColor = summary.PrestigeImpact > 0 ? "#00FF66" : (summary.PrestigeImpact < 0 ? "#FF4444" : "#AAAAAA");
                
                string extraText = "";
                if (summary.ExtraBonusPercent > 0)
                {
                    extraText = $" <size=16>(Incluye +{summary.ExtraBonusPercent:0.0}% por {summary.ExtraOrdersCount} pedidos sobre-cuota)</size>";
                }

                PrestigeBadgeText.text = $"Prestigio Empresarial: <color={pColor}><b>{sign}{summary.PrestigeImpact:0.0}%</b></color> (Total: {summary.FinalPrestige:0.0}%){extraText}";
            }

            // 5. Desglose Financiero (+10px) - Simplificado sin propinas ni caducados
            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.fontSize = 26;
                FinancialBreakdownText.text =
                    $"📦 Pedidos Entregados: <b>{summary.CompletedOrders}</b>  <color=#00AA44>(+${summary.GrossEarnings})</color>\n" +
                    $"────────────────────────────────────────\n" +
                    $"💵 Saldo Neto Final: <size=28><b>${summary.NetMoney}</b></size>";
            }

            // 6. Estado de Cuotas (+10px)
            if (QuotasStatusText != null && Economy != null)
            {
                QuotasStatusText.fontSize = 25;
                string m1 = summary.NetMoney >= Economy.MinimumQuota ? "<color=#00AA44>[✓ CUMPLIDA]</color>" : "<color=#CC2222>[✗ PENDIENTE]</color>";
                string m2 = summary.NetMoney >= Economy.MediumQuota ? "<color=#00AA44>[✓ CUMPLIDA]</color>" : "<color=#888888>[✗]</color>";
                string m3 = summary.NetMoney >= Economy.MaximumQuota ? "<color=#00AA44>[✓ CUMPLIDA]</color>" : "<color=#888888>[✗]</color>";

                QuotasStatusText.text =
                    $"Cuota Mínima (${Economy.MinimumQuota}): {m1} (1 ★)\n" +
                    $"Cuota Media (${Economy.MediumQuota}): {m2} (2 ★)\n" +
                    $"Cuota Máxima (${Economy.MaximumQuota}): {m3} (3 ★)";
            }

            // 7. Texto del Botón (VOLVER A INTENTAR en negro)
            if (NextDayButtonText != null)
            {
                NextDayButtonText.text = "VOLVER A INTENTAR";
                NextDayButtonText.color = new Color(0.1f, 0.1f, 0.1f);
                NextDayButtonText.fontSize = 20;
            }

            EnforceButtonTextColors();

            if (NextDayButton != null) NextDayButton.gameObject.SetActive(true);
            if (RetryInfiniteButton != null) RetryInfiniteButton.gameObject.SetActive(false);
            if (ReturnToMenuButton != null) ReturnToMenuButton.gameObject.SetActive(true);
        }

        public void RenderInfiniteSummary(int roundsCleared, System.Collections.Generic.List<int> roundEarnings, int totalEarnings)
        {
            // 0. Registrar récord en el sistema de rankings Top 5
            LogiPack.Core.RankingManager.RecordInfiniteScore(roundsCleared, totalEarnings);

            if (HeaderTitleText != null)
            {
                HeaderTitleText.fontSize = 32;
                HeaderTitleText.text = "💥 FIN DE LA JORNADA INFINITA\n<size=18>CUOTA NO ALCANZADA A TIEMPO</size>";
            }

            if (StarsDisplayText != null)
            {
                StarsDisplayText.text = "";
            }

            // Textos después de 'RONDAS SUPERADAS': aumentados para máxima legibilidad
            if (RatingTitleText != null)
            {
                RatingTitleText.fontSize = 34;
                RatingTitleText.text = "¡BUEN TRABAJO!";
                RatingTitleText.color = new Color(0.85f, 0.55f, 0.05f);
            }

            if (PrestigeBadgeText != null)
            {
                PrestigeBadgeText.fontSize = 24;
                PrestigeBadgeText.text = $"Ronda máxima: {roundsCleared}";
            }

            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.fontSize = 19;
                FinancialBreakdownText.verticalOverflow = VerticalWrapMode.Overflow;
                string breakdown = "<b>Desglose por Rondas (Recaudado / Meta de Ronda):</b>\n";
                int quota = 500;
                if (roundEarnings != null && roundEarnings.Count > 0)
                {
                    for (int i = 0; i < roundEarnings.Count; i++)
                    {
                        string status = (i < roundEarnings.Count - 1) ? "<color=#00AA44>[✓ CUMPLIDA]</color>" : "<color=#CC2222>[✗ NO ALCANZADA]</color>";
                        breakdown += $"  • Ronda {i + 1} (Meta: ${quota}): <b>${roundEarnings[i]}</b>  {status}\n";
                        quota = Mathf.RoundToInt(quota * 1.18f);
                    }
                }
                else
                {
                    breakdown += "  • Ronda 1 (Meta: $500): $0  <color=#CC2222>[✗ NO ALCANZADA]</color>\n";
                }
                breakdown += $"<b>TOTAL RECAUDADO ACUMULADO: <color=#008833>${totalEarnings}</color></b>";
                FinancialBreakdownText.text = breakdown;
            }

            if (QuotasStatusText != null)
            {
                QuotasStatusText.fontSize = 22;
                QuotasStatusText.text = "¡Supera tu récord en la próxima partida!";
            }

            EnforceButtonTextColors();

            if (NextDayButton != null) NextDayButton.gameObject.SetActive(false);
            if (RetryInfiniteButton != null) RetryInfiniteButton.gameObject.SetActive(true);
            if (ReturnToMenuButton != null) ReturnToMenuButton.gameObject.SetActive(true);
        }

        private void EnforceButtonTextColors()
        {
            Color blackInk = new Color(0.1f, 0.1f, 0.1f);

            if (NextDayButton != null)
            {
                var txt = NextDayButton.GetComponentInChildren<Text>();
                if (txt != null) { txt.color = blackInk; txt.fontSize = 20; }
            }
            if (RetryInfiniteButton != null)
            {
                var txt = RetryInfiniteButton.GetComponentInChildren<Text>();
                if (txt != null) { txt.color = blackInk; txt.fontSize = 20; }
            }
            if (ReturnToMenuButton != null)
            {
                var txt = ReturnToMenuButton.GetComponentInChildren<Text>();
                if (txt != null) { txt.color = blackInk; txt.fontSize = 20; }
            }
        }

        public void RenderTutorialSummary(int completedOrders, int earnings)
        {
            // Registrar tutorial completado de forma persistente
            LogiPack.Core.RankingManager.SetTutorialCompleted(true);

            if (HeaderTitleText != null)
            {
                HeaderTitleText.fontSize = 32;
                HeaderTitleText.text = "🎓 PRUEBA LABORAL COMPLETADA\n<size=18>SUPERVISIÓN: MOCHI MILES</size>";
            }

            if (StarsDisplayText != null)
            {
                StarsDisplayText.fontSize = 58;
                StarsDisplayText.text = "<color=#FFD700>★ ★ ★</color>";
            }

            if (RatingTitleText != null)
            {
                RatingTitleText.fontSize = 34;
                RatingTitleText.text = "<color=#00FFAA><b>CAPACITACIÓN COMPLETADA</b></color>";
            }

            if (PrestigeBadgeText != null)
            {
                PrestigeBadgeText.gameObject.SetActive(false);
            }

            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.fontSize = 26;
                FinancialBreakdownText.text =
                    $"📦 Pedidos de Prueba: <b>{completedOrders}/5</b>  <color=#00AA44>(+${earnings})</color>\n" +
                    $"⚡ Precisión Lógica: <color=#00FFAA>100%</color>\n" +
                    $"────────────────────────────────────────\n" +
                    $"💵 Saldo Inicial Ganado: <size=28><b>${earnings}</b></size>";
            }

            if (QuotasStatusText != null)
            {
                QuotasStatusText.fontSize = 25;
                QuotasStatusText.text =
                    "Proposiciones Atómicas: <color=#00AA44>[✓ DOMINADAS]</color>\n" +
                    "Conectivos y Equivalencias: <color=#00AA44>[✓ DOMINADOS]</color>\n" +
                    "Anidamiento de Sub-Cajas: <color=#00AA44>[✓ APROBADO]</color>";
            }

            if (NextDayButtonText != null)
            {
                NextDayButtonText.text = "REPETIR TUTORIAL";
                NextDayButtonText.color = new Color(0.1f, 0.1f, 0.1f);
                NextDayButtonText.fontSize = 20;
            }

            EnforceButtonTextColors();

            if (NextDayButton != null) NextDayButton.gameObject.SetActive(true);
            if (RetryInfiniteButton != null) RetryInfiniteButton.gameObject.SetActive(false);
            if (ReturnToMenuButton != null) ReturnToMenuButton.gameObject.SetActive(true);

            SetModalVisible(true);
        }

        public void OnClickNextDay()
        {
            LogiPack.Core.GameModeManager.SetGameplayPaused(false);
            SetModalVisible(false);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TutorialScene")
            {
                if (LogiPack.Managers.SceneTransitionManager.Instance != null)
                {
                    LogiPack.Managers.SceneTransitionManager.Instance.LoadSceneWithWipe("TutorialScene", 1.0f);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene");
                }
                return;
            }

            if (Economy != null)
            {
                Economy.ResetForNewDay();
            }

            if (Orders != null)
            {
                Orders.ClearAllOrders();
            }

            if (Environment != null)
            {
                Environment.RestartDay(false);
            }
            else if (EnvironmentManager.Instance != null)
            {
                EnvironmentManager.Instance.RestartDay(false);
            }

            // Reiniciar orden actual para que no quede congelada la del día anterior
            var propOrders = FindAnyObjectByType<PropositionalOrderManager>();
            if (propOrders != null)
            {
                propOrders.SpawnNextOrder();
            }

            // Limpiar mesa de armado
            var workbench = FindAnyObjectByType<Gameplay.AssemblyWorkbenchManager>();
            if (workbench != null)
            {
                workbench.ClearWorkbench();
            }
        }

        public void OnClickReturnToMenu()
        {
            LogiPack.Core.GameModeManager.SetGameplayPaused(false);
            Debug.Log("[DayEndSummaryUI] Regresando al Menú Principal...");
            if (LogiPack.Managers.SceneTransitionManager.Instance != null)
            {
                LogiPack.Managers.SceneTransitionManager.Instance.LoadSceneWithWipe("MainMenuScene", 1.0f);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
            }
        }

        public void OnClickRetryInfinite()
        {
            LogiPack.Core.GameModeManager.SetGameplayPaused(false);
            SetModalVisible(false);

            if (Economy != null)
            {
                Economy.ResetForNewDay();
            }

            var infiniteCtrl = FindAnyObjectByType<Gameplay.InfiniteModeController>();
            if (infiniteCtrl != null)
            {
                infiniteCtrl.InitInfiniteMode();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
            }

            // Reiniciar orden para la nueva partida infinita
            var propOrders = FindAnyObjectByType<PropositionalOrderManager>();
            if (propOrders != null)
            {
                propOrders.SpawnNextOrder();
            }

            // Limpiar mesa de armado
            var workbench = FindAnyObjectByType<Gameplay.AssemblyWorkbenchManager>();
            if (workbench != null)
            {
                workbench.ClearWorkbench();
            }
        }
    }
}
