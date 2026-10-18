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
        public Image CardBackground;
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

            // 1. Distinción visual de fondo
            if (CardBackground == null) CardBackground = GetComponent<Image>();
            if (CardBackground != null)
            {
                if (order.IsCompanyOrder)
                {
                    CardBackground.color = new Color(0.35f, 0.26f, 0.08f, 0.98f); // Tinte Ámbar / Dorado corporativo
                }
                else
                {
                    CardBackground.color = new Color(0.14f, 0.18f, 0.25f, 0.95f); // Tinte Azul Oscuro estándar
                }
            }

            // 2. Encabezado / Título
            if (OrderIdText != null)
            {
                if (order.IsCompanyOrder && !string.IsNullOrEmpty(order.CompanyName))
                {
                    OrderIdText.text = $"★ {order.CompanyName}";
                    OrderIdText.color = new Color(1f, 0.92f, 0.45f); // Dorado brillante
                }
                else
                {
                    OrderIdText.text = order.OrderId;
                    OrderIdText.color = Color.white;
                }
            }

            // 3. Recompensa (Limpia, sin especulaciones de propina)
            if (RewardText != null)
            {
                RewardText.text = $"${order.MonetaryReward}";
                RewardText.color = order.IsCompanyOrder ? new Color(1f, 0.85f, 0.25f) : new Color(0.3f, 0.95f, 0.4f);
            }

            // 4. Condición y encabezado en el panel hover
            if (ConditionText != null)
            {
                if (order.IsCompanyOrder)
                {
                    string pkg = order.Condition != null ? order.Condition.NaturalLanguageText : "";
                    if (pkg == "Embalaje estándar" || string.IsNullOrEmpty(pkg))
                    {
                        ConditionText.text = $"<b>{order.CompanyName}</b>\n<color=#A0E0FF>Solicitud de Lote Empresarial</color>";
                    }
                    else
                    {
                        ConditionText.text = $"<b>{order.CompanyName}</b>\n<color=#FFD700>• {pkg}</color>";
                    }
                }
                else
                {
                    ConditionText.text = order.Condition != null ? order.Condition.NaturalLanguageText : "";
                }
            }

            // 5. Requerimientos / Fórmula Simbólica y Candidatos Válidos
            if (ItemsListText != null)
            {
                if (order.IsCompanyOrder && order.SetRule != null)
                {
                    // Ajuste 4: Notación Simbólica Formal para evaluación académica
                    string formal = $"<color=#00FFFF><b>Fórmula de Lote:</b></color> {order.SetRule.FormalFormula}\n";
                    string desc = $"<b>Objetivo:</b> Empacar {order.TargetItemCount}x {order.SetRule.NaturalLanguageDescription}\n";

                    // Ajuste 3: Lista de Candidatos Válidos del Almacén
                    string validStr = "<color=#98FB98><b>Ítems Válidos:</b></color> ";
                    if (order.CachedValidItems != null && order.CachedValidItems.Count > 0)
                    {
                        var names = new System.Collections.Generic.List<string>();
                        foreach (var it in order.CachedValidItems)
                        {
                            names.Add(it.ItemName);
                        }
                        validStr += string.Join(", ", names);
                    }
                    else
                    {
                        validStr += "Ver estantería";
                    }

                    ItemsListText.text = $"{formal}{desc}{validStr}";
                }
                else
                {
                    string reqStr = "";
                    foreach (var req in order.Requirements)
                    {
                        reqStr += $"• {req.RequiredCount}x {req.Item.ItemName}\n";
                    }
                    ItemsListText.text = reqStr;
                }
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
