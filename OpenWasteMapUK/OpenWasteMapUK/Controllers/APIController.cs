using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Repositories;
using RestSharp;

namespace OpenWasteMapUK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        public ApiController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }
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

            var country = (postcodeData["result"]?["country"] ?? "Unknown").Value<string>();

            if (country.Equals("England", StringComparison.CurrentCultureIgnoreCase))
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
        //[ServiceFilter(typeof(CacheCheckFilter))]
        public async Task<IActionResult> GetFeatures()
        {
            var data = await _dataRepository.GetElementsFromCache();
            return Ok(data);

            // TODO: Output as GeoJson
            // TODO: Compound WikiData too if possible
        }

        [Route("RefreshCache")]
        public async Task<IActionResult> RefreshCache()
        {
            await _dataRepository.RefreshCache();

            return Created(string.Empty, string.Empty);
        }

        [Route("SuggestChange")]
        public async Task<IActionResult> SuggestChange(string hwrcName, string hwrcText, double lat, double lng)
        {
            // LIVE
            IRestClient client = new RestClient("https://api.openstreetmap.org/api/0.6/notes");
            // TEST
            //IRestClient client = new RestClient("https://master.apis.dev.openstreetmap.org/api/0.6/notes");
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
