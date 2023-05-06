using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class CameraControl : MonoBehaviour
    {
        private ChangerListener<Color> _fakeLightingDarknessColor;

        [FormerlySerializedAs("Owner")] [SerializeField] private Level _owner;
        [FormerlySerializedAs("Target")] [SerializeField] private Hero _target;

        [FormerlySerializedAs("FakeLight")] [SerializeField] private FakeLightingFeature _fakeLight;
        public Level Owner
        {
            get => _owner;
            set => _owner = value;
        }

        public Hero Target
        {
            get => _target;
            set => _target = value;
        }

        private void Start()
        {
            var component = this.GetComponent<Camera>();
            _fakeLightingDarknessColor = new ChangerListener<Color>(_owner.Lighting.FakeLightingDarknessColor);
            _fakeLightingDarknessColor.OnValueChanged += FakeLightingDarknessColor_OnValueChanged;
            FakeLightingDarknessColor_OnValueChanged(_fakeLightingDarknessColor.Value);
        }

        private void FakeLightingDarknessColor_OnValueChanged(Color oldColor)
        {      
            if (_owner.Lighting.FakeLightingDarknessColor.grayscale < 1)
            {
	            _fakeLight.FillLightColor = _owner.Lighting.FakeLightingDarknessColor;
	            _fakeLight.Enabled = true;
            }
            else
            {
	            _fakeLight.Enabled = false;
            }         
        }

        private void Update()
        {
            _fakeLightingDarknessColor.CheckValue(_owner.Lighting.FakeLightingDarknessColor);
        }    
    }

    public class ChangerListener<T> where T: IEquatable<T>
    {
        public event Action<T> OnValueChanged;

        private T _value;

        public ChangerListener(T value)
        {
            _value = value;         
        }

        public T Value => _value;

        public bool CheckValue(T newValue)
        {
            if (!_value.Equals(newValue))
            {
                T oldValue = _value;
                _value = newValue;
                OnValueChanged?.Invoke(oldValue);

                return true;
            }

            return false;
        }
    }
}