using System.Collections;
using LogiPack.Core;
using LogiPack.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    public class GameplayNotebookGuideUI : MonoBehaviour
    {
        [Header("Referencias del Modal")]
        public GameObject ModalRoot;
        public RectTransform NotebookWindowRect;
        public Image NotebookWindowImage;
        public Button CloseButton;
        public Button OverlayCloseButton;
        public Button TriggerButton;

        [Header("Sprites")]
        public Sprite CuadernoAbierto;
        public Sprite[] AbrirFrames;

        private bool _isOpen;
        private bool _isAnimating;
        private Coroutine _currentAnim;

        private void Awake()
        {
            if (TriggerButton != null)
            {
                TriggerButton.onClick.RemoveAllListeners();
                TriggerButton.onClick.AddListener(OpenGuide);
            }

            if (CloseButton != null)
            {
                CloseButton.onClick.RemoveAllListeners();
                CloseButton.onClick.AddListener(CloseGuide);
            }

            if (OverlayCloseButton != null)
            {
                OverlayCloseButton.onClick.RemoveAllListeners();
                OverlayCloseButton.onClick.AddListener(CloseGuide);
            }

            if (ModalRoot != null)
            {
                ModalRoot.SetActive(false);
            }
        }

        public void OpenGuide()
        {
            if (_isOpen || _isAnimating) return;
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(AnimateOpen());
        }

        public void CloseGuide()
        {
            if (!_isOpen || _isAnimating) return;
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(AnimateClose());
        }

        private IEnumerator AnimateOpen()
        {
            _isAnimating = true;
            _isOpen = true;

            GameModeManager.SetGameplayPaused(true);

            if (ModalRoot != null)
            {
                ModalRoot.SetActive(true);
                ModalRoot.transform.SetAsLastSibling();
            }

            if (NotebookWindowRect != null)
            {
                NotebookWindowRect.localScale = Vector3.one * 0.75f;
            }

            float duration = 0.22f;
            float elapsed = 0f;
            int totalFrames = (AbrirFrames != null) ? AbrirFrames.Length : 0;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (NotebookWindowRect != null)
                {
                    NotebookWindowRect.localScale = Vector3.Lerp(Vector3.one * 0.75f, Vector3.one, ease);
                }

                if (NotebookWindowImage != null && totalFrames > 0)
                {
                    if (t < 0.85f)
                    {
                        int idx = Mathf.Clamp(Mathf.FloorToInt(t / 0.85f * totalFrames), 0, totalFrames - 1);
                        NotebookWindowImage.sprite = AbrirFrames[idx];
                    }
                    else
                    {
                        NotebookWindowImage.sprite = CuadernoAbierto != null ? CuadernoAbierto : AbrirFrames[totalFrames - 1];
                    }
                }

                yield return null;
            }

            if (NotebookWindowRect != null) NotebookWindowRect.localScale = Vector3.one;
            if (NotebookWindowImage != null && CuadernoAbierto != null) NotebookWindowImage.sprite = CuadernoAbierto;

            _isAnimating = false;
        }

        private IEnumerator AnimateClose()
        {
            _isAnimating = true;

            float duration = 0.18f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = Mathf.SmoothStep(0f, 1f, t);

                if (NotebookWindowRect != null)
                {
                    NotebookWindowRect.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.75f, ease);
                }

                yield return null;
            }

            if (ModalRoot != null)
            {
                ModalRoot.SetActive(false);
            }

            _isOpen = false;
            _isAnimating = false;
            GameModeManager.SetGameplayPaused(false);
        }
    }
}
