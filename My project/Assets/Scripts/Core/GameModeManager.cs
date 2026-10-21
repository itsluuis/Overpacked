namespace LogiPack.Core
{
    /// <summary>
    /// Identificador de los modos de juego soportados en Overpacked.
    /// </summary>
    public enum GameSessionMode
    {
        Tutorial,   // Modo estándar de cátedra (06:00 a 22:00, cuotas fijas, progresión tranquila)
        Infinite    // Modo arcade contrarreloj (rondas de 60s a 30s, cuotas crecientes, racha x1.5)
    }

    /// <summary>
    /// Almacena el estado de la sesión activa entre transiciones de escenas.
    /// </summary>
    public static class GameModeManager
    {
        public static GameSessionMode ActiveMode = GameSessionMode.Tutorial;

        public static bool IsInfiniteMode => ActiveMode == GameSessionMode.Infinite;
        public static bool IsTutorialMode => ActiveMode == GameSessionMode.Tutorial;
    }
}
