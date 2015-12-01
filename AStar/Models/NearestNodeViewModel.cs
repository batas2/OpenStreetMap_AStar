using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AStar.Models
{
    public class NearestNodeViewModel
    {
        public long NodeId { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public double Distance { get; set; }
    }
}