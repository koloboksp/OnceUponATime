using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace Assets.Scripts.Core
{
    public interface IManualMoved
    {
        Vector2 NextStepSpeed { get; }
        bool SpeedChanged { get; }
        Vector2 CurrentStepSpeed { get; }
        bool DirectionChanged { get; }
    }

    public class MovablePlatform : MonoBehaviour, IOrderedFixedUpdate
    {
        const float ShiftThreshold = 0.01f;

        readonly PathTraveler mPathTraveler = new PathTraveler();

        public MovablePlatformMovePart MovablePart;
        public float Speed = 1.5f;
        
        public int Order { get { return 1; } }

        protected virtual void OnEnable()
        {
            OrderedFixedUpdateManager.Add(this);
        }

        protected virtual void OnDisable()
        {
            OrderedFixedUpdateManager.Remove(this);
        }

        void Start()
        {
            mPathTraveler.RebuildPath(gameObject.GetComponentsInChildren<IPoint>());
            mPathTraveler.Move(Speed, Time.fixedDeltaTime);
        }
      
        public void OrderedFixedUpdate()
        {
            var previousPositionShift = mPathTraveler.Position - mPathTraveler.PreviousPosition;
            mPathTraveler.Move(Speed, Time.fixedDeltaTime);
            var nextPositionShift = mPathTraveler.Position - mPathTraveler.PreviousPosition;

            bool speedChanged = false;
            bool directionChanged = false;

            if (mPathTraveler.IsReachedTheEnd)
                mPathTraveler.MovingDirection = -mPathTraveler.MovingDirection;

            var deltaShift = (nextPositionShift - previousPositionShift) / Time.fixedDeltaTime;
            if (deltaShift.magnitude > ShiftThreshold)
                speedChanged = true;
          
            MovablePart.MovePosition(mPathTraveler.PreviousPosition, nextPositionShift / Time.fixedDeltaTime, previousPositionShift / Time.fixedDeltaTime, speedChanged, directionChanged);
        }

        void OnDrawGizmos()
        {
            var points = gameObject.GetComponentsInChildren<IPoint>();
            for (var ppIndex = 0; ppIndex < points.Length - 1; ppIndex++)
            {
                var point = points[ppIndex];
                var nextPoint = points[ppIndex + 1];

                Gizmos.color = Color.red;
                Gizmos.DrawLine(point.Position, nextPoint.Position);
            }
        }

        class PathTraveler
        {
            List<IPoint> mPathPoints;
            float mPathLength;

            float mTraveledDistance;
            Vector3 mPreviousPosition;
            Vector3 mPosition;
            float mMovingDirection = 1;

            public void RebuildPath(IEnumerable<IPoint> points)
            {
                mPathPoints = new List<IPoint>(points);
                CalculateLength();

                mTraveledDistance = 0.0f;
                Move(0, 0);
            }

            void CalculateLength()
            {
                mPathLength = 0.0f;
                for (int ppIndex = 0; ppIndex < mPathPoints.Count - 1; ppIndex++)
                {
                    var pathPoint = mPathPoints[ppIndex];
                    var nextPathPoint = mPathPoints[ppIndex + 1];

                    mPathLength += (nextPathPoint.Position - pathPoint.Position).magnitude;
                }
            }
            public void Move(float speed, float dTime)
            {
                mPreviousPosition = mPosition;

                if (mPathPoints.Count == 0)
                    return;
                if (mPathPoints.Count == 1)
                    mPosition = mPathPoints[0].Position;
                else
                {
                   // float offset = mMovingDirection * speed * dTime;
                    mTraveledDistance += mMovingDirection * speed * dTime;
                    mTraveledDistance = Mathf.Clamp(mTraveledDistance, 0.0f, mPathLength);

                    float checkedDistance = 0.0f;
                    for (int ppIndex = 0; ppIndex < mPathPoints.Count - 1; ppIndex++)
                    {
                        var pathPoint = mPathPoints[ppIndex];
                        var nextPathPoint = mPathPoints[ppIndex + 1];

                        Vector3 pathPartVec = nextPathPoint.Position - pathPoint.Position;
                        float pathPartLength = pathPartVec.magnitude;
                        if (pathPartLength <= float.Epsilon)
                            continue;

                        if (checkedDistance <= mTraveledDistance && mTraveledDistance <= checkedDistance + pathPartLength)
                        {
                            var pathPartTraveledDistance = mTraveledDistance - checkedDistance;

                            mPosition = Vector3.Lerp(pathPoint.Position, nextPathPoint.Position, pathPartTraveledDistance / pathPartLength);
                            break;
                        }

                        checkedDistance += pathPartLength;
                    }
                }
            }

            public Vector3 Position => mPosition;
            public Vector3 PreviousPosition => mPreviousPosition;

            public float MovingDirection
            {
                get
                {
                    return mMovingDirection;
                }
                set
                {
                    if (mMovingDirection != value)
                        mMovingDirection = value;
                }
            }

            public bool IsReachedTheEnd
            {
                get
                {
                    if (mTraveledDistance <= 0 || mTraveledDistance >= mPathLength)
                        return true;
                    return false;
                }
            }
        }
    }
}