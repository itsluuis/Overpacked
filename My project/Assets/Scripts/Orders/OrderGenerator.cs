using LogiPack.Core;
using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Orders
{
    /// <summary>
    /// Proveedor de reglas procedurales de Teoría de Conjuntos y nombres de empresas
    /// para los pedidos del almacén en LogiPack.
    /// </summary>
    public static class OrderGenerator
    {
        public static readonly string[] CompanyNames = new string[]
        {
            "Ferretería Pim Pom Pim",
            "Chispitas & Voltios Bip-Bop",
            "Telas Pom Pón",
            "Cristales Glup-Glup",
            "Taller Tuerca Loca",
            "Industrias Patapúm",
            "Laboratorio Burbujitas"
        };

        public static SetTheoryRule GenerateRandomSetRule()
        {
            int ruleType = UnityEngine.Random.Range(0, 15);
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
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        SetCriterion.CreateDirectPropositions("P"),
                        "Fijaciones metálicas VIP (Perno Titanio p)"
                    );

                case 3:
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateFamily("Óptica", "Óptica"),
                        SetCriterion.CreateDirectPropositions("P"),
                        "Piezas ópticas VIP (Cristal de Laboratorio s)"
                    );

                case 4:
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        SetCriterion.CreateNegatedPropositions("¬P"),
                        "Fijaciones metálicas básicas (Tornillo ¬p)"
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
                        SetCriterion.CreateDirectPropositions("P"),
                        "Herrajes metálicos básicos excluyendo piezas VIP (Tornillo ¬p)"
                    );

                case 7:
                    return new SetTheoryRule(
                        SetOperator.Difference,
                        SetCriterion.CreateFamily("Óptica", "Óptica"),
                        SetCriterion.CreateDirectPropositions("P"),
                        "Cristalería básica excluyendo piezas VIP (Vidrio Plano ¬s)"
                    );

                case 8:
                    return new SetTheoryRule(
                        SetOperator.Complement,
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        null,
                        "Cualquier producto del almacén EXCEPTO fijaciones metálicas"
                    );

                case 9:
                    return new SetTheoryRule(
                        SetOperator.Complement,
                        SetCriterion.CreateAttribute(ItemAttributes.Fragile, "Frágiles"),
                        null,
                        "Cualquier insumo del almacén que NO sea frágil"
                    );

                case 10:
                    // Conjunto de Proposiciones Negadas: Tornillo (~p), Cable (~q), Vidrio (~s), Algodón (~r)
                    return new SetTheoryRule(
                        SetOperator.Union,
                        SetCriterion.CreateNegatedPropositions("¬P"),
                        null,
                        "Cualquier insumo del conjunto de proposiciones negadas (Tornillo, Cable, Vidrio o Algodón)"
                    );

                case 11:
                    // Intersección: Proposiciones Negadas ∩ Metales -> Tornillo Estándar (~p)
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateNegatedPropositions("¬P"),
                        SetCriterion.CreateFamily("Metales", "Metales"),
                        "Fijaciones metálicas del conjunto de proposiciones negadas (Tornillo ¬p)"
                    );

                case 12:
                    // Intersección: Proposiciones Directas ∩ Eléctricos -> Batería Litio (q)
                    return new SetTheoryRule(
                        SetOperator.Intersection,
                        SetCriterion.CreateDirectPropositions("P"),
                        SetCriterion.CreateFamily("Electricidad", "Eléctricos"),
                        "Componentes eléctricos del conjunto de proposiciones directas (Batería Litio q)"
                    );

                case 13:
                    // Diferencia: Proposiciones Negadas − Textiles -> Tornillo, Cable o Vidrio
                    return new SetTheoryRule(
                        SetOperator.Difference,
                        SetCriterion.CreateNegatedPropositions("¬P"),
                        SetCriterion.CreateFamily("Textiles", "Textiles"),
                        "Proposiciones negadas de taller excluyendo telas (Tornillo, Cable o Vidrio)"
                    );

                case 14:
                default:
                    // Unión: Proposiciones Negadas ∪ Proposiciones Directas (insumos proposicionales puros)
                    return new SetTheoryRule(
                        SetOperator.Union,
                        SetCriterion.CreateNegatedPropositions("¬P"),
                        SetCriterion.CreateDirectPropositions("P"),
                        "Insumos proposicionales puros (directas o negadas, sin piezas intermedias)"
                    );
            }
        }
    }
}
