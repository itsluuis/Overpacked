using LogiPack.Orders;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Representa la tarjeta visual de un pedido en la barra superior (estilo Overcooked).
    /// Muestra barra de expiración con color progresivo (Verde -> Amarillo -> Rojo).
    /// Al pasar el mouse por encima (hover), expande el portapapeles con la condición
    /// ambiental lógica y los items requeridos.
    /// </summary>
    public class OrderCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Datos Vinculados")]
        public Order LinkedOrder;

        [Header("Referencias Visuales Compactas")]
        public Text OrderIdText;
        public Text RewardText;
        public Image TimeBarFill;

        [Header("Panel Expandido al Hover")]
        public GameObject DetailsPanel;
        public Text ConditionText;
        public Text ItemsListText;

        public void BindOrder(Order order)
        {
            LinkedOrder = order;

            if (OrderIdText != null) OrderIdText.text = order.OrderId;
            if (RewardText != null) RewardText.text = $"${order.MonetaryReward}";

            if (ConditionText != null) ConditionText.text = order.Condition.NaturalLanguageText;

            // Armar texto de requerimientos
            if (ItemsListText != null)
            {
                string reqStr = "";
                foreach (var req in order.Requirements)
                {
                    reqStr += $"• {req.RequiredCount}x {req.Item.ItemName}\n";
                }
                ItemsListText.text = reqStr;
            }

            if (DetailsPanel != null) DetailsPanel.SetActive(false);
        }

        private void Update()
        {
            if (LinkedOrder == null) return;

            // Actualizar barra de tiempo y color progresivo
            if (TimeBarFill != null)
            {
                float t = LinkedOrder.TimeNormalized;
                TimeBarFill.fillAmount = t;

                // Verde (>50%), Amarillo (25-50%), Rojo (<25%)
                if (t > 0.5f)
                    TimeBarFill.color = Color.Lerp(Color.yellow, new Color(0.2f, 0.85f, 0.3f), (t - 0.5f) * 2f);
                else
                    TimeBarFill.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), Color.yellow, t * 2f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DetailsPanel != null) DetailsPanel.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (DetailsPanel != null) DetailsPanel.SetActive(false);
        }
    }
}
