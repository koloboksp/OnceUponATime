using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.ShapeEditor.Geometry
{
    public partial class CompositeShape
    {
        public class CompositeShapeData
        {
            public readonly Point[] Points;
            public readonly Polygon Polygon;
            public readonly int[] Triangles;

            public List<CompositeShapeData> Parents = new List<CompositeShapeData>();
            public List<CompositeShapeData> Holes = new List<CompositeShapeData>();
            public bool IsValidShape { get; private set; }

            public CompositeShapeData(Point[] points)
            {
                this.Points = points.Select(v => v).ToArray();
                IsValidShape = points.Length >= 3 && !IntersectsWithSelf();

                if (IsValidShape)
                {
                    Polygon = new Polygon(this.Points);
                    Triangulator t = new Triangulator(Polygon);
                    Triangles = t.Triangulate();
                }
            }
            
            public void ValidateHoles()
            {
                for (int i = 0; i < Holes.Count; i++)
                {
                    for (int j = i + 1; j < Holes.Count; j++)
                    {
                        bool overlap = Holes[i].OverlapsPartially(Holes[j]);

                        if (overlap)
                        {
                            Holes[i].IsValidShape = false;
                            break;
                        }
                    }
                }

                for (int i = Holes.Count - 1; i >= 0; i--)
                {
                    if (!Holes[i].IsValidShape)
                    {
                        Holes.RemoveAt(i);
                    }
                }
            }

            public bool IsParentOf(CompositeShapeData otherShape)
            {
                if (otherShape.Parents.Contains(this))
                {
                    return true;
                }
                if (Parents.Contains(otherShape))
                {
                    return false;
                }
                
                bool pointInsideShape = false;
                for (int i = 0; i < Triangles.Length; i += 3)
                {
                    if (Maths2D.PointInTriangle(Polygon.Points[Triangles[i]].Position, Polygon.Points[Triangles[i + 1]].Position, 
                        Polygon.Points[Triangles[i + 2]].Position, otherShape.Points[0].Position))
                    {
                        pointInsideShape = true;
                        break;
                    }
                }

                if (!pointInsideShape)
                {
                    return false;
                }
                
                for (int i = 0; i < Points.Length; i++)
                {
                    LineSegment parentSeg = new LineSegment(Points[i].Position, Points[(i + 1) % Points.Length].Position);
                    for (int j = 0; j < otherShape.Points.Length; j++)
                    {
                        LineSegment childSeg = new LineSegment(otherShape.Points[j].Position, otherShape.Points[(j + 1) % otherShape.Points.Length].Position);
                        if (Maths2D.LineSegmentsIntersect(parentSeg.a, parentSeg.b, childSeg.a, childSeg.b))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            
            public bool OverlapsPartially(CompositeShapeData otherShape)
            {
                for (int i = 0; i < Points.Length; i++)
                {
                    LineSegment segA = new LineSegment(Points[i].Position, Points[(i + 1) % Points.Length].Position);
                    for (int j = 0; j < otherShape.Points.Length; j++)
                    {
                        LineSegment segB = new LineSegment(otherShape.Points[j].Position, otherShape.Points[(j + 1) % otherShape.Points.Length].Position);
                        if (Maths2D.LineSegmentsIntersect(segA.a, segA.b, segB.a, segB.b))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            
            public bool IntersectsWithSelf()
            {

                for (int i = 0; i < Points.Length; i++)
                {
                    LineSegment segA = new LineSegment(Points[i].Position, Points[(i + 1) % Points.Length].Position);
                    for (int j = i + 2; j < Points.Length; j++)
                    {
                        if ((j + 1) % Points.Length == i)
                        {
                            continue;
                        }
                        LineSegment segB = new LineSegment(Points[j].Position, Points[(j + 1) % Points.Length].Position);
                        if (Maths2D.LineSegmentsIntersect(segA.a, segA.b, segB.a, segB.b))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public struct LineSegment
            {
                public readonly Vector2 a;
                public readonly Vector2 b;

                public LineSegment(Vector2 a, Vector2 b)
                {
                    this.a = a;
                    this.b = b;
                }
            }
        }
    }
}