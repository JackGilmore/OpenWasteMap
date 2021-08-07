using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenWasteMapUK.Models;
using RestSharp;

namespace OpenWasteMapUK.Repositories
{
    public interface IDataRepository
    {
        public Task<IEnumerable<OsmElement>> GetElementsFromCache();
        public Task RefreshCache();
        public Task CacheAgeCheck();
    }
    public class DataRepository : IDataRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<IDataRepository> _logger;
        public DataRepository(ApplicationDbContext dbContext, IServiceScopeFactory serviceScopeFactory, ILogger<IDataRepository> logger)
        {
            _dbContext = dbContext;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
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
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", "data=%5Bout%3Ajson%5D%5Btimeout%3A25%5D%3B%0A(%0A++node%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A++way%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A)%3B%0Aout+body%3B%0A%3E%3B%0Aout+skel+qt%3B", ParameterType.RequestBody);

            _logger.LogInformation("Querying OSM...");
            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                _logger.LogWarning($"Cache refresh fail: {response.StatusCode} {response.Content}");
            }

            _logger.LogInformation($"Got response back from OSM: {response.StatusCode}");
            var osmResponse = JsonConvert.DeserializeObject<OsmResponse>(response.Content);

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
    }
}
