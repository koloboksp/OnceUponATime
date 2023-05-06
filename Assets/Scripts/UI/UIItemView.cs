using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.HeroMisc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIItemView : UIUserButton
    {
        private InventoryItem _inventoryItem;

        [FormerlySerializedAs("IconImage")] [SerializeField] private Image _iconImage;
        [FormerlySerializedAs("CountLabel")] [SerializeField] private Text _countLabel;
        
        public InventoryItem InventoryItem
        {
            set
            {
                _inventoryItem = value;
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(_inventoryItem.ItemPrefab);

                _iconImage.sprite = itemViewInfo.Icon;
                _countLabel.text = _inventoryItem.Count.ToString();
            }
            get { return _inventoryItem; }
        }
    }
}