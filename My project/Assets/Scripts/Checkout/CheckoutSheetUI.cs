using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Checkout
{
    /// <summary>
    /// Controla una hoja física de expediente en la mesa de auditoría:
    /// visualización de FBF, enfoque dinámico, estampación de sello diegético y archivado.
    /// </summary>
    public class CheckoutSheetUI : MonoBehaviour
    {
        [Header("Referencias Visuales de la Hoja")]
        public RectTransform RootRectTransform;
        public CanvasGroup SheetCanvasGroup;
        public Text HeaderText;
        public Text FormulaText;
        public Text NaturalLanguageText;
        public GameObject HighlightBorder;

        [Header("Overlay del Sello Diegético")]
        public GameObject StampContainer;
        public RectTransform StampRectTransform;
        public Text StampText;
        public Image StampBorderImage;

        [Header("Sonido")]
        public AudioClip StampClip;

        public CheckoutFormula CurrentFormula { get; private set; }

        private Vector2 _originalPosition;
        private Vector3 _originalScale = Vector3.one;
        private bool _isInitialized = false;

        private void Awake()
        {
            if (RootRectTransform == null) RootRectTransform = GetComponent<RectTransform>();
            if (SheetCanvasGroup == null) SheetCanvasGroup = GetComponent<CanvasGroup>();
            if (RootRectTransform != null)
            {
                _originalPosition = RootRectTransform.anchoredPosition;
                _originalScale = RootRectTransform.localScale;
                _isInitialized = true;
            }

            if (StampContainer != null) StampContainer.SetActive(false);
            if (HighlightBorder != null) HighlightBorder.SetActive(false);
        }

        public void Setup(CheckoutFormula formula, int orderIndex)
        {
            if (!_isInitialized && RootRectTransform != null)
            {
                _originalPosition = RootRectTransform.anchoredPosition;
                _originalScale = RootRectTransform.localScale;
                _isInitialized = true;
            }

            CurrentFormula = formula;

            if (HeaderText != null) HeaderText.text = $"EXPEDIENTE #{orderIndex:D2}";
            if (FormulaText != null) FormulaText.text = formula != null ? formula.FormulaFBF : "---";
            if (NaturalLanguageText != null) NaturalLanguageText.text = formula != null ? $"\"{formula.NaturalLanguage}\"" : "";

            if (StampContainer != null) StampContainer.SetActive(false);
            if (HighlightBorder != null) HighlightBorder.SetActive(false);

            if (RootRectTransform != null)
            {
                RootRectTransform.anchoredPosition = _originalPosition;
                RootRectTransform.localScale = _originalScale;
            }

            if (SheetCanvasGroup != null)
            {
                SheetCanvasGroup.alpha = 1f;
                SheetCanvasGroup.blocksRaycasts = false; // Solo la hoja activa interactúa si fuera necesario
            }
        }

        public void SetFocused(bool focused)
        {
            if (HighlightBorder != null) HighlightBorder.SetActive(focused);

            if (RootRectTransform != null)
            {
                // La hoja activa se amplía sutilmente a 1.04x; las demás a 0.96x
                RootRectTransform.localScale = focused ? _originalScale * 1.04f : _originalScale * 0.96f;
            }

            if (SheetCanvasGroup != null)
            {
                SheetCanvasGroup.alpha = focused ? 1.0f : 0.85f;
            }
        }

        public void PlayStamp(bool isCorrect, Action onComplete)
        {
            StartCoroutine(StampRoutine(isCorrect, onComplete));
        }

        private IEnumerator StampRoutine(bool isCorrect, Action onComplete)
        {
            // Reproducir SFX si está disponible
            if (LogiPack.Audio.AudioManager.Instance != null && StampClip != null)
            {
                LogiPack.Audio.AudioManager.Instance.PlaySFX(StampClip);
            }
            else
            {
                var aSrc = GetComponent<AudioSource>();
                if (aSrc != null && StampClip != null) aSrc.PlayOneShot(StampClip, 0.7f);
            }

            if (StampContainer != null && StampText != null && StampRectTransform != null)
            {
                Color stampColor = isCorrect
                    ? new Color(0.11f, 0.50f, 0.20f, 0.95f)  // Verde bosque (#1B7A32)
                    : new Color(0.70f, 0.13f, 0.13f, 0.95f); // Rojo carmesí (#B22222)

                StampText.text = isCorrect ? "✓ DESPACHADO" : "✗ ERROR DESTINO";
                StampText.color = stampColor;

                if (StampBorderImage != null)
                {
                    StampBorderImage.color = stampColor;
                }

                // Rotación diegética: -4° para acierto, +5° para error
                float rotZ = isCorrect ? -4.5f : 5.0f;
                StampRectTransform.localEulerAngles = new Vector3(0, 0, rotZ);

                // Micro-rebote de impacto del sello
                StampContainer.SetActive(true);
                StampRectTransform.localScale = Vector3.one * 1.5f;

                float elapsed = 0f;
                float duration = 0.15f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    StampRectTransform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
                    yield return null;
                }
                StampRectTransform.localScale = Vector3.one;
            }

            // Pausa obligatoria diegética de 0.4s para que el jugador registre el resultado
            yield return new WaitForSecondsRealtime(0.40f);

            onComplete?.Invoke();
        }

        public void SlideOut(Action onComplete)
        {
            StartCoroutine(SlideOutRoutine(onComplete));
        }

        private IEnumerator SlideOutRoutine(Action onComplete)
        {
            float elapsed = 0f;
            float duration = 0.35f;

            Vector2 startPos = RootRectTransform != null ? RootRectTransform.anchoredPosition : Vector2.zero;
            Vector2 targetPos = startPos + new Vector2(0, 500f); // Deslizar hacia arriba fuera de mesa

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (RootRectTransform != null)
                {
                    RootRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                }

                if (SheetCanvasGroup != null)
                {
                    SheetCanvasGroup.alpha = Mathf.Lerp(1f, 0f, ease);
                }

                yield return null;
            }

            if (gameObject != null) gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
