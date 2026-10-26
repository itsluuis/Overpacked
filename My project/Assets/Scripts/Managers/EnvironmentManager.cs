using System;
using System.Collections.Generic;
using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Controlador del entorno y del reloj de la jornada (6:00 AM a 22:00 PM).
    /// Jornada de 4 minutos reales (240s): avanza 1 hora exacta cada 15 segundos reales.
    /// Sincroniza el clima y la temperatura al cambio de hora, generando un pronóstico
    /// dinámico con probabilidad de repetición decayente.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public const float REAL_DAY_DURATION = 240f; // 4 minutos en la vida real
        public const float SECONDS_PER_HOUR = 15f;   // 15 segundos reales por hora de juego
        public const int START_HOUR = 6;
        public const int END_HOUR = 22;

        public static EnvironmentManager Instance { get; private set; }

        public EnvironmentState CurrentState;

        [Header("Pronóstico para la Próxima Hora")]
        public WeatherType NextWeather;
        public TemperatureType NextTemperature;

        public bool IsDayOver { get; private set; }
        public bool IsDayPaused = false;
        public int CurrentDayNumber { get; private set; } = 1;

        public event Action<EnvironmentState> OnEnvironmentChanged;
        public event Action OnDayCompleted;

        private float _elapsedRealTime = 0f;
        private int _currentHourInt = START_HOUR;
        private int _consecutiveWeatherRepeats = 0;
        private int _consecutiveTempRepeats = 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _elapsedRealTime = 0f;
            _currentHourInt = START_HOUR;
            IsDayOver = false;

            // Estado inicial a las 06:00
            CurrentState.CurrentHour = START_HOUR;
            CurrentState.TimeOfDay = TimeOfDay.Morning;
            CurrentState.Weather = WeatherType.Clear;
            CurrentState.Temperature = TemperatureType.Normal;

            // Pronóstico inicial para las 07:00
            NextWeather = PickNextWeather(CurrentState.Weather);
            NextTemperature = PickNextTemperature(CurrentState.Temperature);

            OnEnvironmentChanged?.Invoke(CurrentState);
            Debug.Log($"[EnvironmentManager] Jornada iniciada a las {START_HOUR:D2}:00. Clima: {CurrentState.Weather}, Temp: {CurrentState.Temperature}. Pronóstico: {NextWeather}, {NextTemperature}.");
        }

        private void Update()
        {
            if (IsDayOver || IsDayPaused || LogiPack.Core.GameModeManager.IsGameplayPaused) return;

            _elapsedRealTime += Time.deltaTime;

            // En Modo Infinito, el ambiente rota dinámicamente sin finalizar a las 22:00 (el fin lo decide InfiniteModeController)
            if (LogiPack.Core.GameModeManager.IsInfiniteMode)
            {
                int currentHourStep = START_HOUR + Mathf.FloorToInt(_elapsedRealTime / SECONDS_PER_HOUR);
                if (currentHourStep > _currentHourInt)
                {
                    _currentHourInt = currentHourStep;
                    int wrappedHour = START_HOUR + ((currentHourStep - START_HOUR) % (END_HOUR - START_HOUR + 1));
                    CurrentState.CurrentHour = wrappedHour;
                    CurrentState.TimeOfDay = wrappedHour switch
                    {
                        < 12 => TimeOfDay.Morning,
                        < 19 => TimeOfDay.Afternoon,
                        _ => TimeOfDay.Night
                    };
                    CurrentState.Weather = NextWeather;
                    CurrentState.Temperature = NextTemperature;
                    NextWeather = PickNextWeather(CurrentState.Weather);
                    NextTemperature = PickNextTemperature(CurrentState.Temperature);
                    OnEnvironmentChanged?.Invoke(CurrentState);
                }
                return;
            }

            // Calcular hora de juego actual (discreta, de hora en hora: 6, 7, 8... 22)
            int targetHour = START_HOUR + Mathf.FloorToInt(_elapsedRealTime / SECONDS_PER_HOUR);

            if (targetHour > _currentHourInt && targetHour <= END_HOUR)
            {
                AdvanceHour(targetHour);
            }

            // Fin de jornada a las 22:00 PM (al completarse los 240s)
            if (_elapsedRealTime >= REAL_DAY_DURATION)
            {
                _elapsedRealTime = REAL_DAY_DURATION;
                CurrentState.CurrentHour = END_HOUR;
                IsDayOver = true;
                Debug.Log("[EnvironmentManager] ¡Fin de la jornada laboral a las 22:00 PM!");
                OnEnvironmentChanged?.Invoke(CurrentState);
                OnDayCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Reinicia la jornada laboral a las 06:00 AM para un nuevo turno o reintento.
        /// </summary>
        public void RestartDay(bool advanceDayCount = true)
        {
            if (advanceDayCount)
            {
                CurrentDayNumber++;
            }

            _elapsedRealTime = 0f;
            _currentHourInt = START_HOUR;
            IsDayOver = false;

            // Restablecer estado inicial
            CurrentState.CurrentHour = START_HOUR;
            CurrentState.TimeOfDay = TimeOfDay.Morning;
            CurrentState.Weather = WeatherType.Clear;
            CurrentState.Temperature = TemperatureType.Normal;

            NextWeather = PickNextWeather(CurrentState.Weather);
            NextTemperature = PickNextTemperature(CurrentState.Temperature);

            OnEnvironmentChanged?.Invoke(CurrentState);
            Debug.Log($"<color=cyan><b>[EnvironmentManager]</b></color> Nueva jornada iniciada (Día {CurrentDayNumber}) a las {START_HOUR:D2}:00.");
        }

        private void AdvanceHour(int newHour)
        {
            _currentHourInt = newHour;
            CurrentState.CurrentHour = _currentHourInt;

            // Actualizar franja pasiva
            if (_currentHourInt < 12)
                CurrentState.TimeOfDay = TimeOfDay.Morning;
            else if (_currentHourInt < 18)
                CurrentState.TimeOfDay = TimeOfDay.Afternoon;
            else
                CurrentState.TimeOfDay = TimeOfDay.Night;

            // El pronóstico de la hora anterior se convierte en el estado actual
            CurrentState.Weather = NextWeather;
            CurrentState.Temperature = NextTemperature;

            // Generar nuevo pronóstico para la hora siguiente con decaimiento de repetición
            NextWeather = PickNextWeather(CurrentState.Weather);
            NextTemperature = PickNextTemperature(CurrentState.Temperature);

            OnEnvironmentChanged?.Invoke(CurrentState);
            Debug.Log($"<color=green><b>[EnvironmentManager]</b></color> Nueva hora: {_currentHourInt:D2}:00. Clima actual: {CurrentState.Weather} | Pronóstico: {NextWeather}. Temp actual: {CurrentState.Temperature} | Pronóstico: {NextTemperature}.");
        }

        private WeatherType PickNextWeather(WeatherType current)
        {
            // Probabilidad baja de repetir (20%), decayendo drásticamente si se repite consecutivamente
            float repeatChance = 0.20f / (_consecutiveWeatherRepeats + 1);

            if (UnityEngine.Random.value < repeatChance)
            {
                _consecutiveWeatherRepeats++;
                return current;
            }

            _consecutiveWeatherRepeats = 0;
            var available = new List<WeatherType>();
            foreach (WeatherType w in Enum.GetValues(typeof(WeatherType)))
            {
                if (w != current) available.Add(w);
            }

            return available[UnityEngine.Random.Range(0, available.Count)];
        }

        private TemperatureType PickNextTemperature(TemperatureType current)
        {
            // Probabilidad baja de repetir (20%), decayendo drásticamente si se repite consecutivamente
            float repeatChance = 0.20f / (_consecutiveTempRepeats + 1);

            if (UnityEngine.Random.value < repeatChance)
            {
                _consecutiveTempRepeats++;
                return current;
            }

            _consecutiveTempRepeats = 0;
            var available = new List<TemperatureType>();
            foreach (TemperatureType t in Enum.GetValues(typeof(TemperatureType)))
            {
                if (t != current) available.Add(t);
            }

            return available[UnityEngine.Random.Range(0, available.Count)];
        }
    }
}
