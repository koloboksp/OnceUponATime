using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Flags]
    public enum InputAction
    {
        None = 0,
        Esc = 1 << 6,

        MoveLeft = 1 << 0,
        MoveRight = 1 << 1,
        Jump = 1 << 2,
        ActionType1 = 1 << 4,
        ActionType2 = 1 << 5,

        UIEnter = 1 << 7,
        UIMoveLeft = 1 << 8,
        UIMoveRight = 1 << 9,
        UIMoveUp = 1 << 10,
        UIMoveDown = 1 << 11,
    }

    public enum InputProviderType
    {
        Keyboard,
        Joystick,
        Touch,
        UI
    }

    public interface IInputProvider
    {
        InputProviderType ProviderType { get; }
        bool Active { get; }
        event Action<IInputProvider> OnActiveStatusChanged;
    }
    public static class InputManager
    {
        public struct ActionInfo
        {
            public readonly IInputProvider Provider;
            public readonly InputAction Action;
           
            public ActionInfo(IInputProvider provider, InputAction action)
            {
                Provider = provider;
                Action = action;       
            }
        }

        static readonly List<ActionInfo> mActionInfos = new List<ActionInfo>();
        static readonly List<KeyValuePair<InputAction, Vector3>> mActionValues = new List<KeyValuePair<InputAction, Vector3>>();

        static InputAction mActions;
        static InputAction mStartedActions;
        static InputAction mEndedActions;

        public static event Action<InputAction, InputAction> OnInputActionChanged;
        public static event Action<InputAction, InputAction> OnInputActionEnded;
        public static event Action<InputAction, InputAction> OnInputActionStarted;

        public static event Action<IInputProvider> OnProviderAdded;
        public static event Action<IInputProvider> OnProviderRemoved;
        public static event Action<IInputProvider> OnProviderActiveStatusChanged;

        public static void AddProvider(IInputProvider provider)
        {
            if (mActionInfos.FindIndex(i => i.Provider == provider) >= 0)
                throw new ArgumentException($"Provider '{provider}' already added.");

            mActionInfos.Add(new ActionInfo(provider, InputAction.None));
            provider.OnActiveStatusChanged += Provider_OnActiveStatusChanged;

            try
            {
                if (OnProviderAdded != null)
                    OnProviderAdded(provider);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void RemoveProvider(IInputProvider provider)
        {
            mActionInfos.RemoveAt(mActionInfos.FindIndex(i => i.Provider == provider));

            provider.OnActiveStatusChanged -= Provider_OnActiveStatusChanged;

            UpdateResult();

            try
            {
                if (OnProviderRemoved != null)
                    OnProviderRemoved(provider);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Provider_OnActiveStatusChanged(IInputProvider provider)
        {
            UpdateResult();

            try
            {
                if (OnProviderActiveStatusChanged != null)
                    OnProviderActiveStatusChanged(provider);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        static void UpdateResult()
        {
            InputAction previousResult = mActions;
            mActions = InputAction.None;

            for (int index = 0; index < mActionInfos.Count; index++)
            {
                var providerPair = mActionInfos[index];
                if (providerPair.Provider.Active)
                    mActions = AddAction(mActions, providerPair.Action);
            }

            InputAction previousStartedActionsResult = mStartedActions;
            mStartedActions = (mActions ^ previousResult) & mActions;

            InputAction previousEndedActionsResult = mEndedActions;
            mEndedActions = (mActions ^ previousResult) & previousResult;


            if (mActions != previousResult)
            {
                try
                {
                    if (OnInputActionChanged != null)
                        OnInputActionChanged(mActions, previousResult);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (previousEndedActionsResult != mEndedActions)
            {
                try
                {
                    if (OnInputActionEnded != null)
                        OnInputActionEnded(mEndedActions, previousEndedActionsResult);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (previousStartedActionsResult != mStartedActions)
            {
                try
                {
                    if (OnInputActionStarted != null)
                        OnInputActionStarted(mStartedActions, previousStartedActionsResult);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        
        public static void SetActionValue(InputAction action, Vector3 value)
        {
            var fIndex = mActionValues.FindIndex(i => i.Key == action);
            if(fIndex >= 0)
                mActionValues[fIndex] = new KeyValuePair<InputAction, Vector3>(action, value);
            else
            {
                mActionValues.Add(new KeyValuePair<InputAction, Vector3>(action, value));
            }
        }
        public static Vector3 GetActionValue(InputAction action)
        {
            var fIndex = mActionValues.FindIndex(i => i.Key == action);
            if (fIndex >= 0)
                return mActionValues[fIndex].Value;

            return Vector3.zero;
        }
        public static void SetAction(IInputProvider sender, InputAction action)
        {
            bool providerFound = false;

            for (int pIndex = 0; pIndex < mActionInfos.Count; pIndex++)
                if (mActionInfos[pIndex].Provider == sender)
                {
                    mActionInfos[pIndex] = new ActionInfo(sender, action);
                    providerFound = true;
                    break;
                }

            if (!providerFound)
                Debug.LogError($"Provider {sender} not added.");

            UpdateResult();
        }
       
        public static InputAction Actions { get { return mActions; } }
        public static InputAction AddAction(InputAction target, InputAction action) { return target | action; }
        public static InputAction EraseAction(InputAction target, InputAction action) { return target & ~action; }
        public static bool HasAction(InputAction target, InputAction action) { return (target & action) == action; }

        public static bool HasAction(InputAction action, InputProviderType providerType)
        {
            for (var pIndex = 0; pIndex < mActionInfos.Count; pIndex++)
            {
                var providerPair = mActionInfos[pIndex];
                if (providerPair.Provider.ProviderType == providerType)
                    return HasAction(providerPair.Action, action);
            }

            return false;
        }

        public static IInputProvider GetProvider(InputProviderType providerType)
        {
            for (var pIndex = 0; pIndex < mActionInfos.Count; pIndex++)
            {
                var providerPair = mActionInfos[pIndex];
                if (providerPair.Provider.ProviderType == providerType)
                    return providerPair.Provider;
            }

            return null;
        }

        public static bool GetProviderActiveState(InputProviderType providerType)
        {
            var inputProvider = GetProvider(providerType);

            return (inputProvider != null) ? inputProvider.Active : false;
        }
    }
}