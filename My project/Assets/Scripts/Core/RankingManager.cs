using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogiPack.Core
{
    [Serializable]
    public class NormalDayRecord
    {
        public int NetMoney;
        public int Stars;
        public string Timestamp;
    }

    [Serializable]
    public class InfiniteRecord
    {
        public int RoundsCleared;
        public int TotalMoney;
        public string Timestamp;
    }

    [Serializable]
    public class NormalDayRankingContainer
    {
        public List<NormalDayRecord> Records = new List<NormalDayRecord>();
    }

    [Serializable]
    public class InfiniteRankingContainer
    {
        public List<InfiniteRecord> Records = new List<InfiniteRecord>();
    }

    /// <summary>
    /// Gestor persistente y desacoplado del sistema de rankings (Top 5) y estado del tutorial.
    /// Almacena datos localmente mediante PlayerPrefs y JsonUtility.
    /// </summary>
    public static class RankingManager
    {
        private const string PREF_KEY_NORMAL = "LogiPack_Ranking_NormalDay_Top5";
        private const string PREF_KEY_INFINITE = "LogiPack_Ranking_Infinite_Top5";
        private const string PREF_KEY_TUTORIAL_COMPLETED = "LogiPack_Tutorial_Completed";

        #region Registro de Puntuaciones

        /// <summary>
        /// Registra una partida de Jornada Normal en el Top 5.
        /// Criterio de ordenación: Mayor saldo neto ($), desempate por estrellas (★).
        /// </summary>
        public static void RecordNormalDayScore(int netMoney, int stars)
        {
            var records = GetNormalTop5();

            var newRecord = new NormalDayRecord
            {
                NetMoney = netMoney,
                Stars = Mathf.Clamp(stars, 0, 3),
                Timestamp = DateTime.Now.ToString("dd/MM HH:mm")
            };

            records.Add(newRecord);

            // Ordenar: primero por NetMoney desc, luego por Stars desc
            records.Sort((a, b) =>
            {
                int cmp = b.NetMoney.CompareTo(a.NetMoney);
                if (cmp != 0) return cmp;
                return b.Stars.CompareTo(a.Stars);
            });

            // Limitar al Top 5
            if (records.Count > 5)
            {
                records = records.GetRange(0, 5);
            }

            var container = new NormalDayRankingContainer { Records = records };
            string json = JsonUtility.ToJson(container);
            PlayerPrefs.SetString(PREF_KEY_NORMAL, json);
            PlayerPrefs.Save();

            Debug.Log($"[RankingManager] Guardado récord Jornada Normal: ${netMoney} ({stars}★)");
        }

        /// <summary>
        /// Registra una partida de Jornada Infinita en el Top 5.
        /// Criterio de ordenación: Mayor cantidad de rondas superadas, desempate por dinero acumulado ($).
        /// </summary>
        public static void RecordInfiniteScore(int roundsCleared, int totalMoney)
        {
            var records = GetInfiniteTop5();

            var newRecord = new InfiniteRecord
            {
                RoundsCleared = roundsCleared,
                TotalMoney = totalMoney,
                Timestamp = DateTime.Now.ToString("dd/MM HH:mm")
            };

            records.Add(newRecord);

            // Ordenar: primero por RoundsCleared desc, luego por TotalMoney desc
            records.Sort((a, b) =>
            {
                int cmp = b.RoundsCleared.CompareTo(a.RoundsCleared);
                if (cmp != 0) return cmp;
                return b.TotalMoney.CompareTo(a.TotalMoney);
            });

            // Limitar al Top 5
            if (records.Count > 5)
            {
                records = records.GetRange(0, 5);
            }

            var container = new InfiniteRankingContainer { Records = records };
            string json = JsonUtility.ToJson(container);
            PlayerPrefs.SetString(PREF_KEY_INFINITE, json);
            PlayerPrefs.Save();

            Debug.Log($"[RankingManager] Guardado récord Jornada Infinita: Ronda {roundsCleared} - ${totalMoney}");
        }

        #endregion

        #region Consulta de Rankings

        /// <summary>
        /// Retorna la lista con los 5 mejores récords de Jornada Normal.
        /// Siempre devuelve una lista no nula.
        /// </summary>
        public static List<NormalDayRecord> GetNormalTop5()
        {
            if (!PlayerPrefs.HasKey(PREF_KEY_NORMAL))
            {
                return new List<NormalDayRecord>();
            }

            try
            {
                string json = PlayerPrefs.GetString(PREF_KEY_NORMAL, "");
                if (string.IsNullOrEmpty(json)) return new List<NormalDayRecord>();

                var container = JsonUtility.FromJson<NormalDayRankingContainer>(json);
                return container != null && container.Records != null ? container.Records : new List<NormalDayRecord>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RankingManager] Error al parsear ranking normal: {ex.Message}");
                return new List<NormalDayRecord>();
            }
        }

        /// <summary>
        /// Retorna la lista con los 5 mejores récords de Jornada Infinita.
        /// Siempre devuelve una lista no nula.
        /// </summary>
        public static List<InfiniteRecord> GetInfiniteTop5()
        {
            if (!PlayerPrefs.HasKey(PREF_KEY_INFINITE))
            {
                return new List<InfiniteRecord>();
            }

            try
            {
                string json = PlayerPrefs.GetString(PREF_KEY_INFINITE, "");
                if (string.IsNullOrEmpty(json)) return new List<InfiniteRecord>();

                var container = JsonUtility.FromJson<InfiniteRankingContainer>(json);
                return container != null && container.Records != null ? container.Records : new List<InfiniteRecord>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RankingManager] Error al parsear ranking infinito: {ex.Message}");
                return new List<InfiniteRecord>();
            }
        }

        #endregion

        #region Estado del Tutorial

        public static void SetTutorialCompleted(bool completed)
        {
            PlayerPrefs.SetInt(PREF_KEY_TUTORIAL_COMPLETED, completed ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt(PREF_KEY_TUTORIAL_COMPLETED, 0) == 1;
        }

        #endregion

        #region Utilidades / Resets

        public static void ClearAllRankings()
        {
            PlayerPrefs.DeleteKey(PREF_KEY_NORMAL);
            PlayerPrefs.DeleteKey(PREF_KEY_INFINITE);
            PlayerPrefs.DeleteKey(PREF_KEY_TUTORIAL_COMPLETED);
            PlayerPrefs.Save();
            Debug.Log("[RankingManager] Se han reiniciado todos los rankings y el progreso del tutorial.");
        }

        #endregion
    }
}
