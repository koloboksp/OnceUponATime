using UnityEngine;

namespace Assets.ShapeEditor
{
    public static class ExtensionMethods
    {
        public static Vector2 To2Dimensional(this Vector3 v3)
        {
            return new Vector2(v3.x, v3.y);
        }
        public static Vector3 To3Dimensional(this Vector2 v2, float value)
        {  
            return new Vector3(v2.x, v2.y, value);
        }

        public static bool FloatEquals(float l, float r, float tolerance = 0.0001f)
        {
            if (Mathf.Abs(l - r) <= tolerance)
                return true;
            return false;
        }

    }
}
