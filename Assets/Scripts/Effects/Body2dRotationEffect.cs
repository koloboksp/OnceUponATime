using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class Body2dRotationEffect : MonoBehaviour
    {
        public Rigidbody2D Body;
        public float AngleSpeed = 30.0f;
        public bool FixPosition = true;

        Vector2 mStartPosition;

        void Start()
        {
            mStartPosition = Body.position;
        }
        void FixedUpdate()
        {
            if(FixPosition)
                Body.MovePosition(mStartPosition);

            Body.MoveRotation(Body.rotation + AngleSpeed * Time.fixedDeltaTime);
        }
    }
}