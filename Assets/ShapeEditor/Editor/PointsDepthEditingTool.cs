using Assets.ShapeEditor.Editor.PointsDepthEditingTools;
using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ShapeEditor.Editor
{
    public class PointsDepthEditingTool : EditingTool
    {
        const float LineDepthBias = 0.01f;

        private readonly MouseOverInfo _mouseOverInfo = new MouseOverInfo();
        private readonly SelectionInfo _mouseSelectionInfo = new SelectionInfo();

        public PointsDepthEditingTool(ShapeEditor owner) : base(owner)
        {
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if (!_mouseSelectionInfo.ShapeSelected && Owner.ShapeCreator.Count > 0)
                _mouseSelectionInfo.SelectShape(0);
        }

        public override void Repaint()
        {
            base.Repaint();

            var worldShapeNormal = Owner.ShapeCreator.transform.TransformDirection(-Camera.current.transform.forward);

            var oldZTestValue = Handles.zTest;
            Handles.zTest = CompareFunction.Less;

            for (int shapeIndex = 0; shapeIndex < Owner.ShapeCreator.Count; shapeIndex++)
            {
                Shape shapeToDraw = Owner.ShapeCreator[shapeIndex];
                bool shapeIsSelected = shapeIndex == _mouseOverInfo.ShapeIndex;

                var color = (shapeIsSelected) ? ShapeEditor.ContoursHighlightedColor : ShapeEditor.ContoursNormalColor;
                for (int i = 0; i < shapeToDraw.Count; i++)
                {
                    var point0 = shapeToDraw[i];
                    var point1 = shapeToDraw[(i + 1) % shapeToDraw.Count];

                    Vector3 p0f = Owner.ShapeCreator.transform.TransformPoint(point0.GetPosition(-Owner.ShapeCreator.Depth, Side.Front));
                    Vector3 p0b = Owner.ShapeCreator.transform.TransformPoint(point0.GetPosition(Owner.ShapeCreator.Depth, Side.Back));
                    Vector3 p1f = Owner.ShapeCreator.transform.TransformPoint(point1.GetPosition(-Owner.ShapeCreator.Depth, Side.Front));
                    Vector3 p1b = Owner.ShapeCreator.transform.TransformPoint(point1.GetPosition(Owner.ShapeCreator.Depth, Side.Back));

                    var depthBiasUpVec = Vector3.Cross(p1f - p0f, Owner.ShapeCreator.transform.forward).normalized * LineDepthBias;

                    Handles.color = color;
                    Handles.DrawDottedLine(p0f + depthBiasUpVec, p1f + depthBiasUpVec, 4);
                    Handles.DrawDottedLine(p0b + depthBiasUpVec, p1b + depthBiasUpVec, 4);

                    Handles.DrawWireDisc(p0f, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p0f));
                    Handles.DrawWireDisc(p0b, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p0b));
                }
            }

            if (_mouseOverInfo.IsOverPoint)
            {
                var shape = Owner.ShapeCreator[_mouseOverInfo.ShapeIndex];
                var point = shape[_mouseOverInfo.PointIndex];

                Vector3 pPosition = Owner.ShapeCreator.transform.TransformPoint(point.GetPosition(
                    _mouseOverInfo.PointSide == Side.Front ? -Owner.ShapeCreator.Depth : Owner.ShapeCreator.Depth,
                    _mouseOverInfo.PointSide));

                Handles.color = Color.red;
                Handles.DrawWireDisc(pPosition, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(pPosition, 1));
            }

            Handles.zTest = oldZTestValue;
        }

        public override void HandleInput(Event guiEvent)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    if (_mouseOverInfo.IsOverPoint)
                    {
                        _mouseSelectionInfo.Select(_mouseOverInfo.ShapeIndex, _mouseOverInfo.PointIndex, _mouseOverInfo.PointSide, 
                            Owner.ShapeCreator, guiEvent.mousePosition);  
                    }
                    else
                    {
                        Owner.BreakEditing();
                    }
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    _mouseSelectionInfo.StopDrag(Owner.ShapeCreator);            
                }
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                if (Event.current.button == 0)
                {
                    if (_mouseSelectionInfo.PointSelected)
                    {
                        _mouseSelectionInfo.DragPoint(Owner.ShapeCreator, guiEvent.mousePosition);
                        Owner.MarkSceneUIDirty();
                        Owner.MarkShapeDirty();
                    }
                }
            }
            if (Event.current.type == EventType.MouseMove)
            {
                Plane targetPlane = new Plane(
                    Owner.ShapeCreator.transform.position,
                    Owner.ShapeCreator.transform.position + Owner.ShapeCreator.transform.right,
                    Owner.ShapeCreator.transform.position + Owner.ShapeCreator.transform.up);

                float distance;
                Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                targetPlane.Raycast(mouseRay, out distance);

                var mousePosition = mouseRay.GetPoint(distance);  
                if(_mouseOverInfo.UpdateMouseOverInfo(Owner.ShapeCreator, mousePosition))
                    Owner.MarkSceneUIDirty();
            }
        }
    }
}