using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Models.PostcodesDotIO;
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

            if (!postcodeResponse.IsSuccessful)
            {
                return BadRequest("Unknown error occurred getting postcode data");
            }

            var postcodeData = JsonConvert.DeserializeObject<PostcodeResult>(postcodeResponse.Content);

            if (!(postcodeData is { Status: 200 }))
            {
                return BadRequest(
                    $"Error occurred getting postcode data. Status {postcodeData.Status}: {postcodeData.Error}");
            }

            string councilArea;
            string councilCode;

            var country = postcodeData.Result.Country;
            
            if (country.Equals("England", StringComparison.CurrentCultureIgnoreCase) && postcodeData.Result.AdminCounty != null)
            {
                councilArea = postcodeData.Result.AdminCounty;
                councilCode = postcodeData.Result.Codes.AdminCounty;
            }
            else
            {
                councilArea = postcodeData.Result.AdminDistrict;
                councilCode = postcodeData.Result.Codes.AdminDistrict;
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

            var count = await _dataRepository.GetTableCount();

            return Created(string.Empty, count);
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
