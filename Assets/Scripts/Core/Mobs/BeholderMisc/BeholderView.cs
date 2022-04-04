using UnityEngine;

namespace Assets.Scripts.Core.Mobs.BeholderMisc
{
    public class BeholderView : MonoBehaviour
    {
        public Transform BeamAnchor;
        public GameObject BeholderBeamEffectPrefab;
        GameObject BeholderBeamEffectInstance;

        public Beholder Owner;
        public Animator Animator;

        public AnimationClip MeleeAttackPartClip;
        public AnimationClip MeleeAttackWaitPartClip;

        public float RunAnimationDerivedScaler = 2.0f;

        void OnEnable()
        {
            Owner.OnTakeDamage += OnTakeDamage;

            Owner.OnAttackStateChanged += Owner_OnAttackStateChanged;

            Owner.OnSmoothRotationStart += Owner_OnSmoothRotationStart;
            Owner.OnSmoothRotationProcess += Owner_OnSmoothRotationProcess;
            Owner.OnSmoothRotationEnd += Owner_OnSmoothRotationEnd;
        }


        void OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            Animator.SetTrigger("getDamage");

            Animator.SetBool("IsAlive", Owner.IsAlive);
        }

        void Owner_OnAttackStateChanged(Beholder sender)
        {
            if (Owner.AttackOperation.InProcess)
            {
                if (!Owner.AttackOperation.InWaitingPart)
                {
                    var animationLength = MeleeAttackPartClip.length + MeleeAttackWaitPartClip.length;
                    var attackAnimationPart = MeleeAttackPartClip.length / animationLength;
                    var waitAnimationPart = MeleeAttackWaitPartClip.length / animationLength;
                    var useTime = Owner.AttackOperation.Time;
                    var attackTime = Owner.AttackOperation.Time - Owner.AttackOperation.WaitPartTime;
                    var waitTime = Owner.AttackOperation.WaitPartTime;

                    Animator.SetFloat("AttackPartSpeedMultiplier", (attackAnimationPart / (attackTime / useTime)) * (animationLength / useTime));
                    Animator.SetFloat("AttackWaitPartSpeedMultiplier", (waitAnimationPart / (waitTime / useTime)) * (animationLength / useTime));

                    Animator.SetTrigger("Attack");
                    Animator.SetBool("InAttackState", true);

                    BeholderBeamEffectInstance = Object.Instantiate(BeholderBeamEffectPrefab);
                    BeholderBeamEffectInstance.transform.SetParent(BeamAnchor);
                    BeholderBeamEffectInstance.transform.localPosition = Vector3.zero;
                    BeholderBeamEffectInstance.transform.localRotation = Quaternion.identity;
                    BeholderBeamEffectInstance.SetActive(true);

                    var component = BeholderBeamEffectInstance.GetComponent<Animation>();
                    component["BeholderBeam"].normalizedSpeed *= 1.0f / attackTime;
                }
                else
                {
                    Destroy(BeholderBeamEffectInstance);
                    Animator.SetBool("InAttackWaitPartState", true);
                }
            }
            else
            {
                Animator.SetBool("InAttackState", false);
                Animator.SetBool("InAttackWaitPartState", false);
            }
        }

        void Owner_OnAttackStart(Digger obj)
        {
            Animator.SetBool("InMainWeaponAttackState", true);
        }

        void Owner_OnAttackEnd(Digger obj)
        {
            Animator.SetBool("InMainWeaponAttackState", false);
        }

        void Owner_OnSmoothRotationStart()
        {
            Animator.SetInteger("SmoothRotationDirection", Owner.SmoothRotationOperation.RotationDirection == RotationDirection.Clockwise ? 0 : 1);
            Animator.SetBool("InSmoothRotationState", true);
        }
        void Owner_OnSmoothRotationProcess()
        {
            Animator.SetFloat("SmoothRotationNormTime", Owner.SmoothRotationOperation.NormElapsedTime);
        }
        void Owner_OnSmoothRotationEnd()
        {
            Animator.SetBool("InSmoothRotationState", false);
            Animator.SetInteger("SmoothRotationDirection", -1);
        }

        void Update()
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