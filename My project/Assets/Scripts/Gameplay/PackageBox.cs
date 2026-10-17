using System;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Estados posibles en el ciclo de vida de la caja.
    /// </summary>
    public enum BoxState
    {
        Assembling,        // En mesa de armado: acepta items y cambio de embalaje
        InPackagingSlot,   // En ranura de embalaje: bloqueada/sellada
        Dispatched         // Despachada
    }

    /// <summary>
    /// Materiales de embalaje adicionales soportados por la caja.
    /// Usa [System.Flags] para permitir evaluar los 4 cuadrantes lógicos (V/V, V/F, F/V, F/F).
    /// </summary>
    [Flags]
    public enum PackagingMaterial
    {
        None = 0,
        Foam = 1 << 0,         // Proposición r: Relleno de Espuma
        ThermalFilm = 1 << 1   // Proposición s: Film Térmico
    }

    /// <summary>
    /// Representa una caja en la que se acumulan items arrastrados por el jugador.
    /// Incluye feedback visual de los items acumulados y overlays del material de embalaje.
    /// </summary>
    public class PackageBox : MonoBehaviour
    {
        public readonly Dictionary<ItemData, int> Contents = new Dictionary<ItemData, int>();
        public int SlotIndex = -1; // -1 si está en armado, 0 a 4 si está en embalaje

        [Header("Estado y Embalaje")]
        public BoxState State = BoxState.Assembling;
        public PackagingMaterial Material = PackagingMaterial.None;

        [Header("Contenedores Visuales")]
        public Transform ItemContainer;
        public GameObject FoamOverlay;
        public GameObject ThermalFilmOverlay;

        public event Action OnContentsChanged;
        public event Action OnPackagingChanged;
        public event Action<BoxState> OnStateChanged;

        public bool HasFoamPadding => (Material & PackagingMaterial.Foam) != 0;
        public bool HasThermalFilm => (Material & PackagingMaterial.ThermalFilm) != 0;

        private void Awake()
        {
            if (ItemContainer == null)
            {
                GameObject containerGO = new GameObject("BoxItemIcons");
                containerGO.transform.parent = transform;
                containerGO.transform.localPosition = Vector3.zero;
                ItemContainer = containerGO.transform;
            }

            EnsureVisualOverlays();
            UpdateVisualPackaging();
        }

        public void SetState(BoxState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(State);
            Debug.Log($"<color=orange><b>[PackageBox]</b></color> Caja cambió de estado a: <b>{State}</b>");
        }

        public void ToggleFoam()
        {
            if (State != BoxState.Assembling)
            {
                Debug.LogWarning("[PackageBox] No se puede modificar el embalaje de una caja fuera de la mesa de armado.");
                return;
            }

            if (HasFoamPadding)
            {
                Material &= ~PackagingMaterial.Foam;
            }
            else
            {
                Material |= PackagingMaterial.Foam;
            }

            UpdateVisualPackaging();
            OnPackagingChanged?.Invoke();
            Debug.Log($"<color=yellow><b>[PackageBox]</b></color> Espuma: {HasFoamPadding}. Embalaje actual: {Material}");
        }

        public void ToggleThermalFilm()
        {
            if (State != BoxState.Assembling)
            {
                Debug.LogWarning("[PackageBox] No se puede modificar el embalaje de una caja fuera de la mesa de armado.");
                return;
            }

            if (HasThermalFilm)
            {
                Material &= ~PackagingMaterial.ThermalFilm;
            }
            else
            {
                Material |= PackagingMaterial.ThermalFilm;
            }

            UpdateVisualPackaging();
            OnPackagingChanged?.Invoke();
            Debug.Log($"<color=cyan><b>[PackageBox]</b></color> Film Térmico: {HasThermalFilm}. Embalaje actual: {Material}");
        }

        public void ResetPackaging()
        {
            Material = PackagingMaterial.None;
            UpdateVisualPackaging();
            OnPackagingChanged?.Invoke();
        }

        public void AddItem(ItemData item)
        {
            if (item == null) return;

            if (State != BoxState.Assembling)
            {
                Debug.LogWarning("[PackageBox] No se pueden agregar items a una caja que no esté en la mesa de armado.");
                return;
            }

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

        private void EnsureVisualOverlays()
        {
            var baseSR = GetComponent<SpriteRenderer>();
            Sprite boxSprite = baseSR != null ? baseSR.sprite : null;

            // 1. Overlay de Espuma (Amarillo suave acolchado)
            if (FoamOverlay == null)
            {
                var existing = transform.Find("FoamOverlay");
                if (existing != null)
                {
                    FoamOverlay = existing.gameObject;
                }
                else
                {
                    FoamOverlay = new GameObject("FoamOverlay");
                    FoamOverlay.transform.parent = transform;
                    FoamOverlay.transform.localPosition = new Vector3(0, 0, -0.05f);
                    FoamOverlay.transform.localScale = Vector3.one * 1.05f;

                    var sr = FoamOverlay.AddComponent<SpriteRenderer>();
                    sr.sprite = boxSprite;
                    sr.color = new Color(1.0f, 0.92f, 0.35f, 0.55f); // Amarillo acolchado
                    sr.sortingOrder = 5;
                }
            }

            // 2. Overlay de Film Térmico (Cian brillante)
            if (ThermalFilmOverlay == null)
            {
                var existing = transform.Find("ThermalFilmOverlay");
                if (existing != null)
                {
                    ThermalFilmOverlay = existing.gameObject;
                }
                else
                {
                    ThermalFilmOverlay = new GameObject("ThermalFilmOverlay");
                    ThermalFilmOverlay.transform.parent = transform;
                    ThermalFilmOverlay.transform.localPosition = new Vector3(0, 0, -0.08f);
                    ThermalFilmOverlay.transform.localScale = Vector3.one * 1.10f;

                    var sr = ThermalFilmOverlay.AddComponent<SpriteRenderer>();
                    sr.sprite = boxSprite;
                    sr.color = new Color(0.25f, 0.85f, 1.0f, 0.50f); // Azul/Cian brillante
                    sr.sortingOrder = 6;
                }
            }
        }

        private void UpdateVisualPackaging()
        {
            EnsureVisualOverlays();

            if (FoamOverlay != null)
            {
                FoamOverlay.SetActive(HasFoamPadding);
            }

            if (ThermalFilmOverlay != null)
            {
                ThermalFilmOverlay.SetActive(HasThermalFilm);
            }
        }

        private void UpdateVisualContents()
        {
            if (ItemContainer == null) return;

            foreach (Transform child in ItemContainer)
            {
                Destroy(child.gameObject);
            }

            int index = 0;
            foreach (var kvp in Contents)
            {
                ItemData item = kvp.Key;
                int count = kvp.Value;

                for (int i = 0; i < count; i++)
                {
                    GameObject iconGO = new GameObject($"Icon_{item.ItemName}_{i}");
                    iconGO.transform.parent = ItemContainer;

                    float xOffset = ((index % 3) - 1f) * 0.45f;
                    float yOffset = ((index / 3) * 0.45f) - 0.2f;
                    iconGO.transform.localPosition = new Vector3(xOffset, yOffset, -0.1f);
                    iconGO.transform.localScale = Vector3.one * 0.4f;

                    var sr = iconGO.AddComponent<SpriteRenderer>();
                    sr.sprite = item.Icon;
                    sr.sortingOrder = 10;

                    index++;
                }
            }
        }

        public bool IsEmpty => Contents.Count == 0;
    }
}
