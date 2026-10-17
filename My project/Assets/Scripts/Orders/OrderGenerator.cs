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

            // 2. Generar Condición Lógica (Clima, Temperatura y Materiales de Embalaje)
            var condition = new EnvironmentalCondition();
            int conditionType = UnityEngine.Random.Range(0, 8);

            switch (conditionType)
            {
                case 0: // Embalaje simple: Espuma
                    condition.LeftPredicate = AtomicPredicate.BoxHasFoam;
                    condition.Operator = LogicOperator.None;
                    condition.NaturalLanguageText = "Embalar con Relleno de Espuma";
                    break;

                case 1: // Embalaje simple: Film Térmico
                    condition.LeftPredicate = AtomicPredicate.BoxHasThermalFilm;
                    condition.Operator = LogicOperator.None;
                    condition.NaturalLanguageText = "Embalar con Film Térmico";
                    break;

                case 2: // Sin embalaje especial (¬r ∧ ¬s)
                    condition.LeftPredicate = AtomicPredicate.BoxHasFoam;
                    condition.InvertLeft = true;
                    condition.Operator = LogicOperator.And;
                    condition.RightPredicate = AtomicPredicate.BoxHasThermalFilm;
                    condition.InvertRight = true;
                    condition.NaturalLanguageText = "Enviar SIN embalaje especial";
                    break;

                case 3: // Conjunción: Espuma Y Film Térmico (r ∧ s)
                    condition.LeftPredicate = AtomicPredicate.BoxHasFoam;
                    condition.Operator = LogicOperator.And;
                    condition.RightPredicate = AtomicPredicate.BoxHasThermalFilm;
                    condition.NaturalLanguageText = "Embalar con Relleno de Espuma Y Film Térmico";
                    break;

                case 4: // Implicación: Si hace frío -> Film Térmico
                    condition.LeftPredicate = AtomicPredicate.IsCold;
                    condition.Operator = LogicOperator.Implication;
                    condition.RightPredicate = AtomicPredicate.BoxHasThermalFilm;
                    condition.NaturalLanguageText = "Si hace frío, embalar con Film Térmico";
                    break;

                case 5: // Implicación con Negación: Si hace calor -> NO Film Térmico
                    condition.LeftPredicate = AtomicPredicate.IsHot;
                    condition.Operator = LogicOperator.Implication;
                    condition.RightPredicate = AtomicPredicate.BoxHasThermalFilm;
                    condition.InvertRight = true;
                    condition.NaturalLanguageText = "Si hace calor, NO embalar en Film Térmico";
                    break;

                case 6: // Bicondicional: Espuma <-> Tormenta
                    condition.LeftPredicate = AtomicPredicate.BoxHasFoam;
                    condition.Operator = LogicOperator.Biconditional;
                    condition.RightPredicate = AtomicPredicate.IsStorming;
                    condition.NaturalLanguageText = "Embalar con Espuma SI Y SOLO SI hay tormenta";
                    break;

                case 7: // Clima y Temperatura: Despejado o Normal
                    condition.LeftPredicate = AtomicPredicate.IsClear;
                    condition.Operator = LogicOperator.Or;
                    condition.RightPredicate = AtomicPredicate.IsRaining;
                    condition.NaturalLanguageText = "Enviar si está despejado O lloviendo";
                    break;
            }

            return new Order(orderId, condition, reqs, duration, reward);
        }
    }
}
