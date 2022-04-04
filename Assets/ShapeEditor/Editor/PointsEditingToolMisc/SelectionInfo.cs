using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsEditingToolMisc
{
    public class SelectionInfo
    {
        public int mShapeIndex = -1;
        int mPointIndex = -1;    
        
        bool mDragEnable;
        bool mDragHappened;

        Plane mDragPlane;
        Point mPointStateAtDragStart;
        Vector2 mLocalPointPositionAtStartOfDrag;
        Vector2 mLocalMouseOffsetAtStartOfDrag;

        public int ShapeIndex => mShapeIndex;
        public int PointIndex => mPointIndex;
        public bool PointIsSelected => PointIndex != -1;
        public bool DragHappened => mDragHappened;
        public bool DragEnable => mDragEnable;

        public void DeselectPoint()
        {
            mPointIndex = -1;
        }
        public void SelectPointByMouse(int shapeIndex, int pointIndex, ShapeCreator target, Vector2 ssMousePosition)
        {
            mShapeIndex = shapeIndex;
            mPointIndex = pointIndex;
  
            mDragPlane = new Plane(
                target.transform.position,
                target.transform.position + target.transform.right,
                target.transform.position + target.transform.up);

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            mDragPlane.Raycast(mouseRay, out distance);
            var mousePosition = mouseRay.GetPoint(distance);
            var lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            mPointStateAtDragStart = target[shapeIndex][mPointIndex];
            mLocalPointPositionAtStartOfDrag = target[shapeIndex][mPointIndex].Position;
            mLocalMouseOffsetAtStartOfDrag = mLocalPointPositionAtStartOfDrag - (Vector2)lMousePosition;
           
            mDragHappened = false;
            mDragEnable = true;
        }
  
        public void DragPoint(ShapeCreator target, Vector2 ssMousePosition)
        {
            if (!mDragEnable)
                return;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            mDragPlane.Raycast(mouseRay, out distance);
            var mousePosition = mouseRay.GetPoint(distance);
            var lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            var point = target[mShapeIndex][mPointIndex];
            point.Position = (Vector2)lMousePosition + mLocalMouseOffsetAtStartOfDrag;
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
                Undo.RecordObject(target, "Drag point");
                target[mShapeIndex][mPointIndex] = pointCurrentState;
            }

            mDragHappened = false;
        }

    }
}