using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIUserButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        enum State
        {
            Normal,
            Pressed,
            Disabled,
        }
        public const float DefaultPressedSize = 0.9f;
        public event Action<UIUserButton> OnPress;
        public event Action<UIUserButton> OnClick;

        public Color DisabledColor = new Color(0.5f, 0.5f, 0.5f, 1);
        public Color PressedColor = new Color(1.0f, 1.0f, 1.0f, 1);
        public bool AutoReturn;
        public Image Image;

        public object UserData;

        bool mIsPressed = false;
       // bool mIsManualUpdated = false;
        readonly List<Graphic> mAllInnerGraphicConponents = new List<Graphic>();
        readonly Dictionary<Graphic, Color> mSavedNormalColors = new Dictionary<Graphic, Color>();
        bool mInitialized;        
        int mUpdateCycleIndexOnPointerDown;
        int mCurrentUpdateCycleIndex = 0;
        bool mIgnoreInput = false;

        public bool IgnoreInput
        {
            get { return mIgnoreInput; }
            set
            {
                mIgnoreInput = value;
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

        void CheckInitialization()
        {
            if (!mInitialized)
            {
                mInitialized = true;

                mAllInnerGraphicConponents.Clear();
                gameObject.GetComponents<Graphic>(mAllInnerGraphicConponents);
                gameObject.GetComponentsInChildren<Graphic>(true, mAllInnerGraphicConponents);

                mSavedNormalColors.Clear();
                for (int iIndex = 0; iIndex < mAllInnerGraphicConponents.Count; iIndex++)
                    mSavedNormalColors.Add(mAllInnerGraphicConponents[iIndex], mAllInnerGraphicConponents[iIndex].color);
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
        void UpdateGraphical(State state)
        {
            CheckInitialization();

            if (state == State.Pressed)
            {
                for (int iColor = 0; iColor < mAllInnerGraphicConponents.Count; iColor++)
                    mAllInnerGraphicConponents[iColor].color *= PressedColor;

                this.transform.localScale = DefaultPressedSize * Vector3.one;
            }
            else if (state == State.Disabled)
            {
                for (int iColor = 0; iColor < mAllInnerGraphicConponents.Count; iColor++)
                    mAllInnerGraphicConponents[iColor].color *= DisabledColor;

                this.transform.localScale = Vector3.one;
            }
            else
            {
                for (int iColor = 0; iColor < mAllInnerGraphicConponents.Count; iColor++)
                    mAllInnerGraphicConponents[iColor].color = mSavedNormalColors[mAllInnerGraphicConponents[iColor]];

                this.transform.localScale = Vector3.one;
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (!enabled)
                return;
            if (mIgnoreInput)
                return;

            mUpdateCycleIndexOnPointerDown = mCurrentUpdateCycleIndex;
            mIsPressed = true;
            UpdateGraphical(State.Pressed);

            if (OnPress != null)
                OnPress(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enabled)
                return;

            if (mIsPressed)
            {
                mIsPressed = false;
                UpdateGraphical(State.Normal);

                if (OnClick != null)
                    OnClick(this);
            }
        }

        public void ManualUpdate()
        {
            if (mUpdateCycleIndexOnPointerDown != mCurrentUpdateCycleIndex)
            {
                if (AutoReturn)
                {
                    if (mIsPressed)
                    {
                        mIsPressed = false;
                        UpdateGraphical(State.Normal);
                    }
                }
                else
                {
                    if (mIsPressed)
                    {
                        if (OnPress != null)
                            OnPress(this);
                    }
                }
            }

            mCurrentUpdateCycleIndex++;
        }

        public void ResetState()
        {
            enabled = true;
            mIgnoreInput = true;

            if (mIsPressed)
            {
                mIsPressed = false;
                UpdateGraphical(State.Normal);
            }
        }

        public bool IsPressed
        {
            get
            {
                return mIsPressed;
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
    }
}