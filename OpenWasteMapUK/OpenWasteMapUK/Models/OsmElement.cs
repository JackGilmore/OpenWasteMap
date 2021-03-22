using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenWasteMapUK.Models
{
    public class OsmElement
    {
        public string Type { get; set; }
        public long Id { get; set; }
        public List<long> Nodes { get; set; }
        public Dictionary<string,string> Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

    }
}
