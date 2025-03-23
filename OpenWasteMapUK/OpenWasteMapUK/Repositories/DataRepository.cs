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
using Newtonsoft.Json;
using OpenWasteMapUK.Models;
using RestSharp;
using Z.BulkOperations;

namespace OpenWasteMapUK.Repositories
{
    public interface IDataRepository
    {
        public Task<IEnumerable<OsmElement>> GetElementsFromCache();
        public Task RefreshCache();
        public Task CacheAgeCheck();
        public Task<int> GetTableCount();
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

            //_ = CacheAgeCheck();

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
            var client = new RestClient("https://overpass.kumi.systems/api/interpreter");
            var request = new RestRequest
            {
                Method = Method.Post
            };

            try
            {
                var queryFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Data", "osm_query.txt");
                var queryFile = await File.ReadAllTextAsync(queryFilePath);
                var encodedQuery = HttpUtility.UrlEncode(queryFile);
                request.AddParameter("application/x-www-form-urlencoded", $"data={encodedQuery}", ParameterType.RequestBody);
                _logger.LogInformation(queryFile);
            }
            catch (Exception e)
            {
                _logger.LogError(e, string.Empty);
                throw;
            }

            _logger.LogInformation("Querying OSM...");
            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning($"Cache refresh fail: {response.StatusCode} {response.Content}");
            }

            _logger.LogInformation($"Got response back from OSM: {response.StatusCode}");
            _logger.LogInformation(response.Content);
            var osmResponse = JsonConvert.DeserializeObject<OsmResponse>(response.Content);

            if (!string.IsNullOrEmpty(osmResponse.Remark) && osmResponse.Remark.Contains("error"))
            {
                var ex = new Exception($"Cache refresh fail: {osmResponse.Remark}");
                _logger.LogCritical(ex.Message);
                throw ex;
            }

            try
            {
                await db.Database.ExecuteSqlRawAsync("DELETE FROM [OsmElements];");

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

        public async Task CacheAgeCheck()
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

        public async Task<int> GetTableCount() => await _dbContext.OsmElements.CountAsync();
    }
}
