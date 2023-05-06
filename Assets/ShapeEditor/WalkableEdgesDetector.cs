using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class WalkableEdgesDetector
    {
        private float mWalkableAngle;
        private List<List<int>> mEdgesPointsIndexes;

        public List<List<int>> EdgesPointsIndexes => mEdgesPointsIndexes;

        public bool Found => mEdgesPointsIndexes.Count > 0;

        public WalkableEdgesDetector(List<Point> hullPoints, float walkableAngle)
        {
            mWalkableAngle = walkableAngle;
           
            mEdgesPointsIndexes = FindWalkableEdges(hullPoints);
        }

        private List<List<int>> FindWalkableEdges(List<Point> hullPoints)
        {
            List<List<int>> walkablePolyEdges = new List<List<int>>();

            List<bool> walkableOfEdges = new List<bool>();
            for (var index = 0; index < hullPoints.Count; index++)
            {
                var polygonPoint = hullPoints[index % hullPoints.Count];
                var polygonNextPoint = hullPoints[(index + 1) % hullPoints.Count];

                var segmentVec = polygonPoint.Position - polygonNextPoint.Position;
                var segmentDir = segmentVec.normalized;

                bool isWalkable = false;
                var angleToUp = Mathf.Acos(Vector2.Dot(segmentDir, Vector2.up)) * Mathf.Rad2Deg;
                var angleToRight = Mathf.Acos(Vector2.Dot(segmentDir, Vector2.right)) * Mathf.Rad2Deg;

                if (Mathf.PI * Mathf.Rad2Deg * 0.5f - mWalkableAngle < angleToUp
                    && angleToUp < Mathf.PI * Mathf.Rad2Deg * 0.5f + mWalkableAngle
                    && angleToRight < Mathf.PI * Mathf.Rad2Deg * 0.5f)
                {
                    isWalkable = true;
                }
                walkableOfEdges.Add(isWalkable);
            }

            var firstNotWalkablePointIndex = walkableOfEdges.FindIndex(i => i == false);
            if (firstNotWalkablePointIndex >= 0)
            {
                bool prevWalkable = false;

                for (int index = firstNotWalkablePointIndex; index <= firstNotWalkablePointIndex + hullPoints.Count; index++)
                {
                    int derivedIndex = index % hullPoints.Count;
                    var walkable = walkableOfEdges[derivedIndex];

                    if (prevWalkable != walkable)
                    {
                        if (walkable)
                        {
                            walkablePolyEdges.Add(new List<int>());
                            walkablePolyEdges.Last().Add(derivedIndex);
                        }
                        else
                        {
                            walkablePolyEdges.Last().Add(derivedIndex);
                        }
                    }
                    else
                    {
                        if (walkable)
                        {
                            walkablePolyEdges.Last().Add(derivedIndex);
                        }
                    }

                    prevWalkable = walkable;
                }
            }
            else
            {
                walkablePolyEdges.Add(new List<int>(hullPoints.Select((p, i) => i)));
            }

            return walkablePolyEdges;
        }
    }
}