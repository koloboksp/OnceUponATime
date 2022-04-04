using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsDepthEditingTools
{
    public class SelectionInfo
    {
        int mShapeIndex = -1;
        int mPointIndex = -1;
        Side mPointSide;

        bool mDragEnable;
        bool mDragHappened;
        Plane mDragPlane;
        Vector3 mLocalPointPositionAtDragStart;
        Vector3 mLocalMouseOffsetAtDragStart;
        Point mPointStateAtDragStart;

        public int PointIndex => mPointIndex;
        public int ShapeIndex => mShapeIndex;

        public bool PointSelected => mPointIndex != -1;
        public bool ShapeSelected => mShapeIndex != -1;

        public void Select(int shapeIndex, int pointIndex, Side pointSide, ShapeCreator target, Vector2 ssMousePosition)
        {
            mShapeIndex = shapeIndex;
            mPointIndex = pointIndex;
            mPointSide = pointSide;

            var lPointPosition = target[mShapeIndex][mPointIndex].GetPosition(0, mPointSide);
            var pointPosition = target.transform.TransformPoint(lPointPosition);
            var pp0 = pointPosition;
            var pp1 = pointPosition + target.transform.forward;
            var pp2 = pointPosition + Vector3.Cross(target.transform.forward, Camera.current.transform.forward);

            mDragPlane = new Plane(pp0, pp1, pp2);

            float distance;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            mDragPlane.Raycast(mouseRay, out distance);

            Vector3 mousePosition = mouseRay.GetPoint(distance);
            Vector3 lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            mPointStateAtDragStart = target[mShapeIndex][mPointIndex];
            mLocalPointPositionAtDragStart = lPointPosition;
            mLocalMouseOffsetAtDragStart = lPointPosition - lMousePosition;

            mDragEnable = true;
            mDragHappened = false;
        }

        public void DragPoint(ShapeCreator target, Vector2 ssMousePosition)
        {
            if(!mDragEnable)
                return;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            mDragPlane.Raycast(mouseRay, out distance);
            
            Vector3 mousePosition = mouseRay.GetPoint(distance);
            Vector3 lMousePosition = target.transform.InverseTransformPoint(mousePosition);
            
            Vector3 lAxisDir = Vector3.forward;
           
            var vecFromMouseToP0 = mLocalPointPositionAtDragStart - (lMousePosition + mLocalMouseOffsetAtDragStart);
            var pointOnAxis = mLocalPointPositionAtDragStart - Vector3.Project(vecFromMouseToP0, lAxisDir);

            var point = target[mShapeIndex][mPointIndex];
            point.SetPosition(pointOnAxis, mPointSide);

            target[mShapeIndex][mPointIndex] = point;

            mDragHappened = true;
        }

        public void StopDrag(ShapeCreator target)
        {
            mDragEnable = false;
            if (mDragHappened)
            {
                var pointCurrentState = target[mShapeIndex][mPointIndex];
                target[mShapeIndex][mPointIndex] = mPointStateAtDragStart;
                Undo.RecordObject(target, "Depth drag point");
                target[mShapeIndex][mPointIndex] = pointCurrentState;
            }
        }

        public void SelectShape(int shapeIndex)
        {
            mShapeIndex = shapeIndex;
        }
    }
}