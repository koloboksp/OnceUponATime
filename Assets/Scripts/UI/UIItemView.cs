using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.HeroMisc;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIItemView : UIUserButton
    {
        public Image IconImage;
        public Text CountLabel;

        InventoryItem mInventoryItem;

        public InventoryItem InventoryItem
        {
            set
            {
                mInventoryItem = value;
                UIItemViewInfo itemViewInfo = UIItemViewInfoManager.Instance.GetInfo(mInventoryItem.ItemPrefab);

                IconImage.sprite = itemViewInfo.Icon;
                CountLabel.text = mInventoryItem.Count.ToString();
            }
            get { return mInventoryItem; }
        }
      
    }
}