using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.DiggerMisc
{
    public class DiggerMind : MonoBehaviour
    {
        public Digger Owner;
        public HeroDetectionTrigger FrontAttackTrigger;

        public float MaxRemovalDistance = 5.0f;

        public float WaitTimeAfterTakeDamage = 1.0f;

        public float AttackSpeed = 0.545f;
        public float AttackDamage = 1f;
        public float AttackForce = 2f;
        public float AttackWaitingPart = 0.3f;
        public Vector2 AttackDamageInterval = new Vector2(0, 1);

        public float RotationSpeed = 1.0f;

        bool mFirstLanding;
        float mTraveledDistance;

        Operation mTakeDamageOperation = new Operation();
        Operation mDealDamageOnContactOperation = new Operation();


        void Start()
        {     
            Owner.OnLanded += Owner_OnOnLanded;    
            Owner.OnTakeDamage += Owner_OnTakeDamage;
            Owner.OnDealDamageOnContact += Owner_OnDealDamageOnContact;
        }
   
        void Owner_OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            mTakeDamageOperation.Execute(WaitTimeAfterTakeDamage);

            Owner.StopMove();
            mDealDamageOnContactOperation.Abort();
        }

      
        void Owner_OnOnLanded(GroundMovementCharacter obj)
        {
            mFirstLanding = true;
        }

        void Owner_OnDealDamageOnContact()
        {
            mDealDamageOnContactOperation.Execute(1.0f);

            Owner.StopMove();
        }

        void Update()
        {
            DetermineCurrentAction();


            if (Owner.IsMoving && !mTakeDamageOperation.InProcess)
            {
                mTraveledDistance += Owner.BodyRelativeVelocity.magnitude * Time.deltaTime;
            }

            mTakeDamageOperation.Process(Time.deltaTime);
            mDealDamageOnContactOperation.Process(Time.deltaTime);
        }

        void DetermineCurrentAction()
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

            if (Owner.AttackOperation.InProcess)
            {
                return;
            }

            if(Owner.IsBreaking)
                return;

            if (Owner.SmoothRotationOperation.InProcess)
                return;
            

            {       
                if (FrontAttackTrigger.EnemiesCount > 0)
                {
                    if (Owner.IsMoving)
                    {
                        Owner.StopMove();
                        
                    }
                    else
                    {
                        Owner.Attack(AttackSpeed, AttackWaitingPart, AttackDamageInterval,
                            DamageType.Cut, AttackDamage, AttackForce, 1.4f, 0.1f);
                    }

                    return;
                }
            }

          
            {
                if (Owner.CantMove || mTraveledDistance >= MaxRemovalDistance)
                {
                    if(Owner.IsMoving)
                    {
                        Owner.StopMove(CalculateAcceleration(Owner.WalkSpeed, 0.5f));
                    }
                    else
                    { 
                        Owner.ChangeDirectionSmooth(
                            Owner.Direction == Direction.Left ? Direction.Right : Direction.Left,
                            Owner.Direction == Direction.Left ? RotationDirection.Clockwise : RotationDirection.AntiClockwise,
                            1.0f / RotationSpeed,
                            Owner.SmoothRotationRoot);

                        mTraveledDistance = 0.0f;
                    }

                    return;
                }
            }

            Owner.SetMovingSpeed(Owner.WalkSpeed);
            Owner.Move(CalculateAcceleration(Owner.WalkSpeed, 0.5f));
        }

        float CalculateAcceleration(float velocity, float distance)
        {
            return (velocity * velocity) / (2.0f * distance);
        }
    }
}