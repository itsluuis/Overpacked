using UnityEngine;
using UnityEngine.InputSystem;

namespace LogiPack.UI
{
    /// <summary>
    /// Gestiona el cursor visual de Overpacked utilizando el Cozy UI Pack.
    /// Sustituye dinámicamente el puntero del sistema operativo por Cursor1 en reposo
    /// y Cursor1Clicked al presionar o arrastrar elementos.
    /// </summary>
    public class GameCursorManager : MonoBehaviour
    {
        public static GameCursorManager Instance { get; private set; }

        [Header("Texturas del Cursor Cozy UI")]
        [SerializeField] private Texture2D _defaultCursor;
        [SerializeField] private Texture2D _clickedCursor;

        [Header("Configuración de Hotspot")]
        [SerializeField] private Vector2 _hotspot = Vector2.zero;
        [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;

        private bool _isPressed = false;

        public Texture2D DefaultCursor { get => _defaultCursor; set => _defaultCursor = value; }
        public Texture2D ClickedCursor { get => _clickedCursor; set => _clickedCursor = value; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyCursor(false);
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;

            bool currentlyPressed = pointer.press.isPressed;
            if (currentlyPressed != _isPressed)
            {
                _isPressed = currentlyPressed;
                ApplyCursor(_isPressed);
            }
        }

        private void ApplyCursor(bool pressed)
        {
            Texture2D targetTexture = pressed ? (_clickedCursor != null ? _clickedCursor : _defaultCursor) : _defaultCursor;
            if (targetTexture != null)
            {
                Cursor.SetCursor(targetTexture, _hotspot, _cursorMode);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ApplyCursor(_isPressed);
            }
        }

        private void OnDisable()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }
}
