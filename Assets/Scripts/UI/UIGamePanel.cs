using System.Linq;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIGamePanel : MonoBehaviour
    {
        public RectTransform Root;
        public Transform HeroHealthBarRoot;

        public UIUserButton InventoryBtn;
        public UIInventory Inventory;

        public void Start()
        {
            InventoryBtn.OnClick += InventoryBtn_OnClick; 

            var mTarget = FindObjectOfType<Hero>();
            mTarget.OnItemInWeaponSlotsChanged += Target_OnItemInWeaponSlotsChanged;
            Target_OnItemInWeaponSlotsChanged(mTarget);
        }

        void Target_OnItemInWeaponSlotsChanged(Hero sender)
        {
            var firstOrDefault = sender.MainWeaponSlots.FirstOrDefault(i => i.InventoryItem != null);
            if (firstOrDefault != null)
            {
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(firstOrDefault.InventoryItem.ItemPrefab);
                if (itemViewInfo != null)
                {
                    InventoryBtn.Image.sprite = itemViewInfo.Icon;
                }
            }
        }

        void InventoryBtn_OnClick(UIUserButton obj)
        {
            Inventory.Show();
        }
    }
}