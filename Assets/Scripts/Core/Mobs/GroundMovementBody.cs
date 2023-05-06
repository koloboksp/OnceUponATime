using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class GroundMovementBody : MonoBehaviour, IOrderedFixedUpdate
    {
        private readonly RaycastHit2D[] _noAllocGroundDetectionResults = new RaycastHit2D[10];
        private readonly List<IManualMoved> _noAllocManualMovedResults = new List<IManualMoved>();

        private readonly ReuseList<ContactInfo> _contacts = new ReuseList<ContactInfo>(() => new ContactInfo());
        private readonly ReuseList<ContactInfo> _raycastContacts = new ReuseList<ContactInfo>(() => new ContactInfo());

        private readonly MovingOperationInfo _movingOperationInfo = new MovingOperationInfo();
        private readonly JumpOperationInfo _jumpOperationInfo = new JumpOperationInfo();

        private GameObject _underfootObject;
        private GameObject _previousFrameUnderfootObject;

        private bool _staticObjectDetectedInMovementDirection;
        private bool _movableObjectDetectedInMovementDirection;

        private Vector2 _previousLocalMovingAxis;
        private Vector2 _bodyRelativeVelocity;

        private float _breakMovingSpeed;
        private bool _needToApplyBrakeForce = false;

        [FormerlySerializedAs("Owner")] [SerializeField] private GroundMovementCharacter _owner;

        [FormerlySerializedAs("Body")] [SerializeField] private Rigidbody2D _body;
        [FormerlySerializedAs("BodyCollider")] [SerializeField] private Collider2D _bodyCollider;
        [FormerlySerializedAs("BodyGroundCollider")] [SerializeField] private Collider2D _bodyGroundCollider;

        [FormerlySerializedAs("GroundAngle")] [SerializeField] private float _groundAngle = 45;
        [FormerlySerializedAs("GroundDetectionDistance")] [SerializeField] private float _groundDetectionDistance = 0.2f;

        [FormerlySerializedAs("JumpHeight")] [SerializeField] private float _jumpHeight = 1.5f;
        [FormerlySerializedAs("TimeIntervalBetweenJumps")] [SerializeField] private float _timeIntervalBetweenJumps = 0.2f;

        public MovingOperationInfo MovingOperationInfo => _movingOperationInfo;
        public JumpOperationInfo JumpOperationInfo => _jumpOperationInfo;

        public GameObject UnderfootObject => _underfootObject;
        public Vector2 RelativeVelocity => _bodyRelativeVelocity;

        public bool StaticObjectDetectedInMovementDirection => _staticObjectDetectedInMovementDirection;
        public bool MovableObjectOnMovingDirectionDetected => _movableObjectDetectedInMovementDirection;

        public int Order { get { return 0; } }

        public Collider2D BodyCollider => _bodyCollider;
        public Collider2D BodyGroundCollider => _bodyGroundCollider;

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
            if(_underfootObject != null)
                UnderfootObjectIsManualMoved(ref externalVelocity);

            _jumpOperationInfo.Execute(_timeIntervalBetweenJumps, externalVelocity);
        }

        public void StartMoving(float speed, bool instant = true, float acceleration = float.MaxValue)
        {
            _movingOperationInfo.Move(speed, instant, acceleration);
        }

        public void StopMoving(bool instant = true, float acceleration = float.MaxValue)
        {
            _movingOperationInfo.Break(instant, acceleration);
        }

        public void BreakMoving()
        {
            var inverseRotation = Quaternion.Inverse(transform.rotation);
            Vector2 localVelocity = inverseRotation * _body.velocity;
            CalculateBreakForce(localVelocity);     
        }

        public void ChangeDirection()
        {
            _previousLocalMovingAxis.y = -_previousLocalMovingAxis.y;
        }

        public void AbortAllControlledOperations()
        {
            _jumpOperationInfo.Abort();
            _movingOperationInfo.Abort();

            if (_needToApplyBrakeForce)
            {
                _needToApplyBrakeForce = false;
            }
        }

        public void AddDamageForce(float forceValue, Vector2 forceDir)
        {
            Vector2 localVelocity = transform.InverseTransformDirection(_body.velocity);
            Vector2 localForce = transform.InverseTransformDirection(forceValue * forceDir);// inverseRotation * (additionalForceInfo.ForceDirection * additionalForceInfo.ForceValue);
            localForce.y = Mathf.Lerp(
                localForce.y,
                CalculateStartSpeedFromHeight(localVelocity),
                Mathf.Clamp01(Vector2.Dot(localForce, Vector2.up)));

            _body.AddForce(transform.TransformDirection(localForce) * _body.mass, ForceMode2D.Impulse);
        }

        private float CalculateStartSpeedFromHeight(Vector2 localVelocity)
        {
            //vs = sqrt(h * 2 * g) - v0; 
            return Mathf.Sqrt(_jumpHeight * 2 * Physics2D.gravity.magnitude * _body.gravityScale) - localVelocity.y;
        }

        private void FindUnderfootObjectInContacts(ref GameObject underfootObject, ref Vector2 relativeVelocity)
        {
            int forwardContactIndex = -1;
            int backwardContactIndex = -1;

            for (var cIndex = 0; cIndex < _contacts.Count; cIndex++)
            {
                var contact = _contacts[cIndex];

                if (contact.AngleToUpAxis <= _groundAngle)
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
                underfootObject = _contacts[forwardContactIndex].Collider.gameObject;
                relativeVelocity = _contacts[forwardContactIndex].RelativeVelocity;
            }
        }

        private void FindUnderfootObjectByRaycast(ref GameObject underfootObject)
        {
            var resultsCount = Physics2D.RaycastNonAlloc(this.transform.position, -transform.up, _noAllocGroundDetectionResults, 1.0f);

            for (int rIndex = 0; rIndex < resultsCount; rIndex++)
            {
                var groundDetectionResult = _noAllocGroundDetectionResults[rIndex];
                if (!groundDetectionResult.collider.isTrigger &&
                    groundDetectionResult.collider != _bodyCollider &&
                    groundDetectionResult.collider != _bodyGroundCollider)
                {
                    if (groundDetectionResult.distance < _groundDetectionDistance)
                    {
                        float contactAngle = Mathf.Acos(Vector2.Dot(groundDetectionResult.normal, transform.up)) * Mathf.Rad2Deg;
                        if (contactAngle <= _groundAngle)
                        {            
                            underfootObject = groundDetectionResult.collider.gameObject;
                            int useIndex;
                            var contactInfo = _raycastContacts.Get(out useIndex);
                            contactInfo.Fill(useIndex, groundDetectionResult.collider, _bodyGroundCollider, groundDetectionResult.point, groundDetectionResult.normal, Vector2.zero);
                            contactInfo.CalculateCache();
                        }
                    }
                }
            }
        }


        private void UnderfootObjectIsManualMoved(ref Vector2 externalLocalVelocity)
        {       
            _underfootObject.GetComponents<IManualMoved>(_noAllocManualMovedResults);

            if (_noAllocManualMovedResults.Count > 0)
            {
                var manualMovedObj = _noAllocManualMovedResults[0];
              
                if (manualMovedObj.SpeedChanged)
                    _body.AddForce((manualMovedObj.NextStepSpeed - _body.velocity) * _body.mass, ForceMode2D.Impulse);     

                externalLocalVelocity = transform.InverseTransformDirection(manualMovedObj.NextStepSpeed);
            }
        }

        private void GetLocalMovingAxisFromRaycast(ref Vector2 localMovingAxis)
        {
            if(_raycastContacts.Count > 0)
                localMovingAxis = ((_owner.MovingDirection == MovingDirection.Forward) ? -1 : 1) * Vector2.Perpendicular(_raycastContacts[0].LocalNormal);
        }

        private void GetLocalMovingAxisFromContacts(ref Vector2 localMovingAxis)
        {
            if (_contacts.Count > 0)
            {
                int fIndex = -1;
                float xValue = (_owner.MovingDirection == MovingDirection.Forward) ? float.MinValue : float.MaxValue;

                for (var cIndex = 0; cIndex < _contacts.Count; cIndex++)
                {
                    var contact = _contacts[cIndex];

                    if (contact.AngleToUpAxis < _groundAngle)
                    {
                        if ((_owner.MovingDirection == MovingDirection.Forward && contact.LocalPoint.x >= xValue)
                            || (_owner.MovingDirection == MovingDirection.Backward && contact.LocalPoint.x <= xValue))
                        {
                            xValue = contact.LocalPoint.x;
                            fIndex = cIndex;
                        }
                    }
                }
                if (fIndex >= 0)
                    localMovingAxis = ((_owner.MovingDirection == MovingDirection.Forward) ? -1 : 1) * Vector2.Perpendicular(_contacts[fIndex].LocalNormal);
            }
        }

        public void OrderedFixedUpdate()
        {
            
            Vector2 localVelocity = transform.InverseTransformDirection(_body.velocity);
            _bodyRelativeVelocity = _body.velocity;

            Vector2 externalLocalVelocity = Vector2.zero;

            var movingAxis = transform.right * (_owner.MovingDirection == MovingDirection.Forward ? 1.0f: -1.0f);
            
            _underfootObject = null;
            _staticObjectDetectedInMovementDirection = false;
            _movableObjectDetectedInMovementDirection = false;

            _raycastContacts.Clear();
            for (var cIndex = 0; cIndex < _contacts.Count; cIndex++)
                _contacts[cIndex].CalculateCache();

            if (_jumpOperationInfo.InProcess && !_jumpOperationInfo.SyncedInFixedUpdate)
            {
                _jumpOperationInfo.SyncInFixedUpdate();
                _body.AddForce(transform.rotation * (CalculateStartSpeedFromHeight(localVelocity - _jumpOperationInfo.ExternalVelocity) * Vector2.up) * _body.mass, ForceMode2D.Impulse);           
            }
            if (!_jumpOperationInfo.InProcess)
            {
                FindUnderfootObjectInContacts(ref _underfootObject, ref _bodyRelativeVelocity);

                if (_underfootObject == null && _previousFrameUnderfootObject != null)
                    FindUnderfootObjectByRaycast(ref _underfootObject);

                if (_underfootObject != null)
                    UnderfootObjectIsManualMoved(ref externalLocalVelocity);
            }

            if (_previousFrameUnderfootObject != _underfootObject)
            {
                _owner.OnUnderfootObjectChanged(_previousFrameUnderfootObject);
            }

            _previousFrameUnderfootObject = _underfootObject;
         
            bool moveInFrame = false;

            if (_movingOperationInfo.InProcess)
            {
                Vector2 localMovingAxis = transform.InverseTransformDirection(movingAxis);

                for (var index = 0; index < _contacts.Count; index++)
                {
                    var contact = _contacts[index];

                    if (Vector2.Dot(contact.Normal, movingAxis) <= 0 && contact.AngleToUpAxis > _groundAngle)
                    {
                        if (_underfootObject != null)
                        {
                            if (contact.Collider.attachedRigidbody == null || contact.Collider.attachedRigidbody.bodyType == RigidbodyType2D.Static)
                                _staticObjectDetectedInMovementDirection = true;
                            else
                                _movableObjectDetectedInMovementDirection = true;
                        }
                        else
                        {
                            _staticObjectDetectedInMovementDirection = true;
                        }
                    }
                }

                if(!_staticObjectDetectedInMovementDirection)
                {
                    if (!_jumpOperationInfo.InProcess)
                    {
                        GetLocalMovingAxisFromRaycast(ref localMovingAxis);
                        GetLocalMovingAxisFromContacts(ref localMovingAxis);
                    }

                    var localOwnVelocity = localVelocity - externalLocalVelocity;
                    var ownVelocityProjectOnMovingAxisVec = UnityExtension.Project(localOwnVelocity, localMovingAxis);
                    var codirectional = Mathf.Sign(Vector2.Dot(localOwnVelocity, localMovingAxis));

                    var deltaSpeed = _movingOperationInfo.MovingSpeed - codirectional * ownVelocityProjectOnMovingAxisVec.magnitude;
                    //if we moving faster than max moving velocity we ignored, don't break
                    deltaSpeed = Mathf.Max(deltaSpeed, 0);
                    //but move with max permissible speed
                    deltaSpeed = Mathf.Min(deltaSpeed, _movingOperationInfo.MovingSpeed);
                
                    _previousLocalMovingAxis = localMovingAxis;

                    moveInFrame = true;
                    _needToApplyBrakeForce = true;
                    _breakMovingSpeed = _movingOperationInfo.MovingSpeed;

                    _body.AddForce((Vector2)(transform.TransformDirection(localMovingAxis * deltaSpeed)) * _body.mass, ForceMode2D.Impulse);
                }
            }

      
            if (_needToApplyBrakeForce && !moveInFrame)
            {
                _needToApplyBrakeForce = false;
                if (!_staticObjectDetectedInMovementDirection)
                {
                    ApplyBreakForce(localVelocity);
                }
            }

            if (_staticObjectDetectedInMovementDirection)
            {
                if (_movingOperationInfo.InBreakingState)
                {
                    _movingOperationInfo.ResetBreakState();
                }
            }
 
            _jumpOperationInfo.Process(Time.fixedDeltaTime);
            _movingOperationInfo.Process(Time.fixedDeltaTime);
        }


        private void CalculateBreakForce(Vector2 localVelocity)
        {
            var velocityOnGroundSideVec = UnityExtension.Project(localVelocity, _previousLocalMovingAxis);
            var velocityAbs = Mathf.Min(Mathf.Abs(velocityOnGroundSideVec.magnitude), _breakMovingSpeed);

            _body.AddForce(transform.rotation * (-_previousLocalMovingAxis * velocityAbs) * _body.mass, ForceMode2D.Impulse);
        }

        private void ApplyBreakForce(Vector2 localVelocity)
        {
            var velocityOnGroundSideVec = UnityExtension.Project(localVelocity, _previousLocalMovingAxis);
            var velocityAbs = Mathf.Min(Mathf.Abs(velocityOnGroundSideVec.magnitude), _breakMovingSpeed);

            _body.AddForce(transform.rotation * (-_previousLocalMovingAxis * velocityAbs) * _body.mass, ForceMode2D.Impulse);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            for (var cIndex = 0; cIndex < collisionInfo.contacts.Length; cIndex++)
            {
                var contactPoint = collisionInfo.contacts[cIndex];
                int useIndex;
                var contactInfo = _contacts.Get(out useIndex);
                contactInfo.Fill(useIndex, contactPoint.collider, contactPoint.otherCollider, contactPoint.point, contactPoint.normal, contactPoint.relativeVelocity);
            }
        }
        protected virtual void OnCollisionStay2D(Collision2D collisionInfo)
        {
            OnCollisionExit2D(collisionInfo);
            OnCollisionEnter2D(collisionInfo);
        }

        protected virtual void OnCollisionExit2D(Collision2D collisionInfo)
        {
            for (var index = _contacts.Count - 1; index >= 0; index--)
            {
                var contactPointInfo = _contacts[index];
                if (contactPointInfo.Collider == collisionInfo.collider && contactPointInfo.OtherCollider == collisionInfo.otherCollider)
                    _contacts.Return(contactPointInfo.UseIndex);
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0 ; i < _contacts.Count; i++)
            {
                Gizmos.color = Color.red;
                var contact = _contacts[i];
                Gizmos.DrawLine(contact.Point, contact.Point + contact.Normal * 2);
            }

        }

        public class ContactInfo
        {
            public int UseIndex { get; private set; }
            public Vector2 Point { get; private set; }
            public Vector2 Normal { get; private set; }
            public Vector2 RelativeVelocity { get; private set; }
            public Collider2D Collider { get; private set; }
            public Collider2D OtherCollider { get; private set; }

            public float AngleToUpAxis { get; private set; }
            public Vector2 LocalPoint { get; private set; }
            public Vector2 LocalNormal { get; private set; }

            public ContactInfo() { }

            public void Fill(int useIndex, Collider2D collider, Collider2D otherCollider, Vector2 point, Vector2 normal, Vector2 relativeVelocity)
            {
                UseIndex = useIndex;
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