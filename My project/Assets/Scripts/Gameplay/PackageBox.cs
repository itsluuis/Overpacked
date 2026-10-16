using System;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Representa una caja en la que se acumulan items arrastrados por el jugador.
    /// Incluye feedback visual de los items acumulados dentro de la caja.
    /// </summary>
    public class PackageBox : MonoBehaviour
    {
        public readonly Dictionary<ItemData, int> Contents = new Dictionary<ItemData, int>();
        public int SlotIndex = -1; // -1 si está en armado, 0 a 4 si está en embalaje

        [Header("Contenedor Visual de Items")]
        public Transform ItemContainer;

        public event Action OnContentsChanged;

        private void Awake()
        {
            if (ItemContainer == null)
            {
                // Crear un sub-objeto para alojar los mini-sprites de los items dentro
                GameObject containerGO = new GameObject("BoxItemIcons");
                containerGO.transform.parent = transform;
                containerGO.transform.localPosition = Vector3.zero;
                ItemContainer = containerGO.transform;
            }
        }

        public void AddItem(ItemData item)
        {
            if (item == null) return;

            if (Contents.ContainsKey(item))
            {
                Contents[item]++;
            }
            else
            {
                Contents[item] = 1;
            }

            UpdateVisualContents();
            OnContentsChanged?.Invoke();
            Debug.Log($"<color=cyan><b>[PackageBox]</b></color> Se añadió 1x <b>{item.ItemName}</b> a la caja. Total en caja: {GetTotalItemCount()} items.");
        }

        public void ClearBox()
        {
            Contents.Clear();
            UpdateVisualContents();
            OnContentsChanged?.Invoke();
        }

        public int GetTotalItemCount()
        {
            int total = 0;
            foreach (var kvp in Contents) total += kvp.Value;
            return total;
        }

        private void UpdateVisualContents()
        {
            if (ItemContainer == null) return;

            // Limpiar iconos previos
            foreach (Transform child in ItemContainer)
            {
                Destroy(child.gameObject);
            }

            // Mostrar miniaturas de los items acumulados dentro de la caja
            int index = 0;
            foreach (var kvp in Contents)
            {
                ItemData item = kvp.Key;
                int count = kvp.Value;

                for (int i = 0; i < count; i++)
                {
                    GameObject iconGO = new GameObject($"Icon_{item.ItemName}_{i}");
                    iconGO.transform.parent = ItemContainer;

                    // Posicionar en una cuadrícula compacta sobre la caja
                    float xOffset = ((index % 3) - 1f) * 0.45f;
                    float yOffset = ((index / 3) * 0.45f) - 0.2f;
                    iconGO.transform.localPosition = new Vector3(xOffset, yOffset, -0.1f);
                    iconGO.transform.localScale = Vector3.one * 0.4f;

                    var sr = iconGO.AddComponent<SpriteRenderer>();
                    sr.sprite = item.Icon;
                    sr.sortingOrder = 10; // Encima de la caja

                    index++;
                }
            }
        }

        public bool IsEmpty => Contents.Count == 0;
    }
}
