using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.ShapeEditor.Editor
{
    public class GeneratedMeshToAsset : EditorWindow
    {
        [MenuItem("Edit/Shape mesh to asset.")]
        public static void Snapping()
        {
            var shapeInstance = Selection.objects.OfType<GameObject>().Where(i => i.GetComponent<ShapeCreator>() != null).Select(i => i.GetComponent<ShapeCreator>()).FirstOrDefault();

            if (shapeInstance != null)
            {
                var shapeMeshFilter = shapeInstance.gameObject.GetComponent<MeshFilter>();
                if (shapeMeshFilter != null)
                {
                    if (shapeMeshFilter.sharedMesh != null)
                    {
                        AssetDatabase.CreateAsset(Instantiate(shapeMeshFilter.sharedMesh), $"Assets/{shapeInstance.gameObject.name}.mesh");
                        AssetDatabase.SaveAssets();
                        var loadAssetAtPath = AssetDatabase.LoadAssetAtPath<Mesh>($"Assets/{shapeInstance.gameObject.name}.mesh");
                         
                        var shapePrefab = PrefabUtility.SaveAsPrefabAsset(shapeInstance.gameObject, $"Assets/{shapeInstance.gameObject.name}.prefab");
                        var shapePrefabMeshFilter = shapePrefab.GetComponent<MeshFilter>();
                        shapePrefabMeshFilter.sharedMesh = loadAssetAtPath;       
                    }
                }
            }
        }  
    }
}