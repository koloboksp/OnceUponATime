using System;
using System.Collections;
using Assets.Scripts.Core.Mobs.DiggerMisc;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Digger : GroundMob
    {
        public event Action<Digger> OnAttackStateChanged;
        
        public Transform OrientedByGroundRoot;
        readonly RotationInterpolator mOrientedByGroundRootInterpolator = new RotationInterpolator();
        public Transform SmoothRotationRoot;
 
        readonly SimpleMeleeAttackOperation mAttackOperation = new SimpleMeleeAttackOperation();    
        public AttackOperation AttackOperation => mAttackOperation;

        void Update()
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

        void Attack_Abort(Operation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        void Attack_InWaitPart(AttackOperation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }

        void Attack_OnComplete(Operation obj)
        {
            if (OnAttackStateChanged != null)
                OnAttackStateChanged(this);
        }
    }
}