using System;
using System.Collections;
using Assets.Scripts.Core.Mobs.DiggerMisc;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Digger : GroundMob
    {
        public event Action<Digger> OnAttackStateChanged;
        
        private readonly RotationInterpolator _orientedByGroundRootInterpolator = new RotationInterpolator();
        private readonly SimpleMeleeAttackOperation _attackOperation = new SimpleMeleeAttackOperation();   
        
        [FormerlySerializedAs("OrientedByGroundRoot")] [SerializeField] private Transform _orientedByGroundRoot;
        [FormerlySerializedAs("SmoothRotationRoot")] [SerializeField] private Transform _smoothRotationRoot;
        
        public AttackOperation AttackOperation => _attackOperation;
        public Transform SmoothRotationRoot => _smoothRotationRoot;
       
        private void Update()
        {
            base.InnerUpdate();

            _attackOperation.Process(Time.deltaTime);
        }

        internal void Attack(float speed, float waitPart, Vector2 dealDamageInterval, DamageType damageType, float attack, float force, float hRange, float vRangePadding)
        {
            var attackTime = 1.0f / speed;
            var waitPartTime = attackTime * waitPart;

            _attackOperation.Execute(this, damageType, attack, force, hRange, vRangePadding, dealDamageInterval * (attackTime - waitPartTime),
                attackTime - waitPartTime, waitPartTime);

            _attackOperation.OnInWaitPart = Attack_InWaitPart;
            _attackOperation.OnComplete = Attack_OnComplete;
            _attackOperation.OnAbort = Attack_Abort;

            OnAttackStateChanged?.Invoke(this);
        }

        private void Attack_Abort(Operation obj)
        {
            OnAttackStateChanged?.Invoke(this);
        }

        private void Attack_InWaitPart(AttackOperation obj)
        {
            OnAttackStateChanged?.Invoke(this);
        }

        private void Attack_OnComplete(Operation obj)
        {
            OnAttackStateChanged?.Invoke(this);
        }
    }
}