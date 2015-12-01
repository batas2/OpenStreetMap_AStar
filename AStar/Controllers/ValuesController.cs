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
        public IEnumerable<RoadSegmentViewModel> FindPath(string startStreetName, string stopStreetName)
        {
            var data = new InMemoryCache().GetOrSet("data", () => new DataProvider.DataProvider().LoadRoadSegments().ToList());

            var startNode =
                data.Where(x => x.RoadName.Contains(startStreetName)).OrderByDescending(x => x.Order).ToList();
            var stopNode = data.Where(x => x.RoadName.Contains(stopStreetName))
                .OrderBy(x => x.Order)
                .ThenBy(x => x.RoadName.Length)
                .ToList();

            if (!startNode.Any() || !stopNode.Any())
                return new RoadSegmentViewModel[0];

            var start = startNode.FirstOrDefault().StartNodeId;
            var goal = stopNode.FirstOrDefault().StartNodeId;

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