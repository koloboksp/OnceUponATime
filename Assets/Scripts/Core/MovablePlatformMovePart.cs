using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class MovablePlatformMovePart : MonoBehaviour, IManualMoved
    {
        private Vector2 _nextSpeed;
        private Vector2 _speed;
        private bool _speedChanged;
        private bool _directionChanged;
        
        [FormerlySerializedAs("Body")] [SerializeField] private Rigidbody2D _body;
         
        public Vector2 NextStepSpeed => _nextSpeed;
        public bool SpeedChanged => _speedChanged;
        public Vector2 CurrentStepSpeed => _speed;
        public bool DirectionChanged => _directionChanged;
        
        public void MovePosition(Vector2 position, Vector2 nextStepSpeed, Vector2 speed, bool speedChanged, bool directionChanged)
        {
            _body.MovePosition(position);

            _speedChanged = speedChanged;
            _directionChanged = directionChanged;

            _nextSpeed = nextStepSpeed;
            _speed = speed;
        }
    }
}