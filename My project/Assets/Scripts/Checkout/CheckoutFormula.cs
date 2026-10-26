using System;
using System.Collections.Generic;

namespace LogiPack.Checkout
{
    /// <summary>
    /// Modela un expediente de auditoría con su FBF, traducción y clasificación teórica.
    /// </summary>
    [Serializable]
    public class CheckoutFormula
    {
        public string Id;
        public string FormulaFBF;
        public string NaturalLanguage;
        public PropositionEvaluation Evaluation;

        public CheckoutFormula(string id, string formulaFBF, string naturalLanguage, PropositionEvaluation eval)
        {
            Id = id;
            FormulaFBF = formulaFBF;
            NaturalLanguage = naturalLanguage;
            Evaluation = eval;
        }
    }

    /// <summary>
    /// Banco curado de más de 30 fórmulas con un máximo de 2 variables (p y q)
    /// garantizando tablas de verdad de 4 filas ágiles y resolución mental fluida.
    /// </summary>
    public static class CheckoutFormulaPool
    {
        private static readonly List<CheckoutFormula> CuratedBank = new List<CheckoutFormula>
        {
            // ==========================================
            // TAUTOLOGÍAS (100% Verdadera en toda fila)
            // ==========================================
            new CheckoutFormula("T01", "p ∨ ~p", "Llueve o no llueve (Tercero excluido)", PropositionEvaluation.Tautology),
            new CheckoutFormula("T02", "p → p", "Si hay stock de titanio, entonces hay stock de titanio", PropositionEvaluation.Tautology),
            new CheckoutFormula("T03", "~(p ∧ ~p)", "No es posible que llueva y no llueva a la vez", PropositionEvaluation.Tautology),
            new CheckoutFormula("T04", "(p ∧ q) → p", "Si llueve y hace frío, entonces seguro llueve", PropositionEvaluation.Tautology),
            new CheckoutFormula("T05", "(p ∧ q) → q", "Si llueve y hace frío, entonces seguro hace frío", PropositionEvaluation.Tautology),
            new CheckoutFormula("T06", "p → (p ∨ q)", "Si llueve, entonces llueve o hace frío", PropositionEvaluation.Tautology),
            new CheckoutFormula("T07", "q → (p ∨ q)", "Si hace frío, entonces llueve o hace frío", PropositionEvaluation.Tautology),
            new CheckoutFormula("T08", "(p ↔ q) ↔ (q ↔ p)", "La equivalencia es idéntica en ambos sentidos", PropositionEvaluation.Tautology),
            new CheckoutFormula("T09", "p → (q → p)", "Si llueve, que haga frío no altera que ya llueve", PropositionEvaluation.Tautology),
            new CheckoutFormula("T10", "(p ∨ q) ∨ ~p", "O se da alguno de los dos eventos, o no ocurre el primero", PropositionEvaluation.Tautology),
            new CheckoutFormula("T11", "(p → q) ∨ p", "O la lluvia implica frío, o simplemente llueve", PropositionEvaluation.Tautology),
            new CheckoutFormula("T12", "(p → q) ∨ (q → p)", "O la lluvia implica frío, o el frío implica lluvia", PropositionEvaluation.Tautology),

            // ==========================================
            // CONTRADICCIONES (100% Falsa en toda fila)
            // ==========================================
            new CheckoutFormula("C01", "p ∧ ~p", "Llueve y no llueve al mismo tiempo", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C02", "~(p ∨ ~p)", "Es imposible que ni llueva ni deje de llover", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C03", "p ↔ ~p", "Llueve si y solo si no llueve", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C04", "(p → q) ∧ (p ∧ ~q)", "Si llueve hace frío, pero llueve y no hace frío", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C05", "~(p → p)", "Es falso que si llueve entonces llueve", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C06", "(p ∨ q) ∧ (~p ∧ ~q)", "Ocurre alguno de los dos, pero ninguno ocurre", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C07", "(p ↔ q) ∧ (p ↔ ~q)", "Coinciden si son iguales y si son opuestos a la vez", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C08", "~p ∧ (~q ∧ (p ∨ q))", "Ni llueve ni hace frío, pero al menos uno ocurre", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C09", "(p ∧ ~p) ∧ q", "Hay contradicción en la lluvia y además hace frío", PropositionEvaluation.Contradiction),
            new CheckoutFormula("C10", "~((p ∧ q) → p)", "Es falso que si ocurren ambos entonces ocurra el primero", PropositionEvaluation.Contradiction),

            // ==========================================
            // CONTINGENCIAS (Mixtas según las variables)
            // ==========================================
            new CheckoutFormula("K01", "p ∧ q", "Llueve y hace frío en el almacén", PropositionEvaluation.Contingency),
            new CheckoutFormula("K02", "p ∨ q", "Llueve o hace frío en el almacén", PropositionEvaluation.Contingency),
            new CheckoutFormula("K03", "p → q", "Si llueve, entonces hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K04", "p ↔ q", "Llueve si y solo si hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K05", "~p ∨ q", "No llueve o hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K06", "p ∧ ~q", "Llueve pero no hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K07", "~p ∧ ~q", "Ni llueve ni hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K08", "p → ~q", "Si llueve, entonces no hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K09", "~p → q", "Si no llueve, entonces hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K10", "(p ∨ q) → (p ∧ q)", "Si ocurre alguno, entonces tienen que ocurrir ambos", PropositionEvaluation.Contingency),
            new CheckoutFormula("K11", "p ∨ (p ∧ q)", "Llueve, o llueve y hace frío", PropositionEvaluation.Contingency),
            new CheckoutFormula("K12", "~(p ∧ q)", "No es cierto que llueva y haga frío simultáneamente", PropositionEvaluation.Contingency)
        };

        /// <summary>
        /// Obtiene una baraja sin reemplazo de 'count' fórmulas seleccionadas aleatoriamente
        /// de forma puramente estocástica (cualquier mezcla de tipos).
        /// </summary>
        public static Queue<CheckoutFormula> GetShuffledSessionPool(int count = 9)
        {
            var list = new List<CheckoutFormula>(CuratedBank);

            // Algoritmo Fisher-Yates
            var rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var val = list[k];
                list[k] = list[n];
                list[n] = val;
            }

            int take = Math.Min(count, list.Count);
            var queue = new Queue<CheckoutFormula>();
            for (int i = 0; i < take; i++)
            {
                queue.Enqueue(list[i]);
            }
            return queue;
        }
    }
}
