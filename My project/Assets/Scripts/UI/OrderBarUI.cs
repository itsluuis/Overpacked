using System.Collections.Generic;
using LogiPack.Managers;
using LogiPack.Orders;
using UnityEngine;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Sincroniza dinámicamente los pedidos activos de OrderManager
    /// con las tarjetas visuales (OrderCardUI) en la Zona Verde superior derecha.
    /// Acumula de derecha a izquierda: el pedido nuevo se coloca al extremo derecho
    /// y desplaza a los pedidos previos hacia la izquierda con espaciado compacto.
    /// </summary>
    public class OrderBarUI : MonoBehaviour
    {
        [Header("Referencias")]
        public OrderManager OrdersManager;
        public GameObject OrderCardPrefab;
        public Transform CardsContainer;

        private readonly Dictionary<Order, OrderCardUI> _activeCards = new Dictionary<Order, OrderCardUI>();

        private void Start()
        {
            if (CardsContainer != null)
            {
                var hlg = CardsContainer.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null)
                {
                    hlg.childAlignment = TextAnchor.MiddleRight;
                    hlg.spacing = 8f; // Compacto, sin espacios gigantescos
                    hlg.childControlWidth = false;
                    hlg.childControlHeight = false;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = false;
                }
            }
        }

        private void Update()
        {
            if (OrdersManager == null || OrderCardPrefab == null || CardsContainer == null) return;

            // 1. Crear tarjetas para nuevos pedidos (se insertan al extremo derecho)
            foreach (var order in OrdersManager.ActiveOrders)
            {
                if (!_activeCards.ContainsKey(order))
                {
                    GameObject cardGO = Instantiate(OrderCardPrefab, CardsContainer);
                    // Colocar como último hermano para que quede al extremo derecho
                    cardGO.transform.SetAsLastSibling();

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
