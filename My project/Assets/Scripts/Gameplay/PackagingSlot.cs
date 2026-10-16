using UnityEngine;

namespace LogiPack.Gameplay
{
    /// <summary>
    /// Componente colocado en cada una de las 5 ranuras de embalaje.
    /// Define la hitbox receptora tolerante para la caja.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class PackagingSlot : MonoBehaviour
    {
        public int SlotIndex;
        public Managers.PackagingZoneManager ZoneManager;

        private void Reset()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.size = new Vector2(2.0f, 2.0f); // Hitbox amplia y tolerante
            }
        }
    }
}
