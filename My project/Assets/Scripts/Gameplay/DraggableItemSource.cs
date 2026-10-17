using LogiPack.Core;
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

        private void OnMouseDown()
        {
            if (Item == null || _mainCamera == null) return;

            _isDragging = true;

            // Crear un objeto visual temporal para el arrastre
            _dragInstance = new GameObject($"Dragging_{Item.ItemName}");
            var sr = _dragInstance.AddComponent<SpriteRenderer>();
            sr.sprite = Item.Icon != null ? Item.Icon : (VisualRenderer != null ? VisualRenderer.sprite : null);
            sr.sortingOrder = 50; // Visible por encima de todo
            _dragInstance.transform.localScale = transform.localScale * 1.15f; // Feedback visual

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

            if (_dragInstance != null)
            {
                Vector3 dropPos = _dragInstance.transform.position;
                Destroy(_dragInstance);

                // Detectar si se soltó sobre una caja de empaque (hitbox tolerante)
                Collider2D hit = Physics2D.OverlapCircle(dropPos, 1.2f);
                if (hit != null)
                {
                    PackageBox targetBox = hit.GetComponent<PackageBox>();
                    if (targetBox != null && targetBox.State == BoxState.Assembling)
                    {
                        targetBox.AddItem(Item);
                    }
                }
            }
        }

        private void UpdateDragPosition()
        {
            Vector2 screenPos = GetPointerScreenPosition();
            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            mouseWorld.z = 0f;
            _dragInstance.transform.position = mouseWorld;
        }
    }
}
