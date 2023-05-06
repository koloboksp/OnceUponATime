using System.Linq;
using Assets.Scripts.Core.Mobs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    public class UIInventory : MonoBehaviour
    {
        private Hero _target;

        [FormerlySerializedAs("ItemView")] [SerializeField] private UIItemView _itemView;
        [FormerlySerializedAs("ItemsRoot")] [SerializeField] private RectTransform _itemsRoot;
        
        public void Show()
        {
            gameObject.SetActive(true);
            _target = FindObjectOfType<Hero>();

            Time.timeScale = 0;

            var uiItemViewInfos = _itemsRoot.GetComponentsInChildren<UIItemView>();
            foreach (var uiItemViewInfo in uiItemViewInfos)
            {
                uiItemViewInfo.OnClick -= UIItemView_OnClick;
                Destroy(uiItemViewInfo.gameObject);
            }

            var inventoryItems = _target.InventoryItems.ToList();
            for (var index = 0; index < inventoryItems.Count; index++)
            {
                var inventoryItem = inventoryItems[index];
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(inventoryItem.ItemPrefab);

                var uiItemView = Instantiate(_itemView);
                uiItemView.InventoryItem = inventoryItem;
                uiItemView.OnClick += UIItemView_OnClick;

                uiItemView.transform.SetParent(_itemsRoot);
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
            _target.EquipMainWeapon(itemView.InventoryItem.ItemInstance);

            Hide();
        }
    }
}