using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroWeaponSlot
    {
        private InventoryItem _inventoryItem;

        public readonly WeaponItemPlacement Placement;
        public readonly Transform Anchor;
        
        public HeroWeaponSlot()
        {
           
        }
        public HeroWeaponSlot(WeaponItemPlacement placement, Transform anchor)
        {
            Placement = placement;
            Anchor = anchor;
        }

        public InventoryItem InventoryItem => _inventoryItem;

        public void ChangeItem(InventoryItem item)
        {
            _inventoryItem?.ItemInstance.DestroyEquipmentViewPart();

            _inventoryItem = item;

            _inventoryItem?.ItemInstance.CreateEquipmentViewPart(Anchor);
        }
    }
}