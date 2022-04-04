using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    [Serializable]
    public class AttackAnimationsInfo
    {
        public HeroAvailableStrikes AttackMovement;
        public int AttackPartAnimationIndex = 0;
        public AnimationClip AttackPartAnimation;
        public AnimationClip WaitPartAnimation;
    }

    public class HeroView : MonoBehaviour
    {
        public string MainWeaponPrepare = "MainWeaponPrepare";

        public string MainWeaponAttackMovement = "MainWeaponAttackMovement";
        public string MainWeaponAttack = "MainWeaponAttack";
        public string InMainWeaponAttackState = "InMainWeaponAttackState";
        public string InMainWeaponWaitPartState = "InMainWeaponWaitPartState";
        
        public string MainWeaponAttackAnimationSpeedScaler = "MainWeaponAttackSpeedScaler";
        public string MainWeaponWaitAnimationSpeedScaler = "MainWeaponWaitSpeedScaler";

        public List<AttackAnimationsInfo> MainWeaponAttackAnimationsInfo = new List<AttackAnimationsInfo>();

        public Hero Owner;

        public Animator Animator;
        public Transform Neck;

        public Transform RightHand;
        public Transform LeftHand;
        public Transform ShotPoint;

      //  public float RunAnimationDerivedScaler = 2.0f;
        public float RunAnimationDefaultSpeed = 5.0f;

        void OnEnable()
        {
            Owner.OnJump += OnJump;

            Owner.OnAttackStarted += OnAttackStarted;
            Owner.OnAttackWaitForNext += OnAttackWaitForNext;
            Owner.OnAttackComplete += OnAttackComplete;
            Owner.OnAttackAbort += OnAttackAbort;

            Owner.OnPrepareToAttackStateChanged += OnPrepareToAttackStateChanged;
            Owner.OnLifeLevelChanged += OnLifeLevelChanged;
        }

       

        void OnJump(GroundMovementCharacter sender)
        {
            Animator.SetTrigger("Jump");
            mInJumpState = true;
            mJumpTimer = 0;
        }

        bool mInJumpState;
        float mJumpTimer;
        float mJumpMinimalViewTime = 0.3f;

        void OnPrepareToAttackStateChanged(Hero sender)
        {
            if (Owner.PrepareToAttackOperation.InProcess)
            {
                var animationsInfo = MainWeaponAttackAnimationsInfo.Find(i => i.AttackMovement == Owner.PrepareToAttackOperation.Movement);

                Animator.SetInteger(MainWeaponAttackMovement, animationsInfo.AttackPartAnimationIndex);
                Animator.SetTrigger(MainWeaponPrepare);              
            }
            
        }

        void OnAttackStarted(Hero sender)
        {
            var animationsInfo = MainWeaponAttackAnimationsInfo.Find(i => i.AttackMovement == Owner.AttackOperation.Movement);

            var animationLength = animationsInfo.AttackPartAnimation.length + animationsInfo.WaitPartAnimation.length;
            var attackAnimationPart = animationsInfo.AttackPartAnimation.length / animationLength;
            var waitAnimationPart = animationsInfo.WaitPartAnimation.length / animationLength;
            var useTime = Owner.AttackOperation.Time;
            var attackTime = Owner.AttackOperation.Time - Owner.AttackOperation.WaitPartTime;
            var waitTime = Owner.AttackOperation.WaitPartTime;

            Animator.SetFloat(MainWeaponAttackAnimationSpeedScaler, (attackAnimationPart / (attackTime / useTime)) * (animationLength / useTime) );
            Animator.SetFloat(MainWeaponWaitAnimationSpeedScaler, (waitAnimationPart / (waitTime / useTime)) * (animationLength / useTime));
            Animator.SetInteger(MainWeaponAttackMovement, animationsInfo.AttackPartAnimationIndex);
           
            Animator.SetBool(InMainWeaponAttackState, true);
            Animator.SetTrigger(MainWeaponAttack);
        }

        void OnAttackWaitForNext(Hero sender)
        {
            Animator.ResetTrigger(MainWeaponAttack);
            Animator.SetBool(InMainWeaponAttackState, false);
            Animator.SetBool(InMainWeaponWaitPartState, true);
        }
        void OnAttackComplete(Hero sender)
        {
            Animator.SetBool(InMainWeaponWaitPartState, false);
        }
        void OnAttackAbort(Hero sender)
        {
            Animator.ResetTrigger(MainWeaponAttack);
            Animator.SetBool(InMainWeaponAttackState, false);
            Animator.SetBool(InMainWeaponWaitPartState, false);
        }
        void OnLifeLevelChanged(Character sender)
        {
            Animator.SetTrigger("getDamage");
            Animator.SetBool("IsAlive", Owner.IsAlive);
        }

        void Update()
        {
            Animator.SetBool("Freefall", !Owner.StayOnGround);

            Animator.SetFloat("BodySpeedInXZPlane", Mathf.Abs((float) Owner.BodyRelativeVelocity.x));
            if (Owner.IsMoving)
            {
                var movingSign = Owner.MovingDirection == MovingDirection.Backward ? -1.0f : 1.0f;
                Animator.SetFloat("MovingSpeedMultiplier", movingSign * Mathf.Abs(Owner.BodyRelativeVelocity.x) / RunAnimationDefaultSpeed );
            }
            else
                Animator.SetFloat("MovingSpeedMultiplier", 1);

           // Animator.SetFloat("RunAnimationDerivedScaler", RunAnimationDerivedScaler);
            Animator.SetBool("InJumpState", mInJumpState);
            if (!Owner.StayOnGround)
                Animator.SetFloat("BodySpeedInYAxis", Owner.BodyRelativeVelocity.y);

            Animator.SetBool("Push", Owner.IsPushing);

            if (mInJumpState)
            {
                mJumpTimer += Time.deltaTime;
                if (mJumpTimer >= mJumpMinimalViewTime)
                    mInJumpState = false;
            }
        }
    }
}