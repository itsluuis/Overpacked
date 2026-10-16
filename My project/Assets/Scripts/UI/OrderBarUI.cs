using System.Collections.Generic;
using LogiPack.Managers;
using LogiPack.Orders;
using UnityEngine;

namespace LogiPack.UI
{
    /// <summary>
    /// Sincroniza dinámicamente los pedidos activos de OrderManager
    /// con las tarjetas visuales (OrderCardUI) en la barra superior de la pantalla.
    /// </summary>
    public class OrderBarUI : MonoBehaviour
    {
        [Header("Referencias")]
        public OrderManager OrdersManager;
        public GameObject OrderCardPrefab;
        public Transform CardsContainer;

        private readonly Dictionary<Order, OrderCardUI> _activeCards = new Dictionary<Order, OrderCardUI>();

        private void Update()
        {
            if (OrdersManager == null || OrderCardPrefab == null || CardsContainer == null) return;

            // 1. Crear tarjetas para nuevos pedidos
            foreach (var order in OrdersManager.ActiveOrders)
            {
                if (!_activeCards.ContainsKey(order))
                {
                    GameObject cardGO = Instantiate(OrderCardPrefab, CardsContainer);
                    var cardUI = cardGO.GetComponent<OrderCardUI>();
                    if (cardUI != null)
                    {
                        cardUI.BindOrder(order);
                        _activeCards[order] = cardUI;
                    }
                }
            }

            // 2. Eliminar tarjetas de pedidos caducados o despachados
            var toRemove = new List<Order>();
            foreach (var kvp in _activeCards)
            {
                if (!OrdersManager.ActiveOrders.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var order in toRemove)
            {
                if (_activeCards.TryGetValue(order, out var cardUI))
                {
                    if (cardUI != null) Destroy(cardUI.gameObject);
                    _activeCards.Remove(order);
                }
            }
        }
    }
}
