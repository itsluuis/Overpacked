using LogiPack.Logic;
using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Muestra en pantalla el panel HUD (Zona Roja):
    /// - Reloj de jornada (06:00 AM a 22:00 PM avanzando de hora en hora).
    /// - Dinero acumulado.
    /// - Clima actual y pronóstico para la siguiente hora.
    /// - Temperatura actual y pronóstico para la siguiente hora.
    /// </summary>
    public class HUDManagerUI : MonoBehaviour
    {
        [Header("Referencias a Managers")]
        public EnvironmentManager Environment;
        public EconomyManager Economy;

        [Header("Textos del HUD")]
        public Text ClockText;
        public Text WeatherText;
        public Text TemperatureText;
        public Text MoneyText;

        private void Start()
        {
            if (Economy != null)
            {
                Economy.OnMoneyChanged += UpdateMoneyDisplay;
                UpdateMoneyDisplay(Economy.CurrentMoney);
            }

            if (Environment != null)
            {
                Environment.OnEnvironmentChanged += UpdateEnvironmentDisplay;
                UpdateEnvironmentDisplay(Environment.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (Economy != null)
            {
                Economy.OnMoneyChanged -= UpdateMoneyDisplay;
            }

            if (Environment != null)
            {
                Environment.OnEnvironmentChanged -= UpdateEnvironmentDisplay;
            }
        }

        private void Update()
        {
            if (Environment != null)
            {
                int h = Mathf.RoundToInt(Environment.CurrentState.CurrentHour);
                if (ClockText != null)
                {
                    ClockText.text = $"🕒 {h:D2}:00 {(h >= 12 ? "PM" : "AM")}";
                }
            }
        }

        private void UpdateEnvironmentDisplay(EnvironmentState state)
        {
            if (Environment == null) return;

            if (WeatherText != null)
            {
                string currW = FormatWeather(state.Weather);
                string nextW = FormatWeather(Environment.NextWeather);
                WeatherText.text = $"🌤️ {currW} (Próx: {nextW})";
            }

            if (TemperatureText != null)
            {
                string currT = FormatTemp(state.Temperature);
                string nextT = FormatTemp(Environment.NextTemperature);
                TemperatureText.text = $"🌡️ {currT} (Próx: {nextT})";
            }
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (MoneyText != null)
            {
                MoneyText.text = $"💰 ${money}";
            }
        }

        private string FormatWeather(WeatherType w)
        {
            return w switch
            {
                WeatherType.Clear => "Despejado ☀️",
                WeatherType.Rain => "Lluvia 🌧️",
                WeatherType.Cloudy => "Nublado ☁️",
                WeatherType.Storm => "Tormenta ⛈️",
                _ => w.ToString()
            };
        }

        private string FormatTemp(TemperatureType t)
        {
            return t switch
            {
                TemperatureType.Cold => "Frío ❄️",
                TemperatureType.Normal => "Templado 🍃",
                TemperatureType.Hot => "Calor 🔥",
                _ => t.ToString()
            };
        }
    }
}
