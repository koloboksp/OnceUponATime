using System.Linq;
using Assets.Scripts.Core.Mobs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    public class UIGamePanel : MonoBehaviour
    {
        [FormerlySerializedAs("Root")] [SerializeField] private RectTransform _root;
        [FormerlySerializedAs("HeroHealthBarRoot")] [SerializeField] private Transform _heroHealthBarRoot;

        [FormerlySerializedAs("InventoryBtn")] [SerializeField] private UIUserButton _inventoryBtn;
        [FormerlySerializedAs("Inventory")] [SerializeField] private UIInventory _inventory;
        public Transform Root => _root;
        public Transform HeroHealthBarRoot => _heroHealthBarRoot;

        public void Start()
        {
            _inventoryBtn.OnClick += InventoryBtn_OnClick; 

            var target = FindObjectOfType<Hero>();
            target.OnItemInWeaponSlotsChanged += Target_OnItemInWeaponSlotsChanged;
            Target_OnItemInWeaponSlotsChanged(target);
        }

        private void Target_OnItemInWeaponSlotsChanged(Hero sender)
        {
            var firstOrDefault = sender.MainWeaponSlots.FirstOrDefault(i => i.InventoryItem != null);
            if (firstOrDefault != null)
            {
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(firstOrDefault.InventoryItem.ItemPrefab);
                if (itemViewInfo != null)
                {
                    _inventoryBtn.Image.sprite = itemViewInfo.Icon;
                }
            }
        }

        private void InventoryBtn_OnClick(UIUserButton obj)
        {
            _inventory.Show();
        }
    }
}