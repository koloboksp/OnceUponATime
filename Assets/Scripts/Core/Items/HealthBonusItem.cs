using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "HealthBonusItem", menuName = "Items/HealthBonusItem", order = 51)]
    public class HealthBonusItem : Bonus
    {
        [FormerlySerializedAs("Power")] 
        [SerializeField] private float _power = 1;

        public float Power => _power;
    }
}