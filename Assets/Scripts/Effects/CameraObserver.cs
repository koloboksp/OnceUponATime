using UnityEngine;

namespace Assets.Scripts.Core
{
    public class CameraObserver : MonoBehaviour
    {
        public Camera Observer;
        public CameraControl Control;

    
        public float Distance = 8;
        public float VerticalOffset = 2;

        private void LateUpdate()
        {
            var screenPointToRay = Observer.ViewportPointToRay(new Vector3(0,  0.0f, 0));
            var screenPointToRay1 = Observer.ViewportPointToRay(new Vector3(1f,  1f, 0));

            var activeLevel = Level.ActiveLevel;
           
            var pos = Control.Target.transform.position;
            Plane plane = new Plane(
                pos,
                pos + activeLevel.transform.right,
                pos + activeLevel.transform.up);
            
            float d;
            plane.Raycast(screenPointToRay, out d);
            Vector3 lb = screenPointToRay.GetPoint(d);
            plane.Raycast(screenPointToRay1, out d);
            Vector3 rt = screenPointToRay1.GetPoint(d);

            Vector3 observerPosition = Control.Target.transform.position - Vector3.forward * Distance + Vector3.up * VerticalOffset;
            Vector3 observerLookAtPoint = Control.Target.transform.position + Vector3.up * VerticalOffset;

            var localLB = activeLevel.transform.InverseTransformPoint(lb);
            var localRT = activeLevel.transform.InverseTransformPoint(rt);
            var observerLocalPosition = activeLevel.transform.InverseTransformPoint(observerPosition);
            var observerLocalLookAtPoint = activeLevel.transform.InverseTransformPoint(observerLookAtPoint);

            var size = localRT - localLB;

            var halfSize = size * 0.5f;
          
            observerLocalPosition.x = Mathf.Clamp(observerLocalPosition.x, activeLevel.Bounds.xMin + halfSize.x, activeLevel.Bounds.xMax - halfSize.x);
            observerLocalPosition.y = Mathf.Clamp(observerLocalPosition.y, activeLevel.Bounds.yMin + halfSize.y, activeLevel.Bounds.yMax - halfSize.y);

            observerLocalLookAtPoint.x = Mathf.Clamp(observerLocalLookAtPoint.x, activeLevel.Bounds.xMin + halfSize.x, activeLevel.Bounds.xMax - halfSize.x);
            observerLocalLookAtPoint.y = Mathf.Clamp(observerLocalLookAtPoint.y, activeLevel.Bounds.yMin + halfSize.y, activeLevel.Bounds.yMax - halfSize.y);


            var clampedObserverPosition = activeLevel.transform.TransformPoint(observerLocalPosition);
            var clampedObserverLookAtPoint = activeLevel.transform.TransformPoint(observerLocalLookAtPoint);

            Debug.DrawLine(screenPointToRay.origin, lb);
            plane.Raycast(screenPointToRay1, out d);
            Debug.DrawLine(screenPointToRay1.origin, rt);


            Observer.transform.position = clampedObserverPosition;
            Observer.transform.LookAt(clampedObserverLookAtPoint);
        }
    }
} 