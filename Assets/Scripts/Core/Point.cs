using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Point : MonoBehaviour, IPoint
    {
        public Vector3 Position
        {
            get { return transform.position; }
        }
    }
}