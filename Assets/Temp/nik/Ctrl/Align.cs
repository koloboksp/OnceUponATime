using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Align : MonoBehaviour
{
    //public void Align
    
}

#if UNITY_EDITOR
[CustomEditor(typeof(Align))]
public class AlignEditor : Editor
{
    private Align t;

    private void OnEnable()
    {
        t = (Align)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //if (GUILayout.Button("Align")) t.
    }
}
#endif

