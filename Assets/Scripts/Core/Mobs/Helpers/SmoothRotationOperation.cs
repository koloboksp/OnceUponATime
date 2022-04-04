using System;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class SmoothRotationOperation : Operation
    {
        Transform mSmoothRotationRoot;

        public RotationDirection RotationDirection { get; private set; }

        Quaternion mStartRotation;
        Quaternion mEndRotation;

        public void Execute(float time, RotationDirection rotationDirection, Transform smoothRotationRoot)
        {         
            RotationDirection = rotationDirection;
            mSmoothRotationRoot = smoothRotationRoot;

            mEndRotation = mSmoothRotationRoot.localRotation;
            mStartRotation = mEndRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);
            mSmoothRotationRoot.localRotation = mStartRotation;

            base.Execute(time);
        }

        protected override void InnerProcess(float dTime)
        {
            base.InnerProcess(dTime);

            float rotationDirection = RotationDirection == RotationDirection.Clockwise ? -1.0f : 1.0f;
            mSmoothRotationRoot.localRotation = mStartRotation* Quaternion.Euler(0, rotationDirection * NormElapsedTime * Mathf.PI * Mathf.Rad2Deg, 0);
        }

        protected override void InnerComplete()
        {
            mSmoothRotationRoot.localRotation = mEndRotation;           
        }
    }
}