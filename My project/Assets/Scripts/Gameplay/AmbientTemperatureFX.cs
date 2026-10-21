using LogiPack.Logic;
using LogiPack.Managers;
using UnityEngine;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Gestiona las partículas ambientales del almacén según la temperatura de la hora actual:
    /// - Frío: Estrellas celestes de 4 puntas cayendo suavemente desde el techo.
    /// - Calor: Ondas y chispas de gradiente cálido ascendiendo desde el suelo.
    /// - Normal: Sin partículas.
    /// 
    /// Capa visual: Sorting Layer Default, Order in Layer = 3 (por encima de las estanterías,
    /// pero por detrás de la mesa de empaque, cajas y tarjetas de pedidos).
    /// </summary>
    public class AmbientTemperatureFX : MonoBehaviour
    {
        [Header("Referencias a Managers")]
        public EnvironmentManager EnvManager;

        [Header("Sistemas de Partículas")]
        public ParticleSystem ColdParticles;
        public ParticleSystem HeatParticles;

        private void Start()
        {
            if (EnvManager == null)
            {
                EnvManager = FindAnyObjectByType<EnvironmentManager>();
            }

            if (EnvManager != null)
            {
                EnvManager.OnEnvironmentChanged += HandleEnvironmentChanged;
                // Aplicar estado térmico actual al iniciar
                HandleEnvironmentChanged(EnvManager.CurrentState);
            }
            else
            {
                // Fallback por defecto: templado (sin partículas)
                SetTemperatureFX(TemperatureType.Normal);
            }
        }

        private void OnDestroy()
        {
            if (EnvManager != null)
            {
                EnvManager.OnEnvironmentChanged -= HandleEnvironmentChanged;
            }
        }

        private void HandleEnvironmentChanged(EnvironmentState state)
        {
            SetTemperatureFX(state.Temperature);
        }

        public void SetTemperatureFX(TemperatureType temperature)
        {
            switch (temperature)
            {
                case TemperatureType.Cold:
                    EnableEmission(ColdParticles, true);
                    EnableEmission(HeatParticles, false);
                    break;

                case TemperatureType.Hot:
                    EnableEmission(ColdParticles, false);
                    EnableEmission(HeatParticles, true);
                    break;

                case TemperatureType.Normal:
                default:
                    EnableEmission(ColdParticles, false);
                    EnableEmission(HeatParticles, false);
                    break;
            }
        }

        private void EnableEmission(ParticleSystem ps, bool enable)
        {
            if (ps == null) return;

            var emission = ps.emission;
            emission.enabled = enable;

            if (enable && !ps.isPlaying)
            {
                ps.Play();
            }
        }
    }
}
