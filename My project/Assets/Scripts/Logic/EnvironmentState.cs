using System;
using UnityEngine;

namespace LogiPack.Logic
{
    /// <summary>
    /// Estados atómicos posibles de las variables ambientales del almacén.
    /// </summary>
    public enum WeatherType
    {
        Clear,      // Despejado / Sol
        Rain,       // Lluvia
        Cloudy,     // Nublado
        Storm       // Tormenta
    }

    public enum TemperatureType
    {
        Cold,       // Frío
        Normal,     // Templado
        Hot         // Calor
    }

    public enum TimeOfDay
    {
        Morning,    // 6:00 - 11:59
        Afternoon,  // 12:00 - 17:59
        Night       // 18:00 - 22:00
    }

    /// <summary>
    /// Estructura que captura el estado del entorno en un momento específico.
    /// Funciona como la asignación de verdad para las proposiciones atómicas.
    /// </summary>
    [Serializable]
    public struct EnvironmentState
    {
        public WeatherType Weather;
        public TemperatureType Temperature;
        public TimeOfDay TimeOfDay;
        public float CurrentHour; // 6.0f a 22.0f

        public bool IsRaining => Weather == WeatherType.Rain || Weather == WeatherType.Storm;
        public bool IsStorming => Weather == WeatherType.Storm;
        public bool IsClear => Weather == WeatherType.Clear;
        public bool IsCloudy => Weather == WeatherType.Cloudy;

        public bool IsCold => Temperature == TemperatureType.Cold;
        public bool IsNormalTemp => Temperature == TemperatureType.Normal;
        public bool IsHot => Temperature == TemperatureType.Hot;

        public bool IsMorning => TimeOfDay == TimeOfDay.Morning;
        public bool IsAfternoon => TimeOfDay == TimeOfDay.Afternoon;
        public bool IsNight => TimeOfDay == TimeOfDay.Night;
    }
}
