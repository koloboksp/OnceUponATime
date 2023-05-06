using UnityEngine;

namespace Assets.Scripts.Core
{
    public class MovablePlatformMovePart : MonoBehaviour, IManualMoved
    {
        public Rigidbody2D Body;
         
        public Vector2 NextStepSpeed => mNextSpeed;
        public bool SpeedChanged => mSpeedChanged;
        public Vector2 CurrentStepSpeed => mSpeed;
        public bool DirectionChanged => mDirectionChanged;

        private Vector2 mNextSpeed;
        private Vector2 mSpeed;
        private bool mSpeedChanged;
        private bool mDirectionChanged;

        public void MovePosition(Vector2 position, Vector2 nextStepSpeed, Vector2 speed, bool speedChanged, bool directionChanged)
        {
            Body.MovePosition(position);

            mSpeedChanged = speedChanged;
            mDirectionChanged = directionChanged;

            mNextSpeed = nextStepSpeed;
            mSpeed = speed;
        }
    }
}