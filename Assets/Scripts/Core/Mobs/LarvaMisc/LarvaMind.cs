using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.LarvaMisc
{
    public class LarvaMind : MonoBehaviour
    {
        public Larva Owner;
        public HeroDetectionTrigger FrontAttackTrigger;

        public float MaxRemovalDistance = 5.0f;

        public float WaitTimeAfterTakeDamage = 1.0f;

        public float AttackSpeed = 0.545f;
        public float AttackDamage = 1f;
        public float AttackForce = 2f;
        public float AttackWaitingPart = 0.3f;
        public Vector2 AttackDamageInterval = new Vector2(0, 1);

        public float RotationSpeed = 1.0f;

        private bool mFirstLanding;
        private float mTraveledDistance;

        private Operation mTakeDamageOperation = new Operation();
        private Operation mDealDamageOnContactOperation = new Operation();


        private void Start()
        {     
            Owner.OnLanded += Owner_OnOnLanded;    
            Owner.OnTakeDamage += Owner_OnTakeDamage;
            Owner.OnDealDamageOnContact += Owner_OnDealDamageOnContact;
        }

        private void Owner_OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            mTakeDamageOperation.Execute(WaitTimeAfterTakeDamage);

            Owner.StopMove();
            mDealDamageOnContactOperation.Abort();
        }


        private void Owner_OnOnLanded(GroundMovementCharacter obj)
        {
            mFirstLanding = true;
        }

        private void Owner_OnDealDamageOnContact()
        {
            mDealDamageOnContactOperation.Execute(1.0f);

            Owner.StopMove();
        }

        private void Update()
        {
            DetermineCurrentAction();


            if (Owner.IsMoving && !mTakeDamageOperation.InProcess)
            {
                mTraveledDistance += Owner.BodyRelativeVelocity.magnitude * Time.deltaTime;
            }

            mTakeDamageOperation.Process(Time.deltaTime);
            mDealDamageOnContactOperation.Process(Time.deltaTime);
        }

        private void DetermineCurrentAction()
        {
            
            if (!Owner.IsAlive)
                return;

            if (!mFirstLanding)
                return;
            
            if (mTakeDamageOperation.InProcess)
            {
                return;
            }
            
            if (mDealDamageOnContactOperation.InProcess)
            {
                return;
            }

            if(Owner.IsBreaking)
                return;

            if (!Owner.StayOnGround)
                return;

            {
                if (Owner.CantMove || mTraveledDistance >= MaxRemovalDistance)
                {
                    if(Owner.IsMoving)
                    {
                        Owner.StopMove(Owner.WalkSpeed * Owner.WalkSpeed * 0.5f / 0.5f * 0.5f);
                    }
                    else
                    { 
                       // Owner.SetMovingDirection(Owner.MovingDirection == MovingDirection.Backward ? MovingDirection.Forward : MovingDirection.Backward);

                        Owner.ChangeDirection(Owner.Direction == Direction.Left ? Direction.Right : Direction.Left);

                        mTraveledDistance = 0.0f;
                    }

                    return;
                }
            }

            Owner.SetMovingSpeed(Owner.WalkSpeed);
            Owner.Move();
        }
        
    }
}