using System;
using UnityEngine;

namespace LogiPack.Core
{
    [Flags]
    public enum ItemAttributes
    {
        None        = 0,
        Tool        = 1 << 0,  // Herramienta
        Metal       = 1 << 1,  // Metálico
        Fragile     = 1 << 2,  // Frágil
        Electrical  = 1 << 3,  // Eléctrico
        Fabric      = 1 << 4   // Tela / Textil
    }

    public enum ItemTier
    {
        Tier1_Basic = 10,
        Tier2_Medium = 18,
        Tier3_Advanced = 28
    }

    /// <summary>
    /// Definición de datos inmutables de un producto del almacén (ScriptableObject).
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "LogiPack/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ItemId;
        public string ItemName;
        public Sprite Icon;
        public Color BaseColor = Color.white;
        public ItemAttributes Attributes;

        [Header("Economía")]
        [Tooltip("Nivel o tier del producto")]
        public ItemTier Tier = ItemTier.Tier1_Basic;

        [Tooltip("Precio base unitario del ítem ($10, $18 o $28)")]
        public int BasePrice = 10;

        [Tooltip("Familia temática: Metales, Electricidad, Óptica, Textiles")]
        public string FamilyName = "General";

        public bool HasAttribute(ItemAttributes attr)
        {
            return (Attributes & attr) == attr;
        }
    }
}
