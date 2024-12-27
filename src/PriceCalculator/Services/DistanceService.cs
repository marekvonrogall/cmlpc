using System.Text.Json;

namespace PriceCalculator.Services
{
    public class DistanceService
    {
        private readonly HttpClient _httpClient;

        public DistanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<double?> GetDistanceInKmAsync(string endLat, string endLon)
        {
            string startLat = "50.697864";
            string startLon = "7.018537";

            var url = $"http://osrm:5000/route/v1/driving/{startLon},{startLat};{endLon},{endLat}?overview=false";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JsonDocument.Parse(response);
                var distance = jsonResponse.RootElement
                    .GetProperty("routes")[0]
                    .GetProperty("legs")[0]
                    .GetProperty("distance")
                    .GetDouble();

                return distance / 1000; // Convert to kilometers
            }
            catch
            {
                return null; // Return null if the distance couldn't be retrieved
            }
        }
    }
}
