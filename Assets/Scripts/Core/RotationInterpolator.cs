using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class RotationInterpolator
    {
        private Quaternion _desiredValue;
        private Quaternion _interpolatedValue;

        private float _speed = 50.0f;
        private bool _reset = true;

        public Quaternion Value
        {
            set
            {
                _desiredValue = value;
                if (_reset)
                {
                    _reset = false;
                    _interpolatedValue = _desiredValue;
                }
            }
        }

        public Quaternion InterpolatedValue
        {
            get { return _interpolatedValue; }
        }

        public void Update(float dTime)
        {
            _interpolatedValue = Quaternion.RotateTowards(_interpolatedValue, _desiredValue, _speed * dTime);
        }

        public void Reset(Quaternion value)
        {
            _desiredValue = value;
            _interpolatedValue = _desiredValue;
        }
    }
}