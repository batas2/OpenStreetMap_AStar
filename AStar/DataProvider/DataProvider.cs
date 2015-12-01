using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AStar.Models;
using System.Configuration;

namespace AStar.DataProvider
{
    public class DataProvider
    {
        private readonly string _conn;

        public DataProvider()
        {
            _conn = ConfigurationManager.ConnectionStrings["Connection"].ToString();
        }

        public double HeuristicCostEstimate(long startNodeId, long stopNodeId)
        {
            double result = 0;
            using (var connection = new SqlConnection(_conn))
            {
                var query =
                    string.Format(
                        "SELECT N1.Location.STDistance(N2.Location) FROM dbo.Nodes N1, dbo.Nodes N2 WHERE N1.Id = {0} AND N2.id = {1}",
                        startNodeId, stopNodeId);

                var command = new SqlCommand(query, connection) { CommandTimeout = 180 };
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetDouble(0);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return result;
        }

        public IList<RoadSegmentViewModel> LoadRoadSegments()
        {
            var rawNodes = GetRawNodes();
            rawNodes = CompressData(rawNodes);
            return rawNodes;
        }

        private ISet<long> Junctions()
        {
            var result = new HashSet<long>();
            using (var connection = new SqlConnection(_conn))
            {
                var query = "SELECT NodeId FROM dbo.RoadNode Group BY NodeId HAVING count(*) > 1";

                var command = new SqlCommand(query, connection) { CommandTimeout = 180 };
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    reader.Close();
                }
                connection.Close();
            }
            return result;
        }

        private IList<RoadSegmentViewModel> CompressData(IList<RoadSegmentViewModel> nodes)
        {
            var result = new List<RoadSegmentViewModel> { nodes.First() };
            var junction = Junctions();

            for (var i = 1; i < nodes.Count(); i++)
            {
                var prev = nodes[i - 1];
                var current = nodes[i];

                if (current.RoadId == prev.RoadId && !junction.Contains(current.StartNodeId))
                {
                    result.Last().StopNodeId = current.StopNodeId;
                    result.Last().Cost += current.Cost;
                }
                else
                {
                    result.Add(current);
                }
            }
            return result;
        }

        private IList<RoadSegmentViewModel> GetRawNodes()
        {
            var result = new List<RoadSegmentViewModel>();
            using (var connection = new SqlConnection(_conn))
            {
                var query =
                    "SELECT R.Name, RN.RoadId, NSTART.Id as StartId, NSTOP.Id as StopId, NSTART.Location.STDistance(NSTOP.Location) As Cost, RN.OrderId" +
                    " FROM [dbo].RoadNode RN" +
                    " JOIN [dbo].Nodes NSTART ON rn.NodeId = NSTART.Id" +
                    " JOIN [dbo].Nodes NSTOP ON rn.DestNodeId = NSTOP.Id" +
                    " JOIN [dbo].Roads R ON RN.RoadId = R.Id" +
                    " ORDER BY R.Name, RN.OrderId";

                var command = new SqlCommand(query, connection) { CommandTimeout = 180 };
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var node = new RoadSegmentViewModel
                        {
                            RoadName = reader.GetString(0),
                            RoadId = reader.GetInt64(1),
                            StartNodeId = reader.GetInt64(2),
                            StopNodeId = reader.GetInt64(3),
                            Cost = reader.GetDouble(4),
                            Order = reader.GetInt32(5)
                        };
                        result.Add(node);
                    }

                    reader.Close();
                }
                connection.Close();
            }
            return result;
        }

        public IEnumerable<NearestNodeViewModel> NearestNodes(string lat, string lng, double radius, int limit)
        {

            //CREATE PROCEDURE NearestNode 
            //    @geo NVARCHAR(512), 
            //    @radius float,
            //    @limit int
            //AS
            //BEGIN
            //    SET NOCOUNT ON;

            //DECLARE @g geography = @geo;
            //SELECT TOP(@limit) Id, Location.STGeometryN(1).Lat AS Lat, Location.STGeometryN(1).Long as Long, Location.STDistance(@g) as Dist FROM dbo.Nodes
            //WHERE Location.STDistance(@g) IS NOT NULL AND Location.STDistance(@g) < @radius
            //ORDER BY Location.STDistance(@g);
            //END

            var result = new List<NearestNodeViewModel>();
            using (var con = new SqlConnection(_conn))
            {
                using (var cmd = new SqlCommand("NearestNode", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@geo", SqlDbType.NVarChar).Value = string.Format("POINT({0} {1})", lat, lng);
                    cmd.Parameters.Add("@radius", SqlDbType.Float).Value = radius;
                    cmd.Parameters.Add("@limit", SqlDbType.Int).Value = limit;

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var node = new NearestNodeViewModel
                            {
                                NodeId = reader.GetInt64(0),
                                Latitude = reader.GetDouble(1),
                                Longitude = reader.GetDouble(2),
                                Distance = reader.GetDouble(3)
                            };
                            result.Add(node);
                        }

                        reader.Close();
                    }
                }
            }
            return result;
        }
    }
}