using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        const string MainWeaponPrepare = "MainWeaponPrepare";

        const string MainWeaponAttackMovement = "MainWeaponAttackMovement";
        const string MainWeaponAttack = "MainWeaponAttack";
        const string InMainWeaponAttackState = "InMainWeaponAttackState";
        const string InMainWeaponWaitPartState = "InMainWeaponWaitPartState";
        
        const string MainWeaponAttackAnimationSpeedScaler = "MainWeaponAttackSpeedScaler";
        const string MainWeaponWaitAnimationSpeedScaler = "MainWeaponWaitSpeedScaler";

        const string MovingSpeedMultiplier = "MovingSpeedMultiplier";
        const string Jump = "Jump";
        const string Push = "Push";
        const string InJumpState = "InJumpState";
        const string BodySpeedInYAxis = "BodySpeedInYAxis";
        const string BodySpeedInXZPlane = "BodySpeedInXZPlane";
        const string Freefall = "Freefall";
        const string IsAlive = "IsAlive";
        const string GetDamage = "getDamage";

        [FormerlySerializedAs("MainWeaponAttackAnimationsInfo")] [SerializeField] private List<AttackAnimationsInfo> _mainWeaponAttackAnimationsInfo = new List<AttackAnimationsInfo>();

        [FormerlySerializedAs("Owner")] [SerializeField] private Hero _owner;

        [FormerlySerializedAs("Animator")] [SerializeField] private Animator _animator;
        [FormerlySerializedAs("Neck")] [SerializeField] private Transform _neck;

        [FormerlySerializedAs("RightHand")] [SerializeField] private Transform _rightHand;
        [FormerlySerializedAs("LeftHand")] [SerializeField] private Transform _leftHand;
        [FormerlySerializedAs("ShotPoint")] [SerializeField] private Transform _shotPoint;

        [FormerlySerializedAs("RunAnimationDefaultSpeed")] [SerializeField] private float _runAnimationDefaultSpeed = 5.0f;
        [SerializeField] private float _jumpMinimalViewTime = 0.3f;

        private bool _inJumpState;
        private float _jumpTimer;

        public Transform Neck => _neck;
        public Transform LeftHand => _leftHand;
        public Transform RightHand => _rightHand;
        public Transform ShotPoint => _shotPoint;

        private void OnEnable()
        {
            _owner.OnJump += OnJump;

            _owner.OnAttackStarted += OnAttackStarted;
            _owner.OnAttackWaitForNext += OnAttackWaitForNext;
            _owner.OnAttackComplete += OnAttackComplete;
            _owner.OnAttackAbort += OnAttackAbort;

            _owner.OnPrepareToAttackStateChanged += OnPrepareToAttackStateChanged;
            _owner.OnLifeLevelChanged += OnLifeLevelChanged;
        }


        private void OnJump(GroundMovementCharacter sender)
        {
            _animator.SetTrigger(Jump);
            _inJumpState = true;
            _jumpTimer = 0;
        }


        private void OnPrepareToAttackStateChanged(Hero sender)
        {
            if (_owner.PrepareToAttackOperation.InProcess)
            {
                var animationsInfo = _mainWeaponAttackAnimationsInfo.Find(i => i.AttackMovement == _owner.PrepareToAttackOperation.Movement);

                _animator.SetInteger(MainWeaponAttackMovement, animationsInfo.AttackPartAnimationIndex);
                _animator.SetTrigger(MainWeaponPrepare);              
            }
            
        }

        private void OnAttackStarted(Hero sender)
        {
            var animationsInfo = _mainWeaponAttackAnimationsInfo.Find(i => i.AttackMovement == _owner.AttackOperation.Movement);

            var animationLength = animationsInfo.AttackPartAnimation.length + animationsInfo.WaitPartAnimation.length;
            var attackAnimationPart = animationsInfo.AttackPartAnimation.length / animationLength;
            var waitAnimationPart = animationsInfo.WaitPartAnimation.length / animationLength;
            var useTime = _owner.AttackOperation.Time;
            var attackTime = _owner.AttackOperation.Time - _owner.AttackOperation.WaitPartTime;
            var waitTime = _owner.AttackOperation.WaitPartTime;

            _animator.SetFloat(MainWeaponAttackAnimationSpeedScaler, (attackAnimationPart / (attackTime / useTime)) * (animationLength / useTime) );
            _animator.SetFloat(MainWeaponWaitAnimationSpeedScaler, (waitAnimationPart / (waitTime / useTime)) * (animationLength / useTime));
            _animator.SetInteger(MainWeaponAttackMovement, animationsInfo.AttackPartAnimationIndex);
           
            _animator.SetBool(InMainWeaponAttackState, true);
            _animator.SetTrigger(MainWeaponAttack);
        }

        private void OnAttackWaitForNext(Hero sender)
        {
            _animator.ResetTrigger(MainWeaponAttack);
            _animator.SetBool(InMainWeaponAttackState, false);
            _animator.SetBool(InMainWeaponWaitPartState, true);
        }

        private void OnAttackComplete(Hero sender)
        {
            _animator.SetBool(InMainWeaponWaitPartState, false);
        }

        private void OnAttackAbort(Hero sender)
        {
            _animator.ResetTrigger(MainWeaponAttack);
            _animator.SetBool(InMainWeaponAttackState, false);
            _animator.SetBool(InMainWeaponWaitPartState, false);
        }

        private void OnLifeLevelChanged(Character sender)
        {
            _animator.SetTrigger(GetDamage);
            _animator.SetBool(IsAlive, _owner.IsAlive);
        }

        private void Update()
        {
            _animator.SetBool(Freefall, !_owner.StayOnGround);

            _animator.SetFloat(BodySpeedInXZPlane, Mathf.Abs((float) _owner.BodyRelativeVelocity.x));
            if (_owner.IsMoving)
            {
                var movingSign = _owner.MovingDirection == MovingDirection.Backward ? -1.0f : 1.0f;
                _animator.SetFloat(MovingSpeedMultiplier, movingSign * Mathf.Abs(_owner.BodyRelativeVelocity.x) / _runAnimationDefaultSpeed );
            }
            else
                _animator.SetFloat(MovingSpeedMultiplier, 1);

            _animator.SetBool(InJumpState, _inJumpState);
            if (!_owner.StayOnGround)
                _animator.SetFloat(BodySpeedInYAxis, _owner.BodyRelativeVelocity.y);

            _animator.SetBool(Push, _owner.IsPushing);

            if (_inJumpState)
            {
                _jumpTimer += Time.deltaTime;
                if (_jumpTimer >= _jumpMinimalViewTime)
                    _inJumpState = false;
            }
        }
    }
}