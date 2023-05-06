using System.Collections;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class GingerBreadMen :  GroundMob
    {
        private Vector3 _startPosition;
        private bool _dealDamageTrigger;
        private bool _takeDamageTrigger;

        [FormerlySerializedAs("MaxRemovalDistance")] [SerializeField] private float _maxRemovalDistance = 5.0f;
        [FormerlySerializedAs("ViewRotationEffectRoot")] [SerializeField] private Transform _viewRotationEffectRoot;
        [FormerlySerializedAs("Radius")] [SerializeField] private float _radius = 0.25f;
        
        protected override void Start() 
        {
            base.Start();
            _startPosition = this.transform.position;

            StartCoroutine(LifeCycle());
        }

        private IEnumerator LifeCycle()
        {
            StartMove();
            while (Lives > 0)
            {
                LogicMove();

                if (_takeDamageTrigger)
                {
                    _takeDamageTrigger = false;

                    StopMove();
                    yield return new WaitForSeconds(1);
                }

                if (_dealDamageTrigger)
                {
                    _dealDamageTrigger = false;

                    StopMove();
                    yield return new WaitForSeconds(1);
                }

                yield return null;
            }
        }

        private void StartMove()
        {
            SetMovingDirection(UnityEngine.Random.Range(0, 2) == 0 ? MovingDirection.Forward: MovingDirection.Backward);
            SetMovingSpeed(WalkSpeed);
        }

        private void ChangeDirection()
        {
            var vecToHomePoint = (_startPosition - transform.position);

            SetMovingDirection(Mathf.Sign(Vector2.Dot(Vector2.right, vecToHomePoint)) > 0 ? MovingDirection.Forward : MovingDirection.Backward);
        }

        private void LogicMove()
        {
            float distanceFromStart = (transform.position - _startPosition).magnitude;

            if (distanceFromStart > _maxRemovalDistance)
                ChangeDirection();
 
            Move();

            float linearDeltaDistance = BodyRelativeVelocity.magnitude * Time.deltaTime * ((MovingDirection == MovingDirection.Forward ) ? 1.0f : -1.0f);

            Rotate(linearDeltaDistance, _radius, _viewRotationEffectRoot);
  
        }
       
        public static void Rotate(float linearDeltaDistance, float radius, Transform rotationRoot)
        {
            float normDeltaAngle = linearDeltaDistance / (2.0f * Mathf.PI * radius);
            float deltaAngle = normDeltaAngle * 2.0f * Mathf.PI * Mathf.Rad2Deg;
            Quaternion additionalCoilRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -deltaAngle));
            rotationRoot.localRotation = additionalCoilRotation * rotationRoot.localRotation;
        }
        
        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            _takeDamageTrigger = true;
        }
    }
}