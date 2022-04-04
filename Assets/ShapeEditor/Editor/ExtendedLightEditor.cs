using Assets.Scripts.Effects;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    [CustomEditor(typeof(ExtendedLight), true)]
    public class ExtendedLightEditor : UnityEditor.Editor
    {
        ExtendedLight mTarget;

        SerializedProperty mRangeProperty;
        SerializedProperty mColorProperty;

        void OnEnable()
        {
            mTarget = target as ExtendedLight;

            mRangeProperty = serializedObject.FindProperty("mRange");
            mColorProperty = serializedObject.FindProperty("mColor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(mRangeProperty, new GUIContent("Range"));

            if (GUI.changed)
            {
                mTarget.UpdateTargets();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.PropertyField(mColorProperty, new GUIContent("Color"));

            if (GUI.changed)
            {
                mTarget.UpdateTargets();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}