using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class Body2dRotationEffect : MonoBehaviour
    {
        public Rigidbody2D Body;
        public float AngleSpeed = 30.0f;
        public bool FixPosition = true;

        private Vector2 mStartPosition;

        private void Start()
        {
            mStartPosition = Body.position;
        }

        private void FixedUpdate()
        {
            if(FixPosition)
                Body.MovePosition(mStartPosition);

            Body.MoveRotation(Body.rotation + AngleSpeed * Time.fixedDeltaTime);
        }
    }
}