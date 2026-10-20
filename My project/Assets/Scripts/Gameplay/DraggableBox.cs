using LogiPack.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Permite arrastrar la caja en cualquier momento:
    /// - Desde la mesa hacia una de las 5 ranuras de embalaje o la basura.
    /// - Desde una ranura de embalaje hacia otra ranura o hacia la basura.
    /// - Garantiza la creación continua de cajas vacías mediante BoxSpawner.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DraggableBox : MonoBehaviour
    {
        public BoxSpawner Spawner;

        private Camera _mainCamera;
        private Vector3 _startDragPos;
        private Vector3 _baseScale = Vector3.one * 0.20f;
        private bool _isDragging;
        private PackageBox _box;

        private void Start()
        {
            _mainCamera = Camera.main;
            _box = GetComponent<PackageBox>();
            _baseScale = transform.localScale;

            // Si Spawner no está asignado, buscarlo automáticamente en la escena
            if (Spawner == null)
            {
                Spawner = FindAnyObjectByType<BoxSpawner>();
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
            if (_mainCamera == null) return;
            _isDragging = true;
            _startDragPos = transform.position; // Guardar posición actual
            if (_baseScale.sqrMagnitude < 0.001f) _baseScale = transform.localScale;
            transform.localScale = _baseScale * 1.02f; // Feedback visual sutil sin deformar
        }

        private void OnMouseDrag()
        {
            if (_isDragging && _mainCamera != null)
            {
                Vector2 screenPos = GetPointerScreenPosition();
                Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                mouseWorld.z = 0f;
                transform.position = mouseWorld;
            }
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;
            transform.localScale = _baseScale;

            int previousSlot = _box.SlotIndex; // -1 si estaba en la mesa, 0-4 si estaba en embalaje

            // Detectar si se soltó sobre una ranura de embalaje o sobre el basurero
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(2.5f, 2.5f), 0f);
            bool actionHandled = false;

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                // 1. ¿Es el bote de basura?
                if (hit.name == "TrashBin" || hit.CompareTag("TrashBin"))
                {
                    Debug.Log("<color=red><b>[DraggableBox]</b></color> Caja arrojada a la basura.");

                    // Si estaba en una ranura de embalaje, liberar la ranura
                    if (previousSlot >= 0)
                    {
                        var zone = FindAnyObjectByType<PackagingZoneManager>();
                        if (zone != null) zone.RemoveBoxFromSlot(previousSlot);
                    }
                    else
                    {
                        // Si estaba en la mesa de armado, solicitar una nueva caja
                        if (Spawner == null) Spawner = FindAnyObjectByType<BoxSpawner>();
                        if (Spawner != null) Spawner.NotifyBoxMovedFromTable();
                    }

                    Destroy(gameObject);
                    return;
                }

                // 2. ¿Es un slot de embalaje?
                PackagingSlot slot = hit.GetComponent<PackagingSlot>();
                if (slot != null && slot.ZoneManager != null)
                {
                    // Si ya estaba en otra ranura, liberar la anterior
                    if (previousSlot >= 0 && previousSlot != slot.SlotIndex)
                    {
                        slot.ZoneManager.RemoveBoxFromSlot(previousSlot);
                    }

                    if (slot.ZoneManager.PlaceBoxInSlot(_box, slot.SlotIndex))
                    {
                        actionHandled = true;

                        // Si vino de la mesa, solicitar una nueva caja vacía para la mesa
                        if (previousSlot == -1)
                        {
                            if (Spawner == null) Spawner = FindAnyObjectByType<BoxSpawner>();
                            if (Spawner != null) Spawner.NotifyBoxMovedFromTable();
                        }
                        break;
                    }
                }
            }

            // Si no se soltó en un destino válido, regresar a la posición previa
            if (!actionHandled)
            {
                transform.position = _startDragPos;
            }
        }
    }
}
