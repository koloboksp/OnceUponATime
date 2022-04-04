using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class WalkableMeshGenerator
    {    
        float mHorizontalOffsetOfExtremePoints;
        float mThickness;
        float mDepth;

       
        List<int> mTriangles = new List<int>();
        List<Vector3> mVertices = new List<Vector3>();
        List<Vector3> mNormals = new List<Vector3>();
        List<Vector2> mUVs = new List<Vector2>();

       
        public List<int> Triangles => mTriangles;
        public List<Vector3> Vertices => mVertices;
        public List<Vector3> Normals => mNormals;
        public List<Vector2> UVs => mUVs;
       
        public WalkableMeshGenerator(List<Point> hullPoints, List<List<int>> edgesPointsIndexes, float depth, float thickness,  float horizontalOffsetOfExtremePoints)
        {
            mDepth = depth;
            mThickness = thickness;
            mHorizontalOffsetOfExtremePoints = horizontalOffsetOfExtremePoints;
 
            var walkableEdgesPoints = edgesPointsIndexes.Select(epi => epi.Select(pi => hullPoints[pi]).ToList()).ToList();
            ApplyHorizontalOffsetToExtremeEdgePoints(walkableEdgesPoints);
            var generatedBottomWalkableEdgesPoints = GenerateBottomPoints(walkableEdgesPoints);
            GenerateMesh(walkableEdgesPoints, generatedBottomWalkableEdgesPoints);     
        }

        void ApplyHorizontalOffsetToExtremeEdgePoints(List<List<Point>> edgesPoints)
        {
            foreach (List<Point> edgePoints in edgesPoints)
            {
                var startEdgeDirection = (edgePoints[0].Position - edgePoints[1].Position).normalized;
                var startPoint = edgePoints[0];
                startPoint.Position = edgePoints[0].Position + startEdgeDirection * mHorizontalOffsetOfExtremePoints;
                edgePoints[0] = startPoint;

                var endEdgeDirection = (edgePoints[edgePoints.Count - 2].Position - edgePoints[edgePoints.Count - 1].Position).normalized;
                var endPoint = edgePoints[edgePoints.Count - 1];
                endPoint.Position = edgePoints[edgePoints.Count - 1].Position - endEdgeDirection * mHorizontalOffsetOfExtremePoints;
                edgePoints[edgePoints.Count - 1] = endPoint;
            }
        }

        List<List<Point>> GenerateBottomPoints(List<List<Point>> edgesPoints)
        {
            List<List<Point>> bottomEdgesPoints = new List<List<Point>>();

            foreach (List<Point> edgePoints in edgesPoints)
            {
                List<Point> bottomEdgePoints = new List<Point>();

                var newStartPoint = new Point(edgePoints[0])
                {
                    Position = edgePoints[0].Position + Vector2.down * mThickness
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
                        Position = point.Position + bottomOffset * mThickness
                    };
                    bottomEdgePoints.Add(newPoint);
                }

                var newEndPoint = new Point(edgePoints[edgePoints.Count - 1])
                {
                    Position = edgePoints[edgePoints.Count - 1].Position + Vector2.down * mThickness
                };
                bottomEdgePoints.Add(newEndPoint);

                bottomEdgesPoints.Add(bottomEdgePoints);
            }

            return bottomEdgesPoints;
        }
        void GenerateMesh(List<List<Point>> edgesPoints, List<List<Point>> bottomEdgesPoints)
        {       
            var halfCommon = (mDepth + mThickness);
            var common = halfCommon * 2.0f;
            var uvThicknessToHalfCommon = mThickness / halfCommon;
          
            int verticesOffset = 0;
            for (int index = 0; index < edgesPoints.Count; index++)
            {
                List<Point> edgePoints = edgesPoints[index];
                List<Point> bottomEdgePoints = bottomEdgesPoints[index];

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    mVertices.Add(edgePoint.GetPosition(-mDepth, Side.Front));
                    mVertices.Add(edgePoint.GetPosition(mDepth, Side.Back));
                }
                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, uvThicknessToHalfCommon));
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, 1 - uvThicknessToHalfCommon));
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

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    var bottomEdgePoint = bottomEdgePoints[pI];
                    mVertices.Add(edgePoint.GetPosition(-mDepth, Side.Front));
                    mVertices.Add(bottomEdgePoint.GetPosition(-mDepth, Side.Front));
                }

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, uvThicknessToHalfCommon));
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, 0));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    mTriangles.Add(verticesOffset + vIndex + 1);
                    mTriangles.Add(verticesOffset + vIndex + 2);
                    mTriangles.Add(verticesOffset + vIndex);

                    mTriangles.Add(verticesOffset + vIndex + 1);
                    mTriangles.Add(verticesOffset + vIndex + 3);
                    mTriangles.Add(verticesOffset + vIndex + 2);
                }

                verticesOffset = mVertices.Count;

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var bottomEdgePoint = bottomEdgePoints[pI];
                    mVertices.Add(bottomEdgePoint.GetPosition(-mDepth, Side.Front));
                    mVertices.Add(bottomEdgePoint.GetPosition(mDepth, Side.Back));
                }

                for (var pI = 0; pI < edgePoints.Count; pI++)
                {
                    var edgePoint = edgePoints[pI];
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, 1));
                    mUVs.Add(new Vector2(edgePoint.Position.x / common, 0));
                }

                for (int pIndex = 0; pIndex < edgePoints.Count - 1; pIndex++)
                {
                    int vIndex = pIndex * 2;

                    mTriangles.Add(verticesOffset + vIndex + 1);
                    mTriangles.Add(verticesOffset + vIndex + 2);
                    mTriangles.Add(verticesOffset + vIndex);

                    mTriangles.Add(verticesOffset + vIndex + 1);
                    mTriangles.Add(verticesOffset + vIndex + 3);
                    mTriangles.Add(verticesOffset + vIndex + 2);
                }

                verticesOffset = mVertices.Count;

                mVertices.Add(edgePoints[0].GetPosition(-mDepth, Side.Front));
                mVertices.Add(edgePoints[0].GetPosition(mDepth, Side.Back));
                mVertices.Add(bottomEdgePoints[0].GetPosition(-mDepth, Side.Front));
                mVertices.Add(bottomEdgePoints[0].GetPosition(mDepth, Side.Back));

                mUVs.Add(new Vector2(0 + uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                mUVs.Add(new Vector2(1 - uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                mUVs.Add(new Vector2(0 + uvThicknessToHalfCommon, 0));
                mUVs.Add(new Vector2(1 - uvThicknessToHalfCommon, 0));

                mTriangles.Add(verticesOffset + 0 + 3);
                mTriangles.Add(verticesOffset + 0 + 0);
                mTriangles.Add(verticesOffset + 0 + 1);

                mTriangles.Add(verticesOffset + 0 + 3);
                mTriangles.Add(verticesOffset + 0 + 2);
                mTriangles.Add(verticesOffset + 0 + 0);

                verticesOffset = mVertices.Count;


                mVertices.Add(edgePoints[edgePoints.Count - 1].GetPosition(-mDepth, Side.Front));
                mVertices.Add(edgePoints[edgePoints.Count - 1].GetPosition(mDepth, Side.Back));
                mVertices.Add(bottomEdgePoints[bottomEdgePoints.Count - 1].GetPosition(-mDepth, Side.Front));
                mVertices.Add(bottomEdgePoints[bottomEdgePoints.Count - 1].GetPosition(mDepth, Side.Back));

                mUVs.Add(new Vector2(0 + uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                mUVs.Add(new Vector2(1 - uvThicknessToHalfCommon, uvThicknessToHalfCommon));
                mUVs.Add(new Vector2(0 + uvThicknessToHalfCommon, 0));
                mUVs.Add(new Vector2(1 - uvThicknessToHalfCommon, 0));

                mTriangles.Add(verticesOffset + 0 + 1);
                mTriangles.Add(verticesOffset + 0 + 0);
                mTriangles.Add(verticesOffset + 0 + 3);

                mTriangles.Add(verticesOffset + 0 + 0);
                mTriangles.Add(verticesOffset + 0 + 2);
                mTriangles.Add(verticesOffset + 0 + 3);

                verticesOffset = mVertices.Count;
            }
        }
    }
}