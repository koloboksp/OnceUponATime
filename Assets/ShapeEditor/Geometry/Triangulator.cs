using System.Collections.Generic;
using UnityEngine;

namespace Assets.ShapeEditor.Geometry
{
	/*
     * https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
     */

	public class Triangulator
    {
        private LinkedList<Vertex> _vertsInClippedPolygon;
        private int[] _tris;
        private int _triIndex;

        public Triangulator(Polygon polygon)
        {
            int numHoleToHullConnectionVerts = 2 * polygon.NumHoles;
            int totalNumVerts = polygon.NumPoints + numHoleToHullConnectionVerts;
            _tris = new int[(totalNumVerts - 2) * 3];
            _vertsInClippedPolygon = GenerateVertexList(polygon);
        }

        public int[] Triangulate()
        {
            while (_vertsInClippedPolygon.Count >= 3)
            {
                bool hasRemovedEarThisIteration = false;
                LinkedListNode<Vertex> vertexNode = _vertsInClippedPolygon.First;
                for (int i = 0; i < _vertsInClippedPolygon.Count; i++)
                {
                    LinkedListNode<Vertex> prevVertexNode = vertexNode.Previous ?? _vertsInClippedPolygon.Last;
                    LinkedListNode<Vertex> nextVertexNode = vertexNode.Next ?? _vertsInClippedPolygon.First;

                    if (vertexNode.Value.isConvex)
                    {
                        if (!TriangleContainsVertex(prevVertexNode.Value, vertexNode.Value, nextVertexNode.Value))
                        {
                            if (!prevVertexNode.Value.isConvex)
                            {
                                LinkedListNode<Vertex> prevOfPrev = prevVertexNode.Previous ?? _vertsInClippedPolygon.Last;

                                prevVertexNode.Value.isConvex = IsConvex(prevOfPrev.Value.position, prevVertexNode.Value.position, nextVertexNode.Value.position);
                            }
                            if (!nextVertexNode.Value.isConvex)
                            {
                                LinkedListNode<Vertex> nextOfNext = nextVertexNode.Next ?? _vertsInClippedPolygon.First;
                                nextVertexNode.Value.isConvex = IsConvex(prevVertexNode.Value.position, nextVertexNode.Value.position, nextOfNext.Value.position);
                            }

                            _tris[_triIndex * 3 + 2] = prevVertexNode.Value.index;
                            _tris[_triIndex * 3 + 1] = vertexNode.Value.index;
                            _tris[_triIndex * 3] = nextVertexNode.Value.index;
                            _triIndex++;

                            hasRemovedEarThisIteration = true;
                            _vertsInClippedPolygon.Remove(vertexNode);
                            break;
                        }
                    }


                    vertexNode = nextVertexNode;
                }

                if (!hasRemovedEarThisIteration)
                {
                    Debug.LogError("Error triangulating mesh. Aborted.");
                    return null;
                }
            }
            return _tris;
        }
        
        private LinkedList<Vertex> GenerateVertexList(Polygon polygon)
        {
            LinkedList<Vertex> vertexList = new LinkedList<Vertex>();
            LinkedListNode<Vertex> currentNode = null;

            for (int i = 0; i < polygon.NumHullPoints; i++)
            {
                int prevPointIndex = (i - 1 + polygon.NumHullPoints) % polygon.NumHullPoints;
                int nextPointIndex = (i + 1) % polygon.NumHullPoints;

                bool vertexIsConvex = IsConvex(polygon.Points[prevPointIndex].Position, polygon.Points[i].Position, polygon.Points[nextPointIndex].Position);
                Vertex currentHullVertex = new Vertex(polygon.Points[i].Position, i, vertexIsConvex);

                if (currentNode == null)
                    currentNode = vertexList.AddFirst(currentHullVertex);
                else
                    currentNode = vertexList.AddAfter(currentNode, currentHullVertex);
            }
            
            List<HoleData> sortedHoleData = new List<HoleData>();

            for (int holeIndex = 0; holeIndex < polygon.NumHoles; holeIndex++)
            {
                // Find index of rightmost point in hole. This 'bridge' point is where the hole will be connected to the hull.
                Vector2 holeBridgePoint = new Vector2(float.MinValue, 0);
                int holeBridgeIndex = 0;
                for (int i = 0; i < polygon.NumPointsPerHole[holeIndex]; i++)
                {
                    if (polygon.GetHolePoint(i, holeIndex).Position.x > holeBridgePoint.x)
                    {
                        holeBridgePoint = polygon.GetHolePoint(i, holeIndex).Position;
                        holeBridgeIndex = i;

                    }
                }
                sortedHoleData.Add(new HoleData(holeIndex, holeBridgeIndex, holeBridgePoint));
            }
            
            sortedHoleData.Sort((x, y) => (x.bridgePoint.x > y.bridgePoint.x) ? -1 : 1);

            foreach (HoleData holeData in sortedHoleData)
            {
                Vector2 rayIntersectPoint = new Vector2(float.MaxValue, holeData.bridgePoint.y);
                List<LinkedListNode<Vertex>> hullNodesPotentiallyInBridgeTriangle = new List<LinkedListNode<Vertex>>();
                LinkedListNode<Vertex> initialBridgeNodeOnHull = null;
                currentNode = vertexList.First;
                while (currentNode != null)
                {
                    LinkedListNode<Vertex> nextNode = (currentNode.Next == null) ? vertexList.First : currentNode.Next;
                    Vector2 p0 = currentNode.Value.position;
                    Vector2 p1 = nextNode.Value.position;

                    if (p0.x > holeData.bridgePoint.x || p1.x > holeData.bridgePoint.x)
                    {
                        if (p0.y > holeData.bridgePoint.y != p1.y > holeData.bridgePoint.y)
                        {
                            float rayIntersectX = p1.x; // only true if line p0,p1 is vertical
                            if (!Mathf.Approximately(p0.x, p1.x))
                            {
                                float intersectY = holeData.bridgePoint.y;
                                float gradient = (p0.y - p1.y) / (p0.x - p1.x);
                                float c = p1.y - gradient * p1.x;
                                rayIntersectX = (intersectY - c) / gradient;
                            }

                            if (rayIntersectX > holeData.bridgePoint.x)
                            {
                                LinkedListNode<Vertex> potentialNewBridgeNode = (p0.x > p1.x) ? currentNode : nextNode;
                                bool isDuplicateEdge = Mathf.Approximately(rayIntersectX, rayIntersectPoint.x);

								bool connectToThisDuplicateEdge = holeData.bridgePoint.y > potentialNewBridgeNode.Previous.Value.position.y;
  
                                if (!isDuplicateEdge || connectToThisDuplicateEdge)
                                {
                                    // if this is the closest ray intersection thus far, set bridge hull node to point in line having greater x pos (since def to right of hole).
                                    if (rayIntersectX < rayIntersectPoint.x || isDuplicateEdge)
                                    {
                                        rayIntersectPoint.x = rayIntersectX;
                                        initialBridgeNodeOnHull = potentialNewBridgeNode;
                                    }
                                }
                            }
                        }
                    }

                    if (currentNode != initialBridgeNodeOnHull)
                    {
                        if (!currentNode.Value.isConvex && p0.x > holeData.bridgePoint.x)
                        {
                            hullNodesPotentiallyInBridgeTriangle.Add(currentNode);
                        }
                    }
                    currentNode = currentNode.Next;
                }

                LinkedListNode<Vertex> validBridgeNodeOnHull = initialBridgeNodeOnHull;
                foreach (LinkedListNode<Vertex> nodePotentiallyInTriangle in hullNodesPotentiallyInBridgeTriangle)
                {
                    if (nodePotentiallyInTriangle.Value.index == initialBridgeNodeOnHull.Value.index)
                    {
                        continue;
                    }
                    
                    if (Maths2D.PointInTriangle(holeData.bridgePoint, rayIntersectPoint, initialBridgeNodeOnHull.Value.position, nodePotentiallyInTriangle.Value.position))
                    {
                        bool isDuplicatePoint = validBridgeNodeOnHull.Value.position == nodePotentiallyInTriangle.Value.position;

                        float currentDstFromHoleBridgeY = Mathf.Abs(holeData.bridgePoint.y - validBridgeNodeOnHull.Value.position.y);
                        float pointInTriDstFromHoleBridgeY = Mathf.Abs(holeData.bridgePoint.y - nodePotentiallyInTriangle.Value.position.y);

                        if (pointInTriDstFromHoleBridgeY < currentDstFromHoleBridgeY || isDuplicatePoint)
                        {
                            validBridgeNodeOnHull = nodePotentiallyInTriangle;

                        }
                    }
                }

                currentNode = validBridgeNodeOnHull;
                for (int i = holeData.bridgeIndex; i <= polygon.NumPointsPerHole[holeData.holeIndex] + holeData.bridgeIndex; i++)
                {
                    int previousIndex = currentNode.Value.index;
                    int currentIndex = polygon.IndexOfPointInHole(i % polygon.NumPointsPerHole[holeData.holeIndex], holeData.holeIndex);
                    int nextIndex = polygon.IndexOfPointInHole((i + 1) % polygon.NumPointsPerHole[holeData.holeIndex], holeData.holeIndex);

                    if (i == polygon.NumPointsPerHole[holeData.holeIndex] + holeData.bridgeIndex) // have come back to starting point
                    {
                        nextIndex = validBridgeNodeOnHull.Value.index; // next point is back to the point on the hull
                    }

                    bool vertexIsConvex = IsConvex(polygon.Points[previousIndex].Position, polygon.Points[currentIndex].Position, polygon.Points[nextIndex].Position);
                    Vertex holeVertex = new Vertex(polygon.Points[currentIndex].Position, currentIndex, vertexIsConvex);
                    currentNode = vertexList.AddAfter(currentNode, holeVertex);
                }

                Vector2 nextVertexPos = (currentNode.Next == null) ? vertexList.First.Value.position : currentNode.Next.Value.position;
                bool isConvex = IsConvex(holeData.bridgePoint, validBridgeNodeOnHull.Value.position, nextVertexPos);
                Vertex repeatStartHullVert = new Vertex(validBridgeNodeOnHull.Value.position, validBridgeNodeOnHull.Value.index, isConvex);
                vertexList.AddAfter(currentNode, repeatStartHullVert);

                LinkedListNode<Vertex> nodeBeforeStartBridgeNodeOnHull = (validBridgeNodeOnHull.Previous == null) ? vertexList.Last : validBridgeNodeOnHull.Previous;
                LinkedListNode<Vertex> nodeAfterStartBridgeNodeOnHull = (validBridgeNodeOnHull.Next == null) ? vertexList.First : validBridgeNodeOnHull.Next;
                validBridgeNodeOnHull.Value.isConvex = IsConvex(nodeBeforeStartBridgeNodeOnHull.Value.position, validBridgeNodeOnHull.Value.position, nodeAfterStartBridgeNodeOnHull.Value.position);
            }
            return vertexList;
        }
        
        private bool TriangleContainsVertex(Vertex v0, Vertex v1, Vertex v2)
        {
            LinkedListNode<Vertex> vertexNode = _vertsInClippedPolygon.First;
            for (int i = 0; i < _vertsInClippedPolygon.Count; i++)
            {
                if (!vertexNode.Value.isConvex)
                {
                    Vertex vertexToCheck = vertexNode.Value;
                    if (vertexToCheck.index != v0.index && vertexToCheck.index != v1.index && vertexToCheck.index != v2.index) // dont check verts that make up triangle
                    {
                        if (Maths2D.PointInTriangle(v0.position, v1.position, v2.position, vertexToCheck.position))
                        {
                            return true;
                        }
                    }
                }
                vertexNode = vertexNode.Next;
            }

            return false;
        }
        
        private bool IsConvex(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return Maths2D.SideOfLine(v0, v2, v1) == -1;
        }

        public struct HoleData
        {
            public readonly int holeIndex;
            public readonly int bridgeIndex;
            public readonly Vector2 bridgePoint;

            public HoleData(int holeIndex, int bridgeIndex, Vector2 bridgePoint)
            {
                this.holeIndex = holeIndex;
                this.bridgeIndex = bridgeIndex;
                this.bridgePoint = bridgePoint;
            }
        }

        public class Vertex
        {
            public readonly Vector2 position;
            public readonly int index;
            public bool isConvex;

            public Vertex(Vector2 position, int index, bool isConvex)
            {
                this.position = position;
                this.index = index;
                this.isConvex = isConvex;
            }
        }
    }

}