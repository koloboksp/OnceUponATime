using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    public class WeaponItem : Item
    {
        [FormerlySerializedAs("MeleeDamageType")]
        [SerializeField] private DamageType _meleeDamageType;
        [FormerlySerializedAs("MeleeAttack")]
        [SerializeField] private float _meleeAttack = 1.0f;
        [FormerlySerializedAs("MeleeForce")]
        [SerializeField] private float _meleeForce = 1.0f;
        [FormerlySerializedAs("MeleeHorizontalRange")]
        [SerializeField] private float _meleeHorizontalRange = 1.0f;
        [FormerlySerializedAs("MeleeVerticalRangePadding")]
        [SerializeField] private float _meleeVerticalRangePadding = 0.0f;

        public DamageType MeleeDamageType => _meleeDamageType;
        public float MeleeAttack => _meleeAttack;
        public float MeleeForce => _meleeForce;
        public float MeleeHorizontalRange => _meleeHorizontalRange;
        public float MeleeVerticalRangePadding => _meleeVerticalRangePadding;
        
        public override void DealDamage(Vector2 position, Vector2 direction)
        {
      
        }
    }

   
}