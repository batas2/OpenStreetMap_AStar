using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AStar.DataProvider;
using AStar.Models;

namespace AStar.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        //http://localhost:51222/path/Marsza%C5%82kowska/Banacha
        public IEnumerable<RoadSegmentViewModel> FindPath(string startStreetName, string stopStreetName)
        {
            var data = new InMemoryCache().GetOrSet("data", () => new DataProvider.DataProvider().LoadRoadSegments().ToList());

            var startNode = data
                            .Where(x => x.RoadName.ToLower().Contains(startStreetName.ToLower()))
                            .OrderByDescending(x => x.Order)
                            .ToList();

            var stopNode = data
                            .Where(x => x.RoadName.ToLower()
                            .Contains(stopStreetName.ToLower()))
                            .OrderBy(x => x.Order)
                            .ThenBy(x => x.RoadName.Length)
                            .ToList();

            if (!startNode.Any() || !stopNode.Any())
                return new RoadSegmentViewModel[0];

            var start = startNode.First().StartNodeId;
            var goal = stopNode.First().StartNodeId;

            return new AStarAlg().FindPath(start, goal, data);
        }

        [HttpGet]
        //http://localhost:51222/NearestNodes/52.1877358/21.6868445/1280/10
        public IEnumerable<NearestNodeViewModel> NearestNodes(string lat, string lng, long radius, int limit)
        {
            return new DataProvider.DataProvider().NearestNodes(lat, lng, radius, limit);
        }
    }
}