using System;
using System.Collections.Generic;
using LogiPack.Core;
using UnityEngine;

namespace LogiPack.Logic
{
    public enum PropositionalNodeType
    {
        Atomic,
        Unary,
        Binary
    }

    [Serializable]
    public class PropositionalNode
    {
        public PropositionalNodeType NodeType;

        // Propiedades atómicas
        public string VariableName;     // "p", "q", "s", "r"
        public bool IsNegated;          // true si es ~p, ~q, etc.
        public ItemData AssociatedItem; // Referencia al ScriptableObject

        // Propiedades unarias
        [System.NonSerialized] public PropositionalNode InnerNode;

        // Propiedades binarias
        [System.NonSerialized] public PropositionalNode Left;
        public PropositionalOperator Operator;
        [System.NonSerialized] public PropositionalNode Right;

        // Firma calculada de la tabla de verdad (16 bits)
        [SerializeField] private ushort _truthSignature;
        [SerializeField] private bool _signatureCalculated;

        public ushort Signature
        {
            get
            {
                if (!_signatureCalculated)
                {
                    _truthSignature = CalculateSignature();
                    _signatureCalculated = true;
                }
                return _truthSignature;
            }
        }

        #region Constructores / Fábricas

        public static PropositionalNode CreateAtomic(string variable, bool isNegated, ItemData item = null)
        {
            var node = new PropositionalNode
            {
                NodeType = PropositionalNodeType.Atomic,
                VariableName = variable.ToLowerInvariant(),
                IsNegated = isNegated,
                AssociatedItem = item
            };
            node._truthSignature = node.CalculateSignature();
            node._signatureCalculated = true;
            return node;
        }

        public static PropositionalNode CreateUnary(PropositionalNode inner)
        {
            if (inner == null) return null;

            var node = new PropositionalNode
            {
                NodeType = PropositionalNodeType.Unary,
                InnerNode = inner
            };
            node._truthSignature = node.CalculateSignature();
            node._signatureCalculated = true;
            return node;
        }

        public static PropositionalNode CreateBinary(PropositionalNode left, PropositionalOperator op, PropositionalNode right)
        {
            if (left == null || right == null) return null;

            var node = new PropositionalNode
            {
                NodeType = PropositionalNodeType.Binary,
                Left = left,
                Operator = op,
                Right = right
            };
            node._truthSignature = node.CalculateSignature();
            node._signatureCalculated = true;
            return node;
        }

        #endregion

        #region Evaluación Booleana

        public bool Evaluate(bool pVal, bool qVal, bool sVal, bool rVal)
        {
            switch (NodeType)
            {
                case PropositionalNodeType.Atomic:
                    bool baseVal = false;
                    switch (VariableName)
                    {
                        case "p": baseVal = pVal; break;
                        case "q": baseVal = qVal; break;
                        case "s": baseVal = sVal; break;
                        case "r": baseVal = rVal; break;
                    }
                    return IsNegated ? !baseVal : baseVal;

                case PropositionalNodeType.Unary:
                    return InnerNode != null ? !InnerNode.Evaluate(pVal, qVal, sVal, rVal) : false;

                case PropositionalNodeType.Binary:
                    if (Left == null || Right == null) return false;
                    bool l = Left.Evaluate(pVal, qVal, sVal, rVal);
                    bool r = Right.Evaluate(pVal, qVal, sVal, rVal);
                    return Operator.Evaluate(l, r);

                default:
                    return false;
            }
        }

        public ushort CalculateSignature()
        {
            ushort sig = 0;
            // 16 combinaciones: p (bit 0), q (bit 1), s (bit 2), r (bit 3)
            for (int i = 0; i < 16; i++)
            {
                bool p = (i & 1) != 0;
                bool q = (i & 2) != 0;
                bool s = (i & 4) != 0;
                bool r = (i & 8) != 0;

                if (Evaluate(p, q, s, r))
                {
                    sig |= (ushort)(1 << i);
                }
            }
            return sig;
        }

        public bool IsEquivalentTo(PropositionalNode other)
        {
            if (other == null) return false;
            return this.Signature == other.Signature;
        }

        #endregion

        #region Formateo a Texto

        public string ToFormulaString(bool outerParens = false)
        {
            switch (NodeType)
            {
                case PropositionalNodeType.Atomic:
                    return IsNegated ? $"~{VariableName}" : VariableName;

                case PropositionalNodeType.Unary:
                    if (InnerNode == null) return "~(?)";
                    if (InnerNode.NodeType == PropositionalNodeType.Atomic && !InnerNode.IsNegated)
                    {
                        return $"~{InnerNode.VariableName}";
                    }
                    return $"~({InnerNode.ToFormulaString(false)})";

                case PropositionalNodeType.Binary:
                    if (Left == null || Right == null) return "(?)";
                    string lStr = Left.ToFormulaString(true);
                    string rStr = Right.ToFormulaString(true);
                    string opStr = Operator.GetSymbol();
                    string res = $"{lStr} {opStr} {rStr}";
                    return outerParens ? $"({res})" : res;

                default:
                    return "?";
            }
        }

        public string ToNaturalLanguage()
        {
            switch (NodeType)
            {
                case PropositionalNodeType.Atomic:
                    string name = AssociatedItem != null ? AssociatedItem.ItemName : VariableName.ToUpper();
                    return IsNegated ? $"no {name.ToLower()}" : name;

                case PropositionalNodeType.Unary:
                    if (InnerNode == null) return "no (...)";
                    return $"no ({InnerNode.ToNaturalLanguage()})";

                case PropositionalNodeType.Binary:
                    if (Left == null || Right == null) return "...";
                    string lText = Left.ToNaturalLanguage();
                    string rText = Right.ToNaturalLanguage();
                    if (Operator == PropositionalOperator.Conditional)
                    {
                        return $"si {lText}, entonces {rText}";
                    }
                    return $"{lText}{Operator.GetNaturalLanguage()}{rText}";

                default:
                    return "";
            }
        }

        public void CollectIcons(List<Sprite> icons, Sprite boxSprite = null)
        {
            if (NodeType == PropositionalNodeType.Atomic)
            {
                if (AssociatedItem != null && AssociatedItem.Icon != null)
                    icons.Add(AssociatedItem.Icon);
            }
            else if (NodeType == PropositionalNodeType.Unary)
            {
                if (InnerNode != null)
                    InnerNode.CollectIcons(icons, boxSprite);
            }
            else if (NodeType == PropositionalNodeType.Binary)
            {
                if (Left != null)
                {
                    if (Left.NodeType == PropositionalNodeType.Binary && boxSprite != null)
                        icons.Add(boxSprite);
                    else
                        Left.CollectIcons(icons, boxSprite);
                }
                if (Right != null)
                {
                    if (Right.NodeType == PropositionalNodeType.Binary && boxSprite != null)
                        icons.Add(boxSprite);
                    else
                        Right.CollectIcons(icons, boxSprite);
                }
            }
        }

        public void CollectItems(List<ItemData> items)
        {
            if (items == null) return;

            if (NodeType == PropositionalNodeType.Atomic)
            {
                if (AssociatedItem != null)
                    items.Add(AssociatedItem);
            }
            else if (NodeType == PropositionalNodeType.Unary)
            {
                if (InnerNode != null)
                    InnerNode.CollectItems(items);
            }
            else if (NodeType == PropositionalNodeType.Binary)
            {
                if (Left != null) Left.CollectItems(items);
                if (Right != null) Right.CollectItems(items);
            }
        }

        public override string ToString()
        {
            return ToFormulaString();
        }

        #endregion
    }
}
