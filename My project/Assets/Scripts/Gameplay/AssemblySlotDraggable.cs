using LogiPack.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Permite hacer clic y arrastrar un ítem/caja colocado en la ranura 1 o 3
    /// para retirarlo (soltándolo fuera) o moverlo a la otra ranura.
    /// </summary>
    public class AssemblySlotDraggable : MonoBehaviour
    {
        public int SlotIndex; // 0 para Ranura 1, 2 para Ranura 3
        public AssemblyWorkbenchManager Workbench;

        private Camera _mainCamera;
        private Vector3 _originLocalPos;
        private bool _isDragging;

        private void Start()
        {
            _mainCamera = Camera.main;
            _originLocalPos = transform.localPosition;
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
            if (_mainCamera == null) _mainCamera = Camera.main;
            _isDragging = true;
        }

        private void OnMouseDrag()
        {
            if (_isDragging)
            {
                transform.position = GetMouseWorldPosition();
            }
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (Workbench == null)
            {
                transform.localPosition = _originLocalPos;
                return;
            }

            Vector3 dropPos = transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(dropPos, 0.7f);
            bool droppedInAnotherSlot = false;

            foreach (var hit in hits)
            {
                int otherIndex = (SlotIndex == 0) ? 2 : 0;
                string targetName = (otherIndex == 0) ? "Ranura_Armado_1" : "Ranura_Armado_3";

                if (hit.name.Contains(targetName))
                {
                    Workbench.SwapOrMoveOperand(SlotIndex, otherIndex);
                    droppedInAnotherSlot = true;
                    break;
                }
            }

            if (!droppedInAnotherSlot)
            {
                // Si se soltó fuera de las dos ranuras de armado, se libera la ranura
                Workbench.ClearOperand(SlotIndex);
            }
        }
    }
}
