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

        readonly SelectionInfo mSelectionInfo = new SelectionInfo();
        readonly MouseOverInfo mMouseOverInfo = new MouseOverInfo();

        InstrumentMode mInstrumentMode = InstrumentMode.AddOrMovePoints;

        Vector3 mLocalMousePosition;


        public PointsEditingTool(ShapeEditor owner) : base(owner)
        {
           
        }

        public override void OnEnable()
        {
            base.OnEnable();

            if(mSelectionInfo.mShapeIndex == -1)
                if (Owner.ShapeCreator.Count >= 0)
                {
                    mSelectionInfo.mShapeIndex = 0;
                    Owner.MarkSceneUIDirty();
                }
        }

        public override void HandleInput(Event guiEvent)
        {
            if (guiEvent.modifiers == EventModifiers.Shift)
            {
                mInstrumentMode = InstrumentMode.AddHole;         
            }
            else if (guiEvent.modifiers == EventModifiers.Control)
            {
                mInstrumentMode = InstrumentMode.RemovePoints;            
            }
            else
            {
                mInstrumentMode = InstrumentMode.AddOrMovePoints;
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
            {
                if (Owner.ShapeCreator.Count == 0)
                {
                    CreateNewShape(mLocalMousePosition);
                    Owner.MarkShapeDirty();
                }
                else
                {
                    if (mInstrumentMode == InstrumentMode.AddHole)
                    {
                        CreateNewShape(mLocalMousePosition);
                        Owner.MarkShapeDirty();
                    }
                    else
                    {
                        
                        if (!(mMouseOverInfo.IsOverLine || mMouseOverInfo.IsOverPoint || mMouseOverInfo.IsOverCross) &&
                            !Owner.ShapeCreator.IsEmpty)
                        {
                            Owner.BreakEditing();
                            Owner.MarkUIDirty();
                            Owner.MarkSceneUIDirty();
                        }
                        else
                        {
                            if (mInstrumentMode == InstrumentMode.RemovePoints)
                            {
                                int pointIndex = -1;

                                if (mMouseOverInfo.IsOverPoint)
                                    pointIndex = mMouseOverInfo.PointIndex;
                                if (mMouseOverInfo.IsOverCross)
                                    pointIndex = mMouseOverInfo.CrossIndex;

                                if (pointIndex != -1)
                                {
                                    if (Owner.ShapeCreator[mMouseOverInfo.ShapeIndex].Count <= 3)
                                    {
                                        Undo.RecordObject(Owner.ShapeCreator, "Delete shape");
                                        Owner.ShapeCreator.RemoveAt(mMouseOverInfo.ShapeIndex);
                                        mMouseOverInfo.Reset();

                                        Owner.MarkUIDirty();
                                        Owner.MarkShapeDirty();
                                        Owner.MarkSceneUIDirty();
                                    }
                                    else
                                    {
                                        Undo.RecordObject(Owner.ShapeCreator, "Delete point");
                                        Owner.ShapeCreator[mMouseOverInfo.ShapeIndex].RemoveAt(pointIndex);
                                        mMouseOverInfo.Reset();
                                        mSelectionInfo.DeselectPoint();

                                        Owner.MarkSceneUIDirty();
                                        Owner.MarkShapeDirty();
                                    }
                                }
                            }
                            else
                            {
                                if (mMouseOverInfo.IsOverPoint || mMouseOverInfo.IsOverCross)
                                {
                                    int pointIndex = -1;
                                    if (mMouseOverInfo.IsOverPoint)
                                        pointIndex = mMouseOverInfo.PointIndex;
                                    if (mMouseOverInfo.IsOverCross)
                                        pointIndex = mMouseOverInfo.CrossIndex;

                                    mSelectionInfo.SelectPointByMouse(mMouseOverInfo.ShapeIndex, pointIndex, Owner.ShapeCreator, guiEvent.mousePosition);
                                }
                                else
                                {
                                    int pointIndex = mMouseOverInfo.LineIndex + 1;
                                    Undo.RecordObject(Owner.ShapeCreator, "Add point");
                                    Owner.ShapeCreator[mMouseOverInfo.ShapeIndex].Insert(pointIndex, new Point(mMouseOverInfo.LocalLineIntersection));

                                    mSelectionInfo.SelectPointByMouse(mMouseOverInfo.ShapeIndex, pointIndex, Owner.ShapeCreator, guiEvent.mousePosition);
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
                if (mSelectionInfo.PointIsSelected)
                {
                    mSelectionInfo.StopDrag(Owner.ShapeCreator);         
                }

            }

            if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                if (mSelectionInfo.PointIsSelected)
                {
                    mSelectionInfo.DragPoint(Owner.ShapeCreator, guiEvent.mousePosition);
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
                mLocalMousePosition = Owner.ShapeCreator.transform.InverseTransformPoint(mousePosition);
                if (mMouseOverInfo.UpdateMouseOverInfo(Owner.ShapeCreator, mousePosition))
                    Owner.MarkSceneUIDirty();
            }
   
        }

        public override void Repaint()
        {
            if (mInstrumentMode == InstrumentMode.AddHole)
            {
                DrawInAddHoleMode(Owner.ShapeCreator);
            }
            else if (mInstrumentMode == InstrumentMode.AddOrMovePoints)
            {
                DrawInAddOrMoveMode(Owner.ShapeCreator);
            }
            else if (mInstrumentMode == InstrumentMode.RemovePoints)
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
            mSelectionInfo.DeselectPoint();
            mSelectionInfo.mShapeIndex = Owner.ShapeCreator.Count - 1;

            var points = GetNewShapePointsStartPack(lMousePosition);
            foreach (var point in points)
                Owner.ShapeCreator[Owner.ShapeCreator.Count - 1].Add(point);    
        }

       
     

        void DrawStartTemplate(ShapeCreator owner)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            var points = GetNewShapePointsStartPack(mLocalMousePosition);

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
            
                var color = (sIndex == mMouseOverInfo.ShapeIndex) ? ShapeEditor.ContoursHighlightedColor : ShapeEditor.ContoursNormalColor;
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
          
            if (mSelectionInfo.PointIsSelected)
            {         
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[mSelectionInfo.ShapeIndex][mSelectionInfo.PointIndex].Position);
                Handles.color = Color.blue;
        
                Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));      
            }
        }

        void DrawHighlight(ShapeCreator owner, Color highlightColor)
        {
            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);

            if (mMouseOverInfo.IsOverPoint)
            {
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[mMouseOverInfo.ShapeIndex][mMouseOverInfo.PointIndex].Position);
                Handles.color = highlightColor;

                Handles.DrawWireDisc(p, worldShapeNormal, ShapeEditor.GetHandelFixedScreenSize(p, 1));
            }
            if (mMouseOverInfo.IsOverCross)
            {
                Vector3 p = owner.transform.TransformPoint((Vector2)Owner.ShapeCreator[mMouseOverInfo.ShapeIndex][mMouseOverInfo.CrossIndex].Position);
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

            if (!mSelectionInfo.DragEnable)
            {
                if (mMouseOverInfo.IsOverLine)
                {
                    Handles.color = Color.red;

                    var intersectionP = owner.transform.TransformPoint((Vector2) mMouseOverInfo.LocalLineIntersection);
                    Handles.DrawWireDisc(intersectionP, worldShapeNormal,
                        ShapeEditor.GetHandelFixedScreenSize(intersectionP));
                    Handles.DrawDottedLine(owner.transform.TransformPoint(mLocalMousePosition), intersectionP, 4);
                }

                if (mMouseOverInfo.IsOverCross)
                {
                    Handles.color = Color.red;

                    Vector3 cP = owner.transform.TransformPoint(Owner.ShapeCreator[mMouseOverInfo.ShapeIndex][mMouseOverInfo.CrossIndex].Position);
                    Handles.DrawDottedLine(owner.transform.TransformPoint(mLocalMousePosition), cP, 4);
                }
            }
        }


        void DrawInRemoveMode(ShapeCreator owner)
        {
            DrawContours(owner);
            DrawHighlight(owner, Color.red);

            var worldShapeNormal = owner.transform.TransformDirection(-Camera.current.transform.forward);
            
            if (mMouseOverInfo.IsOverPoint || mMouseOverInfo.IsOverCross)
            {
                if (Owner.ShapeCreator[mMouseOverInfo.ShapeIndex].Count <= 3)
                {
                    Shape shape = Owner.ShapeCreator[mMouseOverInfo.ShapeIndex];
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