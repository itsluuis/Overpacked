using System;
using System.Collections;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Gameplay;
using LogiPack.Logic;
using LogiPack.Managers;
using LogiPack.Orders;
using LogiPack.UI;
using UnityEngine;

namespace LogiPack.Tutorial
{
    public enum TutorialPhase
    {
        None,
        Phase1_IntroAndConjunction,    // p ∧ q
        Phase2_NegationAndDisjunction, // ~q ∨ p
        Phase3_Conditional,            // p → s
        Phase4A_SubBoxCreation,        // (p ∧ q) con despacho bloqueado
        Phase4B_NestedConditional,     // ((p ∧ q) → s)
        Phase5_BiconditionalAndWrapup, // ((p ∨ r) ↔ s)
        Completed
    }

    /// <summary>
    /// Director y secuenciador pedagógico del Tutorial (Decisiones 1 a 16).
    /// Coordina el flujo lineal de 5 fórmulas FBF, el diálogo con Mochi Miles,
    /// el bloqueo físico de la mesa y los resaltados en World Space.
    /// </summary>
    public class TutorialDirector : MonoBehaviour
    {
        public static TutorialDirector Instance { get; private set; }

        [Header("Referencias del Sistema")]
        public TutorialDialogueUI DialogueUI;
        public TutorialHighlightFX HighlightFX;
        public AssemblyWorkbenchManager Workbench;
        public PropositionalOrderManager OrderManager;
        public DayEndSummaryUI DayEndSummary;
        public EnvironmentManager EnvManager;

        [Header("Puntos de Interés para Resaltado (Transforms de la Mesa)")]
        public Transform DrawerP;
        public Transform DrawerNotP;
        public Transform DrawerQ;
        public Transform DrawerNotQ;
        public Transform DrawerS;
        public Transform DrawerR;
        public Transform Slot1;
        public Transform Slot2_Operator;
        public Transform Slot3;
        public Transform ResultSlot;
        public Transform DispatchZone;
        public Transform NotebookButtonHUD;

        private TutorialPhase _currentPhase = TutorialPhase.None;
        private int _totalEarnings = 0;
        private int _completedOrdersCount = 0;

        // Sub-estados dentro de la Fase 1
        private int _phase1Step = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            Application.runInBackground = true;
            if (OrderManager == null) OrderManager = PropositionalOrderManager.Instance ?? FindAnyObjectByType<PropositionalOrderManager>();
            if (OrderManager != null)
            {
                OrderManager.AutoSpawnNextOrder = false;
            }
        }

        private void Start()
        {
            if (DialogueUI == null) DialogueUI = TutorialDialogueUI.Instance ?? FindAnyObjectByType<TutorialDialogueUI>();
            if (HighlightFX == null) HighlightFX = TutorialHighlightFX.Instance ?? FindAnyObjectByType<TutorialHighlightFX>();
            if (Workbench == null) Workbench = AssemblyWorkbenchManager.Instance ?? FindAnyObjectByType<AssemblyWorkbenchManager>();
            if (OrderManager == null) OrderManager = PropositionalOrderManager.Instance ?? FindAnyObjectByType<PropositionalOrderManager>();
            if (DayEndSummary == null) DayEndSummary = FindAnyObjectByType<DayEndSummaryUI>();
            if (EnvManager == null) EnvManager = FindAnyObjectByType<EnvironmentManager>();

            // 1. Congelar avance del tiempo del día (Decisión 5 y 9)
            if (EnvManager != null)
            {
                EnvManager.IsDayPaused = true; // Reloj del día congelado en 06:00
            }

            // 2. Desactivar auto-generación de pedidos en OrderManager para controlar las 5 FBFs
            if (OrderManager != null)
            {
                OrderManager.AutoSpawnNextOrder = false;
                OrderManager.DispatchBlocked = false;
            }

            // 3. Suscribirse a eventos de la mesa y pedidos
            SubscribeEvents();

            // 4. Iniciar la Fase 1 tras 0.5 segundos de carga
            StartCoroutine(InitTutorialRoutine());
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (Workbench != null)
            {
                Workbench.OnOperandPlaced += HandleOperandPlaced;
                Workbench.OnOperatorSelected += HandleOperatorSelected;
                Workbench.OnResultBoxCreated += HandleResultBoxCreated;
            }

            if (OrderManager != null)
            {
                OrderManager.OnOrderDispatched += HandleOrderDispatched;
                OrderManager.OnOrderDispatchRejected += HandleOrderRejected;
            }
        }

        private void UnsubscribeEvents()
        {
            if (Workbench != null)
            {
                Workbench.OnOperandPlaced -= HandleOperandPlaced;
                Workbench.OnOperatorSelected -= HandleOperatorSelected;
                Workbench.OnResultBoxCreated -= HandleResultBoxCreated;
            }

            if (OrderManager != null)
            {
                OrderManager.OnOrderDispatched -= HandleOrderDispatched;
                OrderManager.OnOrderDispatchRejected -= HandleOrderRejected;
            }
        }

        private IEnumerator InitTutorialRoutine()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            StartPhase1_Intro();
        }

        #region FASE 1: Introducción y Conjunción Básica (p ∧ q)

        private void StartPhase1_Intro()
        {
            _currentPhase = TutorialPhase.Phase1_IntroAndConjunction;
            _phase1Step = 0;

            string[] introLines = new string[]
            {
                "¡Hola! Soy Mochi Miles, supervisora de LogiPack: Almacén Lógico.",
                "Estoy aquí para explicarte cómo funciona la mesa de armado y cómo completar tus pedidos usando lógica formal."
            };

            DialogueUI.StartDialogue(MochiExpression.BrazosCruzados, introLines, () =>
            {
                StartPhase1_Explanation();
            });
        }

        private void StartPhase1_Explanation()
        {
            // Inyectar el pedido 1: p ∧ q
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItem("f1_t3"));
            var qNode = PropositionalNode.CreateAtomic("q", false, FindItem("f2_t3"));
            var formula1 = PropositionalNode.CreateBinary(pNode, PropositionalOperator.And, qNode);
            var order1 = PropositionalOrder.CreateCustomOrder(1, formula1, 100);

            OrderManager.SetManualOrder(order1);

            string[] lines = new string[]
            {
                "A la izquierda y derecha tienes las estanterías con proposiciones atómicas. El Perno de Titanio es 'p' y el Tornillo es su negación '~p'.",
                "Arriba a la derecha tienes tu comanda activa con una Fórmula Bien Formada (FBF): p ∧ q.",
                "Para armar la fórmula, debes colocar las proposiciones en las casillas 1 o 3, y el operador en el medio. Además, por equivalencia lógica, q ∧ p produce el mismo resultado.",
                "¡Comencemos! Arrastra el Perno de Titanio (p) a la primera casilla de la zona de armado."
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                _phase1Step = 1; // Esperando 'p' en Ranura 1
                if (DrawerP != null) HighlightFX.PointAt(DrawerP, Vector3.zero, "Toma 'p'");
            });
        }

        #endregion

        #region FASE 2: Negación y Disyunción (~q ∨ p)

        private void StartPhase2()
        {
            _currentPhase = TutorialPhase.Phase2_NegationAndDisjunction;

            // Inyectar pedido 2: ~q ∨ p
            var notQ = PropositionalNode.CreateAtomic("q", true, FindItem("f2_t1"));
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItem("f1_t3"));
            var formula2 = PropositionalNode.CreateBinary(notQ, PropositionalOperator.Or, pNode);
            var order2 = PropositionalOrder.CreateCustomOrder(2, formula2, 120);

            OrderManager.SetManualOrder(order2);

            string[] lines = new string[]
            {
                "¡Excelente entrega! Ahora pasemos a tu segundo pedido: ~q ∨ p.",
                "Observa que te piden una negación (~q, Cable Simple) y una disyunción ('O').",
                "¡Intenta armarlo y despacharlo por tu cuenta!"
            };

            DialogueUI.StartDialogue(MochiExpression.CinturaOjosCerrados, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                HighlightFX.Hide();
            });
        }

        #endregion

        #region FASE 3: El Condicional (p → s)

        private void StartPhase3()
        {
            _currentPhase = TutorialPhase.Phase3_Conditional;

            // Inyectar pedido 3: p → s
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItem("f1_t3"));
            var sNode = PropositionalNode.CreateAtomic("s", false, FindItem("f3_t3"));
            var formula3 = PropositionalNode.CreateBinary(pNode, PropositionalOperator.Conditional, sNode);
            var order3 = PropositionalOrder.CreateCustomOrder(3, formula3, 140);

            OrderManager.SetManualOrder(order3);

            string[] lines = new string[]
            {
                "¡Muy bien! Ahora aprenderemos el Condicional: p → s ('Si p, entonces s').",
                "¡Cuidado aquí! En el condicional el orden de los operandos SÍ importa. 'p → s' no es equivalente a 's → p'.",
                "Coloca el antecedente (p) en la casilla 1, elige la flecha condicional (→) y coloca el consecuente (s) en la casilla 3."
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                if (Slot2_Operator != null) HighlightFX.PointAt(Slot2_Operator, Vector3.zero, "Elige '→'");
            });
        }

        #endregion

        #region FASE 4: Anidamiento de Sub-Cajas ((p ∧ q) → s)

        private void StartPhase4()
        {
            _currentPhase = TutorialPhase.Phase4A_SubBoxCreation;
            OrderManager.DispatchBlocked = true; // Decisión 12: Bloqueo de despacho prematuro

            // Inyectar pedido 4: ((p ∧ q) → s)
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItem("f1_t3"));
            var qNode = PropositionalNode.CreateAtomic("q", false, FindItem("f2_t3"));
            var subBox = PropositionalNode.CreateBinary(pNode, PropositionalOperator.And, qNode);
            var sNode = PropositionalNode.CreateAtomic("s", false, FindItem("f3_t3"));
            var formula4 = PropositionalNode.CreateBinary(subBox, PropositionalOperator.Conditional, sNode);
            var order4 = PropositionalOrder.CreateCustomOrder(4, formula4, 180);

            OrderManager.SetManualOrder(order4);

            string[] lines = new string[]
            {
                "¡Atención! Este es un pedido compuesto: ((p ∧ q) → s).",
                "¡Las cajas armadas pueden reusarse dentro de otras! Para este pedido, arma primero la sub-fórmula (p ∧ q) en la mesa.",
                "No te preocupes por el despacho aún; la cinta está bloqueada hasta que tengamos la caja completa."
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                if (Slot1 != null) HighlightFX.PointAt(Slot1, Vector3.zero, "Arma (p ∧ q)");
            });
        }

        private void TransitionToPhase4B(PropositionalBox subBox)
        {
            _currentPhase = TutorialPhase.Phase4B_NestedConditional;

            string[] lines = new string[]
            {
                "¡Perfecto! Ya tienes la caja resultante (p ∧ q).",
                "Ahora arrastra esa misma caja a la casilla 1 de la mesa para usarla como antecedente.",
                "Luego selecciona el condicional (→) y coloca el Cristal de Lab (s) en la casilla 3."
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                if (subBox != null && Slot1 != null)
                {
                    HighlightFX.PointAt(Slot1, Vector3.zero, "Mueve la caja aquí");
                }
            });
        }

        #endregion

        #region FASE 5: Explicación de Jornada y Reto Bicondicional ((p ∨ r) ↔ s)

        private void StartPhase5()
        {
            _currentPhase = TutorialPhase.Phase5_BiconditionalAndWrapup;
            OrderManager.DispatchBlocked = false;

            // Inyectar pedido 5: ((p ∨ r) ↔ s) (r = Tejido Ignífugo, Decisión 15)
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItem("f1_t3"));
            var rNode = PropositionalNode.CreateAtomic("r", false, FindItem("f4_t3"));
            var subOr = PropositionalNode.CreateBinary(pNode, PropositionalOperator.Or, rNode);
            var sNode = PropositionalNode.CreateAtomic("s", false, FindItem("f3_t3"));
            var formula5 = PropositionalNode.CreateBinary(subOr, PropositionalOperator.Biconditional, sNode);
            var order5 = PropositionalOrder.CreateCustomOrder(5, formula5, 200);

            OrderManager.SetManualOrder(order5);

            string[] lines = new string[]
            {
                "¡Has dominado el anidamiento! Antes de tu reto final, déjame explicarte cómo funciona la jornada real.",
                "En el modo normal, el reloj avanzará de 06:00 a 22:00, acumulando dinero con cada pedido según tu velocidad.",
                "Si en algún momento dudas sobre un conectivo, haz clic en el cuaderno verde a la derecha para abrir la Guía Rápida de Teoría.",
                "Tu reto final de graduación es: ((p ∨ r) ↔ s). Recuerda que 'r' es el Tejido Ignífugo en la estantería inferior. ¡Demuestra lo aprendido!"
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                HighlightFX.Hide();
            });
        }

        private void FinishTutorial()
        {
            _currentPhase = TutorialPhase.Completed;
            HighlightFX.Hide();

            string[] lines = new string[]
            {
                "¡Extraordinario trabajo! Has completado exitosamente todas las pruebas de lógica proposicional.",
                "Ahora estás 100% listo para enfrentar la jornada laboral completa en el almacén. ¡Felicitaciones!"
            };

            DialogueUI.StartDialogue(MochiExpression.CinturaOjosCerrados, lines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
                if (DayEndSummary != null)
                {
                    DayEndSummary.RenderTutorialSummary(_completedOrdersCount, _totalEarnings);
                }
            });
        }

        #endregion

        #region Manejo de Eventos

        private void HandleOperandPlaced(int slotIndex, PropositionalNode node)
        {
            if (_currentPhase == TutorialPhase.Phase1_IntroAndConjunction)
            {
                if (_phase1Step == 1 && slotIndex == 0)
                {
                    _phase1Step = 2; // 'p' colocado en casilla 1 -> pedir operador
                    if (Slot2_Operator != null) HighlightFX.PointAt(Slot2_Operator, Vector3.zero, "Abre el cajón ∧");
                }
                else if (_phase1Step == 3 && slotIndex == 2)
                {
                    _phase1Step = 4; // 'q' colocado en casilla 3 -> pedir despacho
                    if (DispatchZone != null) HighlightFX.PointAt(DispatchZone, Vector3.zero, "Despacha la caja");
                }
            }
        }

        private void HandleOperatorSelected(PropositionalOperator op)
        {
            if (_currentPhase == TutorialPhase.Phase1_IntroAndConjunction && _phase1Step == 2)
            {
                if (op == PropositionalOperator.And)
                {
                    _phase1Step = 3; // Operador elegido -> pedir 'q'
                    if (DrawerQ != null) HighlightFX.PointAt(DrawerQ, Vector3.zero, "Toma 'q'");
                }
            }
        }

        private void HandleResultBoxCreated(PropositionalBox box)
        {
            if (box == null) return;

            if (_currentPhase == TutorialPhase.Phase1_IntroAndConjunction && _phase1Step >= 3)
            {
                if (DispatchZone != null) HighlightFX.PointAt(DispatchZone, Vector3.zero, "Despacha la caja");
            }
            else if (_currentPhase == TutorialPhase.Phase4A_SubBoxCreation)
            {
                // Se completó la sub-fórmula (p ∧ q)
                if (box.FormulaNode != null && box.FormulaNode.NodeType == PropositionalNodeType.Binary && box.FormulaNode.Operator == PropositionalOperator.And)
                {
                    TransitionToPhase4B(box);
                }
            }
            else if (_currentPhase == TutorialPhase.Phase4B_NestedConditional)
            {
                // Se completó la fórmula compuesta final ((p ∧ q) → s)
                if (box.FormulaNode != null && box.FormulaNode.NodeType == PropositionalNodeType.Binary && box.FormulaNode.Operator == PropositionalOperator.Conditional)
                {
                    OrderManager.DispatchBlocked = false; // Desbloquear despacho
                    if (DispatchZone != null) HighlightFX.PointAt(DispatchZone, Vector3.zero, "¡Despacha la caja!");
                }
            }
        }

        private void HandleOrderDispatched(PropositionalOrder order, bool success)
        {
            if (success)
            {
                _completedOrdersCount++;
                _totalEarnings += order.Reward;
                HighlightFX.Hide();

                switch (_currentPhase)
                {
                    case TutorialPhase.Phase1_IntroAndConjunction:
                        StartPhase2();
                        break;
                    case TutorialPhase.Phase2_NegationAndDisjunction:
                        StartPhase3();
                        break;
                    case TutorialPhase.Phase3_Conditional:
                        StartPhase4();
                        break;
                    case TutorialPhase.Phase4B_NestedConditional:
                        StartPhase5();
                        break;
                    case TutorialPhase.Phase5_BiconditionalAndWrapup:
                        FinishTutorial();
                        break;
                }
            }
        }

        private void HandleOrderRejected(PropositionalBox box)
        {
            // Decisión 13: Red de seguridad pedagógica ante errores sin penalización
            string[] rescueLines = new string[]
            {
                "Esa caja no coincide con el pedido del post-it.",
                "Recuerda verificar bien las proposiciones y operadores, o consulta el cuaderno verde si tienes dudas. ¡Inténtalo de nuevo!"
            };

            DialogueUI.StartDialogue(MochiExpression.Mostrando, rescueLines, () =>
            {
                DialogueUI.HideDialogue(keepMochiVisible: false);
            });
        }

        #endregion

        private ItemData FindItem(string idFragment)
        {
            if (OrderManager != null && OrderManager.AllItems != null)
            {
                return OrderManager.AllItems.Find(x => (x.ItemId != null && x.ItemId.ToLowerInvariant().Contains(idFragment)) ||
                                                       (x.name != null && x.name.ToLowerInvariant().Contains(idFragment)));
            }
            return null;
        }
    }
}
