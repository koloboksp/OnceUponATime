using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class CeilingAndWallsMeshGenerator
    {
        private readonly float _depth;

        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();


        public List<int> Triangles => _triangles;
        public List<Vector3> Vertices => _vertices;
        public List<Vector3> Normals => _normals;
        public List<Vector2> UVs => _uvs;

        public CeilingAndWallsMeshGenerator(List<Point> hullPoints, List<List<int>> edgesPointsIndexes, float depth)
        {
            _depth = depth;
         
            var walkableEdgesPoints = edgesPointsIndexes.Select(epi => epi.Select(pi => hullPoints[pi]).ToList()).ToList();
            GenerateMesh(walkableEdgesPoints);
        }

        private void GenerateMesh(List<List<Point>> edgesPoints)
        {
            var halfCommon = _depth;
            var common = halfCommon * 2.0f;
            
            int verticesOffset = 0;
            for (int index = 0; index < edgesPoints.Count; index++)
            {
                List<Point> edgePoints = edgesPoints[index];

                List<float> angles = new List<float>();
                for (int i = 0; i < edgePoints.Count - 1; i++)
                {
                    var dot = Mathf.Abs(Vector2.Dot((edgePoints[i + 1].Position - edgePoints[i].Position).normalized, Vector2.up));
                    angles.Add(dot);
                }
                angles.Add(Mathf.Abs(Vector2.Dot((edgePoints[edgePoints.Count -1].Position - edgePoints[edgePoints.Count - 2].Position).normalized, Vector2.up)));
                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    _vertices.Add(edgePoint.GetPosition(-_depth, Side.Front));
                    _vertices.Add(edgePoint.GetPosition(_depth, Side.Back));
                }
                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];

                    _uvs.Add(new Vector2(edgePoint.Position.x / common, edgePoint.Position.y / common));
                    _uvs.Add(
                        Vector2.Lerp(
                            new Vector2(edgePoint.Position.x / common, edgePoint.Position.y / common - 1), 
                            new Vector2(edgePoint.Position.x / common - 1, edgePoint.Position.y / common),
                            angles[pI]));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    _triangles.Add(verticesOffset + vIndex);
                    _triangles.Add(verticesOffset + vIndex + 3);
                    _triangles.Add(verticesOffset + vIndex + 1);

                    _triangles.Add(verticesOffset + vIndex);
                    _triangles.Add(verticesOffset + vIndex + 2);
                    _triangles.Add(verticesOffset + vIndex + 3);
                }
                
                verticesOffset = _vertices.Count;
            }
        }
    }
}