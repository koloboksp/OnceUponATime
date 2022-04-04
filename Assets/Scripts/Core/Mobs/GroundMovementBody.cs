using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class GroundMovementBody : MonoBehaviour, IOrderedFixedUpdate
    {  
        readonly RaycastHit2D[] mNoAllocGroundDetectionResults = new RaycastHit2D[10];
        readonly List<IManualMoved> mNoAllocManualMovedResults = new List<IManualMoved>();

        readonly ReuseList<ContactInfo> mContacts = new ReuseList<ContactInfo>();
        readonly ReuseList<ContactInfo> mRaycastContacts = new ReuseList<ContactInfo>();

        readonly MovingOperationInfo mMovingOperationInfo = new MovingOperationInfo();
        readonly JumpOperationInfo mJumpOperationInfo = new JumpOperationInfo();

        GameObject mUnderfootObject;
        GameObject mPreviousFrameUnderfootObject;

        bool mStaticObjectDetectedInMovementDirection;
        bool mMovableObjectDetectedInMovementDirection;

        Vector2 mPreviousLocalMovingAxis;
        Vector2 mBodyRelativeVelocity;

        float mBreakMovingSpeed;
        bool mNeedToApplyBrakeForce = false;

        public GroundMovementCharacter Owner;

        public Rigidbody2D Body;
        public Collider2D BodyCollider;
        public Collider2D BodyGroundCollider;

        public float GroundAngle = 45;
        public float GroundDetectionDistance = 0.2f;

        public float JumpHeight = 1.5f;
        public float TimeIntervalBetweenJumps = 0.2f;

        public MovingOperationInfo MovingOperationInfo => mMovingOperationInfo;
        public JumpOperationInfo JumpOperationInfo => mJumpOperationInfo;

        public GameObject UnderfootObject => mUnderfootObject;
        public Vector2 RelativeVelocity => mBodyRelativeVelocity;

        public bool StaticObjectDetectedInMovementDirection => mStaticObjectDetectedInMovementDirection;
        public bool MovableObjectOnMovingDirectionDetected => mMovableObjectDetectedInMovementDirection;

        public int Order { get { return 0; } }

        protected virtual void OnEnable()
        {
            OrderedFixedUpdateManager.Add(this);
        }

        protected virtual void OnDisable()
        {
            OrderedFixedUpdateManager.Remove(this);
        }

        public void Jump()
        {       
            var externalVelocity = Vector2.zero;
            if(mUnderfootObject != null)
                UnderfootObjectIsManualMoved(ref externalVelocity);

            mJumpOperationInfo.Execute(TimeIntervalBetweenJumps, externalVelocity);
        }

        public void StartMoving(float speed, bool instant = true, float acceleration = float.MaxValue)
        {
            mMovingOperationInfo.Move(speed, instant, acceleration);
        }

        public void StopMoving(bool instant = true, float acceleration = float.MaxValue)
        {
            mMovingOperationInfo.Break(instant, acceleration);
        }

        public void BreakMoving()
        {
            var inverseRotation = Quaternion.Inverse(transform.rotation);
            Vector2 localVelocity = inverseRotation * Body.velocity;
            CalculateBreakForce(localVelocity);     
        }

        public void ChangeDirection()
        {
            mPreviousLocalMovingAxis.y = -mPreviousLocalMovingAxis.y;
        }

        public void AbortAllControlledOperations()
        {
            mJumpOperationInfo.Abort();
            mMovingOperationInfo.Abort();

            if (mNeedToApplyBrakeForce)
            {
                mNeedToApplyBrakeForce = false;
            }
        }

        public void AddDamageForce(float forceValue, Vector2 forceDir)
        {
            Vector2 localVelocity = transform.InverseTransformDirection(Body.velocity);
            Vector2 localForce = transform.InverseTransformDirection(forceValue * forceDir);// inverseRotation * (additionalForceInfo.ForceDirection * additionalForceInfo.ForceValue);
            localForce.y = Mathf.Lerp(
                localForce.y,
                CalculateStartSpeedFromHeight(localVelocity),
                Mathf.Clamp01(Vector2.Dot(localForce, Vector2.up)));

            Body.AddForce(transform.TransformDirection(localForce) * Body.mass, ForceMode2D.Impulse);
        }

        float CalculateStartSpeedFromHeight(Vector2 localVelocity)
        {
            //vs = sqrt(h * 2 * g) - v0; 
            return Mathf.Sqrt(JumpHeight * 2 * Physics2D.gravity.magnitude * Body.gravityScale) - localVelocity.y;
        }

        void FindUnderfootObjectInContacts(ref GameObject underfootObject, ref Vector2 relativeVelocity)
        {
            int forwardContactIndex = -1;
            int backwardContactIndex = -1;

            for (var cIndex = 0; cIndex < mContacts.Count; cIndex++)
            {
                var contact = mContacts[cIndex];

                if (contact.AngleToUpAxis <= GroundAngle)
                {
                    underfootObject = contact.Collider.gameObject;
                    relativeVelocity = contact.RelativeVelocity;
                    return;
                }

                if (contact.LocalPoint.x < 0)
                    forwardContactIndex = cIndex;
                if (contact.LocalPoint.x > 0)
                    backwardContactIndex = cIndex;                  
            }

            if (forwardContactIndex >= 0 && backwardContactIndex >= 0)
            {
                underfootObject = mContacts[forwardContactIndex].Collider.gameObject;
                relativeVelocity = mContacts[forwardContactIndex].RelativeVelocity;
            }
        }

        void FindUnderfootObjectByRaycast(ref GameObject underfootObject)
        {
            var resultsCount = Physics2D.RaycastNonAlloc(this.transform.position, -transform.up, mNoAllocGroundDetectionResults, 1.0f);

            for (int rIndex = 0; rIndex < resultsCount; rIndex++)
            {
                var groundDetectionResult = mNoAllocGroundDetectionResults[rIndex];
                if (!groundDetectionResult.collider.isTrigger &&
                    groundDetectionResult.collider != BodyCollider &&
                    groundDetectionResult.collider != BodyGroundCollider)
                {
                    if (groundDetectionResult.distance < GroundDetectionDistance)
                    {
                        float contactAngle = Mathf.Acos(Vector2.Dot(groundDetectionResult.normal, transform.up)) * Mathf.Rad2Deg;
                        if (contactAngle <= GroundAngle)
                        {            
                            underfootObject = groundDetectionResult.collider.gameObject;
                            var contactInfo = mRaycastContacts.Get();
                            contactInfo.Fill(groundDetectionResult.collider, BodyGroundCollider, groundDetectionResult.point, groundDetectionResult.normal, Vector2.zero);
                            contactInfo.CalculateCache();
                        }
                    }
                }
            }
        }


        void UnderfootObjectIsManualMoved(ref Vector2 externalLocalVelocity)
        {       
            mUnderfootObject.GetComponents<IManualMoved>(mNoAllocManualMovedResults);

            if (mNoAllocManualMovedResults.Count > 0)
            {
                var manualMovedObj = mNoAllocManualMovedResults[0];
              
                if (manualMovedObj.SpeedChanged)
                    Body.AddForce((manualMovedObj.NextStepSpeed - Body.velocity) * Body.mass, ForceMode2D.Impulse);     

                externalLocalVelocity = transform.InverseTransformDirection(manualMovedObj.NextStepSpeed);
            }
        }

        void GetLocalMovingAxisFromRaycast(ref Vector2 localMovingAxis)
        {
            if(mRaycastContacts.Count > 0)
                localMovingAxis = ((Owner.MovingDirection == MovingDirection.Forward) ? -1 : 1) * Vector2.Perpendicular(mRaycastContacts[0].LocalNormal);
        }

        void GetLocalMovingAxisFromContacts(ref Vector2 localMovingAxis)
        {
            if (mContacts.Count > 0)
            {
                int fIndex = -1;
                float xValue = (Owner.MovingDirection == MovingDirection.Forward) ? float.MinValue : float.MaxValue;

                for (var cIndex = 0; cIndex < mContacts.Count; cIndex++)
                {
                    var contact = mContacts[cIndex];

                    if (contact.AngleToUpAxis < GroundAngle)
                    {
                        if ((Owner.MovingDirection == MovingDirection.Forward && contact.LocalPoint.x >= xValue)
                            || (Owner.MovingDirection == MovingDirection.Backward && contact.LocalPoint.x <= xValue))
                        {
                            xValue = contact.LocalPoint.x;
                            fIndex = cIndex;
                        }
                    }
                }
                if (fIndex >= 0)
                    localMovingAxis = ((Owner.MovingDirection == MovingDirection.Forward) ? -1 : 1) * Vector2.Perpendicular(mContacts[fIndex].LocalNormal);
            }
        }

        public void OrderedFixedUpdate()
        {
            
            Vector2 localVelocity = transform.InverseTransformDirection(Body.velocity);
            mBodyRelativeVelocity = Body.velocity;

            Vector2 externalLocalVelocity = Vector2.zero;

            var movingAxis = transform.right * (Owner.MovingDirection == MovingDirection.Forward ? 1.0f: -1.0f);
            
            mUnderfootObject = null;
            mStaticObjectDetectedInMovementDirection = false;
            mMovableObjectDetectedInMovementDirection = false;

            mRaycastContacts.Clear();
            for (var cIndex = 0; cIndex < mContacts.Count; cIndex++)
                mContacts[cIndex].CalculateCache();

            if (mJumpOperationInfo.InProcess && !mJumpOperationInfo.SyncedInFixedUpdate)
            {
                mJumpOperationInfo.SyncInFixedUpdate();
                Body.AddForce(transform.rotation * (CalculateStartSpeedFromHeight(localVelocity - mJumpOperationInfo.ExternalVelocity) * Vector2.up) * Body.mass, ForceMode2D.Impulse);           
            }
            if (!mJumpOperationInfo.InProcess)
            {
                FindUnderfootObjectInContacts(ref mUnderfootObject, ref mBodyRelativeVelocity);

                if (mUnderfootObject == null && mPreviousFrameUnderfootObject != null)
                    FindUnderfootObjectByRaycast(ref mUnderfootObject);

                if (mUnderfootObject != null)
                    UnderfootObjectIsManualMoved(ref externalLocalVelocity);
            }

            if (mPreviousFrameUnderfootObject != mUnderfootObject)
            {
                Owner.OnUnderfootObjectChanged(mPreviousFrameUnderfootObject);
            }

            mPreviousFrameUnderfootObject = mUnderfootObject;
         
            bool moveInFrame = false;

            if (mMovingOperationInfo.InProcess)
            {
                Vector2 localMovingAxis = transform.InverseTransformDirection(movingAxis);

                for (var index = 0; index < mContacts.Count; index++)
                {
                    var contact = mContacts[index];

                    if (Vector2.Dot(contact.Normal, movingAxis) <= 0 && contact.AngleToUpAxis > GroundAngle)
                    {
                        if (mUnderfootObject != null)
                        {
                            if (contact.Collider.attachedRigidbody == null || contact.Collider.attachedRigidbody.bodyType == RigidbodyType2D.Static)
                                mStaticObjectDetectedInMovementDirection = true;
                            else
                                mMovableObjectDetectedInMovementDirection = true;
                        }
                        else
                        {
                            mStaticObjectDetectedInMovementDirection = true;
                        }
                    }
                }

                if(!mStaticObjectDetectedInMovementDirection)
                {
                    if (!mJumpOperationInfo.InProcess)
                    {
                        GetLocalMovingAxisFromRaycast(ref localMovingAxis);
                        GetLocalMovingAxisFromContacts(ref localMovingAxis);
                    }

                    var localOwnVelocity = localVelocity - externalLocalVelocity;
                    var ownVelocityProjectOnMovingAxisVec = UnityExtension.Project(localOwnVelocity, localMovingAxis);
                    var codirectional = Mathf.Sign(Vector2.Dot(localOwnVelocity, localMovingAxis));

                    var deltaSpeed = mMovingOperationInfo.MovingSpeed - codirectional * ownVelocityProjectOnMovingAxisVec.magnitude;
                    //if we moving faster than max moving velocity we ignored, don't break
                    deltaSpeed = Mathf.Max(deltaSpeed, 0);
                    //but move with max permissible speed
                    deltaSpeed = Mathf.Min(deltaSpeed, mMovingOperationInfo.MovingSpeed);
                
                    mPreviousLocalMovingAxis = localMovingAxis;

                    moveInFrame = true;
                    mNeedToApplyBrakeForce = true;
                    mBreakMovingSpeed = mMovingOperationInfo.MovingSpeed;

                    Body.AddForce((Vector2)(transform.TransformDirection(localMovingAxis * deltaSpeed)) * Body.mass, ForceMode2D.Impulse);
                }
            }

      
            if (mNeedToApplyBrakeForce && !moveInFrame)
            {
                mNeedToApplyBrakeForce = false;
                if (!mStaticObjectDetectedInMovementDirection)
                {
                    ApplyBreakForce(localVelocity);
                }
            }

            if (mStaticObjectDetectedInMovementDirection)
            {
                if (mMovingOperationInfo.InBreakingState)
                {
                    mMovingOperationInfo.ResetBreakState();
                }
            }
 
            mJumpOperationInfo.Process(Time.fixedDeltaTime);
            mMovingOperationInfo.Process(Time.fixedDeltaTime);
        }


        void CalculateBreakForce(Vector2 localVelocity)
        {
            var velocityOnGroundSideVec = UnityExtension.Project(localVelocity, mPreviousLocalMovingAxis);
            var velocityAbs = Mathf.Min(Mathf.Abs(velocityOnGroundSideVec.magnitude), mBreakMovingSpeed);

            Body.AddForce(transform.rotation * (-mPreviousLocalMovingAxis * velocityAbs) * Body.mass, ForceMode2D.Impulse);
        }
        void ApplyBreakForce(Vector2 localVelocity)
        {
            var velocityOnGroundSideVec = UnityExtension.Project(localVelocity, mPreviousLocalMovingAxis);
            var velocityAbs = Mathf.Min(Mathf.Abs(velocityOnGroundSideVec.magnitude), mBreakMovingSpeed);

            Body.AddForce(transform.rotation * (-mPreviousLocalMovingAxis * velocityAbs) * Body.mass, ForceMode2D.Impulse);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            for (var cIndex = 0; cIndex < collisionInfo.contacts.Length; cIndex++)
            {
                var contactPoint = collisionInfo.contacts[cIndex];
                var contactInfo = mContacts.Get();
                contactInfo.Fill(contactPoint.collider, contactPoint.otherCollider, contactPoint.point, contactPoint.normal, contactPoint.relativeVelocity);
            }
        }
        protected virtual void OnCollisionStay2D(Collision2D collisionInfo)
        {
            OnCollisionExit2D(collisionInfo);
            OnCollisionEnter2D(collisionInfo);
        }

        protected virtual void OnCollisionExit2D(Collision2D collisionInfo)
        {
            for (var index = mContacts.Count - 1; index >= 0; index--)
            {
                var contactPointInfo = mContacts[index];
                if (contactPointInfo.Collider == collisionInfo.collider && contactPointInfo.OtherCollider == collisionInfo.otherCollider)
                    mContacts.Return(index);
            }
        }

        void OnDrawGizmos()
        {
            for (int i = 0 ; i < mContacts.Count; i++)
            {
                Gizmos.color = Color.red;
                var contact = mContacts[i];
                Gizmos.DrawLine(contact.Point, contact.Point + contact.Normal * 2);
            }

        }

        public class ContactInfo
        {
            public Vector2 Point { get; private set; }
            public Vector2 Normal { get; private set; }
            public Vector2 RelativeVelocity { get; private set; }
            public Collider2D Collider { get; private set; }
            public Collider2D OtherCollider { get; private set; }

            public float AngleToUpAxis { get; private set; }
            public Vector2 LocalPoint { get; private set; }
            public Vector2 LocalNormal { get; private set; }

            public ContactInfo() { }

            public void Fill(Collider2D collider, Collider2D otherCollider, Vector2 point, Vector2 normal, Vector2 relativeVelocity)
            {
                Collider = collider;
                OtherCollider = otherCollider;
                Point = point;
                Normal = normal;
                RelativeVelocity = relativeVelocity;
            }

            public void CalculateCache()
            {
                AngleToUpAxis = Mathf.Acos(Vector2.Dot(Normal, OtherCollider.transform.up)) * Mathf.Rad2Deg;
                LocalPoint = OtherCollider.transform.InverseTransformPoint(Point);
                LocalNormal = OtherCollider.transform.InverseTransformDirection(Normal);
            }
        }  
    }
}