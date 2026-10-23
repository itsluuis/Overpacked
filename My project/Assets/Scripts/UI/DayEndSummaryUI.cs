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
            int dayNumber = Environment != null ? Environment.CurrentDayNumber : 1;

            // 1. Título de la Jornada (+10px)
            if (HeaderTitleText != null)
            {
                HeaderTitleText.fontSize = 32;
                HeaderTitleText.text = $"📋 REPORTE DE JORNADA — DÍA {dayNumber}\n<size=18>CIERRE DE TURNO: 22:00 PM</size>";
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

            // 5. Desglose Financiero (+10px)
            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.fontSize = 26;
                FinancialBreakdownText.text =
                    $"📦 Pedidos Entregados: <b>{summary.CompletedOrders}</b>  <color=#00AA44>(+${summary.GrossEarnings})</color>\n" +
                    $"⚡ Propinas por Rapidez:  <color=#C89000>+${summary.TotalTips}</color>\n" +
                    $"⏳ Pedidos Caducados: <b>{summary.ExpiredOrders}</b>  <color=#CC2222>(-${summary.TotalPenalties})</color>\n" +
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
            if (HeaderTitleText != null)
            {
                HeaderTitleText.fontSize = 32;
                HeaderTitleText.text = "💥 FIN DE LA JORNADA INFINITA\n<size=18>CUOTA NO ALCANZADA A TIEMPO</size>";
            }

            if (StarsDisplayText != null)
            {
                StarsDisplayText.fontSize = 52;
                StarsDisplayText.text = $"🏆 RONDAS SUPERADAS: {roundsCleared}";
            }

            // Textos después de 'RONDAS SUPERADAS': aumentados +14px para máxima legibilidad
            if (RatingTitleText != null)
            {
                RatingTitleText.fontSize = 36; // +14px (era 22)
                RatingTitleText.text = roundsCleared >= 5 ? "¡DESEMPEÑO LEGENDARIO!" : (roundsCleared >= 3 ? "¡EXCELENTE RESISTENCIA!" : "TURNO FINALIZADO");
                RatingTitleText.color = new Color(0.85f, 0.55f, 0.05f);
            }

            if (PrestigeBadgeText != null)
            {
                PrestigeBadgeText.fontSize = 30; // +13px (era 17)
                PrestigeBadgeText.text = $"Modo Arcade Infinito • Rondas: {roundsCleared}";
            }

            if (FinancialBreakdownText != null)
            {
                FinancialBreakdownText.fontSize = 28; // +11px con buen interlineado
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
                breakdown += $"\n<b>TOTAL OBTENIDO: <color=#008833>${totalEarnings}</color></b>";
                FinancialBreakdownText.text = breakdown;
            }

            if (QuotasStatusText != null)
            {
                QuotasStatusText.fontSize = 28; // +12px (era 16)
                QuotasStatusText.text = $"Rondas totales completadas: {roundsCleared}\n¡Buen intento! Supera tu récord en la próxima partida.";
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

        public void OnClickNextDay()
        {
            LogiPack.Core.GameModeManager.SetGameplayPaused(false);
            SetModalVisible(false);

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
