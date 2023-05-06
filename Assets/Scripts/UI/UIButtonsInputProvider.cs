using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIButtonsInputProvider : MonoBehaviour, IInputProvider
    {
        public event Action<IInputProvider> OnActiveStatusChanged;

        public GameObject Root;

        public UIUserButton MoveLeftBtn;
        public UIUserButton MoveRightBtn;
        public UIUserButton JumpBtn;
        public UIAttackBtn Action1Btn;
        public UIUserButton Action2Btn;

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
            MoveLeftBtn.AutoReturn = false;
            MoveRightBtn.AutoReturn = false;
            JumpBtn.AutoReturn = true;
        //    Action1Btn.AutoReturn = true;
            Action2Btn.AutoReturn = true;

            if (OnActiveStatusChanged != null)
                OnActiveStatusChanged(this);
        }

        private void OnDisable()
        {
            if (OnActiveStatusChanged != null)
                OnActiveStatusChanged(this);
        }

        public void Update()
        {
            InputAction result = InputAction.None;

            if (MoveLeftBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.MoveLeft);
            if (MoveRightBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.MoveRight);
            if (JumpBtn.IsPressed)
                result = InputManager.AddAction(result, InputAction.Jump);
            if (Action1Btn.IsPressed)
            {
                result = InputManager.AddAction(result, InputAction.ActionType1);
                InputManager.SetActionValue(InputAction.ActionType1, Action1Btn.Direction);
            }

            if (Action2Btn.IsPressed)
                result = InputManager.AddAction(result, InputAction.ActionType2);

            MoveLeftBtn.ManualUpdate();
            MoveRightBtn.ManualUpdate();
            JumpBtn.ManualUpdate();
          //  Action1Btn.ManualUpdate();
            Action2Btn.ManualUpdate();

            InputManager.SetAction(this, result);
        }

        public void Show()
        {
            Root.SetActive(true);
        }
        public void Hide()
        {
            Root.SetActive(false);

            MoveLeftBtn.ResetState();
            MoveRightBtn.ResetState();
            JumpBtn.ResetState();
            Action1Btn.ResetState();
            Action2Btn.ResetState();
        }
    }
}