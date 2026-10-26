using System.Collections;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LogiPack.UI.Menu
{
    /// <summary>
    /// Representa una fila en el formato tabular de rankings Top 5 (Posición, Métrica y Detalles/Fecha).
    /// </summary>
    [System.Serializable]
    public struct RankingRowUI
    {
        public Text PosText;       // "01", "02"... (Ancho 45px)
        public Text MetricText;    // "$4,250" o "Ronda 8" (Ancho 130px, bold)
        public Text DetailsText;   // "★★★ • 25/10 16:55" (Ancho 190px)
    }

    /// <summary>
    /// Controla la apertura/cierre del cuaderno, la paginación física de 3 hojas
    /// mediante las flechas CozyUI y el despliegue del sistema de rankings Top 5.
    /// </summary>
    public class CuadernoMenuController : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!_isOpen && !_isAnimating)
            {
                OpenNotebook();
            }
        }
        [Header("Sprites de Animación de Apertura")]
        public Image NotebookImage;
        public Sprite CuadernoCerrado;
        public Sprite[] AbrirFrames; // 4 frames: abrir_paso_1..4
        public Sprite CuadernoAbierto;

        [Header("Contenedor de Páginas Abiertas")]
        public GameObject OpenPagesContainer;

        [Header("Hojas Físicas Dobles")]
        public GameObject SheetTutorial;
        public GameObject SheetNormalDay;
        public GameObject SheetInfinite;
        public GameObject SheetCheckout;

        [Header("Botones de Paginación y Cierre")]
        public Button PrevPageButton; // Flecha Izquierda (ArrowBrownOuline22)
        public Button NextPageButton; // Flecha Derecha (ArrowBrownOuline23)
        public Button BackButton;

        [Header("Efecto de Volteo de Página")]
        public Image PageFlipOverlay;
        public Sprite[] PageFlipForwardSprites;  // 5 frames cuaderno_pasar_pagina_der_izq
        public Sprite[] PageFlipBackwardSprites; // 5 frames cuaderno_pasar_pagina_izq_der
        public AudioClip PageFlipClip;

        [Header("Página Derecha: Ficha de Capacitación (Hoja 0)")]
        public Text TutorialStatusText;
        public Image TutorialStatusIcon;
        public Sprite TutorialCompletedSprite;
        public Sprite TutorialPendingSprite;

        [Header("Página Derecha: Top 5 Jornada Normal (Hoja 1) - Formato Tabular")]
        public RankingRowUI[] NormalDayRows = new RankingRowUI[5];

        [Header("Página Derecha: Top 5 Jornada Infinita (Hoja 2) - Formato Tabular")]
        public RankingRowUI[] InfiniteRows = new RankingRowUI[5];

        [Header("Página Derecha: Top 5 Check-out (Hoja 3) - Formato Tabular")]
        public RankingRowUI[] CheckoutRows = new RankingRowUI[5];

        [Header("Botones de Iniciar Partida por Hoja")]
        public Button StartTutorialButton;
        public Button StartNormalDayButton;
        public Button StartInfiniteButton;
        public Button StartCheckoutButton;

        [Header("Referencia al Menú Principal")]
        public MainMenuController MainMenu;

        private RectTransform _rectTransform;
        private Vector2 _closedPosition = new Vector2(460f, -20f);
        private Vector2 _openPosition = new Vector2(0f, 0f);
        private Vector3 _closedScale = new Vector3(0.84f, 0.84f, 1f);
        private Vector3 _openScale = new Vector3(1.25f, 1.25f, 1f);

        private bool _isOpen = false;
        private bool _isAnimating = false;
        private bool _isFlipping = false;
        private int _currentSheetIndex = 0; // 0: Tutorial, 1: Normal, 2: Infinita, 3: Check-out

        public const string TUTORIAL_DESC = "Aprende los fundamentos de la lógica simbólica con la supervisora Mochi Miles. Descubre cómo armar fórmulas FBF con proposiciones atómicas (p, q, s, r), operadores (∧, ∨, →, ↔) y anidamiento de sub-cajas paso a paso sin límite de tiempo.";
        public const string NORMAL_DAY_DESC = "¿Nuevo por aquí? Haz una jornada estándar de 06:00 a 22:00 para descubrir el almacén, cumplir cuotas y poner a prueba tu destreza lógica con pedidos individuales a tu propio ritmo.";
        public const string INFINITE_DESC = "Con una cuota y tiempo límite para alcanzarla, será mejor ponerse serios; alcanza la cuota esperada a tiempo para avanzar de ronda a una más difícil hasta el infinito. ¿Serás capaz de poder manejar todo ese trabajo tú solo?";
        public const string CHECKOUT_DESC = "Demuestra tu dominio de las tablas de verdad como auditor de almacén. Inspecciona expedientes en lotes de 3 y clasifícalos con precisión como Tautología, Contradicción o Contingencia a tu propio ritmo.";

        public bool IsOpen => _isOpen;
        public bool IsAnimating => _isAnimating || _isFlipping;

        private void Awake()
        {
            UnityEngine.Application.runInBackground = true;
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null && NotebookImage != null)
            {
                _rectTransform = NotebookImage.rectTransform;
            }
        }

        private void Start()
        {
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

            if (PageFlipOverlay != null)
            {
                PageFlipOverlay.gameObject.SetActive(false);
            }

            // Configurar listeners de botones
            if (PrevPageButton != null) PrevPageButton.onClick.AddListener(OnPrevPageClicked);
            if (NextPageButton != null) NextPageButton.onClick.AddListener(OnNextPageClicked);
            if (BackButton != null) BackButton.onClick.AddListener(CloseNotebook);

            if (StartTutorialButton != null) StartTutorialButton.onClick.AddListener(() => StartGame(GameSessionMode.Tutorial));
            if (StartNormalDayButton != null) StartNormalDayButton.onClick.AddListener(() => StartGame(GameSessionMode.NormalDay));
            if (StartInfiniteButton != null) StartInfiniteButton.onClick.AddListener(() => StartGame(GameSessionMode.Infinite));
            if (StartCheckoutButton != null) StartCheckoutButton.onClick.AddListener(() => StartGame(GameSessionMode.Checkout));

            ShowSheetInstant(0);
        }

        #region Cinemática de Apertura y Cierre

        public void OpenNotebook()
        {
            if (_isOpen || _isAnimating) return;
            StartCoroutine(AnimateTransition(true));
        }

        public void CloseNotebook()
        {
            if (!_isOpen || _isAnimating || _isFlipping) return;
            StartCoroutine(AnimateTransition(false));
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
                ShowSheetInstant(_currentSheetIndex);
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

        #endregion

        #region Navegación entre Hojas y Volteo Animado

        public void OnNextPageClicked()
        {
            if (_isFlipping || !_isOpen || _currentSheetIndex >= 3) return;
            StartCoroutine(FlipPageRoutine(_currentSheetIndex + 1, forward: true));
        }

        public void OnPrevPageClicked()
        {
            if (_isFlipping || !_isOpen || _currentSheetIndex <= 0) return;
            StartCoroutine(FlipPageRoutine(_currentSheetIndex - 1, forward: false));
        }

        private IEnumerator FlipPageRoutine(int targetIndex, bool forward)
        {
            _isFlipping = true;

            // Reproducir sonido mediante AudioManager si está disponible
            if (PageFlipClip != null)
            {
                if (LogiPack.Audio.AudioManager.Instance != null)
                {
                    LogiPack.Audio.AudioManager.Instance.PlaySFX(PageFlipClip);
                }
                else
                {
                    var aSrc = GetComponent<AudioSource>();
                    if (aSrc != null) aSrc.PlayOneShot(PageFlipClip, 0.6f);
                }
            }

            Sprite[] flipFrames = forward ? PageFlipForwardSprites : PageFlipBackwardSprites;
            int frameCount = (flipFrames != null) ? flipFrames.Length : 0;

            if (PageFlipOverlay != null && frameCount > 0)
            {
                PageFlipOverlay.gameObject.SetActive(true);

                // Cuadros de elevación (0 a 2)
                for (int i = 0; i < Mathf.Min(3, frameCount); i++)
                {
                    PageFlipOverlay.sprite = flipFrames[i];
                    yield return new WaitForSecondsRealtime(0.05f);
                }

                // Cúspide de volteo a 90°: conmutar hojas y actualizar datos
                ShowSheetInstant(targetIndex);

                // Cuadros de asentamiento (3 a 4)
                for (int i = 3; i < frameCount; i++)
                {
                    PageFlipOverlay.sprite = flipFrames[i];
                    yield return new WaitForSecondsRealtime(0.05f);
                }

                PageFlipOverlay.gameObject.SetActive(false);
            }
            else
            {
                // Fallback sin animación si no hay sprites
                ShowSheetInstant(targetIndex);
                yield return new WaitForSecondsRealtime(0.15f);
            }

            _isFlipping = false;
        }

        public void ShowSheetInstant(int index)
        {
            _currentSheetIndex = Mathf.Clamp(index, 0, 3);

            if (SheetTutorial != null) SheetTutorial.SetActive(_currentSheetIndex == 0);
            if (SheetNormalDay != null) SheetNormalDay.SetActive(_currentSheetIndex == 1);
            if (SheetInfinite != null) SheetInfinite.SetActive(_currentSheetIndex == 2);
            if (SheetCheckout != null) SheetCheckout.SetActive(_currentSheetIndex == 3);

            UpdateArrowsVisibility();
            RefreshSheetData(_currentSheetIndex);
        }

        private void UpdateArrowsVisibility()
        {
            if (PrevPageButton != null)
            {
                PrevPageButton.gameObject.SetActive(_currentSheetIndex > 0);
            }
            if (NextPageButton != null)
            {
                NextPageButton.gameObject.SetActive(_currentSheetIndex < 3);
            }
        }

        #endregion

        #region Poblado de Datos y Rankings

        public void RefreshSheetData(int sheetIndex)
        {
            switch (sheetIndex)
            {
                case 0:
                    PopulateTutorialData();
                    break;
                case 1:
                    PopulateNormalDayRankings();
                    break;
                case 2:
                    PopulateInfiniteRankings();
                    break;
                case 3:
                    PopulateCheckoutRankings();
                    break;
            }
        }

        private void PopulateTutorialData()
        {
            bool isCompleted = RankingManager.IsTutorialCompleted();

            if (TutorialStatusText != null)
            {
                TutorialStatusText.text = isCompleted
                    ? "<b>ESTADO: CAPACITACIÓN COMPLETADA [✓]</b>"
                    : "<b>ESTADO: PENDIENTE [!]</b>";
            }

            if (TutorialStatusIcon != null)
            {
                if (isCompleted && TutorialCompletedSprite != null)
                {
                    TutorialStatusIcon.sprite = TutorialCompletedSprite;
                }
                else if (!isCompleted && TutorialPendingSprite != null)
                {
                    TutorialStatusIcon.sprite = TutorialPendingSprite;
                }
            }
        }

        private void PopulateNormalDayRankings()
        {
            var records = RankingManager.GetNormalTop5();

            for (int i = 0; i < 5; i++)
            {
                if (NormalDayRows != null && i < NormalDayRows.Length)
                {
                    var row = NormalDayRows[i];
                    if (row.PosText != null) row.PosText.text = $"{i + 1:D2}";

                    if (i < records.Count)
                    {
                        var r = records[i];
                        if (row.MetricText != null)
                        {
                            row.MetricText.text = $"${r.NetMoney:#,##0}";
                        }
                        if (row.DetailsText != null)
                        {
                            string starDisplay = r.Stars > 0 ? new string('★', Mathf.Clamp(r.Stars, 1, 3)) : "---";
                            row.DetailsText.text = $"{starDisplay} • {r.Timestamp}";
                        }
                    }
                    else
                    {
                        if (row.MetricText != null) row.MetricText.text = "---";
                        if (row.DetailsText != null) row.DetailsText.text = "Sin registro";
                    }
                }
            }
        }

        private void PopulateInfiniteRankings()
        {
            var records = RankingManager.GetInfiniteTop5();

            for (int i = 0; i < 5; i++)
            {
                if (InfiniteRows != null && i < InfiniteRows.Length)
                {
                    var row = InfiniteRows[i];
                    if (row.PosText != null) row.PosText.text = $"{i + 1:D2}";

                    if (i < records.Count)
                    {
                        var r = records[i];
                        if (row.MetricText != null)
                        {
                            row.MetricText.text = $"Ronda {r.RoundsCleared}";
                        }
                        if (row.DetailsText != null)
                        {
                            row.DetailsText.text = $"+${r.TotalMoney:#,##0} • {r.Timestamp}";
                        }
                    }
                    else
                    {
                        if (row.MetricText != null) row.MetricText.text = "---";
                        if (row.DetailsText != null) row.DetailsText.text = "Sin registro";
                    }
                }
            }
        }

        private void PopulateCheckoutRankings()
        {
            var records = RankingManager.GetCheckoutTop5();

            for (int i = 0; i < 5; i++)
            {
                if (CheckoutRows != null && i < CheckoutRows.Length)
                {
                    var row = CheckoutRows[i];
                    if (row.PosText != null) row.PosText.text = $"{i + 1:D2}";

                    if (i < records.Count)
                    {
                        var r = records[i];
                        if (row.MetricText != null)
                        {
                            row.MetricText.text = $"{r.Hits} / {r.Total}";
                        }
                        if (row.DetailsText != null)
                        {
                            string starDisplay = r.Stars > 0 ? new string('★', Mathf.Clamp(r.Stars, 1, 3)) : "---";
                            row.DetailsText.text = $"{starDisplay} • {r.Timestamp}";
                        }
                    }
                    else
                    {
                        if (row.MetricText != null) row.MetricText.text = "---";
                        if (row.DetailsText != null) row.DetailsText.text = "Sin registro";
                    }
                }
            }
        }

        #endregion

        #region Lanzamiento de Partidas

        private void StartGame(GameSessionMode mode)
        {
            if (_isFlipping || _isAnimating) return;

            GameModeManager.ActiveMode = mode;
            string targetScene = (mode == GameSessionMode.Tutorial) ? "TutorialScene"
                               : (mode == GameSessionMode.Checkout) ? "CheckoutScene"
                               : "GameplayScene";
            Debug.Log($"<color=green><b>[CuadernoMenuController]</b></color> Iniciando modo: <b>{mode}</b>. Cargando {targetScene}...");

            if (LogiPack.Managers.SceneTransitionManager.Instance != null)
            {
                LogiPack.Managers.SceneTransitionManager.Instance.LoadSceneWithWipe(targetScene, 1.0f);
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }

        #endregion
    }
}
