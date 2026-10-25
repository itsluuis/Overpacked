using LogiPack.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.Gameplay
{
    public class OperatorSlotTrigger : MonoBehaviour
    {
        public AssemblyWorkbenchManager Workbench;

        private Camera _mainCamera;
        private Collider2D _collider;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _collider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                if (_mainCamera == null) _mainCamera = Camera.main;
                if (_mainCamera == null) return;

                Vector2 screenPos = Pointer.current.position.ReadValue();
                Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
                worldPos.z = 0f;

                if (_collider != null && _collider.OverlapPoint(worldPos))
                {
                    if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                    if (Workbench == null) Workbench = AssemblyWorkbenchManager.Instance;
                    if (Workbench != null && Workbench.IsInteractionsBlocked) return;
                    if (Workbench != null)
                    {
                        Workbench.ToggleOperatorDrawer();
                    }
                }
            }
        }
    }
}
