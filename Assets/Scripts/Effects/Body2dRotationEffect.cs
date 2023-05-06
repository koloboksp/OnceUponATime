using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class Body2dRotationEffect : MonoBehaviour
    {
        private Vector2 _startPosition;

        [FormerlySerializedAs("Body")] [SerializeField] private Rigidbody2D _body;
        [FormerlySerializedAs("AngleSpeed")] [SerializeField] private float _angleSpeed = 30.0f;
        [FormerlySerializedAs("FixPosition")] [SerializeField] private bool _fixPosition = true;
        
        private void Start()
        {
            _startPosition = _body.position;
        }

        private void FixedUpdate()
        {
            if(_fixPosition)
                _body.MovePosition(_startPosition);

            _body.MoveRotation(_body.rotation + _angleSpeed * Time.fixedDeltaTime);
        }
    }
}