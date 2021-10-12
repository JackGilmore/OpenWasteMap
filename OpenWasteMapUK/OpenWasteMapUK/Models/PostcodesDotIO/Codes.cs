using Newtonsoft.Json;

namespace OpenWasteMapUK.Models.PostcodesDotIO
{
    public class Codes
    {
        [JsonProperty("admin_district")]
        public string AdminDistrict { get; set; }
        [JsonProperty("admin_county")]
        public string AdminCounty { get; set; }
        [JsonProperty("admin_ward")]
        public string AdminWard { get; set; }
        public string Parish { get; set; }
        [JsonProperty("parliamentary_constituency")]
        public string ParliamentaryConstituency { get; set; }
        public string Ccg { get; set; }
        [JsonProperty("ccg_id")]
        public string CcgId { get; set; }
        public string Ced { get; set; }
        public string Nuts { get; set; }
        public string Lsoa { get; set; }
        public string Msoa { get; set; }
        public string Lau2 { get; set; }
    }
}