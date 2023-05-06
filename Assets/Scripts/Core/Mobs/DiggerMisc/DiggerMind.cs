using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.DiggerMisc
{
    public class DiggerMind : MonoBehaviour
    {
        private bool _firstLanding;
        private float _traveledDistance;
        
        private readonly Operation _takeDamageOperation = new Operation();
        private readonly Operation _dealDamageOnContactOperation = new Operation();

        [FormerlySerializedAs("Owner")] [SerializeField] private Digger _owner;
        [FormerlySerializedAs("FrontAttackTrigger")] [SerializeField] private HeroDetectionTrigger _frontAttackTrigger;

        [FormerlySerializedAs("MaxRemovalDistance")] [SerializeField] private float _maxRemovalDistance = 5.0f;

        [FormerlySerializedAs("WaitTimeAfterTakeDamage")] [SerializeField] private float _waitTimeAfterTakeDamage = 1.0f;

        [FormerlySerializedAs("AttackSpeed")] [SerializeField] private float _attackSpeed = 0.545f;
        [FormerlySerializedAs("AttackDamage")] [SerializeField] private float _attackDamage = 1f;
        [FormerlySerializedAs("AttackForce")] [SerializeField] private float _attackForce = 2f;
        [FormerlySerializedAs("AttackWaitingPart")] [SerializeField] private float _attackWaitingPart = 0.3f;
        [FormerlySerializedAs("AttackDamageInterval")] [SerializeField] private Vector2 _attackDamageInterval = new Vector2(0, 1);

        [FormerlySerializedAs("RotationSpeed")] [SerializeField] private float _rotationSpeed = 1.0f;
        
        private void Start()
        {     
            _owner.OnLanded += Owner_OnOnLanded;    
            _owner.OnTakeDamage += Owner_OnTakeDamage;
            _owner.OnDealDamageOnContact += Owner_OnDealDamageOnContact;
        }

        private void Owner_OnTakeDamage(Character sender, DamageInfo damageInfo)
        {
            _takeDamageOperation.Execute(_waitTimeAfterTakeDamage);

            _owner.StopMove();
            _dealDamageOnContactOperation.Abort();
        }


        private void Owner_OnOnLanded(GroundMovementCharacter obj)
        {
            _firstLanding = true;
        }

        private void Owner_OnDealDamageOnContact()
        {
            _dealDamageOnContactOperation.Execute(1.0f);

            _owner.StopMove();
        }

        private void Update()
        {
            DetermineCurrentAction();


            if (_owner.IsMoving && !_takeDamageOperation.InProcess)
            {
                _traveledDistance += _owner.BodyRelativeVelocity.magnitude * Time.deltaTime;
            }

            _takeDamageOperation.Process(Time.deltaTime);
            _dealDamageOnContactOperation.Process(Time.deltaTime);
        }

        private void DetermineCurrentAction()
        {
            if (!_owner.IsAlive)
                return;

            if (!_firstLanding)
                return;
            
            if (_takeDamageOperation.InProcess)
            {
                return;
            }
            
            if (_dealDamageOnContactOperation.InProcess)
            {
                return;
            }

            if (_owner.AttackOperation.InProcess)
            {
                return;
            }

            if(_owner.IsBreaking)
                return;

            if (_owner.SmoothRotationOperation.InProcess)
                return;
            

            {       
                if (_frontAttackTrigger.EnemiesCount > 0)
                {
                    if (_owner.IsMoving)
                    {
                        _owner.StopMove();
                        
                    }
                    else
                    {
                        _owner.Attack(_attackSpeed, _attackWaitingPart, _attackDamageInterval,
                            DamageType.Cut, _attackDamage, _attackForce, 1.4f, 0.1f);
                    }

                    return;
                }
            }

          
            {
                if (_owner.CantMove || _traveledDistance >= _maxRemovalDistance)
                {
                    if(_owner.IsMoving)
                    {
                        _owner.StopMove(CalculateAcceleration(_owner.WalkSpeed, 0.5f));
                    }
                    else
                    { 
                        _owner.ChangeDirectionSmooth(
                            _owner.Direction == Direction.Left ? Direction.Right : Direction.Left,
                            _owner.Direction == Direction.Left ? RotationDirection.Clockwise : RotationDirection.AntiClockwise,
                            1.0f / _rotationSpeed,
                            _owner.SmoothRotationRoot);

                        _traveledDistance = 0.0f;
                    }

                    return;
                }
            }

            _owner.SetMovingSpeed(_owner.WalkSpeed);
            _owner.Move(CalculateAcceleration(_owner.WalkSpeed, 0.5f));
        }

        private float CalculateAcceleration(float velocity, float distance)
        {
            return (velocity * velocity) / (2.0f * distance);
        }
    }
}