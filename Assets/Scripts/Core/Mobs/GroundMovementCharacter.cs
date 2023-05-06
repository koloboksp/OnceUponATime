using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

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

        private float mMovingSpeed;
        private float mBrakeAcceleration;
        private float mAcceleration;
        private float mDesiredMovingSpeed;

        public float MovingSpeed => mMovingSpeed;

        public void Move(float movingSpeed, bool instant, float acceleration)
        {
            mDesiredMovingSpeed = movingSpeed;
            if (instant)
            {
                mMovingSpeed = mDesiredMovingSpeed;
                mAcceleration = float.MaxValue;
                InAccelerationState = false;
            }
            else
            {
                mAcceleration = acceleration;
                InAccelerationState = true;
            }
    
            InBreakingState = false;
            InProcess = true;
        }

        public void Break(bool instant, float brakeAcceleration)
        {
            mDesiredMovingSpeed = 0.0f;
            if (instant)
            {
                mMovingSpeed = mDesiredMovingSpeed;
                mBrakeAcceleration = float.MaxValue;
                InBreakingState = false;
                InProcess = false;
            }
            else
            {
                mBrakeAcceleration = brakeAcceleration;
                InBreakingState = true;
            }

            InAccelerationState = false;  
        }

        public void Process(float dTime)
        {
            if (InAccelerationState)
            {
                mMovingSpeed += mAcceleration * dTime;
                if (mMovingSpeed >= mDesiredMovingSpeed)
                {
                    mMovingSpeed = mDesiredMovingSpeed;

                    InAccelerationState = false;      
                }
            }
            if (InBreakingState)
            {
                mMovingSpeed -= mBrakeAcceleration * dTime;
                if (mMovingSpeed <= mDesiredMovingSpeed)
                {
                    mMovingSpeed = mDesiredMovingSpeed;

                    InBreakingState = false;
                    InProcess = false;
                }
            }
        }

        public void ResetBreakState()
        {
            if (InBreakingState)
            {
                mMovingSpeed = 0.0f;

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

        private float mMovingSpeed;
        private Direction mDirection = Direction.Right;
        private MovingDirection mMovingDirection = MovingDirection.Forward;

        private readonly SmoothRotationOperation mSmoothRotationOperation = new SmoothRotationOperation();
        private readonly Operation mStunOperation = new Operation();

        public GroundMovementBody Body;

        public float RunSpeed = 4;
        public float WalkSpeed = 2;
        public float StunForceThresholdValue = 1.0f;
        public float StunTime = 0.5f;   

        public Direction Direction => mDirection;
        public MovingDirection MovingDirection => mMovingDirection;
        public bool StayOnGround => Body.UnderfootObject != null;
        public Vector2 BodyRelativeVelocity => Body.RelativeVelocity;
        public bool IsMoving => Body.MovingOperationInfo.InProcess;
        public bool IsPushing => Body.MovingOperationInfo.InProcess && Body.MovableObjectOnMovingDirectionDetected;
        public bool IsBreaking => Body.MovingOperationInfo.InBreakingState;
        public bool IsJumping => Body.JumpOperationInfo.InProcess;
        public bool CantMove => Body.StaticObjectDetectedInMovementDirection;
        public SmoothRotationOperation SmoothRotationOperation => mSmoothRotationOperation;
        public Operation StunOperation => mStunOperation;

        public void SetMovingSpeed(float speed)
        {
            mMovingSpeed = speed;
        }

        public void SetMovingDirection(MovingDirection direction)
        {
            if (mMovingDirection != direction)
            {
                Body.BreakMoving();

                mMovingDirection = direction;
 
                if (OnMovingDirectionChanged != null)
                    OnMovingDirectionChanged();
            }
        }

        public void Move()
        {
            Body.StartMoving(mMovingSpeed);
        }

        public void Move(float acceleration)
        {
            Body.StartMoving(mMovingSpeed, false, acceleration);
        }

        public void StopMove()
        {
            Body.StopMoving();
        }

        public void StopMove(float brakeAcceleration)
        {
            Body.StopMoving(false, brakeAcceleration);
        }

      
        internal void ChangeDirection(Direction direction)
        {
            if (mDirection != direction)
            {
                Body.BreakMoving();
                Body.ChangeDirection();

                mDirection = direction;

                transform.localRotation = transform.localRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);          
            }
        }

        internal virtual void ChangeDirectionSmooth(Direction direction, RotationDirection rotationDirection, float time, Transform rotationRoot)
        {
            if (mDirection != direction)
            {
                Body.BreakMoving();
                Body.ChangeDirection();

                mDirection = direction;
               
                transform.localRotation = transform.localRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);
               
                mSmoothRotationOperation.Execute(time, rotationDirection, rotationRoot);
                mSmoothRotationOperation.OnProcess = SmoothRotation_OnProcess;
                mSmoothRotationOperation.OnComplete = SmoothRotation_OnComplete;

                if (OnSmoothRotationStart != null)
                    OnSmoothRotationStart();
            }
        }

        private void SmoothRotation_OnProcess(Operation obj)
        {
            if (OnSmoothRotationProcess != null)
                OnSmoothRotationProcess();
        }

        private void SmoothRotation_OnComplete(Operation obj)
        {
            if (OnSmoothRotationEnd != null)
                OnSmoothRotationEnd();
        }

        
        internal void Jump()
        {  
            Body.Jump();

            if (OnJump != null)
                OnJump(this);
        }

        protected virtual void InnerUpdate()
        {
            mStunOperation.Process(Time.fixedDeltaTime);
            mSmoothRotationOperation.Process(Time.deltaTime);
        }

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            if (IsAlive)
            {
                if (damageInfo.ForceValue > StunForceThresholdValue)
                {
                    if (mStunOperation.InProcess)
                        mStunOperation.Execute(mStunOperation.Time / 2.0f);
                    else
                        mStunOperation.Execute(StunTime);

                    Body.AbortAllControlledOperations();
                }

                Body.AddDamageForce(damageInfo.ForceValue, damageInfo.ForceDirection);  
            }
        }

        internal void OnUnderfootObjectChanged(GameObject previous)
        {
            if(Body.UnderfootObject != null)
                if (OnLanded != null)
                    OnLanded(this);
        }

        public override void IgnoreCollisions(Collider2D target, bool ignore)
        {
            if (Body.BodyCollider != null)
                Physics2D.IgnoreCollision(target, Body.BodyCollider, ignore);
            if (Body.BodyGroundCollider != null)
                Physics2D.IgnoreCollision(target, Body.BodyGroundCollider, ignore); 
        }
    }
}