using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class FrontWallsMeshGenerator
    {
        private List<int> mTriangles = new List<int>();
        private List<Vector3> mVertices = new List<Vector3>();
        private List<Vector3> mNormals = new List<Vector3>();
        private List<Vector2> mUVs = new List<Vector2>();

        public List<int> Triangles => mTriangles;
        public List<Vector3> Vertices => mVertices;
        public List<Vector3> Normals => mNormals;
        public List<Vector2> UVs => mUVs;

        public FrontWallsMeshGenerator(Polygon polygon, float depth)
        {
            var halfCommon = depth;
            var common = halfCommon * 2.0f;

            mVertices.AddRange(polygon.points.Select(p => p.GetPosition(-depth, Side.Front)).ToList());
            mUVs.AddRange(mVertices.Select(v=>new Vector2(v.x / common, v.y / common)));
            
            Geometry.Triangulator triangulator = new Geometry.Triangulator(polygon);
            int[] polygonTriangles = triangulator.Triangulate();
            
            if (polygonTriangles != null)
            {
                for (int j = 0; j < polygonTriangles.Length; j++)
                    mTriangles.Add(polygonTriangles[j]);    
            }         
        } 
    }
}