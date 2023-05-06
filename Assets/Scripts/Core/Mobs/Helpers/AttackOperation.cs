using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{ 
    public class AttackOperation : Operation
    {
        private static readonly Collider2D[] NoAllocFoundResults = new Collider2D[10];
        private static readonly List<IDamageable> NoAllocGetComponent = new List<IDamageable>();

        private Action<AttackOperation> _onInWaitPart;

        private float _preparePartTime;
        private float _attackPartTime;
        private float _waitPartTime;

        private WeaponItemUsingType _itemUsingType;

        private Vector2 _meleeDealDamageTimeInterval;
        private MeleeAttackState _meleeAttackState;

        private float _shotTime;
        private RangedAttackState _rangedAttackState;

        public bool InAttackPart { get; private set; }
        public bool InWaitingPart { get; private set; }
        public float WaitPartTime => _waitPartTime;
        public float AttackPartTime => _attackPartTime;

        public Action<AttackOperation> OnInWaitPart
        {
            set => _onInWaitPart = value;
        }

        
        protected void CommonExecute(float attackPartTime, float waitPartTime)
        {
            base.Execute(attackPartTime + waitPartTime);

            _attackPartTime = attackPartTime;
            _waitPartTime = waitPartTime;

            InAttackPart = true;
            InWaitingPart = false;
        }

        protected void MeleeExecute(Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {
            CommonExecute(attackPartTime, waitPartTime);

            _itemUsingType = WeaponItemUsingType.Melee;

            _meleeDealDamageTimeInterval = dealDamageTimeInterval;
            _meleeAttackState = MeleeAttackState.WaitForBegin;
        }
        protected void RangedExecute(float shotTime, float attackPartTime, float waitPartTime)
        {
            CommonExecute(attackPartTime, waitPartTime);

            _itemUsingType = WeaponItemUsingType.Ranged;

            _shotTime = shotTime;
            _rangedAttackState = RangedAttackState.WaitForBegin;
        }

        public override void Abort()
        {
            InAttackPart = false;
            InWaitingPart = false;

            base.Abort();
        }

        protected override void InnerProcess(float dTime)
        {
            base.InnerProcess(dTime);

            if (_itemUsingType == WeaponItemUsingType.Melee)
            {
                if (_meleeAttackState == MeleeAttackState.WaitForBegin)
                {
                    if (_meleeDealDamageTimeInterval.x <= Timer)
                    {
                        _meleeAttackState = MeleeAttackState.Process;
                        BeginMeleeDealDamage();
                    }
                }

                if (_meleeAttackState == MeleeAttackState.Process)
                {
                    MeleeDealDamage();
                    if (_meleeDealDamageTimeInterval.y <= Timer)
                    {
                        _meleeAttackState = MeleeAttackState.End;
                        EndMeleeDealDamage();
                    }
                }
            }
            else if(_itemUsingType == WeaponItemUsingType.Ranged)
            {
                if (_rangedAttackState == RangedAttackState.WaitForBegin)
                {
                    BeginRangedAttack();
                    _rangedAttackState = RangedAttackState.WaitForShot;
                    
                }
                if (_rangedAttackState == RangedAttackState.WaitForShot)
                {
                    if (Timer >= _shotTime)
                    {
                        _rangedAttackState = RangedAttackState.End;
                        RangedShot();
                    }
                }
            }

            if (!InWaitingPart)
            {
                if (Timer > Time - _waitPartTime)
                {
                    InAttackPart = false;
                    InWaitingPart = true;

                    if (_itemUsingType == WeaponItemUsingType.Ranged)
                        EndRangedAttack();

                    if (_onInWaitPart != null)
                        _onInWaitPart(this);
                }
            }
        }

        public static void DrawBox(Vector2 center, Vector2 size, Color color)
        {
            Vector2 extents = size * 0.5f;

            Debug.DrawLine(center + Vector2.right * extents.x * 0.1f, center - Vector2.right * extents.x * 0.1f, color);
            Debug.DrawLine(center + Vector2.up * extents.y * 0.1f, center - Vector2.up * extents.y * 0.1f, color);

            Debug.DrawLine(center + new Vector2(extents.x, extents.y), center + new Vector2(extents.x, -extents.y), color);
            Debug.DrawLine(center + new Vector2(extents.x, -extents.y), center + new Vector2(-extents.x, -extents.y), color);
            Debug.DrawLine(center + new Vector2(-extents.x, -extents.y), center + new Vector2(-extents.x, extents.y), color);
            Debug.DrawLine(center + new Vector2(-extents.x, extents.y), center + new Vector2(extents.x, extents.y), color);
        }

        public Rect GetDamageArea(Character owner, float horizontalRange, float verticalPadding)
        {
            Vector2 forward = owner.transform.right;
            Vector2 up = owner.transform.up;

            Vector2 center = (Vector2)owner.transform.position + forward * horizontalRange * 0.5f + up * (owner.Bounds.center.y);
            Vector2 size = new Vector2(horizontalRange, owner.Bounds.height + verticalPadding);

            return new Rect(center - size * 0.5f, size);
        }

        protected void FindObjectsInArea(Rect damageArea, List<KeyValuePair<IDamageable, Collider2D>> damaged)
        {
            damaged.Clear();

            var resultCount = Physics2D.OverlapBoxNonAlloc(damageArea.center, damageArea.size, 0, NoAllocFoundResults);
            DrawBox(damageArea.center, damageArea.size, Color.red);

            for (int rIndex = 0; rIndex < resultCount; rIndex++)
            {
                var fCollider = NoAllocFoundResults[rIndex];
                fCollider.gameObject.GetComponents<IDamageable>(NoAllocGetComponent);

                for (var index = 0; index < NoAllocGetComponent.Count; index++)
                {
                    var damageable = NoAllocGetComponent[index];
                    damaged.Add(new KeyValuePair<IDamageable, Collider2D>(damageable, fCollider));  
                }
            }
        }
       
        protected virtual void BeginMeleeDealDamage() { }
        protected virtual void MeleeDealDamage() { }
        protected virtual void EndMeleeDealDamage() { }

        protected virtual void BeginRangedAttack() { }
        protected virtual void RangedShot() { }
        protected virtual void EndRangedAttack() { }
        
        private enum MeleeAttackState
        {
            WaitForBegin,
            Process,
            End,
        }

        private enum RangedAttackState
        {
            WaitForBegin,
            WaitForShot,
            End,          
        }
    }
}