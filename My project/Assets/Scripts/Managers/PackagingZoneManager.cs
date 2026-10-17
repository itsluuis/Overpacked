using System;
using System.Collections.Generic;
using LogiPack.Gameplay;
using LogiPack.Logic;
using LogiPack.Orders;
using UnityEngine;

namespace LogiPack.Managers
{
    /// <summary>
    /// Gestiona las 5 ranuras de embalaje y el despacho automático inteligente
    /// evaluando en tiempo real las condiciones proposicionales y los items.
    /// </summary>
    public class PackagingZoneManager : MonoBehaviour
    {
        public const int TOTAL_SLOTS = 5;

        [Header("Referencias a Ranuras (5 slots)")]
        public Transform[] SlotTransforms = new Transform[TOTAL_SLOTS];

        private readonly PackageBox[] _slots = new PackageBox[TOTAL_SLOTS];

        public event Action<PackageBox, Order> OnOrderDispatched;

        public bool PlaceBoxInSlot(PackageBox box, int targetSlot)
        {
            if (targetSlot < 0 || targetSlot >= TOTAL_SLOTS) return false;
            if (_slots[targetSlot] != null) return false;

            _slots[targetSlot] = box;
            box.SlotIndex = targetSlot;
            box.SetState(BoxState.InPackagingSlot);
            box.transform.position = SlotTransforms[targetSlot].position;
            return true;
        }

        public void RemoveBoxFromSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < TOTAL_SLOTS)
            {
                if (_slots[slotIndex] != null)
                {
                    _slots[slotIndex].SlotIndex = -1;
                    _slots[slotIndex] = null;
                }
            }
        }

        /// <summary>
        /// Comprueba las 5 ranuras frente a los pedidos activos y despacha la caja que coincida
        /// con la orden de menor tiempo restante cuyas condiciones lógicas sean Verdaderas.
        /// </summary>
        public void EvaluateAutoDispatch(EnvironmentState envState, List<Order> activeOrders)
        {
            if (activeOrders == null || activeOrders.Count == 0) return;

            for (int i = 0; i < TOTAL_SLOTS; i++)
            {
                PackageBox box = _slots[i];
                if (box == null || box.IsEmpty) continue;

                // Buscar entre las órdenes activas ordenadas por urgencia (menor tiempo restante)
                Order candidateOrder = null;
                float minTime = float.MaxValue;

                foreach (var order in activeOrders)
                {
                    // 1. Debe coincidir la lista de items
                    if (!order.MatchesItems(box.Contents)) continue;

                    // 2. La fórmula ambiental y de embalaje debe ser Verdadera en este instante
                    if (!order.Condition.Evaluate(envState, box)) continue;

                    // 3. Tomar el más urgente
                    if (order.RemainingTime < minTime)
                    {
                        minTime = order.RemainingTime;
                        candidateOrder = order;
                    }
                }

                // Si encontramos coincidencia, se despacha automáticamente
                if (candidateOrder != null)
                {
                    Debug.Log($"[PackagingZone] ¡DESPACHO AUTOMÁTICO! Ranura {i} despachó orden '{candidateOrder.OrderId}'.");
                    _slots[i] = null;
                    box.SlotIndex = -1;
                    box.SetState(BoxState.Dispatched);
                    OnOrderDispatched?.Invoke(box, candidateOrder);
                    Destroy(box.gameObject);
                }
            }
        }
    }
}
