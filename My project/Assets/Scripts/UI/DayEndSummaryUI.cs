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
            int dayNumber = Environment != null ? Environment.CurrentDayNumber : 1;

            // 1. Título de la Jornada
            if (HeaderTitleText != null)
            {
                HeaderTitleText.text = $"📋 REPORTE DE JORNADA — DÍA {dayNumber}\n<size=14>CIERRE DE TURNO: 22:00 PM</size>";
            }

            // 2. Estrellas
            if (StarsDisplayText != null)
            {
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

            // 3. Título de Calificación
            if (RatingTitleText != null)
            {
                string colorHex = summary.Stars switch
                {
                    3 => "#00FFAA", // Cian / Verde brillante
                    2 => "#55FF55", // Verde claro
                    1 => "#FFCC00", // Amarillo
                    _ => "#FF4444"  // Rojo
                };
                RatingTitleText.text = $"<color={colorHex}><b>{summary.RatingTitle.ToUpper()}</b></color>";
            }

            // 4. Badge de Prestigio
            if (PrestigeBadgeText != null)
            {
                string sign = summary.PrestigeImpact > 0 ? "+" : "";
                string pColor = summary.PrestigeImpact > 0 ? "#00FF66" : (summary.PrestigeImpact < 0 ? "#FF4444" : "#AAAAAA");
                
                string extraText = "";
                if (summary.ExtraBonusPercent > 0)
                {
                    extraText = $" <size=12>(Incluye +{summary.ExtraBonusPercent:0.0}% por {summary.ExtraOrdersCount} pedidos sobre-cuota)</size>";
                }

                PrestigeBadgeText.text = $"Prestigio Empresarial: <color={pColor}><b>{sign}{summary.PrestigeImpact:0.0}%</b></color> (Total: {summary.FinalPrestige:0.0}%){extraText}";
            }

            // 5. Desglose Financiero
            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.text =
                    $"📦 Pedidos Entregados: <b>{summary.CompletedOrders}</b>  <color=#00FFAA>(+${summary.GrossEarnings})</color>\n" +
                    $"⚡ Propinas por Rapidez:  <color=#FFD700>+${summary.TotalTips}</color>\n" +
                    $"⏳ Pedidos Caducados: <b>{summary.ExpiredOrders}</b>  <color=#FF5555>(-${summary.TotalPenalties})</color>\n" +
                    $"────────────────────────────────────────\n" +
                    $"💵 Saldo Neto Final: <size=18><b>${summary.NetMoney}</b></size>";
            }

            // 6. Estado de Cuotas
            if (QuotasStatusText != null && Economy != null)
            {
                string m1 = summary.NetMoney >= Economy.MinimumQuota ? "<color=#00FF66>[✓ CUMPLIDA]</color>" : "<color=#FF5555>[✗ PENDIENTE]</color>";
                string m2 = summary.NetMoney >= Economy.MediumQuota ? "<color=#00FF66>[✓ CUMPLIDA]</color>" : "<color=#888888>[✗]</color>";
                string m3 = summary.NetMoney >= Economy.MaximumQuota ? "<color=#00FF66>[✓ CUMPLIDA]</color>" : "<color=#888888>[✗]</color>";

                QuotasStatusText.text =
                    $"Cuota Mínima (${Economy.MinimumQuota}): {m1} (1 ★)\n" +
                    $"Cuota Media (${Economy.MediumQuota}): {m2} (2 ★)\n" +
                    $"Cuota Máxima (${Economy.MaximumQuota}): {m3} (3 ★)";
            }

            // 7. Texto del Botón
            if (NextDayButtonText != null)
            {
                NextDayButtonText.text = "▶ INICIAR SIGUIENTE JORNADA (06:00 AM)";
            }

            if (NextDayButton != null) NextDayButton.gameObject.SetActive(true);
            if (RetryInfiniteButton != null) RetryInfiniteButton.gameObject.SetActive(false);
            if (ReturnToMenuButton != null) ReturnToMenuButton.gameObject.SetActive(true);
        }

        public void RenderInfiniteSummary(int roundsCleared, System.Collections.Generic.List<int> roundEarnings, int totalEarnings)
        {
            if (HeaderTitleText != null)
            {
                HeaderTitleText.text = "💥 FIN DE LA JORNADA INFINITA\n<size=15>CUOTA NO ALCANZADA A TIEMPO</size>";
            }

            if (StarsDisplayText != null)
            {
                StarsDisplayText.text = $"🏆 RONDAS SUPERADAS: {roundsCleared}";
            }

            if (RatingTitleText != null)
            {
                RatingTitleText.text = roundsCleared >= 5 ? "¡DESEMPEÑO LEGENDARIO!" : (roundsCleared >= 3 ? "¡EXCELENTE RESISTENCIA!" : "TURNO FINALIZADO");
                RatingTitleText.color = new Color(1f, 0.85f, 0.2f);
            }

            if (PrestigeBadgeText != null)
            {
                PrestigeBadgeText.text = $"Modo Arcade Infinito • Rondas: {roundsCleared}";
            }

            if (FinancialBreakdownText != null)
            {
                string breakdown = "<b>Desglose de Ganancias por Ronda:</b>\n";
                if (roundEarnings != null && roundEarnings.Count > 0)
                {
                    for (int i = 0; i < roundEarnings.Count; i++)
                    {
                        breakdown += $"  • Ronda {i + 1}: ${roundEarnings[i]}\n";
                    }
                }
                else
                {
                    breakdown += "  • Ronda 1: $0\n";
                }
                breakdown += $"\n<b>TOTAL OBTENIDO: <color=#00FF66>${totalEarnings}</color></b>";
                FinancialBreakdownText.text = breakdown;
            }

            if (QuotasStatusText != null)
            {
                QuotasStatusText.text = $"Rondas totales completadas: {roundsCleared}\n¡Buen intento! Supera tu récord en la próxima partida.";
            }

            if (NextDayButton != null) NextDayButton.gameObject.SetActive(false);
            if (RetryInfiniteButton != null) RetryInfiniteButton.gameObject.SetActive(true);
            if (ReturnToMenuButton != null) ReturnToMenuButton.gameObject.SetActive(true);
        }

        public void OnClickNextDay()
        {
            // Ocultar modal visualmente
            SetModalVisible(false);

            // Reiniciar economía, pedidos y reloj
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
                Environment.RestartDay(advanceDayCount: true);
            }
        }

        public void OnClickReturnToMenu()
        {
            Debug.Log("[DayEndSummaryUI] Regresando al Menú Principal...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }

        public void OnClickRetryInfinite()
        {
            SetModalVisible(false);
            var infiniteCtrl = FindAnyObjectByType<Gameplay.InfiniteModeController>();
            if (infiniteCtrl != null)
            {
                infiniteCtrl.InitInfiniteMode();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
            }
        }
    }
}
