using UnityEngine;

namespace Assets.Scripts.Core
{
    public static class UnityExtension
    {
        public static Vector2 Project(Vector2 vector, Vector2 onNormal)
        {
            float num = Vector2.Dot(onNormal, onNormal);
            if ((double)num < (double)Mathf.Epsilon)
                return Vector2.zero;
            return onNormal * Vector2.Dot(vector, onNormal) / num;
        }
    }
}