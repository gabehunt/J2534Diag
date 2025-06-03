using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace J2534Diag
{
    public static class VinDecoder
    {
        private static readonly HttpClient client = new HttpClient();

        public static VehicleInfo DecodeVin(string vin)
        {
            VehicleInfo rVal = new VehicleInfo();
            try
            {
                string url = $"https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVinValues/{vin}?format=json";
                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                    return rVal;

                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<VehicleResult>(content);

                if (result != null && result.Results != null && result.Results.Count > 0)
                    rVal = result.Results[0];
            }
            catch
            {
                // Ignore all errors (network, JSON, etc.)
            }

            return rVal;
        }
    }
}
