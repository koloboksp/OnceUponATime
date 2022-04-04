using System.Collections.Generic;
using System.Linq;
using Assets.ShapeEditor.Geometry;

namespace Assets.ShapeEditor
{
    public class CeilingAndWallsEdgesDetector
    {
        List<List<int>> mEdgesPointsIndexes;

        public List<List<int>> EdgesPointsIndexes => mEdgesPointsIndexes;

        public CeilingAndWallsEdgesDetector(List<Point> hullPoints, List<List<int>> walkableEdgesPointsIndexes)
        {
            mEdgesPointsIndexes = new List<List<int>>();
            if (walkableEdgesPointsIndexes != null && walkableEdgesPointsIndexes.Count > 0)
            {
                for (var index = 0; index < walkableEdgesPointsIndexes.Count; index++)
                {
                    var walkableEdgePointsIndex = walkableEdgesPointsIndexes[index];
                    var nextWalkableEdgePointsIndex = walkableEdgesPointsIndexes[(index + 1) % walkableEdgesPointsIndexes.Count];

                    mEdgesPointsIndexes.Add(new List<int>());

                    int startIndex = walkableEdgePointsIndex.Last();
                    int endIndex = nextWalkableEdgePointsIndex.First();
                    if (endIndex < startIndex)
                        endIndex += hullPoints.Count;

                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        mEdgesPointsIndexes.Last().Add(i % hullPoints.Count);
                    }
                }
            }
            else
            {
                mEdgesPointsIndexes.Add(hullPoints.Select((p,i)=>i).ToList());
                mEdgesPointsIndexes.Last().Add(0);
            }
        }
    }
}