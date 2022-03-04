using System.Collections.Generic;

namespace OpenWasteMapUK.Models
{
    public class OsmResponse
    {
        public List<OsmElement> Elements { get; set; }
        public string Remark { get; set; }
    }
}
