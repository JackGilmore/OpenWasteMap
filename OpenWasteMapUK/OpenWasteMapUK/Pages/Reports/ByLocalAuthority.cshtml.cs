using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Repositories;

namespace OpenWasteMapUK.Pages.Reports
{
    public class ByLocalAuthorityModel : PageModel
    {
        private readonly IDataRepository _dataRepository;
        public IEnumerable<IGrouping<string, OsmElement>> HWRCs;
        public ByLocalAuthorityModel(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task OnGet()
        {
            var elements = await _dataRepository.GetElementsFromCache();

            HWRCs = elements.Where(e => e.Tags != null)
                .GroupBy(e => e.Tags.GetValueOrDefault("owner"))
                .OrderBy(g => g.Key);
        }
    }



}
