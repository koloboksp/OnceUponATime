using System.Collections;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class GingerBreadMen :  GroundMob
    {
        public float MaxRemovalDistance = 5.0f;

      
        public Transform ViewRotationEffectRoot;
        public float Radius = 0.25f;

        Vector3 mStartPosition;
   
        protected override void Start() 
        {
            base.Start();
            mStartPosition = this.transform.position;

            StartCoroutine(T());
        }

        IEnumerator T()
        {
            StartMove();
            while (Lives > 0)
            {
                LogicMove();

                if (mTakeDamageTrigger)
                {
                    mTakeDamageTrigger = false;

                    StopMove();
                    yield return new WaitForSeconds(1);
                }

                if (mDealDamageTrigger)
                {
                    mDealDamageTrigger = false;

                    StopMove();
                    yield return new WaitForSeconds(1);
                }

                yield return null;
            }
        }
        
        void StartMove()
        {
            SetMovingDirection(UnityEngine.Random.Range(0, 2) == 0 ? MovingDirection.Forward: MovingDirection.Backward);
            SetMovingSpeed(WalkSpeed);
        }

        void ChangeDirection()
        {
            var vecToHomePoint = (mStartPosition - transform.position);

            SetMovingDirection(Mathf.Sign(Vector2.Dot(Vector2.right, vecToHomePoint)) > 0 ? MovingDirection.Forward : MovingDirection.Backward);
        }

        void LogicMove()
        {
            float distanceFromStart = (transform.position - mStartPosition).magnitude;

            if (distanceFromStart > MaxRemovalDistance)
                ChangeDirection();
 
            Move();

            float linearDeltaDistance = BodyRelativeVelocity.magnitude * Time.deltaTime * ((MovingDirection == MovingDirection.Forward ) ? 1.0f : -1.0f);

            Rotate(linearDeltaDistance, Radius, ViewRotationEffectRoot);
  
        }
       
        public static void Rotate(float linearDeltaDistance, float radius, Transform rotationRoot)
        {
            float normDeltaAngle = linearDeltaDistance / (2.0f * Mathf.PI * radius);
            float deltaAngle = normDeltaAngle * 2.0f * Mathf.PI * Mathf.Rad2Deg;
            Quaternion additionalCoilRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -deltaAngle));
            rotationRoot.localRotation = additionalCoilRotation * rotationRoot.localRotation;
        }

        bool mDealDamageTrigger;
        bool mTakeDamageTrigger;

       

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            mTakeDamageTrigger = true;
        }
    }
}