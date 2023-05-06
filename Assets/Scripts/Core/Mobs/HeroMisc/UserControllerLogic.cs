using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class UserControllerLogic : MonoBehaviour
    {
        private bool _needToBreak = false;
        private readonly InputDetectionHelper _jumpInputHelper = new InputDetectionHelper();
        private readonly InputDetectionHelper _attackInputHelper = new InputDetectionHelper();

        [FormerlySerializedAs("Mind")] [SerializeField] private HeroMind _mind;

        [FormerlySerializedAs("DebugMoveLeft")] [SerializeField] private bool _debugMoveLeft = false;
        [FormerlySerializedAs("DebugMoveRight")] [SerializeField] private bool _debugMoveRight = false;
        [FormerlySerializedAs("DebugAttack")] [SerializeField] private bool _debugAttack = false;
        private bool _readAttackValue;
        private bool _prepareComplete;

        private void Start()
        {
            _attackInputHelper.Threshold = 0.4f;

            InputManager.OnInputActionStarted += InputManager_OnInputActionStarted;
            InputManager.OnInputActionEnded += InputManager_OnOnInputActionEnded;
        }
        
        private void InputManager_OnInputActionStarted(InputAction action, InputAction previousAction)
        {
            if (InputManager.HasAction(action, InputAction.Jump))
            {
                _jumpInputHelper.Activate();
            }

            if (InputManager.HasAction(action, InputAction.ActionType1))
            {
                _attackInputHelper.Activate();
                _readAttackValue = true;
                _prepareComplete = false;
            }
        }
        
        private void InputManager_OnOnInputActionEnded(InputAction endedActions, InputAction previousAction)
        {
            if (_readAttackValue)
            {
                if (InputManager.HasAction(endedActions, InputAction.ActionType1))
                {
                    _readAttackValue = false;
                    _prepareComplete = true;

                }
            }
        }
        
        public void Update()
        {
           
            if (_jumpInputHelper.Active)
            {
                if (_mind.CanJump())
                {
                    _jumpInputHelper.Deactivate();
                    _mind.Jump();
                }
            }

            if (_debugAttack)
                _attackInputHelper.Activate();

            if (_attackInputHelper.Active)
            {
                if (_mind.CanAttack(HeroAttackType.MainWeapon))
                {
                    _attackInputHelper.Deactivate();
                    _mind.AttackRequest(HeroAttackType.MainWeapon, InputManager.GetActionValue(InputAction.ActionType1));
                }             
            }

            if (!_attackInputHelper.Active && _readAttackValue)
            {
                //if (Mind.CanAttack(HeroAttackType.MainWeapon))
                    _mind.SetAttackValue(HeroAttackType.MainWeapon, InputManager.GetActionValue(InputAction.ActionType1));
            }

            if (_prepareComplete)
            {
                if (_mind.Owner.PrepareToAttackOperation.InProcess && _mind.Owner.PrepareToAttackOperation.MinimalTimePassed)
                {
                    _prepareComplete = false;
                 
                    _mind.AttackPrepareCompleteRequest(HeroAttackType.MainWeapon,
                        InputManager.GetActionValue(InputAction.ActionType1));
                }
            }

            bool moveLeft = false;
            bool moveRight = false;

            if (InputManager.HasAction(InputManager.Actions, InputAction.MoveLeft) || _debugMoveLeft)
                moveLeft = true;  

            if (InputManager.HasAction(InputManager.Actions, InputAction.MoveRight) || _debugMoveRight)
                moveRight = true;

            bool notMove = true;
            if (moveLeft || moveRight)
            {
                if (_mind.CanMove())
                {
                    if (_mind.CanChangeDirection())
                    {
                        if (moveLeft)
                            _mind.InstantChangeDirection(Direction.Left);
                        if (moveRight)
                            _mind.InstantChangeDirection(Direction.Right);

                        _mind.Move(MovingDirection.Forward, _mind.Owner.RunSpeed);
                    }
                    else
                    {
                        if (_mind.Owner.Direction == Direction.Left)
                        {
                            if (moveLeft)
                                _mind.Move(MovingDirection.Forward, _mind.Owner.WalkSpeed);
                            if (moveRight)
                                _mind.Move(MovingDirection.Backward, _mind.Owner.WalkSpeed);
                        }
                        if (_mind.Owner.Direction == Direction.Right)
                        {
                            if (moveLeft)
                                _mind.Move(MovingDirection.Backward, _mind.Owner.WalkSpeed);
                            if (moveRight)
                                _mind.Move(MovingDirection.Forward, _mind.Owner.WalkSpeed);
                        }       
                    }

                    notMove = false;
                    _needToBreak = true;
                }       
            }
          

            if (_needToBreak && notMove)
            {
                _needToBreak = false;
                _mind.StopMove();
            }

            _jumpInputHelper.Update(Time.deltaTime);
            _attackInputHelper.Update(Time.deltaTime);
        }
    }

    public class InputDetectionHelper
    {
        private float _timer = 0;
        private bool _active;

        public float Threshold { get; set; } = 0.1f;

        public void Activate()
        {
            _timer = 0;
            _active = true;
        }

        public void Update(float dTime)
        {
            if (_active)
            {
                _timer += dTime;
                if (_timer > Threshold)
                {
                    Deactivate();
                }
            }
        }

        public bool Active
        {
            get { return _active; }
        }

        public void Deactivate()
        {
            _active = false;
        }
    }

}