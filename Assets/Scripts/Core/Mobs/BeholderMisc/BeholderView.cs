using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.BeholderMisc
{
    public class BeholderView : MonoBehaviour
    {
        private GameObject _beholderBeamEffectInstance;

        [FormerlySerializedAs("BeamAnchor")] [SerializeField] private Transform _beamAnchor;
        [FormerlySerializedAs("BeholderBeamEffectPrefab")] [SerializeField] private GameObject _beholderBeamEffectPrefab;
        
        [FormerlySerializedAs("Owner")] [SerializeField] private Beholder _owner;
        [FormerlySerializedAs("Animator")] [SerializeField] private Animator _animator;

        [FormerlySerializedAs("MeleeAttackPartClip")] [SerializeField] private AnimationClip _meleeAttackPartClip;
        [FormerlySerializedAs("MeleeAttackWaitPartClip")] [SerializeField] private AnimationClip _meleeAttackWaitPartClip;

        [FormerlySerializedAs("RunAnimationDerivedScaler")] [SerializeField] private float _runAnimationDerivedScaler = 2.0f;

        private void OnEnable()
        {
            _owner.OnTakeDamage += OnTakeDamage;

            _owner.OnAttackStateChanged += Owner_OnAttackStateChanged;

            _owner.OnSmoothRotationStart += Owner_OnSmoothRotationStart;
            _owner.OnSmoothRotationProcess += Owner_OnSmoothRotationProcess;
            _owner.OnSmoothRotationEnd += Owner_OnSmoothRotationEnd;
        }
        
        private void OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            _animator.SetTrigger("getDamage");

            _animator.SetBool("IsAlive", _owner.IsAlive);
        }

        private void Owner_OnAttackStateChanged(Beholder sender)
        {
            if (_owner.AttackOperation.InProcess)
            {
                if (!_owner.AttackOperation.InWaitingPart)
                {
                    var animationLength = _meleeAttackPartClip.length + _meleeAttackWaitPartClip.length;
                    var attackAnimationPart = _meleeAttackPartClip.length / animationLength;
                    var waitAnimationPart = _meleeAttackWaitPartClip.length / animationLength;
                    var useTime = _owner.AttackOperation.Time;
                    var attackTime = _owner.AttackOperation.Time - _owner.AttackOperation.WaitPartTime;
                    var waitTime = _owner.AttackOperation.WaitPartTime;

                    _animator.SetFloat("AttackPartSpeedMultiplier", (attackAnimationPart / (attackTime / useTime)) * (animationLength / useTime));
                    _animator.SetFloat("AttackWaitPartSpeedMultiplier", (waitAnimationPart / (waitTime / useTime)) * (animationLength / useTime));

                    _animator.SetTrigger("Attack");
                    _animator.SetBool("InAttackState", true);

                    _beholderBeamEffectInstance = Object.Instantiate(_beholderBeamEffectPrefab);
                    _beholderBeamEffectInstance.transform.SetParent(_beamAnchor);
                    _beholderBeamEffectInstance.transform.localPosition = Vector3.zero;
                    _beholderBeamEffectInstance.transform.localRotation = Quaternion.identity;
                    _beholderBeamEffectInstance.SetActive(true);

                    var component = _beholderBeamEffectInstance.GetComponent<Animation>();
                    component["BeholderBeam"].normalizedSpeed *= 1.0f / attackTime;
                }
                else
                {
                    Destroy(_beholderBeamEffectInstance);
                    _animator.SetBool("InAttackWaitPartState", true);
                }
            }
            else
            {
                _animator.SetBool("InAttackState", false);
                _animator.SetBool("InAttackWaitPartState", false);
            }
        }

        private void Owner_OnAttackStart(Digger obj)
        {
            _animator.SetBool("InMainWeaponAttackState", true);
        }

        private void Owner_OnAttackEnd(Digger obj)
        {
            _animator.SetBool("InMainWeaponAttackState", false);
        }

        private void Owner_OnSmoothRotationStart()
        {
            _animator.SetInteger("SmoothRotationDirection", _owner.SmoothRotationOperation.RotationDirection == RotationDirection.Clockwise ? 0 : 1);
            _animator.SetBool("InSmoothRotationState", true);
        }

        private void Owner_OnSmoothRotationProcess()
        {
            _animator.SetFloat("SmoothRotationNormTime", _owner.SmoothRotationOperation.NormElapsedTime);
        }

        private void Owner_OnSmoothRotationEnd()
        {
            _animator.SetBool("InSmoothRotationState", false);
            _animator.SetInteger("SmoothRotationDirection", -1);
        }

        private void Update()
        {
            _animator.SetBool("Freefall", !_owner.StayOnGround);

            _animator.SetFloat("BodySpeedInXAxis", Mathf.Abs(_owner.BodyRelativeVelocity.x));

            float movingMultiplier = 0;
            if (Mathf.Abs(_owner.BodyRelativeVelocity.x) > 1)
                movingMultiplier = 1f;
            else if (Mathf.Abs(_owner.BodyRelativeVelocity.x) > 0.1)
                movingMultiplier = 0.3f;

            _animator.SetFloat("MovingMultiplier", movingMultiplier);
            _animator.SetFloat("RunAnimationDerivedScaler", _runAnimationDerivedScaler);
            if (!_owner.StayOnGround)
                _animator.SetFloat("BodySpeedInYAxis", _owner.BodyRelativeVelocity.y);
        }
    }
}