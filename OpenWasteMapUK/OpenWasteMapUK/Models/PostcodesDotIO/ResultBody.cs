using Newtonsoft.Json;

namespace OpenWasteMapUK.Models.PostcodesDotIO
{
    public class ResultBody
    {
        public string Postcode { get; set; }
        public int Quality { get; set; }
        public int Eastings { get; set; }
        public int Northings { get; set; }
        public string Country { get; set; }
        [JsonProperty("nhs_sa")]
        public string NhsHa { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        [JsonProperty("european_electoral_region")]
        public string EuropeanElectoralRegion { get; set; }
        [JsonProperty("primary_care_trust")]
        public string PrimaryCareTrust { get; set; }
        public string Region { get; set; }
        public string Lsoa { get; set; }
        public string Msoa { get; set; }
        public string InCode { get; set; }
        public string OutCode { get; set; }
        [JsonProperty("parliamentary_constituency")]
        public string ParliamentaryConstituency { get; set; }
        [JsonProperty("admin_district")]
        public string AdminDistrict { get; set; }
        public string Parish { get; set; }
        [JsonProperty("admin_county")]
        public string AdminCounty { get; set; }
        [JsonProperty("admin_ward")]
        public string AdminWard { get; set; }
        public string Ced { get; set; }
        public string Ccg { get; set; }
        public string Nuts { get; set; }
        public Codes Codes { get; set; }
    }
}