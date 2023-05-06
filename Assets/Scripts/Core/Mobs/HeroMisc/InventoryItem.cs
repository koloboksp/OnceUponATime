using System;
using Assets.Scripts.Core.Items;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class InventoryItem
    {
        public readonly Item ItemPrefab;
        public readonly Item ItemInstance;
        private int mCount;

        public InventoryItem(Item itemPrefab)
        {
            ItemPrefab = itemPrefab;
            ItemInstance = Object.Instantiate(ItemPrefab);
            mCount = 1;
        }

        public int Count => mCount;

        public void AddItem(Item itemPrefab)
        {
            if(ItemPrefab != itemPrefab)
                throw new ArgumentException("Try to add other item.", nameof(itemPrefab));

            mCount++;
        }
    }
}