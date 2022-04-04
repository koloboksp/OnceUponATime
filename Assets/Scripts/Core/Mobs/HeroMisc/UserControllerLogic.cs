using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class UserControllerLogic : MonoBehaviour
    {
        bool mNeedToBreak = false;

        public HeroMind Mind;

        public bool DebugMoveLeft = false;
        public bool DebugMoveRight = false;
        public bool DebugAttack = false;

        readonly InputDetectionHelper mJumpInputHelper = new InputDetectionHelper();
        readonly InputDetectionHelper mAttackInputHelper = new InputDetectionHelper();

        void Start()
        {
            mAttackInputHelper.Threshold = 0.4f;

            InputManager.OnInputActionStarted += InputManager_OnInputActionStarted;
            InputManager.OnInputActionEnded += InputManager_OnOnInputActionEnded;
        }

     
        void InputManager_OnInputActionStarted(InputAction action, InputAction previousAction)
        {
            if (InputManager.HasAction(action, InputAction.Jump))
            {
                mJumpInputHelper.Activate();
            }

            if (InputManager.HasAction(action, InputAction.ActionType1))
            {
                mAttackInputHelper.Activate();
                mReadAttackValue = true;
                mPrepareComplete = false;
            }
        }

     
        void InputManager_OnOnInputActionEnded(InputAction endedActions, InputAction previousAction)
        {
            if (mReadAttackValue)
            {
                if (InputManager.HasAction(endedActions, InputAction.ActionType1))
                {
                    mReadAttackValue = false;
                    mPrepareComplete = true;

                }
            }
        }

        bool mReadAttackValue;
        bool mPrepareComplete;

        public void Update()
        {
           
            if (mJumpInputHelper.Active)
            {
                if (Mind.CanJump())
                {
                    mJumpInputHelper.Deactivate();
                    Mind.Jump();
                }
            }

            if (DebugAttack)
                mAttackInputHelper.Activate();

            if (mAttackInputHelper.Active)
            {
                if (Mind.CanAttack(HeroAttackType.MainWeapon))
                {
                    mAttackInputHelper.Deactivate();
                    Mind.AttackRequest(HeroAttackType.MainWeapon, InputManager.GetActionValue(InputAction.ActionType1));
                }             
            }

            if (!mAttackInputHelper.Active && mReadAttackValue)
            {
                //if (Mind.CanAttack(HeroAttackType.MainWeapon))
                    Mind.SetAttackValue(HeroAttackType.MainWeapon, InputManager.GetActionValue(InputAction.ActionType1));
            }

            if (mPrepareComplete)
            {
                if (Mind.Owner.PrepareToAttackOperation.InProcess)
                    if (Mind.Owner.PrepareToAttackOperation.MinimalTimePassed)
                {
                    mPrepareComplete = false;
                 
                        Mind.AttackPrepareCompleteRequest(HeroAttackType.MainWeapon,
                            InputManager.GetActionValue(InputAction.ActionType1));
                }
            }

            bool moveLeft = false;
            bool moveRight = false;

            if (InputManager.HasAction(InputManager.Actions, InputAction.MoveLeft) || DebugMoveLeft)
                moveLeft = true;  

            if (InputManager.HasAction(InputManager.Actions, InputAction.MoveRight) || DebugMoveRight)
                moveRight = true;

            bool notMove = true;
            if (moveLeft || moveRight)
            {
                if (Mind.CanMove())
                {
                    if (Mind.CanChangeDirection())
                    {
                        if (moveLeft)
                            Mind.InstantChangeDirection(Direction.Left);
                        if (moveRight)
                            Mind.InstantChangeDirection(Direction.Right);

                        Mind.Move(MovingDirection.Forward, Mind.Owner.RunSpeed);
                    }
                    else
                    {
                        if (Mind.Owner.Direction == Direction.Left)
                        {
                            if (moveLeft)
                                Mind.Move(MovingDirection.Forward, Mind.Owner.WalkSpeed);
                            if (moveRight)
                                Mind.Move(MovingDirection.Backward, Mind.Owner.WalkSpeed);
                        }
                        if (Mind.Owner.Direction == Direction.Right)
                        {
                            if (moveLeft)
                                Mind.Move(MovingDirection.Backward, Mind.Owner.WalkSpeed);
                            if (moveRight)
                                Mind.Move(MovingDirection.Forward, Mind.Owner.WalkSpeed);
                        }       
                    }

                    notMove = false;
                    mNeedToBreak = true;
                }       
            }
          

            if (mNeedToBreak && notMove)
            {
                mNeedToBreak = false;
                Mind.StopMove();
            }

            mJumpInputHelper.Update(Time.deltaTime);
            mAttackInputHelper.Update(Time.deltaTime);
        }
    }

    public class InputDetectionHelper
    {
        float mTimer = 0;
        public float Threshold { get; set; } = 0.1f;

        bool mActive;

        
        public void Activate()
        {
            mTimer = 0;
            mActive = true;
        }

        public void Update(float dTime)
        {
            if (mActive)
            {
                mTimer += dTime;
                if (mTimer > Threshold)
                {
                    Deactivate();
                }
            }
        }

        public bool Active
        {
            get { return mActive; }
        }

        public void Deactivate()
        {
            mActive = false;
        }
    }

}