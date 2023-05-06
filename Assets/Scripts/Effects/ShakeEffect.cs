using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class ShakeEffect : MonoBehaviour
    {
        private float _elapsedTime;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        
        [FormerlySerializedAs("Target")] [SerializeField] private Transform _target;

        [FormerlySerializedAs("ShakePositionAmount")] [SerializeField] private float _shakePositionAmount = 0.05f;
        [FormerlySerializedAs("ShakeAngleAmount")] [SerializeField] private float _shakeAngleAmount = 3.0f;

        [FormerlySerializedAs("ShakeDuration")] [SerializeField] private  float _shakeDuration = 1.0f;
        
        public void OnEnable()
        {
            _startPosition = _target.transform.localPosition;
            _startRotation = _target.transform.localRotation;

            _elapsedTime = 0.0f;
        }

        public void OnDisable()
        {
            _target.transform.localPosition = _startPosition;
            _target.transform.localRotation = _startRotation;
        }
        public void Update()
        {
            var elapsedNormTime = Mathf.Clamp01(_elapsedTime / _shakeDuration);

            var shakeRandom = UnityEngine.Random.onUnitSphere;
            
            _target.localPosition = _startPosition + shakeRandom * _shakePositionAmount * (1.0f - elapsedNormTime);

            _target.localRotation = Quaternion.Euler(shakeRandom * _shakeAngleAmount * (1.0f - elapsedNormTime));
            _elapsedTime += Time.deltaTime;

            if (elapsedNormTime >= 1)
                enabled = false;
        }
    }
}