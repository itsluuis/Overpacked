using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Managers;
using LogiPack.Orders;
using LogiPack.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Controla la lógica exclusiva del Modo Jornada Infinita:
    /// - Rondas sucesivas con tiempo decreciente (60s -> 30s).
    /// - Cuota progresiva escalable por ronda ($500 base +18%).
    /// - Multiplicador de racha (x1.5).
    /// - Visualización de cuota con marco_dinero_plateado.
    /// - Transición a fin de partida si expira el tiempo.
    /// </summary>
    public class InfiniteModeController : MonoBehaviour
    {
        [Header("Managers")]
        public EconomyManager Economy;
        public PropositionalOrderManager PropositionalOrders;
        public DayEndSummaryUI SummaryUI;

        [Header("Panel HUD de Cuota (marco_dinero_plateado)")]
        public GameObject QuotaPanel;
        public Text QuotaRoundText;
        public Text QuotaAmountText;
        public Text RoundTimerText;
        public Text StreakBadgeText;

        public int CurrentRound { get; private set; } = 1;
        public int TargetQuota { get; private set; } = 500;
        public int CurrentRoundEarnings { get; private set; } = 0;
        public int TotalCareerEarnings { get; private set; } = 0;
        public float RoundTimeRemaining { get; private set; } = 60f;
        public float CurrentRoundMaxDuration { get; private set; } = 60f;
        public int CurrentStreak { get; private set; } = 0;

        public readonly List<int> RoundEarningsHistory = new List<int>();
        private bool _isModeActive = false;

        private void Start()
        {
            if (!GameModeManager.IsInfiniteMode)
            {
                if (QuotaPanel != null) QuotaPanel.SetActive(false);
                enabled = false;
                return;
            }

            // Ocultar reloj tradicional de jornada 06:00 - 22:00
            var clock = GameObject.Find("HUD_Clock");
            if (clock != null) clock.SetActive(false);
            var money = GameObject.Find("HUD_Money");
            if (money != null) money.SetActive(false);

            InitInfiniteMode();
        }

        public void InitInfiniteMode()
        {
            _isModeActive = true;
            CurrentRound = 1;
            TargetQuota = 500;
            CurrentRoundEarnings = 0;
            TotalCareerEarnings = 0;
            CurrentRoundMaxDuration = 60f;
            RoundTimeRemaining = CurrentRoundMaxDuration;
            CurrentStreak = 0;
            RoundEarningsHistory.Clear();

            if (QuotaPanel != null) QuotaPanel.SetActive(true);

            if (PropositionalOrders == null)
            {
                PropositionalOrders = FindAnyObjectByType<PropositionalOrderManager>();
            }

            if (PropositionalOrders != null)
            {
                PropositionalOrders.OnOrderDispatched -= HandlePropositionalOrderDispatched;
                PropositionalOrders.OnOrderDispatched += HandlePropositionalOrderDispatched;
            }

            if (Economy == null)
            {
                Economy = FindAnyObjectByType<EconomyManager>();
            }

            if (SummaryUI == null)
            {
                SummaryUI = FindAnyObjectByType<DayEndSummaryUI>();
            }

            LogiPack.Core.GameModeManager.SetGameplayPaused(false);

            if (PropositionalOrders != null)
            {
                PropositionalOrders.SpawnNextOrder();
            }

            UpdateHUD();
            Debug.Log("<color=yellow><b>[InfiniteMode]</b></color> Modo Jornada Infinita iniciado. Ronda 1. Cuota objetivo: $500.");
        }

        private void OnDestroy()
        {
            if (PropositionalOrders != null)
            {
                PropositionalOrders.OnOrderDispatched -= HandlePropositionalOrderDispatched;
            }
        }

        private void Update()
        {
            if (!_isModeActive || GameModeManager.IsGameplayPaused) return;

            RoundTimeRemaining -= Time.deltaTime;
            if (RoundTimeRemaining <= 0f)
            {
                RoundTimeRemaining = 0f;
                UpdateHUD();
                OnRoundFailed();
                return;
            }

            UpdateHUD();
        }

        private void HandlePropositionalOrderDispatched(PropositionalOrder order, bool success)
        {
            if (!_isModeActive || order == null) return;

            if (success)
            {
                CurrentStreak++;

                // Multiplicador de racha x1.5 a partir de 2 entregas consecutivas sin fallos
                float multiplier = CurrentStreak >= 2 ? 1.5f : 1.0f;
                int totalOrderEarning = Mathf.RoundToInt(order.Reward * multiplier);

                CurrentRoundEarnings += totalOrderEarning;
                TotalCareerEarnings += totalOrderEarning;

                UpdateHUD();

                // Verificar si se alcanzó la cuota de la ronda
                if (CurrentRoundEarnings >= TargetQuota)
                {
                    AdvanceToNextRound();
                }
            }
            else
            {
                NotifyBoxDiscardedOrExpired();
            }
        }

        public void NotifyBoxDiscardedOrExpired()
        {
            if (CurrentStreak > 0)
            {
                CurrentStreak = 0;
                UpdateHUD();
            }
        }

        private void AdvanceToNextRound()
        {
            RoundEarningsHistory.Add(CurrentRoundEarnings);
            CurrentRound++;

            // Tiempo por ronda: decae progresivamente hasta 30s (tras al menos 4-5 rondas)
            // R1: 60s, R2: 52.5s, R3: 45s, R4: 37.5s, R5+: 30s
            CurrentRoundMaxDuration = Mathf.Max(30f, 60f - (CurrentRound - 1) * 7.5f);
            RoundTimeRemaining = CurrentRoundMaxDuration;

            // Cuota aumenta ~18% por ronda sin límite
            TargetQuota = Mathf.RoundToInt(TargetQuota * 1.18f);
            CurrentRoundEarnings = 0;

            if (Economy != null)
            {
                Economy.ResetForNewDay();
            }

            UpdateHUD();
            Debug.Log($"<color=green><b>[InfiniteMode]</b></color> ¡CUOTA ALCANZADA! Avanzando a Ronda {CurrentRound}. Nueva cuota: ${TargetQuota}. Tiempo: {CurrentRoundMaxDuration:F0}s.");
        }

        private void OnRoundFailed()
        {
            _isModeActive = false;
            RoundEarningsHistory.Add(CurrentRoundEarnings);
            Debug.Log($"<color=red><b>[InfiniteMode]</b></color> Tiempo agotado. Rondas superadas: {CurrentRound - 1}. Recaudación total: ${TotalCareerEarnings}.");

            if (SummaryUI == null)
            {
                SummaryUI = FindAnyObjectByType<DayEndSummaryUI>();
            }

            if (SummaryUI != null)
            {
                SummaryUI.RenderInfiniteSummary(CurrentRound, RoundEarningsHistory, TotalCareerEarnings);
                SummaryUI.SetModalVisible(true);
            }
        }

        private void UpdateHUD()
        {
            if (QuotaRoundText != null)
            {
                QuotaRoundText.text = $"RONDA {CurrentRound}";
            }

            if (QuotaAmountText != null)
            {
                QuotaAmountText.text = $"${CurrentRoundEarnings} / ${TargetQuota}";
                QuotaAmountText.color = CurrentRoundEarnings >= TargetQuota ? new Color(0.2f, 0.9f, 0.3f) : Color.white;
            }

            if (RoundTimerText != null)
            {
                int secs = Mathf.CeilToInt(RoundTimeRemaining);
                RoundTimerText.text = $"{secs:D2}s";
                RoundTimerText.color = secs <= 10 ? new Color(1f, 0.25f, 0.25f) : (secs <= 20 ? Color.yellow : Color.white);
            }

            if (StreakBadgeText != null)
            {
                if (CurrentStreak >= 2)
                {
                    StreakBadgeText.text = $"🔥 RACHA x1.5 ({CurrentStreak})";
                    StreakBadgeText.gameObject.SetActive(true);
                }
                else
                {
                    StreakBadgeText.gameObject.SetActive(false);
                }
            }
        }
    }
}
