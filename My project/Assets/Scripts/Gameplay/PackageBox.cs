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

        [Header("Sprites de Caja")]
        public Sprite OpenBoxSprite;
        public Sprite ClosedBoxSprite;
        private SpriteRenderer _baseSR;

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
            _baseSR = GetComponent<SpriteRenderer>();

            if (ItemContainer == null)
            {
                GameObject containerGO = new GameObject("BoxItemIcons");
                containerGO.transform.parent = transform;
                containerGO.transform.localPosition = Vector3.zero;
                ItemContainer = containerGO.transform;
            }

            if (OpenBoxSprite != null && _baseSR != null && State == BoxState.Assembling)
            {
                _baseSR.sprite = OpenBoxSprite;
            }

            EnsureVisualOverlays();
            UpdateVisualPackaging();
        }

        public void SetState(BoxState newState)
        {
            if (State == newState) return;
            State = newState;

            if (_baseSR == null) _baseSR = GetComponent<SpriteRenderer>();

            if (State == BoxState.InPackagingSlot || State == BoxState.Dispatched)
            {
                if (ClosedBoxSprite != null && _baseSR != null)
                {
                    _baseSR.sprite = ClosedBoxSprite;
                }
                if (ItemContainer != null)
                {
                    ItemContainer.gameObject.SetActive(false);
                }
                if (FoamOverlay != null && ClosedBoxSprite != null)
                {
                    var sr = FoamOverlay.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = ClosedBoxSprite;
                }
                if (ThermalFilmOverlay != null && ClosedBoxSprite != null)
                {
                    var sr = ThermalFilmOverlay.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = ClosedBoxSprite;
                }
            }
            else if (State == BoxState.Assembling)
            {
                if (OpenBoxSprite != null && _baseSR != null)
                {
                    _baseSR.sprite = OpenBoxSprite;
                }
                if (ItemContainer != null)
                {
                    ItemContainer.gameObject.SetActive(true);
                }
                if (FoamOverlay != null && OpenBoxSprite != null)
                {
                    var sr = FoamOverlay.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = OpenBoxSprite;
                }
                if (ThermalFilmOverlay != null && OpenBoxSprite != null)
                {
                    var sr = ThermalFilmOverlay.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = OpenBoxSprite;
                }
            }

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

        public const int MAX_CAPACITY = 5;

        public void AddItem(ItemData item)
        {
            if (item == null) return;

            if (State != BoxState.Assembling)
            {
                Debug.LogWarning("[PackageBox] No se pueden agregar items a una caja que no esté en la mesa de armado.");
                return;
            }

            // Límite estricto de 5 items máximo por caja (por instancia)
            if (GetTotalItemCount() >= MAX_CAPACITY)
            {
                Debug.LogWarning($"<color=yellow><b>[PackageBox]</b></color> Capacidad máxima alcanzada ({MAX_CAPACITY} items). No se admiten más items en esta caja.");
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
            Debug.Log($"<color=cyan><b>[PackageBox]</b></color> Se añadió 1x <b>{item.ItemName}</b> a la caja ({GetTotalItemCount()}/{MAX_CAPACITY}).");
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

        private SpriteRenderer[] _feedbackSlots;

        private void EnsureFeedbackSlots()
        {
            if (ItemContainer == null)
            {
                var existing = transform.Find("BoxItemIcons");
                if (existing != null)
                {
                    ItemContainer = existing;
                }
                else
                {
                    GameObject containerGO = new GameObject("BoxItemIcons");
                    containerGO.transform.parent = transform;
                    containerGO.transform.localPosition = Vector3.zero;
                    ItemContainer = containerGO.transform;
                }
            }

            if (_feedbackSlots != null && _feedbackSlots.Length == MAX_CAPACITY)
            {
                bool allValid = true;
                for (int i = 0; i < MAX_CAPACITY; i++)
                {
                    if (_feedbackSlots[i] == null) { allValid = false; break; }
                }
                if (allValid) return;
            }

            _feedbackSlots = new SpriteRenderer[MAX_CAPACITY];
            for (int i = 0; i < MAX_CAPACITY; i++)
            {
                string slotName = $"Slot_{i}";
                Transform existingSlot = ItemContainer.Find(slotName);
                GameObject slotGO;
                SpriteRenderer sr;

                if (existingSlot != null)
                {
                    slotGO = existingSlot.gameObject;
                    sr = slotGO.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = slotGO.AddComponent<SpriteRenderer>();
                }
                else
                {
                    slotGO = new GameObject(slotName);
                    slotGO.transform.parent = ItemContainer;
                    slotGO.transform.localScale = Vector3.one * 0.038f;
                    sr = slotGO.AddComponent<SpriteRenderer>();
                }

                sr.sortingOrder = 10;
                slotGO.SetActive(false);
                _feedbackSlots[i] = sr;
            }
        }

        private void UpdateVisualContents()
        {
            EnsureFeedbackSlots();
            if (_feedbackSlots == null || ItemContainer == null) return;

            var itemsList = new List<ItemData>();
            foreach (var kvp in Contents)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    itemsList.Add(kvp.Key);
                }
            }

            int count = Mathf.Min(itemsList.Count, MAX_CAPACITY);

            if (count == 0)
            {
                for (int i = 0; i < MAX_CAPACITY; i++)
                {
                    if (_feedbackSlots[i] != null)
                        _feedbackSlots[i].gameObject.SetActive(false);
                }
                return;
            }

            // Malla horizontal de feedback visual centrada sobre la abertura de la caja
            // Espaciado uniforme para hasta 5 casillas fijas sin solapamiento ni clones fantasma
            float slotSpacing = 0.35f;
            float startX = -((count - 1) * slotSpacing) * 0.5f;

            for (int i = 0; i < MAX_CAPACITY; i++)
            {
                if (i < count)
                {
                    var slot = _feedbackSlots[i];
                    slot.gameObject.SetActive(true);
                    slot.sprite = itemsList[i].Icon;
                    slot.sortingOrder = 10;

                    float xPos = startX + (i * slotSpacing);
                    slot.transform.localPosition = new Vector3(xPos, 0.65f, -0.1f);
                    slot.transform.localScale = Vector3.one * 0.038f;
                }
                else
                {
                    if (_feedbackSlots[i] != null)
                        _feedbackSlots[i].gameObject.SetActive(false);
                }
            }
        }

        public bool IsEmpty => Contents.Count == 0;
    }
}
