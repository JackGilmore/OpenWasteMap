using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Models.Postcodes.io;
using OpenWasteMapUK.Utilities;
using RestSharp;

namespace OpenWasteMapUK.Repositories
{
    public interface IDataRepository
    {
        public Task<IEnumerable<OsmElement>> GetElementsFromCache();
        public Task RefreshCache();
    }
    public class DataRepository : IDataRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<IDataRepository> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DataRepository(ApplicationDbContext dbContext, IServiceScopeFactory serviceScopeFactory, ILogger<IDataRepository> logger, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IEnumerable<OsmElement>> GetElementsFromCache()
        {
            var elements = await _dbContext.OsmElements.AsNoTracking().ToListAsync();

            _ = CacheAgeCheck();

            return elements;
        }

        public async Task RefreshCache()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if (db is null)
            {
                throw new NullReferenceException("DbContext init failed");
            }

            _logger.LogInformation("Running cache refresh...");
            IRestClient client = new RestClient("https://overpass.kumi.systems/api/interpreter");
            IRestRequest request = new RestRequest(Method.POST);

            try
            {
                var queryFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Data", "osm_query.txt");
                var queryFile = await File.ReadAllTextAsync(queryFilePath);
                var encodedQuery = HttpUtility.UrlEncode(queryFile);
                request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", $"data={encodedQuery}", ParameterType.RequestBody);
                _logger.LogInformation(queryFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, string.Empty);
                throw;
            }

            _logger.LogInformation("Querying OSM...");
            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning($"Cache refresh fail: {response.StatusCode} {response.Content}");

            }

            _logger.LogInformation($"Got response back from OSM: {response.StatusCode}");
            var osmResponse = JsonConvert.DeserializeObject<OsmResponse>(response.Content);

            var postCodes = osmResponse.Elements
                .Where(x => x.Tags != null)
                .Select(x => x.Tags.GetValueOrDefault("addr:postcode"))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();

            var gssCodes = await GetGSSCodesForPostcodes(postCodes);

            osmResponse.Elements.Where(x => x.Tags != null).ForEach(x =>
            {
                var postcode = x.Tags.GetValueOrDefault("addr:postcode");
                if (!string.IsNullOrEmpty(postcode))
                {
                    x.Tags.Add("gssCode",gssCodes.GetValueOrDefault(postcode.RemoveWhitespace()));
                }
            });

            try
            {
                await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [OsmElements];");

                await db.SaveChangesAsync();

                await db.OsmElements.AddRangeAsync(osmResponse.Elements);

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        private async Task CacheAgeCheck()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if (db is null)
            {
                throw new NullReferenceException("DbContext init failed");
            }

            var oldestElement = db.OsmElements.OrderBy(e => e.TimeStamp).FirstOrDefault();

            if (oldestElement is null || DateTime.Now.AddMinutes(-5) > oldestElement.TimeStamp)
            {
                await RefreshCache();
            }
        }

        private async Task<Dictionary<string, string>> GetGSSCodesForPostcodes(IEnumerable<string> postcodes)
        {
            var outputDictionary = new Dictionary<string, string>();
            var client = new RestClient("https://api.postcodes.io/postcodes?filter=codes,country");
            foreach (var postcodeBatch in postcodes.Batch(100))
            {
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(new { postcodes = postcodeBatch }, "application/json");
                IRestResponse response = await client.ExecuteAsync(request);

                var decodedResponse = JsonConvert.DeserializeObject<PostcodesResponse>(response.Content);

                decodedResponse.Result
                    .Where(x => x.Result != null)
                    .ForEach(x =>
                        outputDictionary.Add
                            (x.Query.RemoveWhitespace(),
                            x.Result.Country == "England" && x.Result.Codes.AdminCounty != "E99999999"  ? x.Result.Codes.AdminCounty : x.Result.Codes.AdminDistrict
                            )
                        );
            }

            return outputDictionary;
        }
    }
}
