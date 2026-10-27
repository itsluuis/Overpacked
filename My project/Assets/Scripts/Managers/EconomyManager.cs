using System;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Resumen consolidado del desempeño financiero y prestigio al final del día.
    /// </summary>
    [Serializable]
    public struct DailySummary
    {
        public int Stars;                // 0, 1, 2, 3
        public string RatingTitle;       // "Día Pésimo", "Día Normal", "Buen Día", "Día Excelente"
        public float PrestigeImpact;     // Variación neta de prestigio (e.g. -5.0%, 0.0%, +3.0%, +7.0% + extra)
        public float ExtraBonusPercent;  // Bonificación por pedidos sobre la cuota máxima
        public int ExtraOrdersCount;     // Cantidad de pedidos extra estimados sobre cuota máx
        public int GrossEarnings;        // Recompensas base sumadas
        public int TotalTips;            // Propinas ganadas
        public int TotalPenalties;       // Penalizaciones restadas
        public int NetMoney;             // Saldo neto final del día
        public int CompletedOrders;      // Cantidad de pedidos entregados
        public int ExpiredOrders;        // Cantidad de pedidos expirados
        public float FinalPrestige;      // Prestigio total de la empresa post-jornada
    }

    /// <summary>
    /// Gestiona el dinero acumulado, propinas, penalizaciones por caducidad,
    /// cuotas oficiales y la calificación por estrellas (0 a 3) al final del día (22:00 PM).
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [Header("Cuotas Oficiales de la Jornada (4 Min / 240s)")]
        public int MinimumQuota = 350;  // 1 Estrella (Día Normal)
        public int MediumQuota = 700;   // 2 Estrellas (Buen Día)
        public int MaximumQuota = 1000; // 3 Estrellas (Día Excelente)

        [Header("Prestigio de la Empresa")]
        public float CurrentPrestige = 100f; // Comienza al 100%

        // Estadísticas auditables de la jornada actual
        public int GrossEarnings { get; private set; }
        public int TotalTips { get; private set; }
        public int TotalPenalties { get; private set; }
        public int CurrentMoney { get; private set; }
        public int CompletedOrdersCount { get; private set; }
        public int ExpiredOrdersCount { get; private set; }

        public event Action<int> OnMoneyChanged;
        public event Action<DailySummary> OnDailySummaryCalculated;

        public void AddEarnings(int baseAmount, int tipAmount = 0)
        {
            GrossEarnings += baseAmount;
            TotalTips += tipAmount;
            int total = baseAmount + tipAmount;
            CurrentMoney += total;
            CompletedOrdersCount++;

            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"<color=green><b>[Economy]</b></color> +${total} (Base: ${baseAmount}, Propina: +${tipAmount}). Saldo actual: ${CurrentMoney}");
        }

        public void DeductPenalty(int amount)
        {
            TotalPenalties += amount;
            CurrentMoney = Mathf.Max(0, CurrentMoney - amount);
            ExpiredOrdersCount++;

            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"<color=red><b>[Economy]</b></color> -${amount} penalización por expiración. Saldo actual: ${CurrentMoney}");
        }

        /// <summary>
        /// Calcula el balance completo del día con estrellas, prestigio y sobre-cuota según el GDD.
        /// </summary>
        public DailySummary CalculateDailySummary()
        {
            DailySummary summary = new DailySummary
            {
                GrossEarnings = GrossEarnings,
                TotalTips = TotalTips,
                TotalPenalties = TotalPenalties,
                NetMoney = CurrentMoney,
                CompletedOrders = CompletedOrdersCount,
                ExpiredOrders = ExpiredOrdersCount
            };

            if (CurrentMoney < MinimumQuota)
            {
                // 0 Estrellas (Día Pésimo): Disminuye el prestigio
                summary.Stars = 0;
                summary.RatingTitle = "Día Pésimo";
                summary.PrestigeImpact = -5.0f;
                summary.ExtraBonusPercent = 0f;
                summary.ExtraOrdersCount = 0;
            }
            else if (CurrentMoney < MediumQuota)
            {
                // 1 Estrella (Día Normal): Prestigio se mantiene estable
                summary.Stars = 1;
                summary.RatingTitle = "Día Normal";
                summary.PrestigeImpact = 0f;
                summary.ExtraBonusPercent = 0f;
                summary.ExtraOrdersCount = 0;
            }
            else if (CurrentMoney < MaximumQuota)
            {
                // 2 Estrellas (Buen Día): Prestigio aumenta moderado
                summary.Stars = 2;
                summary.RatingTitle = "Buen Día";
                summary.PrestigeImpact = 3.0f;
                summary.ExtraBonusPercent = 0f;
                summary.ExtraOrdersCount = 0;
            }
            else
            {
                // 3 Estrellas (Día Excelente): Prestigio aumenta considerablemente + 1.5% por sobre-cuota
                summary.Stars = 3;
                summary.RatingTitle = "Día Excelente";
                
                int excessMoney = CurrentMoney - MaximumQuota;
                // Estimación oficial del GDD: exceso entre valor medio (~$70)
                summary.ExtraOrdersCount = Mathf.Max(0, Mathf.FloorToInt(excessMoney / 70f));
                summary.ExtraBonusPercent = summary.ExtraOrdersCount * 1.5f;
                summary.PrestigeImpact = 7.0f + summary.ExtraBonusPercent;
            }

            CurrentPrestige = Mathf.Max(0f, CurrentPrestige + summary.PrestigeImpact);
            summary.FinalPrestige = CurrentPrestige;

            OnDailySummaryCalculated?.Invoke(summary);
            return summary;
        }

        /// <summary>
        /// Reinicia las métricas financieras para el inicio de una nueva jornada laboral.
        /// </summary>
        public void ResetForNewDay()
        {
            GrossEarnings = 0;
            TotalTips = 0;
            TotalPenalties = 0;
            CurrentMoney = 0;
            CompletedOrdersCount = 0;
            ExpiredOrdersCount = 0;

            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log("[Economy] Métricas financieras reiniciadas para la nueva jornada laboral.");
        }
    }
}
