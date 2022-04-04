//nik 24.04.2020

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

#pragma warning disable 169
#pragma warning disable 414

public class ToolMisc : EditorWindow
{
    private Vector3 localMove;
    private Vector3 localRotate;
    private bool showAxis;

    private Transform[] sel = new Transform[0];

    [MenuItem("Tools/Nik/Misc")]
    static public void OpenEditor()
    {
        GetWindow<ToolMisc>(false, "Misc", true);
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        localMove = EditorGUILayout.Vector3Field("", localMove);
        if (GUILayout.Button("Local Move")) LocalMove(localMove);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Show/Hide Axis")) showAxis = !showAxis;

        GUILayout.Label("Align. Select Object To Move");
        GUILayout.BeginHorizontal();
        if (sel.Length == 2)
        {
            if (GUILayout.Button(sel[0].name)) Align(0);
            if (GUILayout.Button(sel[1].name)) Align(1);
        }
        else
        {
            GUILayout.Button("Select two objects");
            GUILayout.Button("Select two objects");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Freeze Transform")) FreezeTransform();
    }

    private void LocalMove(Vector3 localMove)
    {
        foreach (var trans in Selection.transforms)
        {
            trans.position +=
                trans.right * localMove.x +
                trans.up * localMove.y +
                trans.forward * localMove.z;
        }
    }

    private void LocalRotate(Vector3 localRotate)
    {

    }

    private void Align(int id)
    {
        if (sel.Length == 2)
        {
            Undo.RecordObject(sel[id], "Align Apply");
            var t0 = sel[id];
            var t1 = sel[1 - id];
            t0.position = t1.position;
            t0.rotation = t1.rotation;
        }
    }

    private void FreezeTransform()
    {
        for (int i = 0; i < sel.Length; i++)
        {
            var rootObj = new GameObject(sel[i].name + "_root");
            var t = rootObj.transform;
            t.SetParent(sel[i].parent);
            t.position = sel[i].position;
            t.rotation = sel[i].rotation;
            t.localScale = sel[i].localScale;
            sel[i].SetParent(t);
        }
    }

    private void OnInspectorUpdate()
    {
        if (sel.Length == 0) return;
        if (showAxis)
        {
            for (int i = 0; i < sel.Length; i++)
            {
                Debug.DrawLine(sel[i].position, sel[i].position + sel[i].right, Color.red);
                Debug.DrawLine(sel[i].position, sel[i].position + sel[i].up, Color.green);
                Debug.DrawLine(sel[i].position, sel[i].position + sel[i].forward, Color.blue);
            }
        }
    }

    private void OnSelectionChange()
    {
        sel = Selection.transforms;
        //Debug.Log("Sel Count: " + sel.Length);
    }
}

#endif