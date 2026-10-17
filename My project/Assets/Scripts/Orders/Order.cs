using System;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Orders
{
    [Serializable]
    public class OrderItemRequirement
    {
        public ItemData Item;
        public int RequiredCount;
    }

    /// <summary>
    /// Modelo de datos de un Pedido activo en el almacén.
    /// Incluye desglose económico (Ítems + Embalaje + Lógica) y propinas por agilidad.
    /// </summary>
    [Serializable]
    public class Order
    {
        public string OrderId;
        public EnvironmentalCondition Condition;
        public List<OrderItemRequirement> Requirements = new List<OrderItemRequirement>();
        
        public float MaxTime;
        public float RemainingTime;

        // Desglose económico modular
        public int ItemsCost;
        public int PackagingBonus;
        public int LogicBonus;
        public int MonetaryReward;

        public bool IsExpired => RemainingTime <= 0;
        public float TimeNormalized => Mathf.Clamp01(RemainingTime / MaxTime);

        /// <summary>
        /// Porcentaje de propina según la zona del temporizador:
        /// Verde (>= 60% tiempo): +25%
        /// Amarillo (25% a 60% tiempo): +10%
        /// Rojo (< 25% tiempo): +0% (solo valor base)
        /// </summary>
        public float GetTipPercent()
        {
            float ratio = TimeNormalized;
            if (ratio >= 0.60f) return 0.25f; // Zona Verde: +25%
            if (ratio >= 0.25f) return 0.10f; // Zona Amarilla: +10%
            return 0f;                       // Zona Roja: +0%
        }

        public int GetTipAmount()
        {
            return Mathf.RoundToInt(MonetaryReward * GetTipPercent());
        }

        public int GetTotalPayment()
        {
            return MonetaryReward + GetTipAmount();
        }

        public Order(string id, EnvironmentalCondition condition, List<OrderItemRequirement> reqs, float duration, int itemsCost, int packagingBonus, int logicBonus)
        {
            OrderId = id;
            Condition = condition;
            Requirements = reqs;
            MaxTime = duration;
            RemainingTime = duration;
            ItemsCost = itemsCost;
            PackagingBonus = packagingBonus;
            LogicBonus = logicBonus;
            MonetaryReward = itemsCost + packagingBonus + logicBonus;
        }

        // Sobrecarga de compatibilidad
        public Order(string id, EnvironmentalCondition condition, List<OrderItemRequirement> reqs, float duration, int reward)
            : this(id, condition, reqs, duration, reward, 0, 0)
        {
        }

        /// <summary>
        /// Comprueba si un conjunto de items empacados satisface exactamente la orden.
        /// </summary>
        public bool MatchesItems(Dictionary<ItemData, int> packedItems)
        {
            if (packedItems == null) return false;

            // Verificar que cada requerimiento esté cubierto exactamente
            foreach (var req in Requirements)
            {
                if (!packedItems.TryGetValue(req.Item, out int count) || count != req.RequiredCount)
                {
                    return false;
                }
            }

            // Verificar que no haya items extra
            foreach (var kvp in packedItems)
            {
                bool existsInReq = Requirements.Exists(r => r.Item == kvp.Key && r.RequiredCount == kvp.Value);
                if (!existsInReq) return false;
            }

            return true;
        }
    }
}
