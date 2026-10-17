using System;
using LogiPack.Gameplay;
using UnityEngine;

namespace LogiPack.Logic
{
    /// <summary>
    /// Tipos de conectores lógicos según la lógica proposicional formal.
    /// </summary>
    public enum LogicOperator
    {
        None,           // Proposición Atómica simple
        Not,            // Negación (¬)
        And,            // Conjunción (∧)
        Or,             // Disyunción inclusiva (∨)
        Xor,            // Disyunción exclusiva (⊻)
        Implication,    // Condicional / Implicación (→)
        Biconditional   // Bicondicional / Doble implicación (↔)
    }

    /// <summary>
    /// Identificadores de las proposiciones atómicas observables en el almacén y en la caja.
    /// </summary>
    public enum AtomicPredicate
    {
        AlwaysTrue,
        IsRaining,          // Llueve (Lluvia o Tormenta)
        IsClear,            // Despejado / Sol
        IsStorming,         // Hay tormenta eléctrica
        IsCold,             // Hace frío
        IsHot,              // Hace calor
        BoxHasFoam,         // ¿La caja tiene Relleno de Espuma? (r)
        BoxHasThermalFilm,  // ¿La caja tiene Film Térmico? (s)
        IsMorning,          // [Deprecado - Informativo pasivo]
        IsAfternoon,        // [Deprecado - Informativo pasivo]
        IsNight             // [Deprecado - Informativo pasivo]
    }

    /// <summary>
    /// Estructura de condición lógica que soporta proposiciones moleculares y FBFs.
    /// Ejemplo: (p ∧ q) ↔ r
    /// </summary>
    [Serializable]
    public class EnvironmentalCondition
    {
        [Header("Fórmula en Lenguaje Natural")]
        public string NaturalLanguageText = "Enviar en cualquier momento";

        [Header("Estructura de la Proposición Molecular")]
        public AtomicPredicate LeftPredicate = AtomicPredicate.AlwaysTrue;
        public bool InvertLeft = false; // ¬p

        public LogicOperator Operator = LogicOperator.None;

        public AtomicPredicate RightPredicate = AtomicPredicate.AlwaysTrue;
        public bool InvertRight = false; // ¬q

        /// <summary>
        /// Evalúa si la proposición molecular es Verdadera (True) o Falsa (False)
        /// dado el estado ambiental actual y el estado de embalaje de la caja evaluada.
        /// </summary>
        public bool Evaluate(EnvironmentState state, PackageBox box = null)
        {
            bool leftVal = EvalPredicate(LeftPredicate, state, box);
            if (InvertLeft) leftVal = !leftVal;

            if (Operator == LogicOperator.None)
            {
                return leftVal;
            }

            bool rightVal = EvalPredicate(RightPredicate, state, box);
            if (InvertRight) rightVal = !rightVal;

            return Operator switch
            {
                LogicOperator.And => leftVal && rightVal,
                LogicOperator.Or => leftVal || rightVal,
                LogicOperator.Xor => leftVal ^ rightVal,
                LogicOperator.Implication => (!leftVal) || rightVal,      // p → q ≡ ¬p ∨ q
                LogicOperator.Biconditional => leftVal == rightVal,       // p ↔ q ≡ (p ∧ q) ∨ (¬p ∧ ¬q)
                LogicOperator.Not => !leftVal,
                _ => true
            };
        }

        private bool EvalPredicate(AtomicPredicate predicate, EnvironmentState state, PackageBox box)
        {
            return predicate switch
            {
                AtomicPredicate.AlwaysTrue => true,
                AtomicPredicate.IsRaining => state.IsRaining,
                AtomicPredicate.IsClear => state.IsClear,
                AtomicPredicate.IsStorming => state.IsStorming,
                AtomicPredicate.IsCold => state.IsCold,
                AtomicPredicate.IsHot => state.IsHot,
                AtomicPredicate.BoxHasFoam => box != null && box.HasFoamPadding,
                AtomicPredicate.BoxHasThermalFilm => box != null && box.HasThermalFilm,
                AtomicPredicate.IsMorning => state.IsMorning,
                AtomicPredicate.IsAfternoon => state.IsAfternoon,
                AtomicPredicate.IsNight => state.IsNight,
                _ => true
            };
        }
    }
}
