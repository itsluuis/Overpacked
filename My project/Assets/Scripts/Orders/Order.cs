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
    /// </summary>
    public class Order
    {
        public string OrderId;
        public EnvironmentalCondition Condition;
        public List<OrderItemRequirement> Requirements = new List<OrderItemRequirement>();
        
        public float MaxTime;
        public float RemainingTime;
        public int MonetaryReward;

        public bool IsExpired => RemainingTime <= 0;
        public float TimeNormalized => Mathf.Clamp01(RemainingTime / MaxTime);

        public Order(string id, EnvironmentalCondition condition, List<OrderItemRequirement> reqs, float duration, int reward)
        {
            OrderId = id;
            Condition = condition;
            Requirements = reqs;
            MaxTime = duration;
            RemainingTime = duration;
            MonetaryReward = reward;
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
