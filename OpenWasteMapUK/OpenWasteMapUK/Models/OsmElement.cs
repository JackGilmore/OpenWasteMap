using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        [NotMapped]
        public bool HasOpenHours => Tags != null && Tags.ContainsKey("opening_hours");

        [NotMapped]
        public int MaterialsListed => Tags != null ? Tags.Keys.Count(t => t.StartsWith("recycling:", StringComparison.CurrentCultureIgnoreCase)) : 0;
        [NotMapped] public bool HasWikiData => Tags != null && Tags.ContainsKey("wikidata");

        public OsmElement()
        {
            TimeStamp = DateTime.Now;
        }
    }
}
