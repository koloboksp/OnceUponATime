using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor.Editor.PointsDepthEditingTools
{
    public class MouseOverInfo
    {
        int mShapeIndex = -1;
        int mPointIndex = -1;
        Side mPointSide;

        public int ShapeIndex => mShapeIndex;
        public int PointIndex => mPointIndex;
        public Side PointSide => mPointSide;

        public bool IsOverPoint => mPointIndex != -1;

       
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
                        mPointSide = Side.Front;
                        break;
                    }
                    var ssBackP = Camera.current.WorldToScreenPoint(owner.transform.TransformPoint(shape[i].GetPosition(owner.Depth, Side.Back)));
                    if (Vector2.Distance(ssBackP, ssMousePosition) < ShapeEditor.HandleRadius)
                    {
                        mouseOverPointIndex = i;
                        mouseOverShapeIndex = shapeIndex;
                        mPointSide = Side.Back;
                        break;
                    }
                }
            }

            if (mouseOverShapeIndex != mShapeIndex || mouseOverPointIndex != mPointIndex)
            {
                mShapeIndex = mouseOverShapeIndex;
                mPointIndex = mouseOverPointIndex;

                return true;
            }

            return false;
        }
    }
}