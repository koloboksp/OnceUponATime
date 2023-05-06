using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    public class VertexSnapping : EditorWindow
    {
        private static readonly FieldInfo FreeMode;
        private static readonly FieldInfo Dragging;
        private static readonly FieldInfo Offset;

        static VertexSnapping()
        {
            FreeMode = GetField("s_FreeMoveMode", typeof(Handles));
            Dragging = GetField("vertexDragging", typeof(Tools));
            Offset = GetField("handleOffset", typeof(Tools));
        }

        [MenuItem("Edit/Vertex Snapping")]
        public static void Snapping()
        {
            var b = (bool)FreeMode.GetValue(null);
            Dragging.SetValue(null, !b);
            FreeMode.SetValue(null, !b);
            Offset.SetValue(null, Vector3.zero);
            SceneView.RepaintAll();
        }

        private static FieldInfo GetField(string name, System.Type type)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}