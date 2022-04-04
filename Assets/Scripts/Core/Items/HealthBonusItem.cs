using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "HealthBonusItem", menuName = "Items/HealthBonusItem", order = 51)]
    public class HealthBonusItem : Bonus
    {
        public float Power = 1;

    }
}