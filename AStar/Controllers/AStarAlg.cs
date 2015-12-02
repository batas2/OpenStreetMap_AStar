using System.Collections.Generic;
using System.Linq;
using AStar.Models;

namespace AStar.Controllers
{
    public class AStarAlg
    {
        public IEnumerable<RoadSegmentViewModel> FindPath(long start, long goal, IList<RoadSegmentViewModel> data)
        {
            var dataProvider = new DataProvider.DataProvider();

            var closedSet = new HashSet<long>();
            var openSet = new HashSet<long>(new[] {start});

            var cameFrom = new Dictionary<long, long>();

            var gScore = data.Select(x => x.StartNodeId).Distinct().ToDictionary(k => k, v => double.MaxValue);
            gScore[start] = 0;

            var fScore = data.Select(x => x.StartNodeId).Distinct().ToDictionary(k => k, v => double.MaxValue);
            fScore[start] = gScore[start] + dataProvider.HeuristicCostEstimate(start, goal);

            while (openSet.Count > 0)
            {
                var currentMin = fScore.Where(x => openSet.Contains(x.Key)).Select(x => x.Value).Min();
                var current = fScore.Where(x => x.Value == currentMin).Select(x => x.Key).FirstOrDefault();

                if (current == goal)
                {
                    var reconstructPath = FormatPath(cameFrom, goal);
                    var path = reconstructPath.Reverse().ToList();

                    var roadSegments = new List<RoadSegmentViewModel>();

                    for (var i = 1; i < path.Count(); i++)
                    {
                        var node = data.FirstOrDefault(x => x.StopNodeId == path[i - 1] && x.StartNodeId == path[i]);
                        if (node != null)
                            roadSegments.Add(node);
                    }

                    return roadSegments;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                var neighbors = Neighbor(current, data);
                foreach (var neighbor in neighbors.Where(x => !closedSet.Contains(x)))
                {
                    var tentativeGScore = gScore[current] + Distance(current, neighbor, data);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else if (tentativeGScore >= gScore[neighbor])
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] +
                                       dataProvider.HeuristicCostEstimate(neighbor, goal);
                }
            }

            return new RoadSegmentViewModel[0];
        }

        private double Distance(long startId, long stopId, IList<RoadSegmentViewModel> data)
        {
            return data.Where(x => x.StartNodeId == startId && x.StopNodeId == stopId)
                    .Select(x => x.Cost)
                    .FirstOrDefault();
        }

        private IEnumerable<long> Neighbor(long nodeId, IList<RoadSegmentViewModel> nodes)
        {
            return nodes.Where(x => x.StartNodeId == nodeId)
                    .Select(x => x.StopNodeId)
                    .Union(nodes.Where(x => x.StopNodeId == nodeId).Select(x => x.StartNodeId))
                    .Distinct();
        }

        private IList<long> FormatPath(Dictionary<long, long> cameFrom, long current)
        {
            var result = new List<long> {current};
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                result.Add(current);
            }
            return result;
        }
    }
}