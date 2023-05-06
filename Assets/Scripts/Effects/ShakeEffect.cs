using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class ShakeEffect : MonoBehaviour
    {
        public Transform Target;

        public float ShakePositionAmount = 0.05f;
        public float ShakeAngleAmount = 3.0f;

        public float ShakeDuration = 1.0f;

        private float mElapsedTime;
        private Vector3 mStartPosition;
        private Quaternion mStartRotation;

        public void OnEnable()
        {
            mStartPosition = Target.transform.localPosition;
            mStartRotation = Target.transform.localRotation;

            mElapsedTime = 0.0f;
        }

        public void OnDisable()
        {
            Target.transform.localPosition = mStartPosition;
            Target.transform.localRotation = mStartRotation;
        }
        public void Update()
        {
            var elapsedNormTime = Mathf.Clamp01(mElapsedTime / ShakeDuration);

            var shakeRandom = UnityEngine.Random.onUnitSphere;
            
            Target.localPosition = mStartPosition + shakeRandom * ShakePositionAmount * (1.0f - elapsedNormTime);

            Target.localRotation = Quaternion.Euler(shakeRandom * ShakeAngleAmount * (1.0f - elapsedNormTime));
            mElapsedTime += Time.deltaTime;

            if (elapsedNormTime >= 1)
                enabled = false;
        }
    }
}