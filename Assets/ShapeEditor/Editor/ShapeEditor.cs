using System;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    [CustomEditor(typeof(GroundCreator), true)]
    public class ShapeEditor : UnityEditor.Editor
    {
        public static Color ContoursHighlightedColor = Color.gray + new Color(0.2f, 0.2f, 0.2f, 0);
        public static Color ContoursNormalColor = Color.gray;


        public const float HandleRadius = 10.0f;

        private const float k_EditColliderButtonWidth = 33;
        private const float k_EditColliderButtonHeight = 23;
        private const float k_SpaceBetweenLabelAndButton = 5;

        public ShapeCreator ShapeCreator { get; private set; }

        PointsEditingTool mPointsEditingTool ;
        PointsDepthEditingTool mPointsDepthEditingTool;

        EditingTool mActiveTool;

        bool mMoveInDepthModeActivated = false;

        int mMouseBtn0ClickCycle = 0;
        bool mMouseBtn0Down;

        bool shapeChangedSinceLastRepaint;
        bool instrumentSceneUIChangedSinceLastRepaint;
        bool instrumentUIChangedSinceLastRepaint;

        bool mEditEnable;

        public int MouseBtn0ClickCycle => mMouseBtn0ClickCycle;

        protected virtual GUIContent editModeButton { get { return EditorGUIUtility.IconContent("EditCollider"); } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Rect rect = EditorGUILayout.GetControlRect(true, 23, GUI.skin.button);
            Rect buttonRect = new Rect(rect.xMin + EditorGUIUtility.labelWidth, rect.yMin, k_EditColliderButtonWidth, k_EditColliderButtonHeight);
            GUIContent labelContent = new GUIContent("Edit");
            Vector2 labelSize = GUI.skin.label.CalcSize(labelContent);

            Rect labelRect = new Rect(
                buttonRect.xMax + k_SpaceBetweenLabelAndButton,
                rect.yMin + (rect.height - labelSize.y) * .5f,
                labelSize.x,
                rect.height);

            mEditEnable = GUI.Toggle(buttonRect, mEditEnable, editModeButton, GUI.skin.button);
            GUI.Label(labelRect, "Edit");
           
            string helpMessage = "'B' start/stop editing.\n" +
                                 "'Left click' to add points.\n" +
                                 "'Ctrl-left click' on point to delete.\n" +
                                 "'Shift-left click' to create new shape.\n" +
                                 "'D' change to depth editing mode." ;

            EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

            if (GUI.changed)
            {
                shapeChangedSinceLastRepaint = true;
                SceneView.RepaintAll();
            }

            instrumentUIChangedSinceLastRepaint = false;
        }

        void OnSceneGUI()
        {
            Tools.hidden = mEditEnable;     
            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.Repaint)
            {
                if (mEditEnable)
                {
                    mActiveTool.Repaint(); 
                }

                if (shapeChangedSinceLastRepaint)
                {
                    try
                    {
                        ShapeCreator.UpdateMeshDisplay();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

              
                shapeChangedSinceLastRepaint = false;
                instrumentSceneUIChangedSinceLastRepaint = false;
                
            }
            else if (guiEvent.type == EventType.Layout)
            {
                if (mEditEnable)
                {   
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));           
                }
            }     
            else
            {

                if (guiEvent.type == EventType.KeyUp)
                {
                    if (guiEvent.keyCode == KeyCode.B)
                    {
                        mEditEnable = !mEditEnable;
                        MarkUIDirty();
                    }
                }

                if (mEditEnable)
                {
                    if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
                    {
                        mMouseBtn0Down = true;
                    }
                    if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
                    {
                        if (mMouseBtn0Down)
                        {
                            mMouseBtn0Down = false;
                            mMouseBtn0ClickCycle++;
                        }
                    }

                    if (guiEvent.type == EventType.KeyUp)
                    {
                        if (guiEvent.keyCode == KeyCode.D)
                        {
                            mMoveInDepthModeActivated = !mMoveInDepthModeActivated;

                            if(mMoveInDepthModeActivated)
                                ChangeTool(mPointsDepthEditingTool);
                            else
                                ChangeTool(mPointsEditingTool);          
                        }
                    }

                    mActiveTool.HandleInput(guiEvent);           
                }

                if (shapeChangedSinceLastRepaint || instrumentSceneUIChangedSinceLastRepaint)
                {
                    HandleUtility.Repaint();
                }

                if (instrumentUIChangedSinceLastRepaint)
                {
                    Repaint();
                }
            }
      
        }

        void ChangeTool(EditingTool newTool)
        {
            if (newTool != mActiveTool)
            {
                mActiveTool = newTool;
                mActiveTool.OnEnable();
                MarkSceneUIDirty(); 
            }
            
        }
     
        public static float GetHandelFixedScreenSize(Vector3 position, float sizeOffset = 0)
        {
            if (Camera.current.orthographic)
                return (HandleRadius + sizeOffset) * Camera.current.orthographicSize * 2.0f / Screen.height;
          
            return ((HandleRadius + sizeOffset) * Camera.current.WorldToScreenPoint(position).z * Camera.current.fieldOfView) / (Mathf.Rad2Deg * Screen.height);
        }

        void OnEnable()
        {
            instrumentSceneUIChangedSinceLastRepaint = true;

            ShapeCreator = target as GroundCreator;

            mPointsEditingTool = new PointsEditingTool(this);
            mPointsDepthEditingTool = new PointsDepthEditingTool(this);

            ChangeTool(mPointsEditingTool);
           
            Undo.undoRedoPerformed += OnUndoOrRedo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoOrRedo;    
        }

        void OnUndoOrRedo()
        {
            MarkShapeDirty();
        }

        public void MarkUIDirty()
        {
            instrumentUIChangedSinceLastRepaint = true;
        }

        public void MarkSceneUIDirty()
        {
            instrumentSceneUIChangedSinceLastRepaint = true;
        }

        public void MarkShapeDirty()
        {
            shapeChangedSinceLastRepaint = true;
        }

        public void BreakEditing()
        {
            mEditEnable = false;
            Tools.hidden = false;
            MarkUIDirty();
        }
    }
}
