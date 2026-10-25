using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Tutorial
{
    public enum MochiExpression
    {
        BrazosCruzados,
        Mostrando,
        CinturaOjosCerrados
    }

    /// <summary>
    /// Controlador visual de Mochi Miles y el sistema de diálogos pedagógicos.
    /// Implementa:
    /// - Anclaje inferior al ras de pantalla (Decisión 2).
    /// - Cajas adaptativas (pequeño/mediano/grande) (Decisiones 4 y 10).
    /// - Efecto máquina de escribir con la Regla de los Dos Clics (Decisión 6).
    /// - Tiempo desescalado independiente del reloj (Decisión 9).
    /// </summary>
    public class TutorialDialogueUI : MonoBehaviour
    {
        public static TutorialDialogueUI Instance { get; private set; }

        [Header("Sprites de Mochi Miles")]
        public Image MochiImage;
        public Sprite MochiBrazosCruzados;
        public Sprite MochiMostrando;
        public Sprite MochiCinturaOjosCerrados;

        [Header("Sprites de Cajas de Diálogo")]
        public Image DialogueBoxImage;
        public Sprite BoxPequeno;
        public Sprite BoxMediano;
        public Sprite BoxGrande;

        [Header("Elementos de Texto e Indicador")]
        public Text DialogueText;
        public GameObject ContinueArrowIndicator; // Flechita parpadeante "▼"
        public AudioSource AudioSource;
        public AudioClip TypingClip;

        [Header("Configuración de Typewriter")]
        public float TypingSpeed = 0.025f; // Segundos por carácter en tiempo real

        private string[] _currentLines;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;
        private bool _isTextFullyDisplayed = false;
        private Coroutine _typewriterCoroutine;
        private Action _onDialogueComplete;
        private string _targetFullText = "";

        private void Awake()
        {
            if (Instance == null) Instance = this;

            if (ContinueArrowIndicator != null)
            {
                ContinueArrowIndicator.SetActive(false);
            }
        }

        private void Start()
        {
            // Suscribirse al clic del bloqueador para avanzar diálogos
            if (TutorialInteractionBlocker.Instance != null)
            {
                TutorialInteractionBlocker.Instance.OnBlockerClicked += HandleClick;
            }
        }

        private void OnDestroy()
        {
            if (TutorialInteractionBlocker.Instance != null)
            {
                TutorialInteractionBlocker.Instance.OnBlockerClicked -= HandleClick;
            }
        }

        /// <summary>
        /// Inicia una secuencia de diálogo con la expresión y líneas indicadas.
        /// </summary>
        public void StartDialogue(MochiExpression expression, string[] lines, Action onComplete)
        {
            if (lines == null || lines.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            SetMochiExpression(expression);
            _currentLines = lines;
            _currentLineIndex = 0;
            _onDialogueComplete = onComplete;

            gameObject.SetActive(true);
            if (TutorialInteractionBlocker.Instance != null)
            {
                TutorialInteractionBlocker.Instance.SetBlocked(true);
            }

            ShowCurrentLine();
        }

        public void SetMochiExpression(MochiExpression expression)
        {
            if (MochiImage == null) return;

            Sprite targetSprite = expression switch
            {
                MochiExpression.BrazosCruzados => MochiBrazosCruzados,
                MochiExpression.Mostrando => MochiMostrando,
                MochiExpression.CinturaOjosCerrados => MochiCinturaOjosCerrados,
                _ => MochiBrazosCruzados
            };

            if (targetSprite != null)
            {
                MochiImage.sprite = targetSprite;
            }
            MochiImage.gameObject.SetActive(true);
        }

        private void ShowCurrentLine()
        {
            if (_currentLineIndex >= _currentLines.Length)
            {
                FinishDialogue();
                return;
            }

            _targetFullText = _currentLines[_currentLineIndex];
            AdaptDialogueBoxSize(_targetFullText.Length);

            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }

            _typewriterCoroutine = StartCoroutine(TypewriterRoutine(_targetFullText));
        }

        private void AdaptDialogueBoxSize(int characterCount)
        {
            if (DialogueBoxImage == null) return;

            RectTransform boxRT = DialogueBoxImage.rectTransform;
            RectTransform textRT = DialogueText != null ? DialogueText.rectTransform : null;

            if (characterCount <= 65 && BoxPequeno != null)
            {
                DialogueBoxImage.sprite = BoxPequeno;
                boxRT.sizeDelta = new Vector2(500f, 180f);
                if (textRT != null)
                {
                    textRT.offsetMin = new Vector2(30f, 25f);
                    textRT.offsetMax = new Vector2(-40f, -25f);
                    DialogueText.fontSize = 24;
                }
            }
            else if (characterCount <= 140 && BoxMediano != null)
            {
                DialogueBoxImage.sprite = BoxMediano;
                boxRT.sizeDelta = new Vector2(680f, 260f);
                if (textRT != null)
                {
                    textRT.offsetMin = new Vector2(40f, 30f);
                    textRT.offsetMax = new Vector2(-45f, -30f);
                    DialogueText.fontSize = 25;
                }
            }
            else
            {
                if (BoxGrande != null) DialogueBoxImage.sprite = BoxGrande;
                boxRT.sizeDelta = new Vector2(850f, 340f);
                if (textRT != null)
                {
                    textRT.offsetMin = new Vector2(50f, 35f);
                    textRT.offsetMax = new Vector2(-50f, -35f);
                    DialogueText.fontSize = 26;
                }
            }

            if (DialogueText != null)
            {
                DialogueText.color = new Color(0.96f, 0.96f, 0.98f);
            }

            DialogueBoxImage.gameObject.SetActive(true);

            // Reubicar la flecha de continuación al vértice inferior derecho de la caja activa
            if (ContinueArrowIndicator != null)
            {
                RectTransform arrowRT = ContinueArrowIndicator.GetComponent<RectTransform>();
                if (arrowRT != null)
                {
                    arrowRT.anchorMin = new Vector2(1f, 0f);
                    arrowRT.anchorMax = new Vector2(1f, 0f);
                    arrowRT.pivot = new Vector2(1f, 0f);
                    arrowRT.anchoredPosition = new Vector2(-30f, 25f);
                }
            }
        }

        private IEnumerator TypewriterRoutine(string fullText)
        {
            _isTyping = true;
            _isTextFullyDisplayed = false;

            if (ContinueArrowIndicator != null) ContinueArrowIndicator.SetActive(false);
            if (DialogueText != null) DialogueText.text = "";

            for (int i = 0; i < fullText.Length; i++)
            {
                if (DialogueText != null)
                {
                    DialogueText.text = fullText.Substring(0, i + 1);
                }

                // Sonido suave de tipeo cada 3 caracteres para no saturar
                if (i % 3 == 0 && AudioSource != null && TypingClip != null)
                {
                    AudioSource.PlayOneShot(TypingClip, 0.4f);
                }

                yield return new WaitForSecondsRealtime(TypingSpeed);
            }

            _isTyping = false;
            _isTextFullyDisplayed = true;
            if (ContinueArrowIndicator != null) ContinueArrowIndicator.SetActive(true);
        }

        /// <summary>
        /// Procesa la Regla de los Dos Clics (Decisión 6).
        /// - Clic 1 mientras escribe: Instant Reveal.
        /// - Clic 2 con texto completo: Avanza a la siguiente frase.
        /// </summary>
        public void HandleClick()
        {
            if (_isTyping)
            {
                // Clic 1: Instant Reveal sin avanzar de paso
                if (_typewriterCoroutine != null)
                {
                    StopCoroutine(_typewriterCoroutine);
                }
                if (DialogueText != null)
                {
                    DialogueText.text = _targetFullText;
                }
                _isTyping = false;
                _isTextFullyDisplayed = true;
                if (ContinueArrowIndicator != null) ContinueArrowIndicator.SetActive(true);
            }
            else if (_isTextFullyDisplayed)
            {
                // Clic 2: Avanzar de frase o terminar
                _currentLineIndex++;
                ShowCurrentLine();
            }
        }

        public void HideDialogue(bool keepMochiVisible = false)
        {
            if (DialogueBoxImage != null) DialogueBoxImage.gameObject.SetActive(false);
            if (DialogueText != null) DialogueText.text = "";
            if (ContinueArrowIndicator != null) ContinueArrowIndicator.SetActive(false);

            if (!keepMochiVisible && MochiImage != null)
            {
                MochiImage.gameObject.SetActive(false);
            }

            if (TutorialInteractionBlocker.Instance != null)
            {
                TutorialInteractionBlocker.Instance.SetBlocked(false);
            }
        }

        private void FinishDialogue()
        {
            _isTyping = false;
            _isTextFullyDisplayed = false;
            _onDialogueComplete?.Invoke();
        }
    }
}
