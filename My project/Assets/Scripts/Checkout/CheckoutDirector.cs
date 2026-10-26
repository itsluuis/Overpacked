using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LogiPack.Checkout
{
    /// <summary>
    /// Director general de la escena de Check-out (Tablas de Verdad).
    /// Coordina las 3 rondas de 3 expedientes (9 en total), clasificaciones por botón
    /// o atajos de teclado (1, 2, 3 / Q, W, E), estampación diegética y reporte final.
    /// </summary>
    public class CheckoutDirector : MonoBehaviour
    {
        public static CheckoutDirector Instance { get; private set; }

        [Header("Tríptico de Expedientes")]
        public CheckoutSheetUI[] Sheets = new CheckoutSheetUI[3];

        [Header("Botones de Clasificación Inferiores")]
        public Button TautologyButton;
        public Button ContradictionButton;
        public Button ContingencyButton;

        [Header("HUD de Progreso Superior Izquierdo")]
        public Text RoundText;
        public Text SheetProgressText;

        [Header("Modal de Resultados")]
        public CheckoutSummaryUI SummaryUI;

        [Header("Audio")]
        public AudioClip CorrectSFX;
        public AudioClip IncorrectSFX;

        // Estado interno de la sesión
        private Queue<CheckoutFormula> _sessionPool;
        private CheckoutFormula[] _currentRoundFormulas = new CheckoutFormula[3];
        private int _currentRound = 0;           // 0, 1, 2
        private int _currentSheetIndex = 0;       // 0, 1, 2
        private int _totalHits = 0;
        private int[] _roundHits = new int[3];
        private bool _isStamping = false;
        private bool _isGameOver = false;

        private void Awake()
        {
            Instance = this;
            UnityEngine.Application.runInBackground = true;
            LogiPack.Core.GameModeManager.ActiveMode = LogiPack.Core.GameSessionMode.Checkout;
        }

        private void Start()
        {
            // Configurar listeners de botones
            if (TautologyButton != null)
                TautologyButton.onClick.AddListener(() => OnSubmitClassification(PropositionEvaluation.Tautology));
            if (ContradictionButton != null)
                ContradictionButton.onClick.AddListener(() => OnSubmitClassification(PropositionEvaluation.Contradiction));
            if (ContingencyButton != null)
                ContingencyButton.onClick.AddListener(() => OnSubmitClassification(PropositionEvaluation.Contingency));

            // Iniciar baraja sin reemplazo de 9 expedientes estocásticos
            _sessionPool = CheckoutFormulaPool.GetShuffledSessionPool(9);
            _currentRound = 0;
            _totalHits = 0;
            _roundHits = new int[3];
            _isGameOver = false;

            StartRound(_currentRound);
        }

        private void Update()
        {
            if (_isGameOver || _isStamping) return;

            // Atajos de teclado diegéticos con New Input System de Unity 6
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame || kb.qKey.wasPressedThisFrame)
            {
                OnSubmitClassification(PropositionEvaluation.Tautology);
            }
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
            {
                OnSubmitClassification(PropositionEvaluation.Contradiction);
            }
            else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
            {
                OnSubmitClassification(PropositionEvaluation.Contingency);
            }
        }

        #region Flujo de Rondas y Hojas

        private void StartRound(int roundIdx)
        {
            _currentRound = roundIdx;
            _currentSheetIndex = 0;
            _isStamping = false;

            UpdateHUD();

            // Desplegar 3 nuevas fórmulas desde el pool
            for (int i = 0; i < 3; i++)
            {
                if (_sessionPool != null && _sessionPool.Count > 0)
                {
                    _currentRoundFormulas[i] = _sessionPool.Dequeue();
                }

                if (Sheets != null && i < Sheets.Length && Sheets[i] != null)
                {
                    Sheets[i].gameObject.SetActive(true);
                    int globalIndex = (_currentRound * 3) + i + 1;
                    Sheets[i].Setup(_currentRoundFormulas[i], globalIndex);
                    Sheets[i].SetFocused(i == 0);
                }
            }

            SetButtonsInteractable(true);
        }

        private void UpdateHUD()
        {
            if (RoundText != null)
            {
                RoundText.text = $"RONDA {_currentRound + 1} / 3";
            }
            if (SheetProgressText != null)
            {
                int globalIndex = (_currentRound * 3) + _currentSheetIndex + 1;
                SheetProgressText.text = $"EXPEDIENTE {globalIndex} / 9 (LOTE #{_currentSheetIndex + 1})";
            }
        }

        public void OnSubmitClassification(PropositionEvaluation chosen)
        {
            if (_isStamping || _isGameOver) return;
            if (Sheets == null || _currentSheetIndex >= Sheets.Length || Sheets[_currentSheetIndex] == null) return;

            var activeSheet = Sheets[_currentSheetIndex];
            var formula = activeSheet.CurrentFormula;
            if (formula == null) return;

            _isStamping = true;
            SetButtonsInteractable(false);

            bool isCorrect = (chosen == formula.Evaluation);
            if (isCorrect)
            {
                _totalHits++;
                _roundHits[_currentRound]++;
                if (LogiPack.Audio.AudioManager.Instance != null && CorrectSFX != null)
                {
                    LogiPack.Audio.AudioManager.Instance.PlaySFX(CorrectSFX);
                }
            }
            else
            {
                if (LogiPack.Audio.AudioManager.Instance != null && IncorrectSFX != null)
                {
                    LogiPack.Audio.AudioManager.Instance.PlaySFX(IncorrectSFX);
                }
            }

            Debug.Log($"[CheckoutDirector] Expediente #{(_currentRound * 3) + _currentSheetIndex + 1}: {formula.FormulaFBF} es {formula.Evaluation}. Respuesta del jugador: {chosen} -> {(isCorrect ? "ACIERTO" : "FALLO")}");

            // Estampar sello diegético con pausa de 0.4s y luego archivar
            activeSheet.PlayStamp(isCorrect, () =>
            {
                activeSheet.SlideOut(() =>
                {
                    AdvanceToNextSheet();
                });
            });
        }

        private void AdvanceToNextSheet()
        {
            _currentSheetIndex++;
            UpdateHUD();

            if (_currentSheetIndex < 3)
            {
                // Enfocar la siguiente hoja del lote
                for (int i = 0; i < Sheets.Length; i++)
                {
                    if (Sheets[i] != null && Sheets[i].gameObject.activeSelf)
                    {
                        Sheets[i].SetFocused(i == _currentSheetIndex);
                    }
                }

                _isStamping = false;
                SetButtonsInteractable(true);
            }
            else
            {
                // Fin de la ronda
                StartCoroutine(RoundEndTransition());
            }
        }

        private IEnumerator RoundEndTransition()
        {
            yield return new WaitForSecondsRealtime(0.35f);

            if (_currentRound < 2)
            {
                // Siguiente ronda (1 -> 2 -> 3)
                StartRound(_currentRound + 1);
            }
            else
            {
                // Partida completada (9 expedientes resueltos)
                _isGameOver = true;
                SetButtonsInteractable(false);

                if (SummaryUI != null)
                {
                    SummaryUI.ShowSummary(_totalHits, 9, _roundHits);
                }
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (TautologyButton != null) TautologyButton.interactable = interactable;
            if (ContradictionButton != null) ContradictionButton.interactable = interactable;
            if (ContingencyButton != null) ContingencyButton.interactable = interactable;
        }

        #endregion
    }
}
