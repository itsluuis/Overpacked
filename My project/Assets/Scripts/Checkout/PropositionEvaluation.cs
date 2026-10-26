namespace LogiPack.Checkout
{
    /// <summary>
    /// Clasificación de una fórmula proposicional según su tabla de verdad.
    /// </summary>
    public enum PropositionEvaluation
    {
        Tautology,      // Verdadera en todos los casos de la tabla de verdad (100% V)
        Contradiction,  // Falsa en todos los casos de la tabla de verdad (100% F)
        Contingency     // Verdadera en algunos casos y falsa en otros (mixta)
    }
}
