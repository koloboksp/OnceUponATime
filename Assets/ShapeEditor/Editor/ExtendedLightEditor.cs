using Assets.Scripts.Effects;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    [CustomEditor(typeof(ExtendedLight), true)]
    public class ExtendedLightEditor : UnityEditor.Editor
    {
        ExtendedLight _target;

        SerializedProperty _rangeProperty;
        SerializedProperty _colorProperty;

        void OnEnable()
        {
            _target = target as ExtendedLight;

            _rangeProperty = serializedObject.FindProperty("Range");
            _colorProperty = serializedObject.FindProperty("Color");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(_rangeProperty, new GUIContent("Range"));

            if (GUI.changed)
            {
                _target.UpdateTargets();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.PropertyField(_colorProperty, new GUIContent("Color"));

            if (GUI.changed)
            {
                _target.UpdateTargets();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}