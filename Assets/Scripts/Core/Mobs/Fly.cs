using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Fly : MonoBehaviour
    {
        private enum State
        {
            Move,
            Attack,
            TakeDamage,
            WaitBeforeDie,
            Die,
        }


        private Vector3 mCenterPoint;

        private float mSpeed = 2.0f;
        public float RandomRangeRadius = 0.5f;

        private Vector3 mDestination;
        private State mCurrentState = State.Move;

        public Vector3 CenterPoint
        {
            set
            {
                mCenterPoint = value;
            }
        }

        private void Start()
        {  
            SelectNewRandomDestinationPoint();
            transform.localPosition = mDestination;
            SelectNewRandomDestinationPoint();
        }

        public void SetLocalDestination(Vector3 localPoint)
        {
            mDestination = localPoint;
        }

        private void SelectNewRandomDestinationPoint()
        {
            mDestination = UnityEngine.Random.insideUnitSphere * RandomRangeRadius;
        }

        private void Update()
        {
            if (mCurrentState == State.Move)
            {
                if (MoveToDestination(Time.deltaTime))
                    SelectNewRandomDestinationPoint();
            }

            else if (mCurrentState == State.WaitBeforeDie)
            {
                mDelayTimer += Time.deltaTime;
                if (mDelayTimer >= mDelay)
                {
                    mCurrentState = State.Die;
                    mSpeed = 0.0f;
                    Vector3 gravity = Physics2D.gravity;
                    mDestination = mDestination + gravity * mDieEffectTime * mDieEffectTime * 0.5f;
                    mInitialLocalScale = transform.localScale;
                }
                else
                {
                    if (MoveToDestination(Time.deltaTime))
                        SelectNewRandomDestinationPoint();
                }
            }
            else if(mCurrentState == State.Die)
            {
                mDieEffectTimer += Time.deltaTime;
                mSpeed += Physics2D.gravity.magnitude * Time.deltaTime;

                MoveToDestination(Time.deltaTime);

                if (mDieEffectTimer > mDieEffectTime * 0.8f)
                {
                    float f = mDieEffectTimer - mDieEffectTime * 0.8f;
                    float d = f / (mDieEffectTime * 0.2f);
                    d = Mathf.Clamp01(d);
                    transform.localScale = mInitialLocalScale * ( 1 - d);
                }
                
            }
            else if(mCurrentState == State.TakeDamage)
            {
                MoveToDestination2(Time.deltaTime, mEffectSpeed);

                mEffectSpeed -= mEffectForceValue * Time.deltaTime;
                mEffectSpeed = Mathf.Max(0, mEffectSpeed);

                
                //var vecToDestination = mDestination - transform.localPosition;
                //var dirToDestination = vecToDestination.normalized;
                //transform.localPosition += dirToDestination * mSpeed;
                mStunEffectTimer += Time.deltaTime;
                if (mStunEffectTimer >= mStunEffectTime)
                {
                    mCurrentState = State.Move;

                    mStunEffectTimer = 0.0f;
                }
            }
        }

        private bool MoveToDestination2(float dTime, float speed)
        {
            var vecToDestination = mDestination - transform.localPosition;
            var timeToDestination = vecToDestination.magnitude / speed;

            float timeParts = timeToDestination / dTime;
            if (timeParts > 1)
            {
                transform.localPosition += vecToDestination * (1.0f / timeParts);
            }
            else
            {
                transform.localPosition = mDestination;
                return true;
            }

            return false;
        }

        private bool MoveToDestination(float dTime)
        {
            var vecToDestination = mDestination - transform.localPosition;
            var timeToDestination = vecToDestination.magnitude / mSpeed;

            float timeParts = timeToDestination / dTime;
            if (timeParts > 1)
            {
                transform.localPosition += vecToDestination * (1.0f / timeParts);
                transform.localRotation = Quaternion.LookRotation(vecToDestination);
            }
            else
            {
                transform.localPosition = mDestination;
                return true;
            }

            return false;
        }

        private float mDelayTimer;
        private float mDelay;

        private float mDieEffectTime;
        private float mDieEffectTimer;
        private Vector3 mInitialLocalScale;
        public Vector3 rot;
        public void Die(float effectDelay, float effectTime)
        {
            mCurrentState = State.WaitBeforeDie;
            
            mDelay = effectDelay;
            mDelayTimer = 0.0f;

            mDieEffectTime = effectTime;
            mDieEffectTimer = 0.0f;
        }

        private float mEffectSpeed;
        private float mEffectForceValue;

        private float mStunEffectTime;
        private float mStunEffectTimer;

        public void StunEffect(Vector3 localForcePoint, float forceValue, float effectTime)
        {
            mStunEffectTime = effectTime;

            mCurrentState = State.TakeDamage;
            mEffectForceValue = forceValue;

            float distanceToTravel = mEffectForceValue * effectTime * effectTime * 0.5f;
            mEffectSpeed = mEffectForceValue * effectTime;

            var vecToForce = transform.localPosition - localForcePoint;
            var dirToForce = vecToForce.normalized;
            mDestination += dirToForce * distanceToTravel; 
        }
    }
}