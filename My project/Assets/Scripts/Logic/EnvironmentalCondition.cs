using System;
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
    /// Identificadores de las proposiciones atómicas observables en el almacén.
    /// </summary>
    public enum AtomicPredicate
    {
        AlwaysTrue,
        IsRaining,          // Llueve (Lluvia o Tormenta)
        IsClear,            // Despejado / Sol
        IsStorming,         // Hay tormenta eléctrica
        IsCold,             // Hace frío
        IsHot,              // Hace calor
        IsMorning,          // Es por la mañana
        IsAfternoon,        // Es por la tarde
        IsNight             // Es por la noche
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
        /// dado el estado actual del almacén en tiempo de ejecución.
        /// </summary>
        public bool Evaluate(EnvironmentState state)
        {
            bool leftVal = EvalPredicate(LeftPredicate, state);
            if (InvertLeft) leftVal = !leftVal;

            if (Operator == LogicOperator.None)
            {
                return leftVal;
            }

            bool rightVal = EvalPredicate(RightPredicate, state);
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

        private bool EvalPredicate(AtomicPredicate predicate, EnvironmentState state)
        {
            return predicate switch
            {
                AtomicPredicate.AlwaysTrue => true,
                AtomicPredicate.IsRaining => state.IsRaining,
                AtomicPredicate.IsClear => state.IsClear,
                AtomicPredicate.IsStorming => state.IsStorming,
                AtomicPredicate.IsCold => state.IsCold,
                AtomicPredicate.IsHot => state.IsHot,
                AtomicPredicate.IsMorning => state.IsMorning,
                AtomicPredicate.IsAfternoon => state.IsAfternoon,
                AtomicPredicate.IsNight => state.IsNight,
                _ => true
            };
        }
    }
}
