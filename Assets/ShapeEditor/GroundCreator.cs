using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;
using UnityEngine;

namespace Assets.ShapeEditor
{
    public class GroundCreator : ShapeCreator
    {
        private float HorizontalOffsetOfWalkableExtremePoints = 0.05f;

        public float WalkableAngle = 35;
        public bool GenerateWalkablePart = true;
  
        public PolygonCollider2D PolygonCollider2D;
        public float WalkableGroundDepthOffset = 0.2f;
        public float WalkableGroundThickness = 0.2f;

        public override void UpdateMeshDisplay()
        {
            var polygons = Process(this);
     

            CeilingAndWallsEdgesDetector ceilingAndWallsEdgesDetector = null;
            HolesWallsEdgesDetector holesWallsEdgesDetector = null;
            WalkableEdgesDetector walkableEdgesDetector = null;

            FrontWallsMeshGenerator frontWallsMeshGenerator = null;
            CeilingAndWallsMeshGenerator ceilingAndWallsMeshGenerator = null;
            CeilingAndWallsMeshGenerator holesCeilingAndWallsMeshGenerator = null;
            WalkableMeshGenerator walkableMeshGenerator = null;

            if (polygons.Length > 0)
            {
                var polygon = polygons[0];
                var hullPoints = polygon.Points.Take(polygon.NumHullPoints).ToList();

                if (GenerateWalkablePart)
                {
                    walkableEdgesDetector = new WalkableEdgesDetector(hullPoints, WalkableAngle);
                    if (walkableEdgesDetector.Found)
                    {
                        walkableMeshGenerator = new WalkableMeshGenerator(hullPoints, walkableEdgesDetector.EdgesPointsIndexes,
                            WalkableGroundDepthOffset + Depth, WalkableGroundThickness, HorizontalOffsetOfWalkableExtremePoints);
                    }
                }

                frontWallsMeshGenerator = new FrontWallsMeshGenerator(polygon, Depth);
                ceilingAndWallsEdgesDetector = new CeilingAndWallsEdgesDetector(hullPoints, walkableEdgesDetector?.EdgesPointsIndexes);
                ceilingAndWallsMeshGenerator = new CeilingAndWallsMeshGenerator(hullPoints, ceilingAndWallsEdgesDetector.EdgesPointsIndexes, Depth);

                holesWallsEdgesDetector = new HolesWallsEdgesDetector(polygon, null);
                
                if (holesWallsEdgesDetector.EdgesPointsIndexes.Count > 0)
                {
                    holesCeilingAndWallsMeshGenerator = new CeilingAndWallsMeshGenerator(polygon.Points.ToList(), holesWallsEdgesDetector.EdgesPointsIndexes, Depth);  
                }
                if (PolygonCollider2D != null)
                    PolygonCollider2D.points = hullPoints.Select(p => p.Position).ToArray();
            }
           
            var triangles = new List<List<int>>();
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            int verticesOffset = 0;

            vertices.AddRange(frontWallsMeshGenerator.Vertices);
            uvs.AddRange(frontWallsMeshGenerator.UVs);
            triangles.Add(frontWallsMeshGenerator.Triangles.Select(ti => ti + verticesOffset).ToList());
            verticesOffset = vertices.Count;

            vertices.AddRange(ceilingAndWallsMeshGenerator.Vertices);
            uvs.AddRange(ceilingAndWallsMeshGenerator.UVs);
            triangles.Last().AddRange(ceilingAndWallsMeshGenerator.Triangles.Select(ti => ti + verticesOffset).ToList());
            verticesOffset = vertices.Count;

            if (holesCeilingAndWallsMeshGenerator != null)
            {
                vertices.AddRange(holesCeilingAndWallsMeshGenerator.Vertices);
                uvs.AddRange(holesCeilingAndWallsMeshGenerator.UVs);
                triangles.Last().AddRange(holesCeilingAndWallsMeshGenerator.Triangles.Select(ti => ti + verticesOffset).ToList());
                verticesOffset = vertices.Count;
            }

            if (walkableMeshGenerator != null)
            {
                vertices.AddRange(walkableMeshGenerator.Vertices);
                uvs.AddRange(walkableMeshGenerator.UVs);
                triangles.Add(walkableMeshGenerator.Triangles.Select(ti => ti + verticesOffset).ToList());
                verticesOffset = vertices.Count;
            }
           
            var mesh = (MeshFilter.sharedMesh == null) ? new Mesh() : MeshFilter.sharedMesh;
            MeshFilter.sharedMesh = mesh;

            mesh.Clear();
            
            mesh.subMeshCount = triangles.Count;
            mesh.name = gameObject.name + "_Mesh";
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(vertices.Select(i=>new Color(0,1,1)).ToList());
            for (var smIndex = 0; smIndex < triangles.Count; smIndex++)
                mesh.SetTriangles(triangles[smIndex], smIndex);

            mesh.RecalculateNormals();
            mesh.UploadMeshData(false);
        }

       
        public Polygon[] Process(ShapeCreator owner)
        {
            CompositeShape.CompositeShapeData[] eligibleShapes = Shapes
                .Select(x => new CompositeShape.CompositeShapeData(x.Points.ToArray())).Where(x => x.IsValidShape)
                .ToArray();

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

            CompositeShape.CompositeShapeData[] holeShapes =
                eligibleShapes.Where(x => x.Parents.Count % 2 != 0).ToArray();
            foreach (CompositeShape.CompositeShapeData holeShape in holeShapes)
            {
                CompositeShape.CompositeShapeData immediateParent =
                    holeShape.Parents.OrderByDescending(x => x.Parents.Count).First();
                immediateParent.Holes.Add(holeShape);
            }

            CompositeShape.CompositeShapeData[] solidShapes =
                eligibleShapes.Where(x => x.Parents.Count % 2 == 0).ToArray();
            foreach (CompositeShape.CompositeShapeData solidShape in solidShapes)
            {
                solidShape.ValidateHoles();

            }

            Polygon[] polygons = solidShapes.Select(x => new Polygon(x.Polygon.Points, x.Holes.Select(h => h.Polygon.Points).ToArray())).ToArray();

            return polygons;
        } 
    }
}