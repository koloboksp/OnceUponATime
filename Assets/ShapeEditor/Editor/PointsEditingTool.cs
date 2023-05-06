using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Editor.PointsEditingToolMisc;
using Assets.ShapeEditor.Geometry;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    public class PointsEditingTool : EditingTool
    {
        public const float LineHandleRadius = ShapeEditor.HandleRadius * 10.0f;

        private  readonly SelectionInfo _selectionInfo = new SelectionInfo();
        private readonly MouseOverInfo _mouseOverInfo = new MouseOverInfo();

        private InstrumentMode _instrumentMode = InstrumentMode.AddOrMovePoints;

        private Vector3 _localMousePosition;
        
        public PointsEditingTool(ShapeEditor owner) : base(owner)
        {
           
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if(_selectionInfo.ShapeIndex == -1)
                if (Owner.ShapeCreator.Count >= 0)
                {
                    _selectionInfo.ShapeIndex = 0;
                    Owner.MarkSceneUIDirty();
                }
        }

        public override void HandleInput(Event guiEvent)
        {
            if (guiEvent.modifiers == EventModifiers.Shift)
            {
                _instrumentMode = InstrumentMode.AddHole;         
            }
            else if (guiEvent.modifiers == EventModifiers.Control)
            {
                _instrumentMode = InstrumentMode.RemovePoints;            
            }
            else
            {
                _instrumentMode = InstrumentMode.AddOrMovePoints;
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                if (Owner.ShapeCreator.Count == 0)
                {
                    CreateNewShape(_localMousePosition);
                    Owner.MarkShapeDirty();
                }
                else
                {
                    if (_instrumentMode == InstrumentMode.AddHole)
                    {
                        CreateNewShape(_localMousePosition);
                        Owner.MarkShapeDirty();
                    }
                    else
                    {
                        
                        if (!(_mouseOverInfo.IsOverLine || _mouseOverInfo.IsOverPoint || _mouseOverInfo.IsOverCross) &&
                            !Owner.ShapeCreator.IsEmpty)
                        {
                            Owner.BreakEditing();
                            Owner.MarkUIDirty();
                            Owner.MarkSceneUIDirty();
                        }
                        else
                        {
                            if (_instrumentMode == InstrumentMode.RemovePoints)
                            {
                                int pointIndex = -1;

                                if (_mouseOverInfo.IsOverPoint)
                                    pointIndex = _mouseOverInfo.PointIndex;
                                if (_mouseOverInfo.IsOverCross)
                                    pointIndex = _mouseOverInfo.CrossIndex;

                                if (pointIndex != -1)
                                {
                                    if (Owner.ShapeCreator[_mouseOverInfo.ShapeIndex].Count <= 3)
                                    {
                                        Undo.RecordObject(Owner.ShapeCreator, "Delete shape");
                                        Owner.ShapeCreator.RemoveAt(_mouseOverInfo.ShapeIndex);
                                        _mouseOverInfo.Reset();

                                        Owner.MarkUIDirty();
                                        Owner.MarkShapeDirty();
                                        Owner.MarkSceneUIDirty();
                                    }
                                    else
                                    {
                                        Undo.RecordObject(Owner.ShapeCreator, "Delete point");
                                        Owner.ShapeCreator[_mouseOverInfo.ShapeIndex].RemoveAt(pointIndex);
                                        _mouseOverInfo.Reset();
                                        _selectionInfo.DeselectPoint();

                                        Owner.MarkSceneUIDirty();
                                        Owner.MarkShapeDirty();
                                    }
                                }
                            }
                            else
                            {
                                if (_mouseOverInfo.IsOverPoint || _mouseOverInfo.IsOverCross)
                                {
                                    int pointIndex = -1;
                                    if (_mouseOverInfo.IsOverPoint)
                                        pointIndex = _mouseOverInfo.PointIndex;
                                    if (_mouseOverInfo.IsOverCross)
                                        pointIndex = _mouseOverInfo.CrossIndex;

                                    _selectionInfo.SelectPointByMouse(_mouseOverInfo.ShapeIndex, pointIndex, Owner.ShapeCreator, guiEvent.mousePosition);
                                }
                                else
                                {
                                    int pointIndex = _mouseOverInfo.LineIndex + 1;
                                    Undo.RecordObject(Owner.ShapeCreator, "Add point");
                                    Owner.ShapeCreator[_mouseOverInfo.ShapeIndex].Insert(pointIndex, new Point(_mouseOverInfo.LocalLineIntersection));

                                    _selectionInfo.SelectPointByMouse(_mouseOverInfo.ShapeIndex, pointIndex, Owner.ShapeCreator, guiEvent.mousePosition);
                                    Owner.MarkShapeDirty();
                                }
                                Owner.MarkSceneUIDirty();
                            }
                        }
                    }
                }

            }


            if ((guiEvent.type == EventType.MouseUp && guiEvent.button == 0))
            {
                if (_selectionInfo.PointIsSelected)
                {
                    _selectionInfo.StopDrag(Owner.ShapeCreator);         
                }

            }

            if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                if (_selectionInfo.PointIsSelected)
                {
                    _selectionInfo.DragPoint(Owner.ShapeCreator, guiEvent.mousePosition);
                    Owner.MarkSceneUIDirty();
                    Owner.MarkShapeDirty();
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
                _localMousePosition = Owner.ShapeCreator.transform.InverseTransformPoint(mousePosition);
                if (_mouseOverInfo.UpdateMouseOverInfo(Owner.ShapeCreator, mousePosition))
                    Owner.MarkSceneUIDirty();
            }
   
        }

        public override void Repaint()
        {
            if (_instrumentMode == InstrumentMode.AddHole)
            {
                DrawInAddHoleMode(Owner.ShapeCreator);
            }
            else if (_instrumentMode == InstrumentMode.AddOrMovePoints)
            {
                DrawInAddOrMoveMode(Owner.ShapeCreator);
            }
            else if (_instrumentMode == InstrumentMode.RemovePoints)
            {
                DrawInRemoveMode(Owner.ShapeCreator);
            }
        }

        List<Point> GetNewShapePointsStartPack(Vector2 lMousePosition)
        {
            List<Point> e = new List<Point>();
            e.Add(new Point(lMousePosition + Vector2.up));
            e.Add(new Point(lMousePosition + (Vector2.down + Vector2.left).normalized));
            e.Add(new Point(lMousePosition + (Vector2.down + Vector2.right).normalized));

            return e;
        }


        void CreateNewShape(Vector3 lMousePosition)
        {
            Undo.RecordObject(Owner.ShapeCreator, "Create shape");
            Owner.ShapeCreator.Add(new Shape());
            _selectionInfo.DeselectPoint();
            _selectionInfo.ShapeIndex = Owner.ShapeCreator.Count - 1;

            var points = GetNewShapePointsStartPack(lMousePosition);
            foreach (var point in points)
                Owner.ShapeCreator[Owner.ShapeCreator.Count - 1].Add(point);    
        }

       
     

        void DrawStartTemplate(ShapeCreator owner)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            var points = GetNewShapePointsStartPack(_localMousePosition);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p0 = owner.transform.TransformPoint(points[i].Position);
                Vector3 p1 = owner.transform.TransformPoint(points[(i + 1) % points.Count].Position);

                Handles.color = Color.red;
                Handles.DrawLine(p0, p1);
                Handles.DrawWireDisc(p0, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p0));
            }
        }


        void DrawContours(ShapeCreator owner)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            for (int sIndex = 0; sIndex < Owner.ShapeCreator.Count; sIndex++)
            {
                Shape shape = Owner.ShapeCreator[sIndex];
            
                var color = (sIndex == _mouseOverInfo.ShapeIndex) ? ShapeEditor.ContoursHighlightedColor : ShapeEditor.ContoursNormalColor;
                for (int i = 0; i < shape.Count; i++)
                {
                    Vector3 worldShapePoint = owner.transform.TransformPoint(shape[i].Position);
                    Vector3 worldShapeNextPoint = owner.transform.TransformPoint(shape[(i + 1) % shape.Count].Position);

                    Handles.color = color;
                    Handles.DrawDottedLine(worldShapePoint, worldShapeNextPoint, 4);
                    Handles.DrawWireDisc(worldShapePoint, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(worldShapePoint));
                }
            }
        }

        void DrawSelection(ShapeCreator owner)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);
          
            if (_selectionInfo.PointIsSelected)
            {         
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[_selectionInfo.ShapeIndex][_selectionInfo.PointIndex].Position);
                Handles.color = Color.blue;
        
                Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));      
            }
        }

        void DrawHighlight(ShapeCreator owner, Color highlightColor)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            if (_mouseOverInfo.IsOverPoint)
            {
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[_mouseOverInfo.ShapeIndex][_mouseOverInfo.PointIndex].Position);
                Handles.color = highlightColor;

                Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));
            }
            if (_mouseOverInfo.IsOverCross)
            {
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[_mouseOverInfo.ShapeIndex][_mouseOverInfo.CrossIndex].Position);
                Handles.color = highlightColor;

                Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));
            }    
        }

        void DrawInAddOrMoveMode(ShapeCreator owner)
        {
            if (Owner.ShapeCreator.Count == 0)
                DrawStartTemplate(owner);

            DrawContours(owner);
            DrawHighlight(owner, Color.blue);
            DrawSelection(owner);

            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            if (!_selectionInfo.DragEnable)
            {
                if (_mouseOverInfo.IsOverLine)
                {
                    Handles.color = Color.red;

                    var intersectionP = owner.transform.TransformPoint((Vector2) _mouseOverInfo.LocalLineIntersection);
                    Handles.DrawWireDisc(intersectionP, worldShapeNormal,
                        ShapeEditor.GetHandelFixedScreenSize(intersectionP));
                    Handles.DrawDottedLine(owner.transform.TransformPoint(_localMousePosition), intersectionP, 4);
                }

                if (_mouseOverInfo.IsOverCross)
                {
                    Handles.color = Color.red;

                    Vector3 cP = owner.transform.TransformPoint(Owner.ShapeCreator[_mouseOverInfo.ShapeIndex][_mouseOverInfo.CrossIndex].Position);
                    Handles.DrawDottedLine(owner.transform.TransformPoint(_localMousePosition), cP, 4);
                }
            }
        }


        void DrawInRemoveMode(ShapeCreator owner)
        {
            DrawContours(owner);
            DrawHighlight(owner, Color.red);

            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);
            
            if (_mouseOverInfo.IsOverPoint || _mouseOverInfo.IsOverCross)
            {
                if (Owner.ShapeCreator[_mouseOverInfo.ShapeIndex].Count <= 3)
                {
                    Shape shape = Owner.ShapeCreator[_mouseOverInfo.ShapeIndex];
                    for (int i = 0; i < shape.Count; i++)
                    {
                        Vector3 p = owner.transform.TransformPoint(shape[i].Position);
                        Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));
                    }
                }
            }
        }

        void DrawInAddHoleMode(ShapeCreator owner)
        {
            DrawStartTemplate(owner);
            DrawContours(owner);
        }


        enum InstrumentMode
        {
            AddOrMovePoints,
            RemovePoints,
            AddHole,
        }
    }
}