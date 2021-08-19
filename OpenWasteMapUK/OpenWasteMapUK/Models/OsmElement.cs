using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace OpenWasteMapUK.Models
{
    public class OsmElement
    {
        public string Type { get; set; }
        public long Id { get; set; }
        public List<long> Nodes { get; set; }
        public Dictionary<string, string> Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        [JsonIgnore]
        public DateTime TimeStamp { get; set; }
        [NotMapped]
        public string OsmLink => $"https://openstreetmap.org/{Type}/{Id}";

        public OsmElement()
        {
            TimeStamp = DateTime.Now;
        }
    }
}
