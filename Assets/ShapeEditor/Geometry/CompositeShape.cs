﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Processes array of shapes into a single mesh
 * Automatically determines which shapes are solid, and which are holes
 * Ignores invalid shapes (contain self-intersections, too few points, overlapping holes)
 */

namespace Assets.ShapeEditor.Geometry
{
    public partial class CompositeShape
    {
        public Point[] vertices;
        public int[] triangles;

        private Shape[] shapes;
       
        public CompositeShape(IEnumerable<Shape> shapes)
        {
            this.shapes = shapes.ToArray();
        }

        public Mesh GetMesh(ShapeCreator owner)
        {
            Process(owner);

            return new Mesh()
            {
                vertices = vertices.Select(p => p.GetPosition(0, Side.Front)).ToArray(),
                triangles = triangles,
                normals = vertices.Select(x => Vector3.forward).ToArray()
            };
        }

        public void Process(ShapeCreator owner)
        {
            // Generate array of valid shape data
            CompositeShapeData[] eligibleShapes = shapes.Select(x => new CompositeShapeData(x.Points.Select(i=>i).ToArray())).Where(x => x.IsValidShape).ToArray();

            // Set parents for all shapes. A parent is a shape which completely contains another shape.
            for (int i = 0; i < eligibleShapes.Length; i++)
            {
                for (int j = 0; j < eligibleShapes.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (eligibleShapes[i].IsParentOf(eligibleShapes[j]))
                    {
                        eligibleShapes[j].parents.Add(eligibleShapes[i]);
                    }
                }
            }

            // Holes are shapes with an odd number of parents.
            CompositeShapeData[] holeShapes = eligibleShapes.Where(x => x.parents.Count % 2 != 0).ToArray();
            foreach (CompositeShapeData holeShape in holeShapes)
            {
                // The most immediate parent (i.e the smallest parent shape) will be the one that has the highest number of parents of its own. 
                CompositeShapeData immediateParent = holeShape.parents.OrderByDescending(x => x.parents.Count).First();
                immediateParent.holes.Add(holeShape);
            }

            // Solid shapes have an even number of parents
            CompositeShapeData[] solidShapes = eligibleShapes.Where(x => x.parents.Count % 2 == 0).ToArray();
            foreach (CompositeShapeData solidShape in solidShapes)
            {
                solidShape.ValidateHoles();

            }
            // Create polygons from the solid shapes and their associated hole shapes
            Polygon[] polygons = solidShapes.Select(x => new Polygon(x.polygon.points, x.holes.Select(h => h.polygon.points).ToArray())).ToArray();
  
            // Flatten the points arrays from all polygons into a single array, and convert the vector2s to vector3s.
            vertices = polygons.SelectMany(x => x.points).ToArray();

            // Triangulate each polygon and flatten the triangle arrays into a single array.
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
                startVertexIndex += polygons[i].numPoints;
            }

            triangles = allTriangles.ToArray();
        }
    }
}