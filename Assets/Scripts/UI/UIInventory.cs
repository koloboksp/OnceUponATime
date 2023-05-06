using System.Linq;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIInventory : MonoBehaviour
    {
        public UIItemView ItemView;
        public RectTransform ItemsRoot;

        private Hero mTarget;

        public void Show()
        {
            this.gameObject.SetActive(true);
            mTarget = FindObjectOfType<Hero>();

            Time.timeScale = 0;

            var uiItemViewInfos = ItemsRoot.GetComponentsInChildren<UIItemView>();
            foreach (var uiItemViewInfo in uiItemViewInfos)
            {
                uiItemViewInfo.OnClick -= UIItemView_OnClick;
                Destroy(uiItemViewInfo.gameObject);
            }

            var inventoryItems = mTarget.InventoryItems.ToList();
            for (var index = 0; index < inventoryItems.Count; index++)
            {
                var inventoryItem = inventoryItems[index];
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(inventoryItem.ItemPrefab);

                var uiItemView = Instantiate(ItemView);
                uiItemView.InventoryItem = inventoryItem;
                uiItemView.OnClick += UIItemView_OnClick;

                uiItemView.transform.SetParent(ItemsRoot);
                uiItemView.transform.localPosition = new Vector3(index * 200, 0);
            }
        }
        public void Hide()
        {
            Time.timeScale = 1;

            this.gameObject.SetActive(false);       
        }

        private void UIItemView_OnClick(UIUserButton sender)
        {
            var itemView = sender as UIItemView;
            mTarget.EquipMainWeapon(itemView.InventoryItem.ItemInstance);

            Hide();
        }

    }
}