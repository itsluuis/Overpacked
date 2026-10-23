using System;

namespace LogiPack.Logic
{
    public enum PropositionalOperator
    {
        And,            // ∧ (Conjunción)
        Or,             // ∨ (Disyunción)
        Conditional,    // → (Condicional / Si...entonces)
        Biconditional   // ↔ (Bicondicional / Si y solo si)
    }

    public static class PropositionalOperatorExtensions
    {
        public static string GetSymbol(this PropositionalOperator op)
        {
            switch (op)
            {
                case PropositionalOperator.And: return "∧";
                case PropositionalOperator.Or: return "∨";
                case PropositionalOperator.Conditional: return "→";
                case PropositionalOperator.Biconditional: return "↔";
                default: return "?";
            }
        }

        public static string GetNaturalLanguage(this PropositionalOperator op)
        {
            switch (op)
            {
                case PropositionalOperator.And: return " y ";
                case PropositionalOperator.Or: return " o ";
                case PropositionalOperator.Conditional: return " entonces ";
                case PropositionalOperator.Biconditional: return " si y solo si ";
                default: return " ";
            }
        }

        public static bool Evaluate(this PropositionalOperator op, bool left, bool right)
        {
            switch (op)
            {
                case PropositionalOperator.And:
                    return left && right;
                case PropositionalOperator.Or:
                    return left || right;
                case PropositionalOperator.Conditional:
                    return !left || right; // p → q ≡ ¬p ∨ q
                case PropositionalOperator.Biconditional:
                    return left == right;  // p ↔ q ≡ (p ∧ q) ∨ (¬p ∧ ¬q)
                default:
                    return false;
            }
        }
    }
}
