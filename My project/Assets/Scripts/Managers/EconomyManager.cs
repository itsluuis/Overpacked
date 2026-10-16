using System;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Gestiona el dinero acumulado, penalizaciones por caducidad
    /// y la calificación por estrellas (0 a 3) al final del día (22:00 PM).
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [Header("Cuotas de la Jornada")]
        public int MinimumQuota = 150;  // 1 Estrella
        public int MediumQuota = 300;   // 2 Estrellas
        public int MaximumQuota = 500;  // 3 Estrellas

        public int CurrentMoney { get; private set; }
        public int CompletedOrdersCount { get; private set; }
        public int ExpiredOrdersCount { get; private set; }

        public event Action<int> OnMoneyChanged;

        public void AddEarnings(int amount)
        {
            CurrentMoney += amount;
            CompletedOrdersCount++;
            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"[Economy] +${amount} ganado. Saldo actual: ${CurrentMoney}");
        }

        public void DeductPenalty(int amount)
        {
            CurrentMoney = Mathf.Max(0, CurrentMoney - amount);
            ExpiredOrdersCount++;
            OnMoneyChanged?.Invoke(CurrentMoney);
            Debug.Log($"[Economy] -${amount} penalización por expiración. Saldo actual: ${CurrentMoney}");
        }

        /// <summary>
        /// Calcula la calificación de estrellas al finalizar la jornada (22:00 PM).
        /// </summary>
        public int CalculateDailyRating(out float extraBonusPercent)
        {
            extraBonusPercent = 0f;

            if (CurrentMoney < MinimumQuota)
            {
                return 0; // Día pésimo (0 estrellas)
            }
            else if (CurrentMoney < MediumQuota)
            {
                return 1; // Día normal (1 estrella)
            }
            else if (CurrentMoney < MaximumQuota)
            {
                return 2; // Buen día (2 estrellas)
            }
            else
            {
                // 3 estrellas: bonificación de 1.5% por cada pedido extra por encima de la cuota máxima
                int extraMoney = CurrentMoney - MaximumQuota;
                int estimatedExtraOrders = extraMoney / 50; // Estimación o conteo directo
                extraBonusPercent = estimatedExtraOrders * 1.5f;
                return 3; // Día excelente
            }
        }
    }
}
