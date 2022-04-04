using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    public class WeaponItem : Item
    {
        [FormerlySerializedAs("DamageType")]
        public DamageType MeleeDamageType;
        [FormerlySerializedAs("Attack")]
        public float MeleeAttack = 1.0f;
        [FormerlySerializedAs("Force")]
        public float MeleeForce = 1.0f;
        [FormerlySerializedAs("HorizontalRange")]
        public float MeleeHorizontalRange = 1.0f;
        [FormerlySerializedAs("VerticalRangePadding")]
        public float MeleeVerticalRangePadding = 0.0f;

        public override void DealDamage(Vector2 position, Vector2 direction)
        {
      
        }
    }

   
}