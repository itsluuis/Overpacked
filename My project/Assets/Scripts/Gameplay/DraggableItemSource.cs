using LogiPack.Core;
using LogiPack.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Componente que permite a un item de la estantería ser arrastrado mediante Drag & Drop
    /// con un clon visual hacia la caja de empaque.
    /// Compatible con el nuevo Input System de Unity 6.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DraggableItemSource : MonoBehaviour
    {
        [Header("Datos del Item")]
        public ItemData Item;

        [Header("Configuración Visual")]
        public SpriteRenderer VisualRenderer;

        private GameObject _dragInstance;
        private Camera _mainCamera;
        private bool _isDragging;

        private void Start()
        {
            _mainCamera = Camera.main;

            if (VisualRenderer == null)
            {
                VisualRenderer = GetComponent<SpriteRenderer>();
            }

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
            if (Item == null) return;
            if (_mainCamera == null) _mainCamera = Camera.main;

            _isDragging = true;

            // Crear un objeto visual temporal para el arrastre
            _dragInstance = new GameObject($"Dragging_{Item.ItemName}");
            var sr = _dragInstance.AddComponent<SpriteRenderer>();
            sr.sprite = Item.Icon != null ? Item.Icon : (VisualRenderer != null ? VisualRenderer.sprite : null);
            sr.sortingOrder = 50; // Visible por encima de todo
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

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;

            Vector3 dropPos = GetMouseWorldPosition();

            if (_dragInstance != null)
            {
                Destroy(_dragInstance);
            }

            // 1. Prioridad: Verificar proximidad directa con la caja activa en mesa de armado
            PackageBox targetBox = null;
            if (BoxSpawner.Instance != null && BoxSpawner.Instance.CurrentActiveBox != null)
            {
                var activeBox = BoxSpawner.Instance.CurrentActiveBox;
                if (activeBox.State == BoxState.Assembling)
                {
                    float dist = Vector2.Distance(dropPos, activeBox.transform.position);
                    if (dist <= 2.2f)
                    {
                        targetBox = activeBox;
                    }
                }
            }

            // 2. Respaldo: OverlapCircle físico
            if (targetBox == null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(dropPos, 2.0f);
                foreach (var h in hits)
                {
                    var box = h.GetComponentInParent<PackageBox>();
                    if (box != null && box.State == BoxState.Assembling)
                    {
                        targetBox = box;
                        break;
                    }
                }
            }

            if (targetBox != null)
            {
                targetBox.AddItem(Item);
            }
        }

        private void UpdateDragPosition()
        {
            if (_dragInstance != null)
            {
                _dragInstance.transform.position = GetMouseWorldPosition();
            }
        }
    }
}
