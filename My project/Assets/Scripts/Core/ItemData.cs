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

        public bool HasAttribute(ItemAttributes attr)
        {
            return (Attributes & attr) == attr;
        }
    }
}
