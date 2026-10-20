using LogiPack.Logic;
using LogiPack.Managers;
using UnityEngine;

namespace LogiPack.UI
{
    /// <summary>
    /// Gestiona las representaciones visuales del entorno:
    /// - Reloj digital LED hora por hora (06:00 a 22:00).
    /// - Pantallas TV de pronóstico (Clima y Temperatura con icono NEXT).
    /// - Fondo meteorológico dinámico visible a través de la ventana y compuerta abierta.
    /// </summary>
    public class HUDEnvironmentVisuals : MonoBehaviour
    {
        [Header("Referencias a Managers")]
        public EnvironmentManager Environment;

        [Header("Reloj Digital")]
        public SpriteRenderer ClockRenderer;
        public Sprite[] ClockSprites; // 17 frames: reloj_06_00 a reloj_22_00

        [Header("Monitores TV (Pronóstico NEXT)")]
        public SpriteRenderer WeatherTVRenderer;
        public Sprite WeatherTVSunny;
        public Sprite WeatherTVCloudy;
        public Sprite WeatherTVRainy;
        public Sprite WeatherTVStormy;

        public SpriteRenderer TempTVRenderer;
        public Sprite TempTVCold;
        public Sprite TempTVNormal;
        public Sprite TempTVHot;

        [Header("Fondo Exterior de la Ventana")]
        public SpriteRenderer WindowWeatherRenderer;
        public Sprite WindowWeatherSunny;
        public Sprite WindowWeatherCloudy;
        public Sprite WindowWeatherRainy;
        public Sprite WindowWeatherStormy;

        [Header("Animación en Bucle de Ventana (Lluvia y Tormenta)")]
        public Sprite[] WindowRainFrames;  // 4 frames: clima_lloviendo_fase_1..4
        public Sprite[] WindowStormFrames; // 4 frames: clima_tormenta_fase_1..4

        private float _weatherAnimTimer = 0f;
        private int _weatherFrameIndex = 0;
        private const float WEATHER_FRAME_DURATION = 0.15f; // ~6.6 FPS

        private void Start()
        {
            if (Environment != null)
            {
                Environment.OnEnvironmentChanged += UpdateVisuals;
                UpdateVisuals(Environment.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (Environment != null)
            {
                Environment.OnEnvironmentChanged -= UpdateVisuals;
            }
        }

        private void Update()
        {
            if (Environment != null)
            {
                // Sincronización continua de la hora actual
                UpdateClock(Mathf.RoundToInt(Environment.CurrentState.CurrentHour));

                // Animación en bucle de lluvia o tormenta
                AnimateWindowWeather(Environment.CurrentState.Weather);
            }
        }

        public void UpdateVisuals(EnvironmentState state)
        {
            UpdateClock(Mathf.RoundToInt(state.CurrentHour));
            UpdateForecastTVs();

            // Si es clima estático, asignar de inmediato
            if (state.Weather == WeatherType.Clear || state.Weather == WeatherType.Cloudy)
            {
                _weatherAnimTimer = 0f;
                _weatherFrameIndex = 0;
                UpdateWindowWeather(state.Weather);
            }
        }

        private void AnimateWindowWeather(WeatherType weather)
        {
            if (WindowWeatherRenderer == null) return;

            if (weather == WeatherType.Rain && WindowRainFrames != null && WindowRainFrames.Length > 0)
            {
                _weatherAnimTimer += Time.deltaTime;
                if (_weatherAnimTimer >= WEATHER_FRAME_DURATION)
                {
                    _weatherAnimTimer = 0f;
                    _weatherFrameIndex = (_weatherFrameIndex + 1) % WindowRainFrames.Length;
                    if (WindowRainFrames[_weatherFrameIndex] != null)
                    {
                        WindowWeatherRenderer.sprite = WindowRainFrames[_weatherFrameIndex];
                    }
                }
            }
            else if (weather == WeatherType.Storm && WindowStormFrames != null && WindowStormFrames.Length > 0)
            {
                _weatherAnimTimer += Time.deltaTime;
                if (_weatherAnimTimer >= WEATHER_FRAME_DURATION)
                {
                    _weatherAnimTimer = 0f;
                    _weatherFrameIndex = (_weatherFrameIndex + 1) % WindowStormFrames.Length;
                    if (WindowStormFrames[_weatherFrameIndex] != null)
                    {
                        WindowWeatherRenderer.sprite = WindowStormFrames[_weatherFrameIndex];
                    }
                }
            }
            else
            {
                _weatherAnimTimer = 0f;
                _weatherFrameIndex = 0;
                UpdateWindowWeather(weather);
            }
        }

        private void UpdateClock(int hour)
        {
            if (ClockRenderer == null || ClockSprites == null || ClockSprites.Length == 0) return;

            // Hora va de 6 a 22 (índice 0 a 16)
            int index = Mathf.Clamp(hour - 6, 0, ClockSprites.Length - 1);
            if (ClockSprites[index] != null)
            {
                ClockRenderer.sprite = ClockSprites[index];
            }
        }

        private void UpdateWindowWeather(WeatherType weather)
        {
            if (WindowWeatherRenderer == null) return;

            WindowWeatherRenderer.sprite = weather switch
            {
                WeatherType.Clear => WindowWeatherSunny,
                WeatherType.Cloudy => WindowWeatherCloudy,
                WeatherType.Rain => (WindowRainFrames != null && WindowRainFrames.Length > 0) ? WindowRainFrames[0] : WindowWeatherRainy,
                WeatherType.Storm => (WindowStormFrames != null && WindowStormFrames.Length > 0) ? WindowStormFrames[0] : WindowWeatherStormy,
                _ => WindowWeatherSunny
            };
        }

        private void UpdateForecastTVs()
        {
            if (Environment == null) return;

            // 1. TV Pronóstico del Clima
            if (WeatherTVRenderer != null)
            {
                WeatherTVRenderer.sprite = Environment.NextWeather switch
                {
                    WeatherType.Clear => WeatherTVSunny,
                    WeatherType.Cloudy => WeatherTVCloudy,
                    WeatherType.Rain => WeatherTVRainy,
                    WeatherType.Storm => WeatherTVStormy,
                    _ => WeatherTVSunny
                };
            }

            // 2. TV Pronóstico de la Temperatura
            if (TempTVRenderer != null)
            {
                TempTVRenderer.sprite = Environment.NextTemperature switch
                {
                    TemperatureType.Cold => TempTVCold,
                    TemperatureType.Normal => TempTVNormal,
                    TemperatureType.Hot => TempTVHot,
                    _ => TempTVNormal
                };
            }
        }
    }
}
