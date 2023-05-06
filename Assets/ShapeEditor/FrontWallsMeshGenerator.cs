using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class FrontWallsMeshGenerator
    {
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();

        public List<int> Triangles => _triangles;
        public List<Vector3> Vertices => _vertices;
        public List<Vector3> Normals => _normals;
        public List<Vector2> UVs => _uvs;

        public FrontWallsMeshGenerator(Polygon polygon, float depth)
        {
            var halfCommon = depth;
            var common = halfCommon * 2.0f;

            _vertices.AddRange(polygon.Points.Select(p => p.GetPosition(-depth, Side.Front)).ToList());
            _uvs.AddRange(_vertices.Select(v=>new Vector2(v.x / common, v.y / common)));
            
            Geometry.Triangulator triangulator = new Geometry.Triangulator(polygon);
            int[] polygonTriangles = triangulator.Triangulate();
            
            if (polygonTriangles != null)
            {
                for (int j = 0; j < polygonTriangles.Length; j++)
                    _triangles.Add(polygonTriangles[j]);    
            }         
        } 
    }
}