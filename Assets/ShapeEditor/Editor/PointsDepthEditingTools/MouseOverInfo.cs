using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsDepthEditingTools
{
    public class MouseOverInfo
    {
        private int _shapeIndex = -1;
        private int _pointIndex = -1;
        private Side _pointSide;

        public int ShapeIndex => _shapeIndex;
        public int PointIndex => _pointIndex;
        public Side PointSide => _pointSide;
        public bool IsOverPoint => _pointIndex != -1;
        
        public bool UpdateMouseOverInfo(ShapeCreator owner, Vector3 mousePosition)
        {
            Vector3 ssMousePosition = Camera.current.WorldToScreenPoint(mousePosition);

            int mouseOverPointIndex = -1;
            int mouseOverShapeIndex = -1;

            for (int shapeIndex = 0; shapeIndex < owner.Count; shapeIndex++)
            {
                Shape shape = owner[shapeIndex];

                for (int i = 0; i < shape.Count; i++)
                {
                    Vector2 ssFrontP = Camera.current.WorldToScreenPoint(owner.transform.TransformPoint(shape[i].GetPosition(-owner.Depth, Side.Front)));
                    if (Vector2.Distance(ssFrontP, ssMousePosition) < ShapeEditor.HandleRadius)
                    {
                        mouseOverPointIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                        _pointSide = Side.Front;
                        break;
                    }
                    var ssBackP = Camera.current.WorldToScreenPoint(owner.transform.TransformPoint(shape[i].GetPosition(owner.Depth, Side.Back)));
                    if (Vector2.Distance(ssBackP, ssMousePosition) < ShapeEditor.HandleRadius)
                    {
                        mouseOverPointIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                        _pointSide = Side.Back;
                        break;
                    }
                }
            }

            if (mouseOverShapeIndex != _shapeIndex || mouseOverPointIndex != _pointIndex)
            {
                _shapeIndex = mouseOverShapeIndex;
                _pointIndex = mouseOverPointIndex;

                return true;
            }

            return false;
        }
    }
}