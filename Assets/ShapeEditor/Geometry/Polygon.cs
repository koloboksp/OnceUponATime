using UnityEngine;

/*
 * Processes given arrays of hull and hole points into single array, enforcing correct -wiseness.
 * Also provides convenience methods for accessing different hull/hole points
 */

namespace Assets.ShapeEditor.Geometry
{
    public class Polygon
    {
        public readonly Point[] points;
        public readonly int numPoints;

        public readonly int numHullPoints;

        public readonly int[] numPointsPerHole;
        public readonly int numHoles;

        readonly int[] holeStartIndices;

        public Polygon(Point[] hull, Point[][] holes)
        {
            numHullPoints = hull.Length;
            numHoles = holes.GetLength(0);

            numPointsPerHole = new int[numHoles];
            holeStartIndices = new int[numHoles];
            int numHolePointsSum = 0;

            for (int i = 0; i < holes.GetLength(0); i++)
            {
                numPointsPerHole[i] = holes[i].Length;

                holeStartIndices[i] = numHullPoints + numHolePointsSum;
                numHolePointsSum += numPointsPerHole[i];
            }

            numPoints = numHullPoints + numHolePointsSum;
            points = new Point[numPoints];


            // add hull points, ensuring they wind in counterclockwise order
            bool reverseHullPointsOrder = !PointsAreCounterClockwise(hull);
            for (int i = 0; i < numHullPoints; i++)
            {
                points[i] = hull[(reverseHullPointsOrder) ? numHullPoints - 1 - i : i];
            }

            // add hole points, ensuring they wind in clockwise order
            for (int i = 0; i < numHoles; i++)
            {
                bool reverseHolePointsOrder = PointsAreCounterClockwise(holes[i]);
                for (int j = 0; j < holes[i].Length; j++)
                {
                    points[IndexOfPointInHole(j, i)] = holes[i][(reverseHolePointsOrder) ? holes[i].Length - j - 1 : j];
                }
            }

        }

        public Polygon(Point[] hull) : this(hull, new Point[0][])
        {
        }

        bool PointsAreCounterClockwise(Point[] testPoints)
        {
            float signedArea = 0;
            for (int i = 0; i < testPoints.Length; i++)
            {
                int nextIndex = (i + 1) % testPoints.Length;
                signedArea += (testPoints[nextIndex].Position.x - testPoints[i].Position.x) * (testPoints[nextIndex].Position.y + testPoints[i].Position.y);
            }

            return signedArea < 0;
        }

        public int IndexOfFirstPointInHole(int holeIndex)
        {
            return holeStartIndices[holeIndex];
        }

        public int IndexOfPointInHole(int index, int holeIndex)
        {
            return holeStartIndices[holeIndex] + index;
        }

        public Point GetHolePoint(int index, int holeIndex)
        {
            return points[holeStartIndices[holeIndex] + index];
        }

    }

}