namespace LogiPack.Core
{
    /// <summary>
    /// Identificador de los modos de juego soportados en Overpacked.
    /// </summary>
    public enum GameSessionMode
    {
        Tutorial,   // Modo inducción con Mochi Miles en TutorialScene.unity
        NormalDay,  // Jornada Normal (06:00 a 22:00 en GameplayScene)
        Infinite,   // Modo arcade contrarreloj (GameplayScene)
        Checkout    // Modo auditoría de tablas de verdad en CheckoutScene.unity
    }

    /// <summary>
    /// Almacena el estado de la sesión activa entre transiciones de escenas.
    /// </summary>
    public static class GameModeManager
    {
        public static GameSessionMode ActiveMode = GameSessionMode.Tutorial;

        public static bool IsInfiniteMode => ActiveMode == GameSessionMode.Infinite;
        public static bool IsTutorialMode => ActiveMode == GameSessionMode.Tutorial;
        public static bool IsNormalDayMode => ActiveMode == GameSessionMode.NormalDay;
        public static bool IsCheckoutMode => ActiveMode == GameSessionMode.Checkout;

        /// <summary>
        /// Congela el avance del reloj, clima, spawn y caducidad de pedidos
        /// mientras el modal de fin de jornada está en pantalla.
        /// </summary>
        public static bool IsGameplayPaused { get; set; } = false;

        public static void SetGameplayPaused(bool paused)
        {
            IsGameplayPaused = paused;
            UnityEngine.Debug.Log($"[GameModeManager] Estado de pausa de juego: {paused}");
        }
    }
}
