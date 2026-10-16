using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Muestra en pantalla el reloj de la jornada (6:00 a 22:00),
    /// el clima actual, la temperatura y el dinero acumulado.
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
        }

        private void Update()
        {
            if (Environment != null)
            {
                // Reloj: formatear hora decimal (ej. 6.5 = 06:30)
                float hour = Environment.CurrentState.CurrentHour;
                int h = Mathf.FloorToInt(hour);
                int m = Mathf.FloorToInt((hour - h) * 60f);
                if (ClockText != null)
                {
                    ClockText.text = $"🕒 {h:D2}:{m:D2} {(h >= 12 ? "PM" : "AM")}";
                }

                // Clima y Temperatura
                if (WeatherText != null)
                {
                    WeatherText.text = $"🌤️ {Environment.CurrentState.Weather}";
                }

                if (TemperatureText != null)
                {
                    TemperatureText.text = $"🌡️ {Environment.CurrentState.Temperature}";
                }
            }
        }

        private void UpdateMoneyDisplay(int money)
        {
            if (MoneyText != null)
            {
                MoneyText.text = $"💰 ${money}";
            }
        }
    }
}
