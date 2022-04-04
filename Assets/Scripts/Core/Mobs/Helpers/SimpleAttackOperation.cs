using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class SimpleMeleeAttackOperation : AttackOperation
    {
        protected Character mOwner;
        DamageType mDamageType;

        readonly List<IDamageable> mAlreadyDamagedObjectsAtCycle = new List<IDamageable>();
        readonly List<KeyValuePair<IDamageable, Collider2D>> mDamagedObjectsAtCycle = new List<KeyValuePair<IDamageable, Collider2D>>();
        float mAttack;
        float mForce;
        protected float mHorizontalRange;
        protected float mVerticalRangePadding;

        public void Execute(Character owner, DamageType damageType, float attack, float force, float hRange, float vRangePadding, 
            Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {         
            mOwner = owner;
            mDamageType = damageType;
            mAttack = attack;
            mForce = force;
            mHorizontalRange = hRange;
            mVerticalRangePadding = vRangePadding;

            MeleeExecute(dealDamageTimeInterval, attackPartTime, waitPartTime);
        }


        protected override void MeleeDealDamage()
        {
            var damageArea = GetDamageArea(mOwner, mHorizontalRange, mVerticalRangePadding);

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
                    damagedPair.Key.TakeDamage(mOwner, new DamageInfo(mDamageType, mAttack, mOwner.transform.position, mForce, dirTo));
                }
            }
        }

        protected override void EndMeleeDealDamage()
        {     
            mAlreadyDamagedObjectsAtCycle.Clear();
        }
    }
}