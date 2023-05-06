using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIUserButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
       
        public const float DefaultPressedSize = 0.9f;
        public event Action<UIUserButton> OnPress;
        public event Action<UIUserButton> OnClick;

        private bool _isPressed = false;
        private readonly List<Graphic> _allInnerGraphicComponents = new List<Graphic>();
        private readonly Dictionary<Graphic, Color> _savedNormalColors = new Dictionary<Graphic, Color>();
        private bool _initialized;
        private int _updateCycleIndexOnPointerDown;
        private int _currentUpdateCycleIndex = 0;
        private bool _ignoreInput = false;
        
        [FormerlySerializedAs("DisabledColor")] [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 1);
        [FormerlySerializedAs("PressedColor")] [SerializeField] private Color _pressedColor = new Color(1.0f, 1.0f, 1.0f, 1);
        [FormerlySerializedAs("AutoReturn")] [SerializeField] private bool _autoReturn;
        [FormerlySerializedAs("Image")] [SerializeField] private Image _image;

        public bool AutoReturn
        {
            get => _autoReturn;
            set => _autoReturn = value;
        }

        public Image Image => _image;
        
        public bool IgnoreInput
        {
            get { return _ignoreInput; }
            set
            {
                _ignoreInput = value;
            }
        }

        protected virtual void Awake() { }
        protected virtual void OnDestroy() { }

        protected virtual void OnEnable()
        {
            CheckInitialization();

            UpdateGraphical(State.Normal);
        }

        protected virtual void OnDisable()
        {
            CheckInitialization();

            UpdateGraphical(State.Disabled);
        }

        private void CheckInitialization()
        {
            if (!_initialized)
            {
                _initialized = true;

                _allInnerGraphicComponents.Clear();
                gameObject.GetComponents<Graphic>(_allInnerGraphicComponents);
                gameObject.GetComponentsInChildren<Graphic>(true, _allInnerGraphicComponents);

                _savedNormalColors.Clear();
                for (int iIndex = 0; iIndex < _allInnerGraphicComponents.Count; iIndex++)
                    _savedNormalColors.Add(_allInnerGraphicComponents[iIndex], _allInnerGraphicComponents[iIndex].color);
            }
        }

        public void SetSkin(string skinName)
        {
            bool finded = false;
            for (int childIndex = 0; childIndex < this.transform.childCount; childIndex++)
            {
                var childTransform = transform.GetChild(childIndex);
                if (childTransform.gameObject.name == skinName)
                {
                    childTransform.gameObject.SetActive(true);
                    finded = true;
                }
                else
                {
                    childTransform.gameObject.SetActive(false);
                }
            }
            if (!finded)
                Debug.LogError(string.Format("Skin '{0}' not found.", skinName), this);
        }

        private void UpdateGraphical(State state)
        {
            CheckInitialization();

            if (state == State.Pressed)
            {
                for (int iColor = 0; iColor < _allInnerGraphicComponents.Count; iColor++)
                    _allInnerGraphicComponents[iColor].color *= _pressedColor;

                this.transform.localScale = DefaultPressedSize * Vector3.one;
            }
            else if (state == State.Disabled)
            {
                for (int iColor = 0; iColor < _allInnerGraphicComponents.Count; iColor++)
                    _allInnerGraphicComponents[iColor].color *= _disabledColor;

                this.transform.localScale = Vector3.one;
            }
            else
            {
                for (int iColor = 0; iColor < _allInnerGraphicComponents.Count; iColor++)
                    _allInnerGraphicComponents[iColor].color = _savedNormalColors[_allInnerGraphicComponents[iColor]];

                this.transform.localScale = Vector3.one;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (!enabled)
                return;
            if (_ignoreInput)
                return;

            _updateCycleIndexOnPointerDown = _currentUpdateCycleIndex;
            _isPressed = true;
            UpdateGraphical(State.Pressed);

            OnPress?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enabled)
                return;

            if (_isPressed)
            {
                _isPressed = false;
                UpdateGraphical(State.Normal);

                OnClick?.Invoke(this);
            }
        }

        public void ManualUpdate()
        {
            if (_updateCycleIndexOnPointerDown != _currentUpdateCycleIndex)
            {
                if (_autoReturn)
                {
                    if (_isPressed)
                    {
                        _isPressed = false;
                        UpdateGraphical(State.Normal);
                    }
                }
                else
                {
                    if (_isPressed)
                    {
                        OnPress?.Invoke(this);
                    }
                }
            }

            _currentUpdateCycleIndex++;
        }

        public void ResetState()
        {
            enabled = true;
            _ignoreInput = true;

            if (_isPressed)
            {
                _isPressed = false;
                UpdateGraphical(State.Normal);
            }
        }

        public bool IsPressed
        {
            get
            {
                return _isPressed;
            }
        }

      
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        private enum State
        {
            Normal,
            Pressed,
            Disabled,
        }
    }
}