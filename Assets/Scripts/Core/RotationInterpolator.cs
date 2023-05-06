using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class RotationInterpolator
    {
        private Quaternion mDesiredValue;
        private Quaternion mInterpolatedValue;

        private float mSpeed = 50.0f;
        private bool mReset = true;

        public Quaternion Value
        {
            set
            {
                mDesiredValue = value;
                if (mReset)
                {
                    mReset = false;
                    mInterpolatedValue = mDesiredValue;
                }
            }
        }

        public Quaternion InterpolatedValue
        {
            get { return mInterpolatedValue; }
        }

        public void Update(float dTime)
        {
            mInterpolatedValue = Quaternion.RotateTowards(mInterpolatedValue, mDesiredValue, mSpeed * dTime);
        }

        public void Reset(Quaternion value)
        {
            mDesiredValue = value;
            mInterpolatedValue = mDesiredValue;
        }
    }
}