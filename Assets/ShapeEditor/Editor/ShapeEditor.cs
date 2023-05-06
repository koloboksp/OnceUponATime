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

        private const float EditColliderButtonWidth = 33;
        private const float EditColliderButtonHeight = 23;
        private const float SpaceBetweenLabelAndButton = 5;

        public ShapeCreator ShapeCreator { get; private set; }

        private PointsEditingTool _pointsEditingTool ;
        private PointsDepthEditingTool _pointsDepthEditingTool;

        private EditingTool _activeTool;

        private bool _moveInDepthModeActivated = false;

        private int _mouseBtn0ClickCycle = 0;
        private bool _mouseBtn0Down;

        private bool _shapeChangedSinceLastRepaint;
        private bool _instrumentSceneUIChangedSinceLastRepaint;
        private bool _instrumentUIChangedSinceLastRepaint;

        private bool _editEnable;

        public int MouseBtn0ClickCycle => _mouseBtn0ClickCycle;

        protected virtual GUIContent editModeButton { get { return EditorGUIUtility.IconContent("EditCollider"); } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Rect rect = EditorGUILayout.GetControlRect(true, 23, GUI.skin.button);
            Rect buttonRect = new Rect(rect.xMin + EditorGUIUtility.labelWidth, rect.yMin, EditColliderButtonWidth, EditColliderButtonHeight);
            GUIContent labelContent = new GUIContent("Edit");
            Vector2 labelSize = GUI.skin.label.CalcSize(labelContent);

            Rect labelRect = new Rect(
                buttonRect.xMax + SpaceBetweenLabelAndButton,
                rect.yMin + (rect.height - labelSize.y) * .5f,
                labelSize.x,
                rect.height);

            _editEnable = GUI.Toggle(buttonRect, _editEnable, editModeButton, GUI.skin.button);
            GUI.Label(labelRect, "Edit");
           
            string helpMessage = "'B' start/stop editing.\n" +
                                 "'Left click' to add points.\n" +
                                 "'Ctrl-left click' on point to delete.\n" +
                                 "'Shift-left click' to create new shape.\n" +
                                 "'D' change to depth editing mode." ;

            EditorGUILayout.HelpBox(helpMessage, MessageType.Info);

            if (GUI.changed)
            {
                _shapeChangedSinceLastRepaint = true;
                SceneView.RepaintAll();
            }

            _instrumentUIChangedSinceLastRepaint = false;
        }

        void OnSceneGUI()
        {
            Tools.hidden = _editEnable;     
            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.Repaint)
            {
                if (_editEnable)
                {
                    _activeTool.Repaint(); 
                }

                if (_shapeChangedSinceLastRepaint)
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

              
                _shapeChangedSinceLastRepaint = false;
                _instrumentSceneUIChangedSinceLastRepaint = false;
                
            }
            else if (guiEvent.type == EventType.Layout)
            {
                if (_editEnable)
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
                        _editEnable = !_editEnable;
                        MarkUIDirty();
                    }
                }

                if (_editEnable)
                {
                    if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
                    {
                        _mouseBtn0Down = true;
                    }
                    if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
                    {
                        if (_mouseBtn0Down)
                        {
                            _mouseBtn0Down = false;
                            _mouseBtn0ClickCycle++;
                        }
                    }

                    if (guiEvent.type == EventType.KeyUp)
                    {
                        if (guiEvent.keyCode == KeyCode.D)
                        {
                            _moveInDepthModeActivated = !_moveInDepthModeActivated;

                            if(_moveInDepthModeActivated)
                                ChangeTool(_pointsDepthEditingTool);
                            else
                                ChangeTool(_pointsEditingTool);          
                        }
                    }

                    _activeTool.HandleInput(guiEvent);           
                }

                if (_shapeChangedSinceLastRepaint || _instrumentSceneUIChangedSinceLastRepaint)
                {
                    HandleUtility.Repaint();
                }

                if (_instrumentUIChangedSinceLastRepaint)
                {
                    Repaint();
                }
            }
      
        }

        void ChangeTool(EditingTool newTool)
        {
            if (newTool != _activeTool)
            {
                _activeTool = newTool;
                _activeTool.OnEnable();
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
            _instrumentSceneUIChangedSinceLastRepaint = true;

            ShapeCreator = target as GroundCreator;

            _pointsEditingTool = new PointsEditingTool(this);
            _pointsDepthEditingTool = new PointsDepthEditingTool(this);

            ChangeTool(_pointsEditingTool);
           
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
            _instrumentUIChangedSinceLastRepaint = true;
        }

        public void MarkSceneUIDirty()
        {
            _instrumentSceneUIChangedSinceLastRepaint = true;
        }

        public void MarkShapeDirty()
        {
            _shapeChangedSinceLastRepaint = true;
        }

        public void BreakEditing()
        {
            _editEnable = false;
            Tools.hidden = false;
            MarkUIDirty();
        }
    }
}
