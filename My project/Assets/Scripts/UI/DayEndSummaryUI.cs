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

        [Header("Botón de Acción")]
        public Button NextDayButton;
        public Text NextDayButtonText;

        private void Awake()
        {
            if (NextDayButton != null)
            {
                NextDayButton.onClick.AddListener(OnClickNextDay);
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
    }
}
