using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsEditingToolMisc
{
    public class MouseOverInfo
    {
        int mShapeIndex = -1;
        int mPointIndex = -1;
        int mLineIndex = -1;
        int mCrossIndex = -1;
        Vector3 mLocalLineIntersection;

        public int ShapeIndex => mShapeIndex;
        public int PointIndex => mPointIndex;
        public int LineIndex => mLineIndex;
        public int CrossIndex => mCrossIndex;
        
        public Vector3 LocalLineIntersection => mLocalLineIntersection;

        public bool IsOverPoint => mPointIndex != -1;
        public bool IsOverLine => mLineIndex != -1;
        public bool IsOverCross => mCrossIndex != -1;

        public bool UpdateMouseOverInfo(ShapeCreator target, Vector3 mousePosition)
        {
            Vector3 ssMousePosition = Camera.current.WorldToScreenPoint(mousePosition);
            Vector3 lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            int newShapeIndex = -1;
            int newPointIndex = -1;
            int newLineIndex = -1;
            int newCrossIndex = -1;

            bool pointFound = false;
            for (int sIndex = 0; sIndex < target.Count; sIndex++)
            {
                Shape shape = target[sIndex];

                for (int pIndex = 0; pIndex < shape.Count; pIndex++)
                {
                    Vector2 ssP = Camera.current.WorldToScreenPoint(target.transform.TransformPoint(shape[pIndex].Position));
                    if (Vector2.Distance(ssP, ssMousePosition) < ShapeEditor.HandleRadius)
                    {
                        newShapeIndex = sIndex;
                        newPointIndex = pIndex;
                        pointFound = true;
                        break;
                    }
                }
            }

            if (pointFound)
            {
                mLineIndex = -1;
                mCrossIndex = -1;

                if (newPointIndex != mPointIndex)
                {
                    mShapeIndex = newShapeIndex;
                    mPointIndex = newPointIndex;

                    return true;
                }

                return false;
            }

            bool crossFound = false;
            bool lineFound = false;
            Vector3 lineIntersection = Vector3.zero;

            var calculatedResults = new List<CalculationResult>();
            for (int sIndex = 0; sIndex < target.Count; sIndex++)
            {
                Shape currentShape = target[sIndex];

                for (int pIndex = 0; pIndex < currentShape.Count; pIndex++)
                {
                    Vector3 lP0 = currentShape[pIndex].Position;
                    Vector3 lP1 = currentShape[(pIndex + 1) % currentShape.Count].Position;
                    Vector3 ssP0 = Camera.current.WorldToScreenPoint(target.transform.TransformPoint(lP0));
                    Vector3 ssP1 = Camera.current.WorldToScreenPoint(target.transform.TransformPoint(lP1));
                
                    if (ssP0.z < 0)
                        ssP0 = ssP1 + (ssP1 - ssP0);
                    if (ssP1.z < 0)
                        ssP1 = ssP0 - (ssP1 - ssP0);

                    var dstFromMouseToSegment = HandleUtility.DistancePointToLineSegment(ssMousePosition, ssP0, ssP1);
                    var distanceToSegment = HandleUtility.DistancePointToLineSegment(lMousePosition, lP0, lP1);

                    if (dstFromMouseToSegment < PointsEditingTool.LineHandleRadius)
                        calculatedResults.Add(new CalculationResult(sIndex, pIndex, distanceToSegment));
                }
            }

            if (calculatedResults.Count > 0)
            {
                calculatedResults.Sort((l, r) => l.LocalDistanceToMouse.CompareTo(r.LocalDistanceToMouse));
                newShapeIndex = calculatedResults[0].ShapeIndex;

                calculatedResults = calculatedResults.Where(i => ExtensionMethods.FloatEquals(calculatedResults[0].LocalDistanceToMouse, i.LocalDistanceToMouse) && i.ShapeIndex == newShapeIndex).ToList();
       
                if (calculatedResults.Count > 1)
                {
                    calculatedResults.Sort((l, r) => l.PointIndex.CompareTo(r.PointIndex));
                    if (calculatedResults.FindIndex(i => i.PointIndex == 0) >= 0 &&
                        calculatedResults.FindIndex(i => i.PointIndex == target[newShapeIndex].Count - 1) >= 0)
                        newCrossIndex = 0;
                    else
                        newCrossIndex = (calculatedResults[0].PointIndex + 1) % target[newShapeIndex].Count;

                    crossFound = true;
                }
                else
                {
                    newLineIndex = calculatedResults[0].PointIndex;
                  
                    Vector3 lP0 = target[newShapeIndex][newLineIndex].Position;
                    Vector3 lP1 = target[newShapeIndex][(newLineIndex + 1) % target[newShapeIndex].Count].Position;
                    Vector3 lSegmentVec = lP1 - lP0;
                    Vector3 lSegmentDir = lSegmentVec.normalized;

                    var vecFromMouseToP0 = lMousePosition - lP0;

                    lineIntersection = lP0 + Vector3.Project(vecFromMouseToP0, lSegmentDir);

                    lineFound = true;
                }  
            }

            if (crossFound)
            {
                mPointIndex = -1;
                mLineIndex = -1;
      
                if (newCrossIndex != mCrossIndex)
                {
                    mShapeIndex = newShapeIndex;
                    mCrossIndex = newCrossIndex;     
                }

                return true;
            }

            if (lineFound)
            {
                mPointIndex = -1;
                mCrossIndex = -1;
                mLocalLineIntersection = lineIntersection;

                if (newLineIndex != mLineIndex)
                {
                    mShapeIndex = newShapeIndex;
                    mLineIndex = newLineIndex;       
                }
              
                
                return true;
            }

            mPointIndex = -1;
            mCrossIndex = -1;
            mLineIndex = -1;
            mShapeIndex = -1;

            return false;
        }

        public void Reset()
        {
            mPointIndex = -1;
            mCrossIndex = -1;
            mLineIndex = -1;
            mShapeIndex = -1;
        }

        public struct CalculationResult
        {
            public readonly int ShapeIndex;
            public readonly int PointIndex;
            public readonly float LocalDistanceToMouse;

            public CalculationResult(int shapeIndex, int pointIndex, float lDistanceToMouse)
            {
                ShapeIndex = shapeIndex;
                PointIndex = pointIndex;
                LocalDistanceToMouse = lDistanceToMouse;
            }
        }
    }
}