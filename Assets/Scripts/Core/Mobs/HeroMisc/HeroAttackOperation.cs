using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroPrepareToAttackOperation : Operation
    {
        private Hero mOwner;
        private ItemPreparationView mItemPreparationViewInstance;

        public HeroAvailableStrikes Movement { get; private set; }
        public HeroWeaponSlot mWeaponSlot;

        public Vector3 Value { get; private set; }
        public float MinimalTime { get; private set; }
        public bool MinimalTimePassed { get; private set; }

        public void Execute(Hero owner, HeroAvailableStrikes movement, HeroWeaponSlot weaponSlot, Vector3 value, float minimalTime, float time)
        {
            base.Execute(time);

            mOwner = owner;
            Movement = movement;
            mWeaponSlot = weaponSlot;

            Value = value;

            MinimalTime = minimalTime;
            MinimalTimePassed = false;
            mItemPreparationViewInstance = mWeaponSlot.InventoryItem.ItemInstance.CreatePreparationView(mOwner.ViewPart.ShotPoint);
            SetValue(Value);
        }
        public void ManualComplete()
        {
            mWeaponSlot.InventoryItem.ItemInstance.DestroyPreparationView();
            mItemPreparationViewInstance = null;

            ForceComplete();
        }

        public void SetValue(Vector3 value)
        {
            Value = value;

            var quaternion = mWeaponSlot.InventoryItem.ItemInstance.Owner.transform.rotation * Quaternion.Euler(0, 0, Value.y);
            if (mItemPreparationViewInstance != null)
                mItemPreparationViewInstance.transform.rotation = quaternion;//Quaternion.Euler(0, 0, value.y);
        }

        protected override void InnerProcess(float dTime)
        {
            base.InnerProcess(dTime);
            if(!MinimalTimePassed)
            if (Timer > MinimalTime)
                MinimalTimePassed = true;
        }
    }

    public class HeroAttackOperation : AttackOperation
    {
        private Hero _owner;
        private HeroWeaponSlot _weaponSlot;
        private float _shotAngle = 0.0f;
        private readonly List<IDamageable> _alreadyDamagedObjectsAtCycle = new List<IDamageable>();
        private readonly List<KeyValuePair<IDamageable, Collider2D>> _damagedObjectsAtCycle = new List<KeyValuePair<IDamageable, Collider2D>>();

        public HeroAvailableStrikes Movement { get; private set; }
        public bool BlockMovement { get; private set; }
        
        public void MeleeExecute(Hero owner, HeroAttackType attackType, HeroAvailableStrikes movement, bool blockMovement,
            HeroWeaponSlot weaponSlot, Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {
            _owner = owner;
            Movement = movement;
            BlockMovement = blockMovement;
            _weaponSlot = weaponSlot;

            MeleeExecute(dealDamageTimeInterval, attackPartTime, waitPartTime);
        }

        public void RangedExecute(Hero owner, HeroAttackType attackType, HeroAvailableStrikes movement, bool blockMovement,
            HeroWeaponSlot weaponSlot, float shotTime, float shotAngle, float attackPartTime, float waitPartTime)
        {
            _owner = owner;
            Movement = movement;
            BlockMovement = blockMovement;
            _weaponSlot = weaponSlot;

            _shotAngle = shotAngle;

            RangedExecute(shotTime, attackPartTime, waitPartTime);
        }

        public void AimExecute(Hero owner, HeroAttackType attackType, HeroAvailableStrikes movement, HeroWeaponSlot weaponSlot, 
            float attackPartTime, float waitPartTime)
        {
          
        }

        protected override void BeginMeleeDealDamage()
        {
            var weaponItem = _weaponSlot.InventoryItem.ItemInstance as WeaponItem;

            var damageArea = GetDamageArea(_owner, weaponItem.MeleeHorizontalRange, weaponItem.MeleeVerticalRangePadding);
            weaponItem.BeginDealDamage(damageArea);
        }

        protected override void MeleeDealDamage()
        {
            var weaponItem = _weaponSlot.InventoryItem.ItemInstance as WeaponItem;

            var damageArea = GetDamageArea(_owner, weaponItem.MeleeHorizontalRange, weaponItem.MeleeVerticalRangePadding);
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
                    damagedPair.Key.TakeDamage(_owner, new DamageInfo(weaponItem.MeleeDamageType, weaponItem.MeleeAttack, damageArea.center, weaponItem.MeleeForce, dirTo));
                }
            }

            _weaponSlot.InventoryItem.ItemInstance.DealDamage(_weaponSlot.Anchor.transform.position, _owner.transform.right);
        }

        protected override void EndMeleeDealDamage()
        {
            _weaponSlot.InventoryItem.ItemInstance.EndDealDamage();

            _alreadyDamagedObjectsAtCycle.Clear();   
        }

        protected override void RangedShot()
        {
            var weaponItem = _weaponSlot.InventoryItem.ItemInstance as WeaponItem;
            var rangedWeaponItem = weaponItem as RangedWeaponItem;
            rangedWeaponItem.Shot(_shotAngle);
        }

        protected override void EndRangedAttack()
        {
            var weaponItem = _weaponSlot.InventoryItem.ItemInstance as WeaponItem;
            var rangedWeaponItem = weaponItem as RangedWeaponItem;
            rangedWeaponItem.EndAttack();       
        }
    }
}