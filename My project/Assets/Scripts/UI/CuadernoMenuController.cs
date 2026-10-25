using System.Collections;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LogiPack.UI.Menu
{
    /// <summary>
    /// Controla la apertura/cierre del cuaderno, la paginación física de 3 hojas
    /// mediante las flechas CozyUI y el despliegue del sistema de rankings Top 5.
    /// </summary>
    public class CuadernoMenuController : MonoBehaviour
    {
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

        [Header("Página Derecha: Top 5 Jornada Normal (Hoja 1)")]
        public Text[] NormalDayRowTexts; // 5 filas

        [Header("Página Derecha: Top 5 Jornada Infinita (Hoja 2)")]
        public Text[] InfiniteRowTexts;  // 5 filas

        [Header("Botones de Iniciar Partida por Hoja")]
        public Button StartTutorialButton;
        public Button StartNormalDayButton;
        public Button StartInfiniteButton;

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
        private int _currentSheetIndex = 0; // 0: Tutorial, 1: Normal, 2: Infinita

        public const string TUTORIAL_DESC = "Aprende los fundamentos de la lógica simbólica con la supervisora Mochi Miles. Descubre cómo armar fórmulas FBF con proposiciones atómicas (p, q, s, r), operadores (∧, ∨, →, ↔) y anidamiento de sub-cajas paso a paso sin límite de tiempo.";
        public const string NORMAL_DAY_DESC = "¿Nuevo por aquí? Haz una jornada estándar de 06:00 a 22:00 para descubrir el almacén, cumplir cuotas y poner a prueba tu destreza lógica con pedidos individuales a tu propio ritmo.";
        public const string INFINITE_DESC = "Con una cuota y tiempo límite para alcanzarla, será mejor ponerse serios; alcanza la cuota esperada a tiempo para avanzar de ronda a una más difícil hasta el infinito. ¿Serás capaz de poder manejar todo ese trabajo tú solo?";

        public bool IsOpen => _isOpen;
        public bool IsAnimating => _isAnimating || _isFlipping;

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
            if (_isFlipping || !_isOpen || _currentSheetIndex >= 2) return;
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

        private void ShowSheetInstant(int index)
        {
            _currentSheetIndex = Mathf.Clamp(index, 0, 2);

            if (SheetTutorial != null) SheetTutorial.SetActive(_currentSheetIndex == 0);
            if (SheetNormalDay != null) SheetNormalDay.SetActive(_currentSheetIndex == 1);
            if (SheetInfinite != null) SheetInfinite.SetActive(_currentSheetIndex == 2);

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
                NextPageButton.gameObject.SetActive(_currentSheetIndex < 2);
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
            }
        }

        private void PopulateTutorialData()
        {
            bool isCompleted = RankingManager.IsTutorialCompleted();

            if (TutorialStatusText != null)
            {
                TutorialStatusText.text = isCompleted
                    ? "<color=#00AA44><b>ESTADO: CAPACITACIÓN COMPLETADA [✓]</b></color>"
                    : "<color=#C88800><b>ESTADO: PENDIENTE [!]</b></color>";
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
                if (NormalDayRowTexts != null && i < NormalDayRowTexts.Length && NormalDayRowTexts[i] != null)
                {
                    string rankTag = i switch
                    {
                        0 => "<color=#FFD700><b>#1</b></color>",
                        1 => "<color=#D0D0D0><b>#2</b></color>",
                        2 => "<color=#CD7F32><b>#3</b></color>",
                        _ => $"<b>#{i + 1}</b>"
                    };

                    if (i < records.Count)
                    {
                        var r = records[i];
                        string starsStr = new string('★', Mathf.Clamp(r.Stars, 0, 3));
                        NormalDayRowTexts[i].text = $"{rankTag}   <color=#00AA44><b>${r.NetMoney}</b></color>   <color=#FFD700>{starsStr}</color>   <size=14><color=#777777>{r.Timestamp}</color></size>";
                    }
                    else
                    {
                        NormalDayRowTexts[i].text = $"{rankTag}   <color=#888888>--- Sin registro ---</color>";
                    }
                }
            }
        }

        private void PopulateInfiniteRankings()
        {
            var records = RankingManager.GetInfiniteTop5();

            for (int i = 0; i < 5; i++)
            {
                if (InfiniteRowTexts != null && i < InfiniteRowTexts.Length && InfiniteRowTexts[i] != null)
                {
                    string rankTag = i switch
                    {
                        0 => "<color=#FFD700><b>#1</b></color>",
                        1 => "<color=#D0D0D0><b>#2</b></color>",
                        2 => "<color=#CD7F32><b>#3</b></color>",
                        _ => $"<b>#{i + 1}</b>"
                    };

                    if (i < records.Count)
                    {
                        var r = records[i];
                        InfiniteRowTexts[i].text = $"{rankTag}   <b>Ronda {r.RoundsCleared}</b>   <color=#00AA44>+${r.TotalMoney}</color>   <size=14><color=#777777>{r.Timestamp}</color></size>";
                    }
                    else
                    {
                        InfiniteRowTexts[i].text = $"{rankTag}   <color=#888888>--- Sin registro ---</color>";
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
            string targetScene = (mode == GameSessionMode.Tutorial) ? "TutorialScene" : "GameplayScene";
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
