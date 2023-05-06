using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;

namespace Assets.ShapeEditor
{
    public class HolesWallsEdgesDetector
    {
        private readonly List<List<int>> _edgesPointsIndexes;
        public List<List<int>> EdgesPointsIndexes => _edgesPointsIndexes;

        public HolesWallsEdgesDetector(Polygon polygon, List<List<int>> walkableEdgesPointsIndexes)
        {
            _edgesPointsIndexes = new List<List<int>>();
            for (int hIndex = 0; hIndex < polygon.NumHoles; hIndex++)
            {
                _edgesPointsIndexes.Add(new List<int>());
                for (int pIndex = 0; pIndex < polygon.NumPointsPerHole[hIndex]; pIndex++)
                {
                    var indexOfPointInHole = polygon.IndexOfPointInHole(pIndex, hIndex);
                    _edgesPointsIndexes.Last().Add(indexOfPointInHole);
                }

                _edgesPointsIndexes.Last().Add(polygon.IndexOfPointInHole(0, hIndex));
            }             
            
        }
    }
}