using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Tutorial
{
    /// <summary>
    /// Indicador visual flotante y animado en World Space 2D (Decisión 14).
    /// Resalta cajones, ranuras, botones de operadores y la cinta de despacho
    /// con una flecha o marco pulsante que se actualiza en tiempo real desescalado.
    /// </summary>
    public class TutorialHighlightFX : MonoBehaviour
    {
        public static TutorialHighlightFX Instance { get; private set; }

        [Header("Configuración Visual")]
        public Sprite ArrowSprite;
        public Color HighlightColor = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo dorado brillante
        public float BounceAmplitude = 0.0f; // Estático sin rebote según especificación
        public float BounceSpeed = 0.0f;

        private Transform _targetTransform;
        private Vector3 _targetWorldPos;
        private Vector3 _offset = Vector3.zero;
        private bool _isFollowingTransform = false;
        private bool _isActive = false;

        private GameObject _visualContainer;
        private SpriteRenderer _sr;
        private Text _labelText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildVisuals();
            Hide();
        }

        private void BuildVisuals()
        {
            if (_visualContainer != null) return;

            _visualContainer = new GameObject("HighlightVisual");
            _visualContainer.transform.SetParent(transform, false);

            _sr = _visualContainer.AddComponent<SpriteRenderer>();
            _sr.color = HighlightColor;
            _sr.sortingOrder = 100; // Al frente de toda la mesa

            if (ArrowSprite != null)
            {
                _sr.sprite = ArrowSprite;
            }
            else
            {
                // Buscar selector_cuadrado o crear un icono de flecha procedural
                var selectorSp = Resources.Load<Sprite>("selector_cuadrado");
                if (selectorSp != null) _sr.sprite = selectorSp;
            }

            // Canvas flotante para texto explicativo corto si se requiere
            var canvasGO = new GameObject("LabelCanvas");
            canvasGO.transform.SetParent(_visualContainer.transform, false);
            canvasGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 101;

            var rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240, 50);
            rt.localScale = Vector3.one * 0.015f;

            _labelText = canvasGO.AddComponent<Text>();
            _labelText.alignment = TextAnchor.MiddleCenter;
            _labelText.fontSize = 28;
            _labelText.color = Color.white;
            Font fredoka = Resources.Load<Font>("Fredoka-Bold");
            if (fredoka != null) _labelText.font = fredoka;
        }

        public void PointAt(Transform target, Vector3 offset, string label = "")
        {
            _targetTransform = target;
            _offset = offset;
            _isFollowingTransform = true;
            _isActive = true;

            if (_labelText != null)
            {
                _labelText.text = label;
                _labelText.gameObject.SetActive(!string.IsNullOrEmpty(label));
            }

            if (_visualContainer != null) _visualContainer.SetActive(true);
            UpdatePosition();
        }

        public void PointAt(Vector3 worldPos, Vector3 offset, string label = "")
        {
            _targetWorldPos = worldPos;
            _offset = offset;
            _isFollowingTransform = false;
            _isActive = true;

            if (_labelText != null)
            {
                _labelText.text = label;
                _labelText.gameObject.SetActive(!string.IsNullOrEmpty(label));
            }

            if (_visualContainer != null) _visualContainer.SetActive(true);
            UpdatePosition();
        }

        public void Hide()
        {
            _isActive = false;
            _targetTransform = null;
            if (_visualContainer != null)
            {
                _visualContainer.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_isActive) return;
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_visualContainer == null) return;

            Vector3 basePos = _isFollowingTransform && _targetTransform != null ? _targetTransform.position : _targetWorldPos;
            // Animación de rebote suave en unscaledTime para que funcione aún si el tiempo está en 0
            float bounce = Mathf.Sin(Time.unscaledTime * BounceSpeed) * BounceAmplitude;
            _visualContainer.transform.position = basePos + _offset + new Vector3(0f, bounce, 0f);
        }
    }
}
