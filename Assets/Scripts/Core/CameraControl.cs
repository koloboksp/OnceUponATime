using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.Effects;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class CameraControl : MonoBehaviour
    {
        ChangerListener<Color> mFakeLightingDarknessColor;

        public Level Owner;
        public Hero Target;

        public FakeLightingFeature FakeLight;

      //  public CameraFakeLightingEffect FakeLightingEffect;

        void Start()
        {
            var component = this.GetComponent<Camera>();
            mFakeLightingDarknessColor = new ChangerListener<Color>(Owner.LightingSettings.FakeLightingDarknessColor);
            mFakeLightingDarknessColor.OnValueChanged += FakeLightingDarknessColor_OnValueChanged;
            FakeLightingDarknessColor_OnValueChanged(mFakeLightingDarknessColor.Value);
        }

        void FakeLightingDarknessColor_OnValueChanged(Color oldColor)
        {      
            if (Owner.LightingSettings.FakeLightingDarknessColor.grayscale < 1)
            {
	            FakeLight.FillLightColor = Owner.LightingSettings.FakeLightingDarknessColor;
	            FakeLight.Enabled = true;
            }
            else
            {
	            FakeLight.Enabled = false;
            }         
        }

        void Update()
        {
            mFakeLightingDarknessColor.CheckValue(Owner.LightingSettings.FakeLightingDarknessColor);
        }    
    }

    public class ChangerListener<T> where T: IEquatable<T>
    {
        public event Action<T> OnValueChanged;

        T mValue;

        public ChangerListener(T value)
        {
            mValue = value;         
        }

        public T Value => mValue;

        public bool CheckValue(T newValue)
        {
            if (!mValue.Equals(newValue))
            {
                T oldValue = mValue;
                mValue = newValue;
                if (OnValueChanged != null)
                    OnValueChanged(oldValue);

                return true;
            }

            return false;
        }
    }
}