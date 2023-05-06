using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroMind : MonoBehaviour
    {
        private const int MaxJumpCountInFreefall = 1;
        private static readonly List<MapItem> NoAllocMapItems = new List<MapItem>();
        private static readonly List<ExitFromLevel> NoAllocExitFromLevel = new List<ExitFromLevel>();

        [FormerlySerializedAs("Owner")] [SerializeField] private Hero _owner;
        [FormerlySerializedAs("FightLogic")] [SerializeField] private HeroFightLogic _fightLogic;
        [FormerlySerializedAs("RepresentationAboutItems")] [SerializeField] private List<HeroRepresentationAboutItem> _representationAboutItems;
        
        private int _jumpCountInFreefall;
        private bool _moveInAttack;
        private float _attackMovingSpeed;
        private MovingDirection _attackMovingDirection;
        private Vector2 _attackMovingTimeInterval;

        private readonly Operation _autoEnterToLevelOperation = new Operation();
        private readonly Operation _autoExitFromLevelOperation = new Operation();
        private bool _attackCombinationPreparationNeeded;

        public List<HeroRepresentationAboutItem> RepresentationAboutItems => _representationAboutItems;
        
        public Hero Owner => _owner;
        
        private void Start()
        {
            _owner.OnTakeDamage += Owner_OnTakeDamage;
            _owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
            _owner.OnLanded += Owner_OnLanded;

            _owner.OnTriggeredSomething += Owner_OnTriggeredSomething;
        }

        private void Owner_OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            if (damageInfo.ForceValue >= _owner.StunForceThresholdValue)
            {
                if (_owner.AttackOperation.InProcess)
                    _owner.AbortAttack();
            }
        }

        private void Owner_OnLanded(GroundMovementCharacter sender)
        {
            _jumpCountInFreefall = 0;
        }

        private void Owner_OnLifeLevelChanged(Character sender)
        {
            
        }
        
        private void Owner_OnTriggeredSomething(Hero sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents(NoAllocMapItems);
            for (var miIndex = 0; miIndex < NoAllocMapItems.Count; miIndex++)
            {
                var mapItem = NoAllocMapItems[miIndex];
                if (mapItem.TargetPrefab is HealthBonusItem)
                {
                    if (_owner.Lives < _owner.MaxLives)
                    {
                        var healthBonusItem = mapItem.TargetPrefab as HealthBonusItem;
                        _owner.Treat(this, new TreatmentInfo(healthBonusItem.Power));
                        mapItem.Taken();
                    }
                }
                else
                {
                    _owner.AddNewItemInInventory(mapItem.TargetPrefab);
                    mapItem.Taken();
                } 
            }

            if (!_autoEnterToLevelOperation.InProcess)
            {
                collider2d.gameObject.GetComponents(NoAllocExitFromLevel);
                if (NoAllocExitFromLevel.Count > 0)
                {

                    if (!_autoExitFromLevelOperation.InProcess)
                    {
                        var exit = NoAllocExitFromLevel[0];
                        exit.ChangeLevel(this._owner);
                        ExitFromLevel();
                    }
                }
            }
        }
        
        public void InstantChangeDirection(Direction direction)
        {
            _owner.ChangeDirection(direction);
        }

        public bool CanMove()
        {
            if (_autoExitFromLevelOperation.InProcess)
                return false;
            if (_autoEnterToLevelOperation.InProcess)
                return false;
            if (_owner.StunOperation.InProcess)
                return false;
            if (_owner.AttackOperation.InAttackPart && _owner.AttackOperation.BlockMovement)
                return false;

            return true;
        }

        public bool CanChangeDirection()
        {  
            if (_owner.PrepareToAttackOperation.InProcess)
                return false;
            if (_owner.AttackOperation.InAttackPart)
                return false;

            return true;
        }

        public void StopMove()
        {
            if (_autoExitFromLevelOperation.InProcess)
                return;
            if (_autoEnterToLevelOperation.InProcess)
                return;

            _owner.StopMove();
        }
        public void Move(MovingDirection direction, float speed)
        {
            _owner.SetMovingSpeed(speed);
            _owner.SetMovingDirection(direction);
            _owner.Move();
        }

        public bool CanJump()
        {
            if (_autoExitFromLevelOperation.InProcess)
                return false;
            if (_autoEnterToLevelOperation.InProcess)
                return false;
            if (_owner.StunOperation.InProcess)
                return false;
 
            if (!_owner.StayOnGround && _jumpCountInFreefall >= MaxJumpCountInFreefall)
                return false;

            if (_owner.IsJumping)
                return false;

            if (_owner.AttackOperation.InAttackPart && _owner.AttackOperation.BlockMovement)
                return false;

            return true;
        }

        public void Jump()
        {
            if (!_owner.StayOnGround)
            {
                _jumpCountInFreefall++;
            }
           

            _owner.Jump();
        }

        public bool CanAttack(HeroAttackType attackType)
        {
            if (_autoExitFromLevelOperation.InProcess)
                return false;
            if (_autoEnterToLevelOperation.InProcess)
                return false;
            if (_owner.StunOperation.InProcess)
                return false;
            if (_owner.AttackOperation.InAttackPart)
                return false;
            if (_owner.PrepareToAttackOperation.InProcess)
                return false;
            if (_owner.IsJumping)
                return false;
            if (!_owner.StayOnGround)
                return false;

            return true;
        }
        
        public void AttackRequest(HeroAttackType attackType, Vector3 value)
        {
            var attackCombination = _fightLogic.GetAttackCombination(attackType);

            _moveInAttack = attackCombination.Move;
            _attackMovingSpeed = attackCombination.MovingSpeed;
            _attackMovingDirection = attackCombination.MovingDirection;
            _attackMovingTimeInterval = attackCombination.MovingTimeInterval * (attackCombination.AttackPartTime) ;
            _attackCombinationPreparationNeeded = attackCombination.PreparationNeeded;

            if (_owner.AttackOperation.InProcess)
                _owner.AbortAttack();

            
            if(_attackCombinationPreparationNeeded)
                _owner.PrepareToAttack(attackCombination, value);
            else
                _owner.Attack(attackCombination);
        }

        public void SetAttackValue(HeroAttackType mainWeapon, Vector3 value)
        {
            if(_attackCombinationPreparationNeeded)
				_owner.UpdatePrepareToAttackState(mainWeapon, value);
        }
        public void AttackPrepareCompleteRequest(HeroAttackType mainWeapon, Vector3 getActionValue)
        {
	        if (_attackCombinationPreparationNeeded)
                _owner.CompletePrepareToAttackState(mainWeapon, getActionValue);
        }

        private void Update()
        {         
            if (_moveInAttack)
            {
                if (_owner.AttackOperation.InAttackPart)
                {
                    if(_attackMovingTimeInterval.x <= _owner.AttackOperation.Timer && _owner.AttackOperation.Timer <= _attackMovingTimeInterval.y)
                    { 
                        _owner.SetMovingDirection(_attackMovingDirection);
                        _owner.SetMovingSpeed(_attackMovingSpeed);
                        _owner.Move();
                    }

                    if (_owner.AttackOperation.Timer > _attackMovingTimeInterval.y)
                    {
                        _owner.StopMove();
                        _moveInAttack = false;
                    }
                }  
            }

            _autoEnterToLevelOperation.Process(Time.deltaTime);
        }

        private void ExitFromLevel()
        {
            _autoEnterToLevelOperation.Execute(1);
            _autoEnterToLevelOperation.OnComplete = mAutoEnterToLevelOperation_OnComplete;

            _owner.Move();
        }
        
        public void EnterToLevel(ExitFromLevel enterToLevel)
        {                
            var exitDirection = enterToLevel.ExitDirection;
            var enterDirection = enterToLevel.EnterDirection;

            _owner.ChangeDirection(enterDirection);
            _owner.SetMovingSpeed(_owner.WalkSpeed);
            _owner.Move();

            _autoEnterToLevelOperation.Execute(1);
            _autoEnterToLevelOperation.OnComplete = mAutoEnterToLevelOperation_OnComplete;

        }

        private void mAutoEnterToLevelOperation_OnComplete(Operation obj)
        {
            _owner.StopMove();
        }

        internal void EquipMainWeapon(InventoryItem inventoryItem)
        {
            var representationAboutItem = _representationAboutItems.OfType<HeroRepresentationAboutWeaponItem>().FirstOrDefault(i => i.Target == inventoryItem.ItemPrefab);

            foreach (WeaponItemPlacement weaponItemPlacement in representationAboutItem.RequiredFreeSlots)
            {
                var firstOrDefault = _owner.MainWeaponSlots.FirstOrDefault(i => i.Placement == weaponItemPlacement);
                firstOrDefault.ChangeItem(null);
            }
            var weaponSlot = _owner.MainWeaponSlots.FirstOrDefault(i => i.Placement == representationAboutItem.Placement);

            weaponSlot.ChangeItem(inventoryItem);
        }
    }
}