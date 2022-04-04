using Assets.Scripts.Core.Items;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [CreateAssetMenu(fileName = "UIItemViewInfo", menuName = "UI/UIItemViewInfo", order = 51)]
    public class UIItemViewInfo : ScriptableObject
    {
        public Item Target;
        public Sprite Icon;
    }
}