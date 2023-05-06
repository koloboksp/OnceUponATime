using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsDepthEditingTools
{
    public class SelectionInfo
    {
        private int _shapeIndex = -1;
        private int _pointIndex = -1;
        private Side _pointSide;

        private bool _dragEnable;
        private bool _dragHappened;
        private Plane _dragPlane;
        private Vector3 _localPointPositionAtDragStart;
        private Vector3 _localMouseOffsetAtDragStart;
        private Point _pointStateAtDragStart;

        public int PointIndex => _pointIndex;
        public int ShapeIndex => _shapeIndex;

        public bool PointSelected => _pointIndex != -1;
        public bool ShapeSelected => _shapeIndex != -1;

        public void Select(int shapeIndex, int pointIndex, Side pointSide, ShapeCreator target, Vector2 ssMousePosition)
        {
            _shapeIndex = shapeIndex;
            _pointIndex = pointIndex;
            _pointSide = pointSide;

            var lPointPosition = target[_shapeIndex][_pointIndex].GetPosition(0, _pointSide);
            var pointPosition = target.transform.TransformPoint(lPointPosition);
            var pp0 = pointPosition;
            var pp1 = pointPosition + target.transform.forward;
            var pp2 = pointPosition + Vector3.Cross(target.transform.forward, Camera.current.transform.forward);

            _dragPlane = new Plane(pp0, pp1, pp2);

            float distance;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            _dragPlane.Raycast(mouseRay, out distance);

            Vector3 mousePosition = mouseRay.GetPoint(distance);
            Vector3 lMousePosition = target.transform.InverseTransformPoint(mousePosition);

            _pointStateAtDragStart = target[_shapeIndex][_pointIndex];
            _localPointPositionAtDragStart = lPointPosition;
            _localMouseOffsetAtDragStart = lPointPosition - lMousePosition;

            _dragEnable = true;
            _dragHappened = false;
        }

        public void DragPoint(ShapeCreator target, Vector2 ssMousePosition)
        {
            if(!_dragEnable)
                return;

            Ray mouseRay = HandleUtility.GUIPointToWorldRay(ssMousePosition);
            float distance;
            _dragPlane.Raycast(mouseRay, out distance);
            
            Vector3 mousePosition = mouseRay.GetPoint(distance);
            Vector3 lMousePosition = target.transform.InverseTransformPoint(mousePosition);
            
            Vector3 lAxisDir = Vector3.forward;
           
            var vecFromMouseToP0 = _localPointPositionAtDragStart - (lMousePosition + _localMouseOffsetAtDragStart);
            var pointOnAxis = _localPointPositionAtDragStart - Vector3.Project(vecFromMouseToP0, lAxisDir);

            var point = target[_shapeIndex][_pointIndex];
            point.SetPosition(pointOnAxis, _pointSide);

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
                Undo.RecordObject(target, "Depth drag point");
                target[_shapeIndex][_pointIndex] = pointCurrentState;
            }
        }

        public void SelectShape(int shapeIndex)
        {
            _shapeIndex = shapeIndex;
        }
    }
}