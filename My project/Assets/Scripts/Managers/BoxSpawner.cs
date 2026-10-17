using LogiPack.Gameplay;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Gestiona la creación continua de cajas vacías en la mesa de armado.
    /// Si una caja se traslada a la zona de embalaje o se tira a la basura,
    /// genera inmediatamente una nueva caja vacía para no interrumpir el flujo.
    /// </summary>
    public class BoxSpawner : MonoBehaviour
    {
        [Header("Configuración de Spawn")]
        public GameObject BoxPrefab;
        public Transform SpawnPoint;

        public static BoxSpawner Instance { get; private set; }

        public PackageBox CurrentActiveBox { get; private set; }

        public event System.Action<PackageBox> OnBoxSpawned;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SpawnNewBox();
        }

        public void SpawnNewBox()
        {
            if (BoxPrefab == null || SpawnPoint == null) return;

            GameObject newBoxGO = Instantiate(BoxPrefab, SpawnPoint.position, Quaternion.identity);
            newBoxGO.name = "CurrentBox";

            CurrentActiveBox = newBoxGO.GetComponent<PackageBox>();
            var draggable = newBoxGO.GetComponent<DraggableBox>();
            if (draggable != null)
            {
                draggable.Spawner = this;
            }

            OnBoxSpawned?.Invoke(CurrentActiveBox);
        }

        /// <summary>
        /// Llamado cuando la caja activa actual sale de la mesa de armado
        /// (se colocó en un slot de embalaje o fue arrojada a la basura).
        /// </summary>
        public void NotifyBoxMovedFromTable()
        {
            CurrentActiveBox = null;
            // Spawnear una nueva caja en la mesa para continuar armando
            SpawnNewBox();
        }
    }
}
