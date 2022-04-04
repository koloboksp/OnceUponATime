using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroWeaponSlot
    {
        public readonly WeaponItemPlacement Placement;
        public readonly Transform Anchor;
        InventoryItem mInventoryItem;

        public HeroWeaponSlot()
        {
           
        }
        public HeroWeaponSlot(WeaponItemPlacement placement, Transform anchor)
        {
            Placement = placement;
            Anchor = anchor;
        }

        public InventoryItem InventoryItem => mInventoryItem;

        public void ChangeItem(InventoryItem item)
        {
            if(mInventoryItem != null)
                mInventoryItem.ItemInstance.DestroyEquipmentViewPart();

            mInventoryItem = item;

            if (mInventoryItem != null)
            {
                mInventoryItem.ItemInstance.CreateEquipmentViewPart(Anchor);
            }
        }
    }
}