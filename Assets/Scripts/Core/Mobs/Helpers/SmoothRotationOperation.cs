using System;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class SmoothRotationOperation : Operation
    {
        private Transform _smoothRotationRoot;
        
        private Quaternion _startRotation;
        private Quaternion _endRotation;

        public RotationDirection RotationDirection { get; private set; }

        public void Execute(float time, RotationDirection rotationDirection, Transform smoothRotationRoot)
        {         
            RotationDirection = rotationDirection;
            _smoothRotationRoot = smoothRotationRoot;

            _endRotation = _smoothRotationRoot.localRotation;
            _startRotation = _endRotation * Quaternion.Euler(0, Mathf.PI * Mathf.Rad2Deg, 0);
            _smoothRotationRoot.localRotation = _startRotation;

            base.Execute(time);
        }

        protected override void InnerProcess(float dTime)
        {
            base.InnerProcess(dTime);

            float rotationDirection = RotationDirection == RotationDirection.Clockwise ? -1.0f : 1.0f;
            _smoothRotationRoot.localRotation = _startRotation* Quaternion.Euler(0, rotationDirection * NormElapsedTime * Mathf.PI * Mathf.Rad2Deg, 0);
        }

        protected override void InnerComplete()
        {
            _smoothRotationRoot.localRotation = _endRotation;           
        }
    }
}