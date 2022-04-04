using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{ 
    public class AttackOperation : Operation
    {
        enum MeleeAttackState
        {
            WaitForBegin,
            Process,
            End,
        }

        enum RangedAttackState
        {
            WaitForBegin,
            WaitForShot,
            End,          
        }

        static readonly Collider2D[] mNoAllocFoundResults = new Collider2D[10];
        static readonly List<IDamageable> mNoAllocGetComponent = new List<IDamageable>();

        Action<AttackOperation> mOnInWaitPart;

        float mPreparePartTime;
        float mAttackPartTime;
        float mWaitPartTime;
        
        WeaponItemUsingType mUsingType;

        Vector2 mMeleeDealDamageTimeInterval;
        MeleeAttackState mMeleeAttackState;

        float mShotTime;
        RangedAttackState mRangedAttackState;

        public bool InAttackPart { get; private set; }
        public bool InWaitingPart { get; private set; }
        public float WaitPartTime => mWaitPartTime;
        public float AttackPartTime => mAttackPartTime;

        public Action<AttackOperation> OnInWaitPart
        {
            set => mOnInWaitPart = value;
        }

        
        protected void CommonExecute(float attackPartTime, float waitPartTime)
        {
            base.Execute(attackPartTime + waitPartTime);

            mAttackPartTime = attackPartTime;
            mWaitPartTime = waitPartTime;

            InAttackPart = true;
            InWaitingPart = false;
        }

        protected void MeleeExecute(Vector2 dealDamageTimeInterval, float attackPartTime, float waitPartTime)
        {
            CommonExecute(attackPartTime, waitPartTime);

            mUsingType = WeaponItemUsingType.Melee;

            mMeleeDealDamageTimeInterval = dealDamageTimeInterval;
            mMeleeAttackState = MeleeAttackState.WaitForBegin;
        }
        protected void RangedExecute(float shotTime, float attackPartTime, float waitPartTime)
        {
            CommonExecute(attackPartTime, waitPartTime);

            mUsingType = WeaponItemUsingType.Ranged;

            mShotTime = shotTime;
            mRangedAttackState = RangedAttackState.WaitForBegin;
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

            if (mUsingType == WeaponItemUsingType.Melee)
            {
                if (mMeleeAttackState == MeleeAttackState.WaitForBegin)
                {
                    if (mMeleeDealDamageTimeInterval.x <= Timer)
                    {
                        mMeleeAttackState = MeleeAttackState.Process;
                        BeginMeleeDealDamage();
                    }
                }

                if (mMeleeAttackState == MeleeAttackState.Process)
                {
                    MeleeDealDamage();
                    if (mMeleeDealDamageTimeInterval.y <= Timer)
                    {
                        mMeleeAttackState = MeleeAttackState.End;
                        EndMeleeDealDamage();
                    }
                }
            }
            else if(mUsingType == WeaponItemUsingType.Ranged)
            {
                if (mRangedAttackState == RangedAttackState.WaitForBegin)
                {
                    BeginRangedAttack();
                    mRangedAttackState = RangedAttackState.WaitForShot;
                    
                }
                if (mRangedAttackState == RangedAttackState.WaitForShot)
                {
                    if (Timer >= mShotTime)
                    {
                        mRangedAttackState = RangedAttackState.End;
                        RangedShot();
                    }
                }
            }

            if (!InWaitingPart)
            {
                if (Timer > Time - mWaitPartTime)
                {
                    InAttackPart = false;
                    InWaitingPart = true;

                    if (mUsingType == WeaponItemUsingType.Ranged)
                        EndRangedAttack();

                    if (mOnInWaitPart != null)
                        mOnInWaitPart(this);
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

            var resultCount = Physics2D.OverlapBoxNonAlloc(damageArea.center, damageArea.size, 0, mNoAllocFoundResults);
            DrawBox(damageArea.center, damageArea.size, Color.red);

            for (int rIndex = 0; rIndex < resultCount; rIndex++)
            {
                var fCollider = mNoAllocFoundResults[rIndex];
                fCollider.gameObject.GetComponents<IDamageable>(mNoAllocGetComponent);

                for (var index = 0; index < mNoAllocGetComponent.Count; index++)
                {
                    var damageable = mNoAllocGetComponent[index];
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
    }
}