using LogiPack.Orders;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LogiPack.UI
{
    /// <summary>
    /// Representa la tarjeta visual de un pedido en forma de Post-it con tachuela física.
    /// Pedidos estándar: Post-it amarillo con tachuela roja.
    /// Pedidos especiales: Post-it fucsia con tachuela amarilla.
    /// Al hacer hover, se despliega la versión grande del post-it con la información detallada.
    /// </summary>
    public class OrderCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Datos Vinculados")]
        public Order LinkedOrder;

        [Header("Referencias Visuales de Post-it")]
        public Image CardBackground;
        public Image ThumbtackImage;
        public Text OrderIdText;
        public Text RewardText;
        public Image TimeBarFill;

        [Header("Panel Expandido al Hover (Post-it Grande)")]
        public GameObject DetailsPanel;
        public Image BigDetailsBackground;
        public Text DetailsHeaderText;
        public Text ConditionText;
        public Text ItemsListText;

        [Header("Sprites de Post-its y Tachuelas")]
        public Sprite SmallYellowPostit;
        public Sprite SmallFuchsiaPostit;
        public Sprite BigYellowPostit;
        public Sprite BigFuchsiaPostit;
        public Sprite RedThumbtack;
        public Sprite YellowThumbtack;

        public void BindOrder(Order order)
        {
            LinkedOrder = order;

            bool isSpecial = order != null && order.IsCompanyOrder;

            // 1. Asignar Sprite del Post-it y Tachuela
            if (CardBackground != null)
            {
                CardBackground.color = Color.white;
                CardBackground.sprite = isSpecial ? SmallFuchsiaPostit : SmallYellowPostit;
            }

            if (ThumbtackImage != null)
            {
                ThumbtackImage.sprite = isSpecial ? YellowThumbtack : RedThumbtack;
                ThumbtackImage.color = Color.white;
            }

            if (BigDetailsBackground != null)
            {
                BigDetailsBackground.color = Color.white;
                BigDetailsBackground.sprite = isSpecial ? BigFuchsiaPostit : BigYellowPostit;
            }

            // 2. Encabezado / Título (Texto oscuro de tinta legible sobre papel)
            Color inkColor = new Color(0.16f, 0.12f, 0.08f); // Tinta carbón cálido
            if (OrderIdText != null)
            {
                if (isSpecial && !string.IsNullOrEmpty(order.CompanyName))
                {
                    OrderIdText.text = $"★ {order.CompanyName}";
                    OrderIdText.color = new Color(0.38f, 0.06f, 0.22f); // Tinta borgoña/fucsia oscuro
                }
                else
                {
                    OrderIdText.text = order.OrderId;
                    OrderIdText.color = inkColor;
                }
            }

            // 3. Recompensa monetaria
            if (RewardText != null)
            {
                RewardText.text = $"${order.MonetaryReward}";
                RewardText.color = isSpecial ? new Color(0.55f, 0.15f, 0.0f) : new Color(0.10f, 0.48f, 0.15f);
            }

            // 4. Configurar Barra de Tiempo horizontal (se encoge de derecha a izquierda)
            if (TimeBarFill != null)
            {
                TimeBarFill.type = Image.Type.Filled;
                TimeBarFill.fillMethod = Image.FillMethod.Horizontal;
                TimeBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
                TimeBarFill.fillAmount = 1f;
            }

            // 5. Encabezado en el Post-it Grande
            if (DetailsHeaderText != null)
            {
                if (isSpecial)
                {
                    DetailsHeaderText.text = $"★ {order.CompanyName}\n<color=#116622><size=18>Lote Especial • Recompensa: ${order.MonetaryReward}</size></color>";
                    DetailsHeaderText.color = new Color(0.38f, 0.06f, 0.22f);
                }
                else
                {
                    DetailsHeaderText.text = $"PEDIDO: {order.OrderId}   <color=#106020><size=19>+${order.MonetaryReward}</size></color>";
                    DetailsHeaderText.color = inkColor;
                }
            }

            // 6. Condición lógica en el Post-it Grande
            if (ConditionText != null)
            {
                ConditionText.color = inkColor;
                if (isSpecial)
                {
                    string pkg = order.Condition != null ? order.Condition.NaturalLanguageText : "";
                    if (string.IsNullOrEmpty(pkg) || pkg == "Embalaje estándar")
                    {
                        ConditionText.text = "<b><color=#004477>Embalaje Solicitado:</color></b> Estándar (sin protección extra).";
                    }
                    else
                    {
                        ConditionText.text = $"<b><color=#884400>Embalaje Solicitado:</color></b> {pkg}";
                    }
                }
                else
                {
                    string cond = order.Condition != null ? order.Condition.NaturalLanguageText : "Envío normal";
                    string formula = order.Condition != null ? order.Condition.FormalFormula : "p";
                    ConditionText.text = $"<b><color=#004477>Regla Lógica:</color></b> {cond}\n<color=#882200><b>Fórmula Formal:</b> {formula}</color>";
                }
            }

            // 7. Requerimientos / Fórmula Simbólica y Candidatos Válidos
            if (ItemsListText != null)
            {
                ItemsListText.color = inkColor;
                if (isSpecial && order.SetRule != null)
                {
                    string formal = $"<b>Fórmula de Lote:</b> <color=#003366>{order.SetRule.FormalFormula}</color>\n";
                    string desc = $"<b>Objetivo:</b> Empacar {order.TargetItemCount}x {order.SetRule.NaturalLanguageDescription}\n";

                    string validStr = "<b>Candidatos Válidos:</b> ";
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

                    ItemsListText.text = $"{formal}{desc}<color=#116622>{validStr}</color>";
                }
                else
                {
                    string reqStr = "<b>Artículos a Empacar:</b>\n";
                    foreach (var req in order.Requirements)
                    {
                        reqStr += $"  • {req.RequiredCount}x {req.Item.ItemName}\n";
                    }
                    ItemsListText.text = reqStr;
                }
            }

            if (DetailsPanel != null) DetailsPanel.SetActive(false);
        }

        private void Update()
        {
            if (LinkedOrder == null) return;

            // Barra de tiempo decreciente horizontal y cambio tricolor
            if (TimeBarFill != null)
            {
                float t = LinkedOrder.TimeNormalized;
                TimeBarFill.fillAmount = t;

                // Verde (>=60%), Amarillo (25% a 60%), Rojo (<25%)
                if (t >= 0.60f)
                {
                    TimeBarFill.color = new Color(0.20f, 0.78f, 0.30f); // Verde
                }
                else if (t >= 0.25f)
                {
                    TimeBarFill.color = new Color(0.95f, 0.75f, 0.15f); // Amarillo
                }
                else
                {
                    TimeBarFill.color = new Color(0.90f, 0.22f, 0.20f); // Rojo
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DetailsPanel != null)
            {
                DetailsPanel.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (DetailsPanel != null)
            {
                DetailsPanel.SetActive(false);
            }
        }
    }
}
