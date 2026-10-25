using System;
using LogiPack.Logic;
using LogiPack.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LogiPack.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public class PropositionalBox : MonoBehaviour
    {
        [Header("Datos Lógicos")]
        [System.NonSerialized] public PropositionalNode FormulaNode;
        public int NestingDepth = 1; // Máximo 3 niveles según diseño

        [Header("Referencias Visuales")]
        public Text FormulaLabel;
        public Sprite DefaultBoxSprite;
        [Header("Interior de la Caja (Pool de Ítems)")]
        public Transform ItemContainer;
        private SpriteRenderer[] _feedbackSlots;
        private const int MAX_SLOTS = 4;

        private Camera _mainCamera;
        private SpriteRenderer _sr;
        private Vector3 _startPosition;
        private Vector3 _baseScale;
        private bool _isDragging;

        public event Action<PropositionalBox> OnBoxDestroyed;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            if (_baseScale.sqrMagnitude < 0.001f) _baseScale = Vector3.one * 0.11f;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _startPosition = transform.position;
            UpdateVisuals();
        }

        public void SetFormula(PropositionalNode node, int depth = 1)
        {
            FormulaNode = node;
            NestingDepth = depth;
            UpdateVisuals();
        }

        public void SetHomePosition(Vector3 pos)
        {
            _startPosition = pos;
            transform.position = pos;
        }

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
                    containerGO.transform.SetParent(transform, false);
                    containerGO.transform.localPosition = Vector3.zero;
                    ItemContainer = containerGO.transform;
                }
            }

            if (_feedbackSlots != null && _feedbackSlots.Length == MAX_SLOTS)
            {
                bool allValid = true;
                for (int i = 0; i < MAX_SLOTS; i++)
                {
                    if (_feedbackSlots[i] == null) { allValid = false; break; }
                }
                if (allValid) return;
            }

            _feedbackSlots = new SpriteRenderer[MAX_SLOTS];
            for (int i = 0; i < MAX_SLOTS; i++)
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
                    slotGO.transform.SetParent(ItemContainer, false);
                    sr = slotGO.AddComponent<SpriteRenderer>();
                }

                sr.sortingOrder = 18;
                slotGO.SetActive(false);
                _feedbackSlots[i] = sr;
            }
        }

        public void UpdateVisuals()
        {
            if (FormulaLabel != null)
            {
                FormulaLabel.text = FormulaNode != null ? FormulaNode.ToFormulaString() : "";
            }
            if (_sr != null && DefaultBoxSprite != null)
            {
                _sr.sprite = DefaultBoxSprite;
            }

            EnsureFeedbackSlots();
            if (_feedbackSlots == null || ItemContainer == null) return;

            if (FormulaNode == null)
            {
                for (int i = 0; i < MAX_SLOTS; i++)
                {
                    if (_feedbackSlots[i] != null) _feedbackSlots[i].gameObject.SetActive(false);
                }
                return;
            }

            var icons = new System.Collections.Generic.List<Sprite>();
            FormulaNode.CollectIcons(icons, DefaultBoxSprite);

            int count = Mathf.Min(icons.Count, MAX_SLOTS);
            float slotSpacing = 2.4f;
            float startX = -((count - 1) * slotSpacing) * 0.5f;

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (i < count)
                {
                    var slot = _feedbackSlots[i];
                    slot.gameObject.SetActive(true);
                    slot.sprite = icons[i];
                    slot.sortingOrder = 18;
                    float xPos = startX + (i * slotSpacing);
                    slot.transform.localPosition = new Vector3(xPos, 1.8f, -0.05f);
                    slot.transform.localScale = Vector3.one * 0.35f;
                }
                else
                {
                    if (_feedbackSlots[i] != null)
                        _feedbackSlots[i].gameObject.SetActive(false);
                }
            }
        }

        private Vector2 GetPointerScreenPosition()
        {
            if (Pointer.current != null)
            {
                return Pointer.current.position.ReadValue();
            }
            return Vector2.zero;
        }

        private void OnMouseDown()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            var workbench = AssemblyWorkbenchManager.Instance;
            if (workbench != null && workbench.IsInteractionsBlocked) return;

            if (_mainCamera == null) _mainCamera = Camera.main;
            _isDragging = true;
            _startPosition = transform.position;
            transform.localScale = _baseScale * 1.05f;
            if (_sr != null) _sr.sortingOrder = 30; // Al frente mientras se arrastra

            // La referencia a la caja se retiene hasta su consumo terminal (despacho, revisión o anidamiento)
        }

        private void OnMouseDrag()
        {
            if (!_isDragging || _mainCamera == null) return;
            Vector2 screenPos = GetPointerScreenPosition();
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0f;
            transform.position = worldPos;
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;
            transform.localScale = _baseScale;
            if (_sr != null) _sr.sortingOrder = 15;

            // Detectar zonas bajo la caja
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.8f);
            bool handled = false;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // 1. Zona de Despacho
                if (hit.name.Contains("Despacho"))
                {
                    var dispatcher = FindAnyObjectByType<Managers.PropositionalOrderManager>();
                    if (dispatcher != null)
                    {
                        bool valid = dispatcher.ValidateAndDispatch(this);
                        if (valid)
                        {
                            handled = true;
                            DestroyBox();
                            return;
                        }
                        else
                        {
                            // Inválido: se queda esperando o vuelve a su posición
                            handled = true;
                            ReturnToHome();
                            return;
                        }
                    }
                }

                // 2. Zona de Revisión / Descarte
                if (hit.name.Contains("Revision") || hit.name.Contains("Trash") || (hit.CompareTag("TrashBin")))
                {
                    handled = true;
                    FloatingFeedbackFX.Show(transform.position, "Caja revisada con éxito", new Color(0.2f, 0.85f, 0.5f));
                    DestroyBox();
                    return;
                }

                // 3. Ranuras de armado (Ranura 1 o Ranura 3)
                var workbench = FindAnyObjectByType<AssemblyWorkbenchManager>();
                if (workbench != null)
                {
                    if (hit.name.Contains("Ranura_Armado_1") || hit.name.Contains("Slot_1"))
                    {
                        handled = workbench.TryPlaceBoxInSlot(this, 0);
                        if (handled) return;
                    }
                    else if (hit.name.Contains("Ranura_Armado_3") || hit.name.Contains("Slot_3"))
                    {
                        handled = workbench.TryPlaceBoxInSlot(this, 2);
                        if (handled) return;
                    }
                }
            }

            if (!handled)
            {
                ReturnToHome();
            }
        }

        public void ReturnToHome()
        {
            transform.position = _startPosition;
        }

        public void DestroyBox()
        {
            OnBoxDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
