using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AStar.Models
{
    public class RoadSegmentViewModel
    {
        [DataMember]
        public long RoadId { get; set; }

        [DataMember]
        public string RoadName { get; set; }

        [DataMember]
        public long StartNodeId { get; set; }

        [DataMember]
        public long StopNodeId { get; set; }

        [DataMember]
        public double Cost { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int Order { get; set; }

        public override string ToString()
        {
            return string.Format("{2} - {3}({4}) {1}({0})", RoadId, RoadName, StartNodeId, StopNodeId, Cost);
        }
    }
}