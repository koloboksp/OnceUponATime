using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    public class VertexSnapping : EditorWindow
    {
        private static FieldInfo freeMode, dragging, offset;
        static VertexSnapping()
        {
            freeMode = GetField("s_FreeMoveMode", typeof(Handles));
            dragging = GetField("vertexDragging", typeof(Tools));
            offset = GetField("handleOffset", typeof(Tools));
        }

        [MenuItem("Edit/Vertex Snapping")]
        public static void Snapping()
        {
            var b = (bool)freeMode.GetValue(null);
            dragging.SetValue(null, !b);
            freeMode.SetValue(null, !b);
            offset.SetValue(null, Vector3.zero);
            SceneView.RepaintAll();
        }

        private static FieldInfo GetField(string name, System.Type type)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}