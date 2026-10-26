using System;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;

namespace LogiPack.Logic
{
    public enum SetOperator
    {
        Union,          // A ∪ B
        Intersection,   // A ∩ B
        Difference,     // A - B
        Complement      // Aᶜ (U - A)
    }

    public enum CriterionType
    {
        Family,
        Attribute,
        Tier,
        PropositionalPolarity
    }

    [Serializable]
    public class SetCriterion
    {
        public CriterionType Type;
        public string Family;
        public ItemAttributes Attribute;
        public ItemTier Tier;
        public bool IsNegatedProposition;
        public string DisplayName;

        public static SetCriterion CreateFamily(string family, string displayName)
        {
            return new SetCriterion
            {
                Type = CriterionType.Family,
                Family = family,
                DisplayName = displayName
            };
        }

        public static SetCriterion CreateAttribute(ItemAttributes attr, string displayName)
        {
            return new SetCriterion
            {
                Type = CriterionType.Attribute,
                Attribute = attr,
                DisplayName = displayName
            };
        }

        public static SetCriterion CreateTier(ItemTier tier, string displayName)
        {
            return new SetCriterion
            {
                Type = CriterionType.Tier,
                Tier = tier,
                DisplayName = displayName
            };
        }

        public static SetCriterion CreateNegatedPropositions(string displayName = "¬P")
        {
            return new SetCriterion
            {
                Type = CriterionType.PropositionalPolarity,
                IsNegatedProposition = true,
                DisplayName = displayName
            };
        }

        public static SetCriterion CreateDirectPropositions(string displayName = "P")
        {
            return new SetCriterion
            {
                Type = CriterionType.PropositionalPolarity,
                IsNegatedProposition = false,
                DisplayName = displayName
            };
        }

        public bool Matches(ItemData item)
        {
            if (item == null) return false;

            switch (Type)
            {
                case CriterionType.Family:
                    return !string.IsNullOrEmpty(item.FamilyName) &&
                           item.FamilyName.IndexOf(Family, StringComparison.OrdinalIgnoreCase) >= 0;

                case CriterionType.Attribute:
                    return item.HasAttribute(Attribute);

                case CriterionType.Tier:
                    return item.Tier == Tier;

                case CriterionType.PropositionalPolarity:
                    // En LogiPack, las proposiciones negadas (~p, ~q, ~s, ~r) corresponden al Tier 1 (básicos: Tornillo, Cable, Vidrio, Algodón)
                    // y las proposiciones directas (p, q, s, r) corresponden al Tier 3 (avanzados: Titanio, Batería, Cristal Lab, Ignífugo).
                    return IsNegatedProposition
                        ? (item.Tier == ItemTier.Tier1_Basic)
                        : (item.Tier == ItemTier.Tier3_Advanced);

                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Modelo formal para reglas de Teoría de Conjuntos aplicadas al inventario de LogiPack.
    /// Resuelve subconjuntos válidos mediante Unión, Intersección, Diferencia y Complemento.
    /// </summary>
    [Serializable]
    public class SetTheoryRule
    {
        public SetOperator Operator;
        public SetCriterion LeftCriterion;
        public SetCriterion RightCriterion;

        public string FormalFormula;
        public string NaturalLanguageDescription;

        public SetTheoryRule(SetOperator op, SetCriterion left, SetCriterion right, string naturalDesc)
        {
            Operator = op;
            LeftCriterion = left;
            RightCriterion = right;
            NaturalLanguageDescription = naturalDesc;
            FormalFormula = BuildFormalFormula();
        }

        private string BuildFormalFormula()
        {
            string leftName = LeftCriterion != null ? LeftCriterion.DisplayName : "A";
            string rightName = RightCriterion != null ? RightCriterion.DisplayName : "B";

            switch (Operator)
            {
                case SetOperator.Union:
                    if (RightCriterion == null) return $"x ∈ {leftName}";
                    return $"x ∈ ({leftName} ∪ {rightName})";

                case SetOperator.Intersection:
                    return $"x ∈ ({leftName} ∩ {rightName})";

                case SetOperator.Difference:
                    return $"x ∈ ({leftName} − {rightName})";

                case SetOperator.Complement:
                    return $"x ∈ ({leftName})ᶜ";

                default:
                    return $"x ∈ {leftName}";
            }
        }

        /// <summary>
        /// Evalúa si un ítem individual cumple con la regla de teoría de conjuntos.
        /// </summary>
        public bool Matches(ItemData item)
        {
            if (item == null) return false;

            bool leftMatch = LeftCriterion != null && LeftCriterion.Matches(item);
            bool rightMatch = RightCriterion != null && RightCriterion.Matches(item);

            switch (Operator)
            {
                case SetOperator.Union:
                    return (RightCriterion != null) ? (leftMatch || rightMatch) : leftMatch;

                case SetOperator.Intersection:
                    return leftMatch && rightMatch;

                case SetOperator.Difference:
                    return leftMatch && !rightMatch;

                case SetOperator.Complement:
                    return !leftMatch;

                default:
                    return leftMatch;
            }
        }

        /// <summary>
        /// Resuelve y devuelve el conjunto de ítems del almacén que satisfacen la regla.
        /// </summary>
        public HashSet<ItemData> ResolveValidItems(List<ItemData> universe)
        {
            var validItems = new HashSet<ItemData>();
            if (universe == null) return validItems;

            foreach (var item in universe)
            {
                if (item != null && Matches(item))
                {
                    validItems.Add(item);
                }
            }

            return validItems;
        }
    }
}
