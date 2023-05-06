using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsEditingToolMisc
{
    public class SelectionInfo
    {
        private int _shapeIndex = -1;
        private int _pointIndex = -1;    
        
        private bool _dragEnable;
        private bool _dragHappened;

        private Plane _dragPlane;
        private Point _pointStateAtDragStart;
        private Vector2 _localPointPositionAtStartOfDrag;
        private Vector2 _localMouseOffsetAtStartOfDrag;

        public int ShapeIndex
        {
            get => _shapeIndex;
            set => _shapeIndex = value;
        }

        public int PointIndex => _pointIndex;
        public bool PointIsSelected => PointIndex != -1;
        public bool DragHappened => _dragHappened;
        public bool DragEnable => _dragEnable;

        public void DeselectPoint()
        {
            _pointIndex = -1;
        }
        public void SelectPointByMouse(int shapeIndex, int pointIndex, ShapeCreator target, Vector2 ssMousePosition)
        {
            _shapeIndex = shapeIndex;
            _pointIndex = pointIndex;
  
            _dragPlane = new Plane(
                target.transform.position,
                target.transform.position + target.transform.right,
                target.transform.position + target.transform.up);

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            _dragPlane.Raycast(mouseRay, out distance);
            var mousePosition = mouseRay.GetPoint(distance);
            var lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            _pointStateAtDragStart = target[shapeIndex][_pointIndex];
            _localPointPositionAtStartOfDrag = target[shapeIndex][_pointIndex].Position;
            _localMouseOffsetAtStartOfDrag = _localPointPositionAtStartOfDrag - (Vector2)lMousePosition;
           
            _dragHappened = false;
            _dragEnable = true;
        }
  
        public void DragPoint(ShapeCreator target, Vector2 ssMousePosition)
        {
            if (!_dragEnable)
                return;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            _dragPlane.Raycast(mouseRay, out distance);
            var mousePosition = mouseRay.GetPoint(distance);
            var lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            var point = target[_shapeIndex][_pointIndex];
            point.Position = (Vector2)lMousePosition + _localMouseOffsetAtStartOfDrag;
            target[_shapeIndex][_pointIndex] = point;

            _dragHappened = true;
        }

        public void StopDrag(ShapeCreator target)
        {
            _dragEnable = false;
            if (_dragHappened)
            {
                var pointCurrentState = target[_shapeIndex][_pointIndex];
                target[_shapeIndex][_pointIndex] = _pointStateAtDragStart;
                Undo.RecordObject(target, "Drag point");
                target[_shapeIndex][_pointIndex] = pointCurrentState;
            }

            _dragHappened = false;
        }

    }
}