using System;
using LogiPack.Logic;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Controlador del entorno y del reloj de la jornada (6:00 AM a 22:00 PM).
    /// Genera eventos aleatorios climáticos y de temperatura.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        [Header("Configuración del Tiempo")]
        [Tooltip("Duración de la jornada completa (6:00 a 22:00 = 16 horas) en segundos reales.")]
        public float RealDayDurationSeconds = 180f; // 3 minutos por día laboral

        public EnvironmentState CurrentState;
        public bool IsDayOver { get; private set; }

        public event Action<EnvironmentState> OnEnvironmentChanged;
        public event Action OnDayCompleted;

        private float _timer;
        private float _eventTimer;
        private float _nextEventInterval = 20f;

        private void Start()
        {
            // Inicialización de la jornada a las 6:00 AM
            CurrentState.CurrentHour = 6.0f;
            CurrentState.TimeOfDay = TimeOfDay.Morning;
            CurrentState.Weather = WeatherType.Clear;
            CurrentState.Temperature = TemperatureType.Normal;

            OnEnvironmentChanged?.Invoke(CurrentState);
        }

        private void Update()
        {
            if (IsDayOver) return;

            _timer += Time.deltaTime;
            _eventTimer += Time.deltaTime;

            // Progresión de la hora: de 6.0f a 22.0f
            float progress = Mathf.Clamp01(_timer / RealDayDurationSeconds);
            CurrentState.CurrentHour = Mathf.Lerp(6.0f, 22.0f, progress);

            // Actualizar rango horario
            if (CurrentState.CurrentHour < 12.0f)
                CurrentState.TimeOfDay = TimeOfDay.Morning;
            else if (CurrentState.CurrentHour < 18.0f)
                CurrentState.TimeOfDay = TimeOfDay.Afternoon;
            else
                CurrentState.TimeOfDay = TimeOfDay.Night;

            // Eventos climáticos aleatorios periódicos
            if (_eventTimer >= _nextEventInterval)
            {
                _eventTimer = 0f;
                _nextEventInterval = UnityEngine.Random.Range(15f, 30f);
                RandomizeWeatherAndTemp();
            }

            OnEnvironmentChanged?.Invoke(CurrentState);

            // Fin de la jornada a las 22:00 PM
            if (_timer >= RealDayDurationSeconds)
            {
                IsDayOver = true;
                Debug.Log("[EnvironmentManager] ¡Fin de la jornada laboral a las 22:00 PM!");
                OnDayCompleted?.Invoke();
            }
        }

        private void RandomizeWeatherAndTemp()
        {
            // Clima aleatorio
            Array weathers = Enum.GetValues(typeof(WeatherType));
            CurrentState.Weather = (WeatherType)weathers.GetValue(UnityEngine.Random.Range(0, weathers.Length));

            // Temperatura aleatoria
            Array temps = Enum.GetValues(typeof(TemperatureType));
            CurrentState.Temperature = (TemperatureType)temps.GetValue(UnityEngine.Random.Range(0, temps.Length));

            Debug.Log($"[EnvironmentManager] Cambio ambiental: Clima = {CurrentState.Weather}, Temp = {CurrentState.Temperature}, Hora = {CurrentState.CurrentHour:F1}h");
        }
    }
}
