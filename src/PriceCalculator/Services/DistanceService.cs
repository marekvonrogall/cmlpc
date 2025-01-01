using System.Text.Json;

namespace PriceCalculator.Services
{
    public class DistanceService
    {
        private readonly HttpClient _httpClient;
        private readonly GeocodeService _geocodeService;

        public DistanceService(HttpClient httpClient, GeocodeService geocodeService)
        {
            _httpClient = httpClient;
            _geocodeService = geocodeService;
        }

        public async Task<int> GetDistance(ConfigService config, string plzRouteEnd)
        {
            var endCoords = _geocodeService.GetCoordinates(plzRouteEnd);

            if (endCoords == null) return 0; // Invalid postal code

            string startLat = config.Routenkalkulierung.RouteCalculation_StartPointLat.Value.ToString();
            string startLon = config.Routenkalkulierung.RouteCalculation_StartPointLon.Value.ToString();
            string endLat = endCoords.Value.lat.ToString();
            string endLon = endCoords.Value.lon.ToString();

            var distanceInKm = await OSRM_CalculateRouteKm(startLat, startLon, endLat, endLon);

            if (distanceInKm == null) return 0; // Something went wrong / OSRM isn't running.

            return (int)Math.Round(distanceInKm.Value);
        }

        public async Task<double?> OSRM_CalculateRouteKm(string startLat, string startLon, string endLat, string endLon)
        {
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
