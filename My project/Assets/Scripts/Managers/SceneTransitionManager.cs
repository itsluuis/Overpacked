using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LogiPack.Managers
{
    /// <summary>
    /// Gestiona las transiciones cinematográficas entre escenas con cortinilla (wipe) de derecha a izquierda.
    /// Opera en DontDestroyOnLoad con un Canvas de máxima prioridad (sortingOrder = 999).
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RectTransform _curtainRect;
        private Image _curtainImage;

        public bool IsTransitioning { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureTransitionUI();
        }

        private void EnsureTransitionUI()
        {
            if (_canvas == null)
            {
                _canvas = gameObject.GetComponent<Canvas>();
                if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();

                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.overrideSorting = true;
                _canvas.sortingOrder = 999;

                var scaler = gameObject.GetComponent<CanvasScaler>();
                if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                var raycaster = gameObject.GetComponent<GraphicRaycaster>();
                if (raycaster == null) raycaster = gameObject.AddComponent<GraphicRaycaster>();

                _canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                _canvasGroup.blocksRaycasts = false;
            }

            if (_curtainRect == null)
            {
                Transform existing = transform.Find("WipeCurtain");
                GameObject curtainObj;
                if (existing != null)
                {
                    curtainObj = existing.gameObject;
                }
                else
                {
                    curtainObj = new GameObject("WipeCurtain");
                    curtainObj.transform.SetParent(transform, false);
                }

                _curtainRect = curtainObj.GetComponent<RectTransform>();
                if (_curtainRect == null) _curtainRect = curtainObj.AddComponent<RectTransform>();

                _curtainRect.anchorMin = new Vector2(0, 0);
                _curtainRect.anchorMax = new Vector2(1, 1);
                _curtainRect.sizeDelta = Vector2.zero;
                _curtainRect.anchoredPosition = new Vector2(2500, 0); // Off-screen a la derecha

                _curtainImage = curtainObj.GetComponent<Image>();
                if (_curtainImage == null) _curtainImage = curtainObj.AddComponent<Image>();
                _curtainImage.color = Color.black;
                curtainObj.SetActive(false);
            }
        }

        public void LoadSceneWithWipe(string sceneName, float duration = 1.0f)
        {
            if (IsTransitioning) return;
            StartCoroutine(WipeTransitionRoutine(sceneName, Mathf.Max(0.2f, duration)));
        }

        private IEnumerator WipeTransitionRoutine(string sceneName, float totalDuration)
        {
            IsTransitioning = true;
            EnsureTransitionUI();

            _curtainRect.gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;

            float halfDuration = totalDuration * 0.5f;

            // 1. Fase de Entrada: Cortina negra entra de derecha (+1920) al centro (0)
            float elapsed = 0f;
            Vector2 startPos = new Vector2(2000f, 0f);
            Vector2 centerPos = Vector2.zero;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / halfDuration));
                _curtainRect.anchoredPosition = Vector2.Lerp(startPos, centerPos, t);
                yield return null;
            }

            _curtainRect.anchoredPosition = centerPos;

            // 2. Carga asíncrona de la escena destino
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
            loadOp.allowSceneActivation = true;

            while (!loadOp.isDone)
            {
                yield return null;
            }

            // Pequeña pausa de un frame para estabilizar el render de la nueva escena
            yield return null;

            // 3. Fase de Salida: Cortina negra continúa su camino hacia la izquierda (0 a -2000)
            elapsed = 0f;
            Vector2 exitPos = new Vector2(-2000f, 0f);

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / halfDuration));
                _curtainRect.anchoredPosition = Vector2.Lerp(centerPos, exitPos, t);
                yield return null;
            }

            _curtainRect.anchoredPosition = new Vector2(2500f, 0f); // Reset offscreen
            _curtainRect.gameObject.SetActive(false);
            _canvasGroup.blocksRaycasts = false;
            IsTransitioning = false;
        }
    }
}
