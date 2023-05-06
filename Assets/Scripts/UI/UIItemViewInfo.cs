using Assets.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    [CreateAssetMenu(fileName = "UIItemViewInfo", menuName = "UI/UIItemViewInfo", order = 51)]
    public class UIItemViewInfo : ScriptableObject
    {
        [FormerlySerializedAs("Target")] [SerializeField] private Item _target;
        [FormerlySerializedAs("Icon")] [SerializeField] private Sprite _icon;
        public Item Target => _target;
        public Sprite Icon => _icon;

    }
}