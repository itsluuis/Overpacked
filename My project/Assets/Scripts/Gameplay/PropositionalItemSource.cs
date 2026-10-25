using LogiPack.Core;
using LogiPack.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PropositionalItemSource : MonoBehaviour
    {
        [Header("Definición Lógica")]
        public string VariableName = "p"; // "p", "q", "s", "r"
        public bool IsNegated = false;     // true si es ~p, etc.
        public ItemData Item;

        [Header("Visuales")]
        public SpriteRenderer VisualRenderer;

        private GameObject _dragInstance;
        private Camera _mainCamera;
        private bool _isDragging;

        private void Start()
        {
            _mainCamera = Camera.main;
            if (VisualRenderer == null) VisualRenderer = GetComponent<SpriteRenderer>();

            if (Item != null && VisualRenderer != null && Item.Icon != null)
            {
                VisualRenderer.sprite = Item.Icon;
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

        private Vector3 GetMouseWorldPosition()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            Vector2 screenPos = GetPointerScreenPosition();
            Vector3 world = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            world.z = 0f;
            return world;
        }

        private void OnMouseDown()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            var wb = FindAnyObjectByType<AssemblyWorkbenchManager>();
            if (wb != null && wb.IsInteractionsBlocked) return;

            if (_mainCamera == null) _mainCamera = Camera.main;
            _isDragging = true;

            _dragInstance = new GameObject($"Dragging_{VariableName}_{(IsNegated ? "Neg" : "Pos")}");
            var sr = _dragInstance.AddComponent<SpriteRenderer>();
            sr.sprite = (VisualRenderer != null && VisualRenderer.sprite != null) ? VisualRenderer.sprite : (Item != null ? Item.Icon : null);
            sr.sortingOrder = 60;
            _dragInstance.transform.localScale = transform.lossyScale;

            UpdateDragPosition();
        }

        private void OnMouseDrag()
        {
            if (_isDragging && _dragInstance != null)
            {
                UpdateDragPosition();
            }
        }

        private void UpdateDragPosition()
        {
            if (_dragInstance != null)
            {
                _dragInstance.transform.position = GetMouseWorldPosition();
            }
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;

            Vector3 dropPos = GetMouseWorldPosition();
            if (_dragInstance != null)
            {
                Destroy(_dragInstance);
            }

            // Buscar si se soltó sobre una de las ranuras de la zona 1x3
            var workbench = FindAnyObjectByType<AssemblyWorkbenchManager>();
            if (workbench != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(dropPos, 0.9f);
                foreach (var hit in hits)
                {
                    if (hit.name.Contains("Ranura_Armado_1") || hit.name.Contains("Slot_1"))
                    {
                        var node = PropositionalNode.CreateAtomic(VariableName, IsNegated, Item);
                        workbench.SetOperand(0, node, (VisualRenderer != null ? VisualRenderer.sprite : null));
                        return;
                    }
                    else if (hit.name.Contains("Ranura_Armado_3") || hit.name.Contains("Slot_3"))
                    {
                        var node = PropositionalNode.CreateAtomic(VariableName, IsNegated, Item);
                        workbench.SetOperand(2, node, (VisualRenderer != null ? VisualRenderer.sprite : null));
                        return;
                    }
                }
            }
        }
    }
}
