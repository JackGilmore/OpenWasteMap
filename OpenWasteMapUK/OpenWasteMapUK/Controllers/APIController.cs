using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenWasteMapUK.Models;
using RestSharp;

namespace OpenWasteMapUK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        [Route("search")]
        public async Task<IActionResult> Search(string postcode, string waste)
        {
            if (string.IsNullOrEmpty(postcode) || string.IsNullOrEmpty(waste))
            {
                return BadRequest("You must fill out postcode and waste type");
            }

            IRestClient postcodeClient = new RestClient($"https://api.postcodes.io/postcodes/{postcode}");

            IRestRequest postcodeRequest = new RestRequest();

            var postcodeResponse = await postcodeClient.ExecuteAsync(postcodeRequest);

            var postcodeData = JObject.Parse(postcodeResponse.Content);

            if (!postcodeResponse.IsSuccessful)
            {
                return BadRequest((postcodeData["error"] ?? "Unknown error occurred").Value<string>());
            }

            string councilArea;
            string councilCode;

            //var councilArea = (postcodeData["result"]?["admin_district"] ?? "Unknown").Value<string>();
            //var councilCode = (postcodeData["result"]?["codes"]?["admin_district"] ?? "Unknown").Value<string>();
            var country = (postcodeData["result"]?["country"] ?? "Unknown").Value<string>();

            if (country.Equals("England",StringComparison.CurrentCultureIgnoreCase))
            {
                councilArea = (postcodeData["result"]?["admin_county"] ?? "Unknown").Value<string>();
                councilCode = (postcodeData["result"]?["codes"]?["admin_county"] ?? "Unknown").Value<string>();
            }
            else
            {
                councilArea = (postcodeData["result"]?["admin_district"] ?? "Unknown").Value<string>();
                councilCode = (postcodeData["result"]?["codes"]?["admin_district"] ?? "Unknown").Value<string>();
            }

            var wasteOSMTag = TagMappings.Values.GetValueOrDefault(waste, null);

            return Ok(new
            {
                councilArea,
                councilCode,
                country,
                wasteOSMTag
            });
        }
        [Route("GetFeatures")]
        public async Task<IActionResult> GetFeatures()
        {
            // TODO: Cache this
            IRestClient client = new RestClient("https://overpass.kumi.systems/api/interpreter");
            IRestRequest request = new RestRequest(Method.POST);
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", "data=%5Bout%3Ajson%5D%5Btimeout%3A25%5D%3B%0A(%0A++node%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A++way%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A)%3B%0Aout+body%3B%0A%3E%3B%0Aout+skel+qt%3B", ParameterType.RequestBody);

            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return BadRequest("Could not retrieve map data");
            }

            var osmResponse = JsonConvert.DeserializeObject<OsmResponse>(response.Content);

            // TODO: Output as GeoJson
            // TODO: Compound WikiData too if possible

            return Ok(osmResponse.Elements);
        }

        [Route("SuggestChange")]
        public async Task<IActionResult> SuggestChange(string hwrcName, string hwrcText, double lat, double lng)
        {
            // LIVE
            //IRestClient client = new RestClient("https://api.openstreetmap.org/api/0.6/notes");
            // TEST
            IRestClient client = new RestClient("https://master.apis.dev.openstreetmap.org/api/0.6/notes");
            IRestRequest request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("lat", lat);
            request.AddParameter("lon", lng);
            request.AddParameter("text", $"Suggested change to {hwrcName}: {hwrcText}");
            IRestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
