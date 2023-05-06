using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class WalkableMeshGenerator
    {
        private readonly float _horizontalOffsetOfExtremePoints;
        private readonly float _thickness;
        private readonly float _depth;
        
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        
        public List<int> Triangles => _triangles;
        public List<Vector3> Vertices => _vertices;
        public List<Vector3> Normals => _normals;
        public List<Vector2> UVs => _uvs;
       
        public WalkableMeshGenerator(List<Point> hullPoints, List<List<int>> edgesPointsIndexes, float depth, float thickness,  float horizontalOffsetOfExtremePoints)
        {
            _depth = depth;
            _thickness = thickness;
            _horizontalOffsetOfExtremePoints = horizontalOffsetOfExtremePoints;
 
            var walkableEdgesPoints = edgesPointsIndexes.Select(epi => epi.Select(pi => hullPoints[pi]).ToList()).ToList();
            ApplyHorizontalOffsetToExtremeEdgePoints(walkableEdgesPoints);
            var generatedBottomWalkableEdgesPoints = GenerateBottomPoints(walkableEdgesPoints);
            GenerateMesh(walkableEdgesPoints, generatedBottomWalkableEdgesPoints);     
        }

        private void ApplyHorizontalOffsetToExtremeEdgePoints(List<List<Point>> edgesPoints)
        {
            foreach (List<Point> edgePoints in edgesPoints)
            {
                var startEdgeDirection = (edgePoints[0].Position - edgePoints[1].Position).normalized;
                var startPoint = edgePoints[0];
                startPoint.Position = edgePoints[0].Position + startEdgeDirection * _horizontalOffsetOfExtremePoints;
                edgePoints[0] = startPoint;

                var endEdgeDirection = (edgePoints[edgePoints.Count - 2].Position - edgePoints[edgePoints.Count - 1].Position).normalized;
                var endPoint = edgePoints[edgePoints.Count - 1];
                endPoint.Position = edgePoints[edgePoints.Count - 1].Position - endEdgeDirection * _horizontalOffsetOfExtremePoints;
                edgePoints[edgePoints.Count - 1] = endPoint;
            }
        }

        private List<List<Point>> GenerateBottomPoints(List<List<Point>> edgesPoints)
        {
            List<List<Point>> bottomEdgesPoints = new List<List<Point>>();

            foreach (List<Point> edgePoints in edgesPoints)
            {
                List<Point> bottomEdgePoints = new List<Point>();

                var newStartPoint = new Point(edgePoints[0])
                {
                    Position = edgePoints[0].Position + Vector2.down * _thickness
                };
                bottomEdgePoints.Add(newStartPoint);

                for (var pIndex = 1; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    var point = new Point(edgePoints[pIndex]);
                    var previousEdgeDir = (edgePoints[pIndex - 1].Position - point.Position).normalized;
                    var nextEdgeDir = (point.Position - edgePoints[pIndex + 1].Position).normalized;

                    var averagePointDir = (previousEdgeDir + nextEdgeDir).normalized;
                    var bottomOffset = -Vector2.Perpendicular(averagePointDir);

                    var newPoint = new Point(edgePoints[pIndex])
                    {
                        Position = point.Position + bottomOffset * _thickness
                    };
                    bottomEdgePoints.Add(newPoint);
                }

                var newEndPoint = new Point(edgePoints[edgePoints.Count - 1])
                {
                    Position = edgePoints[edgePoints.Count - 1].Position + Vector2.down * _thickness
                };
                bottomEdgePoints.Add(newEndPoint);

                bottomEdgesPoints.Add(bottomEdgePoints);
            }

            return bottomEdgesPoints;
        }

        private void GenerateMesh(List<List<Point>> edgesPoints, List<List<Point>> bottomEdgesPoints)
        {       
            var halfCommon = (_depth + _thickness);
            var common = halfCommon * 2.0f;
            var uvThicknessToHalfCommon = _thickness / halfCommon;
          
            int verticesOffset = 0;
            for (int index = 0; index < edgesPoints.Count; index++)
            {
                List<Point> edgePoints = edgesPoints[index];
                List<Point> bottomEdgePoints = bottomEdgesPoints[index];

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    _vertices.Add(edgePoint.GetPosition(-_depth, Side.Front));
                    _vertices.Add(edgePoint.GetPosition(_depth, Side.Back));
                }
                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, uvThicknessToHalfCommon));
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, 1 - uvThicknessToHalfCommon));
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

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    var bottomEdgePoint = bottomEdgePoints[pI];
                    _vertices.Add(edgePoint.GetPosition(-_depth, Side.Front));
                    _vertices.Add(bottomEdgePoint.GetPosition(-_depth, Side.Front));
                }

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, uvThicknessToHalfCommon));
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, 0));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    _triangles.Add(verticesOffset + vIndex + 1);
                    _triangles.Add(verticesOffset + vIndex + 2);
                    _triangles.Add(verticesOffset + vIndex);

                    _triangles.Add(verticesOffset + vIndex + 1);
                    _triangles.Add(verticesOffset + vIndex + 3);
                    _triangles.Add(verticesOffset + vIndex + 2);
                }

                verticesOffset = _vertices.Count;

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var bottomEdgePoint = bottomEdgePoints[pI];
                    _vertices.Add(bottomEdgePoint.GetPosition(-_depth, Side.Front));
                    _vertices.Add(bottomEdgePoint.GetPosition(_depth, Side.Back));
                }

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, 1));
                    _uvs.Add(new Vector2(edgePoint.Position.x / common, 0));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    _triangles.Add(verticesOffset + vIndex + 1);
                    _triangles.Add(verticesOffset + vIndex + 2);
                    _triangles.Add(verticesOffset + vIndex);

                    _triangles.Add(verticesOffset + vIndex + 1);
                    _triangles.Add(verticesOffset + vIndex + 3);
                    _triangles.Add(verticesOffset + vIndex + 2);
                }

                verticesOffset = _vertices.Count;

                _vertices.Add(edgePoints[0].GetPosition(-_depth, Side.Front));
                _vertices.Add(edgePoints[0].GetPosition(_depth, Side.Back));
                _vertices.Add(bottomEdgePoints[0].GetPosition(-_depth, Side.Front));
                _vertices.Add(bottomEdgePoints[0].GetPosition(_depth, Side.Back));

                _uvs.Add(new Vector2(0 + uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                _uvs.Add(new Vector2(1 - uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                _uvs.Add(new Vector2(0 + uvThicknessToHalfCommon, 0));
                _uvs.Add(new Vector2(1 - uvThicknessToHalfCommon, 0));

                _triangles.Add(verticesOffset + 0 + 3);
                _triangles.Add(verticesOffset + 0 + 0);
                _triangles.Add(verticesOffset + 0 + 1);

                _triangles.Add(verticesOffset + 0 + 3);
                _triangles.Add(verticesOffset + 0 + 2);
                _triangles.Add(verticesOffset + 0 + 0);

                verticesOffset = _vertices.Count;


                _vertices.Add(edgePoints[edgePoints.Count - 1].GetPosition(-_depth, Side.Front));
                _vertices.Add(edgePoints[edgePoints.Count - 1].GetPosition(_depth, Side.Back));
                _vertices.Add(bottomEdgePoints[bottomEdgePoints.Count - 1].GetPosition(-_depth, Side.Front));
                _vertices.Add(bottomEdgePoints[bottomEdgePoints.Count - 1].GetPosition(_depth, Side.Back));

                _uvs.Add(new Vector2(0 + uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                _uvs.Add(new Vector2(1 - uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                _uvs.Add(new Vector2(0 + uvThicknessToHalfCommon, 0));
                _uvs.Add(new Vector2(1 - uvThicknessToHalfCommon, 0));

                _triangles.Add(verticesOffset + 0 + 1);
                _triangles.Add(verticesOffset + 0 + 0);
                _triangles.Add(verticesOffset + 0 + 3);

                _triangles.Add(verticesOffset + 0 + 0);
                _triangles.Add(verticesOffset + 0 + 2);
                _triangles.Add(verticesOffset + 0 + 3);

                verticesOffset = _vertices.Count;
            }
        }
    }
}