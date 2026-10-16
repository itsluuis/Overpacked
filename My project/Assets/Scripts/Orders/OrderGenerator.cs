using System;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Orders
{
    /// <summary>
    /// Genera pedidos procedurales asegurando consistencia lógica justa
    /// (nunca pedir condiciones que ya pasaron de hora).
    /// </summary>
    public static class OrderGenerator
    {
        private static int _orderSequence = 1;

        public static Order GenerateRandomOrder(EnvironmentState currentState, List<ItemData> availableItems)
        {
            string orderId = $"ORD-#{_orderSequence++:D3}";
            int reward = UnityEngine.Random.Range(30, 75);
            float duration = UnityEngine.Random.Range(35f, 60f);

            // 1. Generar Requerimientos de items (entre 1 y 3 tipos de item)
            var reqs = new List<OrderItemRequirement>();
            if (availableItems != null && availableItems.Count > 0)
            {
                int itemsToPick = UnityEngine.Random.Range(1, Mathf.Min(3, availableItems.Count + 1));
                var shuffled = new List<ItemData>(availableItems);
                
                // Shuffle simple
                for (int i = 0; i < shuffled.Count; i++)
                {
                    int rnd = UnityEngine.Random.Range(0, shuffled.Count);
                    (shuffled[i], shuffled[rnd]) = (shuffled[rnd], shuffled[i]);
                }

                for (int i = 0; i < itemsToPick; i++)
                {
                    reqs.Add(new OrderItemRequirement
                    {
                        Item = shuffled[i],
                        RequiredCount = UnityEngine.Random.Range(1, 3)
                    });
                }
            }

            // 2. Generar Condición Ambiental Justa
            var condition = new EnvironmentalCondition();
            int conditionType = UnityEngine.Random.Range(0, 4);

            switch (conditionType)
            {
                case 0: // Clima simple
                    condition.LeftPredicate = AtomicPredicate.IsRaining;
                    condition.NaturalLanguageText = "Enviar mientras esté lloviendo";
                    break;

                case 1: // Rango horario futuro o actual válido
                    if (currentState.CurrentHour < 12.0f)
                    {
                        condition.LeftPredicate = AtomicPredicate.IsMorning;
                        condition.NaturalLanguageText = "Enviar durante la mañana";
                    }
                    else if (currentState.CurrentHour < 18.0f)
                    {
                        condition.LeftPredicate = AtomicPredicate.IsAfternoon;
                        condition.NaturalLanguageText = "Enviar durante la tarde";
                    }
                    else
                    {
                        condition.LeftPredicate = AtomicPredicate.IsNight;
                        condition.NaturalLanguageText = "Enviar durante la noche";
                    }
                    break;

                case 2: // Conjunción (Lluvia ∧ Frío)
                    condition.LeftPredicate = AtomicPredicate.IsRaining;
                    condition.Operator = LogicOperator.And;
                    condition.RightPredicate = AtomicPredicate.IsCold;
                    condition.NaturalLanguageText = "Enviar si llueve Y hace frío";
                    break;

                case 3: // Disyunción / Bicondicional
                    condition.LeftPredicate = AtomicPredicate.IsClear;
                    condition.Operator = LogicOperator.Biconditional;
                    condition.RightPredicate = AtomicPredicate.IsHot;
                    condition.NaturalLanguageText = "Enviar si está despejado SI Y SOLO SI hace calor";
                    break;
            }

            return new Order(orderId, condition, reqs, duration, reward);
        }
    }
}
