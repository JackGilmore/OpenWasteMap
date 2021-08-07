using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenWasteMapUK.Models;
using OpenWasteMapUK.Repositories;

namespace OpenWasteMapUK.Pages
{
    public class ListModel : PageModel
    {
        private readonly IDataRepository _dataRepository;
        public IEnumerable<OsmElement> HWRCs;
        public ListModel(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public async Task OnGet()
        {
            var elements = await _dataRepository.GetElementsFromCache();

            HWRCs = elements.Where(e => e.Tags != null).OrderBy(hwrc => hwrc.Tags.GetValueOrDefault("owner"));
        }
    }
}
