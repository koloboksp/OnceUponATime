using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class KeyboardInputProvider : MonoBehaviour, IInputProvider
    {
        [RuntimeInitializeOnLoadMethod()]
        private static void CreateInstance()
        {
            GameObject container = new GameObject(nameof(KeyboardInputProvider));
            DontDestroyOnLoad(container);
            container.AddComponent<KeyboardInputProvider>();
        }

        public class Association
        {
            public InputAction Action { get; }
            private List<KeyCode> mKeyCodes;

            public Association(InputAction action, List<KeyCode> codes)
            {
                Action = action;
                mKeyCodes = codes;
            }

            public IReadOnlyList<KeyCode> KeyCodes { get { return mKeyCodes; } }

            public void ChangeKeyCodes(IEnumerable<KeyCode> newKeyCodes)
            {
                mKeyCodes.Clear();
                mKeyCodes.AddRange(newKeyCodes);
            }
        }

        public InputProviderType ProviderType { get { return InputProviderType.Keyboard; } }

        public bool Active
        {
            get { return isActiveAndEnabled; }
        }

        public event Action<IInputProvider> OnActiveStatusChanged;

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
            OnActiveStatusChanged?.Invoke(this);
        }

        private void OnDisable()
        {
            OnActiveStatusChanged?.Invoke(this);
        }

        public void Update()
        {
            InputAction result = InputAction.None;

            CheckKeys(mSystemKeysAssociations, ref result);
            CheckKeys(mUserKeysAssociations, ref result);

            InputManager.SetAction(this, result);
        }

        private void CheckKeys(List<Association> associations, ref InputAction result)
        {
            for (var aIndex = 0; aIndex < associations.Count; aIndex++)
            {
                Association association = associations[aIndex];
                for (var akcIndex = 0; akcIndex < association.KeyCodes.Count; akcIndex++)
                {
                    KeyCode actionKeyCode = association.KeyCodes[akcIndex];
                    if (UnityEngine.Input.GetKey(actionKeyCode))
                    {
                        result = InputManager.AddAction(result, association.Action);
                        break;
                    }
                }
            }
        }

        public static void UpdateUserKeys(InputAction inputAction, IEnumerable<KeyCode> keyCodes)
        {
            Association foundAssociation = mUserKeysAssociations.Find(i => i.Action == inputAction);
            foundAssociation?.ChangeKeyCodes(keyCodes);
        }

        public static IEnumerable<Association> UserKeysAssociations { get { return mUserKeysAssociations; } }

        private static readonly List<Association> mSystemKeysAssociations = new List<Association>()
        {
            new Association(InputAction.UIMoveUp, new List<KeyCode>(){KeyCode.UpArrow, KeyCode.W}),
            new Association(InputAction.UIMoveDown, new List<KeyCode>(){KeyCode.DownArrow, KeyCode.S}),
            new Association(InputAction.UIMoveLeft, new List<KeyCode>(){KeyCode.LeftArrow, KeyCode.A}),
            new Association(InputAction.UIMoveRight, new List<KeyCode>(){KeyCode.RightArrow, KeyCode.D}),
            new Association(InputAction.UIEnter, new List<KeyCode>(){KeyCode.Return, KeyCode.Space}),
            new Association(InputAction.Esc, new List<KeyCode>(){KeyCode.Escape, KeyCode.Backspace}),
        };

        private static readonly List<Association> mUserKeysAssociations = new List<Association>()
        {
            new Association(InputAction.Jump, new List<KeyCode>(){KeyCode.UpArrow, KeyCode.W, KeyCode.Space}),
            new Association(InputAction.MoveLeft, new List<KeyCode>(){KeyCode.LeftArrow, KeyCode.A}),
            new Association(InputAction.MoveRight, new List<KeyCode>(){KeyCode.RightArrow, KeyCode.D}),
            new Association(InputAction.ActionType1, new List<KeyCode>(){KeyCode.LeftControl, KeyCode.RightControl}),
            new Association(InputAction.ActionType2, new List<KeyCode>(){KeyCode.LeftShift, KeyCode.RightShift}),
        };
    }
}