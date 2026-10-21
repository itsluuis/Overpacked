using System.Collections;
using LogiPack.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LogiPack.UI.Menu
{
    /// <summary>
    /// Controla la cinemática de apertura/cierre del cuaderno, la selección
    /// de modos de juego y el lanzamiento de la partida hacia GameplayScene.
    /// </summary>
    public class CuadernoMenuController : MonoBehaviour
    {
        [Header("Sprites de Animación")]
        public Image NotebookImage;
        public Sprite CuadernoCerrado;
        public Sprite[] AbrirFrames; // 4 frames: abrir_paso_1..4
        public Sprite CuadernoAbierto;

        [Header("Contenedor de Páginas Abiertas")]
        public GameObject OpenPagesContainer;

        [Header("Elementos de Selección (Página Izquierda)")]
        public Button TutorialButton;
        public Image TutorialHighlight;
        public Button InfiniteButton;
        public Image InfiniteHighlight;
        public Button BackButton;

        [Header("Elementos de Información (Página Derecha)")]
        public Text ModeTitleText;
        public Text ModeDescriptionText;
        public Button StartButton;

        [Header("Referencia al Controlador del Menú")]
        public MainMenuController MainMenu;

        private RectTransform _rectTransform;
        private Vector2 _closedPosition = new Vector2(460f, -20f);
        private Vector2 _openPosition = new Vector2(0f, 0f);
        private Vector3 _closedScale = new Vector3(0.84f, 0.84f, 1f);
        private Vector3 _openScale = new Vector3(1.25f, 1.25f, 1f);

        private bool _isOpen = false;
        private bool _isAnimating = false;
        private GameSessionMode _selectedMode = GameSessionMode.Tutorial;

        public const string TUTORIAL_DESC = "Tu primer turno, aquí te presentamos el almacén y todos sus productos, cómo embalar, cómo entregar, cómo tu trabajo cambia a lo largo del día y cuándo terminas.";
        public const string INFINITE_DESC = "Con una cuota y tiempo límite para alcanzarla, será mejor ponerse serios; alcanza la cuota esperada a tiempo para avanzar de ronda a una más difícil hasta el infinito. ¿Serás capaz de poder manejar todo ese trabajo tú solo?";

        public bool IsOpen => _isOpen;
        public bool IsAnimating => _isAnimating;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null && NotebookImage != null)
            {
                _rectTransform = NotebookImage.rectTransform;
            }
        }

        private void Start()
        {
            // Inicializar en estado cerrado y reposo
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _closedPosition;
                _rectTransform.localScale = _closedScale;
            }

            if (NotebookImage != null && CuadernoCerrado != null)
            {
                NotebookImage.sprite = CuadernoCerrado;
            }

            if (OpenPagesContainer != null)
            {
                OpenPagesContainer.SetActive(false);
            }

            // Configurar listeners de botones
            if (TutorialButton != null) TutorialButton.onClick.AddListener(() => SelectMode(GameSessionMode.Tutorial));
            if (InfiniteButton != null) InfiniteButton.onClick.AddListener(() => SelectMode(GameSessionMode.Infinite));
            if (BackButton != null) BackButton.onClick.AddListener(CloseNotebook);
            if (StartButton != null) StartButton.onClick.AddListener(StartGame);

            SelectMode(GameSessionMode.Tutorial);
        }

        public void OpenNotebook()
        {
            if (_isOpen || _isAnimating) return;
            StartCoroutine(AnimateTransition(true));
        }

        public void CloseNotebook()
        {
            if (!_isOpen || _isAnimating) return;
            StartCoroutine(AnimateTransition(false));
        }

        public void SelectMode(GameSessionMode mode)
        {
            _selectedMode = mode;

            bool isTutorial = mode == GameSessionMode.Tutorial;

            if (TutorialHighlight != null)
            {
                TutorialHighlight.color = isTutorial ? new Color(0.20f, 0.65f, 0.25f, 0.55f) : new Color(0.1f, 0.1f, 0.1f, 0.15f);
            }

            if (InfiniteHighlight != null)
            {
                InfiniteHighlight.color = !isTutorial ? new Color(0.85f, 0.45f, 0.10f, 0.65f) : new Color(0.1f, 0.1f, 0.1f, 0.15f);
            }

            if (ModeTitleText != null)
            {
                ModeTitleText.text = isTutorial ? "TUTORIAL" : "JORNADA INFINITA";
                ModeTitleText.color = isTutorial ? new Color(0.15f, 0.45f, 0.18f) : new Color(0.75f, 0.25f, 0.05f);
            }

            if (ModeDescriptionText != null)
            {
                ModeDescriptionText.text = isTutorial ? TUTORIAL_DESC : INFINITE_DESC;
            }
        }

        private IEnumerator AnimateTransition(bool opening)
        {
            _isAnimating = true;

            if (opening)
            {
                if (MainMenu != null) MainMenu.SetLeftHemisphereVisible(false);
                if (OpenPagesContainer != null) OpenPagesContainer.SetActive(false);
            }
            else
            {
                if (OpenPagesContainer != null) OpenPagesContainer.SetActive(false);
            }

            float duration = 0.55f;
            float elapsed = 0f;

            Vector2 startPos = opening ? _closedPosition : _openPosition;
            Vector2 targetPos = opening ? _openPosition : _closedPosition;

            Vector3 startScale = opening ? _closedScale : _openScale;
            Vector3 targetScale = opening ? _openScale : _closedScale;

            int totalFrames = (AbrirFrames != null) ? AbrirFrames.Length : 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (_rectTransform != null)
                {
                    _rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                    _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, ease);
                }

                // Ciclar frames de animación de apertura / cierre
                if (NotebookImage != null && totalFrames > 0)
                {
                    if (opening)
                    {
                        if (t < 0.85f)
                        {
                            int frameIndex = Mathf.Clamp(Mathf.FloorToInt(t / 0.85f * totalFrames), 0, totalFrames - 1);
                            NotebookImage.sprite = AbrirFrames[frameIndex];
                        }
                        else
                        {
                            NotebookImage.sprite = CuadernoAbierto != null ? CuadernoAbierto : AbrirFrames[totalFrames - 1];
                        }
                    }
                    else
                    {
                        if (t < 0.85f)
                        {
                            int frameIndex = Mathf.Clamp(totalFrames - 1 - Mathf.FloorToInt(t / 0.85f * totalFrames), 0, totalFrames - 1);
                            NotebookImage.sprite = AbrirFrames[frameIndex];
                        }
                        else
                        {
                            NotebookImage.sprite = CuadernoCerrado;
                        }
                    }
                }

                yield return null;
            }

            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = targetPos;
                _rectTransform.localScale = targetScale;
            }

            if (opening)
            {
                if (NotebookImage != null && CuadernoAbierto != null)
                {
                    NotebookImage.sprite = CuadernoAbierto;
                }
                if (OpenPagesContainer != null)
                {
                    OpenPagesContainer.SetActive(true);
                }
                _isOpen = true;
            }
            else
            {
                if (NotebookImage != null && CuadernoCerrado != null)
                {
                    NotebookImage.sprite = CuadernoCerrado;
                }
                if (MainMenu != null)
                {
                    MainMenu.SetLeftHemisphereVisible(true);
                }
                _isOpen = false;
            }

            _isAnimating = false;
        }

        private void StartGame()
        {
            GameModeManager.ActiveMode = _selectedMode;
            Debug.Log($"<color=green><b>[CuadernoMenuController]</b></color> Iniciando modo: <b>{_selectedMode}</b>. Cargando GameplayScene...");
            SceneManager.LoadScene("GameplayScene");
        }
    }
}
