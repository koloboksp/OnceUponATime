using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Beholder : GroundMob
    {
        
        public event Action<Beholder> OnAttackStateChanged;

        public Transform SmoothRotationRoot;

        private readonly SimpleMeleeAttackOperation mAttackOperation = new SimpleMeleeAttackOperation();
        public AttackOperation AttackOperation => mAttackOperation;

        internal override void ChangeDirectionSmooth(Direction direction, RotationDirection rotationDirection, float time, Transform rotationRoot)
        {
            base.ChangeDirectionSmooth(direction, rotationDirection, time, rotationRoot);
      
        }


        private void Update()
        {
            base.InnerUpdate();
                  
            mAttackOperation.Process(Time.deltaTime);
        }

        internal void Attack(float speed, float waitPart, Vector2 dealDamageInterval, DamageType damageType, float attack, float force, float hRange, float vRangePadding)
        {
            var attackTime = 1.0f / speed;
            var waitPartTime = attackTime * waitPart;

            mAttackOperation.Execute(this, damageType, attack, force, hRange, vRangePadding, dealDamageInterval * (attackTime - waitPartTime), 
                attackTime - waitPartTime, waitPartTime);

            mAttackOperation.OnInWaitPart = Attack_InWaitPart;
            mAttackOperation.OnComplete = Attack_OnComplete;
            mAttackOperation.OnAbort = Attack_Abort;

            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        private void Attack_Abort(Operation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        private void Attack_InWaitPart(AttackOperation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        private void Attack_OnComplete(Operation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        
    }
}