using System.Collections;
using LogiPack.Core;
using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    public class GameplayNotebookGuideUI : MonoBehaviour
    {
        [Header("Referencias Visuales")]
        public RectTransform NotebookRect;
        public Image NotebookImage;
        public GameObject ContentPagesContainer;
        public GameObject DarkOverlay;
        public Button CloseButton;
        public Button TriggerButton;

        [Header("Sprites de Animación")]
        public Sprite CuadernoCerrado;
        public Sprite CuadernoAbierto;
        public Sprite[] AbrirFrames;

        [Header("Posiciones y Escalas")]
        private Vector2 _idlePosition;
        private Vector3 _idleScale;
        private Vector2 _centerPosition = Vector2.zero;
        private Vector3 _openScale = Vector3.one * 0.90f;

        private bool _isOpen;
        private bool _isAnimating;

        private void Awake()
        {
            if (NotebookRect == null) NotebookRect = GetComponent<RectTransform>();
            if (NotebookImage == null) NotebookImage = GetComponent<Image>();

            if (NotebookRect != null)
            {
                _idlePosition = NotebookRect.anchoredPosition;
                _idleScale = NotebookRect.localScale;
            }

            if (TriggerButton != null)
            {
                TriggerButton.onClick.AddListener(OpenGuide);
            }

            if (CloseButton != null)
            {
                CloseButton.onClick.AddListener(CloseGuide);
            }

            if (DarkOverlay != null)
            {
                DarkOverlay.SetActive(false);
                var overlayBtn = DarkOverlay.GetComponent<Button>();
                if (overlayBtn != null) overlayBtn.onClick.AddListener(CloseGuide);
            }

            if (ContentPagesContainer != null)
            {
                ContentPagesContainer.SetActive(false);
            }
        }

        public void OpenGuide()
        {
            if (_isOpen || _isAnimating) return;
            StartCoroutine(AnimateTransition(true));
        }

        public void CloseGuide()
        {
            if (!_isOpen || _isAnimating) return;
            StartCoroutine(AnimateTransition(false));
        }

        private IEnumerator AnimateTransition(bool opening)
        {
            _isAnimating = true;

            if (opening)
            {
                GameModeManager.SetGameplayPaused(true);
                if (DarkOverlay != null) DarkOverlay.SetActive(true);
                if (ContentPagesContainer != null) ContentPagesContainer.SetActive(false);
            }
            else
            {
                if (ContentPagesContainer != null) ContentPagesContainer.SetActive(false);
            }

            float duration = 0.45f;
            float elapsed = 0f;

            Vector2 startPos = opening ? _idlePosition : _centerPosition;
            Vector2 targetPos = opening ? _centerPosition : _idlePosition;

            Vector3 startScale = opening ? _idleScale : _openScale;
            Vector3 targetScale = opening ? _openScale : _idleScale;

            int totalFrames = (AbrirFrames != null) ? AbrirFrames.Length : 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (NotebookRect != null)
                {
                    NotebookRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
                    NotebookRect.localScale = Vector3.Lerp(startScale, targetScale, ease);
                }

                if (NotebookImage != null && totalFrames > 0)
                {
                    if (opening)
                    {
                        if (t < 0.85f)
                        {
                            int idx = Mathf.Clamp(Mathf.FloorToInt(t / 0.85f * totalFrames), 0, totalFrames - 1);
                            NotebookImage.sprite = AbrirFrames[idx];
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
                            int idx = Mathf.Clamp(totalFrames - 1 - Mathf.FloorToInt(t / 0.85f * totalFrames), 0, totalFrames - 1);
                            NotebookImage.sprite = AbrirFrames[idx];
                        }
                        else
                        {
                            NotebookImage.sprite = CuadernoCerrado;
                        }
                    }
                }

                yield return null;
            }

            if (NotebookRect != null)
            {
                NotebookRect.anchoredPosition = targetPos;
                NotebookRect.localScale = targetScale;
            }

            if (opening)
            {
                _isOpen = true;
                if (NotebookImage != null && CuadernoAbierto != null) NotebookImage.sprite = CuadernoAbierto;
                if (ContentPagesContainer != null) ContentPagesContainer.SetActive(true);
            }
            else
            {
                _isOpen = false;
                if (DarkOverlay != null) DarkOverlay.SetActive(false);
                if (NotebookImage != null && CuadernoCerrado != null) NotebookImage.sprite = CuadernoCerrado;
                GameModeManager.SetGameplayPaused(false);
            }

            _isAnimating = false;
        }
    }
}
