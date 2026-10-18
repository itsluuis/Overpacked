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

        // Órdenes Especiales de Empresas (Teoría de Conjuntos)
        public bool IsCompanyOrder;
        public string CompanyName;
        public SetTheoryRule SetRule;
        public int TargetItemCount;
        public int CustomPenalty;
        public HashSet<ItemData> CachedValidItems { get; set; } = new HashSet<ItemData>();

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

        // Constructor para Órdenes Especiales de Empresa (Teoría de Conjuntos)
        public Order(string id, string companyName, SetTheoryRule setRule, HashSet<ItemData> validItems, int targetCount, EnvironmentalCondition condition, float duration, int reward, int penalty)
        {
            OrderId = id;
            IsCompanyOrder = true;
            CompanyName = companyName;
            SetRule = setRule;
            CachedValidItems = validItems ?? new HashSet<ItemData>();
            TargetItemCount = targetCount;
            Condition = condition;
            MaxTime = duration;
            RemainingTime = duration;
            MonetaryReward = reward;
            CustomPenalty = penalty;
            ItemsCost = reward;
            PackagingBonus = 0;
            LogicBonus = 0;
        }

        /// <summary>
        /// Comprueba si un conjunto de items empacados satisface exactamente la orden.
        /// </summary>
        public bool MatchesItems(Dictionary<ItemData, int> packedItems)
        {
            if (packedItems == null) return false;

            // 1. Caso Órdenes de Empresa (Teoría de Conjuntos con Cardinalidad Flexible)
            if (IsCompanyOrder)
            {
                if (CachedValidItems == null || CachedValidItems.Count == 0) return false;

                int totalCount = 0;
                foreach (var kvp in packedItems)
                {
                    ItemData item = kvp.Key;
                    int count = kvp.Value;
                    if (count <= 0) continue;

                    // Validación O(1) con el HashSet cacheado
                    if (!CachedValidItems.Contains(item))
                    {
                        return false; // Contiene un ítem fuera del conjunto permitido
                    }

                    totalCount += count;
                }

                return totalCount == TargetItemCount;
            }

            // 2. Caso Estándar: Verificar que cada requerimiento esté cubierto exactamente
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
