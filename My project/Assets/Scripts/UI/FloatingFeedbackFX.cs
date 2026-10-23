using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    public class FloatingFeedbackFX : MonoBehaviour
    {
        private static FloatingFeedbackFX _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }

        public static void Show(Vector3 worldPosition, string message, Color color)
        {
            if (_instance == null)
            {
                var go = new GameObject("FloatingFeedbackFX_Manager");
                _instance = go.AddComponent<FloatingFeedbackFX>();
            }

            _instance.StartCoroutine(_instance.AnimateFloatingText(worldPosition, message, color));
        }

        private IEnumerator AnimateFloatingText(Vector3 worldPos, string message, Color color)
        {
            // Crear Canvas flotante en World Space
            GameObject textGO = new GameObject("FloatingText_" + message);
            textGO.transform.position = worldPos + Vector3.up * 0.5f;

            Canvas canvas = textGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 150; // Por encima de todo

            RectTransform rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(350, 70);
            rt.localScale = Vector3.one * 0.015f;

            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Text textComp = textGO.AddComponent<Text>();
            textComp.text = message;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 32;
            textComp.color = color;

            Font customFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font fredoka = Resources.Load<Font>("Fredoka-Bold");
            if (fredoka != null) textComp.font = fredoka;
            else if (customFont != null) textComp.font = customFont;

            float duration = 1.3f;
            float elapsed = 0f;
            Vector3 startPos = textGO.transform.position;
            Vector3 targetPos = startPos + Vector3.up * 1.4f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                textGO.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                
                // Fade out en el último tercio
                if (t > 0.6f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                    textComp.color = new Color(color.r, color.g, color.b, alpha);
                    outline.effectColor = new Color(0f, 0f, 0f, alpha * 0.85f);
                }

                yield return null;
            }

            Destroy(textGO);
        }
    }
}
