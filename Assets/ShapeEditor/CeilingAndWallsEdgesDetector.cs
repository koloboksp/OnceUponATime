using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;

namespace Assets.ShapeEditor
{
    public class CeilingAndWallsEdgesDetector
    {
        private readonly List<List<int>> _edgesPointsIndexes;

        public List<List<int>> EdgesPointsIndexes => _edgesPointsIndexes;

        public CeilingAndWallsEdgesDetector(List<Point> hullPoints, List<List<int>> walkableEdgesPointsIndexes)
        {
            _edgesPointsIndexes = new List<List<int>>();
            if (walkableEdgesPointsIndexes != null && walkableEdgesPointsIndexes.Count > 0)
            {
                for (var index = 0; index < walkableEdgesPointsIndexes.Count; index++)
                {
                    var walkableEdgePointsIndex = walkableEdgesPointsIndexes[index];
                    var nextWalkableEdgePointsIndex = walkableEdgesPointsIndexes[(index + 1) % walkableEdgesPointsIndexes.Count];

                    _edgesPointsIndexes.Add(new List<int>());

                    int startIndex = walkableEdgePointsIndex.Last();
                    int endIndex = nextWalkableEdgePointsIndex.First();
                    if (endIndex < startIndex)
                        endIndex += hullPoints.Count;

                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        _edgesPointsIndexes.Last().Add(i % hullPoints.Count);
                    }
                }
            }
            else
            {
                _edgesPointsIndexes.Add(hullPoints.Select((p,i)=>i).ToList());
                _edgesPointsIndexes.Last().Add(0);
            }
        }
    }
}