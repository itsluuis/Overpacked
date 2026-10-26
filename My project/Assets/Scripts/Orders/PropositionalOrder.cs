using System;
using System.Collections.Generic;
using LogiPack.Core;
using LogiPack.Logic;
using LogiPack.Gameplay;
using UnityEngine;

namespace LogiPack.Orders
{
    [Serializable]
    public class PropositionalOrder
    {
        public int OrderId;
        [System.NonSerialized] public PropositionalNode FormulaNode;
        public int Reward;
        public string FBFString;
        public string NaturalLanguage;

        // Soporte para Teoría de Conjuntos (50% de probabilidad en el almacén)
        public bool IsSetTheoryOrder = false;
        public SetTheoryRule SetRule;
        public int TargetItemCount = 2;
        public string CompanyName;

        public bool MatchesBox(PropositionalBox box)
        {
            if (box == null || box.FormulaNode == null) return false;

            if (!IsSetTheoryOrder)
            {
                return FormulaNode != null && box.FormulaNode.IsEquivalentTo(FormulaNode);
            }

            // Para orden de teoría de conjuntos: extraer items atómicos del árbol de la caja
            var items = new List<ItemData>();
            box.FormulaNode.CollectItems(items);
            if (SetRule == null) return false;

            int matchCount = 0;
            foreach (var it in items)
            {
                if (SetRule.Matches(it)) matchCount++;
            }
            return matchCount >= TargetItemCount;
        }

        public static PropositionalOrder GenerateRandomOrder(int orderId, List<ItemData> allItems)
        {
            // 50% de probabilidad: Lote de Empresa con Regla de Teoría de Conjuntos
            if (UnityEngine.Random.value < 0.50f)
            {
                var rule = OrderGenerator.GenerateRandomSetRule();
                string compName = OrderGenerator.CompanyNames[UnityEngine.Random.Range(0, OrderGenerator.CompanyNames.Length)];
                int targetCount = UnityEngine.Random.Range(2, 4); // 2 o 3 ítems
                int setReward = UnityEngine.Random.Range(120, 161);

                return new PropositionalOrder
                {
                    OrderId = orderId,
                    IsSetTheoryOrder = true,
                    SetRule = rule,
                    TargetItemCount = targetCount,
                    CompanyName = compName,
                    Reward = setReward,
                    FBFString = rule.FormalFormula,
                    NaturalLanguage = $"Empacar {targetCount}x {rule.NaturalLanguageDescription}"
                };
            }

            // 50% restante: Lógica Proposicional Clásica
            // Mapear items a variables mediante ItemId
            // p = Titanio (f1_t3), q = Litio (f2_t3), s = Cristal (f3_t3), r = Ignífugo (f4_t3)
            // ~p = Tornillo (f1_t1), ~q = Cable (f2_t1), ~s = Vidrio (f3_t1), ~r = Algodón (f4_t1)
            var pNode = PropositionalNode.CreateAtomic("p", false, FindItemById(allItems, "f1_t3"));
            var qNode = PropositionalNode.CreateAtomic("q", false, FindItemById(allItems, "f2_t3"));
            var sNode = PropositionalNode.CreateAtomic("s", false, FindItemById(allItems, "f3_t3"));
            var rNode = PropositionalNode.CreateAtomic("r", false, FindItemById(allItems, "f4_t3"));

            var notPNode = PropositionalNode.CreateAtomic("p", true, FindItemById(allItems, "f1_t1"));
            var notQNode = PropositionalNode.CreateAtomic("q", true, FindItemById(allItems, "f2_t1"));
            var notSNode = PropositionalNode.CreateAtomic("s", true, FindItemById(allItems, "f3_t1"));
            var notRNode = PropositionalNode.CreateAtomic("r", true, FindItemById(allItems, "f4_t1"));

            PropositionalNode[] atomicPool = new PropositionalNode[]
            {
                pNode, qNode, sNode, rNode,
                notPNode, notQNode, notSNode, notRNode
            };

            PropositionalOperator[] opPool = new PropositionalOperator[]
            {
                PropositionalOperator.And,
                PropositionalOperator.Or,
                PropositionalOperator.Conditional,
                PropositionalOperator.Biconditional
            };

            int pattern = UnityEngine.Random.Range(0, 10);
            PropositionalNode rootNode;
            int reward = 100;

            if (pattern == 0)
            {
                // Término atómico o negado directo (ej. p, o ~s)
                rootNode = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                reward = 80;
            }
            else if (pattern <= 4)
            {
                // Operación Binaria Simple: A [op] B (ej. ~q ∧ p, p → s)
                var left = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                var right = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                var op = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                rootNode = PropositionalNode.CreateBinary(left, op, right);
                reward = 120;
            }
            else if (pattern <= 7)
            {
                // Operación Binaria con Subfórmula Izquierda: (A [op1] B) [op2] C
                var a = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                var b = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                var op1 = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                var subLeft = PropositionalNode.CreateBinary(a, op1, b);

                var c = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                var op2 = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                rootNode = PropositionalNode.CreateBinary(subLeft, op2, c);
                reward = 160;
            }
            else
            {
                // Operación con Negación Externa o Derecha: A [op1] (B [op2] C) o ~(A [op] B)
                if (UnityEngine.Random.value < 0.5f)
                {
                    var a = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                    var b = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                    var op = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                    rootNode = PropositionalNode.CreateUnary(PropositionalNode.CreateBinary(a, op, b));
                    reward = 150;
                }
                else
                {
                    var a = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                    var b = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                    var c = atomicPool[UnityEngine.Random.Range(0, atomicPool.Length)];
                    var op1 = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                    var op2 = opPool[UnityEngine.Random.Range(0, opPool.Length)];
                    rootNode = PropositionalNode.CreateBinary(a, op1, PropositionalNode.CreateBinary(b, op2, c));
                    reward = 170;
                }
            }

            return new PropositionalOrder
            {
                OrderId = orderId,
                FormulaNode = rootNode,
                Reward = reward,
                FBFString = rootNode.ToFormulaString(false),
                NaturalLanguage = rootNode.ToNaturalLanguage()
            };
        }

        public static PropositionalOrder CreateCustomOrder(int orderId, PropositionalNode formula, int reward)
        {
            return new PropositionalOrder
            {
                OrderId = orderId,
                FormulaNode = formula,
                Reward = reward,
                FBFString = formula != null ? formula.ToFormulaString(false) : "",
                NaturalLanguage = formula != null ? formula.ToNaturalLanguage() : ""
            };
        }

        private static ItemData FindItemById(List<ItemData> items, string idFragment)
        {
            if (items == null) return null;
            return items.Find(x => (x.ItemId != null && x.ItemId.ToLowerInvariant().Contains(idFragment)) ||
                                   (x.name != null && x.name.ToLowerInvariant().Contains(idFragment)));
        }
    }
}
