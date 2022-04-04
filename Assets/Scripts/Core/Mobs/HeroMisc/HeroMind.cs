using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroMind : MonoBehaviour
    {
        public Hero Owner;
        public HeroFightLogic FightLogic;
        public List<HeroRepresentationAboutItem> RepresentationAboutItems;

        const int MaxJumpCountInFreefall = 1;
        int mJumpCountInFreefall;
      
        bool mMoveInAttack;
        float mAttackMovingSpeed;
        MovingDirection mAttackMovingDirection;
        Vector2 mAttackMovingTimeInterval;

        void Start()
        {
            Owner.OnTakeDamage += Owner_OnTakeDamage;
            Owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
            Owner.OnLanded += Owner_OnLanded;

            Owner.OnTriggeredSomething += Owner_OnTriggeredSomething;
        }

        void Owner_OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            if (damageInfo.ForceValue >= Owner.StunForceThresholdValue)
            {
                if (Owner.AttackOperation.InProcess)
                    Owner.AbortAttack();
            }
        }

        void Owner_OnLanded(GroundMovementCharacter sender)
        {
            mJumpCountInFreefall = 0;
        }

        void Owner_OnLifeLevelChanged(Character sender)
        {
            
        }

        static readonly List<MapItem> mNoAllocMapItems = new List<MapItem>();
        static readonly List<ExitFromLevel> mNoAllocExitFromLevel = new List<ExitFromLevel>();

        void Owner_OnTriggeredSomething(Hero sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents(mNoAllocMapItems);
            for (var miIndex = 0; miIndex < mNoAllocMapItems.Count; miIndex++)
            {
                var mapItem = mNoAllocMapItems[miIndex];
                if (mapItem.TargetPrefab is HealthBonusItem)
                {
                    if (Owner.Lives < Owner.MaxLives)
                    {
                        var healthBonusItem = mapItem.TargetPrefab as HealthBonusItem;
                        Owner.Treat(this, new TreatmentInfo(healthBonusItem.Power));
                        mapItem.Taken();
                    }
                }
                else
                {
                    Owner.AddNewItemInInventory(mapItem.TargetPrefab);
                    mapItem.Taken();
                } 
            }

            if (!mAutoEnterToLevelOperation.InProcess)
            {
                collider2d.gameObject.GetComponents(mNoAllocExitFromLevel);
                if (mNoAllocExitFromLevel.Count > 0)
                {

                    if (!mAutoExitFromLevelOperation.InProcess)
                    {
                        var exit = mNoAllocExitFromLevel[0];
                        exit.ChangeLevel(this.Owner);
                        ExitFromLevel();
                    }
                }
            }
        }

       
        public void InstantChangeDirection(Direction direction)
        {
            Owner.ChangeDirection(direction);
        }

        public bool CanMove()
        {
            if (mAutoExitFromLevelOperation.InProcess)
                return false;
            if (mAutoEnterToLevelOperation.InProcess)
                return false;
            if (Owner.StunOperation.InProcess)
                return false;
            if (Owner.AttackOperation.InAttackPart && Owner.AttackOperation.BlockMovement)
                return false;

            return true;
        }

        public bool CanChangeDirection()
        {  
            if (Owner.PrepareToAttackOperation.InProcess)
                return false;
            if (Owner.AttackOperation.InAttackPart)
                return false;

            return true;
        }

        public void StopMove()
        {
            if (mAutoExitFromLevelOperation.InProcess)
                return;
            if (mAutoEnterToLevelOperation.InProcess)
                return;

            Owner.StopMove();
        }
        public void Move(MovingDirection direction, float speed)
        {
            Owner.SetMovingSpeed(speed);
            Owner.SetMovingDirection(direction);
            Owner.Move();
        }

        public bool CanJump()
        {
            if (mAutoExitFromLevelOperation.InProcess)
                return false;
            if (mAutoEnterToLevelOperation.InProcess)
                return false;
            if (Owner.StunOperation.InProcess)
                return false;
 
            if (!Owner.StayOnGround && mJumpCountInFreefall >= MaxJumpCountInFreefall)
                return false;

            if (Owner.IsJumping)
                return false;

            if (Owner.AttackOperation.InAttackPart && Owner.AttackOperation.BlockMovement)
                return false;

            return true;
        }

        public void Jump()
        {
            if (!Owner.StayOnGround)
            {
                mJumpCountInFreefall++;
            }
           

            Owner.Jump();
        }

        public bool CanAttack(HeroAttackType attackType)
        {
            if (mAutoExitFromLevelOperation.InProcess)
                return false;
            if (mAutoEnterToLevelOperation.InProcess)
                return false;
            if (Owner.StunOperation.InProcess)
                return false;
            if (Owner.AttackOperation.InAttackPart)
                return false;
            if (Owner.PrepareToAttackOperation.InProcess)
                return false;
            if (Owner.IsJumping)
                return false;
            if (!Owner.StayOnGround)
                return false;

            return true;
        }


        bool attackCombinationPreparationNeeded;
        public void AttackRequest(HeroAttackType attackType, Vector3 value)
        {
            var attackCombination = FightLogic.GetAttackCombination(attackType);

            mMoveInAttack = attackCombination.Move;
            mAttackMovingSpeed = attackCombination.MovingSpeed;
            mAttackMovingDirection = attackCombination.MovingDirection;
            mAttackMovingTimeInterval = attackCombination.MovingTimeInterval * (attackCombination.AttackPartTime) ;
            attackCombinationPreparationNeeded = attackCombination.PreparationNeeded;

            if (Owner.AttackOperation.InProcess)
                Owner.AbortAttack();

            
            if(attackCombinationPreparationNeeded)
                Owner.PrepareToAttack(attackCombination, value);
            else
                Owner.Attack(attackCombination);
        }

        public void SetAttackValue(HeroAttackType mainWeapon, Vector3 value)
        {
            if(attackCombinationPreparationNeeded)
				Owner.UpdatePrepareToAttackState(mainWeapon, value);
        }
        public void AttackPrepareCompleteRequest(HeroAttackType mainWeapon, Vector3 getActionValue)
        {
	        if (attackCombinationPreparationNeeded)
                Owner.CompletePrepareToAttackState(mainWeapon, getActionValue);
        }

        void Update()
        {         
            if (mMoveInAttack)
            {
                if (Owner.AttackOperation.InAttackPart)
                {
                    if(mAttackMovingTimeInterval.x <= Owner.AttackOperation.Timer && Owner.AttackOperation.Timer <= mAttackMovingTimeInterval.y)
                    { 
                        Owner.SetMovingDirection(mAttackMovingDirection);
                        Owner.SetMovingSpeed(mAttackMovingSpeed);
                        Owner.Move();
                    }

                    if (Owner.AttackOperation.Timer > mAttackMovingTimeInterval.y)
                    {
                        Owner.StopMove();
                        mMoveInAttack = false;
                    }
                }  
            }

            mAutoEnterToLevelOperation.Process(Time.deltaTime);
        }

        readonly Operation mAutoEnterToLevelOperation = new Operation();
        readonly Operation mAutoExitFromLevelOperation = new Operation();


        void ExitFromLevel()
        {
            mAutoEnterToLevelOperation.Execute(1);
            mAutoEnterToLevelOperation.OnComplete = mAutoEnterToLevelOperation_OnComplete;

            Owner.Move();
        }
        public void EnterToLevel(ExitFromLevel enterToLevel)
        {                
            var exitDirection = enterToLevel.ExitDirection;
            var enterDirection = enterToLevel.EnterDirection;

            Owner.ChangeDirection(enterDirection);
            Owner.SetMovingSpeed(Owner.WalkSpeed);
            Owner.Move();

            mAutoEnterToLevelOperation.Execute(1);
            mAutoEnterToLevelOperation.OnComplete = mAutoEnterToLevelOperation_OnComplete;

        }

        void mAutoEnterToLevelOperation_OnComplete(Operation obj)
        {
            Owner.StopMove();
        }

        internal void EquipMainWeapon(InventoryItem inventoryItem)
        {
            var representationAboutItem = RepresentationAboutItems.OfType<HeroRepresentationAboutWeaponItem>().FirstOrDefault(i => i.Target == inventoryItem.ItemPrefab);

            foreach (WeaponItemPlacement weaponItemPlacement in representationAboutItem.RequiredFreeSlots)
            {
                var firstOrDefault = Owner.MainWeaponSlots.FirstOrDefault(i => i.Placement == weaponItemPlacement);
                firstOrDefault.ChangeItem(null);
            }
            var weaponSlot = Owner.MainWeaponSlots.FirstOrDefault(i => i.Placement == representationAboutItem.Placement);

            weaponSlot.ChangeItem(inventoryItem);
        }


        
    }
}