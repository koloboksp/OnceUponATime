using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class SimpleMeleeAttackOperation : AttackOperation
    {
        protected Character _owner;
        private DamageType _damageType;

        private readonly List<IDamageable> _alreadyDamagedObjectsAtCycle = new List<IDamageable>();
        private readonly List<KeyValuePair<IDamageable, Collider2D>> _damagedObjectsAtCycle = new List<KeyValuePair<IDamageable, Collider2D>>();
        private float _attack;
        private float _force;
        protected float _horizontalRange;
        protected float _verticalRangePadding;

        public void Execute(Character owner, DamageType damageType, float attack, float force, float hRange, float vRangePadding, 
            Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {         
            _owner = owner;
            _damageType = damageType;
            _attack = attack;
            _force = force;
            _horizontalRange = hRange;
            _verticalRangePadding = vRangePadding;

            MeleeExecute(dealDamageTimeInterval, attackPartTime, waitPartTime);
        }
        
        protected override void MeleeDealDamage()
        {
            var damageArea = GetDamageArea(_owner, _horizontalRange, _verticalRangePadding);

            FindObjectsInArea(damageArea, _damagedObjectsAtCycle);

            for (var dpIndex = 0; dpIndex < _damagedObjectsAtCycle.Count; dpIndex++)
            {
                var damagedPair = _damagedObjectsAtCycle[dpIndex];
                if (!ReferenceEquals(damagedPair.Key, _owner) &&
                    !_alreadyDamagedObjectsAtCycle.Contains(damagedPair.Key))
                {
                    _alreadyDamagedObjectsAtCycle.Add(damagedPair.Key);

                    var vecTo = _owner.transform.position - damagedPair.Value.gameObject.transform.position;
                    var dirTo = -vecTo.normalized;
                    damagedPair.Key.TakeDamage(_owner, new DamageInfo(_damageType, _attack, _owner.transform.position, _force, dirTo));
                }
            }
        }

        protected override void EndMeleeDealDamage()
        {     
            _alreadyDamagedObjectsAtCycle.Clear();
        }
    }
}