using System;
using Assets.Scripts.Core.Items;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class InventoryItem
    {
        private int _count;
        
        public readonly Item ItemPrefab;
        public readonly Item ItemInstance;
        public InventoryItem(Item itemPrefab)
        {
            ItemPrefab = itemPrefab;
            ItemInstance = Object.Instantiate(ItemPrefab);
            _count = 1;
        }

        public int Count => _count;

        public void AddItem(Item itemPrefab)
        {
            if(ItemPrefab != itemPrefab)
                throw new ArgumentException("Try to add other item.", nameof(itemPrefab));

            _count++;
        }
    }
}