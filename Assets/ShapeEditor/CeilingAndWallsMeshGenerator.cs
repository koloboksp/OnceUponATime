using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class CeilingAndWallsMeshGenerator
    {
        private float mDepth;

        private List<int> mTriangles = new List<int>();
        private List<Vector3> mVertices = new List<Vector3>();
        private List<Vector3> mNormals = new List<Vector3>();
        private List<Vector2> mUVs = new List<Vector2>();


        public List<int> Triangles => mTriangles;
        public List<Vector3> Vertices => mVertices;
        public List<Vector3> Normals => mNormals;
        public List<Vector2> UVs => mUVs;

        public CeilingAndWallsMeshGenerator(List<Point> hullPoints, List<List<int>> edgesPointsIndexes, float depth)
        {
            mDepth = depth;
         
            var walkableEdgesPoints = edgesPointsIndexes.Select(epi => epi.Select(pi => hullPoints[pi]).ToList()).ToList();
            GenerateMesh(walkableEdgesPoints);
        }

        private void GenerateMesh(List<List<Point>> edgesPoints)
        {
            var halfCommon = mDepth;
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
                    mVertices.Add(edgePoint.GetPosition(-mDepth, Side.Front));
                    mVertices.Add(edgePoint.GetPosition(mDepth, Side.Back));
                }
                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];

                    mUVs.Add(new Vector2(edgePoint.Position.x / common, edgePoint.Position.y / common));
                    mUVs.Add(
                        Vector2.Lerp(
                            new Vector2(edgePoint.Position.x / common, edgePoint.Position.y / common - 1), 
                            new Vector2(edgePoint.Position.x / common - 1, edgePoint.Position.y / common),
                            angles[pI]));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    mTriangles.Add(verticesOffset + vIndex);
                    mTriangles.Add(verticesOffset + vIndex + 3);
                    mTriangles.Add(verticesOffset + vIndex + 1);

                    mTriangles.Add(verticesOffset + vIndex);
                    mTriangles.Add(verticesOffset + vIndex + 2);
                    mTriangles.Add(verticesOffset + vIndex + 3);
                }
                
                verticesOffset = mVertices.Count;
            }
        }
    }
}