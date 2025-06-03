using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534Diag
{
    public class VehicleInfo
    {
        public string ModelYear { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Series { get; set; }
        public string DisplacementCI { get; set; }
        public string EngineCylinders { get; set; }
        public string EngineHP { get; set; }
        public string DisplacementL { get; set; }
    }

    public class VehicleResult
    {
        public int Count { get; set; }
        public string Message { get; set; }
        public string SearchCriteria { get; set; }
        public List<VehicleInfo> Results { get; set; }
    }

}
