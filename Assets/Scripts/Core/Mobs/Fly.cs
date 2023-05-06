using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Fly : MonoBehaviour
    {
        private float _speed = 2.0f;
        
        private Vector3 _destination;
        private State _currentState = State.Move;

        private float _delayTimer;
        private float _delay;

        private float _dieEffectTime;
        private float _dieEffectTimer;
        private Vector3 _initialLocalScale;
        
        private float _effectSpeed;
        private float _effectForceValue;

        private float _stunEffectTime;
        private float _stunEffectTimer;
        
        [FormerlySerializedAs("RandomRangeRadius")] [SerializeField] private float _randomRangeRadius = 0.5f;

        public float RandomRangeRadius
        {
            get => _randomRangeRadius; 
            set => _randomRangeRadius = value;
        }

        private void Start()
        {  
            SelectNewRandomDestinationPoint();
            transform.localPosition = _destination;
            SelectNewRandomDestinationPoint();
        }

        public void SetLocalDestination(Vector3 localPoint)
        {
            _destination = localPoint;
        }

        private void SelectNewRandomDestinationPoint()
        {
            _destination = UnityEngine.Random.insideUnitSphere * _randomRangeRadius;
        }

        private void Update()
        {
            if (_currentState == State.Move)
            {
                if (MoveToDestination(Time.deltaTime))
                    SelectNewRandomDestinationPoint();
            }
            else if (_currentState == State.WaitBeforeDie)
            {
                _delayTimer += Time.deltaTime;
                if (_delayTimer >= _delay)
                {
                    _currentState = State.Die;
                    _speed = 0.0f;
                    Vector3 gravity = Physics2D.gravity;
                    _destination = _destination + gravity * _dieEffectTime * _dieEffectTime * 0.5f;
                    _initialLocalScale = transform.localScale;
                }
                else
                {
                    if (MoveToDestination(Time.deltaTime))
                        SelectNewRandomDestinationPoint();
                }
            }
            else if(_currentState == State.Die)
            {
                _dieEffectTimer += Time.deltaTime;
                _speed += Physics2D.gravity.magnitude * Time.deltaTime;

                MoveToDestination(Time.deltaTime);

                if (_dieEffectTimer > _dieEffectTime * 0.8f)
                {
                    float f = _dieEffectTimer - _dieEffectTime * 0.8f;
                    float d = f / (_dieEffectTime * 0.2f);
                    d = Mathf.Clamp01(d);
                    transform.localScale = _initialLocalScale * ( 1 - d);
                }
                
            }
            else if(_currentState == State.TakeDamage)
            {
                MoveToDestination2(Time.deltaTime, _effectSpeed);

                _effectSpeed -= _effectForceValue * Time.deltaTime;
                _effectSpeed = Mathf.Max(0, _effectSpeed);
                
                _stunEffectTimer += Time.deltaTime;
                if (_stunEffectTimer >= _stunEffectTime)
                {
                    _currentState = State.Move;

                    _stunEffectTimer = 0.0f;
                }
            }
        }

        private bool MoveToDestination2(float dTime, float speed)
        {
            var vecToDestination = _destination - transform.localPosition;
            var timeToDestination = vecToDestination.magnitude / speed;

            float timeParts = timeToDestination / dTime;
            if (timeParts > 1)
            {
                transform.localPosition += vecToDestination * (1.0f / timeParts);
            }
            else
            {
                transform.localPosition = _destination;
                return true;
            }

            return false;
        }

        private bool MoveToDestination(float dTime)
        {
            var vecToDestination = _destination - transform.localPosition;
            var timeToDestination = vecToDestination.magnitude / _speed;

            float timeParts = timeToDestination / dTime;
            if (timeParts > 1)
            {
                transform.localPosition += vecToDestination * (1.0f / timeParts);
                transform.localRotation = Quaternion.LookRotation(vecToDestination);
            }
            else
            {
                transform.localPosition = _destination;
                return true;
            }

            return false;
        }

        public void Die(float effectDelay, float effectTime)
        {
            _currentState = State.WaitBeforeDie;
            
            _delay = effectDelay;
            _delayTimer = 0.0f;

            _dieEffectTime = effectTime;
            _dieEffectTimer = 0.0f;
        }
        
        public void StunEffect(Vector3 localForcePoint, float forceValue, float effectTime)
        {
            _stunEffectTime = effectTime;

            _currentState = State.TakeDamage;
            _effectForceValue = forceValue;

            float distanceToTravel = _effectForceValue * effectTime * effectTime * 0.5f;
            _effectSpeed = _effectForceValue * effectTime;

            var vecToForce = transform.localPosition - localForcePoint;
            var dirToForce = vecToForce.normalized;
            _destination += dirToForce * distanceToTravel; 
        }
        
        private enum State
        {
            Move,
            Attack,
            TakeDamage,
            WaitBeforeDie,
            Die,
        }
    }
}