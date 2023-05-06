using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public enum MovingDirection
    {
        Forward,
        Backward,
    }
    public enum Direction
    {
        Right,
        Left,
    }
    public enum RotationDirection
    {
        Clockwise,
        AntiClockwise,
    }
    public class MovingOperationInfo 
    {
        public bool InProcess { get; private set; }
        public bool InAccelerationState { get; private set; }
        public bool InBreakingState { get; private set; }

        private float _movingSpeed;
        private float _brakeAcceleration;
        private float _acceleration;
        private float _desiredMovingSpeed;

        public float MovingSpeed => _movingSpeed;

        public void Move(float movingSpeed, bool instant, float acceleration)
        {
            _desiredMovingSpeed = movingSpeed;
            if (instant)
            {
                _movingSpeed = _desiredMovingSpeed;
                _acceleration = float.MaxValue;
                InAccelerationState = false;
            }
            else
            {
                _acceleration = acceleration;
                InAccelerationState = true;
            }
    
            InBreakingState = false;
            InProcess = true;
        }

        public void Break(bool instant, float brakeAcceleration)
        {
            _desiredMovingSpeed = 0.0f;
            if (instant)
            {
                _movingSpeed = _desiredMovingSpeed;
                _brakeAcceleration = float.MaxValue;
                InBreakingState = false;
                InProcess = false;
            }
            else
            {
                _brakeAcceleration = brakeAcceleration;
                InBreakingState = true;
            }

            InAccelerationState = false;  
        }

        public void Process(float dTime)
        {
            if (InAccelerationState)
            {
                _movingSpeed += _acceleration * dTime;
                if (_movingSpeed >= _desiredMovingSpeed)
                {
                    _movingSpeed = _desiredMovingSpeed;

                    InAccelerationState = false;      
                }
            }
            if (InBreakingState)
            {
                _movingSpeed -= _brakeAcceleration * dTime;
                if (_movingSpeed <= _desiredMovingSpeed)
                {
                    _movingSpeed = _desiredMovingSpeed;

                    InBreakingState = false;
                    InProcess = false;
                }
            }
        }

        public void ResetBreakState()
        {
            if (InBreakingState)
            {
                _movingSpeed = 0.0f;

                InBreakingState = false;
                InProcess = false;
            }
        }

        public void Abort()
        {
            InProcess = false;
            InBreakingState = false;
            InAccelerationState = false;
        }
    }

    public class JumpOperationInfo : Operation
    {
        public bool SyncedInFixedUpdate { private set; get; }
        public Vector2 ExternalVelocity { private set; get; }

        public void Execute(float time, Vector2 externalVelocity)
        {
            base.Execute(time);

            SyncedInFixedUpdate = false;
            ExternalVelocity = externalVelocity;
        }

        public void SyncInFixedUpdate()
        {
            SyncedInFixedUpdate = true;
        }
    }

    public class GroundMovementCharacter : Character
    {
        public event Action<GroundMovementCharacter> OnJump;
        public event Action<GroundMovementCharacter> OnLanded;

        public event Action OnMovingDirectionChanged;

        public event Action OnSmoothRotationStart;
        public event Action OnSmoothRotationEnd;
        public event Action OnSmoothRotationProcess;

        private float _movingSpeed;
        private Direction _direction = Direction.Right;
        private MovingDirection _movingDirection = MovingDirection.Forward;

        private readonly SmoothRotationOperation _smoothRotationOperation = new SmoothRotationOperation();
        private readonly Operation _stunOperation = new Operation();

        [FormerlySerializedAs("Body")] [SerializeField] private GroundMovementBody _body;

        [FormerlySerializedAs("RunSpeed")] [SerializeField] private float _runSpeed = 4;
        [FormerlySerializedAs("WalkSpeed")] [SerializeField] private float _walkSpeed = 2;
        [FormerlySerializedAs("StunForceThresholdValue")] [SerializeField] private float _stunForceThresholdValue = 1.0f;
        [FormerlySerializedAs("StunTime")] [SerializeField] private float _stunTime = 0.5f;   

        public Direction Direction => _direction;
        public MovingDirection MovingDirection => _movingDirection;
        public bool StayOnGround => _body.UnderfootObject != null;
        public Vector2 BodyRelativeVelocity => _body.RelativeVelocity;
        public bool IsMoving => _body.MovingOperationInfo.InProcess;
        public bool IsPushing => _body.MovingOperationInfo.InProcess && _body.MovableObjectOnMovingDirectionDetected;
        public bool IsBreaking => _body.MovingOperationInfo.InBreakingState;
        public bool IsJumping => _body.JumpOperationInfo.InProcess;
        public bool CantMove => _body.StaticObjectDetectedInMovementDirection;
        public SmoothRotationOperation SmoothRotationOperation => _smoothRotationOperation;
        public Operation StunOperation => _stunOperation;
        public float WalkSpeed => _walkSpeed;
        public float StunForceThresholdValue => _stunForceThresholdValue;
        public float RunSpeed => _runSpeed;

        public void SetMovingSpeed(float speed)
        {
            _movingSpeed = speed;
        }

        public void SetMovingDirection(MovingDirection direction)
        {
            if (_movingDirection != direction)
            {
                _body.BreakMoving();

                _movingDirection = direction;

                OnMovingDirectionChanged?.Invoke();
            }
        }

        public void Move()
        {
            _body.StartMoving(_movingSpeed);
        }

        public void Move(float acceleration)
        {
            _body.StartMoving(_movingSpeed, false, acceleration);
        }

        public void StopMove()
        {
            _body.StopMoving();
        }

        public void StopMove(float brakeAcceleration)
        {
            _body.StopMoving(false, brakeAcceleration);
        }
        
        internal void ChangeDirection(Direction direction)
        {
            if (_direction != direction)
            {
                _body.BreakMoving();
                _body.ChangeDirection();

                _direction = direction;

                transform.localRotation = transform.localRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);          
            }
        }

        internal virtual void ChangeDirectionSmooth(Direction direction, RotationDirection rotationDirection, float time, Transform rotationRoot)
        {
            if (_direction != direction)
            {
                _body.BreakMoving();
                _body.ChangeDirection();

                _direction = direction;
               
                transform.localRotation = transform.localRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);
               
                _smoothRotationOperation.Execute(time, rotationDirection, rotationRoot);
                _smoothRotationOperation.OnProcess = SmoothRotation_OnProcess;
                _smoothRotationOperation.OnComplete = SmoothRotation_OnComplete;

                OnSmoothRotationStart?.Invoke();
            }
        }

        private void SmoothRotation_OnProcess(Operation obj)
        {
            OnSmoothRotationProcess?.Invoke();
        }

        private void SmoothRotation_OnComplete(Operation obj)
        {
            OnSmoothRotationEnd?.Invoke();
        }
        
        internal void Jump()
        {  
            _body.Jump();

            OnJump?.Invoke(this);
        }

        protected virtual void InnerUpdate()
        {
            _stunOperation.Process(Time.fixedDeltaTime);
            _smoothRotationOperation.Process(Time.deltaTime);
        }

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            if (IsAlive)
            {
                if (damageInfo.ForceValue > _stunForceThresholdValue)
                {
                    if (_stunOperation.InProcess)
                        _stunOperation.Execute(_stunOperation.Time / 2.0f);
                    else
                        _stunOperation.Execute(_stunTime);

                    _body.AbortAllControlledOperations();
                }

                _body.AddDamageForce(damageInfo.ForceValue, damageInfo.ForceDirection);  
            }
        }

        internal void OnUnderfootObjectChanged(GameObject previous)
        {
            if(_body.UnderfootObject != null)
            {
                OnLanded?.Invoke(this);
            }
        }

        public override void IgnoreCollisions(Collider2D target, bool ignore)
        {
            if (_body.BodyCollider != null)
                Physics2D.IgnoreCollision(target, _body.BodyCollider, ignore);
            if (_body.BodyGroundCollider != null)
                Physics2D.IgnoreCollision(target, _body.BodyGroundCollider, ignore); 
        }
    }
}