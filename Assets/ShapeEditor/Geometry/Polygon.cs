namespace Assets.ShapeEditor.Geometry
{
    public class Polygon
    {
        public readonly Point[] Points;
        public readonly int NumPoints;

        public readonly int NumHullPoints;

        public readonly int[] NumPointsPerHole;
        public readonly int NumHoles;

        private readonly int[] _holeStartIndices;

        public Polygon(Point[] hull, Point[][] holes)
        {
            NumHullPoints = hull.Length;
            NumHoles = holes.GetLength(0);

            NumPointsPerHole = new int[NumHoles];
            _holeStartIndices = new int[NumHoles];
            int numHolePointsSum = 0;

            for (int i = 0; i < holes.GetLength(0); i++)
            {
                NumPointsPerHole[i] = holes[i].Length;

                _holeStartIndices[i] = NumHullPoints + numHolePointsSum;
                numHolePointsSum += NumPointsPerHole[i];
            }

            NumPoints = NumHullPoints + numHolePointsSum;
            Points = new Point[NumPoints];
            
            bool reverseHullPointsOrder = !PointsAreCounterClockwise(hull);
            for (int i = 0; i < NumHullPoints; i++)
            {
                Points[i] = hull[(reverseHullPointsOrder) ? NumHullPoints - 1 - i : i];
            }

            for (int i = 0; i < NumHoles; i++)
            {
                bool reverseHolePointsOrder = PointsAreCounterClockwise(holes[i]);
                for (int j = 0; j < holes[i].Length; j++)
                {
                    Points[IndexOfPointInHole(j, i)] = holes[i][(reverseHolePointsOrder) ? holes[i].Length - j - 1 : j];
                }
            }

        }

        public Polygon(Point[] hull) : this(hull, new Point[0][])
        {
        }

        private bool PointsAreCounterClockwise(Point[] testPoints)
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
            return _holeStartIndices[holeIndex];
        }

        public int IndexOfPointInHole(int index, int holeIndex)
        {
            return _holeStartIndices[holeIndex] + index;
        }

        public Point GetHolePoint(int index, int holeIndex)
        {
            return Points[_holeStartIndices[holeIndex] + index];
        }
    }
}