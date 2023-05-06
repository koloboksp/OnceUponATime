using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class SimpleMeleeAttackOperation : AttackOperation
    {
        protected Character mOwner;
        private DamageType mDamageType;

        private readonly List<IDamageable> mAlreadyDamagedObjectsAtCycle = new List<IDamageable>();
        private readonly List<KeyValuePair<IDamageable, Collider2D>> mDamagedObjectsAtCycle = new List<KeyValuePair<IDamageable, Collider2D>>();
        private float _attack;
        private float _force;
        protected float _horizontalRange;
        protected float _verticalRangePadding;

        public void Execute(Character owner, DamageType damageType, float attack, float force, float hRange, float vRangePadding, 
            Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {         
            mOwner = owner;
            mDamageType = damageType;
            _attack = attack;
            _force = force;
            _horizontalRange = hRange;
            _verticalRangePadding = vRangePadding;

            MeleeExecute(dealDamageTimeInterval, attackPartTime, waitPartTime);
        }


        protected override void MeleeDealDamage()
        {
            var damageArea = GetDamageArea(mOwner, _horizontalRange, _verticalRangePadding);

            FindObjectsInArea(damageArea, mDamagedObjectsAtCycle);

            for (var dpIndex = 0; dpIndex < mDamagedObjectsAtCycle.Count; dpIndex++)
            {
                var damagedPair = mDamagedObjectsAtCycle[dpIndex];
                if (!ReferenceEquals(damagedPair.Key, mOwner) &&
                    !mAlreadyDamagedObjectsAtCycle.Contains(damagedPair.Key))
                {
                    mAlreadyDamagedObjectsAtCycle.Add(damagedPair.Key);

                    var vecTo = mOwner.transform.position - damagedPair.Value.gameObject.transform.position;
                    var dirTo = -vecTo.normalized;
                    damagedPair.Key.TakeDamage(mOwner, new DamageInfo(mDamageType, _attack, mOwner.transform.position, _force, dirTo));
                }
            }
        }

        protected override void EndMeleeDealDamage()
        {     
            mAlreadyDamagedObjectsAtCycle.Clear();
        }
    }
}