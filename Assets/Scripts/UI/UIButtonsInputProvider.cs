using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    public class UIButtonsInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<IInputProvider> OnActiveStatusChanged;

        [FormerlySerializedAs("Root")] [SerializeField] private GameObject _root;

        [FormerlySerializedAs("MoveLeftBtn")] [SerializeField] private UIUserButton _moveLeftBtn;
        [FormerlySerializedAs("MoveRightBtn")] [SerializeField] private UIUserButton _moveRightBtn;
        [FormerlySerializedAs("JumpBtn")] [SerializeField] private UIUserButton _jumpBtn;
        [FormerlySerializedAs("Action1Btn")] [SerializeField] private UIAttackBtn _action1Btn;
        [FormerlySerializedAs("Action2Btn")] [SerializeField] private UIUserButton _action2Btn;

        public InputProviderType ProviderType { get; } = InputProviderType.UI;

        public bool Active { get { return isActiveAndEnabled; } }
        
        private void Awake()
        {
            InputManager.AddProvider(this);
        }

        private void OnDestroy()
        {
            InputManager.RemoveProvider(this);
        }

        private void OnEnable()
        {
            _moveLeftBtn.AutoReturn = false;
            _moveRightBtn.AutoReturn = false;
            _jumpBtn.AutoReturn = true;
        //    Action1Btn.AutoReturn = true;
            _action2Btn.AutoReturn = true;

            OnActiveStatusChanged?.Invoke(this);
        }

        private void OnDisable()
        {
            OnActiveStatusChanged?.Invoke(this);
        }

        public void Update()
        {
            InputAction result = InputAction.None;

            if (_moveLeftBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.MoveLeft);
            if (_moveRightBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.MoveRight);
            if (_jumpBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.Jump);
            if (_action1Btn.IsPressed)
            {
                result = InputManager.AddAction(result, InputAction.ActionType1);
                InputManager.SetActionValue(InputAction.ActionType1, _action1Btn.Direction);
            }

            if (_action2Btn.IsPressed)
                result = InputManager.AddAction(result, InputAction.ActionType2);

            _moveLeftBtn.ManualUpdate();
            _moveRightBtn.ManualUpdate();
            _jumpBtn.ManualUpdate();
          //  Action1Btn.ManualUpdate();
            _action2Btn.ManualUpdate();

            InputManager.SetAction(this, result);
        }

        public void Show()
        {
            _root.SetActive(true);
        }
        public void Hide()
        {
            _root.SetActive(false);

            _moveLeftBtn.ResetState();
            _moveRightBtn.ResetState();
            _jumpBtn.ResetState();
            _action1Btn.ResetState();
            _action2Btn.ResetState();
        }
    }
}