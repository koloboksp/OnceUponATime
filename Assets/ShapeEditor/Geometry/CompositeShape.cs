using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.ShapeEditor.Geometry
{
    public partial class CompositeShape
    {
        private Point[] _vertices;
        private int[] _triangles;

        private Shape[] _shapes;
       
        public CompositeShape(IEnumerable<Shape> shapes)
        {
            this._shapes = shapes.ToArray();
        }

        public Mesh GetMesh(ShapeCreator owner)
        {
            Process(owner);

            return new Mesh()
            {
                vertices = _vertices.Select(p => p.GetPosition(0, Side.Front)).ToArray(),
                triangles = _triangles,
                normals = _vertices.Select(x => Vector3.forward).ToArray()
            };
        }

        public void Process(ShapeCreator owner)
        {
           CompositeShapeData[] eligibleShapes = _shapes.Select(x => new CompositeShapeData(x.Points.Select(i=>i).ToArray())).Where(x => x.IsValidShape).ToArray();

            for (int i = 0; i < eligibleShapes.Length; i++)
            {
                for (int j = 0; j < eligibleShapes.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (eligibleShapes[i].IsParentOf(eligibleShapes[j]))
                    {
                        eligibleShapes[j].Parents.Add(eligibleShapes[i]);
                    }
                }
            }

            CompositeShapeData[] holeShapes = eligibleShapes.Where(x => x.Parents.Count % 2 != 0).ToArray();
            foreach (CompositeShapeData holeShape in holeShapes)
            {
                CompositeShapeData immediateParent = holeShape.Parents.OrderByDescending(x => x.Parents.Count).First();
                immediateParent.Holes.Add(holeShape);
            }

            CompositeShapeData[] solidShapes = eligibleShapes.Where(x => x.Parents.Count % 2 == 0).ToArray();
            foreach (CompositeShapeData solidShape in solidShapes)
            {
                solidShape.ValidateHoles();

            }
            Polygon[] polygons = solidShapes.Select(x => new Polygon(x.Polygon.Points, x.Holes.Select(h => h.Polygon.Points).ToArray())).ToArray();
  
            _vertices = polygons.SelectMany(x => x.Points).ToArray();

            List<int> allTriangles = new List<int>();
            int startVertexIndex = 0;
            for (int i = 0; i < polygons.Length; i++)
            {
                Triangulator triangulator = new Triangulator(polygons[i]);
                int[] polygonTriangles = triangulator.Triangulate();

                for (int j = 0; j < polygonTriangles.Length; j++)
                {
                    allTriangles.Add(polygonTriangles[j] + startVertexIndex);
                }
                startVertexIndex += polygons[i].NumPoints;
            }

            _triangles = allTriangles.ToArray();
        }
    }
}