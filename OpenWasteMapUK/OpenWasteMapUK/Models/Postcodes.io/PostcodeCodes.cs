using Newtonsoft.Json;

namespace OpenWasteMapUK.Models.Postcodes.io
{
    public class PostcodeCodes
    {
        [JsonProperty("admin_district")]
        public string AdminDistrict { get; set; }
        [JsonProperty("admin_county")]
        public string AdminCounty { get; set; }
        [JsonProperty("admin_ward")]
        public string AdminWard { get; set; }
        [JsonProperty("parish")]
        public string Parish { get; set; }
        [JsonProperty("parliamentary_constituency")]
        public string ParliamentaryConstituency { get; set; }
        [JsonProperty("ccg")]
        public string Ccg { get; set; }
        [JsonProperty("ccg_id")]
        public string CcgId { get; set; }
        [JsonProperty("ced")]
        public string Ced { get; set; }
        [JsonProperty("nuts")]
        public string Nuts { get; set; }
        [JsonProperty("lsoa")]
        public string Lsoa { get; set; }
        [JsonProperty("msoa")]
        public string Msoa { get; set; }
        [JsonProperty("lau2")]
        public string Lau2 { get; set; }
    }
}