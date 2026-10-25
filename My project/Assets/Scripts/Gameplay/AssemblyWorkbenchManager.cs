using System;
using LogiPack.Gameplay;
using LogiPack.Logic;
using LogiPack.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.Gameplay
{
    public class AssemblyWorkbenchManager : MonoBehaviour
    {
        public static AssemblyWorkbenchManager Instance { get; private set; }

        [Header("Ranuras de la Zona 1x3")]
        public Transform Slot1_Transform;
        public Transform Slot2_Operator_Transform;
        public Transform Slot3_Transform;
        public Transform ResultSlot_Transform;

        [Header("Visuales de Ranura")]
        public Text OperatorSymbolText;
        public GameObject OperatorDrawer1x4; // Objeto con el sprite cajon_1x4.png

        [Header("Prefab de Caja Resultante")]
        public GameObject PropositionalBoxPrefab;
        public Sprite DefaultBoxSprite;

        // Estado lógico actual de la mesa
        public PropositionalNode OperandLeft { get; private set; }
        public PropositionalNode OperandRight { get; private set; }
        public PropositionalOperator CurrentOperator = PropositionalOperator.And;

        // Representaciones visuales de los operandos en mesa
        private GameObject _visualOperandLeft;
        private GameObject _visualOperandRight;
        private PropositionalBox _activeResultBox;

        [Header("Control de Interacción / Modo Tutorial")]
        public bool IsInteractionsBlocked = false;

        public event Action<int, PropositionalNode> OnOperandPlaced;
        public event Action<PropositionalOperator> OnOperatorSelected;
        public event Action<PropositionalBox> OnResultBoxCreated;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (OperatorDrawer1x4 != null)
            {
                OperatorDrawer1x4.SetActive(false);
            }
            UpdateOperatorVisual();
            UpdateAssemblyState();
        }

        #region Manejo de Operandos

        public void SetOperand(int slotIndex, PropositionalNode node, Sprite icon = null)
        {
            if (slotIndex == 0)
            {
                OperandLeft = node;
                UpdateOperandVisual(0, icon, node);
            }
            else if (slotIndex == 2)
            {
                OperandRight = node;
                UpdateOperandVisual(2, icon, node);
            }

            UpdateAssemblyState();
            OnOperandPlaced?.Invoke(slotIndex, node);
        }

        public void ClearOperand(int slotIndex)
        {
            if (slotIndex == 0)
            {
                OperandLeft = null;
                if (_visualOperandLeft != null) Destroy(_visualOperandLeft);
            }
            else if (slotIndex == 2)
            {
                OperandRight = null;
                if (_visualOperandRight != null) Destroy(_visualOperandRight);
            }

            UpdateAssemblyState();
        }

        public void SwapOrMoveOperand(int fromSlot, int toSlot)
        {
            if (fromSlot == toSlot) return;

            var fromNode = (fromSlot == 0) ? OperandLeft : OperandRight;
            var toNode = (toSlot == 0) ? OperandLeft : OperandRight;

            // Intercambiar
            if (fromSlot == 0)
            {
                OperandLeft = toNode;
                OperandRight = fromNode;
            }
            else
            {
                OperandRight = toNode;
                OperandLeft = fromSlot == 0 ? toNode : fromNode;
            }

            RefreshVisuals();
            UpdateAssemblyState();
        }

        public bool TryPlaceBoxInSlot(PropositionalBox box, int slotIndex)
        {
            if (box == null || box.FormulaNode == null) return false;

            if (box.NestingDepth >= 3)
            {
                FloatingFeedbackFX.Show(box.transform.position, "Límite de anidamiento (máx. 3)", Color.yellow);
                return false;
            }

            // Si la caja colocada es la que estaba en el resultado, liberamos la referencia
            if (box == _activeResultBox)
            {
                _activeResultBox = null;
            }

            if (slotIndex == 0)
            {
                OperandLeft = box.FormulaNode;
                OperandRight = null; // Como acordado en el diseño
                if (_visualOperandRight != null) Destroy(_visualOperandRight);
                UpdateOperandVisual(0, DefaultBoxSprite, box.FormulaNode);
            }
            else if (slotIndex == 2)
            {
                OperandRight = box.FormulaNode;
                OperandLeft = null;
                if (_visualOperandLeft != null) Destroy(_visualOperandLeft);
                UpdateOperandVisual(2, DefaultBoxSprite, box.FormulaNode);
            }

            box.DestroyBox();
            UpdateAssemblyState();
            return true;
        }

        private void UpdateOperandVisual(int slotIndex, Sprite icon, PropositionalNode node)
        {
            Transform parentSlot = (slotIndex == 0) ? Slot1_Transform : Slot3_Transform;
            if (parentSlot == null) return;

            ref GameObject visualObj = ref (slotIndex == 0 ? ref _visualOperandLeft : ref _visualOperandRight);
            if (visualObj != null) Destroy(visualObj);

            visualObj = new GameObject($"PlacedItem_Slot_{slotIndex}");
            visualObj.transform.SetParent(parentSlot, false);
            visualObj.transform.localPosition = Vector3.zero;
            bool isBox = (icon == DefaultBoxSprite);
            visualObj.transform.localScale = Vector3.one * (isBox ? 0.11f : 0.09f);

            var sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = icon;
            sr.sortingOrder = 12;

            var col = visualObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(10f, 10f);

            var draggable = visualObj.AddComponent<AssemblySlotDraggable>();
            draggable.SlotIndex = slotIndex;
            draggable.Workbench = this;

            // Si es una subfórmula (caja anidada), añadir etiqueta de texto
            if (node != null && (node.NodeType == PropositionalNodeType.Binary || node.NodeType == PropositionalNodeType.Unary))
            {
                CreateMiniLabel(visualObj.transform, node.ToFormulaString());
            }
        }

        private void CreateMiniLabel(Transform parent, string text)
        {
            var textGO = new GameObject("MiniLabel");
            textGO.transform.SetParent(parent, false);
            textGO.transform.localPosition = new Vector3(0f, -4.5f, 0f);

            var canvas = textGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 14;

            var rt = textGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 60);
            rt.localScale = Vector3.one * 0.08f;

            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.fontSize = 24;
            textComp.color = new Color(0.95f, 0.95f, 0.95f);

            Font fredoka = Resources.Load<Font>("Fredoka-Bold");
            if (fredoka != null) textComp.font = fredoka;
        }

        private void RefreshVisuals()
        {
            if (OperandLeft != null)
            {
                UpdateOperandVisual(0, OperandLeft.AssociatedItem != null ? OperandLeft.AssociatedItem.Icon : DefaultBoxSprite, OperandLeft);
            }
            else if (_visualOperandLeft != null)
            {
                Destroy(_visualOperandLeft);
            }

            if (OperandRight != null)
            {
                UpdateOperandVisual(2, OperandRight.AssociatedItem != null ? OperandRight.AssociatedItem.Icon : DefaultBoxSprite, OperandRight);
            }
            else if (_visualOperandRight != null)
            {
                Destroy(_visualOperandRight);
            }
        }

        #endregion

        #region Operador y Cajón 1x4

        public void ToggleOperatorDrawer()
        {
            if (OperatorDrawer1x4 != null)
            {
                OperatorDrawer1x4.SetActive(!OperatorDrawer1x4.activeSelf);
            }
        }

        public void SelectOperator(PropositionalOperator op)
        {
            CurrentOperator = op;
            if (OperatorDrawer1x4 != null)
            {
                OperatorDrawer1x4.SetActive(false); // Cierre automático tras selección
            }
            UpdateOperatorVisual();
            UpdateAssemblyState();
            OnOperatorSelected?.Invoke(op);
        }

        public void UpdateOperatorVisual()
        {
            if (OperatorSymbolText == null) return;

            // Ajuste visual acordado:
            // Si Ranura 1 está vacía y Ranura 3 ocupada, muestra "~" (Modo Negador Unario)
            if (OperandLeft == null && OperandRight != null)
            {
                OperatorSymbolText.text = "~";
                OperatorSymbolText.color = new Color(1f, 0.45f, 0.45f); // Realce sutil rojizo de negación
            }
            else
            {
                OperatorSymbolText.text = CurrentOperator.GetSymbol();
                OperatorSymbolText.color = Color.white;
            }
        }

        #endregion

        #region Ensamblaje Reactivo y Caja de Resultado

        public void UpdateAssemblyState()
        {
            UpdateOperatorVisual();

            PropositionalNode resultNode = null;
            int depth = 1;

            if (OperandLeft != null && OperandRight == null)
            {
                // Término afirmativo / directo
                resultNode = OperandLeft;
                depth = 1;
            }
            else if (OperandLeft == null && OperandRight != null)
            {
                // Modo Negador Unario: ~(B)
                resultNode = PropositionalNode.CreateUnary(OperandRight);
                depth = 1;
            }
            else if (OperandLeft != null && OperandRight != null)
            {
                // Operación Binaria: (A [Op] B)
                resultNode = PropositionalNode.CreateBinary(OperandLeft, CurrentOperator, OperandRight);
                depth = 2;
            }

            if (resultNode != null)
            {
                SpawnOrUpdateResultBox(resultNode, depth);
            }
            else
            {
                ClearResultBox();
            }
        }

        private void SpawnOrUpdateResultBox(PropositionalNode node, int depth)
        {
            if (ResultSlot_Transform == null) return;

            if (_activeResultBox == null)
            {
                GameObject boxGO;
                if (PropositionalBoxPrefab != null)
                {
                    boxGO = Instantiate(PropositionalBoxPrefab, ResultSlot_Transform.position, Quaternion.identity);
                    boxGO.transform.localScale = Vector3.one * 0.11f;
                }
                else
                {
                    boxGO = new GameObject("PropositionalResultBox");
                    boxGO.transform.position = ResultSlot_Transform.position;
                    boxGO.transform.localScale = Vector3.one * 0.11f;

                    var sr = boxGO.AddComponent<SpriteRenderer>();
                    sr.sprite = DefaultBoxSprite;
                    sr.sortingOrder = 15;

                    var col = boxGO.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(9f, 9f);

                    var pbox = boxGO.AddComponent<PropositionalBox>();
                    pbox.DefaultBoxSprite = DefaultBoxSprite;

                    // Crear World-Space Canvas para la etiqueta
                    var canvasGO = new GameObject("FormulaLabelCanvas");
                    canvasGO.transform.SetParent(boxGO.transform, false);
                    canvasGO.transform.localPosition = new Vector3(0f, -2.5f, 0f);

                    var canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.sortingOrder = 19;

                    var rt = canvasGO.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(350, 70);
                    rt.localScale = Vector3.one * 0.08f;

                    var textComp = canvasGO.AddComponent<Text>();
                    textComp.alignment = TextAnchor.MiddleCenter;
                    textComp.fontSize = 28;
                    textComp.color = new Color(0.12f, 0.12f, 0.12f); // Negro carbón

                    Font fredoka = Resources.Load<Font>("Fredoka-Bold");
                    if (fredoka != null) textComp.font = fredoka;

                    pbox.FormulaLabel = textComp;
                }

                _activeResultBox = boxGO.GetComponent<PropositionalBox>();
                _activeResultBox.SetHomePosition(ResultSlot_Transform.position);
                _activeResultBox.OnBoxDestroyed += OnActiveBoxDestroyed;
            }

            _activeResultBox.SetFormula(node, depth);
            _activeResultBox.SetHomePosition(ResultSlot_Transform.position);
            _activeResultBox.gameObject.SetActive(true);
            OnResultBoxCreated?.Invoke(_activeResultBox);
        }

        public void OnResultBoxPickedUp(PropositionalBox box)
        {
            if (_activeResultBox == box)
            {
                _activeResultBox = null;
                OperandLeft = null;
                OperandRight = null;
                if (_visualOperandLeft != null)
                {
                    Destroy(_visualOperandLeft);
                    _visualOperandLeft = null;
                }
                if (_visualOperandRight != null)
                {
                    Destroy(_visualOperandRight);
                    _visualOperandRight = null;
                }

                UpdateOperatorVisual();
            }
        }

        private void OnActiveBoxDestroyed(PropositionalBox box)
        {
            if (_activeResultBox == box)
            {
                _activeResultBox = null;
                ClearOperand(0);
                ClearOperand(2);
            }
        }

        private void ClearResultBox()
        {
            if (_activeResultBox != null)
            {
                Destroy(_activeResultBox.gameObject);
                _activeResultBox = null;
            }
        }

        #endregion
    }
}
