using UnityEngine;

namespace Assets.Scripts.Core.Mobs.LarvaMisc
{
    public class LarvaView : MonoBehaviour
    {  
        public Larva Owner;
        public Animator Animator;

        public AnimationClip MeleeAttackPartClip;
        public AnimationClip MeleeAttackWaitPartClip;

        public float RunAnimationDerivedScaler = 2.0f;

        private void OnEnable()
        {
            Owner.OnTakeDamage += OnTakeDamage;
            Owner.OnDealDamageOnContact += Owner_OnDealDamageOnContact;

            Owner.OnMovingDirectionChanged += OwnerOnOnMovingDirectionChanged;

            Owner.OnSmoothRotationStart += Owner_OnSmoothRotationStart;
            Owner.OnSmoothRotationProcess += Owner_OnSmoothRotationProcess;
            Owner.OnSmoothRotationEnd += Owner_OnSmoothRotationEnd;
        }

        private void Owner_OnDealDamageOnContact()
        {
            Animator.SetTrigger("getDamage");
        }

        private void OwnerOnOnMovingDirectionChanged()
        {
            Owner.SmoothRotationRoot.localRotation *= Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);
        }

        private void OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            Animator.SetTrigger("getDamage");

            Animator.SetBool("IsAlive", Owner.IsAlive);
        }

        private void Owner_OnAttackStart(Digger obj)
        {
            Animator.SetBool("InMainWeaponAttackState", true);
        }

        private void Owner_OnAttackEnd(Digger obj)
        {
            Animator.SetBool("InMainWeaponAttackState", false);
        }

        private void Owner_OnSmoothRotationStart()
        {
            Animator.SetInteger("SmoothRotationDirection", Owner.SmoothRotationOperation.RotationDirection == RotationDirection.Clockwise ? 0 : 1);
            Animator.SetBool("InSmoothRotationState", true);
        }

        private void Owner_OnSmoothRotationProcess()
        {
            Animator.SetFloat("SmoothRotationNormTime", Owner.SmoothRotationOperation.NormElapsedTime);      
        }

        private void Owner_OnSmoothRotationEnd()
        {
            Animator.SetBool("InSmoothRotationState", false);
            Animator.SetInteger("SmoothRotationDirection", -1);  
        }

        private void Update()
        {
            Animator.SetBool("Freefall", !Owner.StayOnGround);

            Animator.SetFloat("BodySpeedInXAxis", Mathf.Abs(Owner.BodyRelativeVelocity.x));

            float movingMultiplier = 0;
            if (Mathf.Abs(Owner.BodyRelativeVelocity.x) > 1)
                movingMultiplier = 1f;
            else if (Mathf.Abs(Owner.BodyRelativeVelocity.x) > 0.1)
                movingMultiplier = 0.3f;
         
            Animator.SetFloat("MovingMultiplier", movingMultiplier);
            Animator.SetFloat("RunAnimationDerivedScaler", RunAnimationDerivedScaler);
            if (!Owner.StayOnGround)
                Animator.SetFloat("BodySpeedInYAxis", Owner.BodyRelativeVelocity.y);
        }

    }
}