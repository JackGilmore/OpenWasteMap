using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenWasteMapUK.Models;
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
        public DataRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<OsmElement>> GetElementsFromCache()
        {
            return await _dbContext.OsmElements.ToListAsync();
        }

        public async Task RefreshCache()
        {
            IRestClient client = new RestClient("https://overpass.kumi.systems/api/interpreter");
            IRestRequest request = new RestRequest(Method.POST);
            request.AddParameter("application/x-www-form-urlencoded; charset=UTF-8", "data=%5Bout%3Ajson%5D%5Btimeout%3A25%5D%3B%0A(%0A++node%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A++way%5B%7E%22%5Eowner%22%7E%22Council%22,i%5D%5B%22recycling_type%22%3D%22centre%22%5D(49.88,-8.39,61.06,2.47)%3B%0A)%3B%0Aout+body%3B%0A%3E%3B%0Aout+skel+qt%3B", ParameterType.RequestBody);

            IRestResponse response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Failed to refresh cache", response.ErrorException);
            }

            var osmResponse = JsonConvert.DeserializeObject<OsmResponse>(response.Content);

            try
            {
                foreach (var element in osmResponse.Elements)
                {
                    if (await _dbContext.OsmElements.AnyAsync(cacheElement => cacheElement.Id == element.Id))
                    {
                        _dbContext.OsmElements.Update(element);
                    }
                    else
                    {
                        await _dbContext.OsmElements.AddAsync(element);
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
