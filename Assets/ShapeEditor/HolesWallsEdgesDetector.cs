using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;

namespace Assets.ShapeEditor
{
    public class HolesWallsEdgesDetector
    {
        private List<List<int>> mEdgesPointsIndexes;
        public List<List<int>> EdgesPointsIndexes => mEdgesPointsIndexes;

        public HolesWallsEdgesDetector(Polygon polygon, List<List<int>> walkableEdgesPointsIndexes)
        {
            mEdgesPointsIndexes = new List<List<int>>();
            for (int hIndex = 0; hIndex < polygon.numHoles; hIndex++)
            {
                mEdgesPointsIndexes.Add(new List<int>());
                for (int pIndex = 0; pIndex < polygon.numPointsPerHole[hIndex]; pIndex++)
                {
                    var indexOfPointInHole = polygon.IndexOfPointInHole(pIndex, hIndex);
                    mEdgesPointsIndexes.Last().Add(indexOfPointInHole);
                }

                mEdgesPointsIndexes.Last().Add(polygon.IndexOfPointInHole(0, hIndex));
            }             
            
        }
    }
}