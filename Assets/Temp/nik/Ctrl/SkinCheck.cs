using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinCheck : MonoBehaviour
{
    public SkinnedMeshRenderer skin;
    private Transform[] bones;

    public float axisLength = 1f;

    public bool showGizmos;

    private void Awake()
    {
        bones = skin.bones;
    }


    private void Start()
    {
        Debug.Log(skin.bones.Length);
        

    }

    private void Update()
    {
        //show bones
       
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        //show bones
        if (bones == null) bones = new Transform[skin.bones.Length];
        bones = skin.bones;

        Vector3 pos;
       

        for (int i = 0; i < bones.Length; i++)
        {
            pos = bones[i].position;

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(pos, bones[i].parent.position);

            //show axis
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + bones[i].forward * axisLength);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, pos + bones[i].up  * axisLength);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + bones[i].right * axisLength);


            //Gizmos.DrawCube(bones[i].position, Vector3.one * .02f);
        }
    }
}
