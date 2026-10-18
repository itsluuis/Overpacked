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

        private static readonly string[] CompanyNames = new string[]
        {
            "Ferretería Pim Pom Pim",
            "Chispitas & Voltios Bip-Bop",
            "Telas Pom Pón",
            "Cristales Glup-Glup",
            "Taller Tuerca Loca",
            "Industrias Patapúm",
            "Laboratorio Burbujitas"
        };

        public static Order GenerateRandomOrder(EnvironmentState currentState, List<ItemData> availableItems)
        {
            // 15% de probabilidad: Orden Especial de Empresa (Teoría de Conjuntos)
            if (UnityEngine.Random.value < 0.15f && availableItems != null && availableItems.Count > 0)
            {
                return GenerateCompanyOrder(availableItems);
            }

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

            // 3. Cálculo Modular de Recompensa (GDD Oficial)
            // A. Costo por ítems requeridos
            int itemsCost = 0;
            foreach (var req in reqs)
            {
                int unitPrice = (req.Item != null && req.Item.BasePrice > 0) ? req.Item.BasePrice : 10;
                itemsCost += unitPrice * req.RequiredCount;
            }

            // B. Bonus por Material de Embalaje Solicitado:
            // Estándar: +$0 | 1 Material (Espuma o Film): +$15 | Doble Material (Espuma Y Film): +$35
            int packagingBonus = 0;
            if (conditionType == 3)
            {
                packagingBonus = 35; // Doble material
            }
            else if (conditionType == 0 || conditionType == 1 || conditionType == 4 || conditionType == 6)
            {
                packagingBonus = 15; // 1 material
            }

            // C. Bonus por Complejidad Lógica:
            // Proposición Atómica: +$10 | Molecular con Conectivo: +$20 | Bicondicional: +$35
            int logicBonus = 10;
            if (condition.Operator == LogicOperator.Biconditional)
            {
                logicBonus = 35;
            }
            else if (condition.Operator != LogicOperator.None)
            {
                logicBonus = 20;
            }

            return new Order(orderId, condition, reqs, duration, itemsCost, packagingBonus, logicBonus);
        }

        private static Order GenerateCompanyOrder(List<ItemData> availableItems)
        {
            string orderId = $"EMP-#{_orderSequence++:D3}";
            string companyName = CompanyNames[UnityEngine.Random.Range(0, CompanyNames.Length)];
            float duration = UnityEngine.Random.Range(70f, 81f);
            int reward = UnityEngine.Random.Range(100, 151);
            int penalty = UnityEngine.Random.Range(35, 46);
            int targetCount = UnityEngine.Random.Range(2, 4); // 2 o 3 piezas en total

            // 1. Generar Regla de Teoría de Conjuntos
            SetTheoryRule rule = GenerateRandomSetRule();
            HashSet<ItemData> validItems = rule.ResolveValidItems(availableItems);

            // Garantía de no vacuidad: si por algún motivo la regla resolviera 0 ítems, usar unión segura
            if (validItems.Count == 0)
            {
                rule = new SetTheoryRule(
                    SetOperator.Union,
                    SetCriterion.CreateFamily("Metales", "Metales"),
                    SetCriterion.CreateFamily("Textiles", "Textiles"),
                    "Insumos de metales o telas suaves"
                );
                validItems = rule.ResolveValidItems(availableItems);
            }

            // 2. Condición opcional de embalaje (Espuma / Film Térmico)
            var condition = new EnvironmentalCondition();
            int pkgRoll = UnityEngine.Random.Range(0, 4);
            if (pkgRoll == 1)
            {
                condition.LeftPredicate = AtomicPredicate.BoxHasFoam;
                condition.Operator = LogicOperator.None;
                condition.NaturalLanguageText = "Embalar con Relleno de Espuma";
            }
            else if (pkgRoll == 2)
            {
                condition.LeftPredicate = AtomicPredicate.BoxHasThermalFilm;
                condition.Operator = LogicOperator.None;
                condition.NaturalLanguageText = "Embalar con Film Térmico";
            }
            else
            {
                condition.LeftPredicate = AtomicPredicate.AlwaysTrue;
                condition.Operator = LogicOperator.None;
                condition.NaturalLanguageText = "Embalaje estándar";
            }

            // Crear la orden con CachedValidItems asignado inmediatamente (Ajuste 1 de arquitectura)
            return new Order(orderId, companyName, rule, validItems, targetCount, condition, duration, reward, penalty);
        }

        private static SetTheoryRule GenerateRandomSetRule()
        {
            int ruleType = UnityEngine.Random.Range(0, 10);
            switch (ruleType)
            {
                case 0:
                    return new SetTheoryRule(
                        SetOperator.Union,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        SetCriterion.CreateFamily("Textiles", "Textiles"),
                        "Piezas de fijación metálica O telas suaves"
                    );

                case 1:
                    return new SetTheoryRule(
                        SetOperator.Union,
                        SetCriterion.CreateFamily("Electricidad", "Eléctricos"),
                        SetCriterion.CreateFamily("Óptica", "Óptica"),
                        "Componentes eléctricos O cristales ópticos"
                    );

                case 2:
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateFamily("Electricidad", "Eléctricos"),
                        SetCriterion.CreateAttribute(ItemAttributes.Tool, "Herramientas"),
                        "Insumos eléctricos utilizables como herramientas"
                    );

                case 3:
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateFamily("Óptica", "Óptica"),
                        SetCriterion.CreateAttribute(ItemAttributes.Electrical, "Eléctricos"),
                        "Piezas ópticas con propiedades eléctricas"
                    );

                case 4:
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        SetCriterion.CreateTier(ItemTier.Tier2_Medium, "Tier 2"),
                        "Fijaciones metálicas de calidad intermedia (Tier 2)"
                    );

                case 5:
                    return new SetTheoryRule(
                        SetOperator.Difference,
                        SetCriterion.CreateFamily("Electricidad", "Eléctricos"),
                        SetCriterion.CreateAttribute(ItemAttributes.Fragile, "Frágiles"),
                        "Componentes eléctricos resistentes (NO frágiles)"
                    );

                case 6:
                    return new SetTheoryRule(
                        SetOperator.Difference,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        SetCriterion.CreateTier(ItemTier.Tier3_Advanced, "Tier 3"),
                        "Herrajes metálicos ligeros (NO de titanio pesado)"
                    );

                case 7:
                    return new SetTheoryRule(
                        SetOperator.Difference,
                        SetCriterion.CreateFamily("Óptica", "Óptica"),
                        SetCriterion.CreateAttribute(ItemAttributes.Tool, "Herramientas"),
                        "Cristalería óptica pura (sin instrumental de lab)"
                    );

                case 8:
                    return new SetTheoryRule(
                        SetOperator.Complement,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        null,
                        "Cualquier producto del almacén EXCEPTO fijaciones metálicas"
                    );

                case 9:
                default:
                    return new SetTheoryRule(
                        SetOperator.Complement,
                        SetCriterion.CreateAttribute(ItemAttributes.Fragile, "Frágiles"),
                        null,
                        "Cualquier insumo del almacén que NO sea frágil"
                    );
            }
        }
    }
}
